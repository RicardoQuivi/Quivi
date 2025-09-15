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

            var methodsImg = ReadResourceAsText("Quivi.Application.Resources.methods.svg") ?? throw new Exception();
            var quiviLogo = ReadResourceAsText("Quivi.Application.Resources.logo.svg") ?? throw new Exception();

            try
            {
                QuestPDF.Settings.License = LicenseType.Community;
                var pdfDocument = Document.Create(container =>
                {
                    var auxContainer = container;

                    var pageConfiguration = new PageConfiguration(command.PageSize);
                    var cardSize = new PageSize((float)CardComponent.Width_Cm, (float)CardComponent.Height_cm, Unit.Centimetre);

                    var availableSpaceXBetweenCards = pageConfiguration.Size.Width - (pageConfiguration.ItemsPerRow * cardSize.Width);
                    var availableSpaceYBetweenCards = pageConfiguration.Size.Height - (pageConfiguration.ItemsPerColumn * cardSize.Height);
                    var marginXBetweenCard = availableSpaceXBetweenCards / (pageConfiguration.ItemsPerRow * 2);
                    var marginYBetweenCard = availableSpaceYBetweenCards / (pageConfiguration.ItemsPerColumn * 2);

                    foreach (var channels in channelsQuery.Chunk(pageConfiguration.ItemsPerRow * pageConfiguration.ItemsPerColumn))
                        auxContainer = auxContainer.Page(page =>
                        {
                            page.Size(pageConfiguration.Size);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontFamily("Poppins"));

                            page.Content().Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    for (int i = 0; i < pageConfiguration.ItemsPerRow; i++)
                                        columns.ConstantColumn(cardSize.Width + marginXBetweenCard * 2);
                                });

                                foreach (var channel in channels)
                                {
                                    var card = new CardComponent
                                    {
                                        MerchantLogo = merchantLogo,
                                        MethodsSvg = methodsImg,
                                        QuiviSvg = quiviLogo,
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
            }
        }

        private static string ReadResourceAsText(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName) ?? throw new Exception($"Resource {resourceName} not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private class PageConfiguration
        {
            public PageSize Size { get; }
            public int ItemsPerRow { get; }
            public int ItemsPerColumn { get; }

            public PageConfiguration(QrCodePageSize pageSize)
            {
                switch (pageSize)
                {
                    case QrCodePageSize.Card:
                        Size = new PageSize((float)CardComponent.Width_Cm, (float)CardComponent.Height_cm, Unit.Centimetre);
                        ItemsPerColumn = 1;
                        ItemsPerRow = 1;
                        break;
                    case QrCodePageSize.A4:
                        Size = PageSizes.A4.Landscape();
                        ItemsPerRow = 3;
                        ItemsPerColumn = 3;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private class CardComponent : IComponent
        {
            public static readonly decimal Width_Cm = 8.5M;
            public static readonly decimal Height_cm = 5.5M;

            const decimal marginX_cm = 0.45M;
            const decimal marginY_cm = 0.45M;
            const decimal line1_cm = 3.30M;
            const decimal line0_cm = line1_cm + 2.35M;

            public required Stream? MerchantLogo { get; init; }
            public required Channel Channel { get; init; }
            public required IAppHostsSettings HostsSettings { get; init; }
            public required IIdConverter IdConverter { get; init; }
            public required string MethodsSvg { get; init; }
            public required string QuiviSvg { get; init; }
            public required string? MainText { get; init; }
            public required string? SecondaryText { get; init; }

            public void Compose(IContainer container)
            {
                container
                    .Width((float)Width_Cm, Unit.Centimetre)
                    .Height((float)Height_cm, Unit.Centimetre)
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
                                    .PaddingRight(0.3f, Unit.Centimetre) //Small padding so the text won't touch the QR Code
                                    .Text(t =>
                                    {
                                        t.AlignEnd();
                                        t.Span(Channel.Identifier)
                                            .FontSize(20)
                                            .SemiBold();
                                    });

                                row.ConstantItem((float)rowHeight, Unit.Centimetre)
                                    .AlignBottom()
                                    .AlignCenter()
                                    .Column(c =>
                                    {
                                        c.Spacing(0);
                                        //c.Item()
                                        //    .AlignCenter()
                                        //    .AlignMiddle()
                                        //    .Height(0.3f, Unit.Centimetre)
                                        //    .Text(text =>
                                        //    {
                                        //        text.AlignCenter();
                                        //    });

                                        var qrCode = GetQrCode(HostsSettings.GuestsApp.CombineUrl($"/c/{IdConverter.ToPublicId(Channel.Id)}"));
                                        c.Item()
                                            .Height((float)(rowHeight), Unit.Centimetre)
                                            .AlignBottom()
                                            .AlignCenter()
                                            .Svg(qrCode)
                                            .FitArea();

                                        //c.Item()
                                        //    .AlignCenter()
                                        //    .AlignMiddle()
                                        //    .Height(0.3f, Unit.Centimetre)
                                        //    .Text(text =>
                                        //    {
                                        //        text.AlignCenter();
                                        //    });
                                    });
                            });

                        column.Item().Text("\n").FontSize(3);
                        column.Item().Text("\n").FontSize(3);
                        column.Item().Text("\n").FontSize(3);

                        column.Item()
                            .Text(text =>
                            {
                                text.AlignCenter();
                                text.Span(MainText)
                                    .SemiBold()
                                    .FontSize(12);
                            });

                        column.Item().Text("\n").FontSize(2);
                        column.Item()
                            .Text(text =>
                            {
                                text.AlignCenter();
                                text.Span(SecondaryText)
                                    .FontColor("#A9A9A9")
                                    .FontSize(10);
                            });

                        column.Item().Text("\n").FontSize(3);
                        column.Item().Text("\n").FontSize(3);

                        column.Item()
                            .Height(0.6f, Unit.Centimetre)
                            .Row(row =>
                            {
                                row.ConstantItem(2.3f, Unit.Centimetre)
                                    .AlignMiddle()
                                    .AlignBottom()
                                    .Svg(MethodsSvg).FitArea();

                                row.RelativeItem()
                                    .AlignCenter()
                                    .AlignBottom()
                                    .Text(t =>
                                    {
                                        t.AlignCenter();
                                        t.Span(Channel.ChannelProfile!.Name)
                                            .FontColor("#A9A9A9")
                                            .FontSize(8);
                                    });

                                row.ConstantItem(1.6f, Unit.Centimetre)
                                    .AlignCenter()
                                    .AlignBottom()
                                    .Svg(QuiviSvg).FitArea();
                            });
                    });
            }

            private static string GetQrCode(string qrCodeStr)
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeStr, QRCodeGenerator.ECCLevel.Q);
                SvgQRCode qrCode = new SvgQRCode(qrCodeData);
                return qrCode.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White, false);
            }
        }
    }
}