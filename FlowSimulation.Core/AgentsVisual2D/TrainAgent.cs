using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using FlowSimulation.SimulationScenario;
using System.Windows;
using FlowSimulation.Service;
using FlowSimulation.Map.Model;

namespace FlowSimulation.Agents
{
    class TrainAgent : VehicleAgentBase
    {
        private int number_of_carriges;
        private Point[] positions;
        private double[] currentLength;
        private bool?[] need_draw;
        private double[] angles;

        public Point[] Positions
        {
            get { return positions; }
        }

        public bool?[] NeedDraw
        {
            get { return need_draw; }
        }

        public double[] Angles
        {
            get { return angles; }
        }

        public TrainAgent(int id, Scenario scenario, int group, int speed, int number_of_carriges) : base(id, scenario, group, speed)
        {
            this.number_of_carriges = number_of_carriges;
            positions = new Point[number_of_carriges];
            for (int i = 0; i < number_of_carriges; i++)
            {
                positions[i] = new Point(-1, -1);
            }
            currentLength = new double[number_of_carriges];
            need_draw = new bool?[number_of_carriges];
            angles = new double[number_of_carriges];
            CheckPointsList = new List<WayPoint>();
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

        public override void Life()
        {
            throw new NotImplementedException();
        }

        private void LocateAgent()
        {
            for (int i = 0; i < number_of_carriges; i++)
            {
                currentLength[i] = -i * (Size.X + 2);
            }
            if (WayPointsList.Count > 1)
            {
                CheckPointsList = new List<WayPoint>();
                for (int i = 1; i < WayPointsList.Count; i++)
                {
                    RoadGraph.ViewedNodes.Clear();
                    foreach (var node in RoadGraph.GetNodesWay(WayPointsList[i - 1], WayPointsList[i]))
                    {
                        if (CheckPointsList.Count == 0 || !CheckPointsList[CheckPointsList.Count - 1].Equals(node))
                        {
                            CheckPointsList.Add(node);
                        }
                    }
                }
                positions[0] = CheckPointsList[0].LocationPoint;
                position = CheckPointsList[0].LocationPoint;
                need_draw[0] = true;
                CurrentSpeed = MaxSpeed;
            }
        }

        private void Revers(double route_length)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                currentLength[i] += (route_length - currentLength[i]) * 2;
            }
            for (int i = 0; i < positions.Length / 2; i++)
            {
                var t = positions[i];
                positions[i] = positions[positions.Length - i - 1];
                positions[positions.Length - i - 1] = t;

                double dt = currentLength[i];
                currentLength[i] = currentLength[currentLength.Length - i - 1];
                currentLength[currentLength.Length - i - 1] = dt;
            }
            position = positions[0];
        }

        private void CalculateOtherCarrigesPosition()
        {
            for (int n = 1; n < number_of_carriges; n++)
            {
                if (need_draw[n] == false)
                {
                    continue;
                }
                currentLength[n] = currentLength[0] - n * (Size.X + 2);
                if (currentLength[n] >= 0)
                {
                    need_draw[n] = true;
                    positions[n] = CalculateCoordinateLocationOfCarrige(n);
                    if (positions[n] == new Point(-1, -1))
                    {
                        need_draw[n] = false;
                    }
                }
            }
            if (need_draw[number_of_carriges - 1] == false)
            {
                WayPointsList.Clear();
            }
            position.X += 1;
        }

        private Point CalculateCoordinateLocationOfCarrige(int n)
        {
            double tmp = currentLength[n];
            for (int i = 1; i < CheckPointsList.Count; i++)
            {
                //Поиск дороги
                PathFigure road = null;
                var edges = RoadGraph.GetEdgesFrom(CheckPointsList[i - 1]).GetEnumerator();
                while (edges.MoveNext())
                {
                    if (edges.Current.End.Equals(CheckPointsList[i]))
                    {
                        road = edges.Current.Data;
                        break;
                    }
                }
                //Если дорога не найдена
                if (road == null)
                {
                    CalculateAngle(n, positions[n], CheckPointsList[i].LocationPoint);
                    if (n == 0)
                    {
                        position = CheckPointsList[i].LocationPoint;
                    }
                    else if (need_draw[0] == false)
                    {
                        position.X += 1;
                        position.Y += 1;
                    }
                    return new Point(-1, -1);
                }
                //Если найдена
                double road_lenght = CalculateLength(road);
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
                        seg_l = CalculateLength(new PathFigure(startPoint, new List<PathSegment>() { seg }, false));
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
                        seg_l = CalculateLength(new PathFigure(startPoint, new List<PathSegment>() { seg }, false));
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
                    CalculateAngle(n, positions[n], new Point(x, y));
                    return new Point(x, y);
                }
            }
            return new Point(-1, -1);
        }

        public override void Step()
        {
            try
            {
                if (position == new Point(-1, -1))
                {
                    LocateAgent();
                }
                //Длина пути, которая будет пройдена за один шаг моделирвания
                double step_lenght = (double)Scenario.STEP_TIME_MS / CurrentSpeed;
                if (need_draw[0] == false)
                {
                    currentLength[0] += step_lenght;
                    CalculateOtherCarrigesPosition();
                    return;
                }
                double tmp = currentLength[0];
                double route_length = 0;
                for (int i = 1; i < CheckPointsList.Count; i++)
                {
                    //Поиск дороги
                    PathFigure road = null;
                    var edges = RoadGraph.GetEdgesFrom(CheckPointsList[i - 1]).GetEnumerator();
                    while (edges.MoveNext())
                    {
                        if (edges.Current.End.Equals(CheckPointsList[i]))
                        {
                            road = edges.Current.Data;
                            break;
                        }
                    }
                    //Расчет длины маршрута и данного участка
                    double road_length = CalculateLength(road);
                    route_length += road_length;

                    //Если этот участок еще не пройден
                    if (tmp < road_length)
                    {
                        //Если участок не будет пройден за этот шаг
                        if (tmp + step_lenght + Size.X / 2 < road_length)
                        {
                            currentLength[0] += step_lenght;
                            positions[0] = CalculateCoordinateLocationOfCarrige(0);
                            CalculateOtherCarrigesPosition();
                            break;
                        }
                        else if (tmp + step_lenght + Size.X / 2 > road_length)
                        {
                            if (tmp + Size.X / 2 <= road_length)
                            {
                                if (CheckPointsList[i].IsServicePoint)
                                {
                                    //Паркуемся
                                    if (tmp != road_length - Size.X / 2)
                                    {
                                        currentLength[0] += road_length - tmp - Size.X / 2;
                                        positions[0] = CalculateCoordinateLocationOfCarrige(0);
                                        CalculateOtherCarrigesPosition();
                                    }
                                    //Работаем с сервисом
                                    if (!ReadyToIOOperation)
                                    {
                                        ServiceBase service = scenario.ServicesList.Find(delegate(ServiceBase sb) { return sb.ID == CheckPointsList[i].ServiceID; });
                                        if (service.ID == -1)
                                        {
                                            throw new ArgumentException("Service not found, service id = " + CheckPointsList[i].ServiceID);
                                        }
                                        if (service is StopService)
                                        {
                                            (service as StopService).OpenInputPoints(ID);
                                            ReadyToIOOperation = true;
                                        }
                                    }
                                    if (go)
                                    {
                                        ReadyToIOOperation = false;
                                        go = false;
                                    }
                                    if (ReadyToIOOperation)
                                    {
                                        return;
                                    }
                                }
                                //Задержка на маршрутной точке
                                if (CheckPointsList[i].IsWaitPoint)
                                {
                                    if (CheckPointsList[i].MinWait > 0)
                                    {
                                        CheckPointsList[i].MinWait -= Scenario.STEP_TIME_MS;
                                        return;
                                    }
                                }
                                //Точка с присвоением фиксированной скорости
                                if (CheckPointsList[i].IsFixSpeedPoint)
                                {
                                    CurrentSpeed = Convert.ToInt32(CheckPointsList[i].FixSpeed);
                                }
                                else
                                {
                                    CurrentSpeed = MaxSpeed;
                                }
                                if (CheckPointsList.Count > i + 1 && i > 0 && CheckPointsList[i + 1].Equals(CheckPointsList[i - 1]))
                                {
                                    Revers(route_length);
                                }
                            }
                            currentLength[0] += step_lenght;
                            CalculateCoordinateLocationOfCarrige(0);
                            CalculateOtherCarrigesPosition();
                            break;
                        }
                    }
                    else
                    {
                        if (i == CheckPointsList.Count - 1)
                        {
                            need_draw[0] = false;
                            currentLength[0] += step_lenght;
                            CalculateOtherCarrigesPosition();
                        }
                        tmp -= road_length;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error in Thread! Agent ID [{0}]. Error message [{1}]", ID, ex.Message));
                WayPointsList.Clear();
                return;
            }
        }

        private void CalculateAngle(int n, Point first, Point next)
        {
            Vector sub = next - first;
            if (first.Y >= next.Y)
            {
                angles[n] = -90 + Math.Asin(sub.X / Math.Sqrt(Math.Pow(sub.X, 2) + Math.Pow(sub.Y, 2))) / Math.PI * 180;
            }
            else
            {
                angles[n] = 90 - Math.Asin(sub.X / Math.Sqrt(Math.Pow(sub.X, 2) + Math.Pow(sub.Y, 2))) / Math.PI * 180;
            }
        }
    }
}
