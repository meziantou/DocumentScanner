using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using WIA;

namespace DocumentScanner
{
    public class Scanner
    {
        public static bool Scan(string path)
        {
            string tempPath = Path.GetTempFileName();
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            ImageFormat format = ImageFormat.Jpeg;
            var dlg = new CommonDialog();
            ImageFile image = dlg.ShowAcquireImage(WiaDeviceType.ScannerDeviceType, WiaImageIntent.ColorIntent, WiaImageBias.MinimizeSize, format.Guid.ToString("B"), false, false, false);
            if (image == null)
                return false;

            image.SaveFile(tempPath);

            // Get a bitmap.
            var bmp1 = new Bitmap(tempPath);
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            Encoder myEncoder = Encoder.Quality;

            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 80L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(path, jgpEncoder, myEncoderParameters);
            return true;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
    }
}
