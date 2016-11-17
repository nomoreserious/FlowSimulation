using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Enviroment.Model;
using System.Drawing;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Enviroment;
using System.Windows.Media.Media3D;

namespace FlowSimulation.Agents.OldHuman
{
    internal sealed class OldHuman : Contracts.Agents.AgentBase
    {
        private List<GraphNode> _nodes;
        private bool _inService;
        private AgentServiceBase _lastService;

        public OldHuman(Map map, IEnumerable<AgentServiceBase> services)
            : base(map, services)
        { }

        public override void DoStep(double msInterval)
        {
            CurrentSpeed += msInterval;
            while (CurrentSpeed > _maxSpeed)
            {
                if (RouteList.Count == 0)
                {
                    return;
                }
                var nextCheckPoint = RouteList.First();
                Point nextDirection = new Point(-1, -1);
                //Если следующая точка - сервис
                if (nextCheckPoint.IsServicePoint)
                {
                    if (_lastService == null)
                    {
                        //получаем сервис
                        _lastService = _services.FirstOrDefault(s => s.Id == nextCheckPoint.ServiceId);
                        if (_lastService == null)
                            throw new ArgumentNullException("Service {id:" + nextCheckPoint.ServiceId + "} not found");
                    }
                    //Если уже под управлением сервиса
                    if (_inService)
                    {
                        if (_lastService.ContainsAgent(this.Id))
                        {
                            CurrentSpeed = 0;
                            //...то ждем окончания
                            break;
                        }
                        else
                        {
                            //... и по окончании сваливаем
                            _lastService = null;
                            _inService = false;
                            RouteList.Remove(nextCheckPoint);
                            //повторяем цикл, чтобы быстро уйти из зоны обслуживания.
                            CurrentSpeed += _maxSpeed;
                            continue;
                        }
                    }
                    //иначе пытаемся получить направление движения
                    if (!_lastService.TryGetDirection(this, out nextDirection))
                    {
                        CurrentSpeed = 0;
                        break;
                    }
                }
                //Переход между уровнями
                else if (nextCheckPoint.LayerId != LayerId)
                {
                    var positiveResult = false;
                    for (int i = nextCheckPoint.X; i < nextCheckPoint.X + nextCheckPoint.Width; i++)
                    {
                        for (int j = nextCheckPoint.Y; j < nextCheckPoint.Y + nextCheckPoint.Height; j++)
                        {
                            var newPosition = new Point(i, j);
                            if (_map.TryHoldPosition(newPosition, nextCheckPoint.LayerId, Weigth) == true)
                            {
                                _map.ReleasePosition(Position, LayerId, Weigth);
                                Position = newPosition;
                                LayerId = nextCheckPoint.LayerId;
                                positiveResult = true;
                            }
                        }
                    }
                    if (positiveResult)
                    {
                        RouteList.Remove(nextCheckPoint);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    nextDirection = nextCheckPoint.Center;
                }
                //Если еще нет маршрута по графу или его надо перестроить, то строим его (Дейкстра)
                if (_nodes == null || _direction != nextDirection)
                {
                    _direction = nextDirection;
                    _nodes = _map.GetRoute(Position, nextCheckPoint);
                    if (_nodes == null)
                    {
                        RouteList.Remove(nextCheckPoint);
                        continue;
                    }
                }
                if (_nodes.Count == 0)
                {
                    _nodes = null;
                    if (nextCheckPoint.IsServicePoint)
                    {
                        //Переходим под управление сервиса
                        _lastService.AddAgent(this);
                        _inService = true;
                    }
                    //Удаляем пройденную точку
                    RouteList.Remove(nextCheckPoint);
                    continue;
                }
                //----------------------------------------------------------
                var nextNode = _nodes.First().SourceWP;
                if (Position.X < nextNode.X || Position.X >= nextNode.X + nextNode.Width
                    || Position.Y < nextNode.Y || Position.Y >= nextNode.Y + nextNode.Height)
                {
                    Point p = new Point(Position.X - 1, Position.Y - 1);
                    var rand = new Random();

                    #region *** 1 ***
                    if (Position.X >= nextNode.X + nextNode.Width && Position.Y >= nextNode.Y + nextNode.Height)
                    {
                        if (rand.NextDouble() > 0.5)
                        {
                            if (_map.TryHoldPosition(new Point(p.X, p.Y), LayerId, Weigth) == true)
                                p.Offset(0, 0);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y + 1), LayerId, Weigth) == true)
                                p.Offset(0, 1);
                            else if (_map.TryHoldPosition(new Point(p.X + 1, p.Y), LayerId, Weigth) == true)
                                p.Offset(1, 0);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(0, 2);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y), LayerId, Weigth) == true)
                                p.Offset(2, 0);
                            else
                                p = new Point(-1, -1);
                        }
                        else
                        {
                            if (_map.TryHoldPosition(new Point(p.X, p.Y), LayerId, Weigth) == true)
                                p.Offset(0, 0);
                            else if (_map.TryHoldPosition(new Point(p.X + 1, p.Y), LayerId, Weigth) == true)
                                p.Offset(1, 0);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y + 1), LayerId, Weigth) == true)
                                p.Offset(0, 1);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y), LayerId, Weigth) == true)
                                p.Offset(2, 0);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(0, 2);

                            else
                                p = new Point(-1, -1);
                        }
                    }
                    #endregion
                    #region *** 2 ***
                    else if (Position.X >= nextNode.X && Position.X < nextNode.X + nextNode.Width && Position.Y >= nextNode.Y + nextNode.Height)
                    {
                        if (_map.TryHoldPosition(new Point(p.X + 1, p.Y), LayerId, Weigth) == true)
                            p.Offset(1, 0);
                        else if (_map.TryHoldPosition(new Point(p.X, p.Y), LayerId, Weigth) == true)
                            p.Offset(0, 0);
                        else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y), LayerId, Weigth) == true)
                            p.Offset(2, 0);
                        else if (_map.TryHoldPosition(new Point(p.X, p.Y + 1), LayerId, Weigth) == true)
                            p.Offset(0, 1);
                        else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 1), LayerId, Weigth) == true)
                            p.Offset(2, 1);
                        else
                            p = new Point(-1, -1);
                    }
                    #endregion
                    #region *** 3 ***
                    else if (Position.X < nextNode.X && Position.Y >= nextNode.Y + nextNode.Height)
                    {

                        if (rand.NextDouble() > 0.5)
                        {
                            if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 1), LayerId, Weigth) == true)
                                p.Offset(2, 1);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y), LayerId, Weigth) == true)
                                p.Offset(2, 0);
                            else if (_map.TryHoldPosition(new Point(p.X + 1, p.Y), LayerId, Weigth) == true)
                                p.Offset(1, 0);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(2, 2);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y), LayerId, Weigth) == true)
                                p.Offset(0, 0);
                            else
                                p = new Point(-1, -1);
                        }
                        else
                        {
                            if (_map.TryHoldPosition(new Point(p.X + 1, p.Y), LayerId, Weigth) == true)
                                p.Offset(1, 0);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y), LayerId, Weigth) == true)
                                p.Offset(2, 0);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 1), LayerId, Weigth) == true)
                                p.Offset(2, 1);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y), LayerId, Weigth) == true)
                                p.Offset(0, 0);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(2, 2);
                            else
                                p = new Point(-1, -1);
                        }
                    }
                    #endregion
                    #region *** 4 ***
                    else if (Position.X >= nextNode.X + nextNode.Width && Position.Y >= nextNode.Y && Position.Y < nextNode.Y + nextNode.Height)
                    {
                        if (_map.TryHoldPosition(new Point(p.X, p.Y + 1), LayerId, Weigth) == true)
                            p.Offset(0, 1);
                        else if (_map.TryHoldPosition(new Point(p.X, p.Y), LayerId, Weigth) == true)
                            p.Offset(0, 0);
                        else if (_map.TryHoldPosition(new Point(p.X, p.Y + 2), LayerId, Weigth) == true)
                            p.Offset(0, 2);
                        else if (_map.TryHoldPosition(new Point(p.X + 1, p.Y), LayerId, Weigth) == true)
                            p.Offset(1, 0);
                        else if (_map.TryHoldPosition(new Point(p.X + 1, p.Y + 2), LayerId, Weigth) == true)
                            p.Offset(1, 2);
                        else
                            p = new Point(-1, -1);
                    }
                    #endregion
                    #region *** 5 ***
                    else if (Position.X < nextNode.X && Position.Y >= nextNode.Y && Position.Y < nextNode.Y + nextNode.Height)
                    {
                        if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 1), LayerId, Weigth) == true)
                            p.Offset(2, 1);
                        else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y), LayerId, Weigth) == true)
                            p.Offset(2, 0);
                        else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 2), LayerId, Weigth) == true)
                            p.Offset(2, 2);
                        else if (_map.TryHoldPosition(new Point(p.X + 1, p.Y), LayerId, Weigth) == true)
                            p.Offset(1, 0);
                        else if (_map.TryHoldPosition(new Point(p.X + 1, p.Y + 2), LayerId, Weigth) == true)
                            p.Offset(1, 2);
                        else
                            p = new Point(-1, -1);
                    }
                    #endregion
                    #region *** 6 ***
                    else if (Position.X >= nextNode.X + nextNode.Width && Position.Y < nextNode.Y)
                    {

                        if (rand.NextDouble() > 0.5)
                        {
                            if (_map.TryHoldPosition(new Point(p.X, p.Y + 1), LayerId, Weigth) == true)
                                p.Offset(0, 1);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(0, 2);
                            else if (_map.TryHoldPosition(new Point(p.X + 1, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(1, 2);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y), LayerId, Weigth) == true)
                                p.Offset(0, 0);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(2, 2);
                            else
                                p = new Point(-1, -1);
                        }
                        else
                        {
                            if (_map.TryHoldPosition(new Point(p.X + 1, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(1, 2);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(0, 2);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y + 1), LayerId, Weigth) == true)
                                p.Offset(0, 1);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(2, 2);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y), LayerId, Weigth) == true)
                                p.Offset(0, 0);
                            else
                                p = new Point(-1, -1);
                        }
                    }
                    #endregion
                    #region *** 7 ***
                    else if (Position.X >= nextNode.X && Position.X < nextNode.X + nextNode.Width && Position.Y < nextNode.Y)
                    {
                        if (_map.TryHoldPosition(new Point(p.X + 1, p.Y + 2), LayerId, Weigth) == true)
                            p.Offset(1, 2);
                        else if (_map.TryHoldPosition(new Point(p.X, p.Y + 2), LayerId, Weigth) == true)
                            p.Offset(0, 2);
                        else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 2), LayerId, Weigth) == true)
                            p.Offset(2, 2);
                        else if (_map.TryHoldPosition(new Point(p.X + 0, p.Y + 1), LayerId, Weigth) == true)
                            p.Offset(0, 1);
                        else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 1), LayerId, Weigth) == true)
                            p.Offset(2, 1);
                        else
                            p = new Point(-1, -1);
                    }
                    #endregion
                    #region *** 8 ***
                    else if (Position.X < nextNode.X && Position.Y < nextNode.Y)
                    {
                        if (rand.NextDouble() > 0.5)
                        {
                            if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 1), LayerId, Weigth) == true)
                                p.Offset(2, 1);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(2, 2);
                            else if (_map.TryHoldPosition(new Point(p.X + 1, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(1, 2);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 0), LayerId, Weigth) == true)
                                p.Offset(2, 0);
                            else if (_map.TryHoldPosition(new Point(p.X + 0, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(0, 2);
                            else
                                p = new Point(-1, -1);
                        }
                        else
                        {
                            if (_map.TryHoldPosition(new Point(p.X + 1, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(1, 2);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(2, 2);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y + 1), LayerId, Weigth) == true)
                                p.Offset(2, 1);
                            else if (_map.TryHoldPosition(new Point(p.X, p.Y + 2), LayerId, Weigth) == true)
                                p.Offset(0, 2);
                            else if (_map.TryHoldPosition(new Point(p.X + 2, p.Y), LayerId, Weigth) == true)
                                p.Offset(2, 0);
                            else
                                p = new Point(-1, -1);
                        }
                    }
                    #endregion

                    if (p != new Point(-1, -1) && p != new Point(Position.X - 1, Position.Y - 1))
                    {
                        _map.ReleasePosition(Position, LayerId, Weigth);
                        Position = p;
                    }
                }
                else
                {
                    var rem = _nodes.First();
                    _nodes.Remove(rem);
                    continue;
                }

                CurrentSpeed -= _maxSpeed;
            }
        }

        public override void DoStepAsync(double msInterval)
        {
            throw new NotImplementedException();
        }

        public override void Initialize(Dictionary<string, object> settings)
        {
            Random rand = new Random();
            var size = Enviroment.Constants.CELL_SIZE * (0.75 + rand.NextDouble() / 4.0);
            Size = new Size3D(size, size, size * 2.5);
            _maxSpeed = 360 + (rand.NextDouble() - 0.7) * 100;
            _acceleration = 0;
            _deceleration = 0;
            _direction = null;
        }
    }
}
