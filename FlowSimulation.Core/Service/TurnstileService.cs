using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using FlowSimulation.Agents;
using FlowSimulation.Map.Model;

namespace FlowSimulation.Service
{
    public class TurnstileService :ServiceBase
    {
        [XmlElement]
        public PathFigure TurnstileGeometry { get; set; }

        [XmlIgnore]
        private List<List<int>> AgentsQueuesList;

        [XmlIgnore]
        private List<int> Times;

        [XmlArray]
        [XmlArrayItem(typeof(WayPoint))]
        public List<WayPoint> InputPoints { get; set; }

        [XmlAttribute]
        public int TurnstileCount { get; set; }

        /// <summary>
        /// Not parametred constructor
        /// </summary>
        public TurnstileService()
        { }

        public TurnstileService(PathFigure geom)
        {
            if (geom == null)
            {
                throw new ArgumentException("The end point equals the start point", "end");
            }
            TurnstileGeometry = geom;
            int possible_count = (int)Math.Floor((CalculateLength(geom) - 1) / 2);
            TurnstileCount = possible_count;
            InputPoints = new List<WayPoint>(CreateInputPoints());
        }

        public TurnstileService(PathFigure geom, int turnstile_count)
        {
            if (geom == null)
            {
                throw new ArgumentException("The end point equals the start point", "end");
            }
            int possible_count = (int)Math.Floor((CalculateLength(geom) - 1) / 2);
            if (possible_count < turnstile_count)
            {
                throw new ArgumentException("Between the start and the end point is possible only " + possible_count + " turnstiles", "turnstile_count");
            }
            TurnstileGeometry = geom;
            TurnstileCount = turnstile_count;
            InputPoints = new List<WayPoint>(CreateInputPoints());
        }

        internal override void Initialize()
        {
            InputPoints = new List<WayPoint>(CreateInputPoints());
            Point start, end;
            for (int i = 0; i < InputPoints.Count; i++)
            {
                start = i == 0 ? TurnstileGeometry.StartPoint : InputPoints[i - 1].LocationPoint;
                if (i == InputPoints.Count - 1)
                {
                    PathSegment seg = TurnstileGeometry.Segments[TurnstileGeometry.Segments.Count - 1];
                    if (seg is BezierSegment)
                    {
                        end = new Point(Math.Round((seg as BezierSegment).Point3.X, 0), Math.Round((seg as BezierSegment).Point3.Y, 0));
                    }
                    else if (seg is LineSegment)
                    {
                        end = new Point(Math.Round((seg as LineSegment).Point.X, 0), Math.Round((seg as LineSegment).Point.Y, 0));
                    }
                    else if (seg is PolyLineSegment)
                    {
                        end = new Point(Math.Round((seg as PolyLineSegment).Points[(seg as PolyLineSegment).Points.Count - 1].X, 0), Math.Round((seg as PolyLineSegment).Points[(seg as PolyLineSegment).Points.Count - 1].Y, 0));
                    }
                    else
                    {
                        end = InputPoints[i - 1].LocationPoint;
                    }
                }
                else
                {
                    end = InputPoints[i].LocationPoint;
                }
                Vector sub = end - start;

                while(start.X!=end.X || start.Y!=end.Y)
                {
                    if (start.X != end.X)
                    {
                        start.X += sub.X / Math.Abs(sub.X);
                    }
                    if (start.Y != end.Y)
                    {
                        start.Y += sub.Y / Math.Abs(sub.Y);
                    }
                    if (start!=InputPoints[i].LocationPoint)
                    {
                        scenario.map.SetMapFrameFlag(MapOld.CellState.Closed, true, (int)start.X, (int)start.Y, 1, 1);
                    }
                }
            }
            AgentsQueuesList = new List<List<int>>(TurnstileCount);
            for (int i = 0; i < TurnstileCount; i++)
            {
                AgentsQueuesList.Add(new List<int>());
            }
            Times = new List<int>(TurnstileCount);
            for (int i = 0; i < TurnstileCount; i++)
            {
                Times.Add(0);
            }
        }

        public override void DoStep()
        {

        }

        public override WayPoint GetAgentDirection(int agentID, System.Windows.Point position)
        {
            int index = InputPoints.FindIndex(delegate(WayPoint item) { return item.LocationPoint == position; });
            WayPoint wp = new WayPoint();
            if (index != -1)
            {
                wp.LocationPoint = InputPoints[index].LocationPoint;
                AgentsQueuesList.ForEach(delegate(List<int> queue) { queue.Remove(agentID); });
            }
            else
            {
                //index = new Random().Next(0, InputPoints.Count);
                //wp.LocationPoint = InputPoints[index].LocationPoint;

                //Если раскоментить, то очереди будут заполняться ровномерно
                int queue_index = AgentsQueuesList.FindIndex(delegate(List<int> queue) { return queue.Count != 0 && queue.Contains(agentID); });
                if (queue_index == -1)
                {
                    index = 0;
                    int count = AgentsQueuesList[index].Count;
                    for (int i = 1; i < AgentsQueuesList.Count; i++)
                    {
                        if (AgentsQueuesList[i].Count < count)
                        {
                            index = i;
                            count = AgentsQueuesList[i].Count;
                        }
                    }
                    AgentsQueuesList[index].Add(agentID);
                    wp.LocationPoint = InputPoints[index].LocationPoint;
                }
                else
                {
                    wp.LocationPoint = InputPoints[queue_index].LocationPoint;
                }

            }
            wp.PointWidth = 1;
            wp.PointHeight = 1;
            wp.IsWaitPoint = true;
            wp.MinWait = new Random(agentID).Next(MinServedTime, MaxServedTime);
            wp.IsServicePoint = true;
            wp.ServiceID = ID;
            return wp;
        }

        public override bool AddAgentToQueue(int agentID, Point location)
        {
            return true;
        }

        public override ServiceBase.ServedState GetServedState(int agentID)
        {
            if (!agentsList.Contains(agentID))
            {
                return ServedState.NotInService;
            }
            else
            {
                //int index = AgentsQueuesList.FindIndex(delegate(List<int> queue) { return queue.Count != 0 && queue.Contains(agentID); });
                //if (index != -1)
                //{
                //    return ServedState.InQueue;
                //}
                agentsList.Remove(agentID);
                return ServedState.Served;
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

        private Point CalculateCoordinateLocationOfTurnstile(double tmp)
        {
            double x = 0, y = 0, t = 0, seg_l = 0;
            for (int g = 0; g < TurnstileGeometry.Segments.Count; g++)
            {
                Point startPoint = TurnstileGeometry.StartPoint;
                if (TurnstileGeometry.Segments[g] is BezierSegment)
                {
                    BezierSegment seg = TurnstileGeometry.Segments[g] as BezierSegment;
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
                else if (TurnstileGeometry.Segments[g] is LineSegment)
                {
                    LineSegment seg = TurnstileGeometry.Segments[g] as LineSegment;
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
                else if (TurnstileGeometry.Segments[g] is PolyLineSegment)
                {
                    PolyLineSegment seg = TurnstileGeometry.Segments[g] as PolyLineSegment;
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
            }
            return new Point(x, y);
        }

        private WayPoint[] CreateInputPoints()
        {
            WayPoint[] wpl = new WayPoint[TurnstileCount]; 
            double length = CalculateLength(TurnstileGeometry);
            for (int i = 0; i < TurnstileCount; i++)
            {
                double tmp = (i + 1) * length / (TurnstileCount + 1);
                Point coordinate = CalculateCoordinateLocationOfTurnstile(tmp);
                wpl[i] = new WayPoint((int)Math.Floor(coordinate.X), (int)Math.Floor(coordinate.Y), 1, 1)
                {
                    IsWaitPoint = true,
                    MinWait = MinServedTime,
                    MaxWait = MaxServedTime
                };
            }
            return wpl;
        }
    }
}
