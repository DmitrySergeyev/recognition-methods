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
        private WriteableBitmap originalBitmapImage;
        private WriteableBitmap binaryBitmapImage;
        private WriteableBitmap equalisedBitmapImage;
        private WriteableBitmap filteredBitmapImage;
        private WriteableBitmap skeletBitmapImage;
        private WriteableBitmap recognitionBitmap;

        public byte[] getSkeletBitmapImagePixels()
        {
            return this.getImagePixels(this.skeletBitmapImage);
        }

        private WriteableBitmap toFourierBitmap;
        private WriteableBitmap fourierImage;
        private WriteableBitmap fromFourierBitmap;

        private byte[] transformFunctionNorm;
        private float[] transformFunction;
        private Complex[,] fourierData;
        private Complex[,] fourierData2;

        private byte[] getOriginalPixels()
        {
            byte[] result = this.getImagePixels(this.originalBitmapImage);
            return result;
        }

        public byte[,] GetOriginalPixels2D()
        {
            byte[,] result = this.getTwoDimArray(this.getOriginalPixels(), this.GetOriginalWidth());
            return result;
        }

        public byte[] getImagePixels(BitmapSource imageSource)
        {
            byte[] pixelData = new byte[imageSource.PixelHeight * imageSource.PixelWidth];
            imageSource.CopyPixels(
                pixelData,
                imageSource.PixelWidth * imageSource.Format.BitsPerPixel / 8,
                0);

            return pixelData;
        }

        private byte[,] GetOrigignalPixels2D()
        {
            return this.getTwoDimArray(this.getOriginalPixels(), this.GetOriginalWidth());
        }

        public byte[,] GetImagePixels2D()
        {
            return this.getTwoDimArray(this.getImagePixels(this.toFourierBitmap), this.toFourierBitmap.PixelWidth);
        }

        public byte[,] getTwoDimArray(byte[] oneDimArray, int arrayWidth)
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

                //for (int i = 0; i < x; i++)
                //{
                //    result[i] = new byte[y];
                //}

                for (int i = 0; i < oneDimArray.Length; i++)
                {
                    result[i / y, i % y] = oneDimArray[i];
                }

                return result;
            }
        }

        private double[] getOneDimArray(double[,] twoDimArray)
        {
            List<double> result = new List<double>();

            for (int i = 0; i <= twoDimArray.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= twoDimArray.GetUpperBound(1); j++)
                {
                    result.Add(twoDimArray[i, j]);
                }
            }

            return result.ToArray();
        }

        private byte[] getOneDimArray(byte[,] twoDimArray)
        {
            List<byte> buff = new List<byte>();

            foreach (byte a in twoDimArray)
            {
                buff.Add(a);
            }

            byte[] result = buff.ToArray();
            return result;
        }

        public WriteableBitmap writePixels(WriteableBitmap bitmap, byte[] pixels)
        {
            bitmap.WritePixels(
                            new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                            pixels,
                            bitmap.PixelWidth * bitmap.Format.BitsPerPixel / 8,
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

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(this.path);
            bi.EndInit();
            this.originalBitmapImage = new WriteableBitmap(bi);

            //BitmapImage bi = new BitmapImage();
            //bi.BeginInit();
            //bi.UriSource = new Uri(this.path);
            //bi.EndInit();
            //CroppedBitmap cb = new CroppedBitmap(bi, new Int32Rect(50, 50, 16, 16));
            //this.originalBitmapImage = new WriteableBitmap( cb );
        }

        public int GetOriginalHeight()
        {
            return this.originalBitmapImage.PixelHeight;
        }

        public int GetOriginalWidth()
        {
            return this.originalBitmapImage.PixelWidth;
        }

        // public BitmapImage GetOriginalImage()
        public WriteableBitmap GetOriginalImage()
        {
            return this.originalBitmapImage;
        }

        public float[] GetOriginalImageHistogram()
        {
            return this.getHistogramFromBitmap(this.originalBitmapImage);
        }

        public ImageSource GetSubImage(int x, int y, int dx, int dy)
        {
            byte[] pixels = this.GetSomePixels(x, y, dx, dy);

            this.toFourierBitmap = new WriteableBitmap(
                dx,
                dy,
                this.originalBitmapImage.DpiX,
                this.originalBitmapImage.DpiY,
                this.originalBitmapImage.Format,
                this.originalBitmapImage.Palette
            );

            //CroppedBitmap cb = new CroppedBitmap(this.originalBitmapImage, new Int32Rect(x, y, dx, dy));
            //w.image2DFourierSeries.Source = cb;

            //this.image2D = new WriteableBitmap(
            //           64,
            //           64,
            //           this.originalBitmapImage.DpiX,
            //           this.originalBitmapImage.DpiY,
            //           this.originalBitmapImage.Format,
            //           this.originalBitmapImage.Palette);

            this.writePixels(this.toFourierBitmap, pixels);

            return this.toFourierBitmap;
        }

        public ImageSource GetSubRecognitionImage(int x, int y, int dx, int dy)
        {
            byte[] pixels = this.GetSomePixels(x, y, dx, dy);

            this.recognitionBitmap = new WriteableBitmap(
                dx,
                dy,
                this.originalBitmapImage.DpiX,
                this.originalBitmapImage.DpiY,
                this.originalBitmapImage.Format,
                this.originalBitmapImage.Palette
            );

            this.writePixels(this.recognitionBitmap, pixels);

            return this.recognitionBitmap;
        }

        public WriteableBitmap GetBinaryBitmapImage()
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

        public float[] GetEqualisedImageHistogram()
        {
            if (this.equalisedBitmapImage == null)
            {
                this.GetEqualisedBitmapImage();
            }

            return this.getHistogramFromBitmap(this.equalisedBitmapImage);
        }

        public WriteableBitmap GetEqualisedBitmapImage()
        {
            if (this.equalisedBitmapImage == null)
            {
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
            if (this.transformFunctionNorm == null)
            {
                this.GetEqualisedBitmapImage();
            }

            return this.transformFunctionNorm;
        }
        public ImageSource GetFilteredImage(int filterValue)
        {
            this.filteredBitmapImage = new WriteableBitmap(this.originalBitmapImage);
            byte[,] pixels = this.GetOriginalPixels2D();

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

        public int[] GetSomePixels(int x, int y, int pixelsCount)
        {
            int[] result = new int[pixelsCount];

            if (this.GetOriginalWidth() * y + x + pixelsCount <= this.getOriginalPixelsCount())
            {
                Array.Copy(this.getOriginalPixels(), (this.GetOriginalWidth() * y + x), result, 0, pixelsCount);
            }

            return result;
        }

        public byte[] GetSomePixels(int x, int y, int dx, int dy)
        {
            byte [] result = this.getOneDimArray(this.GetSome2DPixelsFromOriginImage(x, y, dx, dy));
            return result;
        }

        public byte[,] GetSome2DPixels(byte[,] source, int x, int y, int dy, int dx)
        {
            byte[,] result = new byte[dy, dx];

            //if ((x + dx) < this.GetOriginalWidth() && (y + dy) < this.GetOriginalHeight())
            //{
            for (int j = 0; j < dy; j++)
            {
                for (int i = 0; i < dx; i++)
                {
                    //result[i, j] = source[y + i, x + j];
                    result[j, i] = source[y + j, x + i];
                }
            }
            //}

            return result;
        }

        public byte[,] GetSome2DPixelsFromOriginImage(int x, int y, int dx, int dy)
        {
            byte[,] result = this.GetSome2DPixels(this.GetOriginalPixels2D(), x, y, dx, dy);
            return result;
        }

        public double[] GetOneDimentuionalFourier(int[] pixels)
        {
            int m = pixels.Length;
            double[] result = new double[m];

            double re = 0.0d;
            double im = 0.0d;

            for (int i = 0; i < m; i++)
            {
                re = 0.0d;
                im = 0.0d;

                for (int x = 0; x < m; x++)
                {
                    double q = 2 * Math.PI * i * x / m;
                    double s1 = Math.Sin(q);
                    double s2 = Math.Cos(q);
                    re += pixels[x] * Math.Cos(q) * Math.Pow(-1.0, x);
                    im -= pixels[x] * Math.Sin(q) * Math.Pow(-1.0, x);
                }

                re /= m;
                im /= m;

                result[i] = Math.Pow((Math.Pow(re, 2) + Math.Pow(im, 2)), 0.5);
            }

            return result;
        }

        public Complex[,] ToFourier(byte[,] input)
        {
            int n = input.GetLength(0);
            int m = input.GetLength(1);

            Complex[,] output = this.GetComplexArray(n, m);

            double re = 0.0d;
            double im = 0.0d;
            double arg;
            int c;

            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < m; x++)
                {
                    re = 0.0d;
                    im = 0.0d;

                    for (int v = 0; v < n; v++)
                    {
                        for (int u = 0; u < m; u++)
                        {
                            arg = 2.0 * Math.PI * (((double)u * x / m) + ((double)v * y / n));

                            if ((u + v) % 2 == 1)
                            {
                                c = -1;
                            }
                            else
                            {
                                c = 1;
                            }

                            re += input[v, u] * Math.Cos(arg) * c;
                            im -= input[v, u] * Math.Sin(arg) * c;
                        }
                    }

                    re /= m;
                    re /= n;
                    im /= m;
                    im /= n;

                    output[y, x].a = re;
                    output[y, x].b = im;
                }
            }

            return output;
        }

        private Complex[,] GetComplexArray(int n, int m)
        {
            Complex[,] output = new Complex[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    output[i, j] = new Complex();
                }
            }

            return output;
        }

        public Complex[,] FromFourier(Complex[,] input)
        {
            int n = input.GetLength(0);
            int m = input.GetLength(1);

            Complex[,] output = this.GetComplexArray(n, m);

            double re = 0.0d;
            double im = 0.0d;
            double arg, c, d;

            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < m; x++)
                {
                    re = 0.0d;
                    im = 0.0d;

                    for (int v = 0; v < n; v++)
                    {
                        for (int u = 0; u < m; u++)
                        {
                            arg = 2.0 * Math.PI * (((double)u * x / m) + ((double)v * y / n));
                            c = Math.Cos(arg);
                            d = Math.Sin(arg);

                            re += input[v, u].a * c - input[v, u].b * d;
                            im += input[v, u].a * d + input[v, u].b * c;
                        }
                    }

                    output[y, x].a = re;
                    output[y, x].b = im;
                }
            }

            return output;
        }

        private double[,] getFourierSpectre(Complex[,] fourierImage)
        {
            double[,] result = new double[fourierImage.GetLength(0), fourierImage.GetLength(1)];

            for (int i = 0; i < fourierImage.GetLength(0); i++)
            {
                for (int j = 0; j < fourierImage.GetLength(1); j++)
                {
                    result[i, j] = Math.Pow(
                        Math.Pow(fourierImage[i, j].a, 2) +
                        Math.Pow(fourierImage[i, j].b, 2),
                        0.5);
                }
            }

            return result;
        }

        public WriteableBitmap GetTwoDimentuionalFourierImage()
        {
            byte[,] pixels = this.GetImagePixels2D();
            this.fourierData = this.ToFourier(pixels);

            double[] buf = this.getOneDimArray(
                this.getFourierSpectre(
                    this.fourierData));

            int n = pixels.GetUpperBound(0) + 1;
            int m = pixels.GetUpperBound(1) + 1;

            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] = Math.Log(buf[i] /*- min + 1*/, 2);
            }


            double min = buf.Min<double>();
            double max = buf.Max<double>();
            double span = max - min;

            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] -= min;
            }

            byte[] result = new byte[n * m];

            for (int i = 0; i < n * m; i++)
            {
                result[i] = Convert.ToByte((buf[i]) / span * 255.0);
            }

            this.fourierImage = new WriteableBitmap(this.toFourierBitmap);
            //  result = this.logBitmapImage(result);
            this.fourierImage = this.writePixels(this.fourierImage, result);

            return this.fourierImage;
        }

        public WriteableBitmap GetTwoDimentuionalBackFourierImage()
        {
            byte[,] pixels = this.GetImagePixels2D();
            Complex[,] buf = this.FromFourier(this.fourierData);

            int n = pixels.GetLength(0);
            int m = pixels.GetLength(1);

            double[] buf1 = new double[n * m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    buf1[i * n + j] = buf[i, j].a;
                }
            }

            double max = buf1.Max<double>();
            double min = buf1.Min<double>();
            double span = max - min;

            byte[] result = new byte[n * m];

            for (int i = 0; i < n * m; i++)
            {
                result[i] = Convert.ToByte((buf1[i] - min) / span * 255.0);
            }

            // result = this.logBitmapImage(result);

            this.fromFourierBitmap = new WriteableBitmap(this.toFourierBitmap);
            this.fromFourierBitmap = this.writePixels(this.fromFourierBitmap, result);

            return this.fromFourierBitmap;
        }

        private byte[] logBitmapImage(double[] pixels)
        {

            double[] f = new double[256];
            byte[] t = new byte[256];
            byte[] result = new byte[pixels.Length];

            for (int i = 0; i < f.Length; i++)
            {
                f[i] = Math.Log(i + 1, 2);
            }

            double max = f.Max<double>();
            double min = f.Min<double>();
            double span = max - min;

            for (int i = 0; i < f.Length; i++)
            {
                t[i] = Convert.ToByte((f[i] - min) / span * 255.0);
            }

            for (int i = 0; i < pixels.GetLength(0); i++)
            {
                result[i] = t[Convert.ToByte(pixels[i])];
            }

            return result;
        }
        private byte[] logBitmapImage(byte[] pixels)
        {
            double[] f = new double[256];
            byte[] t = new byte[256];

            for (int i = 0; i < f.Length; i++)
            {
                f[i] = Math.Log(i + 1, 2);
            }

            double max = f.Max<double>();
            double min = f.Min<double>();
            double span = max - min;

            for (int i = 0; i < f.Length; i++)
            {
                t[i] = Convert.ToByte((f[i] - min) / span * 255.0);
            }

            for (int i = 0; i < pixels.GetLength(0); i++)
            {
                pixels[i] = t[pixels[i]];
            }

            return pixels;
        }

        public WriteableBitmap GetSkeletBitmapImage()
        {
            //if (this.skeletBitmapImage == null)
            //{
            this.skeletBitmapImage = this.GetBinaryBitmapImage();

            byte[,] pixels1 = this.GetOrigignalPixels2D();

            int height = pixels1.GetUpperBound(0) + 1;
            int width = pixels1.GetUpperBound(1) + 1;

            byte[,] pixels2 = new byte[height, width];

            // Binarisation 
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pixels1[i, j] = pixels1[i, j] >= 128 ? (byte)0 : (byte)1;
                    pixels2[i, j] = 1;
                }
            }

            // Borders
            //for (int i = 0; i < height; i++)
            //{
            //    pixels1[i, 0] = 255;
            //    pixels1[i, width - 1] = 255;
            //}

            //for (int i = 0; i < width; i++)
            //{
            //    pixels1[0, i] = 255;
            //    pixels1[height - 1, i] = 255;
            //}

            //    byte[,] pixels2 = pixels1;
            int goOn = 0;

            //do
            //{
            goOn = 0;

            // STEP 1
            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 1; j < width - 1; j++)
                {
                    int[] D8 = {
                                pixels1[i - 1, j], pixels1[i - 1, j + 1], pixels1[i, j + 1], pixels1[i + 1, j + 1],
                                pixels1[i + 1, j], pixels1[i + 1, j - 1], pixels1[i, j - 1], pixels1[i - 1, j - 1],
                            };

                    int Np = D8.Sum();
                    int Tp = 0;

                    for (int k = 0; k < D8.Length - 1; k++)
                    {
                        if (D8[k] == 0 && D8[k + 1] == 1)
                        {
                            Tp++;
                        }

                    }

                    if (D8[D8.Length - 1] == 0 && D8[0] == 1)
                    {
                        Tp++;
                    }

                    if (Np >= 2 && Np <= 6)
                    {
                        if (Tp == 1)
                        {
                            if ((D8[0] * D8[2] * D8[4]) == 0)
                            {
                                if ((D8[2] * D8[4] * D8[6]) == 0)
                                {
                                    pixels2[i, j] = 0;
                                    goOn++;
                                }
                            }
                        }
                    }
                }
            }

            // DELETE PIXELS
            // pixels1 = pixels2
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pixels1[i, j] *= pixels2[i, j];
                }
            }

            // STEP 2
            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 1; j < width - 1; j++)
                {
                    int[] D8 = {
                                pixels1[i - 1, j], pixels1[i - 1, j + 1], pixels1[i, j + 1], pixels1[i + 1, j + 1],
                                pixels1[i + 1, j], pixels1[i + 1, j - 1], pixels1[i, j - 1], pixels1[i - 1, j - 1],
                            };

                    int Np = D8.Sum();
                    int Tp = 0;

                    for (int k = 0; k < D8.Length - 1; k++)
                    {
                        if (D8[k] == 0 && D8[k + 1] == 1)
                        {
                            Tp++;
                        }

                        if (D8[D8.Length - 1] == 0 && D8[0] == 1)
                        {
                            Tp++;
                        }
                    }

                    if (Np >= 2 && Np <= 6)
                    {
                        if (Tp == 1)
                        {
                            if ((D8[0] * D8[2] * D8[6]) == 0)
                            {
                                if ((D8[0] * D8[4] * D8[6]) == 0)
                                {
                                    pixels2[i, j] = 0;
                                    goOn++;
                                }
                            }
                        }
                    }
                }
            }

            // DELETE PIXELS
            //pixels1 = pixels2;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pixels1[i, j] *= pixels2[i, j];
                }
            }
            //}
            //while (goOn != 0);

            byte[] result = this.getOneDimArray(pixels1);

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] == 0)
                {
                    result[i] = 255;
                }
                else
                {
                    result[i] = 0;
                }
            }

            return this.writePixels(this.skeletBitmapImage, result);
            //}
            //else
            //{
            //    return this.skeletBitmapImage;
            //}
        }

        public WriteableBitmap getFilteredSkeletBitmapImage()
        {
            byte[,] pixels1 = this.getTwoDimArray(this.getImagePixels(this.skeletBitmapImage), this.GetOriginalWidth());
            int height = pixels1.GetUpperBound(0);
            int width = pixels1.GetUpperBound(1);

            for (int i = 0; i < height - 1; i++)
            {
                for (int j = 0; j < width - 1; j++)
                {
                    int[] D4 =
                    {
                        pixels1[i, j], pixels1[i, j + 1], pixels1[j + 1, i + 1], pixels1[i, j + 1]
                    };

                    if (D4.Sum() == 3)
                    {
                        if (D4[0] == 1 && D4[1] == 1 && D4[2] == 1) pixels1[i, j + 1] = 0;
                        if (D4[1] == 1 && D4[2] == 1 && D4[3] == 1) pixels1[i + 1, j + 1] = 0;
                        if (D4[2] == 1 && D4[3] == 1 && D4[0] == 1) pixels1[i + 1, j] = 0;
                        if (D4[3] == 1 && D4[0] == 1 && D4[1] == 1) pixels1[i, j] = 0;
                    }
                }
            }

            byte[] result = this.getOneDimArray(pixels1);

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] == 0)
                {
                    result[i] = 255;
                }
                else
                {
                    result[i] = 0;
                }
            }

            return this.writePixels(this.skeletBitmapImage, result);
        }

        private double[] calculateMoments(byte[,] pixels)
        {
            int x = pixels.GetLength(0);
            int y = pixels.GetLength(1);

            long m00 = 0;
            long m01 = 0;
            long m11 = 0;
            long m02 = 0;
            long m03 = 0;
            long m10 = 0;
            long m20 = 0;
            long m30 = 0;
            long m12 = 0;
            long m21 = 0;

            for (int j = 0; j < y; j++)
            {
                for (int i = 0; i < x; i++)
                {
                    m00 += pixels[i, j];
                    m01 += m00 * i;
                    m10 += m00 * j;

                    m11 += m00 * i * j;
                    m02 += m01 * i;
                    m20 += m10 * j;

                    m03 += m02 * i;
                    m30 += m20 * j;
                    m21 += m20 * i;
                    m12 += m02 * j;
                }
            }

            double X = 0;
            double Y = 0;

            if (m00 == 0)
            {
                X = m10 / m00;
                Y = m01 / m00;
            }
            else
            {
                X = 0;
                Y = 0;
            }

            double k00 = m00;
            double k10 = 0.0;
            double k01 = 0.0;
            double k11 = m11 - X * m10;
            double k20 = m20 - X * m10;
            double k02 = m02 - Y * m01;
            double k30 = m30 - 3 * X * m20 + 2 * Math.Pow(X, 2.0) * m10;
            double k03 = m03 - 3 * Y * m02 + 2 * Math.Pow(Y, 2.0) * m01;
            double k21 = m21 - 2 * X * m11 - Y * m20 + 2 * Math.Pow(X, 2.0) * m01;
            double k12 = m12 - 2 * Y * m11 - X * m02 + 2 * Math.Pow(Y, 2.0) * m10;

            double p11 = k11 / Math.Pow(k00, 2.0);
            double p20 = k20 / Math.Pow(k00, 2.0);
            double p30 = k30 / Math.Pow(k00, 2.5);
            double p02 = k02 / Math.Pow(k00, 2.0);
            double p03 = k02 / Math.Pow(k00, 2.5);
            double p12 = k12 / Math.Pow(k00, 2.5);
            double p21 = k21 / Math.Pow(k00, 2.5);

            double F1 = p20 - p02;
            double F2 = Math.Pow((p20 - p02), 2.0) + 4 * Math.Pow(p11, 2.0);
            double F3 = Math.Pow((p30 - 3 * p12), 2.0) + Math.Pow((3 * p21 - p03), 2.0);
            double F4 = Math.Pow((p30 + p12), 2.0) + Math.Pow((p21 + p03), 2.0);

            return new double[] { F1, F2, F3, F4 };
        }

        private double calculateDiff(double[] mainMoments, double[] currentMomments)
        {
            double result = 0.0;

            for (int i = 0; i < 4; i++)
            {
                result += Math.Pow((mainMoments[i] - currentMomments[i]), 2.0) / Math.Pow(mainMoments[i], 2.0);
            }

            return result;
        }

        public ImageSource calculateMomentResults(byte[,] point)
        {
            double[] mainMoments = calculateMoments(point);

            int x = this.GetOriginalWidth();
            int y = this.GetOriginalHeight();
            double[,] resultDouble = new double[y, x];
            byte[,] resultByte = new byte[y, x];
            byte[,] originalPixels = this.getTwoDimArray(this.getOriginalPixels(), x);
            double min = 0;
            double max = 0;

            for (int j = 5; j < (y - 5); j++)
            {
                for (int i = 5; i < (x - 5); i++)
                {
                    double[] currentMoments = calculateMoments(this.GetSome2DPixels(originalPixels, i - 7, j - 7, 9, 9));
                    double diff = calculateDiff(mainMoments, currentMoments);

                    if (diff > max) max = diff;

                    resultDouble[j, i] = diff;
                }
            }

            for (int j = 5; j < (y - 5); j++)
            {
                for (int i = 5; i < (x - 5); i++)
                {
                    resultByte[j, i] = (byte)(resultDouble[j, i] / max * 255.0);
                }
            }
            
            this.recognitionBitmap = new WriteableBitmap(this.originalBitmapImage);
            //return this.recognitionBitmap;
            return this.writePixels(this.recognitionBitmap, this.getOneDimArray(resultByte));
        }
    }
   
    public class Complex
    {
        public double a;
        public double b;

        public Complex()
        {
            this.a = 0;
            this.b = 0;
        }

        public Complex (double a, double b)
        {
            this.a = a;
            this.b = b;
        }
    }
}