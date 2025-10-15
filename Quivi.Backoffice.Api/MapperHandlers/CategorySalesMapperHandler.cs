using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class CategorySalesMapperHandler : IMapperHandler<CategorySales, Dtos.CategorySales>
    {
        private readonly IIdConverter idConverter;

        public CategorySalesMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.CategorySales Map(CategorySales model)
        {
            return new Dtos.CategorySales
            {
                MenuCategoryId = idConverter.ToPublicId(model.MenuCategoryId),

                From = new DateTimeOffset(model.From, TimeSpan.Zero),
                To = new DateTimeOffset(model.To, TimeSpan.Zero),

                TotalItemsSoldQuantity = model.TotalQuantity,
                TotalBilledAmount = model.TotalBilledAmount,
            };
        }
    }
}