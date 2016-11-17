using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using FlowSimulation.Contracts.Agents;
using FlowSimulation.Contracts.ViewPort;
using FlowSimulation.Core;
using FlowSimulation.Enviroment;
using FlowSimulation.Enviroment.IO;
using FlowSimulation.Helpers.MVVM;
using FlowSimulation.Managers;
using FlowSimulation.Scenario.Model;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Collections.ObjectModel;

namespace FlowSimulation.ViewModel
{
    public class SimulationViewModel : ViewModelBase
    {
        #region Private Members
        private ScenarioModel _scenario;
        private Simulation _sim;
        private IViewPort _currentViewPort;
        private DispatcherTimer _updateTimer;

        private double _speedRatio = 1.0;
        private string _openedScenario;
        private bool _useVisualization = true;
        private bool _useFullScreenMode = false;
        private bool _scenarioLoaded = false;
        #endregion

        #region Constructors
        public SimulationViewModel()
        {
            InitCommands();

            if (Properties.Settings.Default.LastScenarios == null)
            {
                Properties.Settings.Default.LastScenarios = new System.Collections.Specialized.StringCollection();
                Properties.Settings.Default.Save();
            }
            LastScenarios = new ObservableCollection<Tuple<string, string>>();
            foreach (var item in Properties.Settings.Default.LastScenarios)
            {
                LastScenarios.Insert(0, new Tuple<string, string>(item, item.Split('\\', '/').Last()));
            }

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(200);
            _updateTimer.Tick += (s, e) => Invalidate();
        }

        public SimulationViewModel(string scenarioPath)
            : this()
        {
            if (!string.IsNullOrWhiteSpace(scenarioPath) && File.Exists(scenarioPath))
            {
                Loading(scenarioPath);
            }
        } 
        #endregion

        #region Commands
        public ICommand NewCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public ICommand StopCommand { get; private set; }
        public ICommand PlayOrPauseCommand { get; private set; }
        public ICommand FasterCommand { get; private set; }
        public ICommand SlowerCommand { get; private set; }

        public ICommand ViewPortSelectionChangedCommand { get; private set; }
        public ICommand MapConfigCommand { get; private set; }
        public ICommand TransitionGraphConfigCommand { get; private set; }
        public ICommand RoadGraphConfigCommand { get; private set; }
        public ICommand AgentGroupConfigCommand { get; private set; }
        public ICommand ServiceConfigCommand { get; private set; }
        public ICommand ModulesConfigCommand { get; private set; }
        #endregion

        #region Public Properties

        /// <summary>
        /// Флаг, открывающией функционал при заргузке или создании сценария
        /// </summary>
        public bool ScenarioLoaded
        {
            get { return _scenarioLoaded; }
            set { _scenarioLoaded = value; OnPropertyChanged("ScenarioLoaded"); }
        }
        

        public ObservableCollection<Tuple<string, string>> LastScenarios { get; private set; }

        public Tuple<string, string> SelectedScenario
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    if (!string.IsNullOrWhiteSpace(value.Item1) && File.Exists(value.Item1))
                    {
                        Open(value.Item1);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("По данному пути сценарий не найдет, возможно он перемещен или удален.");
                        LastScenarios.Remove(value);
                        Properties.Settings.Default.LastScenarios.Remove(value.Item1);
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }
        

        public double SpeedRatio
        {
            get { return _speedRatio; }
            set
            {
                if (_speedRatio == value)
                    return;
                _speedRatio = value;
                _sim.SpeedRatio = _speedRatio;
                OnPropertyChanged("SpeedRatio");
            }
        }

        public string WindowTitle
        {
            get
            {
                string scenarioName = string.Empty;
                if (_sim!=null && Properties.Settings.Default.LastScenarios.Count > 0)
                {
                    string path = Properties.Settings.Default.LastScenarios[Properties.Settings.Default.LastScenarios.Count - 1];
                    scenarioName = path.Substring(path.LastIndexOf('\\') + 1);
                    scenarioName = scenarioName.Remove(scenarioName.LastIndexOf('.'));
                    scenarioName += " - ";
                }

                return scenarioName + "Среда имитационного моделирования";
            }
        }

        public bool UseVisualization
        {
            get { return _useVisualization; }
            set
            {
                if (_useVisualization == value)
                    return;
                _useVisualization = value;
                if (!_useVisualization)
                {
                    //Чистим ViewPort;
                    CurrentViewPort.Update(new AgentBase[0]);
                }
                OnPropertyChanged("UseVisualization");
            }
        }

        public bool UseFullScreenMode
        {
            get { return _useFullScreenMode; }
            set
            {
                if (_useFullScreenMode == value)
                    return;
                _useFullScreenMode = value;
                OnPropertyChanged("UseFullScreenMode");
            }
        }

        public int AgentsCount { get { return _sim == null ? 0 : _sim.Agents.Count(); } }

        public UserControl CurrentViewPortControl
        {
            get
            {
                if (CurrentViewPort == null)
                {
                    var uc = new View.InitialPanel();
                    uc.DataContext = this;
                    return uc;
                }
                return CurrentViewPort.ViewPort;
            }
        }

        public IEnumerable<ToolBarItemViewModel> ViewPortCollection
        {
            get
            {
                return from m in ViewPortManager.Instance.ViewPortMetadatas.OrderBy(m=>m.Code) select new ToolBarItemViewModel(m.Code, m.FancyName, m.IconUri, ViewPortSelectionChangedCommand);
            }
        }

        public IViewPort CurrentViewPort
        {
            get { return _currentViewPort; }
            set
            {
                //if (_currentViewPort == value)
                //    return;
                _currentViewPort = value;
                OnPropertyChanged("CurrentViewPort");
                OnPropertyChanged("CurrentViewPortControl");
            }
        }

        public string SimulationTime { get { return _sim == null ? string.Empty : _sim.SimulationTime.ToString("HH:mm:ss"); } }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value)
                    return;
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }
        private string _busyContent;
        public string BusyContent
        {
            get { return _busyContent; }
            set
            {
                _busyContent = value;
                OnPropertyChanged("BusyContent");
            }
        } 
        #endregion

        #region Private Methods
        private void InitCommands()
        {
            NewCommand = new DelegateCommand(New);
            OpenCommand = new DelegateCommand<string>(Open);
            SaveCommand = new DelegateCommand(Save, () => ScenarioLoaded);
            CloseCommand = new DelegateCommand(() => System.Windows.Application.Current.Shutdown(0));

            StopCommand = new DelegateCommand(Stop, () => _sim != null);
            PlayOrPauseCommand = new DelegateCommand<bool>(PlayOrPause, (b) => _sim != null);
            FasterCommand = new DelegateCommand(Faster, () => _sim != null);
            SlowerCommand = new DelegateCommand(Slower, () => _sim != null);

            ViewPortSelectionChangedCommand = new DelegateCommand<string>(ViewPortSelectionChanged, (s) => _sim != null);
            MapConfigCommand = new DelegateCommand(MapConfig, () => _scenario != null);
            AgentGroupConfigCommand = new DelegateCommand(AgentGroupConfig, () => _scenario != null);
            TransitionGraphConfigCommand = new DelegateCommand(TransitionGraphConfig, () => _scenario != null && _scenario.Map != null);
            RoadGraphConfigCommand = new DelegateCommand(RoadGraphConfig, () => _scenario != null && _scenario.Map != null);
            ServiceConfigCommand = new DelegateCommand(ServiceConfiguration, () => _scenario != null && _scenario.Map != null);
            ModulesConfigCommand = new DelegateCommand(ModulesConfig, () => _scenario != null);
        }

        private void ViewPortSelectionChanged(string code)
        {
            if (string.IsNullOrEmpty(code))
                return;
            var vp = ViewPortManager.Instance.GetViewPortByCode(code);
            if (vp != null)
            {
                CurrentViewPort = vp;
                if (_scenario.ModulesSettings.ContainsKey(code))
                {
                    CurrentViewPort.Initialize(_sim.Map, _scenario.ModulesSettings[code]);
                }
                else
                {
                    CurrentViewPort.Initialize(_sim.Map, new Dictionary<string, object>());
                }
            }
        }

        public string Benchmark { get { return _sim != null ? _sim.Benchmark.ToString() : string.Empty; } }

        private void Invalidate()
        {
            if (CurrentViewPort != null && UseVisualization)
            {
                CurrentViewPort.Update(_sim.Agents);
                //System.Threading.Tasks.Task.Factory.StartNew(() => CurrentViewPort.Update(_sim.Agents));
            }

            OnPropertyChanged("AgentsCount");
            OnPropertyChanged("SimulationTime");
            OnPropertyChanged("Benchmark");
        }

        /// <summary>
        /// TEST Method
        /// </summary>
        public ScenarioModel CreateScenario()
        {
            ScenarioModel model = new ScenarioModel();
            using (StreamReader reader = new StreamReader(@"test.svg"))
            {
                List<string> mapSource = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    mapSource.Add(line);
                }
            }
            //using (MapReader mreader = new MapReader(@"test.svg"))
            //{
            //    if (mreader.Read())
            //    {
            //        model.Map = mreader.GetMap(new Dictionary<string, byte>());
            //    }
            //}
            model.AgentGroups = new List<AgentsGroup>();
            model.StartTime = new DateTime(2013, 4, 15);
            model.EndTime = new DateTime(2013, 4, 16);
            var group = new AgentsGroup(0,"");
            group.AgentsDistibution.Add(DayOfWeek.Monday, new int[] { 200, 200, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20 });
            //group.AgentTypes = new Dictionary<string, double>();
            //group.AgentTypes.Add("passenger", 1.0);
            var settings = new Dictionary<string, object>();
            settings.Add("startPosition", new Point());
            settings.Add("checkPoints", new List<WayPoint>() { new WayPoint(100, 100), new WayPoint(400, 100), new WayPoint(780, 580) });
            settings.Add("size", new Size3D(0.5, 1.8, 0.3));
            settings.Add("maxSpeed", 1);
            settings.Add("acceleration", 0);
            settings.Add("deceleration", 0);
            //group.AgentsConfig.Add("passenger", settings);
            group.Type = AgentGroupInitType.Distribution;
            group.Name = "Тестовая группа";
            group.SourcePoint = new Enviroment.WayPoint(0, 0, 0, 1, 8);
            model.AgentGroups = new List<AgentsGroup>();
            model.AgentGroups.Add(group);

            return model;
        }

        private void New()
        {
            _scenario = new ScenarioModel();
            _sim = new Core.Simulation(_scenario);

            ScenarioLoaded = true;

            SpeedRatio = 1.0D;

            if (ViewPortCollection.Count() > 0)
            {
                ViewPortSelectionChanged(ViewPortCollection.First().Code);
            }
        }

        private void Save()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Файлы сценария (*.scn)|*.scn";
            if (sfd.ShowDialog() == true)
            {
                using (Scenario.IO.ScenarioStream st = new Scenario.IO.ScenarioStream(sfd.FileName))
                {
                    st.Write(_scenario);
                }
                while (Properties.Settings.Default.LastScenarios.Contains(sfd.FileName))
                {
                    Properties.Settings.Default.LastScenarios.Remove(sfd.FileName);
                }
                Properties.Settings.Default.LastScenarios.Add(sfd.FileName);
                Properties.Settings.Default.Save();
                OnPropertyChanged("WindowTitle");
            }
        }

        private void Open(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Файлы сценария (*.scn)|*.scn";
                if (dlg.ShowDialog() == true)
                {
                    path =  dlg.FileName;
                }
            }

            if (File.Exists(path))
            {
                Loading(path);
            }
        }

        private void Loading(string path)
        {
            BusyContent = "Загрузка сценария...";
            IsBusy = true;
            
            BackgroundWorker bw = new BackgroundWorker();
            bw.RunWorkerCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    System.Windows.MessageBox.Show("Сценарий поврежден и не может быть прочитан");
                }

                _sim = new Core.Simulation(_scenario);

                ScenarioLoaded = true;

                if (ViewPortCollection.Count() > 0)
                {
                    ViewPortSelectionChanged(ViewPortCollection.First().Code);
                }

                while (Properties.Settings.Default.LastScenarios.Contains(path))
                {
                    Properties.Settings.Default.LastScenarios.Remove(path);
                }
                Properties.Settings.Default.LastScenarios.Add(path);
                Properties.Settings.Default.Save();

                OnPropertyChanged("WindowTitle");

                IsBusy = false;
            };
            bw.DoWork += (s, e) =>
            {
                string p = (string)e.Argument;
                Scenario.IO.ScenarioStream st = new Scenario.IO.ScenarioStream(p);
                _scenario = st.Read();
                
                _scenario.Map.Init();
            };
            bw.RunWorkerAsync(path);
        }

        private void PlayOrPause(bool play)
        {
            if (play)
            {
                _sim.Start();
                _updateTimer.Start();
            }
            else
            {
                _sim.Stop();
                _updateTimer.Stop();
            }
        }

        private void Stop()
        {
            _sim.Stop();
            _updateTimer.Stop();
            _sim = new Simulation(_scenario);
            Invalidate();
        }

        private void Step()
        {
            if (_sim.IsRunning)
            {
                _sim.Stop();
            }
            _sim.Step();
            Invalidate();
        }

        private void Slower()
        {
            if (SpeedRatio > 1.0)
            {
                SpeedRatio = (SpeedRatio - 1.0) <= 1.0 ? 1.0 : SpeedRatio - 1.0;
            }
            else
            {
                SpeedRatio = (SpeedRatio - 0.1) <= 0.5 ? 0.5 : SpeedRatio - 0.1;
            }
        }

        private void Faster()
        {
            if (SpeedRatio >= 1.0)
            {
                SpeedRatio = (SpeedRatio + 1.0) >= 10.0 ? 10.0 : SpeedRatio + 1.0;
            }
            else
            {
                SpeedRatio += 0.1;
            }
        }

        private void AgentGroupConfig()
        {
            var wnd = new View.ConfigWindows.AgentGroupConfigWindow();
            var vm = new ViewModel.AgentGroupConfigViewModel(_scenario.InputOutputPoints, _scenario.AgentGroups);
            wnd.DataContext = vm;
            wnd.ShowDialog();
            if (vm.DialogResult == true)
            {
                _scenario.AgentGroups = vm.AgentGroups.Select(agvm=>agvm.Group).ToList();
            }
        }

        private void ServiceConfiguration()
        {
            var wnd = new View.ConfigWindows.ServiceConfigWindow();
            wnd.Owner = Helpers.MVVM.MVVMHelper.GetActiveWindow();
            var vm = new ViewModel.ServiceConfigViewModel(_scenario);
            wnd.DataContext = vm;
            wnd.ShowDialog();
            if (vm.DialogResult == true)
            {
                _scenario.Services = vm.ServicesOnMap.ToList();
            }
        }

        private void ModulesConfig()
        {
            var wnd = new View.ConfigWindows.ModulesConfigView();
            wnd.Owner = Helpers.MVVM.MVVMHelper.GetActiveWindow();
            var vm = new ViewModel.Settings.ModuleSettingsViewModel(_scenario);
            wnd.DataContext = vm;
            wnd.ShowDialog();
        }

        private void MapConfig()
        {
            var wnd = new View.ConfigWindows.MapConfigWindow();
            wnd.Owner = Helpers.MVVM.MVVMHelper.GetActiveWindow();
            var vm = new ViewModel.MapConfigViewModel(_scenario);
            wnd.DataContext = vm;
            wnd.ShowDialog();
            if (vm.DialogResult == true)
            {
                _scenario.Map = vm.Map;
                _scenario.InputOutputPoints = vm.AllWayPoints.Select(pvm => pvm.WayPoint).ToList();
                OnPropertyChanged("MapSizeInfo");
                if (ViewPortCollection.Count() > 0)
                {
                    ViewPortSelectionChanged(ViewPortCollection.First().Code);
                }
            }
        }

        private void TransitionGraphConfig()
        {
            var wnd = new View.ConfigWindows.TransitionGraphConfigWindow();
            wnd.Owner = Helpers.MVVM.MVVMHelper.GetActiveWindow();
            var vm = new ViewModel.TransitionGraphConfigViewModel(_scenario);
            wnd.DataContext = vm;
            wnd.ShowDialog();
            if (vm.DialogResult == true)
            {
                _scenario.TransitionGraph = vm.TransitionGraph;
            }
        }

        private void RoadGraphConfig()
        {
            var wnd = new View.ConfigWindows.RoadGraphConfigWindow();
            var vm = new ViewModel.RoadGraphConfigViewModel(_scenario);
            wnd.DataContext = vm;
            wnd.Owner = Helpers.MVVM.MVVMHelper.GetActiveWindow();
            wnd.ShowDialog();
            if (vm.DialogResult == true)
            {
                _scenario.RoadGraph = new Helpers.Graph.Graph<WayPoint, string>();
                foreach (var node in vm.RoadGraph.Nodes)
                {
                    _scenario.RoadGraph.Add(node);
                }
                foreach (var edge in vm.RoadGraph.Edges)
                {
                    _scenario.RoadGraph.AddEdge(edge.Start,edge.End,edge.Data.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                _sim.RoadGraph = vm.RoadGraph;
            }
        }

        #endregion
    }
}
