using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quivi.Domain.Repositories.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Charges",
                columns: table => new
                {
                    ChargeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ChargePartner = table.Column<int>(type: "int", nullable: true),
                    ChargeMethod = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChainedChargeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charges", x => x.ChargeId);
                    table.ForeignKey(
                        name: "FK_Charges_Charges_ChainedChargeId",
                        column: x => x.ChainedChargeId,
                        principalTable: "Charges",
                        principalColumn: "ChargeId");
                });

            migrationBuilder.CreateTable(
                name: "Journals",
                columns: table => new
                {
                    JournalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChargeMethod = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JournalLinkId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journals", x => x.JournalId);
                    table.ForeignKey(
                        name: "FK_Journals_Journals_JournalLinkId",
                        column: x => x.JournalLinkId,
                        principalTable: "Journals",
                        principalColumn: "JournalId");
                });

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    SettlementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.SettlementId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardCharges",
                columns: table => new
                {
                    ChargeId = table.Column<int>(type: "int", nullable: false),
                    AuthorizationToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FormContext = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastStatusCheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardCharges", x => x.ChargeId);
                    table.ForeignKey(
                        name: "FK_CardCharges_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MbWayCharges",
                columns: table => new
                {
                    ChargeId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastStatusCheckDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MbWayCharges", x => x.ChargeId);
                    table.ForeignKey(
                        name: "FK_MbWayCharges_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerminalCharge",
                columns: table => new
                {
                    ChargeId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastStatusCheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalCharge", x => x.ChargeId);
                    table.ForeignKey(
                        name: "FK_TerminalCharge_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketMobileCharges",
                columns: table => new
                {
                    ChargeId = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMobileCharges", x => x.ChargeId);
                    table.ForeignKey(
                        name: "FK_TicketMobileCharges_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalChanges",
                columns: table => new
                {
                    JournalHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: false),
                    JournalLinkId = table.Column<int>(type: "int", nullable: true),
                    JournalId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalChanges", x => x.JournalHistoryId);
                    table.ForeignKey(
                        name: "FK_JournalChanges_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalChanges_Journals_JournalId1",
                        column: x => x.JournalId1,
                        principalTable: "Journals",
                        principalColumn: "JournalId");
                    table.ForeignKey(
                        name: "FK_JournalChanges_Journals_JournalLinkId",
                        column: x => x.JournalLinkId,
                        principalTable: "Journals",
                        principalColumn: "JournalId");
                });

            migrationBuilder.CreateTable(
                name: "JournalDetails",
                columns: table => new
                {
                    JournalId = table.Column<int>(type: "int", nullable: false),
                    IncludedTip = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalDetails", x => x.JournalId);
                    table.ForeignKey(
                        name: "FK_JournalDetails_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiClientRequests",
                columns: table => new
                {
                    ApiClientRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RedeemCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    DeviceReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    SubMerchantId = table.Column<int>(type: "int", nullable: false),
                    ApiClientId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiClientRequests", x => x.ApiClientRequestId);
                });

            migrationBuilder.CreateTable(
                name: "ApiClients",
                columns: table => new
                {
                    ApiClientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    ClientType = table.Column<int>(type: "int", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    PersonId = table.Column<int>(type: "int", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiClients", x => x.ApiClientId);
                    table.ForeignKey(
                        name: "FK_ApiClients_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApnsConnection",
                columns: table => new
                {
                    PushNotificationDeviceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceToken = table.Column<string>(type: "char(256)", maxLength: 256, nullable: false),
                    SessionGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceType = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PersonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApnsConnection", x => x.PushNotificationDeviceId);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserMerchant",
                columns: table => new
                {
                    ApplicationUserId = table.Column<int>(type: "int", nullable: false),
                    MerchantsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserMerchant", x => new { x.ApplicationUserId, x.MerchantsId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserMerchant_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditNotification",
                columns: table => new
                {
                    AuditNotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationType = table.Column<long>(type: "bigint", nullable: false),
                    LastSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotificationsContactId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditNotification", x => x.AuditNotificationId);
                });

            migrationBuilder.CreateTable(
                name: "ChannelProfile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrePaidOrderingMinimumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SendToPreparationTimerSeconds = table.Column<int>(type: "int", nullable: true),
                    Features = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    PosIntegrationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderConfigurableField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsAutoFill = table.Column<bool>(type: "bit", nullable: false),
                    AssignedOn = table.Column<int>(type: "int", nullable: false),
                    PrintedOn = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChannelProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderConfigurableField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderConfigurableField_ChannelProfile_ChannelProfileId",
                        column: x => x.ChannelProfileId,
                        principalTable: "ChannelProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderConfigurableFieldTranslation",
                columns: table => new
                {
                    Language = table.Column<int>(type: "int", nullable: false),
                    OrderConfigurableFieldId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderConfigurableFieldTranslation", x => new { x.OrderConfigurableFieldId, x.Language });
                    table.ForeignKey(
                        name: "FK_OrderConfigurableFieldTranslation_OrderConfigurableField_OrderConfigurableFieldId",
                        column: x => x.OrderConfigurableFieldId,
                        principalTable: "OrderConfigurableField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoSIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Identifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdentifierSortable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChannelProfileId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Channels_ChannelProfile_ChannelProfileId",
                        column: x => x.ChannelProfileId,
                        principalTable: "ChannelProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpatialChannel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RelativePositionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RelativePositionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Shape = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChannelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpatialChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpatialChannel_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomChargeMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomChargeMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MerchantCustomCharge",
                columns: table => new
                {
                    ChargeId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomChargeMethodId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantCustomCharge", x => x.ChargeId);
                    table.ForeignKey(
                        name: "FK_MerchantCustomCharge_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MerchantCustomCharge_CustomChargeMethods_CustomChargeMethodId",
                        column: x => x.CustomChargeMethodId,
                        principalTable: "CustomChargeMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deposit",
                columns: table => new
                {
                    ChargeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsumerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposit", x => x.ChargeId);
                    table.ForeignKey(
                        name: "FK_Deposit_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositCaptureJournals",
                columns: table => new
                {
                    DepositId = table.Column<int>(type: "int", nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositCaptureJournals", x => x.DepositId);
                    table.ForeignKey(
                        name: "FK_DepositCaptureJournals_Deposit_DepositId",
                        column: x => x.DepositId,
                        principalTable: "Deposit",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepositCaptureJournals_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositJournals",
                columns: table => new
                {
                    DepositId = table.Column<int>(type: "int", nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositJournals", x => x.DepositId);
                    table.ForeignKey(
                        name: "FK_DepositJournals_Deposit_DepositId",
                        column: x => x.DepositId,
                        principalTable: "Deposit",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepositJournals_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositRefundJournals",
                columns: table => new
                {
                    DepositId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositRefundJournals", x => x.DepositId);
                    table.ForeignKey(
                        name: "FK_DepositRefundJournals_Deposit_DepositId",
                        column: x => x.DepositId,
                        principalTable: "Deposit",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepositRefundJournals_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositSurchargeJournal",
                columns: table => new
                {
                    DepositId = table.Column<int>(type: "int", nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositSurchargeJournal", x => x.DepositId);
                    table.ForeignKey(
                        name: "FK_DepositSurchargeJournal_Deposit_DepositId",
                        column: x => x.DepositId,
                        principalTable: "Deposit",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepositSurchargeJournal_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositSurcharges",
                columns: table => new
                {
                    DepositId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppliedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppliedUnit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositSurcharges", x => x.DepositId);
                    table.ForeignKey(
                        name: "FK_DepositSurcharges_Deposit_DepositId",
                        column: x => x.DepositId,
                        principalTable: "Deposit",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositCaptures",
                columns: table => new
                {
                    DepositId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositCaptures", x => x.DepositId);
                    table.ForeignKey(
                        name: "FK_DepositCaptures_Deposit_DepositId",
                        column: x => x.DepositId,
                        principalTable: "Deposit",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeNotificationsContact",
                columns: table => new
                {
                    NotificationsContactId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeNotificationsContact", x => x.NotificationsContactId);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PosIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PinCodeHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Restrictions = table.Column<int>(type: "int", nullable: false),
                    LogoutInactivityInSeconds = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PosIdentifier = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItemCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemCategoryTranslation",
                columns: table => new
                {
                    Language = table.Column<int>(type: "int", nullable: false),
                    ItemCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategoryTranslation", x => new { x.ItemCategoryId, x.Language });
                    table.ForeignKey(
                        name: "FK_ItemCategoryTranslation_ItemCategories_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalTable: "ItemCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemsModifierGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    MinSelection = table.Column<int>(type: "int", nullable: false),
                    MaxSelection = table.Column<int>(type: "int", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsModifierGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsModifierGroupTranslation",
                columns: table => new
                {
                    Language = table.Column<int>(type: "int", nullable: false),
                    MenuItemModifierGroupId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsModifierGroupTranslation", x => new { x.MenuItemModifierGroupId, x.Language });
                    table.ForeignKey(
                        name: "FK_ItemsModifierGroupTranslation_ItemsModifierGroups_MenuItemModifierGroupId",
                        column: x => x.MenuItemModifierGroupId,
                        principalTable: "ItemsModifierGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemsModifierGroupsAssociation",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    MenuItemModifierGroupId = table.Column<int>(type: "int", nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsModifierGroupsAssociation", x => new { x.MenuItemId, x.MenuItemModifierGroupId });
                    table.ForeignKey(
                        name: "FK_ItemsModifierGroupsAssociation_ItemsModifierGroups_MenuItemModifierGroupId",
                        column: x => x.MenuItemModifierGroupId,
                        principalTable: "ItemsModifierGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemCategoryAssociation",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCategoryId = table.Column<int>(type: "int", nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemCategoryAssociation", x => new { x.MenuItemId, x.ItemCategoryId });
                    table.ForeignKey(
                        name: "FK_MenuItemCategoryAssociation_ItemCategories_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalTable: "ItemCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemModifier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MenuItemModifierGroupId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemModifier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemModifier_ItemsModifierGroups_MenuItemModifierGroupId",
                        column: x => x.MenuItemModifierGroupId,
                        principalTable: "ItemsModifierGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stock = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceType = table.Column<int>(type: "int", nullable: false),
                    ShowWhenNotAvailable = table.Column<bool>(type: "bit", nullable: false),
                    VatRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HiddenFromGuestApp = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemTranslation",
                columns: table => new
                {
                    Language = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemTranslation", x => new { x.MenuItemId, x.Language });
                    table.ForeignKey(
                        name: "FK_MenuItemTranslation_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemWeeklyAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartAtSeconds = table.Column<int>(type: "int", nullable: false),
                    EndAtSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MenuItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemWeeklyAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemWeeklyAvailabilities_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchantFees",
                columns: table => new
                {
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    ChargeMethod = table.Column<int>(type: "int", nullable: false),
                    FeeType = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeUnit = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantFees", x => new { x.MerchantId, x.ChargeMethod, x.FeeType });
                });

            migrationBuilder.CreateTable(
                name: "MerchantFile",
                columns: table => new
                {
                    MerchantFileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileMetadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantFile", x => x.MerchantFileId);
                });

            migrationBuilder.CreateTable(
                name: "MerchantInvoiceDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    DocumentReference = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    ChargeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantInvoiceDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantInvoiceDocuments_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "ChargeId");
                });

            migrationBuilder.CreateTable(
                name: "Merchants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Iban = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VatNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VatRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IbanProofUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDemo = table.Column<bool>(type: "bit", nullable: false),
                    TermsAndConditionsAcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisabledFeatures = table.Column<int>(type: "int", nullable: false),
                    TransactionFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionFeeUnit = table.Column<int>(type: "int", nullable: false),
                    SurchargeFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SurchargeFeeUnit = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentMerchantId = table.Column<int>(type: "int", nullable: true),
                    SetUpFeeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Merchants_Merchants_ParentMerchantId",
                        column: x => x.ParentMerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotificationsContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscribedNotifications = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationsContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationsContacts_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    PayLater = table.Column<bool>(type: "bit", nullable: false),
                    Origin = table.Column<int>(type: "int", nullable: false),
                    ScheduledTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SessionGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PersonType = table.Column<int>(type: "int", nullable: false),
                    Vat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: true),
                    SubMerchantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_People_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_People_Merchants_SubMerchantId",
                        column: x => x.SubMerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PosCharges",
                columns: table => new
                {
                    ChargeId = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SurchargeFeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Payment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tip = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VatNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalRefund = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentRefund = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TipRefund = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaptureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceRefundType = table.Column<int>(type: "int", nullable: true),
                    RefundReason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    RefundEmployeeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosCharges", x => x.ChargeId);
                    table.ForeignKey(
                        name: "FK_PosCharges_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosCharges_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosCharges_Employees_RefundEmployeeId",
                        column: x => x.RefundEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PosCharges_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PosCharges_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosCharges_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PosIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegrationType = table.Column<int>(type: "int", nullable: false),
                    ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SyncState = table.Column<int>(type: "int", nullable: false),
                    LastSyncingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SyncStateModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiagnosticErrorsMuted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosIntegrations_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosNotificationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<long>(type: "bigint", nullable: false),
                    JsonMessage = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosNotificationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosNotificationMessages_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreparationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdditionalNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    ParentPreparationGroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationGroups_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreparationGroups_PreparationGroups_ParentPreparationGroupId",
                        column: x => x.ParentPreparationGroupId,
                        principalTable: "PreparationGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PreparationGroups_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrinterWorker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identifier = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterWorker", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrinterWorker_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettlementDetails",
                columns: table => new
                {
                    SettlementDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubMerchantIban = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubMerchantVatRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SettlementDays = table.Column<int>(type: "int", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncludedTip = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncludedNetTip = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DatetimeUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChargeMethod = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    SubMerchantId = table.Column<int>(type: "int", nullable: false),
                    SettlementId = table.Column<int>(type: "int", nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementDetails", x => x.SettlementDetailId);
                    table.ForeignKey(
                        name: "FK_SettlementDetails_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementDetails_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementDetails_Merchants_SubMerchantId",
                        column: x => x.SubMerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementDetails_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "SettlementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PushNotificationsContact",
                columns: table => new
                {
                    NotificationsContactId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PushDeviceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushNotificationsContact", x => x.NotificationsContactId);
                    table.ForeignKey(
                        name: "FK_PushNotificationsContact_ApnsConnection_PushDeviceId",
                        column: x => x.PushDeviceId,
                        principalTable: "ApnsConnection",
                        principalColumn: "PushNotificationDeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PushNotificationsContact_NotificationsContacts_NotificationsContactId",
                        column: x => x.NotificationsContactId,
                        principalTable: "NotificationsContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderAdditionalInfo",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderConfigurableFieldId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAdditionalInfo", x => new { x.OrderId, x.OrderConfigurableFieldId });
                    table.ForeignKey(
                        name: "FK_OrderAdditionalInfo_OrderConfigurableField_OrderConfigurableFieldId",
                        column: x => x.OrderConfigurableFieldId,
                        principalTable: "OrderConfigurableField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderAdditionalInfo_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EatOrderState = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderChangeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderChangeLogs_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderMenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(38,18)", precision: 38, scale: 18, nullable: false),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceType = table.Column<int>(type: "int", nullable: false),
                    VatRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    ParentOrderMenuItemId = table.Column<int>(type: "int", nullable: true),
                    MenuItemModifierGroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderMenuItems_ItemsModifierGroups_MenuItemModifierGroupId",
                        column: x => x.MenuItemModifierGroupId,
                        principalTable: "ItemsModifierGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderMenuItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderMenuItems_OrderMenuItems_ParentOrderMenuItemId",
                        column: x => x.ParentOrderMenuItemId,
                        principalTable: "OrderMenuItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderMenuItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderSequence",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSequence", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_OrderSequence_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchantServices",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantServices", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_MerchantServices_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MerchantServices_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Postings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Postings_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Postings_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosChargeSyncAttempt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    State = table.Column<int>(type: "int", nullable: false),
                    SyncedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PosChargeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosChargeSyncAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosChargeSyncAttempt_PosCharges_PosChargeId",
                        column: x => x.PosChargeId,
                        principalTable: "PosCharges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    PosChargeId = table.Column<int>(type: "int", nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.PosChargeId);
                    table.ForeignKey(
                        name: "FK_Reviews_PosCharges_PosChargeId",
                        column: x => x.PosChargeId,
                        principalTable: "PosCharges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosNotificationInboxMessages",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PosNotificationMessageId = table.Column<int>(type: "int", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosNotificationInboxMessages", x => new { x.PosNotificationMessageId, x.EmployeeId });
                    table.ForeignKey(
                        name: "FK_PosNotificationInboxMessages_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosNotificationInboxMessages_PosNotificationMessages_PosNotificationMessageId",
                        column: x => x.PosNotificationMessageId,
                        principalTable: "PosNotificationMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderPreparationGroup",
                columns: table => new
                {
                    OrdersId = table.Column<int>(type: "int", nullable: false),
                    PreparationGroupsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPreparationGroup", x => new { x.OrdersId, x.PreparationGroupsId });
                    table.ForeignKey(
                        name: "FK_OrderPreparationGroup_Orders_OrdersId",
                        column: x => x.OrdersId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderPreparationGroup_PreparationGroups_PreparationGroupsId",
                        column: x => x.PreparationGroupsId,
                        principalTable: "PreparationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreparationGroupItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalQuantity = table.Column<int>(type: "int", nullable: false),
                    RemainingQuantity = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentPreparationGroupItemId = table.Column<int>(type: "int", nullable: true),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    PreparationGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationGroupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationGroupItems_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PreparationGroupItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreparationGroupItems_PreparationGroupItems_ParentPreparationGroupItemId",
                        column: x => x.ParentPreparationGroupItemId,
                        principalTable: "PreparationGroupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PreparationGroupItems_PreparationGroups_PreparationGroupId",
                        column: x => x.PreparationGroupId,
                        principalTable: "PreparationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrinterNotificationsContact",
                columns: table => new
                {
                    NotificationsContactId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrinterWorkerId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterNotificationsContact", x => x.NotificationsContactId);
                    table.ForeignKey(
                        name: "FK_PrinterNotificationsContact_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrinterNotificationsContact_NotificationsContacts_NotificationsContactId",
                        column: x => x.NotificationsContactId,
                        principalTable: "NotificationsContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrinterNotificationsContact_PrinterWorker_PrinterWorkerId",
                        column: x => x.PrinterWorkerId,
                        principalTable: "PrinterWorker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PosChargeInvoiceItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(38,18)", precision: 38, scale: 18, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentPosChargeInvoiceItemId = table.Column<int>(type: "int", nullable: true),
                    PosChargeId = table.Column<int>(type: "int", nullable: false),
                    OrderMenuItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosChargeInvoiceItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosChargeInvoiceItem_OrderMenuItems_OrderMenuItemId",
                        column: x => x.OrderMenuItemId,
                        principalTable: "OrderMenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosChargeInvoiceItem_PosChargeInvoiceItem_ParentPosChargeInvoiceItemId",
                        column: x => x.ParentPosChargeInvoiceItemId,
                        principalTable: "PosChargeInvoiceItem",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PosChargeInvoiceItem_PosCharges_PosChargeId",
                        column: x => x.PosChargeId,
                        principalTable: "PosCharges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PosChargeSelectedMenuItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(38,18)", precision: 38, scale: 18, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PosChargeId = table.Column<int>(type: "int", nullable: false),
                    OrderMenuItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosChargeSelectedMenuItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosChargeSelectedMenuItem_OrderMenuItems_OrderMenuItemId",
                        column: x => x.OrderMenuItemId,
                        principalTable: "OrderMenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosChargeSelectedMenuItem_PosCharges_PosChargeId",
                        column: x => x.PosChargeId,
                        principalTable: "PosCharges",
                        principalColumn: "ChargeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettlementServiceDetails",
                columns: table => new
                {
                    SettlementServiceDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubMerchantIban = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubMerchantVatRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    SubMerchantId = table.Column<int>(type: "int", nullable: false),
                    SettlementId = table.Column<int>(type: "int", nullable: false),
                    MerchantServiceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementServiceDetails", x => x.SettlementServiceDetailId);
                    table.ForeignKey(
                        name: "FK_SettlementServiceDetails_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementServiceDetails_MerchantServices_MerchantServiceId",
                        column: x => x.MerchantServiceId,
                        principalTable: "MerchantServices",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SettlementServiceDetails_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementServiceDetails_Merchants_SubMerchantId",
                        column: x => x.SubMerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementServiceDetails_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "SettlementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiClientRequests_ApiClientId",
                table: "ApiClientRequests",
                column: "ApiClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiClientRequests_SubMerchantId",
                table: "ApiClientRequests",
                column: "SubMerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiClients_MerchantId",
                table: "ApiClients",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiClients_PersonId",
                table: "ApiClients",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiClients_UserId",
                table: "ApiClients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiClients_UserName",
                table: "ApiClients",
                column: "UserName",
                unique: true,
                filter: "[UserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "ApiClients",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApnsConnection_PersonId",
                table: "ApnsConnection",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "ApnsConnection",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserMerchant_MerchantsId",
                table: "ApplicationUserMerchant",
                column: "MerchantsId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId_ClaimType",
                table: "AspNetUserClaims",
                columns: new[] { "UserId", "ClaimType" },
                unique: true,
                filter: "[ClaimType] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditNotification_NotificationsContactId_NotificationType",
                table: "AuditNotification",
                columns: new[] { "NotificationsContactId", "NotificationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelProfile_MerchantId",
                table: "ChannelProfile",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelProfile_PosIntegrationId",
                table: "ChannelProfile",
                column: "PosIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "ChannelProfile",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_ChannelProfileId_Identifier",
                table: "Channels",
                columns: new[] { "ChannelProfileId", "Identifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Channels_MerchantId",
                table: "Channels",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "Channels",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_ChainedChargeId",
                table: "Charges",
                column: "ChainedChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomChargeMethods_MerchantId",
                table: "CustomChargeMethods",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_ConsumerId",
                table: "Deposit",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositCaptureJournals_JournalId",
                table: "DepositCaptureJournals",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositCaptures_PersonId",
                table: "DepositCaptures",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositJournals_JournalId",
                table: "DepositJournals",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositRefundJournals_JournalId",
                table: "DepositRefundJournals",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositSurchargeJournal_JournalId",
                table: "DepositSurchargeJournal",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeNotificationsContact_EmployeeId",
                table: "EmployeeNotificationsContact",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "EmployeeNotificationsContact",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_MerchantId",
                table: "Employees",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "Employees",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_MerchantId",
                table: "ItemCategories",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "ItemCategories",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "ItemCategoryTranslation",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsModifierGroups_MerchantId",
                table: "ItemsModifierGroups",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "ItemsModifierGroups",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsModifierGroupsAssociation_MenuItemModifierGroupId",
                table: "ItemsModifierGroupsAssociation",
                column: "MenuItemModifierGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "ItemsModifierGroupTranslation",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JournalChanges_JournalId",
                table: "JournalChanges",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalChanges_JournalId1",
                table: "JournalChanges",
                column: "JournalId1");

            migrationBuilder.CreateIndex(
                name: "IX_JournalChanges_JournalLinkId",
                table: "JournalChanges",
                column: "JournalLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_Journals_JournalLinkId",
                table: "Journals",
                column: "JournalLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_MerchantId",
                table: "Location",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "Location",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemCategoryAssociation_ItemCategoryId",
                table: "MenuItemCategoryAssociation",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemModifier_MenuItemId",
                table: "MenuItemModifier",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemModifier_MenuItemModifierGroupId",
                table: "MenuItemModifier",
                column: "MenuItemModifierGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "MenuItemModifier",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_LocationId",
                table: "MenuItems",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MerchantId",
                table: "MenuItems",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "MenuItems",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "MenuItemTranslation",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemWeeklyAvailabilities_MenuItemId",
                table: "MenuItemWeeklyAvailabilities",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "MenuItemWeeklyAvailabilities",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantCustomCharge_CustomChargeMethodId",
                table: "MerchantCustomCharge",
                column: "CustomChargeMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "MerchantFees",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantFile_MerchantId",
                table: "MerchantFile",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "MerchantFile",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantInvoiceDocuments_ChargeId",
                table: "MerchantInvoiceDocuments",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantInvoiceDocuments_MerchantId",
                table: "MerchantInvoiceDocuments",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_ParentMerchantId",
                table: "Merchants",
                column: "ParentMerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_SetUpFeeId",
                table: "Merchants",
                column: "SetUpFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "Merchants",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantServices_MerchantId",
                table: "MerchantServices",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationsContacts_MerchantId",
                table: "NotificationsContacts",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "NotificationsContacts",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAdditionalInfo_OrderConfigurableFieldId",
                table: "OrderAdditionalInfo",
                column: "OrderConfigurableFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderChangeLogs_OrderId",
                table: "OrderChangeLogs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderConfigurableField_ChannelProfileId",
                table: "OrderConfigurableField",
                column: "ChannelProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "OrderConfigurableField",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "OrderConfigurableFieldTranslation",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMenuItems_MenuItemId",
                table: "OrderMenuItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMenuItems_MenuItemModifierGroupId",
                table: "OrderMenuItems",
                column: "MenuItemModifierGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMenuItems_OrderId",
                table: "OrderMenuItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMenuItems_ParentOrderMenuItemId",
                table: "OrderMenuItems",
                column: "ParentOrderMenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPreparationGroup_PreparationGroupsId",
                table: "OrderPreparationGroup",
                column: "PreparationGroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ChannelId",
                table: "Orders",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EmployeeId",
                table: "Orders",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_MerchantId",
                table: "Orders",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SessionId",
                table: "Orders",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_People_MerchantId",
                table: "People",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_People_SessionGuid",
                table: "People",
                column: "SessionGuid");

            migrationBuilder.CreateIndex(
                name: "IX_People_SubMerchantId",
                table: "People",
                column: "SubMerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "People",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PosChargeInvoiceItem_OrderMenuItemId",
                table: "PosChargeInvoiceItem",
                column: "OrderMenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PosChargeInvoiceItem_ParentPosChargeInvoiceItemId",
                table: "PosChargeInvoiceItem",
                column: "ParentPosChargeInvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PosChargeInvoiceItem_PosChargeId",
                table: "PosChargeInvoiceItem",
                column: "PosChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCharges_ChannelId",
                table: "PosCharges",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCharges_LocationId",
                table: "PosCharges",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCharges_MerchantId",
                table: "PosCharges",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCharges_RefundEmployeeId",
                table: "PosCharges",
                column: "RefundEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCharges_SessionId",
                table: "PosCharges",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PosChargeSelectedMenuItem_OrderMenuItemId",
                table: "PosChargeSelectedMenuItem",
                column: "OrderMenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PosChargeSelectedMenuItem_PosChargeId",
                table: "PosChargeSelectedMenuItem",
                column: "PosChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_PosChargeSyncAttempt_PosChargeId",
                table: "PosChargeSyncAttempt",
                column: "PosChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_PosIntegrations_IntegrationType",
                table: "PosIntegrations",
                column: "IntegrationType");

            migrationBuilder.CreateIndex(
                name: "IX_PosIntegrations_MerchantId",
                table: "PosIntegrations",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "PosIntegrations",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PosNotificationInboxMessages_EmployeeId",
                table: "PosNotificationInboxMessages",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PosNotificationMessages_MerchantId",
                table: "PosNotificationMessages",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Postings_JournalId",
                table: "Postings",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_Postings_PersonId",
                table: "Postings",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationGroupItems_LocationId",
                table: "PreparationGroupItems",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationGroupItems_MenuItemId",
                table: "PreparationGroupItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationGroupItems_ParentPreparationGroupItemId",
                table: "PreparationGroupItems",
                column: "ParentPreparationGroupItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationGroupItems_PreparationGroupId",
                table: "PreparationGroupItems",
                column: "PreparationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationGroups_MerchantId",
                table: "PreparationGroups",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationGroups_ParentPreparationGroupId",
                table: "PreparationGroups",
                column: "ParentPreparationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationGroups_SessionId",
                table: "PreparationGroups",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrinterNotificationsContact_LocationId",
                table: "PrinterNotificationsContact",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PrinterNotificationsContact_PrinterWorkerId",
                table: "PrinterNotificationsContact",
                column: "PrinterWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "PrinterNotificationsContact",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PrinterWorker_Identifier",
                table: "PrinterWorker",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrinterWorker_MerchantId",
                table: "PrinterWorker",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationsContact_PushDeviceId",
                table: "PushNotificationsContact",
                column: "PushDeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "PushNotificationsContact",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ChannelId",
                table: "Sessions",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_EmployeeId",
                table: "Sessions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementDetails_JournalId",
                table: "SettlementDetails",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementDetails_MerchantId",
                table: "SettlementDetails",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementDetails_SettlementId",
                table: "SettlementDetails",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementDetails_SubMerchantId",
                table: "SettlementDetails",
                column: "SubMerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementServiceDetails_JournalId",
                table: "SettlementServiceDetails",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementServiceDetails_MerchantId",
                table: "SettlementServiceDetails",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementServiceDetails_MerchantServiceId",
                table: "SettlementServiceDetails",
                column: "MerchantServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementServiceDetails_SettlementId",
                table: "SettlementServiceDetails",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementServiceDetails_SubMerchantId",
                table: "SettlementServiceDetails",
                column: "SubMerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_SpatialChannel_ChannelId",
                table: "SpatialChannel",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_T_DeletedDate_NotDeleted",
                table: "SpatialChannel",
                column: "DeletedDate",
                filter: "[DeletedDate] IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiClientRequests_ApiClients_ApiClientId",
                table: "ApiClientRequests",
                column: "ApiClientId",
                principalTable: "ApiClients",
                principalColumn: "ApiClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiClientRequests_Merchants_SubMerchantId",
                table: "ApiClientRequests",
                column: "SubMerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApiClients_Merchants_MerchantId",
                table: "ApiClients",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiClients_People_PersonId",
                table: "ApiClients",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApnsConnection_People_PersonId",
                table: "ApnsConnection",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserMerchant_Merchants_MerchantsId",
                table: "ApplicationUserMerchant",
                column: "MerchantsId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditNotification_NotificationsContacts_NotificationsContactId",
                table: "AuditNotification",
                column: "NotificationsContactId",
                principalTable: "NotificationsContacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelProfile_Merchants_MerchantId",
                table: "ChannelProfile",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelProfile_PosIntegrations_PosIntegrationId",
                table: "ChannelProfile",
                column: "PosIntegrationId",
                principalTable: "PosIntegrations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Merchants_MerchantId",
                table: "Channels",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomChargeMethods_Merchants_MerchantId",
                table: "CustomChargeMethods",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deposit_People_ConsumerId",
                table: "Deposit",
                column: "ConsumerId",
                principalTable: "People",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepositCaptures_People_PersonId",
                table: "DepositCaptures",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeNotificationsContact_Employees_EmployeeId",
                table: "EmployeeNotificationsContact",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeNotificationsContact_NotificationsContacts_NotificationsContactId",
                table: "EmployeeNotificationsContact",
                column: "NotificationsContactId",
                principalTable: "NotificationsContacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Merchants_MerchantId",
                table: "Employees",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategories_Merchants_MerchantId",
                table: "ItemCategories",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsModifierGroups_Merchants_MerchantId",
                table: "ItemsModifierGroups",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsModifierGroupsAssociation_MenuItems_MenuItemId",
                table: "ItemsModifierGroupsAssociation",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Location_Merchants_MerchantId",
                table: "Location",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemCategoryAssociation_MenuItems_MenuItemId",
                table: "MenuItemCategoryAssociation",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemModifier_MenuItems_MenuItemId",
                table: "MenuItemModifier",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Merchants_MerchantId",
                table: "MenuItems",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantFees_Merchants_MerchantId",
                table: "MerchantFees",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantFile_Merchants_MerchantId",
                table: "MerchantFile",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantInvoiceDocuments_Merchants_MerchantId",
                table: "MerchantInvoiceDocuments",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Merchants_MerchantServices_SetUpFeeId",
                table: "Merchants",
                column: "SetUpFeeId",
                principalTable: "MerchantServices",
                principalColumn: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantServices_Merchants_MerchantId",
                table: "MerchantServices");

            migrationBuilder.DropForeignKey(
                name: "FK_People_Merchants_MerchantId",
                table: "People");

            migrationBuilder.DropForeignKey(
                name: "FK_People_Merchants_SubMerchantId",
                table: "People");

            migrationBuilder.DropTable(
                name: "ApiClientRequests");

            migrationBuilder.DropTable(
                name: "ApplicationUserMerchant");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditNotification");

            migrationBuilder.DropTable(
                name: "CardCharges");

            migrationBuilder.DropTable(
                name: "DepositCaptureJournals");

            migrationBuilder.DropTable(
                name: "DepositCaptures");

            migrationBuilder.DropTable(
                name: "DepositJournals");

            migrationBuilder.DropTable(
                name: "DepositRefundJournals");

            migrationBuilder.DropTable(
                name: "DepositSurchargeJournal");

            migrationBuilder.DropTable(
                name: "DepositSurcharges");

            migrationBuilder.DropTable(
                name: "EmployeeNotificationsContact");

            migrationBuilder.DropTable(
                name: "ItemCategoryTranslation");

            migrationBuilder.DropTable(
                name: "ItemsModifierGroupsAssociation");

            migrationBuilder.DropTable(
                name: "ItemsModifierGroupTranslation");

            migrationBuilder.DropTable(
                name: "JournalChanges");

            migrationBuilder.DropTable(
                name: "JournalDetails");

            migrationBuilder.DropTable(
                name: "MbWayCharges");

            migrationBuilder.DropTable(
                name: "MenuItemCategoryAssociation");

            migrationBuilder.DropTable(
                name: "MenuItemModifier");

            migrationBuilder.DropTable(
                name: "MenuItemTranslation");

            migrationBuilder.DropTable(
                name: "MenuItemWeeklyAvailabilities");

            migrationBuilder.DropTable(
                name: "MerchantCustomCharge");

            migrationBuilder.DropTable(
                name: "MerchantFees");

            migrationBuilder.DropTable(
                name: "MerchantFile");

            migrationBuilder.DropTable(
                name: "MerchantInvoiceDocuments");

            migrationBuilder.DropTable(
                name: "OrderAdditionalInfo");

            migrationBuilder.DropTable(
                name: "OrderChangeLogs");

            migrationBuilder.DropTable(
                name: "OrderConfigurableFieldTranslation");

            migrationBuilder.DropTable(
                name: "OrderPreparationGroup");

            migrationBuilder.DropTable(
                name: "OrderSequence");

            migrationBuilder.DropTable(
                name: "PosChargeInvoiceItem");

            migrationBuilder.DropTable(
                name: "PosChargeSelectedMenuItem");

            migrationBuilder.DropTable(
                name: "PosChargeSyncAttempt");

            migrationBuilder.DropTable(
                name: "PosNotificationInboxMessages");

            migrationBuilder.DropTable(
                name: "Postings");

            migrationBuilder.DropTable(
                name: "PreparationGroupItems");

            migrationBuilder.DropTable(
                name: "PrinterNotificationsContact");

            migrationBuilder.DropTable(
                name: "PushNotificationsContact");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "SettlementDetails");

            migrationBuilder.DropTable(
                name: "SettlementServiceDetails");

            migrationBuilder.DropTable(
                name: "SpatialChannel");

            migrationBuilder.DropTable(
                name: "TerminalCharge");

            migrationBuilder.DropTable(
                name: "TicketMobileCharges");

            migrationBuilder.DropTable(
                name: "ApiClients");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Deposit");

            migrationBuilder.DropTable(
                name: "ItemCategories");

            migrationBuilder.DropTable(
                name: "CustomChargeMethods");

            migrationBuilder.DropTable(
                name: "OrderConfigurableField");

            migrationBuilder.DropTable(
                name: "OrderMenuItems");

            migrationBuilder.DropTable(
                name: "PosNotificationMessages");

            migrationBuilder.DropTable(
                name: "PreparationGroups");

            migrationBuilder.DropTable(
                name: "PrinterWorker");

            migrationBuilder.DropTable(
                name: "ApnsConnection");

            migrationBuilder.DropTable(
                name: "NotificationsContacts");

            migrationBuilder.DropTable(
                name: "PosCharges");

            migrationBuilder.DropTable(
                name: "Journals");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ItemsModifierGroups");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Charges");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "ChannelProfile");

            migrationBuilder.DropTable(
                name: "PosIntegrations");

            migrationBuilder.DropTable(
                name: "Merchants");

            migrationBuilder.DropTable(
                name: "MerchantServices");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
