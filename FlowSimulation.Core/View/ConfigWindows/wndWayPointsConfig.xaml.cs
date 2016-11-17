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
using FlowSimulation.SimulationScenario;
using FlowSimulation.Agents;
using FlowSimulation.SimulationScenario.IO;
using FlowSimulation.Map.Model;

namespace FlowSimulation.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для wndWayPointsConfig.xaml
    /// </summary>
    public partial class wndWayPointsConfig : Window
    {
        Scenario scena;
        private double _zoom;
        private bool _isDown;
        private bool _isDragging;

        private Point _startPoint;
        private Point _positionOnSource;

        private List<WayPointVisual> WayPointVisualList;
        private WayPointVisual _sourceElement;

        private WayPointVisual SourceElement
        {
            get
            {
                return _sourceElement;
            }
            set
            {
                _sourceElement = value;
                pnlWayPointSettings.DataContext = _sourceElement;
            }
        }
    
        public List<WayPoint> WayPointsList { get; set; }

        private double Zoom
        {
            get { return _zoom; }
            set
            {
                double old_zoom = _zoom;
                _zoom = value;
                if (scena != null)
                {
                    ZoomMap(old_zoom);
                }
            }
        }

        public wndWayPointsConfig()
        {
            WayPointVisualList = new List<WayPointVisual>();
            WayPointsList = new List<WayPoint>();
            InitializeComponent();
            if ((Application.Current.MainWindow as MainWindow).Scena == null)
            {
                MessageBox.Show("нет карты");
                return;
            }
            scena = (Application.Current.MainWindow as MainWindow).Scena;
            for (int i = 0; i < scena.ServicesList.Count; i++)
            {
                cbSevice.Items.Add(scena.ServicesList[i]);
            }
            Zoom = 4.0;
            PaintMap(scena.paintObjectList);
            lblMapSize.Content = string.Format("{0} x {1}", Math.Round(pnlMap.Width / Zoom), Math.Round(pnlMap.Height / Zoom));
        }

        public wndWayPointsConfig(List<WayPoint> wayPointList)
        {
            WayPointVisualList = new List<WayPointVisual>();
            WayPointsList = new List<WayPoint>();
            InitializeComponent();
            if ((Application.Current.MainWindow as MainWindow).Scena == null)
            {
                MessageBox.Show("нет карты");
                return;
            }
            scena = (Application.Current.MainWindow as MainWindow).Scena;
            for (int i = 0; i < scena.ServicesList.Count; i++)
            {
                cbSevice.Items.Add(scena.ServicesList[i]);
            }
            Zoom = 4.0;
            PaintMap(scena.paintObjectList);
            lblMapSize.Content = string.Format("{0} x {1}", Math.Round(pnlMap.Width / Zoom), Math.Round(pnlMap.Height / Zoom));

            for (int i = 0; i < wayPointList.Count; i++)
            {
                WayPointVisual w = new WayPointVisual()
                {
                    Zoom = Zoom,
                    Number = (i + 1).ToString(),
                    SourcePoint = wayPointList[i]
                };
                WayPointVisualList.Add(w);
                pnlMap.Children.Add(w);
            }
        }

        private void ZoomMap(double oldZoom)
        {
            pnlMap.Width = scena.map.GetMap().GetLength(0) * Zoom;
            pnlMap.Height = scena.map.GetMap().GetLength(1) * Zoom;

            for (int i = 0; i < WayPointVisualList.Count; i++)
            {
                WayPointVisualList[i].Zoom = Zoom;
            }

            for (int i = 0; i < pnlMap.Children.Count; i++)
            {
                if (pnlMap.Children[i] is System.Windows.Shapes.Path)
                {
                    Matrix m = (pnlMap.Children[i] as System.Windows.Shapes.Path).RenderTransform.Value;
                    m.Scale(Zoom / oldZoom, Zoom / oldZoom);
                    (pnlMap.Children[i] as System.Windows.Shapes.Path).RenderTransform = new MatrixTransform(m);
                    continue;
                }
                if (pnlMap.Children[i] is Rectangle)
                {
                    Rectangle rect = pnlMap.Children[i] as Rectangle;

                    rect.SetValue(Canvas.LeftProperty, (double)rect.GetValue(Canvas.LeftProperty) / oldZoom * Zoom);
                    rect.SetValue(Canvas.TopProperty, (double)rect.GetValue(Canvas.TopProperty) / oldZoom * Zoom);
                    Matrix m = rect.RenderTransform.Value;
                    m.Scale(Zoom / oldZoom, Zoom / oldZoom);
                    rect.RenderTransform = new MatrixTransform(m);
                    continue;
                }
            }
        }

        private void PaintMap(List<PaintObject> PaintObjectList)
        {
            pnlMap.Width = scena.map.GetMap().GetLength(0) * Zoom;
            pnlMap.Height = scena.map.GetMap().GetLength(1) * Zoom;
            pnlMap.Children.Clear();

            pnlMap.Background = Brushes.Transparent;
            for (int n = 0; n < PaintObjectList.Count; n++)
            {
                PaintObject obj = PaintObjectList[n];
                if (obj.GetName() == "path")
                {
                    string data = obj.GetAttributeValue("data");
                    SVSObjectStyle style = obj.GetStyle();

                    Path path = new Path();
                    path.Data = Geometry.Parse(data);
                    path.Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(style.FillColor.A, style.FillColor.R, style.FillColor.G, style.FillColor.B));
                    path.Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(style.BorderColor.A, style.BorderColor.R, style.BorderColor.G, style.BorderColor.B));
                    path.StrokeThickness = (double)style.BorderSize;
                    System.Drawing.Drawing2D.Matrix m = obj.GetTransformMatrix();
                    Matrix matrix = new Matrix((double)m.Elements[0], (double)m.Elements[1], (double)m.Elements[2], (double)m.Elements[3], (double)m.OffsetX, (double)m.OffsetY);
                    matrix.Scale(Zoom, Zoom);
                    path.RenderTransform = new MatrixTransform(matrix);
                    pnlMap.Children.Add(path);
                }
                if (obj.GetName() == "rect")
                {
                    double x = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("x"));
                    double y = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("y"));
                    double w = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("width"));
                    double h = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("height"));
                    SVSObjectStyle style = obj.GetStyle();

                    Rectangle rect = new Rectangle();
                    rect.SetValue(Canvas.LeftProperty, x * Zoom);
                    rect.SetValue(Canvas.TopProperty, y * Zoom);
                    rect.Width = w;
                    rect.Height = h;
                    rect.Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(style.FillColor.A, style.FillColor.R, style.FillColor.G, style.FillColor.B));
                    rect.Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(style.BorderColor.A, style.BorderColor.R, style.BorderColor.G, style.BorderColor.B));
                    rect.StrokeThickness = (double)style.BorderSize;
                    System.Drawing.Drawing2D.Matrix m = obj.GetTransformMatrix();
                    Matrix matrix = new Matrix((double)m.Elements[0], (double)m.Elements[1], (double)m.Elements[2], (double)m.Elements[3], (double)m.OffsetX, (double)m.OffsetY);
                    matrix.Scale(Zoom, Zoom);
                    rect.RenderTransform = new MatrixTransform(matrix);
                    pnlMap.Children.Add(rect);
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            lblMouseLocation.Content = string.Format("{0}, {1}", Math.Round(e.GetPosition(pnlMap).X / Zoom), Math.Round(e.GetPosition(pnlMap).Y / Zoom));
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            lblMouseLocation.Content = "";
        }

        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Zoom = e.NewValue;
            lblZoomPersent.Content = Math.Round(Zoom * 25).ToString() + "%";
        }

        private void pnlMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(pnlMap);
            WayPointVisual wpv = new WayPointVisual()
            {
                Zoom = Zoom,
                SourcePoint = new WayPoint(Convert.ToInt32(position.X / Zoom), Convert.ToInt32(position.Y / Zoom),4,4),
                Number = (WayPointVisualList.Count + 1).ToString()
            };
            WayPointVisualList.Add(wpv);
            pnlMap.Children.Add(wpv);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content.ToString() == "Применить")
            {
                for (int i = 0; i < WayPointVisualList.Count; i++)
                {
                    WayPointsList.Add(WayPointVisualList[i].SourcePoint);
                }
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
            if (e.Source is WayPointVisual)
            {
                _isDown = true;          
                SourceElement = e.Source as WayPointVisual;
                //Проблема с Zoom
                _startPoint = e.GetPosition(pnlMap);//new Point(e.GetPosition(pnlMap).X / Zoom, e.GetPosition(pnlMap).Y / Zoom);
                _positionOnSource = new Point(_startPoint.X - SourceElement.SourcePoint.X * Zoom, _startPoint.Y - SourceElement.SourcePoint.Y * Zoom);
                pnlMap.CaptureMouse();
                e.Handled = true;
            }
        }

        private void pnlMap_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDown)
            {
                System.Windows.Input.Mouse.Capture(null);
                _isDragging = false;
                _isDown = false;
                e.Handled = true;
            }
        }

        private void pnlMap_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDown)
            {
                if (_isDragging == false && ((Math.Abs(e.GetPosition(pnlMap).X - _startPoint.X) > 1) || (Math.Abs(e.GetPosition(pnlMap).Y - _startPoint.Y) > 1)))
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
            Point CurrentPosition = System.Windows.Input.Mouse.GetPosition(pnlMap);
            SourceElement.SourcePoint.X = Convert.ToInt32((CurrentPosition.X - _positionOnSource.X) / Zoom);
            SourceElement.SourcePoint.Y = Convert.ToInt32((CurrentPosition.Y - _positionOnSource.Y) / Zoom);
            SourceElement.InvalidateVisual();
        }

        private void tbSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceElement.InvalidateVisual();
        }
    }

    class WayPointVisual : FrameworkElement
    {
       // public WayPoint SourcePoint { get; set; }
        public static readonly DependencyProperty NumberProperty;
        //public static readonly DependencyProperty LocationProperty;
        public static readonly DependencyProperty ZoomProperty;
        //public static readonly DependencyProperty WidthProperty;
        public static readonly DependencyProperty SourcePointProperty;

        public string Number
        {
            get { return (string)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        //public Point Location
        //{
        //    get { return (Point)GetValue(LocationProperty); }
        //    set { SetValue(LocationProperty, value); }
        //}

        //public int Width
        //{
        //    get { return (int)GetValue(WidthProperty); }
        //    set { SetValue(WidthProperty, value); }
        //}
        public WayPoint SourcePoint
        {
            get { return (WayPoint)GetValue(SourcePointProperty); }
            set { SetValue(SourcePointProperty, value); }
        }

        static WayPointVisual()
        {
            FrameworkPropertyMetadata metadata1 = new FrameworkPropertyMetadata();
            metadata1.DefaultValue = "";
            metadata1.AffectsRender = true;
            NumberProperty = DependencyProperty.Register("Number", typeof(string), typeof(WayPointVisual), metadata1);

            //FrameworkPropertyMetadata location_data = new FrameworkPropertyMetadata();
            //location_data.DefaultValue = new Point(0,0);
            //location_data.AffectsRender = true;
            //LocationProperty = DependencyProperty.Register("Location", typeof(Point), typeof(WayPointVisual), location_data);
            
            FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata();
            metadata.DefaultValue = 1.0;
            metadata.AffectsRender = true;
            ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(WayPointVisual), metadata);

            //FrameworkPropertyMetadata mdWidth = new FrameworkPropertyMetadata();
            //mdWidth.DefaultValue = 3;
            //mdWidth.AffectsRender = true;
            //WidthProperty = DependencyProperty.Register("Width", typeof(int), typeof(WayPointVisual), mdWidth);

            FrameworkPropertyMetadata mdHeight = new FrameworkPropertyMetadata();
            mdHeight.DefaultValue = new WayPoint();
            mdHeight.AffectsRender = true;
            SourcePointProperty = DependencyProperty.Register("SourcePoint", typeof(WayPoint), typeof(WayPointVisual), mdHeight);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Brush fill;
            if(Number=="S")
            {
                fill = Brushes.LightGreen;
            }
            else if (Number == "F")
            {
                fill = Brushes.Red;
            }
            else
            {
                fill = Brushes.SkyBlue;
            }
            drawingContext.DrawRectangle(fill, new Pen(Brushes.Gray, 0.5 * Zoom), new Rect(SourcePoint.X * Zoom, SourcePoint.Y * Zoom, SourcePoint.PointWidth * Zoom, SourcePoint.PointHeight * Zoom));
            drawingContext.DrawText(new FormattedText(Number, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), 4 * Zoom, Brushes.Black), new Point((SourcePoint.X + SourcePoint.PointWidth / 2 - 2) * Zoom, (SourcePoint.Y + SourcePoint.PointHeight / 2 - 3) * Zoom));
        }
    }
}
