using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using FlowSimulation.Analisis;
using FlowSimulation.Contracts.Agents;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Enviroment;
using FlowSimulation.Helpers.Graph;
using FlowSimulation.Managers;
using FlowSimulation.Scenario.Model;
using QuickGraph;
using QuickGraph.Algorithms;

namespace FlowSimulation.Core
{
    class Simulation : IDisposable
    {
        public const double STEP_TIME_MS = 200;

        private Random rand = new Random();
        private ScenarioModel _scenario;
        private FlowSimulation.Enviroment.Map _map;
        private System.Timers.Timer _timer;
        private AnalisisCollector _analisisCollector;
        private bool _canElapsed;
        private List<AgentBase> _agents;
        //private Dictionary<long, Queue<AgentBase>> _groupQueue;
        private List<ServiceBase> _services;
        private Dictionary<ulong, double> _agentByStepCounter = new Dictionary<ulong, double>();
        private List<ManualResetEvent> _workThreadCompliteEvents = new List<ManualResetEvent>();
        private List<SimulationTask> _workThreadTasks = new List<SimulationTask>();
        private DateTime _start, _end, _simulationTime;
        private double _speedRatio = 1.0;

        public Simulation(ScenarioModel scenario)
        {
            if (scenario == null)
            {
                throw new ArgumentNullException("scenario is null");
            }

            //System.Threading.Timer t = new System.Threading.Timer(new System.Threading.TimerCallback((o) => Step()), null, 0, (long)STEP_TIME_MS);
         
            _scenario = scenario;
            RoadGraph = new Graph<WayPoint, PathFigure>();
            if (_scenario.RoadGraph != null)
            {
                foreach (var node in _scenario.RoadGraph.Nodes)
                {
                    RoadGraph.Add(node);
                }
                foreach (var edge in _scenario.RoadGraph.Edges)
                {
                    var pathGeom = PathGeometry.CreateFromGeometry(PathGeometry.Parse(edge.Data));
                    RoadGraph.AddEdge(edge.Start, edge.End, pathGeom.Figures.First());
                }
            }

            _analisisCollector = new Analisis.AnalisisCollector();
            _map = _scenario.Map;
            _start = _scenario.StartTime;
            _end = _scenario.EndTime;
            _simulationTime = _scenario.StartTime;
            Init(STEP_TIME_MS);
        }

        public DateTime SimulationTime { get { return _simulationTime; } }

        public double StepTime { get; private set; }

        public IEnumerable<AgentBase> Agents { get { return _agents; } }
        public IEnumerable<ServiceBase> Services { get { return _services; } }
        public Map Map { get { return _scenario.Map; } }
        public Graph<WayPoint, PathFigure> RoadGraph { get; set; }
        public bool IsRunning { get { return _timer != null && _timer.Enabled; } }

        public double SpeedRatio
        {
            get { return _speedRatio; }
            set
            {
                if (_speedRatio == value)
                    return;
                _speedRatio = value;
                _timer.Interval = STEP_TIME_MS / SpeedRatio;
            }
        }

        public void Start()
        {
            if (_timer == null)
            {
                throw new InvalidOperationException("You must Init first!");
            }
            _timer.Start();
        }

        public void Stop()
        {
            if (_timer == null)
            {
                throw new InvalidOperationException("You must Init first!");
            }
            _timer.Stop();
        }

        public void Init(double init_interval_ms)
        {
            if (_end < _start)
            {
                throw new ArgumentException("endTime must biggen then startTime!");
            }

            _agents = new List<AgentBase>();
            _services = new List<ServiceBase>();
            foreach (var s in _scenario.Services)
            {
                var manager = Managers.ServiceManager.Instance.GetManagerByInnerCode(s.ManagerCode);
                if (manager != null)
                {
                    var location = _scenario.InputOutputPoints.FirstOrDefault(p => p.IsServicePoint && p.ServiceId == s.Id);
                    if (location != null)
                    {
                        _services.Add(manager.GetInstance(s.Id, _map, location, s.Settings));
                    }
                    else
                    {
                        Console.WriteLine("Не найдена позиция для сервиса: " + s.Name);
                    }
                }
                else
                {
                    Console.WriteLine("Не найден тип сервиса: " + s.TypeName);
                }
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
            _timer = new System.Timers.Timer(init_interval_ms);
            _timer.AutoReset = true;
            _timer.Elapsed += (s, e) => Step();
            _canElapsed = true;
        }

        public void Dispose()
        {
            if (!object.ReferenceEquals(_timer, null))
            {
                _timer.Dispose();
            }
        }

        private void StopScenario()
        {
            _timer.Stop();
            _agents.Clear();
            //_groupQueue.Clear();
            _map.Clear();
        }

        public void Step()
        {
            if (!_canElapsed)
            {
                return;
            }
            _canElapsed = false;

            Helpers.Timer.HighResolutionTime.Start();

            _simulationTime = _simulationTime.Add(TimeSpan.FromMilliseconds(STEP_TIME_MS));

            if (_simulationTime >= _scenario.EndTime)
            {
                StopScenario();
                System.Windows.MessageBox.Show("Процесс имитационного моделирования окончен", "Окончание процесса", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }
            try
            {
                //if (_scenarioConfig.AgentInputOutput || _scenarioConfig.AgentInputOutputByGroup)
                //{
                //    _removeCount += DoAgentRemoving();
                //    _addCount += DoAddAgentFromGroup();
                //}
                //else
                //{
                AgentRemoving();
                AddAgentFromGroup();
                
                //Выполняем обход сервисов
                foreach (var service in _services)
                {
                    service.DoStep(STEP_TIME_MS);
                }
                
                //Выполняем обход агентов
#if DEBUG
                foreach (var agent in _agents)
                {
                    agent.DoStep((int)STEP_TIME_MS);
                }
#else   
                //Если потоков недостаточно, то наращиваем мощность (поток на каждые 100 агентов)
                if(_agents.Count / 100 + 1 > _workThreadCompliteEvents.Count)
                {
                    for (int i = _workThreadCompliteEvents.Count; i < _agents.Count / 100 + 1; i++)
                    {
                        var mre = new ManualResetEvent(false);
			            _workThreadCompliteEvents.Add(mre);
                        _workThreadTasks.Add(new SimulationTask(mre, STEP_TIME_MS));
                        Console.WriteLine("Создан поток " + i);
                    }
                }
                for (int i = 0; i < _agents.Count / 100 + 1; i++)
                {
                    int taskAgentCount = _agents.Count - 100 * i > 100 ? 100 : _agents.Count - 100 * i;
                    ThreadPool.QueueUserWorkItem(_workThreadTasks[i].ThreadPoolCallback, _agents.GetRange(100 * i, taskAgentCount));
                }
                WaitHandle.WaitAll(_workThreadCompliteEvents.ToArray());
                //foreach (var agent in _agents)
                //{
                //    agent.DoStep((int)STEP_TIME_MS);
                //}
#endif
                Analisis();
            }
            finally
            {
                _canElapsed = true;
            }
            StepTime = Helpers.Timer.HighResolutionTime.GetTime();
            Benchmark = (int)(100 - 100 * StepTime / (_timer.Interval / 1000) > 0 ? 100 - 100 * StepTime / (_timer.Interval / 1000) : 0);
        }

        public int Benchmark { get; private set; }

        private void Analisis()
        {
            //if (_analisisStepCounter < _analisisViewModel.StepBetweenAnalisisCount)
            //{
            //    _analisisStepCounter++;
            //    return;
            //}
            //_analisisStepCounter -= _analisisViewModel.StepBetweenAnalisisCount;
            //if (_scenarioConfig.AverangeQueueLenght)
            //{
            //    double value = (from service in ServicesList where service is QueueService select (service as QueueService).AverangeQueueLenght).Average();
            //    _analisisViewModel.AddPoint(AnalisisConstants.AVERANGE_QUEUE_LENGHT, null, currentTime.TotalSeconds, value);
            //}
            ////Расчет входящего и исходящего трафика
            //if (_scenarioConfig.AgentInputOutput || _scenarioConfig.AgentInputOutputByGroup)
            //{
            //    _analisis.AddAgentInputOutput(currentTime, _addCount, _removeCount);
            //    _analisisViewModel.AddPoint(AnalisisConstants.AGENT_INPUT_OUTPUT_NAME, "Input", currentTime.TotalSeconds, _addCount);
            //    _analisisViewModel.AddPoint(AnalisisConstants.AGENT_INPUT_OUTPUT_NAME, "Output", currentTime.TotalSeconds, -_removeCount);
            //    _addCount = 0;
            //    _removeCount = 0;
            //}
            ////Расчет плотности
            //if (_scenarioConfig.SpectralDensity)
            //{
            //    byte[,] mas = map.GetMap();
            //    for (int i = 0; i < mas.GetLength(0); i++)
            //    {
            //        for (int j = 0; j < mas.GetLength(1); j++)
            //        {
            //            if ((mas[i, j] & 0x40) == 0x40)
            //            {
            //                _analisis.PassengerDensity[i, j] += 1;
            //            }
            //        }
            //    }
            //}
            ////Расчет количества агентов
            //if (_scenarioConfig.AgentCountOnMap)
            //{
            //    _analisisViewModel.AddPoint(AnalisisConstants.AGENT_COUNT_ON_MAP_NAME, null, currentTime.TotalSeconds, agentsList.Count);
            //    _analisis.AddAgentsCount(currentTime, agentsList.Count);
            //}
            ////Расчет количества агентов по группам
            //if (_scenarioConfig.AgentCountOnMapByGroup)
            //{
            //    List<int> grouper = new List<int>();
            //    foreach (var group in agentGroups)
            //    {
            //        int count = agentsList.Count(ag => ag.Group == group.ID);
            //        _analisisViewModel.AddPoint(AnalisisConstants.AGENT_COUNT_ON_MAP_BY_GROUP_NAME, group.Name, currentTime.TotalSeconds, count);
            //        grouper.Add(count);
            //    }
            //    foreach (var service in ServicesList)
            //    {
            //        if (service is StopService)
            //        {
            //            int count = agentsList.Count(ag => ag.Group == (service as StopService).PassengersGroup.ID);
            //            _analisisViewModel.AddPoint(AnalisisConstants.AGENT_COUNT_ON_MAP_BY_GROUP_NAME, (service as StopService).PassengersGroup.Name, currentTime.TotalSeconds, count);
            //            grouper.Add(count);
            //        }
            //    }
            //    _analisis.AddAgentsCountByGroup(currentTime, grouper.ToArray());
            //}
            //_analisisViewModel.AnalisisComplit();
        }

        private int AgentRemoving()
        {
            //if (_scenarioConfig.AgentAverangeLenght)
            //{
            //    foreach (var agent in agentsList.FindAll(delegate(AgentBase ab) { return ab is HumanAgent && ab.WayPointsList.Count == 0; }))
            //    {
            //        _analisis.routeLenght.Add((agent as HumanAgent).RouteLenght);
            //    }
            //}
            //if (_scenarioConfig.AgentAverangeTime)
            //{
            //    foreach (var agent in agentsList.FindAll(delegate(AgentBase ab) { return ab is HumanAgent && ab.WayPointsList.Count == 0; }))
            //    {
            //        _analisis.routeLenght.Add((agent as HumanAgent).RouteTime);
            //    }
            //}
            _agents.ForEach(a => { if (a.RouteList.Count == 0) Map[a.LayerId].ReleasePosition(a.Position, a.Weigth); });
            return _agents.RemoveAll(a => a.RouteList.Count == 0);
        }

        //double vehicleTime = TimeSpan.FromSeconds(40).TotalMilliseconds;
        private int AddAgentFromGroup()
        {
            //HACK автобусы вне группы
            //vehicleTime -= STEP_TIME_MS;
            //if (vehicleTime < 0)
            //{
            //    var mng = AgentManager.Instance.GetManagerByInnerCode("vehicle");
            //    if (mng != null)
            //    {
            //        var vehicle = (VehicleAgentBase)mng.GetInstance(this._map, _services.Where(s => s is AgentServiceBase).Cast<AgentServiceBase>(), new Dictionary<string, object>());
            //        vehicle.GroupId = 741;
            //        vehicle.RoadGraph = _scenario.RoadGraph;
            //        vehicle.RouteList = new List<WayPoint>() { RoadGraph.Nodes.First(n => n.Name == "1"), RoadGraph.Nodes.First(n => n.Name == "5") };
            //        _agents.Add(vehicle);
            //        vehicleTime = TimeSpan.FromMinutes(1).TotalMilliseconds;
            //    }
            //}
            //конец HACK
            int addCount = 0; 
            foreach (var group in _scenario.AgentGroups)
            {
                int count = 0;

                switch (group.Type)
                {
                    case AgentGroupInitType.Distribution:
                        {
                            if (!_agentByStepCounter.ContainsKey(group.Id))
                            {
                                _agentByStepCounter.Add(group.Id, 0.0D);
                            }
                            _agentByStepCounter[group.Id] += group.AgentsDistibution[_simulationTime.DayOfWeek][(int)_simulationTime.TimeOfDay.TotalMinutes / 10] / (60000.0D / STEP_TIME_MS);
                            count = (int)Math.Floor(_agentByStepCounter[group.Id]);
                            break;
                        }
                    case AgentGroupInitType.Network:
                        {
                            break;
                        }
                    case AgentGroupInitType.Count:
                        {
                            if (!_agentByStepCounter.ContainsKey(group.Id))
                            {
                                _agentByStepCounter.Add(group.Id, group.Count);
                            }
                            count = (int)Math.Floor(_agentByStepCounter[group.Id]);
                            break;
                        }
                    case AgentGroupInitType.TimeTable:
                        {
                            if (!_agentByStepCounter.ContainsKey(group.Id))
                            {
                                _agentByStepCounter.Add(group.Id, 0.0D);
                            }
                            if (group.TimeTable[_simulationTime.DayOfWeek].Any(t => (t - _simulationTime.TimeOfDay).TotalMilliseconds > 0 && (t - _simulationTime.TimeOfDay).TotalMilliseconds <= STEP_TIME_MS))
                            {
                                _agentByStepCounter[group.Id]++;
                            }
                            count = (int)Math.Floor(_agentByStepCounter[group.Id]);
                            break;
                        }
                }

                for (int i = 0; i < count; i++)
                {
                    string code = group.AgentTypeCode;
                    
                    var mng = AgentManager.Instance.GetManagerByInnerCode(code);
                    if (mng == null)
                    {
                        Console.WriteLine("Агент с кодом {" + code + "} не найден во внешних сборках");
                        continue;
                    }

                    //AgentBase agent = mng.GetInstance(this._map, this._services, group.AgentsConfig[code]);
                    //TODO Формирование разных типов агентов происходит через менеджеры агентов
                    AgentBase agent = mng.GetInstance(this._map, _services.Where(s => s is AgentServiceBase).Cast<AgentServiceBase>(), new Dictionary<string, object>());
                    agent.GroupId = group.Id;
                    agent.RouteList = new List<WayPoint>(GetAgentRoute(group));
                    Point initPosition;
                    if (GetAgentStartPosition(out initPosition, group.SourcePoint, agent.Weigth))
                    {
                        agent.Position = initPosition;
                        agent.LayerId = group.SourcePoint.LayerId;
                        _agents.Add(agent);
                        _agentByStepCounter[group.Id]--;
                        addCount++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //for (int i = 0; i < ServicesList.Count; i++)
            //{
            //    add_count += ServicesList[i].AddCount;
            //}
            return addCount;
        }

        private AdjacencyGraph<WayPoint, Edge<WayPoint>> _transitionsGraph;
        private Dictionary<ulong, IEnumerable<WayPoint>> _routesHash = new Dictionary<ulong, IEnumerable<WayPoint>>();

        private IEnumerable<WayPoint> GetAgentRoute(AgentsGroup group)
        {
            if (_routesHash.ContainsKey(group.Id))
                return _routesHash[group.Id];

            WayPoint source = group.SourcePoint;
            WayPoint target = group.TargetPoint;

            if (_scenario.TransitionGraph == null || _scenario.TransitionGraph.Nodes.Count() == 0)
                return null;

            if (_transitionsGraph == null)
            {
                _transitionsGraph = new AdjacencyGraph<WayPoint, Edge<WayPoint>>();
                _transitionsGraph.AddVertexRange(_scenario.TransitionGraph.Nodes);
                _transitionsGraph.AddEdgeRange(from edge in _scenario.TransitionGraph.Edges select new Edge<WayPoint>(edge.Start, edge.End));
            }

            var route = new List<WayPoint>();
            route.Add(source);

            if (target != null)
            {
                IEnumerable<Edge<WayPoint>> result;
                if (_transitionsGraph.ShortestPathsDijkstra((e) => _scenario.TransitionGraph.GetEdgeData(e.Source, e.Target), source).Invoke(target, out result))
                {
                    route.AddRange(result.Select(e => e.Target));
                }
                else
                {
                    route = null;
                }
                _routesHash[group.Id] = route;
                return route;
            }
            //TODO допилить адекватное построение маршрута

            var curNode = source;

            int tryCount = _scenario.TransitionGraph.Nodes.Count();

            while (_scenario.TransitionGraph.HasChildNodes(curNode))
            {
                double rand_value = rand.NextDouble();
                var edges = _scenario.TransitionGraph.GetEdgesFrom(curNode).ToList();
                if (route.Count > 1)
                {
                    edges.RemoveAll(e => e.End == route[route.Count - 2] || e.End.LayerId > route.Last().LayerId);
                }
                foreach (var edge in edges)
                {
                    rand_value -= edge.Data / edges.Sum(e => e.Data);
                    if (rand_value <= 0)
                    {
                        route.Add(edge.End);
                        if (edge.End.IsOutput)
                        {
                            route.Remove(route.First());
                            return route;
                        }
                        curNode = edge.End;
                        break;
                    }
                }
                if (tryCount-- == 0) break;
            }
            route.Remove(route.First());
            return route;
        }

        private bool GetAgentStartPosition(out Point position, WayPoint groupStart,double agentWeight)
        {
            int tryCount = groupStart.Width * groupStart.Height;
            while (tryCount > 0)
            {
                tryCount--;
                int x = rand.Next(groupStart.X, groupStart.X + groupStart.Width);
                int y = rand.Next(groupStart.Y, groupStart.Y + groupStart.Height);
                position = new Point(x, y);
                if (Map[groupStart.LayerId].TryHoldPosition(position, agentWeight) == true)
                    return true;
            }
            position = Point.Empty;
            return false;
        }
    }
}
