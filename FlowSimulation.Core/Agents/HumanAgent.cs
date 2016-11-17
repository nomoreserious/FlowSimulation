using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using FlowSimulation.SimulationScenario;
using FlowSimulation.Service;

namespace FlowSimulation.Agents
{
    class HumanAgent : AgentBase
    {
        public Point RouteDirection = new Point(-1, -1); 
        public double RouteLenght { get; set; }
        public uint RouteTime { get; set; }

        public HumanAgent(int id, Scenario scenario, int group, int speed) : base(id, scenario, group, speed) { }

        private bool LocateAgent(bool step)
        {
            if (WayPointsList.Count < 2)
            {
                Abort();
                return false;
            }
            byte[,] mas = scenario.map.GetMapFrame(WayPointsList[0].X, WayPointsList[0].Y, WayPointsList[0].PointWidth, WayPointsList[0].PointHeight);
            Random rand = new Random();
            //10 попыток на размещение в точке появления
            int tryes = 10;
            while (tryes > 0)
            {
                tryes--;
                int i = rand.Next(0, mas.GetLength(0) - 1);
                int j = rand.Next(0, mas.GetLength(1) - 1);
                if ((mas[i, j] & 0xC0) == 0x00)
                {
                    cell = new Point(WayPointsList[0].X + i, WayPointsList[0].Y + j);
                    //position = new System.Windows.Point(WayPointsList[0].X + i, WayPointsList[0].Y + j);
                    break;
                }
                if (!step)
                {
                    Sleep(Convert.ToInt32(MaxSpeed / SpeedRatio / 2));
                }
            }
            if (tryes <= 0)
            {
                if (!step)
                {
                    WayPointsList.Clear();
                }
                return false;
            }
            return true;
        }

        public override void Life()
        {
            if (cell == new Point(-1,-1))
            {
                //Поиск позиции размещения
                if (!LocateAgent(false))
                {
                    lifeThread.Abort();
                    return;
                }
            }
            //Движение
            byte[,] mas;
            int realSpeed = MaxSpeed;
            while (WayPointsList.Count != 0)
            {
                while (Math.Abs(cell.X - WayPointsList[0].X) > 3 || Math.Abs(cell.Y - WayPointsList[0].Y) > 3)
                //while (Math.Abs(cell.X - (WayPointsList[0].X + WayPointsList[0].PointWidth / 2)) > WayPointsList[0].PointWidth / 2 || Math.Abs(cell.Y - (WayPointsList[0].Y + WayPointsList[0].PointHeight / 2)) > WayPointsList[0].PointHeight / 2)
                ////while (Math.Abs(position.X - WayPointsList[0].X) > WayPointsList[0].PointWidth || Math.Abs(position.Y - WayPointsList[0].Y) > WayPointsList[0].PointHeight)
                {
                    if (_break)
                    {
                        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                        lifeThread.Abort();
                        return;
                    }
                    if (lifeThread.ThreadState == ThreadState.AbortRequested || lifeThread.ThreadState == ThreadState.StopRequested)
                    {
                        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                        return;
                    }
                    Point prePosition = cell;
                    Point point = new Point();

                    //Thread.Sleep(Convert.ToInt32(Speed/SpeedRatio));

                    //double sp = 1;
                    //double gip = Math.Sqrt(Math.Pow(realPosition.X - (WayPointsList[0].X + WayPointsList[0].PointWidth / 2), 2) + Math.Pow(realPosition.Y - (WayPointsList[0].Y + WayPointsList[0].PointHeight / 2), 2));
                    //double cos = (WayPointsList[0].X - realPosition.X - WayPointsList[0].PointWidth / 2) / gip;
                    //double sin = (WayPointsList[0].Y - realPosition.Y - WayPointsList[0].PointHeight / 2) / gip;

                    //Point nextPoint = new Point((int)Math.Round(realPosition.X + cos * sp, MidpointRounding.AwayFromZero), (int)Math.Round(realPosition.Y + sin * sp, MidpointRounding.AwayFromZero));
                    //if (nextPoint.Equals(position))
                    //{
                    //    position = nextPoint;
                    //    realPosition.Offset(cos * sp, sin * sp);
                    //}
                    //else
                    //{
                    //    if (scenario.map.SetMapCellTake(true, nextPoint.X, nextPoint.Y))
                    //    {
                    //        scenario.map.SetMapCellTake(false, position.X, position.Y);
                    //        position = nextPoint;
                    //        realPosition.Offset(cos * sp, sin * sp);
                    //    }
                    //}
                    //scenario.map.SetMapCellLock(false, position.X, position.Y);

                    #region *** 1 ***
                    if (cell.X > WayPointsList[0].X && cell.Y > WayPointsList[0].Y)
                    {
                        point = new Point(cell.X - 2, cell.Y - 2);
                        mas = scenario.map.GetAndLockMapFrame(point.X, point.Y, 3, 3);
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X)) / (Math.Abs(cell.Y - WayPointsList[0].Y) + Math.Abs(cell.X - WayPointsList[0].X)) > new Random().NextDouble())
                        {
                            if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(-1, 0);
                                }
                            }
                            //else if ((mas[1, 1] & 0xC0) == 0x00)
                            //{
                            //    if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y - 1))
                            //    {
                            //        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                            //        this.cell.Offset(-1, -1);
                            //    }
                            //}
                            else if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X, cell.Y - 1))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(0, -1);
                                }
                            }
                        }
                        else
                        {
                            if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X, cell.Y - 1))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(0, -1);
                                }
                            }
                            //else if ((mas[1, 1] & 0xC0) == 0x00)
                            //{
                            //    if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y - 1))
                            //    {
                            //        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                            //        this.cell.Offset(-1, -1);
                            //    }
                            //}
                            else if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(-1, 0);
                                }
                            }
                        }
                    }
                    #endregion
                    #region *** 2 ***
                    if (cell.X == WayPointsList[0].X && cell.Y > WayPointsList[0].Y)
                    {
                        point = new Point(cell.X - 1, cell.Y - 2);
                        mas = scenario.map.GetAndLockMapFrame(point.X, point.Y, 3, 3);
                        if ((mas[1, 1] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X, cell.Y - 1))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(0, -1);
                            }
                        }
                        else if ((mas[0, 1] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y - 1))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(-1, -1);
                            }
                        }
                        else if ((mas[2, 1] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y - 1))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(1, -1);
                            }
                        }
                    }
                    #endregion
                    #region *** 3 ***
                    if (cell.X < WayPointsList[0].X && cell.Y > WayPointsList[0].Y)
                    {
                        point = new Point(cell.X, cell.Y - 2);
                        mas = scenario.map.GetAndLockMapFrame(point.X, point.Y, 3, 3);
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X)) / (Math.Abs(cell.Y - WayPointsList[0].Y) + Math.Abs(cell.X - WayPointsList[0].X)) > new Random().NextDouble())
                        {
                            if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(1, 0);
                                }
                            }
                            //else if ((mas[1, 1] & 0xC0) == 0x00)
                            //{
                            //    if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y - 1))
                            //    {
                            //        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                            //        this.cell.Offset(1, -1);
                            //    }
                            //}
                            else if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X, cell.Y - 1))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(0, -1);
                                }
                            }
                        }
                        else
                        {
                            if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X, cell.Y - 1))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(0, -1);
                                }
                            }
                            //else if ((mas[1, 1] & 0xC0) == 0x00)
                            //{
                            //    if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y - 1))
                            //    {
                            //        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                            //        this.cell.Offset(1, -1);
                            //    }
                            //}
                            else if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(1, 0);
                                }
                            }
                        }
                    }
                    #endregion
                    #region *** 4 ***
                    if (cell.X > WayPointsList[0].X && cell.Y == WayPointsList[0].Y)
                    {
                        point = new Point(cell.X - 2, cell.Y - 1);
                        mas = scenario.map.GetAndLockMapFrame(point.X, point.Y, 3, 3);
                        if ((mas[1, 1] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(-1, 0);
                            }
                        }
                        else if ((mas[1, 0] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y - 1))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(-1, -1);
                            }
                        }
                        else if ((mas[1, 2] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y + 1))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(-1, 1);
                            }
                        }
                    }
                    #endregion
                    #region *** 5 ***
                    if (cell.X < WayPointsList[0].X && cell.Y == WayPointsList[0].Y)
                    {
                        point = new Point(cell.X, cell.Y - 1);
                        mas = scenario.map.GetAndLockMapFrame(point.X, point.Y, 3, 3);
                        if ((mas[1, 1] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(1, 0);
                            }
                        }
                        else if ((mas[1, 0] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y - 1))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(1, -1);
                            }
                        }
                        else if ((mas[1, 2] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y + 1))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(1, 1);
                            }
                        }
                    }
                    #endregion
                    #region *** 6 ***
                    if (cell.X > WayPointsList[0].X && cell.Y < WayPointsList[0].Y)
                    {
                        point = new Point(cell.X - 2, cell.Y);
                        mas = scenario.map.GetAndLockMapFrame(point.X, point.Y, 3, 3);
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X)) / (Math.Abs(cell.Y - WayPointsList[0].Y) + Math.Abs(cell.X - WayPointsList[0].X)) > new Random().NextDouble())
                        {
                            if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(-1, 0);
                                }
                            }
                            //else if ((mas[1, 1] & 0xC0) == 0x00)
                            //{
                            //    if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y + 1))
                            //    {
                            //        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                            //        this.cell.Offset(-1, 1);
                            //    }
                            //}
                            else if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X, cell.Y + 1))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(0, 1);
                                }
                            }
                        }
                        else
                        {
                            if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X, cell.Y + 1))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(0, 1);
                                }
                            }
                            //else if ((mas[1, 1] & 0xC0) == 0x00)
                            //{
                            //    if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y + 1))
                            //    {
                            //        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                            //        this.cell.Offset(-1, 1);
                            //    }
                            //}
                            else if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(-1, 0);
                                }
                            }
                        }
                    }
                    #endregion
                    #region *** 7 ***
                    if (cell.X == WayPointsList[0].X && cell.Y < WayPointsList[0].Y)
                    {
                        point = new Point(cell.X - 1, cell.Y);
                        mas = scenario.map.GetAndLockMapFrame(point.X, point.Y, 3, 3);
                        if ((mas[1, 1] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X, cell.Y + 1))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(0, 1);
                            }
                        }
                        else if ((mas[0, 1] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X - 1, cell.Y + 1))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(-1, 1);
                            }
                        }
                        else if ((mas[2, 1] & 0xC0) == 0x00)
                        {
                            if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y + 1))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell.Offset(1, 1);
                            }
                        }
                    }
                    #endregion
                    #region *** 8 ***
                    if (cell.X < WayPointsList[0].X && cell.Y < WayPointsList[0].Y)
                    {
                        point = new Point(cell.X, cell.Y);
                        mas = scenario.map.GetAndLockMapFrame(point.X, point.Y, 3, 3);
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X)) / (Math.Abs(cell.Y - WayPointsList[0].Y) + Math.Abs(cell.X - WayPointsList[0].X)) > new Random().NextDouble())
                        {
                            if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(1, 0);
                                }
                            }
                            //else if ((mas[1, 1] & 0xC0) == 0x00)
                            //{
                            //    if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y + 1))
                            //    {
                            //        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                            //        this.cell.Offset(1, 1);
                            //    }
                            //}
                            else if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X, cell.Y + 1))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(0, 1);
                                }
                            }
                        }
                        else
                        {
                            if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X, cell.Y + 1))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(0, 1);
                                }
                            }
                            //else if ((mas[1, 1] & 0xC0) == 0x00)
                            //{
                            //    if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y + 1))
                            //    {
                            //        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                            //        this.cell.Offset(1, 1);
                            //    }
                            //}
                            else if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                if (scenario.map.SetMapCellTake(true, cell.X + 1, cell.Y))
                                {
                                    scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                    this.cell.Offset(1, 0);
                                }
                            }
                        }
                    }
                    #endregion

                    RouteTime += (uint)realSpeed;

                    if (cell != prePosition)
                    {
                        if (cell.X != prePosition.X && cell.Y != prePosition.Y)
                        {
                            RouteLenght += Math.Sqrt(2 * MapOld.CellSize * MapOld.CellSize);
                        }
                        else
                        {
                            RouteLenght += MapOld.CellSize;
                        }
                    }

                    scenario.map.SetMapFrameLock(false, point.X, point.Y, 3, 3);
                    Sleep(Convert.ToInt32(realSpeed / SpeedRatio));
                }
                if (WayPointsList.Count > 0)
                {
                    WayPointsList.RemoveAt(0);
                }
            }
            scenario.map.SetMapCellTake(false, cell.X, cell.Y);
            Abort();
        }

        public override void Step()
        {
            try
            {
                if (cell == new Point(-1, -1))
                {
                    //Поиск позиции размещения
                    if (!LocateAgent(true))
                    {
                        return;
                    }
                }
                //Движение
                byte[,] mas;

                if (WayPointsList.Count == 0)
                {
                    Remove();
                    return;
                }
                this.RealSpeed += Scenario.STEP_TIME_MS;
                if (RealSpeed < CurrentSpeed)
                {
                    return;
                }
                else
                {
                    RealSpeed -= CurrentSpeed;
                }
                if (WayPointsList[0].IsServicePoint)
                {
                    ServiceBase service = scenario.ServicesList.Find(delegate(ServiceBase sb) { return sb.ID == WayPointsList[0].ServiceID; });
                    if (service == null)
                    {
                        throw new ArgumentException("Service not found, service id = " + WayPointsList[0].ServiceID);
                    }
                    if (service.GetServedState(ID) == ServiceBase.ServedState.NotInService)
                    {
                        service.AddAgentToService(ID);
                    }
                    WayPointsList[0] = service.GetAgentDirection(ID, position);
                }
                if (cell.X < WayPointsList[0].X || cell.X >= WayPointsList[0].X + WayPointsList[0].PointWidth || cell.Y < WayPointsList[0].Y || cell.Y >= WayPointsList[0].Y + WayPointsList[0].PointHeight)
                {
                    Point point = new Point(cell.X - 1, cell.Y - 1);
                    mas = scenario.map.GetAndLockMapFrame(point.X, point.Y, 3, 3);

                    #region *** 1 ***
                    if (cell.X >= WayPointsList[0].X + WayPointsList[0].PointWidth && cell.Y >= WayPointsList[0].Y + WayPointsList[0].PointHeight)
                    {
                        Point sd = point;
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) / (Math.Abs(cell.Y - WayPointsList[0].Y - WayPointsList[0].PointHeight / 2) + Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) > new Random().NextDouble())
                        {
                            if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 1);
                            }
                            else if ((mas[0, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 0);
                            }
                            else if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 0);
                            }
                            else if ((mas[0, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 2);
                            }
                            else if ((mas[2, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 0);
                            }
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        else
                        {
                            if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 0);
                            }
                            else if ((mas[0, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 0);
                            }
                            else if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 1);
                            }
                            else if ((mas[2, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 0);
                            }
                            else if ((mas[0, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 2);
                            }
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        if (sd != new Point(-1, -1) && sd != prePosition)
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                prePosition = cell;
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 2 ***
                    if (cell.X >= WayPointsList[0].X && cell.X < WayPointsList[0].X + WayPointsList[0].PointWidth && cell.Y >= WayPointsList[0].Y + WayPointsList[0].PointHeight)
                    {
                        Point sd = point;
                        if ((mas[1, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(1, 0);
                        }
                        else if ((mas[0, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 0);
                        }
                        else if ((mas[2, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 0);
                        }
                        else if ((mas[0, 1] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 1);
                        }
                        else if ((mas[2, 1] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 1);
                        }
                        else
                        {
                            sd = new Point(-1, -1);
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1) && sd != prePosition)
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                prePosition = cell;
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 3 ***
                    if (cell.X < WayPointsList[0].X && cell.Y >= WayPointsList[0].Y + WayPointsList[0].PointHeight)
                    {
                        Point sd = point;
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) / (Math.Abs(cell.Y - WayPointsList[0].Y - WayPointsList[0].PointHeight / 2) + Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) > new Random().NextDouble())
                        {
                            if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 1);
                            }
                            else if ((mas[2, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 0);
                            }
                            else if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 0);
                            }
                            else if ((mas[2, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 2);
                            }
                            else if ((mas[0, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 0);
                            }
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        else
                        {
                            if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 0);
                            }
                            else if ((mas[2, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 0);
                            }
                            else if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 1);
                            }
                            else if ((mas[0, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 0);
                            }
                            else if ((mas[2, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 2);
                            }
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1) && sd != prePosition)
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                prePosition = cell;
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 4 ***
                    if (cell.X >= WayPointsList[0].X + WayPointsList[0].PointWidth && cell.Y >= WayPointsList[0].Y && cell.Y < WayPointsList[0].Y + WayPointsList[0].PointHeight)
                    {
                        Point sd = point;
                        if ((mas[0, 1] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 1);
                        }
                        else if ((mas[0, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 0);
                        }
                        else if ((mas[0, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 2);
                        }
                        else if ((mas[1, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(1, 0);
                        }
                        else if ((mas[1, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(1, 2);
                        }
                        else
                        {
                            sd = new Point(-1, -1);
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1) && sd != prePosition)
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                prePosition = cell;
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 5 ***
                    if (cell.X < WayPointsList[0].X && cell.Y >= WayPointsList[0].Y && cell.Y < WayPointsList[0].Y + WayPointsList[0].PointHeight)
                    {
                        Point sd = point;
                        if ((mas[2, 1] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 1);
                        }
                        else if ((mas[2, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 0);
                        }
                        else if ((mas[2, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 2);
                        }
                        else if ((mas[1, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(1, 02);
                        }
                        else if ((mas[1, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(1, 2);
                        }
                        else
                        {
                            sd = new Point(-1, -1);
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1) && sd != prePosition)
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                prePosition = cell;
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 6 ***
                    if (cell.X >= WayPointsList[0].X + WayPointsList[0].PointWidth && cell.Y < WayPointsList[0].Y)
                    {
                        Point sd = point;
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) / (Math.Abs(cell.Y - WayPointsList[0].Y - WayPointsList[0].PointHeight / 2) + Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) > new Random().NextDouble())
                        {
                            if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 1);
                            }
                            else if ((mas[0, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 2);
                            }
                            else if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 2);
                            }
                            else if ((mas[0, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 0);
                            }
                            else if ((mas[2, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 2);
                            }
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        else
                        {
                            if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 2);
                            }
                            else if ((mas[0, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 2);
                            }
                            else if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 1);
                            }
                            else if ((mas[2, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 2);
                            }
                            else if ((mas[0, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 0);
                            }
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1) && sd != prePosition)
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                prePosition = cell;
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 7 ***
                    if (cell.X >= WayPointsList[0].X && cell.X < WayPointsList[0].X + WayPointsList[0].PointWidth && cell.Y < WayPointsList[0].Y)
                    {
                        Point sd = point;
                        if ((mas[1, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(1, 2);
                        }
                        else if ((mas[0, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 2);
                        }
                        else if ((mas[2, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 2);
                        }
                        else if ((mas[0, 1] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 1);
                        }
                        else if ((mas[2, 1] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 1);
                        }
                        else
                        {
                            sd = new Point(-1, -1);
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1) && sd != prePosition)
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                prePosition = cell;
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 8 ***
                    if (cell.X < WayPointsList[0].X && cell.Y < WayPointsList[0].Y)
                    {
                        Point sd = point;
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) / (Math.Abs(cell.Y - WayPointsList[0].Y - WayPointsList[0].PointHeight / 2) + Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) > new Random().NextDouble())
                        {
                            if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 1);
                            }
                            else if ((mas[2, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 2);
                            }
                            else if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 2);
                            }
                            else if ((mas[2, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 0);
                            }
                            else if ((mas[0, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 2);
                            }
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        else
                        {
                            if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 2);
                            }
                            else if ((mas[2, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 2);
                            }
                            else if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 1);
                            }
                            else if ((mas[0, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 2);
                            }
                            else if ((mas[2, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 0);
                            }
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1) && sd != prePosition)
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                prePosition = cell;
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion

                    #region its work
                    /*
                      #region *** 1 ***
                    if (cell.X >= WayPointsList[0].X + WayPointsList[0].PointWidth && cell.Y >= WayPointsList[0].Y + WayPointsList[0].PointHeight)
                    {
                        Point sd = point;
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) / (Math.Abs(cell.Y - WayPointsList[0].Y - WayPointsList[0].PointHeight / 2) + Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) > new Random().NextDouble())
                        {
                            if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 1);
                            }
                            else if ((mas[0, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 0);
                            }
                            else if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 0);
                            }
                            //else if ((mas[2, 0] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(2, 0);
                            //}
                            //else if ((mas[0, 2] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(1, 0);
                            //}
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        else
                        {
                            if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 0);
                            }
                            else if ((mas[0, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 0);
                            }
                            else if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 1);
                            }
                            //else if ((mas[0, 2] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(0, 2);
                            //}
                            //else if ((mas[2, 0] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(2, 0);
                            //}
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        if (sd != new Point(-1, -1))
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 2 ***
                    if (cell.X >= WayPointsList[0].X && cell.X < WayPointsList[0].X + WayPointsList[0].PointWidth && cell.Y >= WayPointsList[0].Y + WayPointsList[0].PointHeight)
                    {
                        Point sd = point;
                        if ((mas[1, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(1, 0);
                        }
                        else if ((mas[0, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 0);
                        }
                        else if ((mas[2, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 0);
                        }
                        //else if ((mas[0, 1] & 0xC0) == 0x00)
                        //{
                        //    sd.Offset(0, 1);
                        //}
                        //else if ((mas[2, 1] & 0xC0) == 0x00)
                        //{
                        //    sd.Offset(2, 1);
                        //}
                        else
                        {
                            sd = new Point(-1, -1);
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1))
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 3 ***
                    if (cell.X < WayPointsList[0].X && cell.Y >= WayPointsList[0].Y + WayPointsList[0].PointHeight)
                    {
                        Point sd = point;
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) / (Math.Abs(cell.Y - WayPointsList[0].Y - WayPointsList[0].PointHeight / 2) + Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) > new Random().NextDouble())
                        {
                            if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 1);
                            }
                            else if ((mas[2, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 0);
                            }
                            else if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 0);
                            }
                            //else if ((mas[2, 2] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(2, 2);
                            //}
                            //else if ((mas[0, 0] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(0, 0);
                            //}
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        else
                        {
                            if ((mas[1, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 0);
                            }
                            else if ((mas[2, 0] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 0);
                            }
                            else if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 1);
                            }
                            //else if ((mas[0, 0] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(0, 0);
                            //}
                            //else if ((mas[2, 2] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(2, 2);
                            //}
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1))
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 4 ***
                    if (cell.X >= WayPointsList[0].X + WayPointsList[0].PointWidth && cell.Y >= WayPointsList[0].Y && cell.Y < WayPointsList[0].Y + WayPointsList[0].PointHeight)
                    {
                        Point sd = point;
                        if ((mas[0, 1] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 1);
                        }
                        else if ((mas[0, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 0);
                        }
                        else if ((mas[0, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 2);
                        }
                        //else if ((mas[1, 0] & 0xC0) == 0x00)
                        //{
                        //    sd.Offset(1, 0);
                        //}
                        //else if ((mas[1, 2] & 0xC0) == 0x00)
                        //{
                        //    sd.Offset(1, 2);
                        //}
                        else
                        {
                            sd = new Point(-1, -1);
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1))
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 5 ***
                    if (cell.X < WayPointsList[0].X && cell.Y >= WayPointsList[0].Y && cell.Y < WayPointsList[0].Y + WayPointsList[0].PointHeight)
                    {
                        Point sd = point;
                        if ((mas[2, 1] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 1);
                        }
                        else if ((mas[2, 0] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 0);
                        }
                        else if ((mas[2, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 2);
                        }
                        //else if ((mas[1, 0] & 0xC0) == 0x00)
                        //{
                        //    sd.Offset(1, 02);
                        //}
                        //else if ((mas[1, 2] & 0xC0) == 0x00)
                        //{
                        //    sd.Offset(1, 2);
                        //}
                        else
                        {
                            sd = new Point(-1, -1);
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1))
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 6 ***
                    if (cell.X >= WayPointsList[0].X + WayPointsList[0].PointWidth && cell.Y < WayPointsList[0].Y)
                    {
                        Point sd = point;
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) / (Math.Abs(cell.Y - WayPointsList[0].Y - WayPointsList[0].PointHeight / 2) + Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) > new Random().NextDouble())
                        {
                            if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 1);
                            }
                            else if ((mas[0, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 2);
                            }
                            else if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 2);
                            }
                            //else if ((mas[0, 2] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(0, 2);
                            //}
                            //else if ((mas[2, 2] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(2, 2);
                            //}
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        else
                        {
                            if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 2);
                            }
                            else if ((mas[0, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 2);
                            }
                            else if ((mas[0, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(0, 1);
                            }
                            //else if ((mas[2, 2] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(2, 2);
                            //}
                            //else if ((mas[0, 2] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(0, 2);
                            //}
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1))
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 7 ***
                    if (cell.X >= WayPointsList[0].X && cell.X < WayPointsList[0].X + WayPointsList[0].PointWidth && cell.Y < WayPointsList[0].Y)
                    {
                        Point sd = point;
                        if ((mas[1, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(1, 2);
                        }
                        else if ((mas[0, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(0, 2);
                        }
                        else if ((mas[2, 2] & 0xC0) == 0x00)
                        {
                            sd.Offset(2, 2);
                        }
                        //else if ((mas[0, 1] & 0xC0) == 0x00)
                        //{
                        //    sd.Offset(0, 1);
                        //}
                        //else if ((mas[2, 1] & 0xC0) == 0x00)
                        //{
                        //    sd.Offset(2, 1);
                        //}
                        else
                        {
                            sd = new Point(-1, -1);
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1))
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                    #region *** 8 ***
                    if (cell.X < WayPointsList[0].X && cell.Y < WayPointsList[0].Y)
                    {
                        Point sd = point;
                        if (Convert.ToDouble(Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) / (Math.Abs(cell.Y - WayPointsList[0].Y - WayPointsList[0].PointHeight / 2) + Math.Abs(cell.X - WayPointsList[0].X - WayPointsList[0].PointWidth / 2)) > new Random().NextDouble())
                        {
                            if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 1);
                            }
                            else if ((mas[2, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 2);
                            }
                            else if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 2);
                            }
                            //else if ((mas[2, 0] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(2, 0);
                            //}
                            //else if ((mas[0, 2] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(0, 2);
                            //}
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        else
                        {
                            if ((mas[1, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(1, 2);
                            }
                            else if ((mas[2, 2] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 2);
                            }
                            else if ((mas[2, 1] & 0xC0) == 0x00)
                            {
                                sd.Offset(2, 1);
                            }
                            //else if ((mas[0, 2] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(0, 2);
                            //}
                            //else if ((mas[2, 0] & 0xC0) == 0x00)
                            //{
                            //    sd.Offset(2, 0);
                            //}
                            else
                            {
                                sd = new Point(-1, -1);
                            }
                        }
                        //Try to take cell
                        if (sd != new Point(-1, -1))
                        {
                            if (scenario.map.SetMapCellTake(true, sd.X, sd.Y))
                            {
                                scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                                this.cell = sd;
                            }
                        }
                    }
                    #endregion
                     */

                    #endregion

                    position = new System.Windows.Point(cell.X, cell.Y);
                    RouteTime += (uint)CurrentSpeed;

                    if (cell != prePosition)
                    {
                        if (cell.X != prePosition.X && cell.Y != prePosition.Y)
                        {
                            RouteLenght += Math.Sqrt(2) * MapOld.CellSize;
                        }
                        else
                        {
                            RouteLenght += MapOld.CellSize;
                        }
                    }
                    scenario.map.SetMapFrameLock(false, point.X, point.Y, 3, 3);
                }
                else
                {
                    //Точка входа в сервис
                    if (WayPointsList[0].IsServicePoint)
                    {
                        ServiceBase service = scenario.ServicesList.Find(delegate(ServiceBase sb) { return sb.ID == WayPointsList[0].ServiceID; });
                        if (service.ID == -1)
                        {
                            throw new ArgumentException("Service not found, service id = " + WayPointsList[0].ServiceID);
                        }
                        ServiceBase.ServedState state = service.GetServedState(ID);
                        if (state == ServiceBase.ServedState.MoveToQueue)
                        {
                            service.AddAgentToQueue(ID, position);
                            return;
                        }
                        if (state == ServiceBase.ServedState.InQueue)
                        {
                            return;
                        }
                        if (state == ServiceBase.ServedState.Served)
                        {
                            WayPointsList[0].IsServicePoint = false;
                        }
                    }

                    //Задержка на маршрутной точке
                    if (WayPointsList[0].IsWaitPoint)
                    {
                        if (WayPointsList[0].MinWait > 0)
                        {
                            WayPointsList[0].MinWait -= Scenario.STEP_TIME_MS;
                            RealSpeed = 0;
                            return;
                        }
                    }
                    //Точка с присвоением фиксированной скорости
                    if (WayPointsList[0].IsFixSpeedPoint)
                    {
                        CurrentSpeed = Convert.ToInt32(MapOld.CellSize * 3600 / WayPointsList[0].FixSpeed);
                    }
                    else
                    {
                        CurrentSpeed = MaxSpeed;
                    }
                    WayPointsList.RemoveAt(0);
                    if (WayPointsList.Count == 0)
                    {
                        scenario.map.SetMapCellTake(false, cell.X, cell.Y);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error in Thread! Agent ID [{0}]. Error message [{1}]", ID, ex.Message));
                Remove();
                return;
            }
        }
    }
}
