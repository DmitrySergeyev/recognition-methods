using LiveCharts;
using Microsoft.Win32;
using LiveCharts.Wpf;
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
using System.Windows.Shapes;

namespace WpfRecognitionMethodsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog dlg;
        private string path;
        private ImageProcessor imageProcessor;

        private PlotWindow plotWindow;

        public SeriesCollection Pisels1DSeries { get; set; }
        public string[] Pixels1DLabels { get; set; }
        public Func<double, string> Pixels1DFormatter { get; set; }

        public SeriesCollection Pixels1DFourierSeries { get; set; }
        public string[] Pixels1DFourierLabels { get; set; }
        public Func<double, string> Pixels1DFourierFormatter { get; set; }

        private int dimention = 64;

        public MainWindow()
        {
            InitializeComponent();

            this.dlg = new OpenFileDialog();
            this.dlg.DefaultExt = ".bmp";
            this.dlg.Filter = "Bitmap (.bmp)|*.bmp";
            this.dlg.Title = "Select an image for processing";
        }

        private void setImageSource(Image image, ImageSource source)
        {
            image.Height = this.imageProcessor.GetOriginalHeight();
            image.Width = this.imageProcessor.GetOriginalWidth();
            image.Source = source;
        }

        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            if (dlg.ShowDialog() == true)
            {
                this.path = dlg.FileName;
                this.filePathTextBox.Text = this.path;
                this.imageProcessor = new ImageProcessor(this.path);
                this.setImageSource(this.originalImage, this.imageProcessor.GetOriginalImage());
                this.setImageSource(this.binaryImage, this.imageProcessor.GetBinaryBitmapImage());
                this.dataGrid.IsEnabled = true;
            }
        }

        private void showPLotsButton_Click(object sender, RoutedEventArgs e)
        {
            this.plotWindow = new PlotWindow(this.imageProcessor);
            this.plotWindow.Show();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Header as string;

            switch (tabItem)
            {
                case "2. Equalisation":
                    this.setImageSource(this.equalisedImage, this.imageProcessor.GetEqualisedBitmapImage());
                    DataContext = this;
                    break;

                case "3. Filtration":
                    //   this.setImageSource(this.filteredImage, this.imageProcessor.GetFilteredImage(Convert.ToInt32(this.filter.Value)));
                    break;

                case "4. One dimentional Fourier":

                    int[] pixels1D = this.imageProcessor.GetSomePixels(0, 0, this.dimention);

                    this.Pisels1DSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Values = new ChartValues<int>(pixels1D)
                        }
                    };

                    string[] labels = new string[pixels1D.Length];

                    this.Pixels1DFourierSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Values = new ChartValues<double>(this.imageProcessor.GetOneDimentuionalFourier(pixels1D))
                        }
                    };

                    for (int i = 0; i < labels.Length; i++)
                    {
                        labels[i] = Convert.ToString(i);
                    }

                    this.Pixels1DLabels = labels;

                    DataContext = this;

                    break;

                case "5. Two dimentional Fourier":
                    this.image2DSeries.Source = this.imageProcessor.GetSubImage(0, 0, this.dimention, this.dimention);
                    DataContext = this;
                    break;

                //case "6. Skeletisation":
                //    this.setImageSource(this.skeletImage, this.imageProcessor.GetSkeletBitmapImage());
                //    DataContext = this;
                //    break;

                default:
                    return;
            }
        }

        private void filteringButton_Click(object sender, RoutedEventArgs e)
        {
            this.setImageSource(this.filteredImage, this.imageProcessor.GetFilteredImage(Convert.ToInt32(this.filterSlider.Value)));
        }

        private void filter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.showFilteredImageBtn != null)
            {
                this.showFilteredImageBtn.Content = "Filtering (" + Convert.ToString(this.filterSlider.Value) + ")";
            }
        }

        private void originalImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string tabItem = (this.modifiedTabItem.SelectedItem as TabItem).Header as string;

            Point point = e.GetPosition(this.originalImage);
            int x = Convert.ToInt32(point.X);
            int y = Convert.ToInt32(point.Y);

            switch (tabItem)
            {
                case "4. One dimentional Fourier":
                    //MessageBox.Show(e.GetPosition(this.originalImage).ToString());

                    int[] pixels = this.imageProcessor.GetSomePixels(x, y, 64);
                    this.Pisels1DSeries.Clear();
                    this.Pixels1DFourierSeries.Clear();

                    this.Pisels1DSeries.AddRange(new SeriesCollection
                            {
                                new ColumnSeries
                                {
                                    Values = new ChartValues<int>(pixels)
                                }
                            }
                    );

                    string[] labels = new string[pixels.Length];

                    this.Pixels1DFourierSeries.AddRange(new SeriesCollection
                            {
                                new ColumnSeries
                                {
                                    Values = new ChartValues<double>(this.imageProcessor.GetOneDimentuionalFourier(pixels))
                                }
                            }
                    );

                    for (int i = 0; i < labels.Length; i++)
                    {
                        labels[i] = Convert.ToString(i);
                    }

                    this.Pixels1DLabels = labels;

                    DataContext = this;

                    break;

                case "5. Two dimentional Fourier":

                    this.image2DSeries.Source = this.imageProcessor.GetSubImage(x, y, this.dimention, this.dimention);
                    DataContext = this;

                    break;

                default:
                    return;
            }
        }

        private void calculateBack2DFourier_Click(object sender, RoutedEventArgs e)
        {
            this.imageBack2DSeries.Source = this.imageProcessor.GetTwoDimentuionalBackFourierImage();
            DataContext = this;
        }

        private void calculate2DFourier_Click_1(object sender, RoutedEventArgs e)
        {
            this.image2DFourierSeries.Source = this.imageProcessor.GetTwoDimentuionalFourierImage();
            DataContext = this;
        }

        private void calculateSkelet_Click_1(object sender, RoutedEventArgs e)
        {
            this.skeletImage.Source = this.imageProcessor.GetSkeletBitmapImage();
            this.imageProcessor.writePixels(this.imageProcessor.GetOriginalImage(), this.imageProcessor.getSkeletBitmapImagePixels());

            DataContext = this;
        }
    }
}