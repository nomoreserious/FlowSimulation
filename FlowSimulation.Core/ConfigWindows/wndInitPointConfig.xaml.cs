using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace FlowSimulation.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для wndWayPointsConfig.xaml
    /// </summary>
    public partial class wndInitPointConfig : Window
    {
        private int MaxManPerMin = 0;
        private int[] _initPointDistribution;
        private bool _isDown;

        public List<int> InitPointDistribution 
        {
            get 
            {
                List<int> list = new List<int>(_initPointDistribution);
                return list;
            } 
        }

        public wndInitPointConfig(int max_per_min)
        {
            InitializeComponent();
            MaxManPerMin = max_per_min;
            pnlGraphic.MinHeight = MaxManPerMin;
            _initPointDistribution = new int[1440];
            //lvInitPointList.ItemsSource = null;
            //lvInitPointList.ItemsSource = _initPointDistribution;
            PaintGraphicPanel();
        }

        public wndInitPointConfig(int[] initPointDistribution,int max_per_min)
        {
            InitializeComponent();
            MaxManPerMin = max_per_min;
            pnlGraphic.MinHeight = MaxManPerMin;
            if (initPointDistribution.Length == 0)
            {
                _initPointDistribution = new int[1440];
            }
            else
            {
                _initPointDistribution = initPointDistribution;
            }
            //lvInitPointList.ItemsSource = null;
            //lvInitPointList.ItemsSource = _initPointDistribution;
            PaintGraphicPanel();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            double manPerMin = Math.Ceiling((pnlGraphic.ActualHeight - e.GetPosition(pnlGraphic).Y) * MaxManPerMin / pnlGraphic.ActualHeight);
            tbManPerMin.Text = manPerMin.ToString();
            double totalMin = Math.Ceiling(e.GetPosition(pnlGraphic).X * 1440 / pnlGraphic.ActualWidth);
            TimeSpan time = TimeSpan.FromMinutes(totalMin);
            tbTime.Text = (time.Hours < 10 ? "0" + time.Hours : "" + time.Hours) + ":" + (time.Minutes < 10 ? "0" + time.Minutes : "" + time.Minutes);
            if (_isDown && manPerMin >= 0)
            {
                int min = Convert.ToInt32(Math.Ceiling(totalMin));
                min = min < 1440 ? min : 1439;
                min = min >= 0 ? min : 0;
                _initPointDistribution[min] = Convert.ToInt32(manPerMin);
                Rectangle rect = new Rectangle()
                {
                    Fill = Brushes.BlueViolet,
                    Width = pnlGraphic.ActualWidth / 1440,
                    Height = (double)_initPointDistribution[min] / MaxManPerMin * pnlGraphic.ActualHeight,
                    Uid = min.ToString()
                };
                rect.SetValue(Canvas.LeftProperty, min * pnlGraphic.ActualWidth / 1440);
                rect.SetValue(Canvas.TopProperty, pnlGraphic.ActualHeight - rect.Height);
                for (int i = 0; i < pnlGraphic.Children.Count; i++)
                {
                    if (pnlGraphic.Children[i].Uid == rect.Uid)
                    {
                        pnlGraphic.Children.Remove(pnlGraphic.Children[i]);
                        break;
                    }
                }
                pnlGraphic.Children.Add(rect);
            }
        }

        private void PaintGraphicPanel()
        {
            pnlGraphic.Children.Clear();
            for (int i = 0; i < _initPointDistribution.Length; i++)
            {
                Rectangle rect = new Rectangle()
                {
                    Fill = Brushes.BlueViolet,
                    Width = pnlGraphic.ActualWidth / 1440,
                    Height = (double)_initPointDistribution[i] / MaxManPerMin * pnlGraphic.ActualHeight,
                    Uid = i.ToString()
                };
                rect.SetValue(Canvas.LeftProperty, i * pnlGraphic.ActualWidth / 1440);
                rect.SetValue(Canvas.TopProperty, pnlGraphic.ActualHeight - rect.Height);
                pnlGraphic.Children.Add(rect);
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            tbManPerMin.Text = "";
            tbTime.Text = "";
            _isDown = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content.ToString() == "Сохранить")
            {
                DialogResult = true;
                Close();
            }
            else
            {
                DialogResult = false;
                Close();
            }
        }

        private void pnlMap_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDown = true;
            pnlGraphic.CaptureMouse();

            double manPerMin = Math.Ceiling((pnlGraphic.ActualHeight - e.GetPosition(pnlGraphic).Y) * MaxManPerMin / pnlGraphic.ActualHeight);
            double totalMin = Math.Ceiling(e.GetPosition(pnlGraphic).X * 1440 / pnlGraphic.ActualWidth);
            int min = Convert.ToInt32(Math.Ceiling(totalMin));
            min = min < 1440 ? min : 1439;
            _initPointDistribution[min] = Convert.ToInt32(manPerMin);
            Rectangle rect = new Rectangle()
            {
                Fill = Brushes.BlueViolet,
                Width = pnlGraphic.ActualWidth / 1440,
                Height = (double)_initPointDistribution[min] / MaxManPerMin * pnlGraphic.ActualHeight,
                Uid = min.ToString()
            };
            rect.SetValue(Canvas.LeftProperty, min * pnlGraphic.ActualWidth / 1440);
            rect.SetValue(Canvas.TopProperty, pnlGraphic.ActualHeight - rect.Height);
            for (int i = 0; i < pnlGraphic.Children.Count; i++)
            {
                if (pnlGraphic.Children[i].Uid == rect.Uid)
                {
                    pnlGraphic.Children.Remove(pnlGraphic.Children[i]);
                    break;
                }
            }
            pnlGraphic.Children.Add(rect);
            e.Handled = true;
        }

        private void pnlMap_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDown)
            {
                System.Windows.Input.Mouse.Capture(null);
                _isDown = false;
                e.Handled = true;
            }
        }

        private void pnlGraphic_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PaintGraphicPanel();
        }
    }
}
