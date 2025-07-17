using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
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
                Comment = model.Comment,
                Stars = model.Stars,
            };
        }
    }
}
