using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Enviroment;
using System.Drawing;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Contracts.Agents;

namespace FlowSimulation.Services.Queue
{
    public class Queue : AgentServiceBase
    {
        private List<AgentBase> _queue;
       // private Dictionary<ulong, Point> _directions;
        private Point _servicePoint;

        private double _minServedTime = 600;
        private double _maxServedTime = 1000;
        private double _currentServedTime = 0;
        private PriorityDirection _direction = PriorityDirection.Right;

        private Map _map;
        private Random _random = new Random();

        public List<WayPoint> InputPoints { get; set; }

        public Queue(ulong id, Map map, WayPoint location)
        {
            Id = id;
            _map = map;
            base.Dislocation = location;
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
            if (_queue.Count == 0)
            {
                _currentServedTime = 0;
            }
            _queue.Add(agent);
        }

        public override bool ContainsAgent(ulong id)
        {
            return _queue.Any(a => a.Id == id);
        }

        public override void DoStep(double step_interval)
        {
            if (_queue.Count != 0)
            {
                _currentServedTime += step_interval;
             
                if (_currentServedTime < _maxServedTime)
                    return;
                _currentServedTime = 0;
                var first = _queue.First();
                _map[Dislocation.LayerId].ReleasePosition(_queue.Last().Position, _queue.Last().Weigth);
                for (int i = _queue.Count - 1; i > 0; i--)
                {
                    _queue[i].Position = _queue[i - 1].Position;
                }
                first.Position = _servicePoint;
                _queue.Remove(first);
                //_directions.Remove(first.Id);
            }
        }

        public override void Initialize(Dictionary<string, object> settings)
        {
            _queue = new List<AgentBase>();
            //_directions = new Dictionary<ulong, Point>();

            _minServedTime = (int)settings["minTime"];
            _maxServedTime = (int)settings["maxTime"];

            _direction = PriorityDirection.Top;
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
