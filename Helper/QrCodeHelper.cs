using QRCoder;

namespace TamAnh_EMR_System.Helpers
{
    public static class QrCodeHelper
    {
        public static byte[] Generate(string text)
        {
            using var qrGenerator =
                new QRCodeGenerator();

            using var qrData =
                qrGenerator.CreateQrCode(
                    text,
                    QRCodeGenerator.ECCLevel.Q);

            var qrCode =
                new PngByteQRCode(qrData);

            return qrCode.GetGraphic(20);
        }
    }
}