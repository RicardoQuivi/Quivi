using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class ProductSalesMapperHandler : IMapperHandler<ProductSales, Dtos.ProductSales>
    {
        private readonly IIdConverter idConverter;

        public ProductSalesMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.ProductSales Map(ProductSales model)
        {
            return new Dtos.ProductSales
            {
                MenuItemId = idConverter.ToPublicId(model.MenuItemId),

                From = new DateTimeOffset(model.From, TimeSpan.Zero),
                To = new DateTimeOffset(model.To, TimeSpan.Zero),

                TotalSoldQuantity = model.TotalQuantity,
                TotalBilledAmount = model.TotalBilledAmount,
            };
        }
    }
}