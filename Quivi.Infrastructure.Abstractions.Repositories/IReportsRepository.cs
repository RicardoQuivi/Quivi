using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface IReportsRepository
    {
        Task<IPagedData<Sales>> GetSalesAsync(GetSalesCriteria criteria);
        Task<IPagedData<ProductSales>> GetProductSalesAsync(GetProductSalesCriteria criteria);
        Task<IPagedData<CategorySales>> GetCategorySalesAsync(GetCategorySalesCriteria criteria);
        Task<IPagedData<ChargeMethodSales>> GetChargeMethodSalesAsync(GetChargeMethodSalesCriteria criteria);
    }
}