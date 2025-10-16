using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlReportsRepository : IReportsRepository
    {
        private readonly QuiviContext context;

        public SqlReportsRepository(QuiviContext context)
        {
            this.context = context;
        }

        #region Sales
        public Task<IPagedData<Sales>> GetSalesAsync(GetSalesCriteria criteria)
        {
            IQueryable<PosCharge> query = Filter(criteria);

            if (criteria.Period.HasValue == false)
                return NoPeriod(criteria, query);

            switch (criteria.Period.Value)
            {
                case SalesPeriod.Hourly: return Hourly(criteria, query);
                case SalesPeriod.Daily: return Daily(criteria, query);
                case SalesPeriod.Monthly: return Monthly(criteria, query);
            }

            throw new NotImplementedException();
        }

        private static async Task<IPagedData<Sales>> NoPeriod(GetSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = await query.GroupBy(g => true)
                            .Select(s => new
                            {
                                From = s.Min(i => i.CaptureDate!.Value),
                                To = s.Max(i => i.CaptureDate!.Value),

                                Total = s.Sum(i => i.Total),
                                Payment = s.Sum(i => i.Payment),
                                Tip = s.Sum(i => i.Tip),
                                TotalRefund = s.Sum(i => i.TotalRefund),
                                PaymentRefund = s.Sum(i => i.PaymentRefund),
                                TipRefund = s.Sum(i => i.TipRefund),
                            }).FirstOrDefaultAsync();


            var data = new List<Sales>();
            if (preparedQuery != null)
                data.Add(new Sales
                {
                    From = preparedQuery.From,
                    To = preparedQuery.To,
                    Total = preparedQuery.Total,
                    Payment = preparedQuery.Payment,
                    Tip = preparedQuery.Tip,
                    TotalRefund = preparedQuery.TotalRefund ?? 0.0m,
                    PaymentRefund = preparedQuery.PaymentRefund ?? 0.0m,
                    TipRefund = preparedQuery.TipRefund ?? 0.0m,
                });

            return new PagedData<Sales>(data)
            {
                CurrentPage = 0,
                NumberOfPages = 1,
                TotalItems = 1,
            };
        }

        private static async Task<IPagedData<Sales>> Hourly(GetSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.GroupBy(g => new { g.CaptureDate!.Value.Date, g.CaptureDate!.Value.Hour })
                            .Select(s => new
                            {
                                Date = s.Key.Date,
                                Hour = s.Key.Hour,

                                Total = s.Sum(i => i.Total),
                                Payment = s.Sum(i => i.Payment),
                                Tip = s.Sum(i => i.Tip),
                                TotalRefund = s.Sum(i => i.TotalRefund),
                                PaymentRefund = s.Sum(i => i.PaymentRefund),
                                TipRefund = s.Sum(i => i.TipRefund),
                            })
                            .OrderBy(s => s.Date)
                            .ThenBy(s => s.Hour);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new Sales
            {
                From = s.Date.AddHours(s.Hour),
                To = s.Date.AddHours(s.Hour + 1),

                Total = s.Total,
                Payment = s.Payment,
                Tip = s.Tip,

                TotalRefund = s.TotalRefund ?? 0.0m,
                PaymentRefund = s.PaymentRefund ?? 0.0m,
                TipRefund = s.TipRefund ?? 0.0m,
            }).ToList();

            return new PagedData<Sales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<Sales>> Daily(GetSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.GroupBy(g => g.CaptureDate!.Value.Date).Select(s => new
            {
                Date = s.Key,
                Total = s.Sum(i => i.Total),
                Payment = s.Sum(i => i.Payment),
                Tip = s.Sum(i => i.Tip),
                TotalRefund = s.Sum(i => i.TotalRefund),
                PaymentRefund = s.Sum(i => i.PaymentRefund),
                TipRefund = s.Sum(i => i.TipRefund),
            }).OrderBy(s => s.Date);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new Sales
            {
                From = s.Date,
                To = s.Date.AddDays(1),

                Total = s.Total,
                Payment = s.Payment,
                Tip = s.Tip,

                TotalRefund = s.TotalRefund ?? 0.0m,
                PaymentRefund = s.PaymentRefund ?? 0.0m,
                TipRefund = s.TipRefund ?? 0.0m,
            }).ToList();
            return new PagedData<Sales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<Sales>> Monthly(GetSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.GroupBy(g => new { g.CaptureDate!.Value.Year, g.CaptureDate!.Value.Month })
                            .Select(s => new
                            {
                                Year = s.Key.Year,
                                Month = s.Key.Month,

                                Total = s.Sum(i => i.Total),
                                Payment = s.Sum(i => i.Payment),
                                Tip = s.Sum(i => i.Tip),
                                TotalRefund = s.Sum(i => i.TotalRefund),
                                PaymentRefund = s.Sum(i => i.PaymentRefund),
                                TipRefund = s.Sum(i => i.TipRefund),
                            }).OrderBy(s => s.Year).ThenBy(s => s.Month);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s =>
            {
                var from = new DateTime(s.Year, s.Month, 1);
                return new Sales
                {
                    From = from,
                    To = from.AddMonths(1),

                    Total = s.Total,
                    Payment = s.Payment,
                    Tip = s.Tip,

                    TotalRefund = s.TotalRefund ?? 0.0m,
                    PaymentRefund = s.PaymentRefund ?? 0.0m,
                    TipRefund = s.TipRefund ?? 0.0m,
                };
            }).ToList();

            return new PagedData<Sales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private IQueryable<PosCharge> Filter(GetSalesCriteria criteria)
        {
            IQueryable<PosCharge> query = context.PosCharges.Where(s => s!.CaptureDate.HasValue);

            if (criteria.ParentMerchantIds != null)
                query = query.Where(item => item.Merchant!.ParentMerchantId.HasValue && criteria.ParentMerchantIds!.Contains(item.Merchant.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(item => criteria.MerchantIds!.Contains(item.MerchantId));

            if (criteria.From != null)
                query = query.Where(item => criteria.From <= item.CaptureDate);

            if (criteria.To != null)
                query = query.Where(item => item.CaptureDate < criteria.To);

            return query;
        }
        #endregion

        #region ProductSales
        public Task<IPagedData<ProductSales>> GetProductSalesAsync(GetProductSalesCriteria criteria)
        {
            IQueryable<PosCharge> query = Filter(criteria);

            if (criteria.Period.HasValue == false)
                return NoPeriod(criteria, query);

            switch (criteria.Period.Value)
            {
                case SalesPeriod.Hourly: return Hourly(criteria, query);
                case SalesPeriod.Daily: return Daily(criteria, query);
                case SalesPeriod.Monthly: return Monthly(criteria, query);
            }

            throw new NotImplementedException();
        }

        private static async Task<IPagedData<ProductSales>> NoPeriod(GetProductSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.SelectMany(g => g.PosChargeInvoiceItems!)
                            .GroupBy(g => g.OrderMenuItem!.MenuItemId)
                            .Select(s => new
                            {
                                From = s.Min(i => i.PosCharge!.CaptureDate!.Value),
                                To = s.Max(i => i.PosCharge!.CaptureDate!.Value),

                                MenuItemId = s.Key,
                                TotalSoldQuantity = s.Sum(i => i.Quantity),
                                TotalBilledAmount = s.Sum(i => i.OrderMenuItem!.FinalPrice * i.Quantity),
                            })
                            .OrderByDescending(a => criteria.SortBy == ProductSalesSortBy.SoldQuantity ? a.TotalSoldQuantity : a.TotalBilledAmount);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new ProductSales
            {
                From = s.From,
                To = s.To,

                MenuItemId = s.MenuItemId,

                TotalQuantity = s.TotalSoldQuantity,
                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();

            return new PagedData<ProductSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<ProductSales>> Hourly(GetProductSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.SelectMany(g => g.PosChargeInvoiceItems!)
                            .GroupBy(g => new { g.OrderMenuItem!.MenuItemId, g.PosCharge!.CaptureDate!.Value.Date, g.PosCharge!.CaptureDate!.Value.Hour })
                            .Select(s => new
                            {
                                Date = s.Key.Date,
                                Hour = s.Key.Hour,

                                MenuItemId = s.Key.MenuItemId,
                                TotalSoldQuantity = s.Sum(i => i.Quantity),
                                TotalBilledAmount = s.Sum(i => i.OrderMenuItem!.FinalPrice * i.Quantity),
                            })
                            .OrderBy(s => s.Date)
                            .ThenBy(s => s.Hour);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new ProductSales
            {
                From = s.Date.AddHours(s.Hour),
                To = s.Date.AddHours(s.Hour + 1),

                MenuItemId = s.MenuItemId,

                TotalQuantity = s.TotalSoldQuantity,
                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();

            return new PagedData<ProductSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<ProductSales>> Daily(GetProductSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.SelectMany(g => g.PosChargeInvoiceItems!)
                            .GroupBy(g => new { g.OrderMenuItem!.MenuItemId, g.PosCharge!.CaptureDate!.Value.Date })
                            .Select(s => new
                            {
                                Date = s.Key.Date,
                                MenuItemId = s.Key.MenuItemId,
                                TotalSoldQuantity = s.Sum(i => i.Quantity),
                                TotalBilledAmount = s.Sum(i => i.OrderMenuItem!.FinalPrice * i.Quantity),
                            }).OrderBy(s => s.Date);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new ProductSales
            {
                From = s.Date,
                To = s.Date.AddDays(1),

                MenuItemId = s.MenuItemId,

                TotalQuantity = s.TotalSoldQuantity,
                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();
            return new PagedData<ProductSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<ProductSales>> Monthly(GetProductSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var aux = query.SelectMany(g => g.PosChargeInvoiceItems!)
                            .GroupBy(g => new { g.OrderMenuItem!.MenuItemId, g.PosCharge!.CaptureDate!.Value.Year, g.PosCharge!.CaptureDate!.Value.Month })
                            .Select(s => new
                            {
                                Year = s.Key.Year,
                                Month = s.Key.Month,

                                MenuItemId = s.Key.MenuItemId,
                                TotalSoldQuantity = s.Sum(i => i.Quantity),
                                TotalBilledAmount = s.Sum(i => i.OrderMenuItem!.FinalPrice * i.Quantity),
                            }).OrderBy(s => s.Year).ThenBy(s => s.Month);

            var pagedResult = await aux.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s =>
            {
                var from = new DateTime(s.Year, s.Month, 1);
                return new ProductSales
                {
                    From = from,
                    To = from.AddMonths(1),

                    MenuItemId = s.MenuItemId,

                    TotalQuantity = s.TotalSoldQuantity,
                    TotalBilledAmount = s.TotalBilledAmount,
                };
            }).ToList();

            return new PagedData<ProductSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private IQueryable<PosCharge> Filter(GetProductSalesCriteria criteria)
        {
            IQueryable<PosCharge> query = context.PosCharges.Where(s => s!.CaptureDate.HasValue);

            if (criteria.ParentMerchantIds != null)
                query = query.Where(item => item.Merchant!.ParentMerchantId.HasValue && criteria.ParentMerchantIds!.Contains(item.Merchant.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(item => criteria.MerchantIds!.Contains(item.MerchantId));

            if (criteria.From != null)
                query = query.Where(item => criteria.From <= item.CaptureDate);

            if (criteria.To != null)
                query = query.Where(item => item.CaptureDate < criteria.To);

            return query;
        }
        #endregion

        #region CategorySales
        public Task<IPagedData<CategorySales>> GetCategorySalesAsync(GetCategorySalesCriteria criteria)
        {
            IQueryable<PosCharge> query = Filter(criteria);

            if (criteria.Period.HasValue == false)
                return NoPeriod(criteria, query);

            switch (criteria.Period.Value)
            {
                case SalesPeriod.Hourly: return Hourly(criteria, query);
                case SalesPeriod.Daily: return Daily(criteria, query);
                case SalesPeriod.Monthly: return Monthly(criteria, query);
            }

            throw new NotImplementedException();
        }

        private static async Task<IPagedData<CategorySales>> NoPeriod(GetCategorySalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.SelectMany(g => g.PosChargeInvoiceItems!)
                                        .SelectMany(g => g.OrderMenuItem!.MenuItem!.MenuItemCategoryAssociations!.Select(c => new
                                        {
                                            CaptureDate = g.PosCharge!.CaptureDate!.Value,
                                            Quantity = g.Quantity,
                                            FinalPrice = g.OrderMenuItem!.FinalPrice,
                                            MenuCategoryId = c.ItemCategoryId,
                                        }))
                                        .GroupBy(g => g.MenuCategoryId)
                                        .Select(s => new
                                        {
                                            From = s.Min(i => i.CaptureDate),
                                            To = s.Max(i => i.CaptureDate),

                                            MenuCategoryId = s.Key,
                                            TotalSoldQuantity = s.Sum(i => i.Quantity),
                                            TotalBilledAmount = s.Sum(i => i.FinalPrice * i.Quantity),
                                        })
                                        .OrderByDescending(a => criteria.SortBy == ProductSalesSortBy.SoldQuantity ? a.TotalSoldQuantity : a.TotalBilledAmount);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new CategorySales
            {
                From = s.From,
                To = s.To,

                MenuCategoryId = s.MenuCategoryId,

                TotalQuantity = s.TotalSoldQuantity,
                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();

            return new PagedData<CategorySales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<CategorySales>> Hourly(GetCategorySalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.SelectMany(g => g.PosChargeInvoiceItems!)
                                        .SelectMany(g => g.OrderMenuItem!.MenuItem!.MenuItemCategoryAssociations!.Select(c => new
                                        {
                                            CaptureDate = g.PosCharge!.CaptureDate!.Value,
                                            Quantity = g.Quantity,
                                            FinalPrice = g.OrderMenuItem!.FinalPrice,
                                            MenuCategoryId = c.ItemCategoryId,
                                        }))
                                        .GroupBy(g => new { g.MenuCategoryId, g.CaptureDate.Date, g.CaptureDate.Hour })
                                        .Select(s => new
                                        {
                                            Date = s.Key.Date,
                                            Hour = s.Key.Hour,

                                            MenuCategoryId = s.Key.MenuCategoryId,
                                            TotalSoldQuantity = s.Sum(i => i.Quantity),
                                            TotalBilledAmount = s.Sum(i => i.FinalPrice * i.Quantity),
                                        })
                                        .OrderBy(s => s.Date)
                                        .ThenBy(s => s.Hour);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new CategorySales
            {
                From = s.Date.AddHours(s.Hour),
                To = s.Date.AddHours(s.Hour + 1),

                MenuCategoryId = s.MenuCategoryId,

                TotalQuantity = s.TotalSoldQuantity,
                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();

            return new PagedData<CategorySales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<CategorySales>> Daily(GetCategorySalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.SelectMany(g => g.PosChargeInvoiceItems!)
                                        .SelectMany(g => g.OrderMenuItem!.MenuItem!.MenuItemCategoryAssociations!.Select(c => new
                                        {
                                            CaptureDate = g.PosCharge!.CaptureDate!.Value,
                                            Quantity = g.Quantity,
                                            FinalPrice = g.OrderMenuItem!.FinalPrice,
                                            MenuCategoryId = c.ItemCategoryId,
                                        }))
                                        .GroupBy(g => new { g.MenuCategoryId, g.CaptureDate.Date })
                                        .Select(s => new
                                        {
                                            Date = s.Key.Date,
                                            MenuCategoryId = s.Key.MenuCategoryId,
                                            TotalSoldQuantity = s.Sum(i => i.Quantity),
                                            TotalBilledAmount = s.Sum(i => i.FinalPrice * i.Quantity),
                                        }).OrderBy(s => s.Date);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new CategorySales
            {
                From = s.Date,
                To = s.Date.AddDays(1),

                MenuCategoryId = s.MenuCategoryId,

                TotalQuantity = s.TotalSoldQuantity,
                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();
            return new PagedData<CategorySales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<CategorySales>> Monthly(GetCategorySalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var aux = query.SelectMany(g => g.PosChargeInvoiceItems!)
                            .SelectMany(g => g.OrderMenuItem!.MenuItem!.MenuItemCategoryAssociations!.Select(c => new
                            {
                                CaptureDate = g.PosCharge!.CaptureDate!.Value,
                                Quantity = g.Quantity,
                                FinalPrice = g.OrderMenuItem!.FinalPrice,
                                MenuCategoryId = c.ItemCategoryId,
                            }))
                            .GroupBy(g => new { g.MenuCategoryId, g.CaptureDate.Year, g.CaptureDate.Month })
                            .Select(s => new
                            {
                                Year = s.Key.Year,
                                Month = s.Key.Month,

                                MenuCategoryId = s.Key.MenuCategoryId,
                                TotalSoldQuantity = s.Sum(i => i.Quantity),
                                TotalBilledAmount = s.Sum(i => i.FinalPrice * i.Quantity),
                            }).OrderBy(s => s.Year).ThenBy(s => s.Month);

            var pagedResult = await aux.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s =>
            {
                var from = new DateTime(s.Year, s.Month, 1);
                return new CategorySales
                {
                    From = from,
                    To = from.AddMonths(1),

                    MenuCategoryId = s.MenuCategoryId,

                    TotalQuantity = s.TotalSoldQuantity,
                    TotalBilledAmount = s.TotalBilledAmount,
                };
            }).ToList();

            return new PagedData<CategorySales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private IQueryable<PosCharge> Filter(GetCategorySalesCriteria criteria)
        {
            IQueryable<PosCharge> query = context.PosCharges.Where(s => s!.CaptureDate.HasValue);

            if (criteria.ParentMerchantIds != null)
                query = query.Where(item => item.Merchant!.ParentMerchantId.HasValue && criteria.ParentMerchantIds!.Contains(item.Merchant.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(item => criteria.MerchantIds!.Contains(item.MerchantId));

            if (criteria.From != null)
                query = query.Where(item => criteria.From <= item.CaptureDate);

            if (criteria.To != null)
                query = query.Where(item => item.CaptureDate < criteria.To);

            return query;
        }
        #endregion

        #region ChargeMethodSales
        public Task<IPagedData<ChargeMethodSales>> GetChargeMethodSalesAsync(GetChargeMethodSalesCriteria criteria)
        {
            IQueryable<PosCharge> query = Filter(criteria);

            if (criteria.Period.HasValue == false)
                return NoPeriod(criteria, query);

            switch (criteria.Period.Value)
            {
                case SalesPeriod.Hourly: return Hourly(criteria, query);
                case SalesPeriod.Daily: return Daily(criteria, query);
                case SalesPeriod.Monthly: return Monthly(criteria, query);
            }

            throw new NotImplementedException();
        }

        private static async Task<IPagedData<ChargeMethodSales>> NoPeriod(GetChargeMethodSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.GroupBy(g => g.Charge!.MerchantCustomCharge == null ? 0 : g.Charge!.MerchantCustomCharge.CustomChargeMethodId)
                                        .Select(s => new
                                        {
                                            From = s.Min(i => i.CaptureDate)!.Value,
                                            To = s.Max(i => i.CaptureDate)!.Value,

                                            CustomChargeMethodId = s.Key,
                                            TotalQuantity = s.Count(),
                                            TotalBilledAmount = s.Sum(i => i.Payment + i.Tip),
                                        })
                                        .OrderByDescending(a => criteria.SortBy == ProductSalesSortBy.SoldQuantity ? a.TotalQuantity : a.TotalBilledAmount);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new ChargeMethodSales
            {
                From = s.From,
                To = s.To,

                CustomChargeMethodId = s.CustomChargeMethodId == 0 ? null : s.CustomChargeMethodId,

                TotalQuantity = s.TotalQuantity,
                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();

            return new PagedData<ChargeMethodSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<ChargeMethodSales>> Hourly(GetChargeMethodSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.GroupBy(g => new { CustomChargeMethodId = g.Charge!.MerchantCustomCharge == null ? 0 : g.Charge!.MerchantCustomCharge.CustomChargeMethodId, g.CaptureDate!.Value.Date, g.CaptureDate!.Value.Hour })
                                        .Select(s => new
                                        {
                                            Date = s.Key.Date,
                                            Hour = s.Key.Hour,

                                            CustomChargeMethodId = s.Key.CustomChargeMethodId,
                                            TotalQuantity = s.Count(),
                                            TotalBilledAmount = s.Sum(i => i.Payment + i.Tip),
                                        })
                                        .OrderBy(s => s.Date)
                                        .ThenBy(s => s.Hour);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new ChargeMethodSales
            {
                From = s.Date.AddHours(s.Hour),
                To = s.Date.AddHours(s.Hour + 1),

                CustomChargeMethodId = s.CustomChargeMethodId == 0 ? null : s.CustomChargeMethodId,

                TotalQuantity = s.TotalQuantity,
                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();

            return new PagedData<ChargeMethodSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<ChargeMethodSales>> Daily(GetChargeMethodSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.GroupBy(g => new { CustomChargeMethodId = g.Charge!.MerchantCustomCharge == null ? 0 : g.Charge!.MerchantCustomCharge.CustomChargeMethodId, g.CaptureDate!.Value.Date })
                                        .Select(s => new
                                        {
                                            Date = s.Key.Date,
                                            CustomChargeMethodId = s.Key.CustomChargeMethodId,
                                            TotalQuantity = s.Count(),
                                            TotalBilledAmount = s.Sum(i => i.Payment + i.Tip),
                                        }).OrderBy(s => s.Date);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new ChargeMethodSales
            {
                From = s.Date,
                To = s.Date.AddDays(1),

                CustomChargeMethodId = s.CustomChargeMethodId == 0 ? null : s.CustomChargeMethodId,

                TotalQuantity = s.TotalQuantity,
                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();
            return new PagedData<ChargeMethodSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<ChargeMethodSales>> Monthly(GetChargeMethodSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var aux = query.GroupBy(g => new { CustomChargeMethodId = g.Charge!.MerchantCustomCharge == null ? 0 : g.Charge!.MerchantCustomCharge.CustomChargeMethodId, g.CaptureDate!.Value.Year, g.CaptureDate!.Value.Month })
                            .Select(s => new
                            {
                                Year = s.Key.Year,
                                Month = s.Key.Month,

                                CustomChargeMethodId = s.Key.CustomChargeMethodId,
                                TotalQuantity = s.Count(),
                                TotalBilledAmount = s.Sum(i => i.Payment + i.Tip),
                            }).OrderBy(s => s.Year).ThenBy(s => s.Month);

            var pagedResult = await aux.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s =>
            {
                var from = new DateTime(s.Year, s.Month, 1);
                return new ChargeMethodSales
                {
                    From = from,
                    To = from.AddMonths(1),

                    CustomChargeMethodId = s.CustomChargeMethodId == 0 ? null : s.CustomChargeMethodId,

                    TotalQuantity = s.TotalQuantity,
                    TotalBilledAmount = s.TotalBilledAmount,
                };
            }).ToList();

            return new PagedData<ChargeMethodSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private IQueryable<PosCharge> Filter(GetChargeMethodSalesCriteria criteria)
        {
            IQueryable<PosCharge> query = context.PosCharges.Where(s => s!.CaptureDate.HasValue);

            if (criteria.ParentMerchantIds != null)
                query = query.Where(item => item.Merchant!.ParentMerchantId.HasValue && criteria.ParentMerchantIds!.Contains(item.Merchant.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(item => criteria.MerchantIds!.Contains(item.MerchantId));

            if (criteria.From != null)
                query = query.Where(item => criteria.From <= item.CaptureDate);

            if (criteria.To != null)
                query = query.Where(item => item.CaptureDate < criteria.To);

            return query;
        }
        #endregion

        #region PartnerChargeMethodSales
        public Task<IPagedData<PartnerChargeMethodSales>> GetPartnerChargeMethodSalesAsync(GetPartnerChargeMethodSalesCriteria criteria)
        {
            IQueryable<PosCharge> query = Filter(criteria);

            if (criteria.Period.HasValue == false)
                return NoPeriod(criteria, query);

            switch (criteria.Period.Value)
            {
                case SalesPeriod.Hourly: return Hourly(criteria, query);
                case SalesPeriod.Daily: return Daily(criteria, query);
                case SalesPeriod.Monthly: return Monthly(criteria, query);
            }

            throw new NotImplementedException();
        }

        private static async Task<IPagedData<PartnerChargeMethodSales>> NoPeriod(GetPartnerChargeMethodSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.GroupBy(g => new { g.Charge!.ChargePartner, g.Charge.ChargeMethod })
                                        .Select(s => new
                                        {
                                            From = s.Min(i => i.CaptureDate)!.Value,
                                            To = s.Max(i => i.CaptureDate)!.Value,

                                            ChargePartner = s.Key.ChargePartner,
                                            ChargeMethod = s.Key.ChargeMethod,

                                            TotalSuccess = s.Count(i => i.Charge!.Status == ChargeStatus.Completed),
                                            TotalFailed = s.Count(i => i.Charge!.Status == ChargeStatus.Expired || i.Charge!.Status == ChargeStatus.Failed),
                                            TotalProcessing = s.Count(i => i.Charge!.Status == ChargeStatus.Requested || i.Charge!.Status == ChargeStatus.Processing),

                                            TotalBilledAmount = s.Sum(i => i.Payment + i.Tip),
                                        })
                                        .OrderByDescending(a => a.ChargePartner)
                                        .ThenBy(a => a.ChargeMethod);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new PartnerChargeMethodSales
            {
                From = s.From,
                To = s.To,

                ChargePartner = s.ChargePartner,
                ChargeMethod = s.ChargeMethod,

                TotalQuantity = s.TotalSuccess + s.TotalFailed + s.TotalProcessing,
                TotalSuccess = s.TotalSuccess,
                TotalFailed = s.TotalFailed,
                TotalProcessing = s.TotalProcessing,

                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();

            return new PagedData<PartnerChargeMethodSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<PartnerChargeMethodSales>> Hourly(GetPartnerChargeMethodSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.GroupBy(g => new { g.Charge!.ChargePartner, g.Charge.ChargeMethod, g.CaptureDate!.Value.Date, g.CaptureDate!.Value.Hour })
                                        .Select(s => new
                                        {
                                            Date = s.Key.Date,
                                            Hour = s.Key.Hour,

                                            ChargePartner = s.Key.ChargePartner,
                                            ChargeMethod = s.Key.ChargeMethod,

                                            TotalSuccess = s.Count(i => i.Charge!.Status == ChargeStatus.Completed),
                                            TotalFailed = s.Count(i => i.Charge!.Status == ChargeStatus.Expired || i.Charge!.Status == ChargeStatus.Failed),
                                            TotalProcessing = s.Count(i => i.Charge!.Status == ChargeStatus.Requested || i.Charge!.Status == ChargeStatus.Processing),

                                            TotalBilledAmount = s.Sum(i => i.Payment + i.Tip),
                                        })
                                        .OrderBy(s => s.Date)
                                        .ThenBy(s => s.Hour);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new PartnerChargeMethodSales
            {
                From = s.Date.AddHours(s.Hour),
                To = s.Date.AddHours(s.Hour + 1),

                ChargePartner = s.ChargePartner,
                ChargeMethod = s.ChargeMethod,

                TotalQuantity = s.TotalSuccess + s.TotalFailed + s.TotalProcessing,
                TotalSuccess = s.TotalSuccess,
                TotalFailed = s.TotalFailed,
                TotalProcessing = s.TotalProcessing,

                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();

            return new PagedData<PartnerChargeMethodSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<PartnerChargeMethodSales>> Daily(GetPartnerChargeMethodSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var preparedQuery = query.GroupBy(g => new { g.Charge!.ChargePartner, g.Charge.ChargeMethod, g.CaptureDate!.Value.Date })
                                        .Select(s => new
                                        {
                                            Date = s.Key.Date,

                                            ChargePartner = s.Key.ChargePartner,
                                            ChargeMethod = s.Key.ChargeMethod,

                                            TotalSuccess = s.Count(i => i.Charge!.Status == ChargeStatus.Completed),
                                            TotalFailed = s.Count(i => i.Charge!.Status == ChargeStatus.Expired || i.Charge!.Status == ChargeStatus.Failed),
                                            TotalProcessing = s.Count(i => i.Charge!.Status == ChargeStatus.Requested || i.Charge!.Status == ChargeStatus.Processing),

                                            TotalBilledAmount = s.Sum(i => i.Payment + i.Tip),
                                        }).OrderBy(s => s.Date);

            var pagedResult = await preparedQuery.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s => new PartnerChargeMethodSales
            {
                From = s.Date,
                To = s.Date.AddDays(1),

                ChargePartner = s.ChargePartner,
                ChargeMethod = s.ChargeMethod,

                TotalQuantity = s.TotalSuccess + s.TotalFailed + s.TotalProcessing,
                TotalSuccess = s.TotalSuccess,
                TotalFailed = s.TotalFailed,
                TotalProcessing = s.TotalProcessing,

                TotalBilledAmount = s.TotalBilledAmount,
            }).ToList();
            return new PagedData<PartnerChargeMethodSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private static async Task<IPagedData<PartnerChargeMethodSales>> Monthly(GetPartnerChargeMethodSalesCriteria criteria, IQueryable<PosCharge> query)
        {
            var aux = query.GroupBy(g => new { g.Charge!.ChargePartner, g.Charge.ChargeMethod, g.CaptureDate!.Value.Year, g.CaptureDate!.Value.Month })
                            .Select(s => new
                            {
                                Year = s.Key.Year,
                                Month = s.Key.Month,

                                ChargePartner = s.Key.ChargePartner,
                                ChargeMethod = s.Key.ChargeMethod,

                                TotalSuccess = s.Count(i => i.Charge!.Status == ChargeStatus.Completed),
                                TotalFailed = s.Count(i => i.Charge!.Status == ChargeStatus.Expired || i.Charge!.Status == ChargeStatus.Failed),
                                TotalProcessing = s.Count(i => i.Charge!.Status == ChargeStatus.Requested || i.Charge!.Status == ChargeStatus.Processing),

                                TotalBilledAmount = s.Sum(i => i.Payment + i.Tip),
                            }).OrderBy(s => s.Year).ThenBy(s => s.Month);

            var pagedResult = await aux.ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var data = pagedResult.Select(s =>
            {
                var from = new DateTime(s.Year, s.Month, 1);
                return new PartnerChargeMethodSales
                {
                    From = from,
                    To = from.AddMonths(1),

                    ChargePartner = s.ChargePartner,
                    ChargeMethod = s.ChargeMethod,

                    TotalQuantity = s.TotalSuccess + s.TotalFailed + s.TotalProcessing,
                    TotalSuccess = s.TotalSuccess,
                    TotalFailed = s.TotalFailed,
                    TotalProcessing = s.TotalProcessing,

                    TotalBilledAmount = s.TotalBilledAmount,
                };
            }).ToList();

            return new PagedData<PartnerChargeMethodSales>(data)
            {
                CurrentPage = pagedResult.CurrentPage,
                NumberOfPages = pagedResult.NumberOfPages,
                TotalItems = pagedResult.TotalItems,
            };
        }

        private IQueryable<PosCharge> Filter(GetPartnerChargeMethodSalesCriteria criteria)
        {
            IQueryable<PosCharge> query = context.PosCharges.Where(s => s!.CaptureDate.HasValue);

            if (criteria.ChargePartners != null)
                query = query.Where(item => criteria.ChargePartners!.Contains(item.Charge!.ChargePartner));

            if (criteria.ChargeMethods != null)
                query = query.Where(item => criteria.ChargeMethods!.Contains(item.Charge!.ChargeMethod));

            if (criteria.ParentMerchantIds != null)
                query = query.Where(item => item.Merchant!.ParentMerchantId.HasValue && criteria.ParentMerchantIds!.Contains(item.Merchant.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(item => criteria.MerchantIds!.Contains(item.MerchantId));

            if (criteria.From != null)
                query = query.Where(item => criteria.From <= item.CaptureDate);

            if (criteria.To != null)
                query = query.Where(item => item.CaptureDate < criteria.To);

            return query;
        }
        #endregion
    }
}