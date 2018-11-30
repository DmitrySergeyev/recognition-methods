using LiveCharts;
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
using System.Windows.Shapes;

namespace WpfRecognitionMethodsApp
{
    public partial class PlotWindow : Window
    {
        private ImageProcessor imageProcessor;

        public SeriesCollection OriginalHistogramSeries { get; set; }
        public string[] OriginalHistogramLabels { get; set; }
        public Func<double, string> OriginalHistogramFormatter { get; set; }

        public SeriesCollection EqualisedHistogramSeries { get; set; }
        public string[] EqualisedHistogramLabels { get; set; }
        public Func<double, string> EqualisedHistogramFormatter { get; set; }

        public SeriesCollection TransformFunctionSeries { get; set; }
        public string[] TransformFunctionLabels { get; set; }
        public Func<double, string> TransformFunctionFormatter { get; set; }

        private PlotWindow()
        {
            InitializeComponent();
        }

        public PlotWindow (ImageProcessor imageProsessor) : this()
        {
            this.imageProcessor = imageProsessor;
            string[] labels = new string[256];

            for (int i = 0; i < 256; i++)
            {
                labels[i] = Convert.ToString(i);
            }

            // 1 - Original image histogram plot
            this.OriginalHistogramSeries = new SeriesCollection
            {
                new LineSeries
                {                    
                    Values = new ChartValues<float>(this.imageProcessor.GetOriginalImageHistogram())
                }
            };

            this.OriginalHistogramLabels = labels;

            // 2 - Equalised image histogram plot
            this.EqualisedHistogramSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<float>(this.imageProcessor.GetEqualisedImageHistogram())
                }
            };

            this.EqualisedHistogramLabels = labels;

            // 3 - Transformation function
            this.TransformFunctionSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<float>(this.imageProcessor.GetTransformFunction())
                }
            };

            this.TransformFunctionLabels = labels;

            DataContext = this;
        }
    }
}
