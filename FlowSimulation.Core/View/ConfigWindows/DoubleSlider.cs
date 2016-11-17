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

namespace FlowSimulation.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для SliderWhithTwoThings.xaml
    /// </summary>
    public partial class DoubleSlider : FrameworkElement
    {
        double max, min, min_interval;

        #region Registration state and id property
        public static readonly DependencyProperty FromValueProperty;
        public static readonly DependencyProperty ToValueProperty;
        private bool _isDown;
        private bool _isDragging;
        private double _startValue;
        private bool _isFrom;
        private bool _useTimeFormat;

        public double FromValue
        {
            get { return (double)GetValue(FromValueProperty); }
            set
            {
                if (value < ToValue && value >= 0)
                {
                    SetValue(FromValueProperty, value);
                }
            }
        }
        public double ToValue
        {
            get { return (double)GetValue(ToValueProperty); }
            set
            {
                if (value <= max && value > FromValue)
                {
                    SetValue(ToValueProperty, value);
                }
            }
        }

        static DoubleSlider()
        {
            FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata();
            metadata.DefaultValue = 0.0;
            metadata.AffectsRender = true;
            metadata.PropertyChangedCallback += OnPropertyChanget;

            FromValueProperty = DependencyProperty.Register("FromValue", typeof(double), typeof(DoubleSlider), metadata);

            FrameworkPropertyMetadata metadata1 = new FrameworkPropertyMetadata();
            metadata1.DefaultValue = 0.0;
            metadata1.AffectsRender = true;
            metadata1.PropertyChangedCallback += OnPropertyChanget;

            ToValueProperty = DependencyProperty.Register("ToValue", typeof(double), typeof(DoubleSlider), metadata1);
        }

        static void OnPropertyChanget(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as DoubleSlider).InvalidateVisual();
        }
        #endregion

        public DoubleSlider(double min, double max, double min_interval)
        {
            if (min >= max || max - min < min_interval)
            {
                throw new ArgumentException("Недопустимое значение аргументов");
            }
            this.max = max;
            this.min = min;
            this.min_interval = min_interval;
            _useTimeFormat = true;
        }

        private double LogicalToReal(double value)
        {
            return (ActualWidth - 20) * value / (max - min);
        }

        private double RealToLogical(double value)
        {
            return (max - min) * value / (ActualWidth - 20);
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawLine(new Pen(Brushes.LightGray, 7), new Point(10, 10), new Point(ActualWidth - 10, 10));
            dc.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.Gray, 1), new Rect(LogicalToReal(FromValue), 0, 10, 20));
            dc.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.Gray, 1), new Rect(10 + LogicalToReal(ToValue), 0, 10, 20));
            dc.DrawLine(new Pen(Brushes.DarkOrange, 7), new Point(10 + LogicalToReal(FromValue), 10), new Point(10 + LogicalToReal(ToValue), 10));
            if (_useTimeFormat)
            {
                dc.DrawText(new FormattedText(new DateTime().AddMinutes(Math.Ceiling(FromValue)).ToString("HH:mm"), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), 10, Brushes.Black), new Point(LogicalToReal(FromValue) - 15, 25));
                dc.DrawText(new FormattedText(new DateTime().AddMinutes(Math.Ceiling(ToValue)).ToString("HH:mm") == "00:00" ? "24:00" : new DateTime().AddMinutes(Math.Ceiling(ToValue)).ToString("HH:mm"), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), 10, Brushes.Black), new Point(LogicalToReal(ToValue) + 10, 25));
            }
            else
            {
                dc.DrawText(new FormattedText(Math.Ceiling(FromValue).ToString(), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), 10, Brushes.Black), new Point(LogicalToReal(FromValue), 25));
                dc.DrawText(new FormattedText(Math.Ceiling(ToValue).ToString(), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), 10, Brushes.Black), new Point(LogicalToReal(ToValue) + 5, 25));
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {

        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.GetPosition(this).X < 10 + LogicalToReal(FromValue) && e.GetPosition(this).X > LogicalToReal(FromValue) && e.GetPosition(this).Y > 0 && e.GetPosition(this).Y < 20)
            {
                _isDown = true;
                _startValue = e.GetPosition(this).X;
                _isFrom = true;
                this.CaptureMouse();
                e.Handled = true;
            }
            else if (e.GetPosition(this).X > 10 + LogicalToReal(ToValue) && e.GetPosition(this).X < LogicalToReal(ToValue) + 20 && e.GetPosition(this).Y > 0 && e.GetPosition(this).Y < 20)
            {
                _isDown = true;
                _startValue = e.GetPosition(this).X;
                _isFrom = false;
                this.CaptureMouse();
                e.Handled = true;
            }
        }
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isDown)
            {
                System.Windows.Input.Mouse.Capture(null);
                _isDragging = false;
                _isDown = false;
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDown)
            {
                if (_isDragging == false && (Math.Abs(e.GetPosition(this).X - _startValue) > SystemParameters.MinimumHorizontalDragDistance))
                {
                    _isDragging = true;
                }
                if (_isDragging)
                {
                    DragMoved();
                }
            }
        }

        private void DragStarted()
        {
            _isDragging = true;
        }

        private void DragMoved()
        {
            Point CurrentPosition = System.Windows.Input.Mouse.GetPosition(this);
            if (_isFrom)
            {
                if (CurrentPosition.X > 3 && CurrentPosition.X < 5 + LogicalToReal(ToValue - min_interval))
                {
                    FromValue = RealToLogical(CurrentPosition.X - 5);
                }
            }
            else
            {
                if (CurrentPosition.X > LogicalToReal(FromValue + min_interval) + 15 && CurrentPosition.X < ActualWidth - 3)
                {
                    ToValue = RealToLogical(CurrentPosition.X - 15);
                }
            }
        }
    }
}
