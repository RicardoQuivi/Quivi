using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Entities.Identity;
using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework.Extensions;
using Quivi.Domain.Repositories.EntityFramework.Functions;
using Quivi.Domain.Repositories.EntityFramework.Identity;

namespace Quivi.Domain.Repositories.EntityFramework
{
    public class QuiviContext : IdentityDbContext<ApplicationUser,
                                                    ApplicationRole,
                                                    int,
                                                    ApplicationUserClaim,
                                                    ApplicationUserRole,
                                                    ApplicationUserLogin,
                                                    ApplicationRoleClaim,
                                                    ApplicationUserToken>
    {
        public QuiviContext(DbContextOptions<QuiviContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            BuildIdentity(modelBuilder);
            BuildQuivi(modelBuilder);
            BuildFunctions(modelBuilder);
        }

        private void BuildIdentity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasMany(m => m.ApiClients)
                        .WithOne()
                        .HasForeignKey(c => c.UserId);

                entity.HasOne(m => m.Person)
                        .WithOne()
                        .HasForeignKey<Person>(c => c.UserId);

                entity.HasMany(m => m.Merchants)
                        .WithMany();
            });

            modelBuilder.Entity<ApplicationUserClaim>(entity =>
            {
                entity.Property(m => m.ClaimType).HasMaxLength(128);
                entity.HasIndex(m => new { m.UserId, m.ClaimType }).IsUnique();
            });
        }

        private void BuildQuivi(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PrinterWorker>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasIndex(m => m.Identifier).IsUnique();

                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.PrinterWorkers)
                        .HasForeignKey(m => m.MerchantId);

                entity.HasMany(m => m.PrinterNotificationsContacts)
                        .WithOne(m => m.PrinterWorker)
                        .HasForeignKey(m => m.PrinterWorkerId);
            });

            modelBuilder.Entity<OrderAdditionalInfo>(entity =>
            {
                entity.HasKey(m => new { m.OrderId, m.OrderConfigurableFieldId });
                entity.HasOne(m => m.Order)
                        .WithMany(m => m.OrderAdditionalInfos)
                        .HasForeignKey(m => m.OrderId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.OrderConfigurableField)
                        .WithMany(m => m.OrderAdditionalInfos)
                        .HasForeignKey(m => m.OrderConfigurableFieldId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.Property(m => m.Value).IsRequired();
            });

            modelBuilder.Entity<OrderConfigurableFieldTranslation>(entity =>
            {
                entity.HasKey(m => new { m.OrderConfigurableFieldId, m.Language });
                entity.HasOne(m => m.OrderConfigurableField)
                        .WithMany(m => m.Translations)
                        .HasForeignKey(m => m.OrderConfigurableFieldId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.Property(m => m.Name).IsRequired();

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<OrderConfigurableField>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Name).IsRequired();
                entity.Property(m => m.DefaultValue).IsRequired(false);

                entity.HasMany(m => m.AssociatedChannelProfiles)
                        .WithOne(m => m.OrderConfigurableField)
                        .HasForeignKey(m => m.ChannelProfileId);

                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.OrderConfigurableFields)
                        .HasForeignKey(m => m.MerchantId);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<ChannelProfile>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.ChannelProfiles)
                        .HasForeignKey(m => m.MerchantId)
                        .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(m => m.Channels)
                        .WithOne(m => m.ChannelProfile)
                        .HasForeignKey(m => m.ChannelProfileId);

                entity.Ignore(m => m.SendToPreparationTimer);

                entity.HasMany(m => m.AssociatedOrderConfigurableFields)
                        .WithOne(m => m.ChannelProfile)
                        .HasForeignKey(m => m.ChannelProfileId)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.PosIntegration)
                        .WithMany(m => m.ChannelProfiles)
                        .HasForeignKey(m => m.PosIntegrationId)
                        .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(m => m.AssociatedAvailabilityGroups)
                        .WithOne(m => m.ChannelProfile)
                        .HasForeignKey(m => m.ChannelProfileId);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<OrderConfigurableFieldChannelProfileAssociation>(entity =>
            {
                entity.HasKey(m => new { m.OrderConfigurableFieldId, m.ChannelProfileId });

                entity.HasOne(m => m.ChannelProfile)
                        .WithMany(m => m.AssociatedOrderConfigurableFields)
                        .HasForeignKey(m => m.ChannelProfileId)
                        .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(m => m.OrderConfigurableField)
                        .WithMany(m => m.AssociatedChannelProfiles)
                        .HasForeignKey(m => m.OrderConfigurableFieldId)
                        .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.Locations)
                        .HasForeignKey(m => m.MerchantId);

                entity.HasMany(m => m.MenuItems)
                        .WithOne(m => m.Location)
                        .HasForeignKey(m => m.LocationId);

                entity.HasMany(m => m.PrinterNotificationsContacts)
                        .WithOne(m => m.Location)
                        .HasForeignKey(m => m.LocationId);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<CustomChargeMethod>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.CustomChargeMethods)
                        .HasForeignKey(m => m.MerchantId);
            });

            modelBuilder.Entity<Deposit>(entity =>
            {
                entity.HasKey(m => m.ChargeId);
                entity.HasOne(c => c.Charge)
                        .WithOne(d => d.Deposit)
                        .HasForeignKey<Deposit>(d => d.ChargeId);

                entity.HasOne(r => r.Consumer)
                            .WithMany()
                            .HasForeignKey(r => r.ConsumerId)
                            .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Charge>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.ChainedCharge)
                    .WithMany()
                    .HasForeignKey(c => c.ChainedChargeId);

                entity.HasOne(c => c.MerchantCustomCharge)
                        .WithOne(c => c.Charge)
                        .HasForeignKey<Charge>(c => c.Id);
            });

            modelBuilder.Entity<DepositCapture>(entity =>
            {
                entity.HasKey(m => m.DepositId);
                entity.HasOne(c => c.Deposit)
                        .WithOne(d => d.DepositCapture)
                        .HasForeignKey<DepositCapture>(d => d.DepositId);

                entity.HasOne(m => m.Person)
                        .WithMany()
                        .HasForeignKey(m => m.PersonId);
            });

            modelBuilder.Entity<DepositSurchargeJournal>(entity =>
            {
                entity.HasKey(m => m.DepositId);
                entity.HasOne(c => c.Deposit)
                        .WithOne(d => d.DepositSurchargeJournal)
                        .HasForeignKey<DepositSurchargeJournal>(d => d.DepositId);

                entity.HasOne(m => m.Journal)
                        .WithMany(m => m.DepositSurchargeJournals)
                        .HasForeignKey(m => m.JournalId);
            });

            modelBuilder.Entity<DepositSurcharge>(entity =>
            {
                entity.HasKey(m => m.DepositId);
                entity.HasOne(c => c.Deposit)
                        .WithOne(d => d.DepositSurchage)
                        .HasForeignKey<DepositSurcharge>(d => d.DepositId);
            });

            modelBuilder.Entity<DepositJournal>(entity =>
            {
                entity.HasKey(m => m.DepositId);
                entity.HasOne(c => c.Deposit)
                        .WithOne(d => d.DepositJournal)
                        .HasForeignKey<DepositJournal>(d => d.DepositId);

                entity.HasOne(m => m.Journal)
                        .WithMany(m => m.DepositJournals)
                        .HasForeignKey(m => m.JournalId);
            });

            modelBuilder.Entity<DepositCaptureJournal>(entity =>
            {
                entity.HasKey(m => m.DepositId);
                entity.HasOne(c => c.Deposit)
                        .WithOne(d => d.DepositCaptureJournal)
                        .HasForeignKey<DepositCaptureJournal>(d => d.DepositId);

                entity.HasOne(m => m.Journal)
                        .WithMany(m => m.DepositCaptureJournals)
                        .HasForeignKey(m => m.JournalId);
            });

            modelBuilder.Entity<MerchantInvoiceDocument>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.DocumentId)
                        .HasMaxLength(600)
                        .IsRequired();

                entity.Property(m => m.DocumentReference)
                        .HasMaxLength(600)
                        .IsRequired(false);

                entity.Property(m => m.Path).IsRequired(false);

                entity.HasOne(m => m.Merchant)
                    .WithMany(m => m.InvoiceDocuments)
                    .HasForeignKey(m => m.MerchantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Charge)
                        .WithMany(m => m.InvoiceDocuments)
                        .HasForeignKey(m => m.ChargeId);
            });

            modelBuilder.Entity<MerchantCustomCharge>(entity =>
            {
                entity.HasKey(c => c.ChargeId);
                entity.HasOne(c => c.Charge)
                        .WithOne(c => c.MerchantCustomCharge)
                        .HasForeignKey<MerchantCustomCharge>(c => c.ChargeId);
            });

            modelBuilder.Entity<ApiClient>(entity =>
            {
                entity.HasKey(m => m.ApiClientId);

                entity.HasOne(m => m.Person)
                        .WithMany(m => m.ApiClients)
                        .HasForeignKey(m => m.PersonId);

                entity.Property(m => m.UserName)
                        .HasMaxLength(254);

                entity.Property(m => m.Password)
                        .HasMaxLength(254);

                entity.HasIndex(m => m.UserName)
                        .IsUnique();

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<ApiClientRequest>(entity =>
            {
                entity.HasKey(m => m.ApiClientRequestId);
                entity.Property(m => m.RedeemCode).HasMaxLength(6);
                entity.HasOne(m => m.ApiClient)
                        .WithMany(m => m.ApiClientRequests)
                        .HasForeignKey(m => m.ApiClientId);
                entity.HasOne(m => m.SubMerchant)
                        .WithMany()
                        .HasForeignKey(m => m.SubMerchantId);
            });

            modelBuilder.Entity<Journal>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Type).IsRequired();
                entity.Property(m => m.State).IsRequired();
                entity.Property(m => m.CreatedDate).IsRequired();
                entity.HasOne(m => m.JournalLink)
                                    .WithMany()
                                    .HasForeignKey(m => m.JournalLinkId);

                entity.HasOne(b => b.JournalDetails)
                        .WithOne(b => b.Journal)
                        .HasForeignKey<Journal>(m => m.Id);
            });

            modelBuilder.Entity<DepositRefundJournal>(entity =>
            {
                entity.HasKey(m => m.DepositId);

                entity.HasOne(m => m.Deposit)
                        .WithOne(m => m.DepositRefundJournal)
                        .HasForeignKey<DepositRefundJournal>(m => m.DepositId);

                entity.HasOne(m => m.Journal)
                        .WithMany(m => m.DepositRefundJournals)
                        .HasForeignKey(m => m.JournalId);
            });

            modelBuilder.Entity<JournalChange>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Type).IsRequired();
                entity.Property(m => m.State).IsRequired();
                entity.Property(m => m.Amount).IsRequired();
                entity.Property(m => m.CreatedDate).IsRequired();
                entity.HasOne(m => m.Journal)
                        .WithMany()
                        .HasForeignKey(m => m.JournalId);
                entity.HasOne(m => m.JournalLink)
                        .WithMany()
                        .HasForeignKey(m => m.JournalLinkId);
            });

            modelBuilder.Entity<JournalDetails>(entity =>
            {
                entity.HasKey(m => m.JournalId);
                entity.HasOne(m => m.Journal)
                        .WithOne(m => m.JournalDetails)
                        .HasForeignKey<JournalDetails>(m => m.JournalId);
            });

            modelBuilder.Entity<MerchantAcquirerConfiguration>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.AcquirerConfigurations)
                        .HasForeignKey(m => m.MerchantId);

                entity.HasIndex(m => m.ChargeMethod);
                entity.HasIndex(m => m.ChargePartner);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.ParentMerchant)
                        .WithMany(m => m.ChildMerchants)
                        .HasForeignKey(m => m.ParentMerchantId);

                entity.HasOne(m => m.SetUpFee)
                                    .WithMany()
                                    .HasForeignKey(m => m.SetUpFeeId);

                entity.HasMany(m => m.CustomChargeMethods)
                        .WithOne(m => m.Merchant)
                        .HasForeignKey(m => m.MerchantId);

                entity.HasMany(m => m.PosIntegrations)
                        .WithOne(i => i.Merchant)
                        .HasForeignKey(i => i.MerchantId);

                entity.HasMany(m => m.AcquirerConfigurations)
                        .WithOne(i => i.Merchant)
                        .HasForeignKey(i => i.MerchantId);

                entity.Ignore(m => m.TransactionFees);
                entity.Ignore(m => m.SurchargeFees);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<MerchantService>(entity =>
            {
                entity.HasKey(m => m.PersonId);
                entity.HasOne(m => m.Person)
                        .WithOne(m => m.MerchantService)
                        .HasForeignKey<MerchantService>(m => m.PersonId);

                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.MerchantServices)
                        .HasForeignKey(m => m.MerchantId);
            });

            modelBuilder.Entity<MerchantFile>(entity =>
            {
                entity.HasKey(m => m.MerchantFileId);
                entity.Property(m => m.FileUrl).IsRequired();
                entity.Property(m => m.FileMetadata).IsRequired();
                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.Files)
                        .HasForeignKey(m => m.MerchantId);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<MerchantFee>(entity =>
            {
                entity.HasKey(m => new { m.MerchantId, m.ChargeMethod, m.FeeType });
                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.Fees);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.PhoneNumber).HasMaxLength(15);
                entity.HasOne(p => p.Merchant)
                        .WithMany()
                        .HasForeignKey(p => p.MerchantId);
                entity.HasOne(p => p.SubMerchant)
                        .WithMany(m => m.People)
                        .HasForeignKey(p => p.SubMerchantId);
                entity.HasIndex(p => p.IsAnonymous)
                        .IsUnique()
                        .HasFilter("[IsAnonymous] = 1");

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<Settlement>(entity =>
            {
                entity.HasKey(x => x.SettlementId);
            });

            modelBuilder.Entity<SettlementDetail>(entity =>
            {
                entity.HasKey(x => x.SettlementDetailId);
                entity.HasOne(x => x.Settlement)
                        .WithMany(x => x.SettlementDetails)
                        .HasForeignKey(x => x.SettlementId);
                entity.HasOne(x => x.SubMerchant)
                        .WithMany()
                        .HasForeignKey(x => x.SubMerchantId)
                        .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Merchant)
                        .WithMany()
                        .HasForeignKey(x => x.MerchantId)
                        .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Journal)
                        .WithMany(j => j.SettlementDetails)
                        .HasForeignKey(x => x.JournalId)
                        .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SettlementServiceDetail>(entity =>
            {
                entity.HasKey(x => x.SettlementServiceDetailId);

                entity.HasOne(x => x.Journal)
                        .WithMany(j => j.SettlementServiceDetails)
                        .HasForeignKey(x => x.JournalId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Merchant)
                        .WithMany()
                        .HasForeignKey(x => x.MerchantId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.SubMerchant)
                        .WithMany()
                        .HasForeignKey(x => x.SubMerchantId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Settlement)
                        .WithMany(x => x.SettlementServiceDetails)
                        .HasForeignKey(x => x.SettlementId);

                entity.HasOne(x => x.MerchantService)
                        .WithMany()
                        .HasForeignKey(x => x.MerchantServiceId);
            });


            modelBuilder.Entity<NotificationsContact>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.NotificationContacts)
                        .HasForeignKey(m => m.MerchantId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasDeletedIndex();
            });


            modelBuilder.Entity<AuditNotification>(entity =>
            {
                entity.HasKey(m => m.AuditNotificationId);
                entity.HasOne(m => m.NotificationsContact)
                        .WithMany(m => m.AuditNotifications)
                        .HasForeignKey(m => m.NotificationsContactId)
                        .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(m => new { m.NotificationsContactId, m.NotificationType }).IsUnique();
            });

            modelBuilder.Entity<PushNotificationsContact>(entity =>
            {
                entity.HasKey(m => m.NotificationsContactId);
                entity.HasOne(m => m.BaseNotificationsContact)
                        .WithOne(m => m.PushContact)
                        .HasForeignKey<PushNotificationsContact>(m => m.NotificationsContactId);

                entity.Property(m => m.Name).HasMaxLength(256).IsRequired();

                entity.HasIndex(m => m.PushDeviceId).IsUnique();
                entity.HasOne(m => m.PushDevice)
                        .WithMany(m => m.Contacts)
                        .HasForeignKey(m => m.PushDeviceId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<PushNotificationDevice>(entity =>
            {
                entity.HasKey(x => x.PushNotificationDeviceId);
                entity.HasOne(x => x.Person)
                                     .WithMany()
                                     .HasForeignKey(x => x.PersonId);
                entity.Property(x => x.DeviceToken)
                                     .HasMaxLength(256)
                                     .HasColumnType("char")
                                     .IsRequired();

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<PrinterNotificationsContact>(entity =>
            {
                entity.HasKey(m => m.NotificationsContactId);
                entity.HasOne(m => m.BaseNotificationsContact)
                        .WithOne(m => m.PrinterContact)
                        .HasForeignKey<PrinterNotificationsContact>(m => m.NotificationsContactId);

                entity.Property(m => m.Name).HasMaxLength(256).IsRequired();
                entity.Property(m => m.Address).HasMaxLength(256).IsRequired();

                entity.HasOne(m => m.PrinterWorker)
                        .WithMany(m => m.PrinterNotificationsContacts)
                        .HasForeignKey(m => m.PrinterWorkerId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Location)
                        .WithMany(m => m.PrinterNotificationsContacts)
                        .HasForeignKey(m => m.LocationId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<PrinterNotificationMessage>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Merchant)
                        .WithMany()
                        .HasForeignKey(m => m.MerchantId);

                entity.HasMany(m => m.PrinterMessageTargets)
                        .WithOne(m => m.PrinterNotificationMessage)
                        .HasForeignKey(m => m.PrinterNotificationMessageId);
            });

            modelBuilder.Entity<PrinterMessageTarget>(entity =>
            {
                entity.HasKey(m => new { m.PrinterNotificationsContactId, m.PrinterNotificationMessageId });

                entity.HasOne(m => m.PrinterNotificationMessage)
                        .WithMany(m => m.PrinterMessageTargets)
                        .HasForeignKey(m => m.PrinterNotificationMessageId);

                entity.HasOne(m => m.PrinterNotificationsContact)
                        .WithMany(m => m.PrinterMessageTargets)
                        .HasForeignKey(m => m.PrinterNotificationsContactId);
            });

            modelBuilder.Entity<EmployeeNotificationsContact>(entity =>
            {
                entity.HasKey(m => m.NotificationsContactId);
                entity.HasOne(m => m.BaseNotificationsContact)
                        .WithOne(m => m.EmployeeContact)
                        .HasForeignKey<EmployeeNotificationsContact>(m => m.NotificationsContactId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Employee)
                        .WithMany(m => m.EmployeeContacts)
                        .HasForeignKey(m => m.EmployeeId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<PosNotificationInboxMessage>(entity =>
            {
                entity.HasKey(m => new { m.PosNotificationMessageId, m.EmployeeId });

                entity.HasOne(m => m.PosNotificationMessage)
                        .WithMany(m => m.PosNotificationInboxes)
                        .HasForeignKey(m => m.PosNotificationMessageId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Employee)
                        .WithMany(m => m.PosNotificationInboxMessages)
                        .HasForeignKey(m => m.EmployeeId);
            });

            modelBuilder.Entity<PosNotificationMessage>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.PosNotificationMessages)
                        .HasForeignKey(m => m.MerchantId);

                entity.Property(d => d.JsonMessage).IsRequired().HasMaxLength(8000);
            });

            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasOne(i => i.Merchant)
                        .WithMany(i => i.MenuItems)
                        .HasForeignKey(i => i.MerchantId)
                        .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(m => m.AssociatedAvailabilityGroups)
                        .WithOne(m => m.MenuItem)
                        .HasForeignKey(m => m.MenuItemId);

                entity.HasOne(m => m.Location)
                        .WithMany(m => m.MenuItems)
                        .HasForeignKey(m => m.LocationId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasDeletedIndex();
            });


            modelBuilder.Entity<MenuItemTranslation>(entity =>
            {
                entity.HasKey(m => new { m.MenuItemId, m.Language });
                entity.HasOne(m => m.MenuItem)
                        .WithMany(m => m.MenuItemTranslations)
                        .HasForeignKey(m => m.MenuItemId)
                        .OnDelete(DeleteBehavior.Restrict);
                entity.Property(m => m.Name).IsRequired();

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<AvailabilityGroup>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasMany(m => m.WeeklyAvailabilities)
                        .WithOne(m => m.AvailabilityGroup)
                        .HasForeignKey(m => m.AvailabilityGroupId)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(m => m.AssociatedMenuItems)
                        .WithOne(m => m.AvailabilityGroup)
                        .HasForeignKey(m => m.AvailabilityGroupId)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(m => m.AssociatedChannelProfiles)
                        .WithOne(m => m.AvailabilityGroup)
                        .HasForeignKey(m => m.AvailabilityGroupId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AvailabilityMenuItemAssociation>(entity =>
            {
                entity.HasKey(m => new { m.AvailabilityGroupId, m.MenuItemId });

                entity.HasOne(m => m.AvailabilityGroup)
                        .WithMany(m => m.AssociatedMenuItems)
                        .HasForeignKey(m => m.AvailabilityGroupId);

                entity.HasOne(m => m.MenuItem)
                        .WithMany(m => m.AssociatedAvailabilityGroups)
                        .HasForeignKey(m => m.MenuItemId);
            });

            modelBuilder.Entity<AvailabilityProfileAssociation>(entity =>
            {
                entity.HasKey(m => new { m.AvailabilityGroupId, m.ChannelProfileId });

                entity.HasOne(m => m.AvailabilityGroup)
                        .WithMany(m => m.AssociatedChannelProfiles)
                        .HasForeignKey(m => m.AvailabilityGroupId);

                entity.HasOne(m => m.ChannelProfile)
                        .WithMany(m => m.AssociatedAvailabilityGroups)
                        .HasForeignKey(m => m.ChannelProfileId);
            });

            modelBuilder.Entity<WeeklyAvailability>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Ignore(m => m.StartAt);
                entity.Ignore(m => m.EndAt);

                entity.HasOne(m => m.AvailabilityGroup)
                        .WithMany(m => m.WeeklyAvailabilities)
                        .HasForeignKey(m => m.AvailabilityGroupId);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.Orders)
                        .HasForeignKey(m => m.MerchantId);
                entity.HasOne(x => x.Channel)
                        .WithMany(x => x.Orders)
                        .HasForeignKey(x => x.ChannelId);
                entity.HasOne(x => x.Session)
                                .WithMany(x => x.Orders)
                                .HasForeignKey(x => x.SessionId);

                entity.HasOne(x => x.Employee)
                        .WithMany(x => x.Orders)
                        .HasForeignKey(x => x.EmployeeId);
            });

            modelBuilder.Entity<OrderChangeLog>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Order)
                        .WithMany(m => m.OrderChangeLogs)
                        .HasForeignKey(m => m.OrderId);
                entity.Property(m => m.CreatedDate).IsRequired();
            });

            modelBuilder.Entity<OrderSequence>(entity =>
            {
                entity.HasKey(m => m.OrderId);
                entity.HasOne(m => m.Order)
                        .WithOne(m => m.OrderSequence)
                        .HasForeignKey<OrderSequence>(m => m.OrderId);
            });

            modelBuilder.Entity<OrderMenuItem>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.MenuItem)
                        .WithMany()
                        .HasForeignKey(m => m.MenuItemId);
                entity.HasOne(m => m.Order)
                        .WithMany(m => m.OrderMenuItems)
                        .HasForeignKey(m => m.OrderId);
                entity.HasOne(m => m.MenuItem)
                        .WithMany()
                        .HasForeignKey(m => m.MenuItemId);

                entity.HasOne(m => m.ParentOrderMenuItem)
                        .WithMany(m => m.Modifiers)
                        .HasForeignKey(m => m.ParentOrderMenuItemId);

                entity.Property(b => b.Quantity).HasPrecision(38, 18);
            });

            modelBuilder.Entity<PreparationGroup>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.PreparationGroups)
                        .HasForeignKey(m => m.MerchantId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Session)
                        .WithMany(m => m.PreparationGroups)
                        .HasForeignKey(m => m.SessionId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.Orders)
                        .WithMany(m => m.PreparationGroups);

                entity.HasOne(m => m.ParentPreparationGroup)
                        .WithMany(m => m.ChildrenPreparationGroups)
                        .HasForeignKey(m => m.ParentPreparationGroupId);
            });

            modelBuilder.Entity<PreparationGroupItem>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.PreparationGroup)
                        .WithMany(m => m.PreparationGroupItems)
                        .HasForeignKey(m => m.PreparationGroupId);

                entity.HasOne(m => m.MenuItem)
                        .WithMany(m => m.PreparationGroupItems)
                        .HasForeignKey(m => m.MenuItemId);

                entity.HasOne(m => m.ParentPreparationGroupItem)
                        .WithMany(m => m.Extras)
                        .HasForeignKey(m => m.ParentPreparationGroupItemId);

                entity.HasOne(m => m.Location)
                        .WithMany()
                        .HasForeignKey(m => m.LocationId);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(m => m.PosChargeId);
                entity.HasOne(m => m.PosCharge)
                        .WithOne(m => m.Review)
                        .HasForeignKey<Review>(m => m.PosChargeId);
            });

            modelBuilder.Entity<Channel>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.Channels)
                        .HasForeignKey(m => m.MerchantId)
                        .OnDelete(DeleteBehavior.NoAction);

                entity.Property(m => m.Identifier).HasMaxLength(50).IsRequired();
                entity.Property(m => m.IdentifierSortable).HasMaxLength(50).IsRequired();

                entity.HasIndex(c => new { c.ChannelProfileId, c.Identifier }).IsUnique();

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<PosIntegration>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Merchant)
                        .WithMany(m => m.PosIntegrations)
                        .HasForeignKey(m => m.MerchantId);

                entity.HasIndex(m => m.IntegrationType);
                entity.HasMany(m => m.ChannelProfiles)
                        .WithOne(m => m.PosIntegration);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Channel)
                        .WithMany(m => m.Sessions)
                        .HasForeignKey(m => m.ChannelId);
                entity.HasOne(m => m.Employee)
                        .WithMany()
                        .HasForeignKey(m => m.EmployeeId);
                entity.Property(m => m.PosIdentifier).HasMaxLength(64);
            });


            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Merchant)
                        .WithMany()
                        .HasForeignKey(m => m.MerchantId);

                entity.Ignore(m => m.LogoutInactivity);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<PosCharge>(entity =>
            {
                entity.HasKey(m => m.ChargeId);
                entity.HasOne(m => m.Charge)
                        .WithOne(m => m.PosCharge)
                        .HasForeignKey<PosCharge>(m => m.ChargeId);

                entity.HasOne(m => m.Merchant)
                        .WithMany()
                        .HasForeignKey(m => m.MerchantId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Channel)
                        .WithMany()
                        .HasForeignKey(m => m.ChannelId);

                entity.HasOne(m => m.Session)
                        .WithMany()
                        .HasForeignKey(m => m.SessionId);

                entity.HasOne(m => m.Location)
                        .WithMany()
                        .HasForeignKey(m => m.LocationId);

                entity.HasOne(m => m.RefundEmployee)
                        .WithMany()
                        .HasForeignKey(m => m.RefundEmployeeId);

                entity.HasOne(m => m.Employee)
                        .WithMany()
                        .HasForeignKey(m => m.EmployeeId);

                entity.HasMany(m => m.PosChargeSyncAttempts)
                        .WithOne(m => m.PosCharge)
                        .HasForeignKey(m => m.PosChargeId);

                entity.Property(x => x.RefundReason).HasMaxLength(256);
            });

            modelBuilder.Entity<PosChargeSyncAttempt>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.PosCharge)
                        .WithMany(m => m.PosChargeSyncAttempts)
                        .HasForeignKey(m => m.PosChargeId);
            });

            modelBuilder.Entity<PosChargeSelectedMenuItem>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.PosCharge)
                            .WithMany(m => m.PosChargeSelectedMenuItems)
                            .HasForeignKey(m => m.PosChargeId)
                            .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.OrderMenuItem)
                            .WithMany()
                            .HasForeignKey(m => m.OrderMenuItemId)
                            .OnDelete(DeleteBehavior.Restrict);

                entity.Property(b => b.Quantity).HasPrecision(38, 18);
            });

            modelBuilder.Entity<PosChargeInvoiceItem>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.ParentPosChargeInvoiceItem)
                        .WithMany(m => m.ChildrenPosChargeInvoiceItems)
                        .HasForeignKey(m => m.ParentPosChargeInvoiceItemId);

                entity.HasOne(m => m.PosCharge)
                        .WithMany(m => m.PosChargeInvoiceItems)
                        .HasForeignKey(m => m.PosChargeId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.OrderMenuItem)
                        .WithMany(m => m.PosChargeInvoiceItems)
                        .HasForeignKey(m => m.OrderMenuItemId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.Property(b => b.Quantity).HasPrecision(38, 18);
            });

            modelBuilder.Entity<ItemCategory>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.HasOne(x => x.Merchant)
                        .WithMany()
                        .HasForeignKey(x => x.MerchantId);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<ItemCategoryTranslation>(entity =>
            {
                entity.HasKey(m => new { m.ItemCategoryId, m.Language });
                entity.HasOne(m => m.ItemCategory)
                        .WithMany(m => m.ItemCategoryTranslations)
                        .HasForeignKey(m => m.ItemCategoryId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.Property(m => m.Name).IsRequired();

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<MenuItemCategoryAssociation>(entity =>
            {
                entity.HasKey(x => new { x.MenuItemId, x.ItemCategoryId });

                entity.HasOne(x => x.MenuItem)
                        .WithMany(x => x.MenuItemCategoryAssociations)
                        .HasForeignKey(x => x.MenuItemId);

                entity.HasOne(x => x.ItemCategory)
                        .WithMany(x => x.MenuItemCategoryAssociations)
                        .HasForeignKey(x => x.ItemCategoryId);
            });

            modelBuilder.Entity<ItemsModifierGroup>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(r => r.Merchant)
                        .WithMany()
                        .HasForeignKey(r => r.MerchantId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.Property(m => m.Name).IsRequired().HasMaxLength(512);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<ItemsModifierGroupsAssociation>(entity =>
            {
                entity.HasKey(m => new { m.MenuItemId, m.MenuItemModifierGroupId });

                entity.HasOne(m => m.MenuItem)
                        .WithMany(m => m.MenuItemModifierGroups)
                        .HasForeignKey(m => m.MenuItemId);

                entity.HasOne(m => m.MenuItemModifierGroup)
                        .WithMany(m => m.ItemsModifierGroupsAssociation)
                        .HasForeignKey(m => m.MenuItemModifierGroupId);
            });

            modelBuilder.Entity<ItemsModifierGroupTranslation>(entity =>
            {
                entity.HasKey(x => new { x.MenuItemModifierGroupId, x.Language });
                entity.HasOne(m => m.MenuItemModifierGroup)
                        .WithMany(m => m.ItemsModifierGroupTranslations)
                        .HasForeignKey(m => m.MenuItemModifierGroupId)
                        .OnDelete(DeleteBehavior.Restrict);
                entity.Property(m => m.Name).IsRequired();

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<MenuItemModifier>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.MenuItem)
                        .WithMany(m => m.MenuItemModifiers)
                        .HasForeignKey(m => m.MenuItemId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<SpatialChannel>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Channel)
                        .WithMany(m => m.SpatialChannels)
                        .HasForeignKey(m => m.ChannelId)
                        .OnDelete(DeleteBehavior.Restrict);

                entity.HasDeletedIndex();
            });

            modelBuilder.Entity<Posting>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Person)
                        .WithMany(m => m.Postings)
                        .HasForeignKey(m => m.PersonId);

                entity.HasOne(m => m.Journal)
                        .WithMany(m => m.Postings)
                        .HasForeignKey(m => m.JournalId);

                entity.Property(m => m.AssetType).IsRequired();
            });

            modelBuilder.Entity<AcquirerCharge>(entity =>
            {
                entity.HasKey(m => m.ChargeId);
                entity.HasOne(m => m.Charge)
                        .WithOne(m => m.AcquirerCharge)
                        .HasForeignKey<AcquirerCharge>(m => m.ChargeId);
            });
        }

        private void BuildFunctions(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction(() => QuiviDbFunctions.ToTimeZone(string.Empty, (string?)null))
                .HasName("fn_ToTimeZone")
                .HasSchema("dbo");

            modelBuilder.HasDbFunction(() => QuiviDbFunctions.ToTimeZone(DateTime.MinValue, (string?)null))
                        .HasName("fn_ToTimeZone")
                        .HasSchema("dbo");

            modelBuilder.HasDbFunction(() => QuiviDbFunctions.ToWeeklyAvailabilityInSeconds(default))
                .HasName("fn_ToWeeklyAvailabilityInSeconds")
                .HasSchema("dbo");
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Settlement> Settlements { get; set; }
        public DbSet<SettlementDetail> SettlementDetails { get; set; }
        public DbSet<SettlementServiceDetail> SettlementServiceDetails { get; set; }
        public DbSet<PushNotificationDevice> ApnsConnection { get; set; }
        public DbSet<Journal> Journals { get; set; }
        public DbSet<JournalChange> JournalChanges { get; set; }
        public DbSet<Posting> Postings { get; set; }
        public DbSet<Merchant> Merchants { get; set; }
        public DbSet<MerchantFee> MerchantFees { get; set; }
        public DbSet<ApiClient> ApiClients { get; set; }
        public DbSet<ApiClientRequest> ApiClientRequests { get; set; }
        public DbSet<AcquirerCharge> AcquirerCharges { get; set; }
        public DbSet<Charge> Charges { get; set; }
        public DbSet<MerchantInvoiceDocument> MerchantInvoiceDocuments { get; set; }
        public DbSet<DepositCapture> DepositCaptures { get; set; }
        public DbSet<DepositCaptureJournal> DepositCaptureJournals { get; set; }
        public DbSet<DepositJournal> DepositJournals { get; set; }
        public DbSet<DepositRefundJournal> DepositRefundJournals { get; set; }
        public DbSet<JournalDetails> JournalDetails { get; set; }
        public DbSet<MerchantService> MerchantServices { get; set; }
        public DbSet<PosCharge> PosCharges { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<CustomChargeMethod> CustomChargeMethods { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderChangeLog> OrderChangeLogs { get; set; }
        public DbSet<OrderMenuItem> OrderMenuItems { get; set; }
        public DbSet<PreparationGroup> PreparationGroups { get; set; }
        public DbSet<PreparationGroupItem> PreparationGroupItems { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<PosIntegration> PosIntegrations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<DepositSurcharge> DepositSurcharges { get; set; }
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<ItemsModifierGroup> ItemsModifierGroups { get; set; }
        public DbSet<AvailabilityGroup> Availabilities { get; set; }
        public DbSet<PosNotificationInboxMessage> PosNotificationInboxMessages { get; set; }
        public DbSet<PosNotificationMessage> PosNotificationMessages { get; set; }
        public DbSet<NotificationsContact> NotificationsContacts { get; set; }
    }
}