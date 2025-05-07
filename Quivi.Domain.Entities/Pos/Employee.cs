using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Entities.Notifications;

namespace Quivi.Domain.Entities.Pos
{
    public class Employee : IDeletableEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }
        public string? PosIdentifier { get; set; }
        public string? PinCodeHash { get; set; }
        public EmployeeRestrictions Restrictions { get; set; }
        public int? LogoutInactivityInSeconds { get; set; }
        public TimeSpan? LogoutInactivity
        {
            get => LogoutInactivityInSeconds.HasValue ? TimeSpan.FromSeconds(LogoutInactivityInSeconds.Value) : (TimeSpan?)null;
            set => LogoutInactivityInSeconds = value.HasValue ? (int)value.Value.TotalSeconds : (int?)null;
        }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }


        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        //TODO: I think this should be a one to optional instead of one to many
        public ICollection<EmployeeNotificationsContact>? EmployeeContacts { get; set; }
        public ICollection<PosNotificationInboxMessage>? PosNotificationInboxMessages { get; set; }
        public ICollection<Order>? Orders { get; set; }
        #endregion
    }
}