using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using FlowSimulation.ConfigWindows;
using FlowSimulation.Contracts.Agents;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Scenario.Model;
using FlowSimulation.Scenario.IO;
using Microsoft.Win32;
using System.ComponentModel;
using FlowSimulation.Helpers.Graph;
using FlowSimulation.Helpers.MVVM;
using FlowSimulation.Enviroment.Model;
using FlowSimulation.Enviroment;
using FlowSimulation.Core;

namespace FlowSimulation
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool StepMode = true;

        private DispatcherTimer mapUpdateTimer;
        private DispatcherTimer StepTimer;
        private ExperimentConfig experimentCnfg;
        private ScenarioModel scenario;

        private double _speedRatio;
        private bool _showWalls;
        private bool _showBackgroundImage;
        private bool _showMapMask;
        private bool _useAgentVisual;
        private bool _useFullScreenMode;
        private bool _use3D;

        private Size beforeMaximazeSize;
        private Point beforeMaximazeLocation;
        private TimeSpan startTime;
        private Dictionary<int, Shape> agentVisuals;
        //private Dictionary<int, Model3D> model3DCollection = new Dictionary<int, Model3D>();
        private Dictionary<int, MeshGeometry3D> meshGeometry = new Dictionary<int,MeshGeometry3D>();


        public MainWindow()
        {
            SaveCommand = new DelegateCommand<string>(SaveScenario);
            OpenCommand = new DelegateCommand(OpenScenario);
            NewCommand = new DelegateCommand(NewScenario);
            LoadMapFromSVGFileCommand = new DelegateCommand(LoadMapFromSVGFile, CanLoadMapFromSVGFile);

            StopCommand = new DelegateCommand(Stop);
            PlayCommand = new DelegateCommand<bool>(Play);
            FasterCommand = new DelegateCommand(Faster);
            SlowerCommand = new DelegateCommand(Slower);

            DataContext = this;
            InitializeComponent();

            Initiate();
        }

        public MainWindow(string path):this()
        {
            OpenScenario(path);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                try
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
                catch (Exception ex)
                {

                }

            }
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }
        public ICommand NewCommand { get; private set; }
        public ICommand LoadMapFromSVGFileCommand { get; private set; }

        public ICommand StopCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand FasterCommand { get; private set; }
        public ICommand SlowerCommand { get; private set; }

        public bool UseFullScreenMode
        {
            get { return _useFullScreenMode; }
            set
            {
                _useFullScreenMode = value;
                if (_useFullScreenMode)
                {
                    beforeMaximazeSize = new Size(this.ActualWidth, this.ActualHeight);
                    if (this.WindowState == System.Windows.WindowState.Maximized)
                    {
                        beforeMaximazeLocation = new Point(0, 0);
                    }
                    else
                    {
                        beforeMaximazeLocation = new Point(this.RestoreBounds.Top, this.RestoreBounds.Left);
                    }
                    this.WindowState = System.Windows.WindowState.Normal;
                    this.WindowStyle = System.Windows.WindowStyle.None;
                    this.ResizeMode = System.Windows.ResizeMode.NoResize;
                    this.Top = 0;
                    this.Left = 0;
                    this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                    this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                }
                else
                {
                    this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                    this.ResizeMode = System.Windows.ResizeMode.CanResize;
                    this.Top = beforeMaximazeLocation.X;
                    this.Left = beforeMaximazeLocation.Y;
                    this.Width = beforeMaximazeSize.Width;
                    this.Height = beforeMaximazeSize.Height;
                }
            }
        }

        public bool ShowWalls
        {
            get { return _showWalls; }
            set
            {
                _showWalls = value;
                if (scenario != null)
                {
                    PaintMap();
                }
            }
        }

        public bool ShowBackgroundImage
        {
            get { return _showBackgroundImage; }
            set
            {
                _showBackgroundImage = value;
                if (scenario != null)
                {
                    PaintMap();
                }
            }
        }

        public bool ShowMapMask
        {
            get { return _showMapMask; }
            set
            {
                _showMapMask = value;
                if (scenario != null)
                {
                    PaintMap();
                }
            }
        }

        public bool UseAnimation { get; set; }

        public bool UseAgentVisual
        {
            get { return _useAgentVisual; }
            set
            {
                _useAgentVisual = value;
                if (scenario != null)
                {
                    if (!value)
                    {
                        for (int i = 0; i < pnlMap.Children.Count; i++)
                        {
                            if (pnlMap.Children[i] is Shape)
                            {
                                pnlMap.Children.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }
        }

        public double SpeedRatio
        {
            get { return _speedRatio; }
            set
            {
                _speedRatio = value;
                tbSpeedRation.Text = _speedRatio.ToString();
                if (StepTimer != null)
                {
                    try
                    {
                        //StepTimer.Change(0, Convert.ToInt64(Scenario.StepTime_ms / _speedRatio));
                        StepTimer.Interval = TimeSpan.FromMilliseconds(Core.Simulation.STEP_TIME_MS / SpeedRatio);
                    }
                    catch (ObjectDisposedException) { }
                }
            }
        }

        public bool Use3D
        {
            get { return _use3D; }
            set
            {
                if (value == _use3D)
                    return;
                _use3D = value;
                if (_use3D)
                {
                    if (System.Windows.Media.RenderCapability.Tier >> 16 != 2)
                    {
                        MessageBox.Show("Данный компьютер не поддерживает аппаратное ускорение графики. Отрисовка 3D сцен невозможна");
                        _use3D = false;
                        OnPropertyChanged("Use3D");
                        return;
                    }
                    pnl2D.Visibility = System.Windows.Visibility.Hidden;
                    pnl3D.Visibility = System.Windows.Visibility.Visible;
                    agentVisuals.Clear();
                    Paint3DScena();
                }
                else
                {
                    pnl2D.Visibility = System.Windows.Visibility.Visible;
                    pnl3D.Visibility = System.Windows.Visibility.Hidden;
                    ClearViewport();
                    meshGeometry.Clear();
                    if (agentVisuals != null)
                    {
                        pnlMap.Children.Clear();
                        PaintMap();
                    }
                }
            }
        }

        public ScenarioModel Scena
        {
            get { return scenario; }
        }



        private void Initiate()
        {
            RenderOptions.SetEdgeMode(mainViewport, EdgeMode.Aliased);
            menuEnableVariator.IsEnabled = false;
            agentVisuals = new Dictionary<int, Shape>();
            mapUpdateTimer = new DispatcherTimer(DispatcherPriority.Send);
            if (StepMode)
            {
                StepTimer = new DispatcherTimer();
                StepTimer.Interval = TimeSpan.FromMilliseconds(Simulation.STEP_TIME_MS);
                StepTimer.Tick += new EventHandler(step_timer_Tick);
            }
            mapUpdateTimer.Interval = TimeSpan.FromMilliseconds(500);
            mapUpdateTimer.Tick += new EventHandler(timer_Tick);
        }      

        private void PaintMap()
        {
            tbMapSize.Text = string.Format("{0}x{1}", scenario.Map.GetMap().GetLength(0), scenario.map.GetMap().GetLength(1));
            pnlMap.Width = scenario.map.GetMap().GetLength(0);
            pnlMap.Height = scenario.map.GetMap().GetLength(1);
            if (_showMapMask)
            {
                //ImageBrush brush = new ImageBrush(scenario.Image);
                ImageBrush brush = new ImageBrush(scenario.Map.GetLayerMask(0));
                pnlMap.Background = brush;
            }
            if (_showBackgroundImage)
            {
                try
                {
                    ImageBrush brush = new ImageBrush(new BitmapImage(new Uri(Properties.Settings.Default.ScenarioPath + "image.jpg")));
                    pnlMap.Background = brush;
                }
                catch
                {
                    pnlMap.Background = Brushes.LightPink;
                }
            }
            if ((_showBackgroundImage | _showMapMask) == false)
            {
                pnlMap.Background = Brushes.White;
            }

            for (int i = 0; i < pnlMap.Children.Count; i++)
            {
                if (!(pnlMap.Children[i] is Shape))
                {
                    pnlMap.Children.RemoveAt(i);
                    i--;
                }
            }
            if (_showWalls)
            {
                for (int n = 0; n < scenario.ServicesList.Count; n++)
                {
                    if (scenario.ServicesList[n] is TurnstileService)
                    {
                        var fig = (scenario.ServicesList[n] as TurnstileService).TurnstileGeometry;
                        System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
                        path.Data = new PathGeometry(new PathFigure[] { fig });
                        path.Stroke = Brushes.LightGray;
                        path.StrokeThickness = 2.0;
                        pnlMap.Children.Add(path);
                    }
                    if (scenario.ServicesList[n] is QueueService)
                    {
                        foreach(var point in (scenario.ServicesList[n] as QueueService).InputPoints)
                        {
                            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                            rect.SetValue(Canvas.LeftProperty, (double)point.X - 1);
                            rect.SetValue(Canvas.TopProperty, (double)point.Y - 1);
                            rect.Width = point.PointWidth * 2;
                            rect.Height = point.PointHeight * 2;
                            //rect.Stroke = Brushes.Pink;
                            rect.Fill = Brushes.LimeGreen;
                            //rect.StrokeThickness = 2.0D;
                            pnlMap.Children.Add(rect);
                        }
                    }
                }
                for (int n = 0; n < scenario.paintObjectList.Count; n++)
                {
                    PaintObject obj = scenario.paintObjectList[n];
                    if (obj.GetName() == "path")
                    {
                        string data = obj.GetAttributeValue("data");
                        System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
                        path.Data = Geometry.Parse(data);
                        SVSObjectStyle style = obj.GetStyle();
                        path.Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(style.FillColor.A, style.FillColor.R, style.FillColor.G, style.FillColor.B));
                        path.Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(style.BorderColor.A, style.BorderColor.R, style.BorderColor.G, style.BorderColor.B));
                        //path.Stroke = new System.Windows.Media.SolidColorBrush(Colors.Black);
                        path.StrokeThickness = (double)style.BorderSize;
                        System.Drawing.Drawing2D.Matrix m = obj.GetTransformMatrix();
                        Matrix matrix = new Matrix((double)m.Elements[0], (double)m.Elements[1], (double)m.Elements[2], (double)m.Elements[3], (double)m.OffsetX, (double)m.OffsetY);
                        path.RenderTransform = new MatrixTransform(matrix);
                        pnlMap.Children.Add(path);
                    }
                    if (obj.GetName() == "rect")
                    {
                        double x = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("x"));
                        double y = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("y"));
                        double w = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("width"));
                        double h = PaintObject.StringToDoubleConvertor(obj.GetAttributeValue("height"));
                        SVSObjectStyle style = obj.GetStyle();

                        Rectangle rect = new Rectangle();
                        rect.SetValue(Canvas.LeftProperty, x);
                        rect.SetValue(Canvas.TopProperty, y);
                        rect.Width = w;
                        rect.Height = h;
                        rect.Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(style.FillColor.A, style.FillColor.R, style.FillColor.G, style.FillColor.B));
                        rect.Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(style.BorderColor.A, style.BorderColor.R, style.BorderColor.G, style.BorderColor.B));
                        rect.StrokeThickness = (double)style.BorderSize;
                        System.Drawing.Drawing2D.Matrix m = obj.GetTransformMatrix();
                        Matrix matrix = new Matrix((double)m.Elements[0], (double)m.Elements[1], (double)m.Elements[2], (double)m.Elements[3], (double)m.OffsetX, (double)m.OffsetY);
                        rect.RenderTransform = new MatrixTransform(matrix);
                        pnlMap.Children.Add(rect);
                    }
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            
            if (_useAgentVisual)
            {
                if (_use3D)
                {
                    meshGeometry.Clear();
                    ClearViewport();
                }
                
                for (int i = 0; i < scenario.agentsList.Count; i++)
                {
                    if (_use3D)
                    {
                        if (scenario.agentsList[i].GetPosition() != new Point(-1, -1))
                        {
                            Point3D position = new Point3D(scenario.agentsList[i].GetPosition().X - scenario.map.GetMap().GetLength(0) / 2, 0, scenario.agentsList[i].GetPosition().Y - scenario.map.GetMap().GetLength(1) / 2);

                            int key = scenario.agentsList[i].Group;
                            if (!meshGeometry.ContainsKey(key))
                            {
                                meshGeometry.Add(key, new MeshGeometry3D());
                            }

                            if (scenario.agentsList[i] is HumanAgent)
                            {
                                meshGeometry[key] = HumanAgentVisual3D.AddAgentGeometry(position, new Size3D(1.0D - new Random(scenario.agentsList[i].ID).NextDouble() / 3, 3.0D - new Random(scenario.agentsList[i].ID).NextDouble(), 1.0D - new Random(scenario.agentsList[i].ID).NextDouble() / 3), meshGeometry[key]);
                            }
                            else if (scenario.agentsList[i] is BusAgent)
                            {
                                //TODO Раньше модели создавались в коде, теперь будут подгружаться из внешнего файла
                                //meshGeometry[key] = BusAgentVisual3D.AddAgentGeometry(position, (scenario.agentsList[i] as BusAgent).Size, meshGeometry[key]);
                                mainViewport.Children.Add(BusAgentVisual3D.GetModel(position, (scenario.agentsList[i] as BusAgent).Size, (scenario.agentsList[i] as BusAgent).Angle));
                            }
                            else if (scenario.agentsList[i] is TrainAgent)
                            {
                                TrainAgent ag = (scenario.agentsList[i] as TrainAgent);
                                for (int g = 0; g < ag.Positions.Length; g++)
                                {
                                    if (ag.NeedDraw[g] == true)
                                    {
                                        position = new Point3D(ag.Positions[g].X - scenario.map.GetMap().GetLength(0) / 2, 0, ag.Positions[g].Y - scenario.map.GetMap().GetLength(1) / 2);
                                        //    meshGeometry[key] = TrainAgentVisual3D.AddAgentGeometry(position, (scenario.agentsList[i] as TrainAgent).Size, meshGeometry[key]);
                                        mainViewport.Children.Add(TrainAgentVisual3D.GetModel(position, ag.Size, ag.Angles[g]));
                                    }
                                }
                                
                            }
                        }
                    }
                    else
                    {
                        if (agentVisuals.ContainsKey(scenario.agentsList[i].ID))
                        {
                            if (agentVisuals[scenario.agentsList[i].ID].Location != scenario.agentsList[i].GetPosition())
                            {
                                //double angle;
                                //TODO Здесь раньше расчитывался угол поворота, теперь это все перехало в реализацию агента
                                if (UseAnimation)
                                {
                                    //agentVisuals[scenario.agentsList[i].ID].AngleAn = angle;
                                    agentVisuals[scenario.agentsList[i].ID].LocationAn = scenario.agentsList[i].GetPosition();
                                }
                                else
                                {
                                    //agentVisuals[scenario.agentsList[i].ID].Angle = angle;
                                    agentVisuals[scenario.agentsList[i].ID].Location = scenario.agentsList[i].GetPosition();
                                }
                            }
                        }
                        else
                        {
                            if (scenario.agentsList[i].GetPosition() != new Point(-1, -1))
                            {
                                if (scenario.agentsList[i] is HumanAgent)
                                {
                                    HumanAgentVisual ag = new HumanAgentVisual(scenario.agentsList[i]);
                                    agentVisuals.Add(scenario.agentsList[i].ID, ag);
                                    pnlMap.Children.Add(ag);
                                }
                                else if (scenario.agentsList[i] is BusAgent)
                                {
                                    BusAgentVisual ag = new BusAgentVisual(scenario.agentsList[i]);
                                    agentVisuals.Add(scenario.agentsList[i].ID, ag);
                                    pnlMap.Children.Add(ag);
                                }
                                else if (scenario.agentsList[i] is TrainAgent)
                                {
                                    TrainAgentVisual ag = new TrainAgentVisual(scenario.agentsList[i]);
                                    agentVisuals.Add(scenario.agentsList[i].ID, ag);
                                    pnlMap.Children.Add(ag);
                                }
                            }
                        }
                    }
                }
                if (Use3D)
                {
                    //Делаем группу для отрисовки агентов
                    Model3DGroup group = new Model3DGroup();
                    //Добавляем группы агентов в группу отрисофки
                    foreach (var pair in meshGeometry)
                    {
                        GeometryModel3D geom = new GeometryModel3D(pair.Value, new DiffuseMaterial(AgentVisualBase.GetGroupColor(pair.Key)));
                        group.Children.Add(geom);
                    }
                    //Создаем 3D модель
                    ModelVisual3D model = new ModelVisual3D();
                    model.Content = group;
                    //Чистим ViewPort TODO теперь он чистится раньше
                    //ClearViewport();
                    //Заполняем ViewPort моделью
                    mainViewport.Children.Add(model);
                    LightDirectionChanged();
                }
                else
                {
                    for (int i = 0; i < pnlMap.Children.Count; i++)
                    {
                        if (pnlMap.Children[i] is AgentVisualBase)
                        {
                            AgentVisualBase vag = pnlMap.Children[i] as AgentVisualBase;
                            if (!scenario.agentsList.Contains(vag.agentBase))
                            {
                                pnlMap.Children.Remove(vag);
                                agentVisuals.Remove(vag.agentBase.ID);
                                i--;
                            }
                        }
                    }
                }
            }
            if (!UseAnimation)
            {
                tbBenchmark.Text = ((200 - scenario.Benchmark - (DateTime.Now - now).Ticks / 10000) * 10).ToString();
            }
            else
            {
                tbBenchmark.Text = ((4000 - scenario.Benchmark - (DateTime.Now - now).Ticks / 10000)/2).ToString();
            }
            tbSimulationTime.Text = new DateTime().Add(scenario.currentTime).ToString("HH:mm:ss");
            tbAgentsCount.Text = scenario.agentsList.Count.ToString();
        }

        private void step_timer_Tick(object sender, EventArgs e)
        {
            if (scenario.IsStoped)
            {
                StepTimer.Stop();
                return;
            }
            scenario.DoStep();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (scenario != null)
            {
                if (MessageBox.Show("Вы действительно хотите выйти без сохранения?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                try
                {
                    if (!StepMode)
                    {
                        for (int i = 0; i < scenario.agentsList.Count; i++)
                        {
                            scenario.StopScenario();
                        }
                    }
                    else
                    {
                        StepTimer.Stop();
                        mapUpdateTimer.Stop();
                        scenario.StopScenario();
                        pnlMap.Children.Clear();
                        if (Use3D)
                        {
                            ClearViewport();
                        }
                    }
                }
                catch
                { }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as MenuItem).Header.ToString())
            {
                case "Сохранить изобращение с карты":
                    MessageBox.Show("Пока не реализованно");
                    break;
                case "Печать изображения с карты":
                    MessageBox.Show("Пока не реализованно");
                    break;
                case "Выход":
                    Application.Current.Shutdown(1);
                    break;
                case "Загрузить фоновое изображение":
                    if (scenario == null)
                    {
                        MessageBox.Show("Сначала создайте сценарий");
                        break;
                    }
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Filter = "Файлы изображений (*.jpg)|*.jpg";
                    if (ofd.ShowDialog().GetValueOrDefault())
                    {
                        try
                        {
                            File.Copy(ofd.FileName, Properties.Settings.Default.ScenarioPath + "image.jpg", true);

                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Не удалось загрузить изображение");
                        }
                    }
                    break;
                case "Создать группу агентов":
                    //wndAgentsGroupConfig wnd = new wndAgentsGroupConfig();
                    //wnd.Owner = this;
                    //if (wnd.ShowDialog().GetValueOrDefault())
                    //{
                    //    AgentsGroup group = wnd.GetGroup();
                    //    group.ID = GroupIDEnumerator.GetNextID();
                    //    scenario.agentGroups.Add(group);    
                    //}
                    break;
                case "Править группу агентов":
                    //wndAgentsGroupConfig wndc = new wndAgentsGroupConfig(scenario.agentGroups);
                    //if (wndc.ShowDialog().GetValueOrDefault())
                    //{
                    //    scenario.agentGroups = wndc.GetGroupList();
                    //}
                    break;
                case "Создать сервис":
                    //ServiceConfigWindow scwnd = new ServiceConfigWindow(scenario, false);
                    //scwnd.Owner = this;
                    //if (scwnd.ShowDialog().GetValueOrDefault())
                    //{
                    //    ServiceBase service = scwnd.GetService();
                    //    service.ID = ServiceIDEnumerator.GetNextID();
                    //    scenario.ServicesList.Add(service);
                    //}
                    break;
                //case "Править сервис":
                //    ServiceConfigWindow scwndcnfg = new ServiceConfigWindow(scenario, true);
                //    if (scwndcnfg.ShowDialog().GetValueOrDefault())
                //    {
                //        scenario.ServicesList = scwndcnfg.GetServiceList();
                //    }
                //    break;
                //case "Настройка эксперимента":
                //    wndExperimentConfig wndEC;
                //    if (experimentCnfg == null)
                //    {
                //        wndEC = new wndExperimentConfig();
                //    }
                //    else
                //    {
                //        wndEC = new wndExperimentConfig(experimentCnfg);
                //    }
                //    if (wndEC.ShowDialog().GetValueOrDefault() == true)
                //    {
                //        experimentCnfg = wndEC.Cnfg;
                //    }
                //    break;
                //case "Просмотреть статистику":
                //    //Analisis.wndDiagrammsConfig cnf = new Analisis.wndDiagrammsConfig(scenario.ScenarioAnalisis, experimentCnfg, scenario.paintObjectList);
                //    Analisis.AnalisisView view = new Analisis.AnalisisView();
                //    view.DataContext = scenario.ScenarioAnalisisViewModel;
                //    Window analisisWnd = new Window();
                //    analisisWnd.Content = view;
                //    analisisWnd.Show();
                //    break;
                //case "Сохранить результаты анализа":
                //    MessageBox.Show("Пока не реализованно");
                //    break;
                //case "Править дорожную сеть":
                //    wndRoadGraphConfig rgcnfg = null;
                //    if (scenario.RoadGraph == null)
                //    {
                //        rgcnfg = new wndRoadGraphConfig();
                //    }
                //    else
                //    {
                //        rgcnfg = new wndRoadGraphConfig(Scena.RoadGraph);
                //    }
                //    if (rgcnfg.ShowDialog() == true)
                //    {
                //        scenario.RoadGraph = rgcnfg.RoadGraph;
                //    }
                //    break;
            }           
        }

        //private void LoadMapFromSVGFile()
        //{
        //    OpenFileDialog dlg = new OpenFileDialog();
        //    dlg.Filter = "Файлы SVG (*.svg)|*.svg| Файлы XML (*.xml)|*.xml";
        //    if (dlg.ShowDialog().GetValueOrDefault())
        //    {
        //        try
        //        {
        //            StreamReader sr = new StreamReader(dlg.FileName);
        //            scenario.StringMap = sr.ReadToEnd();
        //            XmlTextReader reader = new XmlTextReader(dlg.FileName);
        //            MapReader mapreader = new MapReader(reader);
        //            scenario.map = new MapOld(mapreader.GetMap());
        //            scenario.paintObjectList = mapreader.GetPaintObjectList();
        //            scenario.Image = mapreader.GetImage();
        //            PaintMap();
        //        }
        //        catch (System.IO.FileNotFoundException)
        //        {
        //            MessageBox.Show("Не удалось загрузить карту");
        //        }
        //    }
        //}

        private bool CanLoadMapFromSVGFile()
        {
            return !(scenario == null);
        }

        private void NewScenario()
        {
            scenario = new Scenario();
            scenario.agentGroups = new List<AgentsGroup>();
            scenario.agentsList = new List<AgentBase>();
            scenario.ServicesList = new List<ServiceBase>();
            //scenario.RoadGraph = new Graph<WayPoint, PathFigure>();
            scenario.paintObjectList = new List<PaintObject>();
            scenario.map = new MapOld(600, 400);

            Properties.Settings.Default.ScenarioPath = null;

            SpeedRatio = 1.0D;
            experimentCnfg = null;
            PaintMap();
            menuEnableVariator.IsEnabled = true;
        }

        private void SaveScenario(bool saveAs)
        {
            if (!saveAs)
            {
                if (Properties.Settings.Default.ScenarioPath != null)
                {
                    SaveScenario(Properties.Settings.Default.ScenarioPath);
                    return;
                }
            }
            SaveFileDialog savedlg = new SaveFileDialog();
            if (savedlg.ShowDialog().GetValueOrDefault())
            {
                string path = savedlg.FileName.Remove(savedlg.FileName.LastIndexOf(@"\") + 1);
                SaveScenario(path);
            }
        }

        private void SaveScenario(string path)
        {
            if (new ScenarioWriter(path, true).WriteScenario(scenario))
            {
                Properties.Settings.Default.ScenarioPath = path;
                Properties.Settings.Default.Save();
            }
        }

        private void OpenScenario()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Файлы сценария (*.scn)|*.scn";
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                string path = dlg.FileName.Remove(dlg.FileName.LastIndexOf(@"\") + 1);
                OpenScenario(path);
            }
        }

        private void OpenScenario(string path)
        {
            ScenarioReader reader = null;
            try
            {
                reader = new ScenarioReader(path);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Путь не задан");
                return;
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("Пути " + path + " не существует");
                return;
            }
            Scenario sc = reader.ReadScenario();
            if (sc == null)
            {
                return;
            }
            scenario = sc;
            experimentCnfg = null;
            SpeedRatio = 1.0D;
            if (FirstGrid.Visibility == System.Windows.Visibility.Visible)
            {
                FirstGrid.Visibility = System.Windows.Visibility.Hidden;
            }
            PaintMap();
            menuEnableVariator.IsEnabled = true;
            Properties.Settings.Default.ScenarioPath = path;
            Properties.Settings.Default.Save();
        }

        private void Stop()
        {
            StepTimer.Stop();
            mapUpdateTimer.Stop();
            scenario.StopScenario();
            for (int i = 0; i < pnlMap.Children.Count; i++)
            {
                if (pnlMap.Children[i] is AgentVisualBase)
                {
                    pnlMap.Children.RemoveAt(i);
                    i--;
                }
            }
            SpeedRatio = 1.0D;
            timer_Tick(mapUpdateTimer, new EventArgs());
            PaintMap();
        }

        private void Play(bool play)
        {
            if (!play)
            {
                if (StepMode)
                {
                    StepTimer.Stop();
                }
                else
                {
                    for (int i = 0; i < scenario.agentsList.Count; i++)
                    {
                        scenario.agentsList[i].Pause();
                    }
                }
                mapUpdateTimer.Stop();
            }
            else
            {
                if (experimentCnfg == null)
                {
                    MenuItem item = new MenuItem
                    {
                        Header = "Настройка эксперимента"
                    };
                    MenuItem_Click(item, new RoutedEventArgs());
                    return;
                }
                if (scenario.agentsList == null || scenario.agentsList.Count == 0 || scenario.IsStoped)
                {
                    scenario.IsStoped = false;
                    if (StepMode)
                    {
                        scenario.InitializeStepScenario(experimentCnfg);
                        StepTimer.Start();
                    }
                    else
                    {
                        scenario.InitializeScenario(experimentCnfg);
                    }
                }
                else
                {
                    StepTimer.Start();
                    for (int i = 0; i < scenario.agentsList.Count; i++)
                    {
                        scenario.agentsList[i].Resume();
                    }
                }
                mapUpdateTimer.Start();
            }
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
            if (StepMode)
            {
                StepTimer.Interval = TimeSpan.FromMilliseconds(Scenario.STEP_TIME_MS / SpeedRatio);
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
            if (StepMode)
            {
                StepTimer.Interval = TimeSpan.FromMilliseconds(Scenario.STEP_TIME_MS / SpeedRatio);
            }
        }

        private void border_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((sender as Border).Name == "b1")
            {
                NewScenario();
                FirstGrid.Visibility = System.Windows.Visibility.Hidden;
            }
            else if ((sender as Border).Name == "b2")
            {
                OpenScenario();
            }
            else
            {
                if (Properties.Settings.Default.ScenarioPath != null)
                {
                    OpenScenario(Properties.Settings.Default.ScenarioPath);
                }   
            }
            if (scenario != null)
            {
                FirstGrid.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private Model3DGroup CreateBackgroundModelGroup(Point3D p0, Point3D p1, Point3D p2, Point3D p3, ImageBrush image)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);

            Vector3D normal = CalculateNormal(p2, p1, p0);

            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TextureCoordinates.Add(new Point(1, 0));
            mesh.TextureCoordinates.Add(new Point(1, 1));
            mesh.TextureCoordinates.Add(new Point(0, 1));
            

            GeometryModel3D model = new GeometryModel3D(mesh, new DiffuseMaterial(image));
            Model3DGroup group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }

        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private void ClearViewport()
        {
            ModelVisual3D m;
            for (int i = mainViewport.Children.Count - 1; i >= 0; i--)
            {
                m = (ModelVisual3D)mainViewport.Children[i];

                if (m.Content is Model3DGroup && !(m.Content as Model3DGroup).IsFrozen && !(m.Content as Model3DGroup).Children.Contains(DirLight))
                {
                    mainViewport.Children.Remove(m);
                }

                //if (m.Content is DirectionalLight == false && m.Content is AmbientLight == false)
                //{
                //    mainViewport.Children.Remove(m);
                //}
            }
        }

        private void Paint3DScena()
        {
            //Создаем фон
            int width = scenario.map.GetMap().GetLength(0), height = scenario.map.GetMap().GetLength(1);
            Point3D p1 = new Point3D(-width / 2, 0, -height / 2);
            Point3D p2 = new Point3D(width / 2, 0, -height / 2);
            Point3D p3 = new Point3D(width / 2, 0, height / 2);
            Point3D p4 = new Point3D(-width / 2, 0, height / 2);
            ImageBrush brush;
            if (File.Exists(Properties.Settings.Default.ScenarioPath + "image.jpg"))
            {
                brush = new ImageBrush(new BitmapImage(new Uri(Properties.Settings.Default.ScenarioPath + "image.jpg")));
            }
            else
            {
                brush = new ImageBrush(scenario.Image);
            }
            //добавляем фон в группу
            Model3DGroup background = new Model3DGroup();
            background.Children.Add(CreateBackgroundModelGroup(p1, p2, p3, p4, brush));
            background.Freeze();
            ModelVisual3D backgroundModel = new ModelVisual3D();
            backgroundModel.Content = background;
            //Заполняем ViewPort фоном
            mainViewport.Children.Add(backgroundModel);
        }

        private void LightDirectionChanged()
        {
            if (DirLight == null)
                return;
            double angle = (scenario.currentTime.TotalMinutes - 240) / 1440 * 360;
            double R = pnlMap.Height / 2;
            double x = 0, y, z;
            z = -R * Math.Cos(angle * Math.PI / 180);
            y = -R * Math.Sin(angle * Math.PI / 180);
            DirLight.Direction = new Vector3D(x, y, z);
        }

        private void CamLocationGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(CamLocationGrid);
                LocationEll.SetValue(Canvas.LeftProperty, pos.X - 5);
                LocationEll.SetValue(Canvas.TopProperty, pos.Y - 5);

                Size size = new Size((double)scenario.map.GetMap().GetLength(0), (double)scenario.map.GetMap().GetLength(1));

                double x = (size.Width + 400) * pos.X / CamLocationGrid.ActualWidth - (size.Width + 400) / 2;
                double y = sldrUpDown.Value;
                double z = (size.Height + 400) * pos.Y / CamLocationGrid.ActualHeight - (size.Height + 400) / 2;

                (mainViewport.Camera as PerspectiveCamera).Position = new Point3D(x, y, z);
                (mainViewport.Camera as PerspectiveCamera).LookDirection = new Vector3D(-x, -y, -z);
            }
        }

        private void sldrZLightDirection_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DirLight == null)
                return;
            double angle = e.NewValue;
            double R = pnlMap.Height / 2;
            double x = 0, y, z;
            z = -R * Math.Cos(angle * Math.PI / 180);
            y = -R * Math.Sin(angle * Math.PI / 180);
            DirLight.Direction = new Vector3D(x, y, z);
        }
    }
}
