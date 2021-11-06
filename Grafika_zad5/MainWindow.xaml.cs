using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace Grafika_zad5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileDialog(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                imgSource.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void ClearSourceHistogram()
        {
            sourceHistogram.Children.Clear();
        }

        private void ClearResultHistogram()
        {
            resultHistogram.Children.Clear();
        }

        private bool IsInputCorrect()
        {
            if (!Int32.TryParse(inputTreshhold.Text, out int result))
            {
                MessageBox.Show("Podane dane są niepoprawne.", "Niepoprawne dane", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (result < 0 || result > 255)
            {
                MessageBox.Show("Podany zakres powinien być od 0 do 255.", "Zły zakres", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        // Dla czarnobialych.
        private void DrawSourceHistogram(byte[] pixelBuffer)
        {
            int[] colorPixels = new int[256];
            for (int i = 0; i + 4< pixelBuffer.Length; i+= 4)
            {
                colorPixels[pixelBuffer[i]]++;
            }
            for (int i = 0; i < colorPixels.Length; i++)
            {
                System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle();
                r.Width = 1;
                r.Height = colorPixels[i] / 5;
                r.Fill = new SolidColorBrush(Colors.Black);
                r.VerticalAlignment = VerticalAlignment.Bottom;
                sourceHistogram.Children.Add(r);
            }
        }

        // Dla czarnobialych.
        private void DrawResultHistogram(byte[] pixelBuffer)
        {
            int[] colorPixels = new int[256];
            for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
            {
                colorPixels[pixelBuffer[i]]++;
            }
            for (int i = 0; i < colorPixels.Length; i++)
            {
                System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle();
                r.Width = 1;
                r.Height = colorPixels[i] / 5;
                r.Fill = new SolidColorBrush(Colors.Black);
                r.VerticalAlignment = VerticalAlignment.Bottom;
                resultHistogram.Children.Add(r);
            }
        }

        private bool ImageExist()
        {
            return imgSource.Source == null ? false : true;
        }

        private void ExtendHistogram(object sender, RoutedEventArgs e)
        {
            if (!ImageExist())
            {
                MessageBox.Show("Nie załadowano obrazka!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Zrodlo.
            Bitmap imgSourceBitmap = ConvertImgToBitmap(imgSource);
            BitmapData sourceBitmapData = imgSourceBitmap.LockBits(new Rectangle(0, 0, imgSourceBitmap.Width, imgSourceBitmap.Height),
                                                             ImageLockMode.ReadOnly,
                                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceBitmapData.Stride * sourceBitmapData.Height];
            Marshal.Copy(sourceBitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            imgSourceBitmap.UnlockBits(sourceBitmapData);

            byte maxValue = 0;
            byte minValue = 255;
            ClearSourceHistogram();
            ClearResultHistogram();
            DrawSourceHistogram(pixelBuffer);
            // Dla czarno-bialego obrazka.
            for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
            {
                if (pixelBuffer[i] > maxValue)
                {
                    maxValue = pixelBuffer[i];
                }
                else if (pixelBuffer[i] < minValue)
                {
                    minValue = pixelBuffer[i];
                }
            }
            for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
            {
                double result = (pixelBuffer[i] - minValue) * (255.0 / (maxValue - minValue));
                result = Math.Round(result, 0);
                pixelBuffer[i] = Convert.ToByte(result);
                pixelBuffer[i + 1] = Convert.ToByte(result);
                pixelBuffer[i + 2] = Convert.ToByte(result);
            }
            DrawResultHistogram(pixelBuffer);

            // Rezultat.
            Bitmap imgResultBitmap = new Bitmap(imgSourceBitmap.Width, imgSourceBitmap.Height);
            BitmapData resultBitmapData = imgResultBitmap.LockBits(new Rectangle(0, 0, imgResultBitmap.Width, imgResultBitmap.Height),
                                                                   ImageLockMode.WriteOnly,
                                                                   System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Marshal.Copy(pixelBuffer, 0, resultBitmapData.Scan0, pixelBuffer.Length);
            imgResultBitmap.UnlockBits(resultBitmapData);
            imgResult.Source = ConvertBitmapToImageSource(imgResultBitmap);
        }

        private void EqualizationHistogram(object sender, RoutedEventArgs e)
        {
            if (!ImageExist())
            {
                MessageBox.Show("Nie załadowano obrazka!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Zrodlo.
            Bitmap imgSourceBitmap = ConvertImgToBitmap(imgSource);
            BitmapData sourceBitmapData = imgSourceBitmap.LockBits(new Rectangle(0, 0, imgSourceBitmap.Width, imgSourceBitmap.Height),
                                                             ImageLockMode.ReadOnly,
                                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceBitmapData.Stride * sourceBitmapData.Height];
            Marshal.Copy(sourceBitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            imgSourceBitmap.UnlockBits(sourceBitmapData);

            ClearSourceHistogram();
            ClearResultHistogram();
            DrawSourceHistogram(pixelBuffer);
            // Dla czarno-bialego obrazka.
            int[] colorPixels = new int[256];
            int numberOfPixels = 0;
            for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
            {
                colorPixels[pixelBuffer[i]]++;
                numberOfPixels++;
            }
            double[] p = new double[256];
            for (int i = 0; i < colorPixels.Length; i++)
            {
                p[i] = colorPixels[i] / (double)numberOfPixels;
            }
            for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
            {
                double sum = 0;
                for (byte j = 0; j < pixelBuffer[i]; j++)
                {
                    sum += p[j];
                }
                double result = Math.Floor(255 * sum);
                pixelBuffer[i] = Convert.ToByte(result);
                pixelBuffer[i + 1] = Convert.ToByte(result);
                pixelBuffer[i + 2] = Convert.ToByte(result);
            }
            DrawResultHistogram(pixelBuffer);

            // Rezultat.
            Bitmap imgResultBitmap = new Bitmap(imgSourceBitmap.Width, imgSourceBitmap.Height);
            BitmapData resultBitmapData = imgResultBitmap.LockBits(new Rectangle(0, 0, imgResultBitmap.Width, imgResultBitmap.Height),
                                                                   ImageLockMode.WriteOnly,
                                                                   System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Marshal.Copy(pixelBuffer, 0, resultBitmapData.Scan0, pixelBuffer.Length);
            imgResultBitmap.UnlockBits(resultBitmapData);
            imgResult.Source = ConvertBitmapToImageSource(imgResultBitmap);
        }

        private void CustomBinarization(object sender, RoutedEventArgs e)
        {
            if (!ImageExist())
            {
                MessageBox.Show("Nie załadowano obrazka!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsInputCorrect())
            {
                return;
            }

            // Zrodlo.
            Bitmap imgSourceBitmap = ConvertImgToBitmap(imgSource);
            BitmapData sourceBitmapData = imgSourceBitmap.LockBits(new Rectangle(0, 0, imgSourceBitmap.Width, imgSourceBitmap.Height),
                                                             ImageLockMode.ReadOnly,
                                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceBitmapData.Stride * sourceBitmapData.Height];
            Marshal.Copy(sourceBitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            imgSourceBitmap.UnlockBits(sourceBitmapData);

            ClearSourceHistogram();
            ClearResultHistogram();
            byte treshold = Convert.ToByte(inputTreshhold.Text);
            // Dla czarno-bialego obrazka.
            for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
            {
                if (pixelBuffer[i] >= treshold)
                {
                    pixelBuffer[i] = 255;
                    pixelBuffer[i + 1] = 255;
                    pixelBuffer[i + 2] = 255;
                }
                else
                {
                    pixelBuffer[i] = 0;
                    pixelBuffer[i + 1] = 0;
                    pixelBuffer[i + 2] = 0;
                }
            }

            // Rezultat.
            Bitmap imgResultBitmap = new Bitmap(imgSourceBitmap.Width, imgSourceBitmap.Height);
            BitmapData resultBitmapData = imgResultBitmap.LockBits(new Rectangle(0, 0, imgResultBitmap.Width, imgResultBitmap.Height),
                                                                   ImageLockMode.WriteOnly,
                                                                   System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Marshal.Copy(pixelBuffer, 0, resultBitmapData.Scan0, pixelBuffer.Length);
            imgResultBitmap.UnlockBits(resultBitmapData);
            imgResult.Source = ConvertBitmapToImageSource(imgResultBitmap);
        }

        private void IterativeBinarization(object sender, RoutedEventArgs e)
        {
            if (!ImageExist())
            {
                MessageBox.Show("Nie załadowano obrazka!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Zrodlo.
            Bitmap imgSourceBitmap = ConvertImgToBitmap(imgSource);
            BitmapData sourceBitmapData = imgSourceBitmap.LockBits(new Rectangle(0, 0, imgSourceBitmap.Width, imgSourceBitmap.Height),
                                                             ImageLockMode.ReadOnly,
                                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceBitmapData.Stride * sourceBitmapData.Height];
            Marshal.Copy(sourceBitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            imgSourceBitmap.UnlockBits(sourceBitmapData);

            ClearSourceHistogram();
            ClearResultHistogram();
            byte thirdByte = 0;
            byte lastByte = 0;
            int objectBytes = 0;
            int backgroundBytes = 0;
            int objectSum = 0;
            int backgroundSum = 0;
            // Dla czarno-bialego obrazka.
            // Pierwsza iteracja.
            for (int i = 4; i + 4 < pixelBuffer.Length; i += 4)
            {
                if (i + 4 > pixelBuffer.Length)
                {
                    lastByte = pixelBuffer[i];
                    thirdByte = pixelBuffer[i - sourceBitmapData.Stride + 4];
                }
                else
                {
                    objectSum += pixelBuffer[i];
                    objectBytes++;
                }
            }
            double averageObjectBrightness = objectSum / (double)objectBytes;
            double averageBackgroundBrightness = (pixelBuffer[0] + pixelBuffer[sourceBitmapData.Stride - 4] + thirdByte + lastByte) / 2.0;
            double treshold = (averageObjectBrightness + averageBackgroundBrightness) / 2.0;
            // Kolejne Iteracje.
            objectBytes = 0;
            backgroundBytes = 0;
            objectSum = 0;
            backgroundSum = 0;
            for (int n = 0; n < sliderIteration.Value; n++)
            {
                for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
                {
                    if (pixelBuffer[i] >= treshold)
                    {
                        objectBytes++;
                        objectSum += pixelBuffer[i];
                    }
                    else
                    {
                        backgroundBytes++;
                        backgroundSum += pixelBuffer[i];
                    }
                }
                averageObjectBrightness = objectSum / (double)objectBytes;
                averageBackgroundBrightness = backgroundSum / (double)backgroundBytes;
                treshold = (averageObjectBrightness + averageBackgroundBrightness) / 2.0;
            }
            // Binaryzacja.
            for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
            {
                if (pixelBuffer[i] >= treshold)
                {
                    pixelBuffer[i] = 255;
                    pixelBuffer[i + 1] = 255;
                    pixelBuffer[i + 2] = 255;
                }
                else
                {
                    pixelBuffer[i] = 0;
                    pixelBuffer[i + 1] = 0;
                    pixelBuffer[i + 2] = 0;
                }
            }

            // Rezultat.
            Bitmap imgResultBitmap = new Bitmap(imgSourceBitmap.Width, imgSourceBitmap.Height);
            BitmapData resultBitmapData = imgResultBitmap.LockBits(new Rectangle(0, 0, imgResultBitmap.Width, imgResultBitmap.Height),
                                                                   ImageLockMode.WriteOnly,
                                                                   System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Marshal.Copy(pixelBuffer, 0, resultBitmapData.Scan0, pixelBuffer.Length);
            imgResultBitmap.UnlockBits(resultBitmapData);
            imgResult.Source = ConvertBitmapToImageSource(imgResultBitmap);
        }

        private Bitmap ConvertImgToBitmap(System.Windows.Controls.Image source)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)source.ActualWidth, (int)source.ActualHeight, 96.0, 96.0, PixelFormats.Pbgra32);
            source.Measure(new System.Windows.Size((int)source.ActualWidth, (int)source.ActualHeight));
            source.Arrange(new Rect(new System.Windows.Size((int)source.ActualWidth, (int)source.ActualHeight)));
            renderTargetBitmap.Render(source);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            MemoryStream stream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            encoder.Save(stream);
            Bitmap bitmap = new Bitmap(stream);
            stream.Close();
            renderTargetBitmap.Clear();
            return bitmap;
        }

        private BitmapImage ConvertBitmapToImageSource(Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        
    }
}
