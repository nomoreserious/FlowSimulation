using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Services;
using System.Drawing;
using FlowSimulation.Contracts.Agents;
using System.Timers;

namespace FlowSimulation.Services.BusStop
{
    public class BusStop : AgentServiceBase
    {
        private VehicleAgentBase _vehicle;
        private Enviroment.Map _map;
        private List<AgentBase> _queue;
        //private Dictionary<ulong, Point> _directions;
        private Point _servicePoint;
        private PriorityDirection _direction;
        private double _current_served_time;
        private int _input_time_ms;

        public BusStop(ulong id, Enviroment.Map map, Enviroment.WayPoint location)
        {
            base.Id = id;
            base.Dislocation = location;
            _map = map;
            _servicePoint = new Point(location.X, location.Y);
        }

        public override bool TryGetDirection(Contracts.Agents.AgentBase agent, out Point direction)
        {
            //if (_directions.ContainsKey(agent.Id))
            //{
            //    direction = _directions[agent.Id];
            //    return true;
            //}
            direction = _queue.Count == 0 ? _servicePoint : _queue.Last().Position;
            //Point position = agent.Position;
            Random rand = new Random((int)agent.Id);
            switch (_direction)
            {
                case PriorityDirection.Top:
                    direction.Offset(rand.Next(-1, 2), -1);
                    break;
                case PriorityDirection.TopRight:
                    direction.Offset(rand.Next(0, 2), rand.Next(-1, 1));
                    break;
                case PriorityDirection.Right:
                    direction.Offset(1, rand.Next(-1, 2));
                    break;
                case PriorityDirection.BottomRight:
                    direction.Offset(rand.Next(0, 2), rand.Next(0, 2));
                    break;
                case PriorityDirection.Bottom:
                    direction.Offset(rand.Next(-1, 2), 1);
                    break;
                case PriorityDirection.BottomLeft:
                    direction.Offset(rand.Next(-1, 1), rand.Next(0, 2));
                    break;
                case PriorityDirection.Left:
                    direction.Offset(-1, rand.Next(-1, 2));
                    break;
                case PriorityDirection.TopLeft:
                    direction.Offset(rand.Next(-1, 1), rand.Next(-1, 1));
                    break;
            }
            var cell = _map[0].GetCell(direction.X, direction.Y);
            return direction == agent.Position || (!cell.HasAgent && cell.CurrentValue != 0x00);
        }

        public override void AddAgent(Contracts.Agents.AgentBase agent)
        {
            if (agent is VehicleAgentBase)
            {
                _vehicle = (VehicleAgentBase)agent;
                _input_time_ms = (int)(TimeSpan.FromMinutes(1).TotalMilliseconds / _vehicle.InputFactor);
                _canInput = true;
            }
            else
            {
                _queue.Add(agent);
            }
        }

        public override bool ContainsAgent(ulong id)
        {
            return _queue.Any(a => a.Id == id);
        }

        //private int _avaliableVolume = 15;
        private bool _canInput = false;

        public override void DoStep(double step_interval)
        {
            if (_canInput)
            {
                if (_queue.Count != 0)
                {
                    _current_served_time += step_interval;

                    while (_current_served_time > _input_time_ms && _queue.Count > 0)
                    {
                        _current_served_time -= _input_time_ms;
                        var first = _queue.First();
                        _map[Dislocation.LayerId].ReleasePosition(_queue.Last().Position, _queue.Last().Weigth);
                        for (int i = _queue.Count - 1; i > 0; i--)
                        {
                            _queue[i].Position = _queue[i - 1].Position;
                        }
                        first.Position = _servicePoint;
                        _queue.Remove(first);
                        _vehicle.CurrentAgentCount++;
                        if (_queue.Count == 0 || _vehicle.CurrentAgentCount >= _vehicle.MaxCapasity)
                        {
                            _current_served_time = 0;
                            _vehicle.Go();
                            _canInput = false;
                            _vehicle = null;
                        }
                        //_directions.Remove(first.Id);
                    }
                }
                else
                {
                    _current_served_time = 0;
                    _vehicle.Go();
                    _canInput = false;
                    _vehicle = null;
                }
            }
        }

        public override void Initialize(Dictionary<string, object> settings)
        {
            _queue = new List<AgentBase>();
            //_directions = new Dictionary<ulong, Point>();

            _input_time_ms = (int)settings["input_time_ms"];

            _direction = PriorityDirection.Right;
        }
    }

    public enum PriorityDirection : byte
    {
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left,
        TopLeft
    }
}

