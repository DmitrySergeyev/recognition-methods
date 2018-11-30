using Microsoft.Win32;
using OxyPlot;
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
                    break;

                case "3. Filtration":
                  //   this.setImageSource(this.filteredImage, this.imageProcessor.GetFilteredImage(Convert.ToInt32(this.filter.Value)));
                    break;

                case "Item3":
                    break;

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
            if(this.showFilteredImageBtn != null)
            {
                this.showFilteredImageBtn.Content = "Filtering (" + Convert.ToString(this.filterSlider.Value) + ")";
            }
        }
    }
}