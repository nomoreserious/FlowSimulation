using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace FlowSimulation.View.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для wndWayPointsConfig.xaml
    /// </summary>
    public partial class AgentsVolumeConfig : MahApps.Metro.Controls.MetroWindow
    {

        public const int MAX_PER_INTERVAL = 600;
        public const int INTERVALS_IN_DAY = 144;
        //private int MaxManPerMin = 0;
        //private int[] _initPointDistribution;
        private bool _isDown;

        public List<DayOfWeekVolume> DaysOfWeekDistribution { get; private set; }

        private DayOfWeekVolume _selectedDay;
        public DayOfWeekVolume SelectedDay
        {
            get { return _selectedDay; }
            set
            {
                if (_selectedDay == value)
                    return;
                _selectedDay = value;
                PaintGraphicPanel();
            }
        }

        public AgentsVolumeConfig(Dictionary<DayOfWeek, int[]> disrtibution = null)
        {
            DaysOfWeekDistribution = new List<DayOfWeekVolume>();

            if (disrtibution == null || disrtibution.Count == 0)
            {
                DaysOfWeekDistribution.Add(new DayOfWeekVolume(DayOfWeek.Monday));
                DaysOfWeekDistribution.Add(new DayOfWeekVolume(DayOfWeek.Tuesday));
                DaysOfWeekDistribution.Add(new DayOfWeekVolume(DayOfWeek.Wednesday));
                DaysOfWeekDistribution.Add(new DayOfWeekVolume(DayOfWeek.Thursday));
                DaysOfWeekDistribution.Add(new DayOfWeekVolume(DayOfWeek.Friday));
                DaysOfWeekDistribution.Add(new DayOfWeekVolume(DayOfWeek.Saturday));
                DaysOfWeekDistribution.Add(new DayOfWeekVolume(DayOfWeek.Sunday));
            }
            else
            {
                foreach (var kv in disrtibution)
                {
                    DaysOfWeekDistribution.Add(new DayOfWeekVolume(kv.Key, kv.Value));
                }
            }
            DataContext = this;
            InitializeComponent();
            pnlGraphic.MinHeight = MAX_PER_INTERVAL;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_selectedDay == null)
                return;
            double manPerMin = Math.Ceiling((pnlGraphic.ActualHeight - e.GetPosition(pnlGraphic).Y) * MAX_PER_INTERVAL / pnlGraphic.ActualHeight);
            tbManPerMin.Text = manPerMin.ToString();
            double totalMin = Math.Ceiling(e.GetPosition(pnlGraphic).X * INTERVALS_IN_DAY / pnlGraphic.ActualWidth);
            var time = DateTime.Today.Add(TimeSpan.FromMinutes(totalMin * 10));
            tbTime.Text = time.ToString("HH:mm");
            if (_isDown && manPerMin >= 0)
            {
                int min = Convert.ToInt32(Math.Ceiling(totalMin));
                min = min < INTERVALS_IN_DAY ? min : INTERVALS_IN_DAY - 1;
                min = min >= 0 ? min : 0;
                _selectedDay.Distribution[min] = Convert.ToInt32(manPerMin);
                Rectangle rect = new Rectangle()
                {
                    Fill = Brushes.DeepSkyBlue,
                    Width = pnlGraphic.ActualWidth / INTERVALS_IN_DAY,
                    Height = (double)_selectedDay.Distribution[min] / MAX_PER_INTERVAL * pnlGraphic.ActualHeight,
                    Uid = min.ToString()
                };
                rect.SetValue(Canvas.LeftProperty, min * pnlGraphic.ActualWidth / INTERVALS_IN_DAY);
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
            if (_selectedDay == null)
                return;
            pnlGraphic.Children.Clear();
            for (int i = 0; i < _selectedDay.Distribution.Length; i++)
            {
                Rectangle rect = new Rectangle()
                {
                    Fill = Brushes.DeepSkyBlue,
                    Width = pnlGraphic.ActualWidth / INTERVALS_IN_DAY,
                    Height = (double)_selectedDay.Distribution[i] / MAX_PER_INTERVAL * pnlGraphic.ActualHeight,
                    Uid = i.ToString()
                };
                rect.SetValue(Canvas.LeftProperty, i * pnlGraphic.ActualWidth / INTERVALS_IN_DAY);
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
            DialogResult = true;
            Close();
        }

        private void pnlMap_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedDay == null)
                return;

            _isDown = true;
            pnlGraphic.CaptureMouse();

            double manPerMin = Math.Ceiling((pnlGraphic.ActualHeight - e.GetPosition(pnlGraphic).Y) * MAX_PER_INTERVAL / pnlGraphic.ActualHeight);
            double totalMin = Math.Ceiling(e.GetPosition(pnlGraphic).X * INTERVALS_IN_DAY / pnlGraphic.ActualWidth);
            int min = Convert.ToInt32(Math.Ceiling(totalMin));
            min = min < INTERVALS_IN_DAY ? min : INTERVALS_IN_DAY - 1;
            _selectedDay.Distribution[min] = Convert.ToInt32(manPerMin);
            Rectangle rect = new Rectangle()
            {
                Fill = Brushes.DeepSkyBlue,
                Width = pnlGraphic.ActualWidth / INTERVALS_IN_DAY,
                Height = (double)_selectedDay.Distribution[min] / MAX_PER_INTERVAL* pnlGraphic.ActualHeight,
                Uid = min.ToString()
            };
            rect.SetValue(Canvas.LeftProperty, min * pnlGraphic.ActualWidth / INTERVALS_IN_DAY);
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

    public class DayOfWeekVolume
    {
        public DayOfWeekVolume(DayOfWeek dayOfWeek, int[] distribution = null)
        {
            Day = dayOfWeek;
            //Получаем локализованное имя дня недели
                DateTime dt = DateTime.Now;
                while (dt.DayOfWeek != dayOfWeek)
                {
                    dt = dt.AddDays(1);
                }
                DayName = dt.ToString("dddd", System.Globalization.CultureInfo.CurrentCulture);
                DayName = char.ToUpper(DayName[0]) + DayName.Substring(1);
            //
            if (distribution != null)
            {
                Distribution = distribution;
            }
        }

        public string DayName { get; private set; }

        public DayOfWeek Day { get; private set; }

        private int[] _distribution = new int[AgentsVolumeConfig.INTERVALS_IN_DAY];
        public int[] Distribution
        {
            get { return _distribution; }
            set { _distribution = value; }
        }
    }
}
