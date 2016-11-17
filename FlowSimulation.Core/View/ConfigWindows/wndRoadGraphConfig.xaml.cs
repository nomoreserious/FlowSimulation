using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FlowSimulation.Agents;
using FlowSimulation.SimulationScenario;
using FlowSimulation.Enviroment.Model;
using FlowSimulation.Enviroment;
using FlowSimulation.Helpers.Graph;

namespace FlowSimulation.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для wndWayPointsConfig.xaml
    /// </summary>
    public partial class wndRoadGraphConfig : Window
    {
        Scenario scena;

        private double _zoom;
        private bool _isDown;
        private bool _isDragging;

        private Point _startPoint;
        private Point _positionOnSource;
        private List<Ellipse> elList = new List<Ellipse>();

        public Graph<WayPoint, PathFigure> RoadGraph { get; set; }
        private object _sourceElement;

        private object SourceElement
        {
            get
            {
                return _sourceElement;
            }
            set
            {
                if (value is Ellipse == false)
                {
                    for (int i = 0; i < EdgeVisualsList.Count; i++)
                    {
                        if (EdgeVisualsList[i].IsSelected)
                        {
                            EdgeVisualsList[i].IsSelected = false;
                            break;
                        }
                    }
                }
                _sourceElement = value;
                if (_sourceElement is EdgeVisual)
                {
                    EdgeVisual ev = _sourceElement as EdgeVisual;
                    ev.IsSelected = true;
                    if (elList.Count == 0)
                    {
                        if (ev.PathData.Segments[0] is BezierSegment)
                        {
                            AddEllipseOnParentPanel((ev.PathData.Segments[0] as BezierSegment).Point1, true);
                            AddEllipseOnParentPanel((ev.PathData.Segments[0] as BezierSegment).Point2, false);
                        }
                        else if (ev.PathData.Segments[0] is LineSegment)
                        {
                            Point endPoint = (ev.PathData.Segments[0] as LineSegment).Point;
                            Point controlPoint = new Point((ev.PathData.StartPoint.X + endPoint.X) / 2, (ev.PathData.StartPoint.Y + endPoint.Y) / 2);
                            ev.PathData.Segments.Clear();
                            ev.PathData.Segments.Add(new BezierSegment(controlPoint, controlPoint, endPoint, true));
                            AddEllipseOnParentPanel((ev.PathData.Segments[0] as BezierSegment).Point1, true);
                            AddEllipseOnParentPanel((ev.PathData.Segments[0] as BezierSegment).Point2, false);
                        }
                    }
                    else
                    {
                        if (ev.PathData.Segments[0] is BezierSegment)
                        {
                            elList[0].SetValue(Canvas.TopProperty, (ev.PathData.Segments[0] as BezierSegment).Point1.Y * Zoom);
                            elList[0].SetValue(Canvas.LeftProperty, (ev.PathData.Segments[0] as BezierSegment).Point1.X * Zoom);
                            elList[1].SetValue(Canvas.TopProperty, (ev.PathData.Segments[0] as BezierSegment).Point2.Y * Zoom);
                            elList[1].SetValue(Canvas.LeftProperty, (ev.PathData.Segments[0] as BezierSegment).Point2.X * Zoom);
                        }
                        else if (ev.PathData.Segments[0] is LineSegment)
                        {
                            Point endPoint = (ev.PathData.Segments[0] as LineSegment).Point;
                            Point controlPoint = new Point((ev.PathData.StartPoint.X + endPoint.X) / 2, (ev.PathData.StartPoint.Y + endPoint.Y) / 2);
                            ev.PathData.Segments.Clear();
                            ev.PathData.Segments.Add(new BezierSegment(controlPoint, controlPoint, endPoint, true));
                            elList[0].SetValue(Canvas.TopProperty, (ev.PathData.Segments[0] as BezierSegment).Point1.Y * Zoom);
                            elList[0].SetValue(Canvas.LeftProperty, (ev.PathData.Segments[0] as BezierSegment).Point1.X * Zoom);
                            elList[1].SetValue(Canvas.TopProperty, (ev.PathData.Segments[0] as BezierSegment).Point2.Y * Zoom);
                            elList[1].SetValue(Canvas.LeftProperty, (ev.PathData.Segments[0] as BezierSegment).Point2.X * Zoom);
                        }
                    }
                }
                if (_sourceElement is VertexVisual)
                {
                    pnlWayPointSettings.DataContext = _sourceElement;
                }
            }
        }

        private void AddEllipseOnParentPanel(Point location, bool is_first)
        {
            Ellipse el = new Ellipse()
            {
                Width = 3.5 * Zoom,
                Height = 3.5 * Zoom,
                Stroke = Brushes.BlueViolet,
                StrokeThickness = 0.7 * Zoom,
                Fill = Brushes.White,
                Tag = is_first
            };
            el.SetValue(Canvas.TopProperty, location.Y * Zoom);
            el.SetValue(Canvas.LeftProperty, location.X * Zoom);
            pnlMap.Children.Add(el);
            elList.Add(el);
        }

        List<VertexVisual> VertexVisualsList { get; set; }
        List<EdgeVisual> EdgeVisualsList { get; set; }

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

        public wndRoadGraphConfig()
        {
            VertexVisualsList = new List<VertexVisual>();
            EdgeVisualsList = new List<EdgeVisual>();
            //RoadGraph = new Graph<WayPoint,PathFigure>();
            InitializeComponent();

            scena = (Application.Current.MainWindow as MainWindow).Scena;
            for (int i = 0; i < scena.ServicesList.Count; i++)
            {
                cbSevice.Items.Add(scena.ServicesList[i]);
            }
            Zoom = 4.0;
            PaintMap();
        }

        public wndRoadGraphConfig(Graph<WayPoint, PathFigure> roadGraph)
        {
            VertexVisualsList = new List<VertexVisual>();
            EdgeVisualsList = new List<EdgeVisual>();
            RoadGraph = roadGraph;
            InitializeComponent();

            scena = (Application.Current.MainWindow as MainWindow).Scena;
            for (int i = 0; i < scena.ServicesList.Count; i++)
            {
                cbSevice.Items.Add(scena.ServicesList[i]);
            }
            Zoom = 4.0;
            PaintMap();

            foreach (var edge in RoadGraph.Edges)
            {
                EdgeVisual ev = new EdgeVisual(edge.Data)
                {
                    NodeFrom = edge.Start,
                    NodeTo = edge.End,
                    Zoom = Zoom
                };
                EdgeVisualsList.Add(ev);
                pnlMap.Children.Add(ev);
            }

            foreach (var node in RoadGraph.Nodes)
            {
                int in_count = 0, out_count = 0;
                foreach (var edge in RoadGraph.GetEdgesTo(node))
                {
                    in_count++;
                }
                foreach (var edge in RoadGraph.GetEdgesFrom(node))
                {
                    out_count++;
                }

                VertexVisual vv = new VertexVisual(node)
                {
                    Number = (VertexVisualsList.Count + 1).ToString(),
                    InCount = in_count,
                    OutCount = out_count,
                    Zoom = Zoom
                };
                VertexVisualsList.Add(vv);
                pnlMap.Children.Add(vv);
            }
        }

        private void ZoomMap(double oldZoom)
        {
            pnlMap.Width = scena.map.GetMap().GetLength(0) * Zoom;
            pnlMap.Height = scena.map.GetMap().GetLength(1) * Zoom;

            for (int i = 0; i < VertexVisualsList.Count; i++)
            {
                VertexVisualsList[i].Zoom = Zoom;
            }
            for (int i = 0; i < EdgeVisualsList.Count; i++)
            {
                EdgeVisualsList[i].Zoom = Zoom;
            }
            for (int i = 0; i < pnlMap.Children.Count; i++)
            {
                if (pnlMap.Children[i] is Ellipse)
                {
                    Ellipse el = pnlMap.Children[i] as Ellipse;
                    el.SetValue(Canvas.TopProperty, (double)el.GetValue(Canvas.TopProperty) / oldZoom * Zoom);
                    el.SetValue(Canvas.LeftProperty, (double)el.GetValue(Canvas.LeftProperty) / oldZoom * Zoom);
                }
            }
        }

        private void PaintMap()
        {
            pnlMap.Width = scena.map.GetMap().GetLength(0) * Zoom;
            pnlMap.Height = scena.map.GetMap().GetLength(1) * Zoom;
            lblMapSize.Content = string.Format("{0} x {1}", Math.Round(pnlMap.Width / Zoom), Math.Round(pnlMap.Height / Zoom));
            if (System.IO.File.Exists(Properties.Settings.Default.ScenarioPath + "image.jpg"))
            {
                pnlMap.Background = new ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(Properties.Settings.Default.ScenarioPath + "image.jpg")));
            }
            else
            {
                pnlMap.Background = new ImageBrush(scena.Image);
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
            WayPoint wp = new WayPoint()
            {
                LocationPoint = new Point(position.X / Zoom, position.Y / Zoom)
            };
            VertexVisual vv = new VertexVisual(wp)
            {
                Number = (VertexVisualsList.Count + 1).ToString(),
                Zoom = Zoom
            };
            VertexVisualsList.Add(vv);
            pnlMap.Children.Add(vv);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool has_dont_linked = false;
            for (int i = 0; i < VertexVisualsList.Count; i++)
            {
                if (VertexVisualsList[i].InCount == 0 && VertexVisualsList[i].OutCount == 0)
                {
                    has_dont_linked = true;
                }
            }
            if (has_dont_linked)
            {
                if (MessageBox.Show("Все точки, не имеющие связей (обозначены серым цветом) будут удалены. Продолжить?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }
                List<WayPoint> remList = new List<WayPoint>();
                foreach (var node in RoadGraph.Nodes)
                {
                    int in_count = 0, out_count = 0;
                    foreach (var edge in RoadGraph.GetEdgesTo(node))
                    {
                        in_count++;
                    }
                    foreach (var edge in RoadGraph.GetEdgesFrom(node))
                    {
                        out_count++;
                    }
                    if (in_count == 0 && out_count == 0)
                    {
                        remList.Add(node);
                    }
                }
                for (int i = 0; i < remList.Count; i++)
                {
                    RoadGraph.Remove(remList[i]);
                }
            }
            //Собираем граф дорожной сети
            //RoadGraph = new Graph<WayPoint, PathFigure>();
            for (int i = 0; i < VertexVisualsList.Count; i++)
            {
                RoadGraph.Add(VertexVisualsList[i].Node);
            }
            for (int i = 0; i < EdgeVisualsList.Count; i++)
            {
                RoadGraph.AddEdge(EdgeVisualsList[i].NodeFrom, EdgeVisualsList[i].NodeTo, EdgeVisualsList[i].PathData);
            }
            DialogResult = true;
            Close();
        }

        private void pnlMap_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is VertexVisual)
            {
                VertexVisual vv = e.Source as VertexVisual;
                _isDown = true;
                SourceElement = vv;
                _startPoint = e.GetPosition(pnlMap);//new Point(e.GetPosition(pnlMap).X / Zoom, e.GetPosition(pnlMap).Y / Zoom);
                _positionOnSource = new Point(_startPoint.X - vv.Node.LocationPoint.X * Zoom, _startPoint.Y - vv.Node.LocationPoint.Y * Zoom);
                pnlMap.CaptureMouse();
                e.Handled = true;
            }
            else if (e.Source is EdgeVisual)
            {
                EdgeVisual ev = e.Source as EdgeVisual;
                _isDown = true;
                SourceElement = ev;
                e.Handled = true;
            }
            else if (e.Source is Ellipse)
            {
                _isDown = true;
                SourceElement = e.Source;
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
                if (_isDragging == false && ((Math.Abs(e.GetPosition(pnlMap).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) || (Math.Abs(e.GetPosition(pnlMap).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
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
            if (SourceElement is VertexVisual)
            {       
                VertexVisual vv = SourceElement as VertexVisual;
                WayPoint prePoint = (WayPoint)vv.Node.Clone();
                vv.Location = new Point((CurrentPosition.X - _positionOnSource.X) / Zoom, (CurrentPosition.Y - _positionOnSource.Y) / Zoom);                
                
                for(int i=0;i<EdgeVisualsList.Count;i++)
                {
                    if (EdgeVisualsList[i].NodeFrom == vv.Node)
                    {
                        EdgeVisualsList[i].PathData.StartPoint = vv.Node.LocationPoint;
                    }
                    if(EdgeVisualsList[i].NodeTo == vv.Node)
                    {
                        if (EdgeVisualsList[i].PathData.Segments[0] is LineSegment)
                        {
                            (EdgeVisualsList[i].PathData.Segments[0] as LineSegment).Point = vv.Node.LocationPoint;
                        }
                        else if (EdgeVisualsList[i].PathData.Segments[0] is BezierSegment)
                        {
                            (EdgeVisualsList[i].PathData.Segments[0] as BezierSegment).Point3 = vv.Node.LocationPoint;
                        }
                    }
                    EdgeVisualsList[i].InvalidateVisual();
                }
            }
            else if (SourceElement is Ellipse)
            {
                Ellipse el = SourceElement as Ellipse;
                bool is_first = (bool)el.Tag;
                Point newLocation = new Point((CurrentPosition.X - _positionOnSource.X) / Zoom, (CurrentPosition.Y - _positionOnSource.Y) / Zoom);
                el.SetValue(Canvas.TopProperty, newLocation.Y * Zoom);
                el.SetValue(Canvas.LeftProperty, newLocation.X * Zoom);
                for (int i = 0; i < EdgeVisualsList.Count; i++)
                {
                    if (EdgeVisualsList[i].IsSelected)
                    {
                        if (is_first)
                        {
                            (EdgeVisualsList[i].PathData.Segments[0] as BezierSegment).Point1 = newLocation;
                        }
                        else
                        {
                            (EdgeVisualsList[i].PathData.Segments[0] as BezierSegment).Point2 = newLocation;
                        }
                        EdgeVisualsList[i].InvalidateVisual();
                        return;
                    }
                }
            }
        }

        private void btnRemoveNode_Click(object sender, RoutedEventArgs e)
        {
            if (_sourceElement is VertexVisual)
            {
                VertexVisualsList.Remove(_sourceElement as VertexVisual);
                pnlMap.Children.Remove(_sourceElement as VertexVisual);
                _sourceElement = null;
            }
        }

        private void btnAddEdge_Click(object sender, RoutedEventArgs e)
        {
            int from, to;
            if (!int.TryParse(tbStart.Text, out from) || !int.TryParse(tbFinish.Text, out to))
            {
                lblErrorInfo.Content = "Укажите номер вершины";
                return;
            }
            try
            {
                WayPoint NodeFrom, NodeTo;
                NodeFrom = VertexVisualsList[VertexVisualsList.FindIndex(delegate (VertexVisual vv){return int.Parse(vv.Number) == from;})].Node;
                NodeTo = VertexVisualsList[VertexVisualsList.FindIndex(delegate (VertexVisual vv){return int.Parse(vv.Number) == to;})].Node;
                if (EdgeVisualsList.FindIndex(delegate(EdgeVisual obj) { return obj.NodeFrom == NodeFrom && obj.NodeTo == NodeTo; }) == -1)
                {
                    EdgeVisual ev = new EdgeVisual(new PathFigure(NodeFrom.LocationPoint, new List<PathSegment>() { new LineSegment(NodeTo.LocationPoint, true) }, false))
                    {
                        NodeFrom = NodeFrom,
                        NodeTo = NodeTo,
                        Zoom = Zoom
                    };
                    EdgeVisualsList.Add(ev);
                    pnlMap.Children.Insert(0, ev);
                    foreach (var vertex in VertexVisualsList.FindAll(delegate(VertexVisual vv) { return vv.Node == NodeFrom; }))
                    {
                        vertex.OutCount++;
                    }
                    foreach (var vertex in VertexVisualsList.FindAll(delegate(VertexVisual vv) { return vv.Node == NodeTo; }))
                    {
                        vertex.InCount++;
                    }
                }
                else
                {
                    lblErrorInfo.Content = "Такое ребро уже существует";
                    return;
                }
            }
            catch (IndexOutOfRangeException ee)
            {
                lblErrorInfo.Content = "Одна из вершин не найдена";
                return;
            }
            catch (ArgumentOutOfRangeException ee)
            {
                lblErrorInfo.Content = "Одна из вершин не найдена";
                return;
            }
            lblErrorInfo.Content = "Ребро добавлено";
        }
    }

    class VertexVisual : FrameworkElement
    {
        public WayPoint _node;
        public WayPoint Node
        {
            get { return _node; }
            set { _node = value; Location = Node.LocationPoint; }
        }

        public static readonly DependencyProperty NumberProperty;
        public static readonly DependencyProperty InCountProperty;
        public static readonly DependencyProperty OutCountProperty;
        public static readonly DependencyProperty LocationProperty;
        public static readonly DependencyProperty ZoomProperty;

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

        public int InCount
        {
            get { return (int)GetValue(InCountProperty); }
            set { SetValue(InCountProperty, value); }
        }

        public int OutCount
        {
            get { return (int)GetValue(OutCountProperty); }
            set { SetValue(OutCountProperty, value); }
        }

        public Point Location
        {
            get { return Node.LocationPoint; }
            set { SetValue(LocationProperty, value); Node.LocationPoint = value; }
        }

        public VertexVisual(WayPoint node)
        {
            Node = node;
        }

        static VertexVisual()
        {
            FrameworkPropertyMetadata metadata1 = new FrameworkPropertyMetadata();
            metadata1.DefaultValue = "";
            metadata1.AffectsRender = true;
            NumberProperty = DependencyProperty.Register("Name", typeof(string), typeof(VertexVisual), metadata1);

            FrameworkPropertyMetadata location_data = new FrameworkPropertyMetadata();
            location_data.DefaultValue = new Point(0,0);
            location_data.AffectsRender = true;
            LocationProperty = DependencyProperty.Register("Location", typeof(Point), typeof(VertexVisual), location_data);
            
            FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata();
            metadata.DefaultValue = 1.0;
            metadata.AffectsRender = true;
            ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(VertexVisual), metadata);

            FrameworkPropertyMetadata mdic = new FrameworkPropertyMetadata();
            mdic.DefaultValue = 0;
            mdic.AffectsRender = true;
            InCountProperty = DependencyProperty.Register("InCount", typeof(int), typeof(VertexVisual), mdic);

            FrameworkPropertyMetadata mdoc = new FrameworkPropertyMetadata();
            mdoc.DefaultValue = 0;
            mdoc.AffectsRender = true;
            OutCountProperty = DependencyProperty.Register("OutCount", typeof(int), typeof(VertexVisual), mdoc);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Brush fill;
            if(InCount == 0 && OutCount == 0)
            {
                fill = Brushes.LightGray;
            }
            else if (InCount > 0 && OutCount == 0)
            {
                fill = Brushes.Red;
            }
            else if (InCount == 0 && OutCount > 0)
            {
                fill = Brushes.Lime;
            }
            else
            {
                fill = Brushes.SkyBlue;
            }
            drawingContext.DrawEllipse(fill, new Pen(Brushes.Black, 0.3 * Zoom), new Point(Location.X * Zoom, Location.Y * Zoom), 5 * Zoom, 5 * Zoom);
            drawingContext.DrawText(new FormattedText(Number, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), 4 * Zoom, Brushes.Black), new Point((Location.X - 2) * Zoom, (Location.Y - 3) * Zoom));
        }
    }

    class EdgeVisual : FrameworkElement
    {
        public static readonly DependencyProperty IsSelectedProperty;
        public static readonly DependencyProperty ZoomProperty;

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public List<Ellipse> elList = new List<Ellipse>();

        public PathFigure PathData { get; set; }

        public WayPoint NodeFrom { get; set; }

        public WayPoint NodeTo { get; set; }


        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public EdgeVisual(PathFigure data)
        {
            PathData = data;
        }

        static EdgeVisual()
        {
            FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata();
            metadata.DefaultValue = 1.0;
            metadata.AffectsRender = true;
            ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(EdgeVisual), metadata);

            FrameworkPropertyMetadata ispmd = new FrameworkPropertyMetadata();
            ispmd.DefaultValue = false;
            ispmd.AffectsRender = true;
            IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(EdgeVisual), ispmd);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (PathData != null)
            {
                PathSegment segment = PathData.Segments[0];
                Point startPoint = PathData.StartPoint;
                double seg_l = 0, t = 0, x = 0, y = 0;
                if (segment is BezierSegment)
                {
                    BezierSegment seg = segment as BezierSegment;
                    seg_l = CalculateLength(new PathFigure(startPoint, new List<PathSegment>() { seg }, false));
                    double tmp = startPoint == NodeFrom.LocationPoint ? seg_l - 7 : 7;
                    t = tmp / seg_l;
                    x = (1 - t) * (1 - t) * (1 - t) * startPoint.X + (1 - t) * (1 - t) * 3 * t * seg.Point1.X + (1 - t) * 3 * t * t * seg.Point2.X + t * t * t * seg.Point3.X;
                    y = (1 - t) * (1 - t) * (1 - t) * startPoint.Y + (1 - t) * (1 - t) * 3 * t * seg.Point1.Y + (1 - t) * 3 * t * t * seg.Point2.Y + t * t * t * seg.Point3.Y;
                }
                else if (segment is LineSegment)
                {
                    LineSegment seg = segment as LineSegment;
                    seg_l = CalculateLength(new PathFigure(startPoint, new List<PathSegment>() { seg }, false));
                    double tmp = startPoint == NodeFrom.LocationPoint ? seg_l - 7 : 7;
                    t = tmp / seg_l;
                    x = seg.Point.X * t + startPoint.X * (1 - t);
                    if (startPoint.X == seg.Point.X)
                    {
                        y = startPoint.Y + (seg.Point.Y - startPoint.Y) * t;
                    }
                    else
                    {
                        y = (x - startPoint.X) * (seg.Point.Y - startPoint.Y) / (seg.Point.X - startPoint.X) + startPoint.Y;
                    }
                }
                else if (segment is PolyLineSegment)
                {
                    PolyLineSegment seg = segment as PolyLineSegment;
                    int l;
                    for (l = 0; l < seg.Points.Count; l++)
                    {
                        seg_l = Math.Sqrt(Math.Pow(startPoint.X - seg.Points[l].X, 2) + Math.Pow(startPoint.Y - seg.Points[l].Y, 2));
                    }
                    double tmp = startPoint == NodeFrom.LocationPoint ? seg_l - 7 : 7;
                    t = tmp / seg_l;
                    x = seg.Points[l].X * t + startPoint.X * (1 - t);
                    y = (x - startPoint.X) * (seg.Points[l].Y - startPoint.Y) / (seg.Points[l].X - startPoint.X) + startPoint.Y;
                }
                PathGeometry geom = new PathGeometry(new List<PathFigure>() { PathData });
                geom.Transform = new ScaleTransform(Zoom, Zoom);
                if (this.IsSelected)
                {
                    drawingContext.DrawEllipse(Brushes.Blue, null, new Point(x * Zoom, y * Zoom), 2 * Zoom, 2 * Zoom);
                    drawingContext.DrawGeometry(null, new Pen(Brushes.Blue, 1.5 * Zoom), geom);
                }
                else
                {
                    drawingContext.DrawEllipse(Brushes.Black, null, new Point(x * Zoom, y * Zoom), 2 * Zoom, 2 * Zoom);
                    drawingContext.DrawGeometry(null, new Pen(Brushes.Black, 1.5 * Zoom), geom);
                }
            }
        }

        private double CalculateLength(PathFigure path)
        {
            PathFigure p = path.GetFlattenedPathFigure(0.1, ToleranceType.Relative);
            double lenght = 0;
            Point start = p.StartPoint;
            for (int i = 0; i < p.Segments.Count; i++)
            {
                if (p.Segments[i] is LineSegment)
                {
                    LineSegment seg = (p.Segments[i] as LineSegment);
                    lenght += Point.Subtract(start, seg.Point).Length;
                    start = seg.Point;
                }
                else if (p.Segments[i] is PolyLineSegment)
                {
                    PolyLineSegment seg = (p.Segments[i] as PolyLineSegment);
                    for (int j = 0; j < seg.Points.Count; j++)
                    {
                        lenght += Point.Subtract(start, seg.Points[j]).Length;
                        start = seg.Points[j];
                    }
                }
            }
            return lenght;
        }
    }
}
