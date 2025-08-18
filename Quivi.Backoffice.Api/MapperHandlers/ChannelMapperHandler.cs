using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class ChannelMapperHandler : IMapperHandler<Channel, Dtos.Channel>
    {
        private readonly IIdConverter idConverter;
        private readonly IAppHostsSettings hostsSettings;

        public ChannelMapperHandler(IIdConverter idConverter, IAppHostsSettings hostsSettings)
        {
            this.idConverter = idConverter;
            this.hostsSettings = hostsSettings;
        }

        public Dtos.Channel Map(Channel model)
        {
            var id = idConverter.ToPublicId(model.Id);
            return new Dtos.Channel
            {
                Id = id,
                Url = hostsSettings.GuestsApp.CombineUrl($"/c/{id}"),
                ChannelProfileId = idConverter.ToPublicId(model.ChannelProfileId),
                Name = model.Identifier,
                IsActive = model.DeletedDate.HasValue == false,
            };
        }
    }
}