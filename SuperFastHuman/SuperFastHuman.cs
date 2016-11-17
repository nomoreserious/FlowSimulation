using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Media3D;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Enviroment;
using FlowSimulation.Enviroment.Model;

namespace FlowSimulation.Agents
{
    internal sealed class SuperFastHuman : Contracts.Agents.AgentBase
    {
        private List<Point> _points;
        private bool _inService;
        private AgentServiceBase _lastService;

        //private int _tryGetWay = 0;

        public SuperFastHuman(Map map, IEnumerable<AgentServiceBase> services)
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
                var targetWayPoint = RouteList.First();
                Point nextDirection = new Point(-1,-1);
                //Если следующая точка - сервис
                if (targetWayPoint.IsServicePoint)
                {
                    if (_lastService == null)
                    {
                        //получаем сервис
                        _lastService = _services.FirstOrDefault(s => s.Id == targetWayPoint.ServiceId);
                        if (_lastService == null)
                            throw new ArgumentNullException("Service {id:" + targetWayPoint.ServiceId + "} not found");
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
                            RouteList.Remove(targetWayPoint);
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
                else if (targetWayPoint.LayerId != LayerId)
                {
                    var positionChanged = false;
                    for (int i = targetWayPoint.X; i < targetWayPoint.X + targetWayPoint.Width; i++)
                    {
                        for (int j = targetWayPoint.Y; j < targetWayPoint.Y + targetWayPoint.Height; j++)
                        {
                            var newPosition = new Point(i, j);
                            if (_map[targetWayPoint.LayerId].TryHoldPosition(newPosition, Weigth) == true)
                            {
                                _map[LayerId].ReleasePosition(Position, Weigth);
                                Position = newPosition;
                                LayerId = targetWayPoint.LayerId;
                                positionChanged = true;
                                break;
                            }
                            if (positionChanged) break;
                        }
                    }
                    if (positionChanged)
                    {
                        RouteList.Remove(targetWayPoint);
                    }
                    else
                    {
                        CurrentSpeed = 0;
                        return;
                    }
                }
                else
                {
                    nextDirection = targetWayPoint.Center;
                }

                if (_points != null && (_points.Count == 0 || targetWayPoint.Contains(Position)))
                {
                    if (targetWayPoint.Contains(Position))
                    {
                        //Переходим под управление сервиса
                        if (targetWayPoint.IsServicePoint)
                        {
                            _lastService.AddAgent(this);
                            _inService = true;
                        }
                        //Удаляем пройденную точку
                        RouteList.Remove(targetWayPoint);
                    }
                    _points = null;
                    continue;
                }
                //----------------------------------------------------------
                //Берем следующую цель на маршруте и строим путь по точкам
                if (_points == null)
                {
                    _points = _map[LayerId].GetWay(Position, targetWayPoint);

                    if (_points == null)
                    {
                        CurrentSpeed = 0;
                        RouteList.Remove(targetWayPoint);
                        return;
                    }
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
                    bool? tryHoldResult = _map[LayerId].TryHoldPosition(point, Weigth);
                    if (!tryHoldResult.HasValue)
                    {
                        //Ждем, пока путь откроется
                        CurrentSpeed = 0;
                        break;
                    }
                    else if (tryHoldResult.Value)
                    {
                        _map[LayerId].ReleasePosition(Position, Weigth);
                        Position = point;
                        _points.Remove(point);
                    }
                    else
                    {
                        //На следующем шаге перестраиваем маршрут
                        //TODO при затыке постоянный пересчет
                        _points = null;
                    }
                    CurrentSpeed -= _maxSpeed;
                }
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
            Size = new Size3D(size, size, size * 4.3);
            _maxSpeed = 360 + (rand.NextDouble() - 0.7) * 100;
            _acceleration = 0;
            _deceleration = 0;
            _direction = null;
        }
    }
}
