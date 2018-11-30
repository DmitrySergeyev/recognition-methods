using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfRecognitionMethodsApp
{
    public class ImageProcessor
    {
        private string path;
        private BitmapImage originalBitmapImage;
        private WriteableBitmap binaryBitmapImage;
        private WriteableBitmap equalisedBitmapImage;
        private WriteableBitmap filteredBitmapImage;
        private byte[] transformFunctionNorm;
        private float[] transformFunction;

        private byte[] getOriginalPixels()
        {
            byte[] pixelData = new byte[this.originalBitmapImage.PixelHeight * this.originalBitmapImage.PixelWidth];
            this.originalBitmapImage.CopyPixels(
                pixelData,
                originalBitmapImage.PixelWidth * originalBitmapImage.Format.BitsPerPixel / 8,
                0);

            return pixelData;
        }

        private byte[,] getOriginalPixels2D ()
        { 
            return this.getTwoDimArray(this.getOriginalPixels(), this.GetOriginalWidth());               
        }

        private byte [,] getTwoDimArray (byte [] oneDimArray, int arrayWidth)
        {
            if (oneDimArray.Length % arrayWidth != 0)
            {
                throw new ArgumentException();
            }
            else
            {

                int x = oneDimArray.Length / arrayWidth;
                int y = arrayWidth;

                byte[,] result = new byte[x, y];

                for (int i = 0; i < oneDimArray.Length; i++)
                {
                    result[i / y, i % y] = oneDimArray[i];
                }

                return result;
            }
        }

        private byte[] getOneDimArray (byte[,] twoDimArray)
        {
            List<byte> buff = new List<byte>();

            foreach (byte a in twoDimArray)
            {
                buff.Add(a);
            }

            return buff.ToArray();
        }

        private WriteableBitmap writePixels(WriteableBitmap bitmap, byte[] pixels)
        {
            bitmap.WritePixels(
                            new Int32Rect(0, 0, originalBitmapImage.PixelWidth, originalBitmapImage.PixelHeight),
                            pixels,
                            originalBitmapImage.PixelWidth * originalBitmapImage.Format.BitsPerPixel / 8,
                            0);

            return bitmap;
        }

        private int getOriginalPixelsCount()
        {
            return this.originalBitmapImage.PixelHeight * this.originalBitmapImage.PixelWidth;
        }

        private float[] getHistogramFromArray(byte[] pixels)
        {
            int n = this.getOriginalPixelsCount();
            float[] histogram = Enumerable.Repeat(0.0f, 256).ToArray();

            pixels.ToList().ForEach(pixel => histogram[pixel]++);

            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] /= n;
            }

            return histogram;
        }
        private float[] getHistogramFromBitmap(BitmapSource bitmap)
        {
            byte[] pixels = new byte[bitmap.PixelHeight * bitmap.PixelWidth];
            bitmap.CopyPixels(
                pixels,
                bitmap.PixelWidth * bitmap.Format.BitsPerPixel / 8,
                0);

            return this.getHistogramFromArray(pixels);
        }

        public ImageProcessor(String path)
        {
            this.path = path;
            this.originalBitmapImage = new BitmapImage();
            this.originalBitmapImage.BeginInit();
            this.originalBitmapImage.UriSource = new Uri(this.path);
            this.originalBitmapImage.EndInit();
        }

        public int GetOriginalHeight()
        {
            return this.originalBitmapImage.PixelHeight;
        }

        public int GetOriginalWidth()
        {
            return this.originalBitmapImage.PixelWidth;
        }

        public BitmapImage GetOriginalImage()
        {
            return this.originalBitmapImage;
        }

        public float[] GetOriginalImageHistogram()
        {
            return this.getHistogramFromBitmap(this.originalBitmapImage);
        }

        public WriteableBitmap GetBinaryBitmapImage ()
        {
            if (this.binaryBitmapImage == null)
            {
                this.binaryBitmapImage = new WriteableBitmap(this.originalBitmapImage);
                byte[] pixels = this.getOriginalPixels();

                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = pixels[i] >= 128 ? (byte)255 : (byte)0;
                }

                return this.writePixels(this.binaryBitmapImage, pixels);
            }
            else
            {
                return this.binaryBitmapImage;
            }
        }

        public float[] GetEqualisedImageHistogram ()
        {
            if(this.equalisedBitmapImage == null)
            {
                this.GetEqualisedBitmapImage();
            }

            return this.getHistogramFromBitmap(this.equalisedBitmapImage);
        }

        public WriteableBitmap GetEqualisedBitmapImage ()
        {
            if (this.equalisedBitmapImage == null) {
                this.equalisedBitmapImage = new WriteableBitmap(this.originalBitmapImage);
                byte[] pixels = this.getOriginalPixels();
                float[] originalHistogram = this.getHistogramFromArray(this.getOriginalPixels());

                this.transformFunction = Enumerable.Repeat(0.0f, 256).ToArray();
                this.transformFunction[0] = originalHistogram[0];

                for (int i = 1; i < originalHistogram.Length; i++)
                {
                    this.transformFunction[i] = this.transformFunction[i - 1] + originalHistogram[i];
                }

                this.transformFunctionNorm = new byte[256];

                for (int i = 1; i < originalHistogram.Length; i++)
                {
                    transformFunctionNorm[i] = Convert.ToByte(this.transformFunction[i] * 255);
                }

                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = transformFunctionNorm[pixels[i]];
                }

                return this.writePixels(this.equalisedBitmapImage, pixels);
            }
            else
            {
                return this.equalisedBitmapImage;
            }
        }
        public float[] GetTransformFunction()
        {
            if (this.transformFunction == null)
            {
                this.GetEqualisedBitmapImage();
            }

            return this.transformFunction;
        }
        public byte[] GetTransformfunctionNorm()
        {
            if(this.transformFunctionNorm == null)
            {
                this.GetEqualisedBitmapImage();
            }

            return this.transformFunctionNorm;
        }
        public ImageSource GetFilteredImage(int filterValue)
        {

            this.filteredBitmapImage = new WriteableBitmap(this.originalBitmapImage);
            byte[,] pixels = this.getOriginalPixels2D();

            int x = pixels.GetLength(0);
            int y = pixels.GetLength(1);
            byte[,] result = new byte[x, y];

            int d = filterValue / 2;
            List<byte> buff = new List<byte>();

            for (int i1 = 0; i1 < x; i1++)
            {
                for (int j1 = 0; j1 < y; j1++)
                {
                    for (int i2 = (i1 - d); i2 <= (i1 + d); i2++)
                    {
                        for (int j2 = (j1 - d); j2 <= (j1 + d); j2++)
                        {
                            if (i2 >= 0 && i2 < x)
                            {
                                if (j2 >= 0 && j2 < y)
                                {
                                    buff.Add(pixels[i2, j2]);
                                }
                            }
                        }
                    }

                    result[i1, j1] = Convert.ToByte(buff.Average(a => a));
                    buff.Clear();
                }
            }

            return this.writePixels(this.filteredBitmapImage, this.getOneDimArray(result));
        }
    }
}
