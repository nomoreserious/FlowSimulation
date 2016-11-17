using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Agents;
using System.Windows.Media;
using System.Windows;
using FlowSimulation.Enviroment;
using FlowSimulation.Contracts.Services;
using System.Windows.Media.Media3D;

namespace FlowSimulation.Agents.Vehicle
{
    public class Vehicle : VehicleAgentBase
    {
        private double _currentLength;
        private Point _position;
        private bool _located = false;

        public new Point Position
        {
            get { return _position; }
            private set
            {
                if (_position == value)
                    return;
                _position = value;
                base.Position = new System.Drawing.Point((int)_position.X, (int)_position.Y);
            }
        }

        public Vehicle(Map map, IEnumerable<AgentServiceBase> services)
            : base(map, services)
        {
            _currentLength = 0.0D;
            Position = new Point();
        }

        public override void DoStep(double msInterval)
        {
            try
            {
                if (!_located)
                {
                    LocateAgent();
                }

                //Длина пути, которая может быть пройдена за один шаг моделирования
                double step_lenght = msInterval / CurrentSpeed;

                double tmp = _currentLength;
                double route_length = 0;
                for (int i = 1; i < RouteList.Count; i++)
                {
                    //Поиск дороги
                    string pathData = RoadGraph.GetEdgeData(RouteList[i - 1], RouteList[i]);
                    PathFigure road = GetPathFigureFromString(pathData);
                    if (road == null)
                    {
                        throw new InvalidCastException("Не удалось десериализовать часть пути из строки (" + pathData + ")");
                    }
                    //Расчет длины маршрута и данного участка
                    double road_length = CalculatePathLength(road);
                    route_length += road_length;

                    //Если этот участок еще не пройден
                    if (_currentLength < road_length)
                    {
                        //Если участок не будет пройден за этот шаг
                        if (_currentLength + step_lenght + Size.X / 2 < road_length)
                        {
                            _currentLength += step_lenght;
                            Position = CalculateCoordinateLocationOfCarrige();
                            break;
                        }
                        else if (tmp + step_lenght + Size.X / 2 > road_length)
                        {
                            if (tmp + Size.X / 2 <= road_length)
                            {
                                if (RouteList[i].IsServicePoint)
                                {
                                    //Паркуемся
                                    if (tmp != road_length - Size.X / 2)
                                    {
                                        _currentLength += road_length - tmp - Size.X / 2;
                                        Position = CalculateCoordinateLocationOfCarrige();
                                    }
                                    //Работаем с сервисом
                                    if (!ReadyToIOOperation)
                                    {
                                        var service = _services.FirstOrDefault(s => s.Id == RouteList[i].ServiceId);
                                        if (service == null)
                                        {
                                            throw new ArgumentException("Service not found, service id = " + RouteList[i].ServiceId);
                                        }
                                        service.AddAgent(this);
                                        ReadyToIOOperation = true;
                                        //if (service is StopService)
                                        //{
                                        //    (service as StopService).OpenInputPoints(Id);
                                        //    ReadyToIOOperation = true;
                                        //}
                                    }
                                    if (_go)
                                    {
                                        ReadyToIOOperation = false;
                                        _go = false;
                                    }
                                    if (ReadyToIOOperation)
                                    {
                                        return;
                                    }
                                }

                                if (RouteList.Count > i + 1 && i > 0 && RouteList[i + 1].Equals(RouteList[i - 1]))
                                {
                                    Revers(route_length);
                                }
                            }
                            _currentLength += step_lenght;
                            Position = CalculateCoordinateLocationOfCarrige();
                        }
                    }
                    else
                    {
                        if (i == RouteList.Count - 1)
                        {
                            _currentLength += step_lenght;
                            RouteList.Clear();
                        }
                        tmp -= road_length;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error in Thread! Agent Id [{0}]", Id));
                Console.WriteLine(ex.ToString());
                RouteList.Clear();
            }
        }

        public override void DoStepAsync(double msInterval)
        {
            throw new NotImplementedException();
        }

        public override void Initialize(Dictionary<string, object> settings)
        {
            Random rand = new Random();
            if (rand.NextDouble() > 0.5)
            {
                Size = new Size3D(8, 2.2, 2.2);
                base.MaxCapasity = 82;
                base.CurrentAgentCount = rand.Next(82);
            }
            else
            {
                Size = new Size3D(4.5, 1.82, 1.5);
                base.MaxCapasity = 20;
                base.CurrentAgentCount = rand.Next(20);
            }
            _maxSpeed = 360 + (rand.NextDouble() - 0.7) * 100;
            InputFactor = 25;
            OutputFactor = 25;
            _acceleration = 0.2;
            _deceleration = 0.4;
        }

        public override System.Windows.Shapes.Shape GetAgentShape()
        {
            if (_shape == null)
            {
                _shape = new System.Windows.Shapes.Rectangle()
                {
                    Width = Size.X * 2.5,
                    Height = Size.Y * 2.5,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 0.1
                };
            }
            return _shape;
        }

        private PathFigure GetPathFigureFromString(string pathData)
        {
            if (string.IsNullOrWhiteSpace(pathData))
            {
                return null;
            }
            var geometry = PathGeometry.CreateFromGeometry(PathGeometry.Parse(pathData));
            PathFigure figure = geometry.Figures.FirstOrDefault();
            return figure;
        }

        private double CalculatePathLength(PathFigure path)
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

        private void LocateAgent()
        {
            if (RouteList.Count > 1 || RoadGraph == null)
            {
                RoadGraph.ViewedNodes.Clear();
                RouteList = RoadGraph.GetNodesWay(RouteList.First(), RouteList.Last());
                Position = RouteList.First().Location;
                base.CurrentSpeed = base._maxSpeed;
                _located = true;
            }
            else
            {
                RouteList.Clear();
            }
        }

        private Point CalculateCoordinateLocationOfCarrige(PathFigure path, double vehiclePosition)
        {
            double tmp = vehiclePosition;
            for (int g = 0; g < path.Segments.Count; g++)
            {
                double x = 0, y = 0, t = 0, seg_l = 0;
                Point startPoint = path.StartPoint;
                if (path.Segments[g] is BezierSegment)
                {
                    BezierSegment seg = path.Segments[g] as BezierSegment;
                    seg_l = CalculatePathLength(new PathFigure(startPoint, new List<PathSegment>() { seg }, false));
                    if (seg_l < tmp)
                    {
                        tmp -= seg_l;
                        startPoint = seg.Point3;
                        continue;
                    }
                    t = tmp / seg_l;
                    x = (1 - t) * (1 - t) * (1 - t) * startPoint.X + (1 - t) * (1 - t) * 3 * t * seg.Point1.X + (1 - t) * 3 * t * t * seg.Point2.X + t * t * t * seg.Point3.X;
                    y = (1 - t) * (1 - t) * (1 - t) * startPoint.Y + (1 - t) * (1 - t) * 3 * t * seg.Point1.Y + (1 - t) * 3 * t * t * seg.Point2.Y + t * t * t * seg.Point3.Y;
                }
                else if (path.Segments[g] is LineSegment)
                {
                    LineSegment seg = path.Segments[g] as LineSegment;
                    seg_l = CalculatePathLength(new PathFigure(startPoint, new List<PathSegment>() { seg }, false));
                    if (seg_l < tmp)
                    {
                        tmp -= seg_l;
                        startPoint = seg.Point;
                        continue;
                    }
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
                else if (path.Segments[g] is PolyLineSegment)
                {
                    PolyLineSegment seg = path.Segments[g] as PolyLineSegment;
                    int l;
                    for (l = 0; l < seg.Points.Count; l++)
                    {
                        seg_l = Math.Sqrt(Math.Pow(startPoint.X - seg.Points[l].X, 2) + Math.Pow(startPoint.Y - seg.Points[l].Y, 2));
                        if (seg_l < tmp)
                        {
                            tmp -= seg_l;
                            startPoint = seg.Points[l];
                            continue;
                        }
                    }
                    t = tmp / seg_l;
                    x = seg.Points[l].X * t + startPoint.X * (1 - t);
                    y = (x - startPoint.X) * (seg.Points[l].Y - startPoint.Y) / (seg.Points[l].X - startPoint.X) + startPoint.Y;
                }
                CalculateAngle(Position, new Point(x, y));
                return new Point(x, y);
            }
            return new Point();
        }

        private Point CalculateCoordinateLocationOfCarrige()
        {
            double tmp = _currentLength;
            for (int i = 1; i < RouteList.Count; i++)
            {
                //Поиск дороги
                string pathData = RoadGraph.GetEdgeData(RouteList[i - 1], RouteList[i]);
                PathFigure road = PathGeometry.CreateFromGeometry(PathGeometry.Parse(pathData)).Figures[0];

                //Если дорога не найдена
                if (road == null)
                {
                    CalculateAngle(Position, RouteList[i].Location);
                    Position = RouteList[i].Location;
                    return new Point();
                }

                //Если найдена
                double road_lenght = CalculatePathLength(road);
                //Если этот участок уже пройден, то смотрим следующий
                if (tmp > road_lenght)
                {
                    tmp -= road_lenght;
                    continue;
                }
                for (int g = 0; g < road.Segments.Count; g++)
                {
                    double x = 0, y = 0, t = 0, seg_l = 0;
                    Point startPoint = road.StartPoint;
                    if (road.Segments[g] is BezierSegment)
                    {
                        BezierSegment seg = road.Segments[g] as BezierSegment;
                        seg_l = CalculatePathLength(new PathFigure(startPoint, new List<PathSegment>() { seg }, false));
                        if (seg_l < tmp)
                        {
                            tmp -= seg_l;
                            startPoint = seg.Point3;
                            continue;
                        }
                        t = tmp / seg_l;
                        x = (1 - t) * (1 - t) * (1 - t) * startPoint.X + (1 - t) * (1 - t) * 3 * t * seg.Point1.X + (1 - t) * 3 * t * t * seg.Point2.X + t * t * t * seg.Point3.X;
                        y = (1 - t) * (1 - t) * (1 - t) * startPoint.Y + (1 - t) * (1 - t) * 3 * t * seg.Point1.Y + (1 - t) * 3 * t * t * seg.Point2.Y + t * t * t * seg.Point3.Y;
                    }
                    else if (road.Segments[g] is LineSegment)
                    {
                        LineSegment seg = road.Segments[g] as LineSegment;
                        seg_l = CalculatePathLength(new PathFigure(startPoint, new List<PathSegment>() { seg }, false));
                        if (seg_l < tmp)
                        {
                            tmp -= seg_l;
                            startPoint = seg.Point;
                            continue;
                        }
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
                    else if (road.Segments[g] is PolyLineSegment)
                    {
                        PolyLineSegment seg = road.Segments[g] as PolyLineSegment;
                        int l;
                        for (l = 0; l < seg.Points.Count; l++)
                        {
                            seg_l = Math.Sqrt(Math.Pow(startPoint.X - seg.Points[l].X, 2) + Math.Pow(startPoint.Y - seg.Points[l].Y, 2));
                            if (seg_l < tmp)
                            {
                                tmp -= seg_l;
                                startPoint = seg.Points[l];
                                continue;
                            }
                        }
                        t = tmp / seg_l;
                        x = seg.Points[l].X * t + startPoint.X * (1 - t);
                        y = (x - startPoint.X) * (seg.Points[l].Y - startPoint.Y) / (seg.Points[l].X - startPoint.X) + startPoint.Y;
                    }
                    CalculateAngle(Position, new Point(x, y));
                    return new Point(x, y);
                }
            }
            return new Point();
        }

        private void Revers(double route_length)
        {
            _currentLength += (route_length - _currentLength) * 2;
        }

        private void CalculateAngle(Point first, Point next)
        {
            Vector sub = next - first;
            if (first.Y >= next.Y)
            {
                Angle = -90 + Math.Asin(sub.X / Math.Sqrt(Math.Pow(sub.X, 2) + Math.Pow(sub.Y, 2))) / Math.PI * 180;
            }
            else
            {
                Angle = 90 - Math.Asin(sub.X / Math.Sqrt(Math.Pow(sub.X, 2) + Math.Pow(sub.Y, 2))) / Math.PI * 180;
            }
        }
    }
}
