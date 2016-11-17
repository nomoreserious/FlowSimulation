using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using FlowSimulation.Agents;
using FlowSimulation.Analisis;
using FlowSimulation.ConfigWindows;
using FlowSimulation.Service;
using FlowSimulation.SimulationScenario.IO;
using FlowSimulation.Helpers.Graph;
using FlowSimulation.Map.Model;
using FlowSimulation.Enviroment;
using FlowSimulation.Contracts.Agents;

namespace FlowSimulation.SimulationScenario
{
    public class Scenario
    {
        public const int STEP_TIME_MS = 100;

        private ExperimentConfig _scenarioConfig;
        private double _speedRatio;
        private AnalisisViewModel _analisisViewModel;
        private AnalisisCollector _analisis;
        private List<BackgroundWorker> _workersList;
        private DispatcherTimer _mainTimer;
        private int _removeCount; 
        private int _addCount;
        private int _analisisStepCounter;

        /// <summary>
        /// Конструктор
        /// </summary>
        public Scenario()
        {
            _mainTimer = new DispatcherTimer();
            _workersList = new List<BackgroundWorker>();
        }

        public AnalisisCollector ScenarioAnalisis { get { return _analisis; } }
        public AnalisisViewModel ScenarioAnalisisViewModel { get { return _analisisViewModel; } }

        public TimeSpan currentTime { get; set; }

        public Dictionary<int, TcpListener> GroupListeners { get; set; }
        public Dictionary<int, Socket> ServersSocketList { get; set; }
        public Dictionary<int, double> AgentByStepCounter { get; set; }
        public System.Windows.Media.Imaging.BitmapImage Image;
        
        public double SpeedRatio 
        {
            get { return _speedRatio; }
            set 
            {
                double oldratio = _speedRatio;
                _speedRatio = value;
                for (int i = 0; i < agentsList.Count; i++)
                {
                    agentsList[i].SpeedRatio = _speedRatio;
                }
                //for (int i = 0; i < timersList.Count; i++)
                //{
                //    timersList[i].Interval = TimeSpan.FromMilliseconds(timersList[i].Interval.TotalMilliseconds * oldratio / _speedRatio);
                //}
                _mainTimer.Interval = TimeSpan.FromMilliseconds(_mainTimer.Interval.TotalMilliseconds * oldratio / _speedRatio);
            }
        }
        public string StringMap { get; set; }
        public double MapSquere;
        public Enviroment.Map map { get; set; }
        public Graph<WayPoint, PathFigure> RoadGraph { get; set; }

        public Dictionary<int, StreamWriter> SocketWritersList { get; set; }
        public List<AgentsGroup> agentGroups { get; set; }
        public List<ServiceBase> ServicesList { get; set; }
        public List<PaintObject> paintObjectList { get; set; }
        public List<AgentBase> agentsList { get; set; }
        
        public double Benchmark { get; set; }

        public bool IsStoped { get; set; }
        public bool IsPaused { get; set; }

        public void InitializeStepScenario(ExperimentConfig cnfg)
        {
            _scenarioConfig = cnfg;
            currentTime = cnfg.StartTime;
            int groupCount = agentGroups.Count;
            for (int i = 0; i < ServicesList.Count; i++)
            {
                if (ServicesList[i] is StopService)
                {
                    if ((ServicesList[i] as StopService).PassengersGroup != null)
                    {
                        groupCount++;
                    }
                }
                if (ServicesList[i] is TurnstileService)
                {
                    if ((ServicesList[i] as TurnstileService).TurnstileGeometry != null)
                    {
                        PaintObject obj = new PaintObject(ServicesList[i].Name);
                        
                    }
                }
                ServicesList[i].Initialize();
            }
            _analisisViewModel = new AnalisisViewModel();

            _analisis = new AnalisisCollector(map.GetMap().GetLength(0), map.GetMap().GetLength(1), groupCount);

            if (GroupListeners != null)
            {
                foreach (var listener in GroupListeners.Values)
                {
                    listener.Stop();
                }
            }
            GroupListeners = new Dictionary<int, TcpListener>();
            ServersSocketList = new Dictionary<int, Socket>();
            agentsList = new List<AgentBase>();
            AgentByStepCounter = new Dictionary<int, double>();
            for (int i = 0; i < agentGroups.Count;i++ )
            {
                if (agentGroups[i].IsNetworkGroup)
                {
                    TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Parse(agentGroups[i].Address), agentGroups[i].Port);
                    tcpListener.Start();
                    GroupListeners.Add(agentGroups[i].ID, tcpListener);
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += new DoWorkEventHandler(AcceptAgent_DoWork);
                    worker.RunWorkerAsync(agentGroups[i]);
                }
                else
                {
                    AgentByStepCounter.Add(agentGroups[i].ID, 0);
                }
            }
        }

        public void InitializeScenario(ExperimentConfig cnfg)
        {
            currentTime = cnfg.StartTime;
            _analisis = new AnalisisCollector(map.GetMap().GetLength(0), map.GetMap().GetLength(1), agentGroups.Count);
            SocketWritersList = new Dictionary<int, StreamWriter>();
            IsStoped = false;

            _workersList.Clear();
            agentsList.Clear();

            foreach (var group in agentGroups.FindAll(delegate(AgentsGroup ag) { return !ag.IsServiceGroup; }))
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerAsync(group);
                _workersList.Add(worker);
                //if (group.ID == 1)
                //{
                //    try
                //    {
                //        TcpClient tcpClient;
                //        tcpClient = new TcpClient();
                //        tcpClient.NoDelay = true;
                //        tcpClient.Connect(System.Net.IPAddress.Loopback, 5554);
                //        Console.WriteLine("Соединение с 127.0.0.1:5554 установлено");
                //        NetworkStream networkStream = tcpClient.GetStream();
                //        StreamWriter streamWriter = new StreamWriter(networkStream);
                //        SocketWritersList.Add(1, streamWriter);
                //    }
                //    catch (SocketException ex)
                //    {
                //        Console.WriteLine(ex);
                //    }
                //}
            }
            _mainTimer.Interval = TimeSpan.FromMilliseconds(300);
            _mainTimer.Tick += new EventHandler(mainTimer_Tick);
            _mainTimer.Start();
            DispatcherTimer analisisTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            analisisTimer.Tick += analisisTimer_Tick;
            analisisTimer.Start();
        }

        void analisisTimer_Tick(object sender, EventArgs e)
        {
            DoAnalisis();
        }

        public void StopScenario()
        {
            for (int i = 0; i < _workersList.Count; i++)
            {
                _workersList[i].CancelAsync();
            }
            for (int i = 0; i < agentsList.Count; i++)
            {
                agentsList[i].Abort();
            }
            _mainTimer.Stop();
            agentsList.Clear();
            map.UnlockAllCells();
            IsStoped = true;
        }

        void mainTimer_Tick(object sender, EventArgs e)
        {
            DoAgentRemoving();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //AgentsGroup group = e.Argument as AgentsGroup;
            //Random rand = new Random();
            //if (group.IsNetworkGroup)
            //{
            //    TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Parse(group.Address), group.Port);
            //    tcpListener.Start();
            //    while (true)
            //    {
            //        if ((sender as BackgroundWorker).CancellationPending)
            //        {
            //            tcpListener.Stop();
            //            break;
            //        }
            //        Socket serverSocket = tcpListener.AcceptSocket();
            //        StreamReader streamReader;
            //        NetworkStream networkStream;
            //        if (serverSocket.Connected)
            //        {
            //            networkStream = new NetworkStream(serverSocket);
            //            streamReader = new StreamReader(networkStream);
            //            while (true)
            //            {
            //                if ((sender as BackgroundWorker).CancellationPending)
            //                {
            //                    serverSocket.Close();
            //                    tcpListener.Stop();
            //                    break;
            //                }
            //                try
            //                {
            //                    streamReader.ReadLine();
            //                    int j = 0;
            //                    double value = rand.NextDouble();
            //                    while (value > 0 && j < group.AgentTemplateList.Count)
            //                    {
            //                        value -= group.AgentTemplateList[j].Persent;
            //                        j++;
            //                    }
            //                    j--;
            //                    int AgentSpeedms = Convert.ToInt32(Map.CellSize * 3600 * 1000 / (rand.Next(Convert.ToInt32(group.AgentTemplateList[j].MinSpeed * 1000), Convert.ToInt32(group.AgentTemplateList[j].MaxSpeed * 1000))));
            //                    AgentBase agent = null;
            //                    if (group.AgentTemplateList[j].Type == typeof(HumanAgent).Name)
                                //{
                                //    agent = new HumanAgent(AgentIDEnumerator.GetNextID(), this, group.ID, AgentSpeedms);
                                //}
                                //else if (group.AgentTemplateList[j].Type == typeof(BusAgent).Name)
                                //{
                                //    agent = new BusAgent(AgentIDEnumerator.GetNextID(), this, group.ID, AgentSpeedms)
                                //    {
                                //        RoadGraph = this.RoadGraph,
                                //        Size = new System.Windows.Media.Media3D.Size3D(group.AgentTemplateList[j].Size.X / Map.CellSize, group.AgentTemplateList[j].Size.Y / Map.CellSize, group.AgentTemplateList[j].Size.Z / Map.CellSize),
                                //        MaxCapasity = group.AgentTemplateList[j].MaxCapasity,
                                //        InputFactor = group.AgentTemplateList[j].InputFactor,
                                //        OutputFactor = group.AgentTemplateList[j].OutputFactor
                                //    };
                                //}
                                //else if (group.AgentTemplateList[j].Type == typeof(TrainAgent).Name)
                                //{
                                //    agent = new TrainAgent(AgentIDEnumerator.GetNextID(), this, group.ID, AgentSpeedms, group.AgentTemplateList[j].NumberOfCarriges)
                                //    {
                                //        RoadGraph = this.RoadGraph,
                                //        Size = new System.Windows.Media.Media3D.Size3D(group.AgentTemplateList[j].Size.X / Map.CellSize, group.AgentTemplateList[j].Size.Y / Map.CellSize, group.AgentTemplateList[j].Size.Z / Map.CellSize),
                                //        MaxCapasity = group.AgentTemplateList[j].MaxCapasity,
                                //        InputFactor = group.AgentTemplateList[j].InputFactor,
                                //        OutputFactor = group.AgentTemplateList[j].OutputFactor
                                //    };
                                //}
            //                    if (agent != null)
            //                    {
            //                        agent.WayPointsList = new List<WayPoint>();
            //                        foreach (var point in group.AgentTemplateList[j].WayPointsList)
            //                        {
            //                            agent.WayPointsList.Add(point);
            //                        }
            //                        agent.SpeedRatio = SpeedRatio;
            //                        lock (agentsList)
            //                        {
            //                            agentsList.Add(agent);
            //                        }
            //                        agent.Start();
            //                    }
            //                }
            //                catch (IOException)
            //                {
            //                    break;
            //                }
            //            }
            //        }
            //        if (serverSocket.Connected)
            //        {
            //            serverSocket.Close();
            //        }
            //    }
            //}
            //else
            //{
            //    while (true)
            //    {
            //        int count = 0;
            //        while (count < group.Max && (count + 1) * group.AgentInterval <= group.GroupInterval)
            //        {
            //            if ((sender as BackgroundWorker).CancellationPending)
            //            {
            //                break;
            //            }
            //            if (IsPaused)
            //            {
            //                break;
            //            }
            //            int j = 0;
            //            double value = rand.NextDouble();
            //            while (value > 0 && j < group.AgentTemplateList.Count)
            //            {
            //                value -= group.AgentTemplateList[j].Persent;
            //                j++;
            //            }
            //            j--;
            //            int AgentSpeedms = Convert.ToInt32(0.5 * 3600 * 1000 / (rand.Next(Convert.ToInt32(group.AgentTemplateList[j].MinSpeed * 1000), Convert.ToInt32(group.AgentTemplateList[j].MaxSpeed * 1000))));
            //            AgentBase agent = null;
            //            if (group.AgentTemplateList[j].Type == typeof(HumanAgent).ToString())
            //            {
            //                agent = new HumanAgent(IDEmunerator.GetNextID(), this, group.ID, AgentSpeedms);
            //            }
            //            else if (group.AgentTemplateList[j].Type == typeof(BusAgent).ToString())
            //            {
            //                agent = new BusAgent(IDEmunerator.GetNextID(), this, group.ID, AgentSpeedms)
            //                {
            //                    RoadGraph = this.RoadGraph,
            //                    Size = new System.Windows.Media.Media3D.Size3D(12 / Map.CellSize, 2.25 / Map.CellSize, 2.5 / Map.CellSize),
            //                    MaxCapasity = 120,
            //                    CurrentAgentCount = new Random().Next(100),
            //                    InputFactor = new Random().NextDouble(),
            //                    OutputFactor = new Random().NextDouble()
            //                };
            //            }
            //            if (agent != null)
            //            {
            //                agent.WayPointsList = new List<WayPoint>();
            //                foreach (var point in group.AgentTemplateList[j].WayPointsList)
            //                {
            //                    agent.WayPointsList.Add(point);
            //                }
            //                agent.SpeedRatio = SpeedRatio;
            //                lock (agentsList)
            //                {
            //                    agentsList.Add(agent);
            //                }
            //                agent.Start();
            //            }
            //            count++;
            //            Thread.CurrentThread.Join(TimeSpan.FromMilliseconds(group.AgentInterval / _speedRatio));
            //        }
            //        Thread.CurrentThread.Join(TimeSpan.FromMilliseconds((group.GroupInterval - group.AgentInterval * count) / _speedRatio));
            //    }
            //}
        }

        void AcceptAgent_DoWork(object sender, DoWorkEventArgs e)
        {
            AgentsGroup group = e.Argument as AgentsGroup;
            TcpListener listener = GroupListeners[group.ID];
            while (true)
            {
                bool pending = false;
                try
                {
                    pending = listener.Pending();
                }
                catch(InvalidOperationException)
                {
                    listener = new TcpListener(System.Net.IPAddress.Parse(group.Address), group.Port);
                    listener.Start();
                    pending = listener.Pending();
                }
                if (pending)
                {
                    Socket serverSocket = listener.AcceptSocket();

                    if (serverSocket.Connected)
                    {
                        NetworkStream networkStream = new NetworkStream(serverSocket);
                        StreamReader streamReader = new StreamReader(networkStream);
                        StreamWriter streamWriter = new StreamWriter(networkStream);
                        Random rand = new Random();
                        while (true)
                        {
                            try
                            {
                                streamReader.ReadLine();
                            }
                            catch (IOException)
                            {
                                break;
                            }
                            int j = 0;
                            double value = rand.NextDouble();
                            while (value > 0 && j < group.AgentTemplateList.Count)
                            {
                                value -= group.AgentTemplateList[j].Persent;
                                j++;
                            }
                            j--;
                            int AgentSpeedms = Convert.ToInt32(MapOld.CellSize * 3600 * 1000 / (rand.Next(Convert.ToInt32(group.AgentTemplateList[j].MinSpeed * 1000), Convert.ToInt32(group.AgentTemplateList[j].MaxSpeed * 1000))));
                            AgentBase agent = null;
                            if (group.AgentTemplateList[j].Type == typeof(HumanAgent).Name)
                            {
                                agent = new HumanAgent(AgentIDEnumerator.GetNextID(), this, group.ID, AgentSpeedms);
                            }
                            else if (group.AgentTemplateList[j].Type == typeof(BusAgent).Name)
                            {
                                agent = new BusAgent(AgentIDEnumerator.GetNextID(), this, group.ID, AgentSpeedms)
                                {
                                    RoadGraph = this.RoadGraph,
                                    Size = new System.Windows.Media.Media3D.Size3D(group.AgentTemplateList[j].Length / MapOld.CellSize, group.AgentTemplateList[j].Width / MapOld.CellSize, group.AgentTemplateList[j].Height / MapOld.CellSize),
                                    MaxCapasity = group.AgentTemplateList[j].Capasity,
                                    InputFactor = group.AgentTemplateList[j].InputFactor,
                                    OutputFactor = group.AgentTemplateList[j].OutputFactor
                                };
                            }
                            else if (group.AgentTemplateList[j].Type == typeof(TrainAgent).Name)
                            {
                                agent = new TrainAgent(AgentIDEnumerator.GetNextID(), this, group.ID, AgentSpeedms, group.AgentTemplateList[j].NumberOfCarriges)
                                {
                                    RoadGraph = this.RoadGraph,
                                    Size = new System.Windows.Media.Media3D.Size3D(group.AgentTemplateList[j].Length / MapOld.CellSize, group.AgentTemplateList[j].Width / MapOld.CellSize, group.AgentTemplateList[j].Height/ MapOld.CellSize),
                                    MaxCapasity = group.AgentTemplateList[j].Capasity,
                                    InputFactor = group.AgentTemplateList[j].InputFactor,
                                    OutputFactor = group.AgentTemplateList[j].OutputFactor
                                };
                            }
                            if (agent != null)
                            {
                                agent.WayPointsList = new List<WayPoint>();
                                foreach (var point in group.AgentTemplateList[j].WayPointsList)
                                {
                                    agent.WayPointsList.Add(point);
                                }
                                agentsList.Add(agent);
                            }
                        }
                    }
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(1000));
            }
        }

        public void DoStep()
        {
            DateTime now = DateTime.Now;
            currentTime = currentTime.Add(TimeSpan.FromMilliseconds(STEP_TIME_MS));

            if (currentTime >= _scenarioConfig.StopTime)
            {
                StopScenario();
                System.Windows.MessageBox.Show("Процесс имитационного моделирования окончен", "Окончание процесса", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }
            if (_scenarioConfig.AgentInputOutput || _scenarioConfig.AgentInputOutputByGroup)
            {
                _removeCount += DoAgentRemoving();
                _addCount += DoAddAgentFromGroup();
            }
            else
            {
                DoAgentRemoving();
                DoAddAgentFromGroup();
            }
            //Выполняем обход агентов
            for (int i = 0; i < agentsList.Count; i++)
            {
                agentsList[i].Step();
            }
            //Выполняем обход сценариев
            for (int i = 0; i < ServicesList.Count;i++ )
            {
                ServicesList[i].DoStep();
            }
            DoAnalisis();
            Benchmark = (DateTime.Now - now).Ticks / 10000;
        }

        private void DoAnalisis()
        {
            if (_analisisStepCounter < _analisisViewModel.StepBetweenAnalisisCount)
            {
                _analisisStepCounter++;
                return;
            }
            _analisisStepCounter -= _analisisViewModel.StepBetweenAnalisisCount;
            if (_scenarioConfig.AverangeQueueLenght)
            {
                double value = (from service in ServicesList where service is QueueService select (service as QueueService).AverangeQueueLenght).Average();
                _analisisViewModel.AddPoint(AnalisisConstants.AVERANGE_QUEUE_LENGHT, null, currentTime.TotalSeconds, value);
            }
            //Расчет входящего и исходящего трафика
            if (_scenarioConfig.AgentInputOutput || _scenarioConfig.AgentInputOutputByGroup)
            {
                _analisis.AddAgentInputOutput(currentTime, _addCount, _removeCount);
                _analisisViewModel.AddPoint(AnalisisConstants.AGENT_INPUT_OUTPUT_NAME, "Input", currentTime.TotalSeconds, _addCount);
                _analisisViewModel.AddPoint(AnalisisConstants.AGENT_INPUT_OUTPUT_NAME, "Output", currentTime.TotalSeconds, -_removeCount);
                _addCount = 0;
                _removeCount = 0;
            }
            //Расчет плотности
            if (_scenarioConfig.SpectralDensity)
            {
                byte[,] mas = map.GetMap();
                for (int i = 0; i < mas.GetLength(0); i++)
                {
                    for (int j = 0; j < mas.GetLength(1); j++)
                    {
                        if ((mas[i, j] & 0x40) == 0x40)
                        {
                            _analisis.PassengerDensity[i, j] += 1;
                        }
                    }
                }
            }
            //Расчет количества агентов
            if (_scenarioConfig.AgentCountOnMap)
            {
                _analisisViewModel.AddPoint(AnalisisConstants.AGENT_COUNT_ON_MAP_NAME, null, currentTime.TotalSeconds, agentsList.Count);
                _analisis.AddAgentsCount(currentTime, agentsList.Count);
            }
            //Расчет количества агентов по группам
            if (_scenarioConfig.AgentCountOnMapByGroup)
            {
                List<int> grouper = new List<int>();
                foreach (var group in agentGroups)
                {
                    int count = agentsList.Count(ag => ag.Group == group.ID);
                    _analisisViewModel.AddPoint(AnalisisConstants.AGENT_COUNT_ON_MAP_BY_GROUP_NAME, group.Name, currentTime.TotalSeconds, count);
                    grouper.Add(count);
                }
                foreach (var service in ServicesList)
                {
                    if (service is StopService)
                    {
                        int count = agentsList.Count(ag => ag.Group == (service as StopService).PassengersGroup.ID);
                        _analisisViewModel.AddPoint(AnalisisConstants.AGENT_COUNT_ON_MAP_BY_GROUP_NAME, (service as StopService).PassengersGroup.Name, currentTime.TotalSeconds, count);
                        grouper.Add(count);
                    }
                }
                _analisis.AddAgentsCountByGroup(currentTime, grouper.ToArray());
            }
            _analisisViewModel.AnalisisComplit();
        }

        private int DoAgentRemoving()
        {
            if (_scenarioConfig.AgentAverangeLenght)
            {
                foreach (var agent in agentsList.FindAll(delegate(AgentBase ab) { return ab is HumanAgent && ab.WayPointsList.Count == 0; }))
                {
                    _analisis.routeLenght.Add((agent as HumanAgent).RouteLenght);
                }
            }
            if (_scenarioConfig.AgentAverangeTime)
            {
                foreach (var agent in agentsList.FindAll(delegate(AgentBase ab) { return ab is HumanAgent && ab.WayPointsList.Count == 0; }))
                {
                    _analisis.routeLenght.Add((agent as HumanAgent).RouteTime);
                }
            }
            return agentsList.RemoveAll(delegate(AgentBase ab) { return ab.WayPointsList.Count == 0; });
        }

        public static AgentBase GetRandomAgentTemplateFromGroup(List<AgentTemplate> atmass, int group_id, Scenario scenario)
        {
            int j = 0;
            Random rand = new Random();
            double value = rand.Next(1, 101);
            while (value > 0 && j < atmass.Count)
            {
                value -= atmass[j].Persent;
                j++;
            }
            j--;
            int AgentSpeedms = Convert.ToInt32(MapOld.CellSize * 3600 * 1000 / (rand.Next(Convert.ToInt32(atmass[j].MinSpeed * 1000), Convert.ToInt32(atmass[j].MaxSpeed * 1000))));
            AgentBase agent = null;
            if (atmass[j].Type == typeof(HumanAgent).Name)
            {
                agent = new HumanAgent(AgentIDEnumerator.GetNextID(), scenario, group_id, AgentSpeedms);
            }
            else if (atmass[j].Type == typeof(BusAgent).Name)
            {
                agent = new BusAgent(AgentIDEnumerator.GetNextID(), scenario, group_id, AgentSpeedms)
                {
                    RoadGraph = scenario.RoadGraph,
                    Size = new System.Windows.Media.Media3D.Size3D(atmass[j].Length / MapOld.CellSize, atmass[j].Width / MapOld.CellSize, atmass[j].Height / MapOld.CellSize),
                    MaxCapasity = atmass[j].Capasity,
                    InputFactor = atmass[j].InputFactor,
                    OutputFactor = atmass[j].OutputFactor
                };
            }
            else if (atmass[j].Type == typeof(TrainAgent).Name)
            {
                agent = new TrainAgent(AgentIDEnumerator.GetNextID(), scenario, group_id, AgentSpeedms, atmass[j].NumberOfCarriges)
                {
                    RoadGraph = scenario.RoadGraph,
                    Size = new System.Windows.Media.Media3D.Size3D(atmass[j].Length / MapOld.CellSize, atmass[j].Width / MapOld.CellSize, atmass[j].Height / MapOld.CellSize),
                    MaxCapasity = atmass[j].Capasity,
                    InputFactor = atmass[j].InputFactor,
                    OutputFactor = atmass[j].OutputFactor
                };
            }
            if (agent != null)
            {
                agent.WayPointsList = new List<WayPoint>();
                foreach (var wp in atmass[j].WayPointsList)
                {
                    agent.WayPointsList.Add((WayPoint)wp.Clone());
                }
            }
            return agent;
        }

        private int DoAddAgentFromGroup()
        {
            int add_count = 0;
            foreach (var group in agentGroups)
            {
                Random rand = new Random();
                if (!group.IsNetworkGroup)
                {
                    AgentByStepCounter[group.ID] += (double)group.AgentDistribution[Convert.ToInt32(Math.Ceiling(currentTime.TotalMinutes))] / (60000.0D / STEP_TIME_MS);
                    while (AgentByStepCounter[group.ID] >= 1)
                    {
                        AgentByStepCounter[group.ID] -= 1;
                        agentsList.Add(GetRandomAgentTemplateFromGroup(group.AgentTemplateList, group.ID, this));
                        add_count++;
                        
                    }
                }
            }
            for (int i = 0; i < ServicesList.Count;i++ )
            {
                add_count += ServicesList[i].AddCount;
            }
            return add_count;
        }
    }

    public static class AgentIDEnumerator
    {
        static int id = 0;
        internal static void Init(int first_id)
        {
            id = first_id;
        }
        internal static int GetNextID()
        {
            id++;
            return id;
        }
    }

    public static class GroupIDEnumerator
    {
        static int id = 0;
        internal static void Init(int first_id)
        {
            id = first_id;
        }
        internal static int GetNextID()
        {
            id++;
            return id;
        }
    }

    public static class ServiceIDEnumerator
    {
        static int id = 0;
        internal static void Init(int first_id)
        {
            id = first_id;
        }
        internal static int GetNextID()
        {
            id++;
            return id;
        }
    }
}
