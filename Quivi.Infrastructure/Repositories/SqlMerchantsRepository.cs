using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;
using System.Linq.Expressions;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlMerchantsRepository : ARepository<Merchant, GetMerchantsCriteria>, IMerchantsRepository
    {
        public SqlMerchantsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Merchant> GetFilteredQueryable(GetMerchantsCriteria criteria)
        {
            IQueryable<Merchant> query = Set;

            if (criteria.IncludeChildMerchants)
                query = query.Include(q => q.ChildMerchants);

            if (criteria.IncludeParentMerchant)
                query = query.Include(q => q.ParentMerchant);
            
            if(criteria.IncludeFees)
                query = query.Include(q => q.Fees);

            if (criteria.Ids != null)
                query = query.Where(x => criteria.Ids.Contains(x.Id));

            if (criteria.ParentIds != null)
                query = query.Where(x => x.ParentMerchantId != null && criteria.ParentIds.Contains(x.ParentMerchantId.Value));

            if (criteria.ChildIds != null)
                query = query.Where(x => x.ChildMerchants!.Any(c => criteria.ChildIds.Contains(c.Id)));

            if (criteria.ChannelIds != null)
                query = query.Where(x => x.Channels!.Any(c => criteria.ChannelIds.Contains(c.Id)));

            if (criteria.IsDeleted.HasValue)
                query = query.Where(x => x.DeletedDate.HasValue == criteria.IsDeleted.Value);

            if (criteria.VatNumbers != null)
                query = query.Where(x => criteria.VatNumbers.Contains(x.VatNumber));

            if (criteria.ApplicationUserIds != null)
            {
                IQueryable<ApplicationUser> applicationUsers = Context.Users.Where(x => criteria.ApplicationUserIds.Contains(x.Id));

                //Direct access
                var accessToMerchantIds = applicationUsers.SelectMany(x => x.Merchants!).Select(m => m.Id);

                //Has Access to a child via parent
                var childMerchantIds = Set.Where(s => accessToMerchantIds.Contains(s.Id)).SelectMany(m => m.ChildMerchants!).Select(m => m.Id);
                
                //Has access to a parent via child
                var parentMerchantIds = Set.Where(s => s.ParentMerchantId.HasValue && accessToMerchantIds.Contains(s.Id)).Select(m => m.ParentMerchantId);

                query = query.Where(x => accessToMerchantIds.Contains(x.Id) || childMerchantIds.Contains(x.Id) || parentMerchantIds.Contains(x.Id));
            }

            if (string.IsNullOrEmpty(criteria.Search) == false)
            {
                Expression<Func<Merchant, bool>> baseExpression = r => r.Name.Contains(criteria.Search) || r.ChildMerchants!.Any(p => p.Name.Contains(criteria.Search));
                if (IsDigitsOnly(criteria.Search))
                {
                    var validVat = criteria.Search.IsValidNif();
                    if (validVat)
                    {
                        Expression<Func<Merchant, bool>> expression = r => r.VatNumber != null && r.VatNumber.Contains(criteria.Search);
                        query = query.Where(baseExpression.Combine(expression, (left, right) => left || right));
                    }
                    else
                    {
                        var merchantId = int.Parse(criteria.Search);
                        Expression<Func<Merchant, bool>> expression = r => r.ChildMerchants!.Any(p => p.Id == merchantId | p.ParentMerchantId == merchantId);
                        query = query.Where(baseExpression.Combine(expression, (left, right) => left || right));
                    }
                }
                else
                    query = query.Where(baseExpression);
            }

            if (criteria.IsParentMerchant.HasValue)
                query = query.Where(p => (p.ParentMerchantId == null) == criteria.IsParentMerchant.Value);

            return query.OrderByDescending(p => p.Id);
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}
