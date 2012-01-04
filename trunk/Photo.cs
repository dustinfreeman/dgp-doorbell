using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DGPDoorbell
{
    static class Photo
    {

        public static string SaveMirrorImage(byte[] pixels, int w, int h)
        {
            byte[] newPixels = new byte[pixels.Length];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    newPixels[4 * ((w - i - 1) + j * w)] = pixels[4 * (i + j * w)];
                    newPixels[4 * ((w - i - 1) + j * w) + 1] = pixels[4 * (i + j * w) + 1];
                    newPixels[4 * ((w - i - 1) + j * w) + 2] = pixels[4 * (i + j * w) + 2];
                    newPixels[4 * ((w - i - 1) + j * w) + 3] = pixels[4 * (i + j * w) + 3];
                }
            }

            return Save(newPixels, w, h);
        }

        public static string Save(byte[] pixels, int w, int h)
        {
            string DirectoryName = "./images/";
            if (!Directory.Exists(DirectoryName))
            {
                Directory.CreateDirectory(DirectoryName);
            }

            //increments automatically.

            int i = 0;

            while (File.Exists(DirectoryName + i + ".jpg"))
            {
                i++;
            }

            Save(pixels, w, h, DirectoryName + i + ".jpg");

            return DirectoryName + i + ".jpg";
        }

        static void Save(byte[] pixels, int w, int h, string path)
        {
            // Define the image palette
            BitmapPalette myPalette = BitmapPalettes.Halftone256;

            // Creates a new empty image with the pre-defined palette

            BitmapSource image = BitmapSource.Create(
                w,
                h,
                96,
                96,
                PixelFormats.Bgr32,
                myPalette,
                pixels,
                w * 4);
            
            FileStream stream = new FileStream(path, FileMode.Create);
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 40;
            //encoder.Interlace = PngInterlaceOption.On;
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(stream);

            stream.Close();

        }
    }
}
