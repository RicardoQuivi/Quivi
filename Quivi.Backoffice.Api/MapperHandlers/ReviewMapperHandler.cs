using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class ReviewMapperHandler : IMapperHandler<Review, Dtos.Review>
    {
        private readonly IIdConverter idConverter;

        public ReviewMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.Review Map(Review model)
        {
            return new Dtos.Review
            {
                Id = idConverter.ToPublicId(model.PosChargeId),
                Comment = string.IsNullOrWhiteSpace(model.Comment) ? null : model.Comment,
                Stars = model.Stars,
                CreatedDate = new DateTimeOffset(model.CreatedDate, TimeSpan.Zero),
                ModifiedDate = new DateTimeOffset(model.ModifiedDate, TimeSpan.Zero),
            };
        }
    }
}