using ESCPOS_NET.Emitters;
using ESCPOS_NET.Emitters.BaseCommandValues;
using ESCPOS_NET.Extensions;
using System.Text;

namespace Quivi.Infrastructure.Pos.ESCPOS_NET
{
    public class CharacterSafeCommandEmitter : BaseCommandEmitter, IDisposable
    {
        protected int LineSize = 48;

        public override byte[] Print(string data)
        {
            return base.Print(CharacterSafeString(data));
        }

        public override byte[] PrintLine(string line)
        {
            return base.PrintLine(CharacterSafeString(line));
        }

        public byte[] AlignToSides(string leftSide, string rightSide)
        {
            leftSide = leftSide ?? "";
            rightSide = rightSide ?? "";
            var spaces = LineSize - leftSide.Length - rightSide.Length;
            var spacesPlaceholder = Enumerable.Repeat(1, Math.Max(0, spaces)).Select(_ => ' ').Aggregate("", (l, r) => l + r);
            return Print($"{leftSide}{spacesPlaceholder}{rightSide}");
        }

        public byte[] PrintLargeQrCode(string data)
        {
            List<byte> list = new List<byte>();
            byte[] array = [Cmd.GS, Barcodes.Set2DCode, Barcodes.PrintBarcode];
            list.AddRange(array, Barcodes.SelectQRCodeModel, (byte)TwoDimensionCodeType.QRCODE_MODEL2, Barcodes.AutoEnding);
            list.AddRange(array, Barcodes.SetQRCodeDotSize, (byte)8);
            list.AddRange(array, Barcodes.SetQRCodeCorrectionLevel, (byte)CorrectionLevel2DCode.PERCENT_7);
            int num = data.Length + 3;
            int num2 = num % 256;
            int num3 = num / 256;
            list.AddRange(array, (byte)num2, (byte)num3, Barcodes.StoreQRCodeData);
            list.AddRange(from x in data.ToCharArray()
                          select (byte)x);
            list.AddRange(array, Barcodes.PrintQRCode);
            return list.ToArray();
        }

        //public byte[] PrintLabelWithBackground(string title, string subtitle)
        //{
        //    Bitmap bitmap = new Bitmap(500, 150);

        //    // Create graphics object from the bitmap
        //    using (Graphics graphics = Graphics.FromImage(bitmap))
        //    {
        //        // Fill the background with white color
        //        graphics.Clear(System.Drawing.Color.Black);

        //        // Define the rectangle to draw the number
        //        System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(0, 0, 500, 150);

        //        // Draw the rectangle
        //        Pen pen = new Pen(System.Drawing.Color.Black, 2);
        //        graphics.DrawRectangle(pen, rectangle);

        //        // Draw title
        //        Font font = new Font("SourceSansPro Regular", 70, FontStyle.Bold);
        //        StringFormat stringFormat = new StringFormat();
        //        stringFormat.Alignment = StringAlignment.Center;
        //        stringFormat.LineAlignment = StringAlignment.Center;
        //        graphics.DrawString(title, font, Brushes.White, rectangle, stringFormat);

        //        if (!string.IsNullOrEmpty(subtitle))
        //        {
        //            // Draw subtitle
        //            font = new Font("SourceSansPro Regular", 15);
        //            stringFormat = new StringFormat();
        //            stringFormat.Alignment = StringAlignment.Center;
        //            stringFormat.LineAlignment = StringAlignment.Far;
        //            graphics.DrawString(subtitle, font, Brushes.White, rectangle, stringFormat);
        //        }

        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
        //            memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position to beginning

        //            return PrintImage(memoryStream.ToArray(), false, true);
        //        }
        //    }
        //}

        private static string CharacterSafeString(string value) => Encoding.UTF8.GetString(Encoding.GetEncoding("iso-8859-7").GetBytes(value));

        public void Dispose() { }
    }
}