using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Windows.Threading;
using FlowSimulation.Scenario;
using FlowSimulation.Scenario.IO;

namespace FlowSimulation.Analisis
{
    /// <summary>
    /// Логика взаимодействия для wndSpectralDensity.xaml
    /// </summary>
    public partial class wndSpectralDensity : Window
    {
        private ushort[,] passengerDensity;
        private double width, height;
        private List<PaintObject> paintObjectList;

        public wndSpectralDensity(ushort[,] pd, List<PaintObject> pol)
        {
            this.width = pd.GetLength(0);
            this.height = pd.GetLength(1);
            this.paintObjectList = pol;
            this.passengerDensity = pd;

            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            InitializeComponent();

            pnlPaint.Width = width;
            pnlPaint.Height = height;

            PaintMap();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Paint();
        }

        private void Paint()
        {
            pnlPaint.Background = GetSpectorImageBrush();
        }

        private void PaintMap()
        {
            pnlPaint.Children.Clear();
            for (int n = 0; n < paintObjectList.Count; n++)
            {
                PaintObject obj = paintObjectList[n];
                if (obj.GetName() == "path")
                {
                    string data = obj.GetAttributeValue("data");
                    System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
                    path.Data = Geometry.Parse(data);
                    SVSObjectStyle style = obj.GetStyle();
                    path.Fill = new SolidColorBrush(Color.FromArgb(style.FillColor.A, style.FillColor.R, style.FillColor.G, style.FillColor.B));
                    path.Stroke = new SolidColorBrush(Color.FromArgb(style.BorderColor.A, style.BorderColor.R, style.BorderColor.G, style.BorderColor.B));
                    path.StrokeThickness = (double)style.BorderSize;
                    System.Drawing.Drawing2D.Matrix m = obj.GetTransformMatrix();
                    Matrix matrix = new Matrix((double)m.Elements[0], (double)m.Elements[1], (double)m.Elements[2], (double)m.Elements[3], (double)m.OffsetX, (double)m.OffsetY);
                    path.RenderTransform = new MatrixTransform(matrix);
                    pnlPaint.Children.Add(path);
                }
                if (obj.GetName() == "rect")
                {
                    double x = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("x"));
                    double y = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("y"));
                    double w = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("width"));
                    double h = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("height"));
                    SVSObjectStyle style = obj.GetStyle();

                    Rectangle rect = new Rectangle();
                    rect.SetValue(Canvas.LeftProperty, x);
                    rect.SetValue(Canvas.TopProperty, y);
                    rect.Width = w;
                    rect.Height = h;
                    rect.Fill = new SolidColorBrush(Color.FromArgb(style.FillColor.A, style.FillColor.R, style.FillColor.G, style.FillColor.B));
                    rect.Stroke = new SolidColorBrush(Color.FromArgb(style.BorderColor.A, style.BorderColor.R, style.BorderColor.G, style.BorderColor.B));
                    rect.StrokeThickness = (double)style.BorderSize;
                    System.Drawing.Drawing2D.Matrix m = obj.GetTransformMatrix();
                    Matrix matrix = new Matrix((double)m.Elements[0], (double)m.Elements[1], (double)m.Elements[2], (double)m.Elements[3], (double)m.OffsetX, (double)m.OffsetY);
                    rect.RenderTransform = new MatrixTransform(matrix);
                    pnlPaint.Children.Add(rect);
                }
            }
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }

        private System.Windows.Media.ImageBrush GetSpectorImageBrush()
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(passengerDensity.GetLength(0), passengerDensity.GetLength(1));
            int max = 0;
            for (int i = 0; i < passengerDensity.GetLength(0); i++)
            {
                for (int j = 0; j < passengerDensity.GetLength(1); j++)
                {
                    if (max < passengerDensity[i, j])
                    {
                        max = passengerDensity[i, j];
                    }
                }
            }

            for (int i = 0; i < passengerDensity.GetLength(0); i++)
            {
                for (int j = 0; j < passengerDensity.GetLength(1); j++)
                {
                    double value = (double)passengerDensity[i, j] / max;
                    if (value < 0.2)
                    {
                        bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(128, 255 - Convert.ToInt32(255 * (value == 0 ? value : value + 0.3)), 255, 255 - Convert.ToInt32(255 * (value == 0 ? value : value + 0.3))));
                    }
                    else if (value < 0.6)
                    {
                        bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(128, 255 - Convert.ToInt32(255 * (value + 0.1)), 255 - Convert.ToInt32(255 * (value + 0.1)), 255));
                    }
                    else
                    {
                        bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(128, 255, 255 - Convert.ToInt32(255 * value), 255 - Convert.ToInt32(255 * value)));
                    }
                }
            }
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            ms.Position = 0;
            System.Windows.Media.Imaging.BitmapImage image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.StreamSource = ms;
            image.EndInit();
            return new System.Windows.Media.ImageBrush(image);
        }
    }
}
