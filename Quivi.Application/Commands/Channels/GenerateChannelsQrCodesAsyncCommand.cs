using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Quivi.Application.Queries.Channels;
using Quivi.Application.Queries.Merchants;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Storage;
using Quivi.Infrastructure.Extensions;
using System.Reflection;

namespace Quivi.Application.Commands.Channels
{
    public enum QrCodePageSize
    {
        Card,
        A4,
    }

    public class GenerateChannelsQrCodesAsyncCommand : ICommand<Task<byte[]>>
    {
        public int MerchantId { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public string? MainText { get; init; }
        public string? SecondaryText { get; init; }
        public QrCodePageSize PageSize { get; init; }
    }

    public class GenerateChannelsQrCodesAsyncCommandHandler : ICommandHandler<GenerateChannelsQrCodesAsyncCommand, Task<byte[]>>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IStorageService storageService;
        private readonly IIdConverter idConverter;
        private readonly IAppHostsSettings hostsSettings;

        public GenerateChannelsQrCodesAsyncCommandHandler(IQueryProcessor queryProcessor, IStorageService storageService, IIdConverter idConverter, IAppHostsSettings hostsSettings)
        {
            this.queryProcessor = queryProcessor;
            this.storageService = storageService;
            this.idConverter = idConverter;
            this.hostsSettings = hostsSettings;
        }

        public async Task<byte[]> Handle(GenerateChannelsQrCodesAsyncCommand command)
        {
            var merchantQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                Ids = [command.MerchantId],
                PageSize = 1,
            });
            var merchant = merchantQuery.Single();

            var channelsQuery = await queryProcessor.Execute(new GetChannelsAsyncQuery
            {
                MerchantIds = [command.MerchantId],
                Ids = command.ChannelIds,
                IncludeChannelProfile = true,
                IsDeleted = false,
                PageSize = null,
            });

            var merchantPhotoPath = merchant.LogoUrl;
            MemoryStream? merchantLogo = null;
            if (string.IsNullOrWhiteSpace(merchantPhotoPath) == false)
            {
                merchantLogo = new MemoryStream();
                using (var s = await storageService.GetFileAsync(merchantPhotoPath))
                    await s.CopyToAsync(merchantLogo);
            }

            var methodsImg = Assembly.GetExecutingAssembly().GetManifestResourceStream("Quivi.Application.Resources.methods.png") ?? throw new Exception();
            var quiviLogo = Assembly.GetExecutingAssembly().GetManifestResourceStream("Quivi.Application.Resources.logo.png") ?? throw new Exception();

            try
            {
                QuestPDF.Settings.License = LicenseType.Community;
                var pdfDocument = Document.Create(container =>
                {
                    var auxContainer = container;

                    int itemsPerRow = command.PageSize == QrCodePageSize.Card ? 1 : 3;
                    int itemsPerColumn = command.PageSize == QrCodePageSize.Card ? 1 : 3;

                    var cardSize = new PageSize((float)CardComponent.Width_Cm, (float)CardComponent.Height_cm, Unit.Centimetre);
                    foreach (var channels in channelsQuery.Chunk(itemsPerRow * itemsPerColumn))
                    {
                        auxContainer = auxContainer.Page(page =>
                        {
                            var pageSize = GetPageSize(command.PageSize);
                            page.Size(pageSize);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontFamily("Poppins"));

                            var availableSpaceXBetweenCards = pageSize.Width - (itemsPerRow * cardSize.Width);
                            var availableSpaceYBetweenCards = pageSize.Height - (itemsPerColumn * cardSize.Height);
                            var marginXBetweenCard = availableSpaceXBetweenCards / (itemsPerRow * 2);
                            var marginYBetweenCard = availableSpaceYBetweenCards / (itemsPerColumn * 2);

                            page.Content().Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    for (int i = 0; i < itemsPerRow; i++)
                                        columns.ConstantColumn(cardSize.Width + marginXBetweenCard * 2);
                                });

                                foreach (var channel in channels)
                                {
                                    var card = new CardComponent
                                    {
                                        MerchantLogo = merchantLogo,
                                        MethodsImg = methodsImg,
                                        QuiviLogo = quiviLogo,
                                        Channel = channel,
                                        HostsSettings = hostsSettings,
                                        IdConverter = idConverter,
                                        MainText = command.MainText,
                                        SecondaryText = command.SecondaryText,
                                    };

                                    table.Cell()
                                        .PaddingHorizontal(marginXBetweenCard)
                                        .PaddingVertical(marginYBetweenCard)
                                        .Element(cell =>
                                        {
                                            if (command.PageSize != QrCodePageSize.Card)
                                                cell = cell.BorderColor(Colors.Grey.Lighten2).Border(1);

                                            cell.Component(card);
                                        });
                                }
                            });
                        });
                    }
                });

                using var ms = new MemoryStream();
                pdfDocument.GeneratePdf(ms);
                return ms.ToArray();
            }
            finally
            {
                if (merchantLogo != null)
                {
                    merchantLogo.Dispose();
                    await merchantLogo.DisposeAsync();
                }
                methodsImg.Dispose();
                quiviLogo.Dispose();

                await methodsImg.DisposeAsync();
                await quiviLogo.DisposeAsync();
            }
        }

        public PageSize GetPageSize(QrCodePageSize pageSize)
        {
            return pageSize switch
            {
                QrCodePageSize.Card => new PageSize((float)CardComponent.Width_Cm, (float)CardComponent.Height_cm, Unit.Centimetre),
                QrCodePageSize.A4 => PageSizes.A4.Landscape(),
                _ => throw new NotImplementedException(),
            };
        }

        public class CardComponent : IComponent
        {
            public static readonly decimal Width_Cm = 8.5M;
            public static readonly decimal Height_cm = 5.5M;

            const decimal marginX_cm = 0.3M;
            const decimal marginY_cm = 0.6M;
            const decimal line1_cm = 3.30M;
            const decimal line0_cm = line1_cm + 2.45M;

            public required Stream? MerchantLogo { get; init; }
            public required Channel Channel { get; init; }
            public required IAppHostsSettings HostsSettings { get; init; }
            public required IIdConverter IdConverter { get; init; }
            public required Stream MethodsImg { get; init; }
            public required Stream QuiviLogo { get; init; }
            public required string? MainText { get; init; }
            public required string? SecondaryText { get; init; }

            public void Compose(IContainer container)
            {
                container
                    .Width((float)(Width_Cm), Unit.Centimetre)
                    .Height((float)(Height_cm), Unit.Centimetre)
                    .PaddingLeft((float)marginX_cm, Unit.Centimetre)
                    .PaddingRight((float)marginX_cm, Unit.Centimetre)
                    .PaddingTop((float)marginY_cm, Unit.Centimetre)
                    .Background(Colors.White)
                    .DefaultTextStyle(x => x.FontFamily("Poppins"))
                    .Column(column =>
                    {
                        column.Spacing(0);

                        var rowHeight = line0_cm - line1_cm;
                        column.Item()
                            .Height((float)rowHeight, Unit.Centimetre)
                            .Row(row =>
                            {
                                var aux = row.ConstantItem((float)rowHeight, Unit.Centimetre)
                                    .AlignMiddle();

                                if (MerchantLogo != null)
                                {
                                    MerchantLogo.Seek(0, SeekOrigin.Begin);
                                    aux.Image(MerchantLogo).FitArea();
                                }

                                row.RelativeItem()
                                    .AlignBottom()
                                    .Text(t =>
                                    {
                                        t.AlignEnd();
                                        t.Span(Channel.Identifier)
                                            .FontSize(20)
                                            .SemiBold();
                                    });

                                row.ConstantItem((float)rowHeight, Unit.Centimetre)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Column(c =>
                                    {
                                        c.Spacing(0);
                                        c.Item()
                                            .AlignCenter()
                                            .AlignMiddle()
                                            .Height(0.3f, Unit.Centimetre)
                                            .Text(text =>
                                            {
                                                text.AlignCenter();
                                            });

                                        var qrCode = GetQRCode(HostsSettings.GuestsApp.CombineUrl($"/c/{IdConverter.ToPublicId(Channel.Id)}"));
                                        c.Item()
                                            .Height((float)(rowHeight - 0.3M - 0.3M), Unit.Centimetre)
                                            .AlignBottom()
                                            .AlignCenter()
                                            .Image(qrCode).FitArea();

                                        c.Item()
                                            .AlignCenter()
                                            .AlignMiddle()
                                            .Height(0.3f, Unit.Centimetre)
                                            .Text(text =>
                                            {
                                                text.AlignCenter();
                                            });
                                    });
                            });

                        column.Item().Text("\n").FontSize(3);
                        column.Item()
                            .Text(text =>
                            {
                                text.AlignCenter();
                                text.Span(MainText)
                                    .SemiBold()
                                    .FontSize(12);
                            });

                        column.Item()
                            .Text(text =>
                            {
                                text.AlignCenter();
                                text.Span(SecondaryText)
                                    .FontColor("#A9A9A9")
                                    .FontSize(8);
                            });

                        column.Item().Text("\n").FontSize(3);
                        column.Item().Text("\n").FontSize(3);

                        column.Item()
                            .Height(0.6f, Unit.Centimetre)
                            .Row(row =>
                            {
                                MethodsImg.Seek(0, SeekOrigin.Begin);
                                row.ConstantItem(2.3f, Unit.Centimetre)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Image(MethodsImg).FitArea();

                                row.RelativeItem()
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text(t =>
                                    {
                                        t.AlignCenter();
                                        t.Span(Channel.ChannelProfile!.Name)
                                            .FontColor("#A9A9A9")
                                            .FontSize(8);
                                    });

                                QuiviLogo.Seek(0, SeekOrigin.Begin);
                                row.ConstantItem(1.6f, Unit.Centimetre)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Image(QuiviLogo).FitArea();
                            });
                    });
            }

            private static byte[] GetQRCode(string qrCodeStr)
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeStr, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                return qrCode.GetGraphic(pixelsPerModule: 20, drawQuietZones: false);
            }
        }
    }
}