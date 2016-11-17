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


namespace FlowSimulation.Analisis
{
    /// <summary>
    /// Логика взаимодействия для wndGraphic.xaml
    /// </summary>
    public partial class wndGraphic : Window
    {
        private DataTable data;
        private double max;

        public DataTable Data
        {
            get { return data; }
            set
            {
                data = value;
            }
        }
    
        public wndGraphic()
        {
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            InitializeComponent();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Paint();
        }

        private void Paint()
        {
            pnlPaint.Children.Clear();
            max = 0;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                for (int j = 1; j < data.Columns.Count; j++)
                {
                    if (max < Convert.ToDouble(data.Rows[i][j]))
                    {
                        max = Convert.ToDouble(data.Rows[i][j]);
                    }
                }
            }

            double val = 10000000000;
            while (max < val)
            {
                val /= 10;
            }
            double scope = val / 10;
            while (max > val)
            {
                val += scope;
            }
            max = val;

            double x = 0, y = 0;
            int count = max > 1 && max < 10 ? (int)max : 10;
            for (int i = 0; i < count + 1; i++)
            {
                Line line = new Line()
                {
                    X1 = 50,
                    Y1 = 25 + (pnlPaint.ActualHeight - 175) - (pnlPaint.ActualHeight - 175) * i / count,
                    X2 = (pnlPaint.ActualWidth - 60) / data.Rows.Count * (data.Rows.Count - 1) + 50,
                    Y2 = 25 + (pnlPaint.ActualHeight - 175) - (pnlPaint.ActualHeight - 175) * i / count,
                    Fill = Brushes.LightGray,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1.0
                };
                pnlPaint.Children.Add(line);
                TextBlock tb = new TextBlock()
                {
                    Text = (max == Math.Round(max) ? Math.Round(max * i / count) : Math.Round(max * i / count, 5)).ToString(),
                    FontSize = 10,
                    FontFamily = new System.Windows.Media.FontFamily("Tahoma")
                };
                tb.SetValue(Canvas.LeftProperty, 20.0D);
                tb.SetValue(Canvas.TopProperty, 25.0D + (pnlPaint.ActualHeight - 175) - (pnlPaint.ActualHeight - 175) * i / count - 5);
                pnlPaint.Children.Add(tb);
                TextBlock tb1 = new TextBlock()
                {
                    Text = (max == Math.Round(max) ? Math.Round(max * i / count) : Math.Round(max * i / count, 5)).ToString(),
                    FontSize = 10,
                    FontFamily = new System.Windows.Media.FontFamily("Tahoma")
                };
                tb1.SetValue(Canvas.LeftProperty, 60.0D + (pnlPaint.ActualWidth - 60) / data.Rows.Count * (data.Rows.Count - 1));
                tb1.SetValue(Canvas.TopProperty, 25.0D + (pnlPaint.ActualHeight - 175) - (pnlPaint.ActualHeight - 175) * i / count - 5);
                pnlPaint.Children.Add(tb1);
            }
            pnlPaint.Width = data.Rows.Count * 15 + 100;
            //Console.WriteLine("max: " + max);
            for (int i = 0; i < data.Rows.Count; i++)
            {
                for (int j = 1; j < data.Columns.Count; j++)
                {
                    Color color = GetColorByIndex(j);
                    double value = Convert.ToDouble(data.Rows[i][j]);
                    //Console.WriteLine(i + ": " + value);

                    x = 50 + i * 15;
                    y = 25 + (pnlPaint.ActualHeight - 175) - (pnlPaint.ActualHeight - 175) * value / max;

                    if (i % 5 == 0 && j == 1)
                    {
                        Line line = new Line()
                        {
                            X1 = x,
                            Y1 = 25,
                            X2 = x,
                            Y2 = pnlPaint.ActualHeight - 150,
                            Fill = Brushes.LightGray,
                            Stroke = Brushes.LightGray,
                            StrokeThickness = 1.0
                        };
                        pnlPaint.Children.Add(line);

                        TextBlock tb = new TextBlock()
                        {
                            Text = data.Rows[i][0].ToString(),
                            FontSize = 12,
                            FontWeight = FontWeights.Bold,
                            FontFamily = new System.Windows.Media.FontFamily("Tahoma")
                        };

                        tb.SetValue(Canvas.LeftProperty, x);
                        tb.SetValue(Canvas.TopProperty, pnlPaint.ActualHeight - 140);
                        tb.RenderTransform = new RotateTransform(45);
                        pnlPaint.Children.Add(tb);
                    }

                    if (i != 0)
                    {
                        Line line = new Line()
                        {
                            X1 = 50 + (i - 1) * 15,
                            Y1 = 25 + (pnlPaint.ActualHeight - 175) - (pnlPaint.ActualHeight - 175) * Convert.ToDouble(data.Rows[i - 1][j]) / max,
                            X2 = x,
                            Y2 = y,
                            Fill = AgentVisualBase.GetGroupColor(j - 1),
                            Stroke = AgentVisualBase.GetGroupColor(j - 1),
                            StrokeThickness = 3.0
                        };
                        pnlPaint.Children.Add(line);
                    }
                    Ellipse el = new Ellipse()
                    {
                        Fill = AgentVisualBase.GetGroupColor(j - 1),
                        Stroke = AgentVisualBase.GetGroupColor(j - 1),
                        Width = 6,
                        Height = 6
                    };
                    el.SetValue(Canvas.LeftProperty, x - 3);
                    el.SetValue(Canvas.TopProperty, y - 3);
                    pnlPaint.Children.Add(el);
                }
            }
        }
        private Color GetColorByScema(int scema, int index, int all)
        {
            int r, g, b;
            switch (scema)
            {
                case 1:
                    r = 225 / all * index + 30;
                    g = 225 / all * index + 30;
                    b = 225 / all * index + 30;
                    return Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
                case 2:
                    r = 225 / all * index + 30;
                    g = 225 / all * index + 30;
                    b = 255;
                    return Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
                case 3:
                    r = 225 / all * index + 30;
                    g = 255;
                    b = 225 / all * index + 30;
                    return Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
                case 4:
                    r = 255;
                    g = 225 / all * index + 30;
                    b = 225 / all * index + 30;
                    return Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
                case 5:
                    r = 255;
                    g = 225 / all * index + 30;
                    b = 255;
                    return Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
                case 6:
                    r = 255;
                    g = 255;
                    b = 225 / all * index + 30;
                    return Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
                case 7:
                    r = 225 / all * index + 30;
                    g = 255;
                    b = 255;
                    return Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
                default:
                    return GetColorByIndex(index);
            }
        }

        private Color GetColorByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        return Colors.Green;
                    }
                case 1:
                    {
                        return Colors.Red;
                    }
                case 2:
                    {
                        return Colors.SkyBlue;
                    }
                case 3:
                    {
                        return Colors.Yellow;
                    }
                case 4:
                    {
                        return Colors.Magenta;
                    }
                case 5:
                    {
                        return Colors.Orange;
                    }
                case 6:
                    {
                        return Colors.YellowGreen;
                    }
                case 7:
                    {
                        return Colors.Blue;
                    }
                case 8:
                    {
                        return Colors.LightSteelBlue;
                    }
                case 9:
                    {
                        return Colors.DarkGray;
                    }
                case 10:
                    {
                        return Colors.Magenta;
                    }
                case 11:
                    {
                        return Colors.Brown;
                    }
                case 12:
                    {
                        return Colors.Linen;
                    }
                case 13:
                    {
                        return Colors.DodgerBlue;
                    }
                case 14:
                    {
                        return Colors.DarkSlateBlue;
                    }
                case 15:
                    {
                        return Colors.ForestGreen;
                    }
                case 16:
                    {
                        return Colors.DeepSkyBlue;
                    }
                case 17:
                    {
                        return Colors.DarkRed;
                    }
                case 18:
                    {
                        return Colors.LemonChiffon;
                    }
                case 19:
                    {
                        return Colors.Gold;
                    }
                case 20:
                    {
                        return Colors.Green;
                    }
                case 21:
                    {
                        return Colors.MediumTurquoise;
                    }
                case 22:
                    {
                        return Colors.HotPink;
                    }
                case 23:
                    {
                        return Colors.IndianRed;
                    }
                case 24:
                    {
                        return Colors.PaleGreen;
                    }
                case 25:
                    {
                        return Colors.Khaki;
                    }
                case 26:
                    {
                        return Colors.Brown;
                    }
                case 27:
                    {
                        return Colors.LawnGreen;
                    }
                case 28:
                    {
                        return Colors.LightBlue;
                    }
                case 29:
                    {
                        return Colors.LightCoral;
                    }
                case 30:
                    {
                        return Colors.LightSeaGreen;
                    }
                case 31:
                    {
                        return Colors.PaleTurquoise;
                    }
                case 32:
                    {
                        return Colors.MediumAquamarine;
                    }
                case 33:
                    {
                        return Colors.PaleGoldenrod;
                    }
                case 34:
                    {
                        return Colors.MediumSlateBlue;
                    }
                case 35:
                    {
                        return Colors.Peru;
                    }
                case 36:
                    {
                        return Colors.Moccasin;
                    }
                case 37:
                    {
                        return Colors.Tan;
                    }
                case 38:
                    {
                        return Colors.OldLace;
                    }
                case 39:
                    {
                        return Colors.Olive;
                    }
                case 40:
                    {
                        return Colors.Orchid;
                    }
                default:
                    {
                        int r, g, b;
                        Random rand = new Random(31745671);
                        r = rand.Next(255);
                        rand = new Random(97654173);
                        g = rand.Next(255);
                        rand = new Random(43461529);
                        b = rand.Next(255);
                        return Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
                    }
            }
        }
    }
}
