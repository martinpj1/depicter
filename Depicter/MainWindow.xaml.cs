using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CheckBox = System.Windows.Controls.CheckBox;
using static Depicter.Utils;

namespace Depicter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region fields
        List<Color> AllDepicterColors = new List<Color>()
        {
            Color.FromRgb(0,0,0),
            Color.FromRgb(87,87,87),
            Color.FromRgb(160,160,160),
            Color.FromRgb(156,39,176),
            Color.FromRgb(157,175,255),
            Color.FromRgb(42,75,215),

            Color.FromRgb(41,208,208),
            Color.FromRgb(129,197,122),
            Color.FromRgb(76,175,80),
            Color.FromRgb(198,255,0),
            Color.FromRgb(255,238,51),
            Color.FromRgb(255,146,51),

            Color.FromRgb(233,222,187),
            Color.FromRgb(129,74,25),
            Color.FromRgb(248,187,208),
            Color.FromRgb(244,67,54),
            Color.FromRgb(173,35,35),
            Color.FromRgb(255,255,255)
        };

        List<Color> UsedDepicterColors = new List<Color>();
        #endregion
        public MainWindow()
        {
            InitializeComponent();

            Style depicterStyle = (Style)this.Resources["DepicterCheckBoxStyle"];

            foreach (var color in AllDepicterColors)
            {
                var checkbox = new CheckBox()
                {
                    Style = depicterStyle,
                    Background = new SolidColorBrush(color),
                    IsChecked = true
                };
                checkbox.Checked += Checkbox_Toggled;
                checkbox.Unchecked += Checkbox_Toggled;

                wrapPanel.Children.Add(checkbox);

                UsedDepicterColors.Add(color);
            }

        }

        #region events
        private void CircleBtn_Click(object sender, RoutedEventArgs e)
        {
            DrawCircle();
        }

        private void Checkbox_Toggled(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var color = (checkbox.Background as SolidColorBrush).Color;

            if (checkbox?.IsChecked ?? false)
            {
                UsedDepicterColors.Add(color);
            }
            else
            {
                UsedDepicterColors.Remove(color);
            }
        }

        private void URLBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var image = new BitmapImage(new Uri(urlTextBox.Text, UriKind.Absolute));
                image.DownloadCompleted += OnImageDownloadCompleted;
                Console.WriteLine("finihsed!");
            }
            catch (Exception ex)
            {
                urlTextBox.Text = "";
            }
        }

        private void OnImageDownloadCompleted(object sender, EventArgs e)
        {
            var image = sender as BitmapImage;

            BitmapSource bitmapSource = new FormatConvertedBitmap(image, PixelFormats.Pbgra32, null, 0);
            WriteableBitmap modifiedImage = new WriteableBitmap(bitmapSource);

            int h = modifiedImage.PixelHeight;
            int w = modifiedImage.PixelWidth;
            uint[] pixelData = new uint[w * h];
            int widthInByte = 4 * w;

            modifiedImage.CopyPixels(pixelData, widthInByte, 0);

            for (int i = 0; i < pixelData.Length; i++)
            {
                byte alpha = (byte)((pixelData[i] & 0xff000000) >> 24);
                byte red = (byte)((pixelData[i] & 0x00ff0000) >> 16);
                byte green = (byte)((pixelData[i] & 0x0000ff00) >> 8);
                byte blue = (byte)(pixelData[i] & 0x000000ff);

                var pixelColor = Color.FromRgb(red, green, blue);

                var depicterColor = Colors.White;
                int minimum = int.MaxValue;

                foreach (var color in AllDepicterColors)
                {
                    int distance = colorDistance(pixelColor, color);
                    if (distance < minimum)
                    {
                        depicterColor = color;
                        minimum = distance;
                    }
                }

                uint convertedColor = 0xff000000; // initialize with no transparency
                convertedColor += (uint)(depicterColor.R << 16);
                convertedColor += (uint)(depicterColor.G << 8);
                convertedColor += depicterColor.B;
                pixelData[i] = convertedColor;
            }

            modifiedImage.WritePixels(new Int32Rect(0, 0, w, h), pixelData, widthInByte, 0);


            drawingPreview.Source = modifiedImage;
            originalImage.Source = image;
        }
        #endregion

        private void DrawCircle()
        {
            // move to center of screen
            int centerX = 65535 / 2;
            int centerY = 65535 / 2;
            int radius = (int)(65535 * 0.05);
            VirtualMouse.MoveTo(centerX + radius, centerY);

            int numSteps = 50;
            double stepSize = Math.PI * 2 / numSteps;

            VirtualMouse.LeftDown();

            double theta = 0;

            double screenWidth = Screen.PrimaryScreen.Bounds.Width;
            double screenHeight = Screen.PrimaryScreen.Bounds.Height;

            double yScaleFactor = screenWidth / screenHeight;

            for (int i = 0; i < numSteps; i++)
            {
                int x = (int)(centerX + radius * Math.Cos(theta));
                int y = (int)(centerY + yScaleFactor * radius * Math.Sin(theta));
                VirtualMouse.MoveTo(x, y);

                theta += stepSize;
                System.Threading.Thread.Sleep(5);
            }

            VirtualMouse.LeftUp();
        }
    }
}
