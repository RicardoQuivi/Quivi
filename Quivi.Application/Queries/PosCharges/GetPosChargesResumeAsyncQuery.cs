using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using System.Linq.Expressions;

namespace Quivi.Application.Queries.PosCharges
{
    public class GetPosChargesResumeAsyncQuery : AGetPosChargesQuery, IQuery<Task<PosChargesResume>>
    {
    }

    public abstract class GetPosChargesResumeAsyncQuery<T> : AGetPosChargesQuery, IQuery<Task<IReadOnlyDictionary<T, PosChargesResume>>>
    {
        public abstract Expression<Func<PosCharge, T>> GroupBy { get; }
    }

    public class GetPosChargesResumeByEmployeeAsyncQuery : GetPosChargesResumeAsyncQuery<Employee>
    {
        public override Expression<Func<PosCharge, Employee>> GroupBy => m => m.Employee!;
    }

    public class GetPosChargesResumeByPaymentMethodAsyncQuery : GetPosChargesResumeAsyncQuery<CustomChargeMethod>
    {
        public override Expression<Func<PosCharge, CustomChargeMethod>> GroupBy => m => m.Charge!.MerchantCustomCharge!.CustomChargeMethod!;
    }

    public class GetPosChargesGroupedQueryHandler : IQueryHandler<GetPosChargesResumeAsyncQuery, Task<PosChargesResume>>,
                                                        IQueryHandler<GetPosChargesResumeByEmployeeAsyncQuery, Task<IReadOnlyDictionary<Employee, PosChargesResume>>>,
                                                        IQueryHandler<GetPosChargesResumeByPaymentMethodAsyncQuery, Task<IReadOnlyDictionary<CustomChargeMethod, PosChargesResume>>>
    {
        private readonly IPosChargesRepository repository;

        public GetPosChargesGroupedQueryHandler(IPosChargesRepository repository)
        {
            this.repository = repository;
        }

        public async Task<PosChargesResume> Handle(GetPosChargesResumeAsyncQuery query)
        {
            var result = await repository.GetResumeAsync(new GetPosChargesResumeCriteria
            {
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                SessionIds = query.SessionIds,
                OrderIds = query.OrderIds,
                LocationIds = query.LocationIds,
                IsCaptured = query.IsCaptured,
                FromCapturedDate = query.FromCapturedDate,
                ToCapturedDate = query.ToCapturedDate,
                HasSession = query.HasSession,
                HasDiscounts = query.HasDiscounts,
                HasReview = query.HasReview,
                HasReviewComment = query.HasReviewComment,
                CustomChargeMethodIds = query.CustomChargeMethodIds,
                HasRefunds = query.HasRefunds,
                SyncingState = query.SyncingState,
                QuiviPaymentsOnly = query.QuiviPaymentsOnly,
            }, t => true, true);

            if (result.Any() == false)
                return new PosChargesResume
                {
                    PaymentAmount = 0,
                    SurchageAmount = 0,
                    TipAmount = 0,
                };
            return result.First().Value;
        }

        private Task<IReadOnlyDictionary<T, PosChargesResume>> HandleGeneric<T>(GetPosChargesResumeAsyncQuery<T> query, T defaultKey)
        {
            return repository.GetResumeAsync(new GetPosChargesResumeCriteria
            {
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                SessionIds = query.SessionIds,
                OrderIds = query.OrderIds,
                LocationIds = query.LocationIds,
                IsCaptured = query.IsCaptured,
                FromCapturedDate = query.FromCapturedDate,
                ToCapturedDate = query.ToCapturedDate,
                HasSession = query.HasSession,
                HasDiscounts = query.HasDiscounts,
                HasReview = query.HasReview,
                HasReviewComment = query.HasReviewComment,
                CustomChargeMethodIds = query.CustomChargeMethodIds,
                HasRefunds = query.HasRefunds,
                SyncingState = query.SyncingState,
                QuiviPaymentsOnly = query.QuiviPaymentsOnly,
            }, query.GroupBy, defaultKey);
        }

        public Task<IReadOnlyDictionary<Employee, PosChargesResume>> Handle(GetPosChargesResumeByEmployeeAsyncQuery query) => HandleGeneric(query, new Employee
        {
            Id = 0,
            Name = "Anonymous",
        });

        public Task<IReadOnlyDictionary<CustomChargeMethod, PosChargesResume>> Handle(GetPosChargesResumeByPaymentMethodAsyncQuery query) => HandleGeneric(query, new CustomChargeMethod
        {
            Name = "Quivi",
            Logo = "/Images/Eats/logo.svg"
        });
    }
}
