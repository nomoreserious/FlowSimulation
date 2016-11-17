using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Media3D;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Enviroment;
using FlowSimulation.Enviroment.Model;

namespace FlowSimulation.Agents.Human
{
    internal sealed class Human : Contracts.Agents.AgentBase
    {
        private List<GraphNode> _nodes;
        private List<Point> _points;
        private bool _inService;
        private AgentServiceBase _lastService;

        private int _tryGetWay = 0;

        public Human(Map map, IEnumerable<AgentServiceBase> services)
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
                Point nextDirection = new Point(-1,-1);
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
                                break;
                            }
                        }
                        if (positiveResult)
                            break;
                    }
                    if (positiveResult)
                        RouteList.Remove(nextCheckPoint);
                    else
                        return;
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
                        bool positionChanged = false;
                        for (int i = nextCheckPoint.X; i < nextCheckPoint.X + nextCheckPoint.Width; i++)
                        {
                            for (int j = nextCheckPoint.Y; j < nextCheckPoint.Y + nextCheckPoint.Height; j++)
                            {
                                var p = new Point(i, j);
                                if (_map.TryHoldPosition(p, nextCheckPoint.LayerId, this.Weigth) == true)
                                {
                                    _map.ReleasePosition(Position, LayerId, Weigth);
                                    Position = p;
                                    positionChanged = true;
                                    break;
                                }
                            }
                            if (positionChanged)
                            {
                                break;
                            }
                        }
                        if (positionChanged)
                        {
                            RouteList.Remove(nextCheckPoint);
                            continue;
                        }
                        else
                        {
                            return;
                        }
                    }
                    _points = null;
                }
                if (_nodes.Count == 0)
                {
                    _nodes = null;
                    //Переходим под управление сервиса
                    if (nextCheckPoint.IsServicePoint)
                    {
                        _lastService.AddAgent(this);
                        _inService = true;
                    }
                    //Удаляем пройденную точку
                    RouteList.Remove(nextCheckPoint);
                    continue;
                }
                //----------------------------------------------------------
                //Берем следующую цель на маршруте и строим путь по точкам (A*)
                var nextNode = _nodes.First();
                if (_points == null)
                {
                    _points = _map.GetWay(Position, nextNode);
                    //Если не удалось построить маршрут, то пробуем еще 3 раза
                    if (_points == null)
                    {
                        _tryGetWay++;
                        if (_tryGetWay > 2)
                        {
                            _nodes.Remove(nextNode);
                            _tryGetWay = 0;
                            continue;
                        }
                        CurrentSpeed = 0;
                        break;
                    }
                }
                if (_points.Count == 0)
                {
                    _nodes.Remove(nextNode);
                    _points = null;
                    continue;
                }
                //Берем следующую клетку маршрута
                var point = _points.First();
                if (point == Position)
                {
                    _points.Remove(point);
                    continue;
                }
                else
                {
                    //Пытаемся занять клетку
                    bool? tryHoldResult = _map.TryHoldPosition(point, LayerId, Weigth);
                    if (!tryHoldResult.HasValue)
                    {
                        //Ждем, пока путь откроется
                        CurrentSpeed = 0;
                        break;
                    }
                    else if (tryHoldResult.Value)
                    {
                        _map.ReleasePosition(Position, LayerId, Weigth);
                        Position = point;
                        if (_points != null)
                        {
                            _points.Remove(point);
                        }
                    }
                    else
                    {
                        //На следующем шаге перестраиваем маршрут
                        _points = null;   
                    }
                    CurrentSpeed -= _maxSpeed;
                }
            //    //Если еще нет маршрута по графу или его надо перестроить, то строим его (Дейкстра)
            //    if (_nodes == null || _direction != nextDirection)
            //    {
            //        //Если изменилась цель, пересчитываем маршрут
            //        _direction = nextDirection;
            //        _nodes = Map.GetRoute(Position, _direction.Value);
            //        if (_nodes == null || _nodes.Count == 0)
            //        {
            //            RouteList.Remove(nextCheckPoint);
            //            continue;
            //        }
            //        _points = null;
            //    }

            //    //----------------------------------------------------------
            //    //Берем следующую цель на маршруте и строим путь по точкам (A*)
            //    var nextNode = _nodes.First();
            //    if (_points == null)
            //    {
            //        _points = Map.GetWay(Position, nextNode);
            //        //
            //        if (_points == null)
            //        {
            //            _nodes.Remove(nextNode);
            //            if (_nodes.Count == 0)
            //            {
            //                RouteList.Remove(nextCheckPoint);
            //            }
            //            return;
            //        }
            //    }
            //    //Берем следующую клетку маршрута
            //    var point = _points.FirstOrDefault();
            //    if (point != null)
            //    {
            //        if (point == Position)
            //        {
            //            _points.Remove(point);
            //        }
            //        else
            //        {
            //            //Пытаемся занять клетку
            //            bool? tryHoldResult = Map.TryHoldPosition(point, Weigth);
            //            if (tryHoldResult == true)
            //            {
            //                Map.ReleasePosition(Position, Weigth);
            //                Position = point;
            //                _points.Remove(point);
            //            }
            //            else 
            //            {
            //                //Перестраиваем маршрут или просто ждем
            //                if (tryHoldResult == false)
            //                    _points = null;
            //                return;
            //            }
                        
            //        }
            //    }
            //    if (_points.Count == 0)
            //    {
            //        _nodes.Remove(nextNode);
            //        if (_nodes.Count == 0)
            //        {
            //            _nodes = null;
            //            //Переходим под управление сервиса
            //            if (nextCheckPoint.IsServicePoint)
            //            {
            //                _lastService.AddAgent(this);
            //                _inService = true;
            //            }
            //            //Удаляем пройденную точку
            //            else
            //            {
            //                RouteList.Remove(nextCheckPoint);
            //            }
            //        }

            //        _points = null;
            //    }
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
