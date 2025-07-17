using Quivi.Domain.Entities;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class LanguageMapperHandler : IMapperHandler<string, Language?>
    {
        public Language? Map(string model)
        {
            switch (model.ToLower())
            {
                case "en":
                case "en-gb":
                case "en-us":
                    return Language.English;
                case "pt":
                case "pt-pt":
                case "pt-br":
                    return Language.Portuguese;
                default:
                    return null;
            }
        }
    }
}