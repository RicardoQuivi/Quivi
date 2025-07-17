using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Merchants;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Merchants
{
    public class AddUserToMerchantsAsyncCommand : ICommand<Task>
    {
        public int UserId { get; init; }
        public required IEnumerable<int> MerchantIds { get; init; }
    }

    public class AddUserToMerchantAsyncCommandHandler : ICommandHandler<AddUserToMerchantsAsyncCommand, Task>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IApplicationUsersRepository usersRepo;
        private readonly IMerchantsRepository merchantRepo;
        private readonly IEventService eventService;

        public AddUserToMerchantAsyncCommandHandler(IUnitOfWork unitOfWork,
                                                    IEventService eventService)
        {
            this.unitOfWork = unitOfWork;
            this.usersRepo = unitOfWork.Users;
            this.merchantRepo = unitOfWork.Merchants;
            this.eventService = eventService;
        }

        public async Task Handle(AddUserToMerchantsAsyncCommand command)
        {
            var usersQuery = await usersRepo.GetAsync(new GetApplicationUsersCriteria
            {
                Ids = [ command.UserId ],
                IncludeMerchants = true,
                PageSize = 1,
            });
            var user = usersQuery.FirstOrDefault();
            if (user == null)
                return;

            var merchantsQuery = await merchantRepo.GetAsync(new GetMerchantsCriteria
            {
                Ids = command.MerchantIds,
                IsDeleted = false,
            });

            foreach(var merchant in merchantsQuery)
                user.Merchants!.Add(merchant);

            await unitOfWork.SaveChangesAsync();

            foreach (var merchant in merchantsQuery)
                await eventService.Publish(new OnMerchantAssociatedOperationEvent
                {
                    Operation = EntityOperation.Create,
                    MerchantId = merchant.Id,
                    UserId = user.Id,
                });
        }
    }
}
