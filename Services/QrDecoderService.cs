using System.Drawing;
using ZXing;
using ZXing.Windows.Compatibility;

namespace TamAnh_EMR_System.Services
{
    public class QrDecoderService
    {
        public string DecodeFromImage(string filePath)
        {
            var reader = new BarcodeReader();

            using var bitmap = (Bitmap)Image.FromFile(filePath);
            var result = reader.Decode(bitmap);

            return result?.Text;
        }
    }
}