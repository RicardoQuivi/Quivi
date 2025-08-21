using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Quivi.Application.Queries.Channels;
using Quivi.Application.Queries.Merchants;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Storage;
using Quivi.Infrastructure.Extensions;
using System.Reflection;

namespace Quivi.Application.Commands.Channels
{
    public class GenerateChannelsQrCodesAsyncCommand : ICommand<Task<byte[]>>
    {
        public int MerchantId { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public string? MainText { get; init; }
        public string? SecondaryText { get; init; }
    }

    public class GenerateChannelsQrCodesAsyncCommandHandler : ICommandHandler<GenerateChannelsQrCodesAsyncCommand, Task<byte[]>>
    {
        const decimal cardWidth_cm = 8.5M;
        const decimal cardHeight_cm = 5.5M;

        const decimal marginX_cm = 0.3M;
        const decimal marginY_cm = 0.6M;
        const decimal line1_cm = 3.30M;
        const decimal line0_cm = line1_cm + 2.45M;

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
                    foreach (var channel in channelsQuery)
                    {
                        auxContainer = auxContainer.Page(page =>
                        {
                            page.Size((float)cardWidth_cm, (float)cardHeight_cm, Unit.Centimetre);
                            page.MarginLeft((float)marginX_cm, Unit.Centimetre);
                            page.MarginRight((float)marginX_cm, Unit.Centimetre);
                            page.MarginTop((float)marginY_cm, Unit.Centimetre);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontFamily("Poppins"));

                            page.Content()
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

                                                if (merchantLogo != null)
                                                {
                                                    merchantLogo.Seek(0, SeekOrigin.Begin);
                                                    aux.Image(merchantLogo).FitArea();
                                                }

                                                row.RelativeItem()
                                                    .AlignBottom()
                                                    .Text(t =>
                                                    {
                                                        t.AlignEnd();
                                                        t.Span(channel.Identifier)
                                                            .FontSize(20)
                                                            .SemiBold();
                                                    });

                                                row.ConstantItem((float)rowHeight, Unit.Centimetre)
                                                    .AlignMiddle()
                                                    .AlignCenter()
                                                    .Background(Color.FromHex("#00FF00"))
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

                                                        var qrCode = GetQRCode(hostsSettings.GuestsApp.CombineUrl($"/c/{idConverter.ToPublicId(channel.Id)}"));

                                                        c.Item()
                                                            .Background(Color.FromHex("#FF0000"))
                                                            .Height((float)rowHeight - 0.3f - 0.3F, Unit.Centimetre)
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
                                                text.Span(command.MainText)
                                                    .SemiBold()
                                                    .FontSize(12);
                                            });

                                    column.Item()
                                            .Text(text =>
                                            {
                                                text.AlignCenter();
                                                text.Span(command.SecondaryText)
                                                    .FontColor("#A9A9A9")
                                                    .FontSize(8);
                                            });
                                    column.Item().Text("\n").FontSize(3);
                                    column.Item().Text("\n").FontSize(3);

                                    column.Item()
                                            .Height(0.6f, Unit.Centimetre)
                                            .Row(row =>
                                            {
                                                methodsImg.Seek(0, SeekOrigin.Begin);
                                                row.ConstantItem(2.3f, Unit.Centimetre)
                                                    .AlignMiddle()
                                                    .AlignCenter()
                                                    .Image(methodsImg).FitArea();

                                                row.RelativeItem()
                                                    .AlignCenter()
                                                    .AlignMiddle()
                                                    .Text(t =>
                                                    {
                                                        t.AlignCenter();
                                                        t.Span(channel.ChannelProfile!.Name)
                                                            .FontColor("#A9A9A9")
                                                            .FontSize(8);
                                                    });

                                                quiviLogo.Seek(0, SeekOrigin.Begin);
                                                row.ConstantItem(1.6f, Unit.Centimetre)
                                                    .AlignCenter()
                                                    .AlignMiddle()
                                                    .Image(quiviLogo).FitArea();
                                            });
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

        private byte[] GetQRCode(string qrCodeStr)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeStr, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule: 20, drawQuietZones: false);
        }
    }
}