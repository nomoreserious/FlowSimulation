using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Helpers.MVVM;
using System.Windows;
using System.Windows.Shapes;
using FlowSimulation.Scenario.Model;
using System.Windows.Media;
using FlowSimulation.Enviroment;
using FlowSimulation.Helpers.Graph;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FlowSimulation.ViewModel
{
    public class RoadGraphConfigViewModel :ViewModelBase, ICloseable
    {
        private bool _isDown;
        private bool _isDragging;
        private double _zoom = 1.0;
        private object _sourceElement;
        
        private Point _startPoint;
        private Point _positionOnSource;
        private List<Ellipse> elList = new List<Ellipse>();

        private List<VertexVisual> _vertexes;
        private List<EdgeVisual> _edges;

        private WayPoint _selectedVertexFrom;
        private WayPoint _selectedVertexTo;

        public RoadGraphConfigViewModel(ScenarioModel scenario)
        {
            if (scenario == null)
            {
                throw new ArgumentNullException("scenario is null");
            }

            GraphPanel = new Canvas();
            GraphPanel.MouseLeftButtonUp += (s, e) => MouseLeftButtonUp(s, e);
            GraphPanel.PreviewMouseLeftButtonDown += (s, e) => PreviewMouseLeftButtonDown(s, e);
            GraphPanel.PreviewMouseLeftButtonUp += (s, e) => PreviewMouseLeftButtonUp(s, e);
            GraphPanel.PreviewMouseMove += (s, e) => PreviewMouseMove(s, e);

            Services = scenario.Services;

            _vertexes = new List<VertexVisual>();
            _edges = new List<EdgeVisual>();

            RoadGraph = new Graph<WayPoint, PathFigure>();
            if (scenario.RoadGraph != null)
            {
                foreach (var edge in scenario.RoadGraph.Edges)
                {
                    var pathGeom = PathGeometry.CreateFromGeometry(PathGeometry.Parse(edge.Data));

                    EdgeVisual ev = new EdgeVisual(pathGeom.Figures.First())
                    {
                        NodeFrom = edge.Start,
                        NodeTo = edge.End,
                        Zoom = Zoom
                    };
                    _edges.Add(ev);
                    GraphPanel.Children.Add(ev);
                }

                foreach (var node in scenario.RoadGraph.Nodes)
                {
                    //int in_count = 0, out_count = 0;
                    //foreach (var edge in RoadGraph.GetEdgesTo(node))
                    //{
                    //    in_count++;
                    //}
                    //foreach (var edge in RoadGraph.GetEdgesFrom(node))
                    //{
                    //    out_count++;
                    //}

                    VertexVisual vv = new VertexVisual(node)
                    {
                        //InCount = in_count,
                        //OutCount = out_count,
                        Zoom = Zoom
                    };
                    _vertexes.Add(vv);
                    GraphPanel.Children.Add(vv);
                }
            }

            GraphPanel.Width = scenario.Map[0].Width;
            GraphPanel.Height = scenario.Map[0].Height;
            if (scenario.Map[0].Substrate!= null)
            {
                GraphPanel.Background = new ImageBrush(Helpers.Imaging.ImageManager.BitmapToBitmapImage(scenario.Map[0].Substrate));
            }
        }

        public ICommand RemoveNodeCommand { get { return new DelegateCommand(RemoveNode, () => _sourceElement is VertexVisual); } }
        public ICommand RemoveEdgeCommand { get { return new DelegateCommand(RemoveEdge, () => _sourceElement is EdgeVisual); } }

        public ICommand AddEdgeCommand { get { return new DelegateCommand(AddEdge, CanAddEdge); } }

        public ICommand SaveCommand { get { return new DelegateCommand(Save); } }

        public IEnumerable<WayPoint> Vertexes { get { return _vertexes.Select(v => v.Node); } }
        public IEnumerable<ServiceModel> Services { get; private set; }
        public Graph<WayPoint, PathFigure> RoadGraph { get; set; }

        private ServiceModel _selectedService;
        public ServiceModel SelectedService
        {
            get { return _selectedService; }
            set
            {
                _selectedService = value;
                if (_sourceElement is VertexVisual)
                {
                    ((VertexVisual)_sourceElement).Node.ServiceId = value.Id;
                }
                OnPropertyChanged("SelectedService");
            }
        }

        private bool _isServicePoint;
        public bool IsServicePoint
        {
            get { return _isServicePoint; }
            set
            {
                _isServicePoint = value;
                if (_sourceElement is VertexVisual)
                {
                    ((VertexVisual)_sourceElement).Node.IsServicePoint = value;
                }
                OnPropertyChanged("IsServicePoint");
            }
        }

        public bool CanEditService { get { return _sourceElement is VertexVisual; } }

        public WayPoint SelectedVertexFrom
        {
            get { return _selectedVertexFrom; }
            set { _selectedVertexFrom = value;OnPropertyChanged("SelectedVertexFrom"); }
        }

        public WayPoint SelectedVertexTo
        {
            get { return _selectedVertexTo; }
            set { _selectedVertexTo = value; OnPropertyChanged("SelectedVertexTo"); }
        }

        public double Zoom
        {
            get { return _zoom; }
            set { _zoom = value; OnPropertyChanged("Zoom"); }
        }

        public Canvas GraphPanel { get; private set; }

        #region ICloseable
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { _dialogResult = value; OnPropertyChanged("DialogResult"); }
        }

        private bool _closeView;
        public bool CloseView
        {
            get { return _closeView; }
            set
            {
                _closeView = value;
                OnPropertyChanged("CloseView");
            }
        }
        #endregion

        private void SetSourceElement(object value)
        {
            //Снимаем выделение предыдущего элемента
            if (_sourceElement != null && !(value is Ellipse))
            {
                if (_sourceElement is VertexVisual)
                {
                    ((VertexVisual)_sourceElement).IsSelected = false;
                }
                if (_sourceElement is EdgeVisual)
                {
                    ((EdgeVisual)_sourceElement).IsSelected = false;
                    foreach (var el in elList)
                    {
                        el.Visibility = Visibility.Hidden;
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
                    foreach (var el in elList)
                    {
                        el.Visibility = Visibility.Visible;
                    }
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
            else if (_sourceElement is VertexVisual)
            {
                var vertex = (VertexVisual)_sourceElement;
                vertex.IsSelected = true;
                IsServicePoint = vertex.Node.IsServicePoint;
                if (IsServicePoint && vertex.Node.ServiceId.HasValue)
                {
                    SelectedService = Services.FirstOrDefault(s => s.Id == vertex.Node.ServiceId);
                }
            }
            else if(!(_sourceElement is Ellipse))
            {
                _sourceElement = null;
            }
            OnPropertyChanged("CanEditService");
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
            GraphPanel.Children.Add(el);
            elList.Add(el);
        }

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(GraphPanel);
            WayPoint wp = new WayPoint()
            {
                Location = new Point(position.X / Zoom, position.Y / Zoom),
                Name = (_vertexes.Count + 1).ToString()
            };
            VertexVisual vv = new VertexVisual(wp)
            {
                Zoom = Zoom
            };
            _vertexes.Add(vv);
            OnPropertyChanged("Vertexes");
            GraphPanel.Children.Add(vv);
        }

        private void Save()
        {
            //if (_vertexes.Any(v=>v.InCount == 0 && v.OutCount == 0))
            //{
            //    if (MessageBox.Show("Все точки, не имеющие связей (обозначены серым цветом) будут удалены. Продолжить?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.No)
            //    {
            //        return;
            //    }
            //    for (int i = 0; i < _vertexes.Count; i++)
            //    {
            //        if (_vertexes[i].InCount == 0 && _vertexes[i].OutCount == 0)
            //        {
            //            _vertexes.RemoveAt(i);
            //            i--;
            //        }
            //    }
            //}
            if (_vertexes.Any(v => v.Node.IsServicePoint && !v.Node.ServiceId.HasValue))
            {
                MessageBox.Show("Для одной или более точек не указан сервис");
                return;
            }
            //Собираем граф дорожной сети
            RoadGraph = new Graph<WayPoint, PathFigure>();
            foreach (var node in _vertexes.Select(v=>v.Node))
            {
                RoadGraph.Add(node);
            }
            foreach (var edge in _edges)
            {
                RoadGraph.AddEdge(edge.NodeFrom, edge.NodeTo, edge.PathData);
            }
            DialogResult = true;
            CloseView = true;
        }

        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == _sourceElement)
                return;
            if (e.Source is VertexVisual)
            {
                VertexVisual vv = (VertexVisual)e.Source;
                _isDown = true;
                SetSourceElement(vv);

                _startPoint = e.GetPosition(GraphPanel);//new Point(e.GetPosition(pnlMap).X / Zoom, e.GetPosition(pnlMap).Y / Zoom);
                _positionOnSource = new Point(_startPoint.X - vv.Node.Location.X * Zoom, _startPoint.Y - vv.Node.Location.Y * Zoom);
                GraphPanel.CaptureMouse();
                e.Handled = true;
            }
            else if (e.Source is EdgeVisual)
            {
                EdgeVisual ev = (EdgeVisual)e.Source;
                _isDown = true;
                SetSourceElement(ev);
                e.Handled = true;
            }
            else if (e.Source is Ellipse)
            {
                _isDown = true;
                SetSourceElement(e.Source);
                e.Handled = true;
            }
        }

        private void PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDown)
            {
                System.Windows.Input.Mouse.Capture(null);
                _isDragging = false;
                _isDown = false;
                e.Handled = true;
            }
        }

        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDown)
            {
                var pos = e.GetPosition(GraphPanel);
                if (_isDragging == false && ((Math.Abs(pos.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) || (Math.Abs(pos.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
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
            Point CurrentPosition = Mouse.GetPosition(GraphPanel);
            if (_sourceElement is VertexVisual)
            {
                VertexVisual vv = (VertexVisual)_sourceElement;
                WayPoint prePoint = (WayPoint)vv.Node.Clone();
                vv.Location = new Point((CurrentPosition.X - _positionOnSource.X) / Zoom, (CurrentPosition.Y - _positionOnSource.Y) / Zoom);                
                
                for(int i=0;i<_edges.Count;i++)
                {
                    if (_edges[i].NodeFrom == vv.Node)
                    {
                        _edges[i].PathData.StartPoint = vv.Node.Location;
                    }
                    if (_edges[i].NodeTo == vv.Node)
                    {
                        if (_edges[i].PathData.Segments[0] is LineSegment)
                        {
                            (_edges[i].PathData.Segments[0] as LineSegment).Point = vv.Node.Location;
                        }
                        else if (_edges[i].PathData.Segments[0] is BezierSegment)
                        {
                            (_edges[i].PathData.Segments[0] as BezierSegment).Point3 = vv.Node.Location;
                        }
                    }
                    _edges[i].InvalidateVisual();
                }
            }
            else if (_sourceElement is Ellipse)
            {
                Ellipse el = (Ellipse)_sourceElement;
                bool is_first = (bool)el.Tag;
                Point newLocation = new Point((CurrentPosition.X - _positionOnSource.X) / Zoom, (CurrentPosition.Y - _positionOnSource.Y) / Zoom);
                el.SetValue(Canvas.TopProperty, newLocation.Y * Zoom);
                el.SetValue(Canvas.LeftProperty, newLocation.X * Zoom);
                for (int i = 0; i < _edges.Count; i++)
                {
                    if (_edges[i].IsSelected)
                    {
                        if (is_first)
                        {
                            (_edges[i].PathData.Segments[0] as BezierSegment).Point1 = newLocation;
                        }
                        else
                        {
                            (_edges[i].PathData.Segments[0] as BezierSegment).Point2 = newLocation;
                        }
                        _edges[i].InvalidateVisual();
                        return;
                    }
                }
            }
        }

        private void AddEdge()
        {
            WayPoint from = _selectedVertexFrom, to = _selectedVertexTo;
            EdgeVisual ev = new EdgeVisual(new PathFigure(from.Location, new List<PathSegment>() { new LineSegment(to.Location, true) }, false))
            {
                NodeFrom = from,
                NodeTo = to,
                Zoom = Zoom
            };
            _edges.Add(ev);
            GraphPanel.Children.Insert(0, ev);
            //_selectedVertexFrom.OutCount++;
            //_selectedVertexTo.InCount++;
        }

        private bool CanAddEdge()
        {
            return _selectedVertexFrom != null && _selectedVertexTo != null && 
                _selectedVertexFrom != _selectedVertexTo && 
                !_edges.Any(e => e.NodeFrom == _selectedVertexFrom && e.NodeTo == _selectedVertexTo);
        }

        private void RemoveNode()
        {
            if (MessageBox.Show("Вы действительно хотите удалить выбранную вершину?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var vertex = (VertexVisual)_sourceElement;
                var edges = _edges.Where(e => e.NodeFrom == vertex.Node || e.NodeTo == vertex.Node);
                foreach (var edge in edges)
                {
                    _edges.Remove(edge);
                    GraphPanel.Children.Remove(edge);
                }
                _vertexes.Remove(vertex);
                GraphPanel.Children.Remove(vertex);
                _sourceElement = null;
                OnPropertyChanged("Vertexes");
            }
        }

        private void RemoveEdge()
        {
            if (MessageBox.Show("Вы действительно хотите удалить выбранное ребро?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _edges.Remove((EdgeVisual)_sourceElement);
                GraphPanel.Children.Remove((EdgeVisual)_sourceElement);
                _sourceElement = null;
            }
        }
    }

    public class VertexVisual : FrameworkElement
    {
        private double _zoom;
        private bool _isSelected;
        private WayPoint _node;

        public VertexVisual(WayPoint node)
        {
            Node = node;
        }

        public WayPoint Node
        {
            get { return _node; }
            set { _node = value; Location = Node.Location; }
        }


        public double Zoom
        {
            get { return _zoom; }
            set { _zoom = value; InvalidateVisual(); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; InvalidateVisual(); }
        }

        public static readonly DependencyProperty InCountProperty;
        public static readonly DependencyProperty OutCountProperty;
        public static readonly DependencyProperty LocationProperty;

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
            get { return Node.Location; }
            set { SetValue(LocationProperty, value); Node.Location = value; }
        }

        static VertexVisual()
        {
            FrameworkPropertyMetadata location_data = new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender);
            LocationProperty = DependencyProperty.Register("Location", typeof(Point), typeof(VertexVisual), location_data);

            FrameworkPropertyMetadata mdic = new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender);
            InCountProperty = DependencyProperty.Register("InCount", typeof(int), typeof(VertexVisual), mdic);

            FrameworkPropertyMetadata mdoc = new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender);
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
            if (IsSelected)
            {
                drawingContext.DrawEllipse(fill, new Pen(Brushes.Black, 1.0 * Zoom), new Point(Location.X * Zoom, Location.Y * Zoom), 4 * Zoom, 4 * Zoom);
            }
            else
            {
                drawingContext.DrawEllipse(fill, new Pen(Brushes.DimGray, 0.3 * Zoom), new Point(Location.X * Zoom, Location.Y * Zoom), 4 * Zoom, 4 * Zoom);
            }
            drawingContext.DrawText(new FormattedText(Node.Name, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), 4 * Zoom, Brushes.Black), new Point((Location.X - 2) * Zoom, (Location.Y - 2.5) * Zoom));
        }
    }

    public class EdgeVisual : FrameworkElement
    {
        private double _zoom;
        private bool _isSelected;

        public EdgeVisual(PathFigure data)
        {
            PathData = data;
        }

        public double Zoom
        {
            get { return _zoom; }
            set { _zoom = value; InvalidateVisual(); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; InvalidateVisual(); }
        }

        public List<Ellipse> elList = new List<Ellipse>();
        public PathFigure PathData { get; set; }
        public WayPoint NodeFrom { get; set; }
        public WayPoint NodeTo { get; set; }

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
                    double tmp = startPoint == NodeFrom.Location ? seg_l - 7 : 7;
                    t = tmp / seg_l;
                    x = (1 - t) * (1 - t) * (1 - t) * startPoint.X + (1 - t) * (1 - t) * 3 * t * seg.Point1.X + (1 - t) * 3 * t * t * seg.Point2.X + t * t * t * seg.Point3.X;
                    y = (1 - t) * (1 - t) * (1 - t) * startPoint.Y + (1 - t) * (1 - t) * 3 * t * seg.Point1.Y + (1 - t) * 3 * t * t * seg.Point2.Y + t * t * t * seg.Point3.Y;
                }
                else if (segment is LineSegment)
                {
                    LineSegment seg = segment as LineSegment;
                    seg_l = CalculateLength(new PathFigure(startPoint, new List<PathSegment>() { seg }, false));
                    double tmp = startPoint == NodeFrom.Location ? seg_l - 7 : 7;
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
                    double tmp = startPoint == NodeFrom.Location ? seg_l - 7 : 7;
                    t = tmp / seg_l;
                    x = seg.Points[l].X * t + startPoint.X * (1 - t);
                    y = (x - startPoint.X) * (seg.Points[l].Y - startPoint.Y) / (seg.Points[l].X - startPoint.X) + startPoint.Y;
                }
                PathGeometry geom = new PathGeometry(new List<PathFigure>() { PathData });
                geom.Transform = new ScaleTransform(Zoom, Zoom);
                if (this.IsSelected)
                {
                    drawingContext.DrawEllipse(Brushes.Black, null, new Point(x * Zoom, y * Zoom), 1.5 * Zoom, 1.5 * Zoom);
                    drawingContext.DrawGeometry(null, new Pen(Brushes.Black, Zoom), geom);
                }
                else
                {
                    drawingContext.DrawEllipse(Brushes.Black, null, new Point(x * Zoom, y * Zoom), 1.5 * Zoom, 1.5 * Zoom);
                    drawingContext.DrawGeometry(null, new Pen(Brushes.Black, 0.3 * Zoom), geom);
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
