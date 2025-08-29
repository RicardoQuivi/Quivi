using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class NotificationsTypesMapperHandler : IMapperHandler<NotificationMessageType, IEnumerable<Dtos.NotificationType>>,
                                                        IMapperHandler<IEnumerable<Dtos.NotificationType>, NotificationMessageType>
    {
        public IEnumerable<Dtos.NotificationType> Map(NotificationMessageType model)
        {
            foreach (NotificationMessageType restriction in Enum.GetValues(typeof(NotificationMessageType)))
            {
                if (restriction == NotificationMessageType.None)
                    continue;

                if (model.HasFlag(restriction))
                    yield return MapValue(restriction);
            }
        }

        private Dtos.NotificationType MapValue(NotificationMessageType model)
        {
            switch (model)
            {
                case NotificationMessageType.Test: return Dtos.NotificationType.Test;
                case NotificationMessageType.FailedCharge: return Dtos.NotificationType.FailedCharge;
                case NotificationMessageType.ExpiredCharge: return Dtos.NotificationType.ExpiredCharge;
                case NotificationMessageType.PosOffline: return Dtos.NotificationType.PosOffline;
                case NotificationMessageType.PosSyncFailure: return Dtos.NotificationType.PosSyncFailure;
                case NotificationMessageType.PosPaymentSyncFailure: return Dtos.NotificationType.PosPaymentSyncFailure;
                case NotificationMessageType.CompletedCharge: return Dtos.NotificationType.CompletedCharge;
                case NotificationMessageType.NewOrder: return Dtos.NotificationType.NewOrder;
                case NotificationMessageType.NewReview: return Dtos.NotificationType.NewReview;
                case NotificationMessageType.ConsumerBill: return Dtos.NotificationType.NewConsumerBill;
                case NotificationMessageType.NewConsumerInvoice: return Dtos.NotificationType.NewConsumerInvoice;
                case NotificationMessageType.NewPreparationRequest: return Dtos.NotificationType.NewPreparationRequest;
                case NotificationMessageType.ChargeSynced: return Dtos.NotificationType.ChargeSynced;
                case NotificationMessageType.OpenCashDrawer: return Dtos.NotificationType.OpenCashDrawer;
                case NotificationMessageType.EndOfDayClosing: return Dtos.NotificationType.EndOfDayClosing;
            }
            throw new NotImplementedException();
        }

        public NotificationMessageType Map(IEnumerable<Dtos.NotificationType> model)
        {
            NotificationMessageType result = NotificationMessageType.None;
            foreach (var flag in model)
                result |= MapValue(flag);
            return result;
        }

        private NotificationMessageType MapValue(Dtos.NotificationType model)
        {
            switch (model)
            {
                case Dtos.NotificationType.Test: return NotificationMessageType.Test;
                case Dtos.NotificationType.FailedCharge: return NotificationMessageType.FailedCharge;
                case Dtos.NotificationType.ExpiredCharge: return NotificationMessageType.ExpiredCharge;
                case Dtos.NotificationType.PosOffline: return NotificationMessageType.PosOffline;
                case Dtos.NotificationType.PosSyncFailure: return NotificationMessageType.PosSyncFailure;
                case Dtos.NotificationType.PosPaymentSyncFailure: return NotificationMessageType.PosPaymentSyncFailure;
                case Dtos.NotificationType.CompletedCharge: return NotificationMessageType.CompletedCharge;
                case Dtos.NotificationType.NewOrder: return NotificationMessageType.NewOrder;
                case Dtos.NotificationType.NewReview: return NotificationMessageType.NewReview;
                case Dtos.NotificationType.NewConsumerBill: return NotificationMessageType.ConsumerBill;
                case Dtos.NotificationType.NewConsumerInvoice: return NotificationMessageType.NewConsumerInvoice;
                case Dtos.NotificationType.NewPreparationRequest: return NotificationMessageType.NewPreparationRequest;
                case Dtos.NotificationType.ChargeSynced: return NotificationMessageType.ChargeSynced;
                case Dtos.NotificationType.OpenCashDrawer: return NotificationMessageType.OpenCashDrawer;
                case Dtos.NotificationType.EndOfDayClosing: return NotificationMessageType.EndOfDayClosing;
            }
            throw new NotImplementedException();
        }
    }
}
