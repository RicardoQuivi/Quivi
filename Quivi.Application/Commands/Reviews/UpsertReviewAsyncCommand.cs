using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Reviews;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Reviews
{
    public interface IUpdatableReview : IUpdatableEntity
    {
        int PosChargeId { get; }
        string? Comment { get; set; }
        int Stars { get; set; }
    }

    public class UpsertReviewAsyncCommand : AUpdateAsyncCommand<Review, IUpdatableReview>
    {
        public int PosChargeId { get; init; }
    }

    public class UpsertReviewAsyncCommandHandler : ICommandHandler<UpsertReviewAsyncCommand, Task<Review>>
    {
        private readonly IPosChargesRepository posChargesRepository;
        private readonly IReviewsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        private class UpdatableReview : IUpdatableReview
        {
            public readonly Review Model;
            public readonly bool IsNew;
            private readonly string? originalComment;
            private readonly int originalStars;

            public UpdatableReview(Review? model, Func<Review> builder)
            {
                this.Model = model ?? builder();
                this.IsNew = model == null;
                this.originalComment = Model.Comment;
                this.originalStars = Model.Stars;
            }

            public int PosChargeId => Model.PosChargeId;

            public string? Comment
            {
                get => Model.Comment;
                set => Model.Comment = value;
            }
            public int Stars 
            {
                get => Model.Stars;
                set => Model.Stars = value;
            }

            public bool HasChanges
            {
                get
                {
                    if (IsNew)
                        return true;

                    if (originalComment != Model.Comment)
                        return true;

                    if(originalStars != Model.Stars)
                        return true;

                    return false;
                }
            }
        }

        public UpsertReviewAsyncCommandHandler(IReviewsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService, IPosChargesRepository posChargesRepository)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
            this.posChargesRepository = posChargesRepository;
        }

        public async Task<Review> Handle(UpsertReviewAsyncCommand command)
        {
            var entityQuery = await posChargesRepository.GetAsync(new GetPosChargesCriteria
            {
                Ids = [command.PosChargeId],
                IncludeReview = true,
                PageIndex = 0,
                PageSize = 1,
            });

            var now = this.dateTimeProvider.GetUtcNow();
            var posCharge = entityQuery.Single();
            var entity = posCharge.Review;

            var updatableEntity = new UpdatableReview(entity, () =>
            {
                var review = new Review
                {
                    PosChargeId = command.PosChargeId,

                    Stars = 1,
                    Comment = null,

                    CreatedDate = now,
                    ModifiedDate = now,
                };
                repository.Add(review);
                return review;
            });

            await command.UpdateAction(updatableEntity);
            if (updatableEntity.HasChanges == false)
                return updatableEntity.Model;

            await repository.SaveChangesAsync();

            await eventService.Publish(new OnReviewOperationEvent
            {
                Id = updatableEntity.Model.PosChargeId,
                MerchantId = posCharge.MerchantId,
                ChannelId = posCharge.ChannelId,
                Operation = updatableEntity.IsNew ? EntityOperation.Create : EntityOperation.Update,
            });
            return updatableEntity.Model;
        }
    }
}