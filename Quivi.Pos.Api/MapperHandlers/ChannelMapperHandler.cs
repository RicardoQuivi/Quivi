using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class ChannelMapperHandler : IMapperHandler<Channel, Dtos.Channel>
    {
        private readonly IIdConverter idConverter;
        private readonly IAppHostsSettings appHostsSettings;

        public ChannelMapperHandler(IIdConverter idConverter, IAppHostsSettings appHostsSettings)
        {
            this.idConverter = idConverter;
            this.appHostsSettings = appHostsSettings;
        }

        public Dtos.Channel Map(Channel model)
        {
            var id = idConverter.ToPublicId(model.Id);
            return new Dtos.Channel
            {
                Id = id,
                Name = model.Identifier,
                Url = appHostsSettings.GuestsApp.CombineUrl($"/c/{id}"),
                ChannelProfileId = idConverter.ToPublicId(model.ChannelProfileId),
                IsDeleted = model.DeletedDate.HasValue,
            };
        }
    }
}