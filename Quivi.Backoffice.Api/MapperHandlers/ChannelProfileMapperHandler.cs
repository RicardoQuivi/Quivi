using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class ChannelProfileMapperHandler : IMapperHandler<ChannelProfile, Dtos.ChannelProfile>
    {
        private readonly IIdConverter idConverter;

        public ChannelProfileMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.ChannelProfile Map(ChannelProfile model)
        {
            var id = idConverter.ToPublicId(model.Id);
            return new Dtos.ChannelProfile
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                MinimumPrePaidOrderAmount = model.PrePaidOrderingMinimumAmount ?? 0.0m,
                SendToPreparationTimer = model.SendToPreparationTimer,
                Features = model.Features,
                PosIntegrationId = idConverter.ToPublicId(model.PosIntegrationId),
            };
        }
    }
}
