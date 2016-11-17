using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FlowSimulation.Enviroment;
using FlowSimulation.Enviroment.IO;
using FlowSimulation.Helpers.MVVM;
using FlowSimulation.Scenario.Model;
using Microsoft.Win32;

namespace FlowSimulation.ViewModel
{
    public class MapConfigViewModel : ViewModelBase, ICloseable
    {
        #region Private Members
        private bool _isDown;
        private bool _isBusy;
        private bool _isDragging;
        private string _busyContent;
        private Point _mouseDownOnSourcePoint;
        private LayerViewModel _selectedLayer;
        private WayPointViewModel _selectedIOPoint;
        private WayPointViewModel _dragSource;
        private ScenarioModel _scenario;
        #endregion

        #region Ctor
        public MapConfigViewModel(ScenarioModel scenario)
        {
            _scenario = scenario;
            AllWayPoints = new ObservableCollection<WayPointViewModel>();
            Layers = new ObservableCollection<LayerViewModel>();

            if (_scenario != null)
            {
                Map = _scenario.Map;
                foreach (var l in Map)
                {
                    Layers.Add(new LayerViewModel(l));
                }
                if (_scenario.InputOutputPoints != null)
                {
                    AllWayPoints = new ObservableCollection<WayPointViewModel>(from p in _scenario.InputOutputPoints select new WayPointViewModel(_scenario.Services, _scenario.InputOutputPoints, p));
                }
            }
        } 
        #endregion

        #region Commands
        public ICommand SaveCommand
        {
            get { return new DelegateCommand(Save); }
        }

        public ICommand AddLayerCommand
        {
            get { return new DelegateCommand(AddLayer); }
        }

        public ICommand RemoveLayerCommand
        {
            get { return new DelegateCommand(RemoveLayer, () => _selectedLayer != null); }
        }

        public ICommand AddPointCommand
        {
            get
            {
                return new DelegateCommand(
                  () => { ConfigPoint(true); },
                  () => _selectedLayer != null);
            }
        }
        public ICommand ConfigPointCommand
        {
            get
            {
                return new DelegateCommand(
                    () => { ConfigPoint(); },
                    () => _selectedLayer != null && _selectedIOPoint != null);
            }
        }
        public ICommand RemovePointCommand
        {
            get
            {
                return new DelegateCommand(
                  () => { AllWayPoints.Remove(_selectedIOPoint); OnPropertyChanged("IOPoints"); },
                  () => _selectedIOPoint != null);
            }
        }

        public ICommand MouseLeftButtonDownCommand
        {
            get
            {
                return new DelegateCommand<UIElement>(OnMouseLeftButtonDown);
            }
        }
        public ICommand MouseLeftButtonUpCommand
        {
            get
            {
                return new DelegateCommand<UIElement>(OnMouseLeftButtonUp);
            }
        }
        public ICommand MouseMoveCommand
        {
            get
            {
                return new DelegateCommand<UIElement>(OnMouseMove);
            }
        } 
        #endregion

        #region Public Properties
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged("IsBusy"); }
        }

        public string BusyContent
        {
            get { return _busyContent; }
            set { _busyContent = value; OnPropertyChanged("BusyContent"); }
        }

        public Map Map { get; private set; }

        public ObservableCollection<LayerViewModel> Layers { get; private set; }

        public LayerViewModel SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                if (_selectedLayer == value)
                    return;
                _selectedLayer = value;
                OnPropertyChanged("SelectedLayer");
                OnPropertyChanged("IOPoints");
            }
        }

        public ObservableCollection<WayPointViewModel> AllWayPoints { get; private set; }


        public IEnumerable<WayPointViewModel> IOPoints
        {
            get { return AllWayPoints.Where(p => p.LayerId == Layers.IndexOf(_selectedLayer)); }
        }

        public WayPointViewModel SelectedIOPoint
        {
            get { return _selectedIOPoint; }
            set
            {
                if (_selectedIOPoint == value)
                    return;
                _selectedIOPoint = value;
                OnPropertyChanged("SelectedIOPoint");
            }
        }

        #region ICloseable
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { _dialogResult = value; OnPropertyChanged("DialogResult"); }
        }

        private bool _closeView;
        public bool CloseView
        {
            get { return _closeView; }
            set
            {
                _closeView = value;
                OnPropertyChanged("CloseView");
            }
        }
        #endregion

        #endregion
        /// <summary>
        /// Метод, отлавливающий захват контрольного комплекса мышью
        /// </summary>
        /// <param name="el">Источник</param>
        private void OnMouseLeftButtonDown(UIElement el)
        {
            _isDown = true;
            _mouseDownOnSourcePoint = Mouse.GetPosition(el);
            _dragSource = IOPoints.FirstOrDefault(p => new Rect(p.X, p.Y, p.Width, p.Height).Contains(_mouseDownOnSourcePoint));
            if (_dragSource == null)
            {
                _isDown = false;
            }
        }

        /// <summary>
        /// Метод, отлавливающий освобождение контрольного комплекса мышью
        /// </summary>
        /// <param name="el">Источник</param>
        private void OnMouseLeftButtonUp(UIElement el)
        {
            if (_isDown)
            {
                if (_isDragging)
                {
                    System.Windows.Input.Mouse.Capture(null);
                    _isDragging = false;
                    _dragSource.SaveCommand.Execute(null);
                    _dragSource = null;
                }
                _isDown = false;
            }
        }

        /// <summary>
        /// Метод, реализующий перемещение контрольного комплекса мышью по карте
        /// </summary>
        /// <param name="el">Источник</param>
        private void OnMouseMove(UIElement el)
        {
            if (_isDown)
            {
                Vector change = _mouseDownOnSourcePoint - Mouse.GetPosition(el);
                if (_isDragging == false && Mouse.LeftButton == MouseButtonState.Pressed && (Math.Abs(change.X) < SystemParameters.MinimumHorizontalDragDistance || Math.Abs(change.Y) < SystemParameters.MinimumVerticalDragDistance))
                {
                    Mouse.Capture(el);
                    _isDragging = true;
                }
                if (_isDragging)
                {
                    Point mousePosition = Mouse.GetPosition(el);
                    _dragSource.X = (int)mousePosition.X;
                    _dragSource.Y = (int)mousePosition.Y;
                }
            }
        }

        private void ConfigPoint(bool createNew = false)
        {
            var wnd = new View.ConfigWindows.DialogWindow();
            wnd.Owner = Helpers.MVVM.MVVMHelper.GetActiveWindow();
            wnd.Title = createNew ? "Создание точки" : "Редактирование точки";
            var cont = new View.ConfigWindows.WayPointConfigPanel();
            wnd.Content = cont;

            WayPointViewModel wpvm = null;
            if (createNew || _selectedIOPoint == null)
            {
                WayPoint wp = new WayPoint(0, 0, Layers.IndexOf(SelectedLayer), 5, 5);
                wpvm = new WayPointViewModel(_scenario.Services, AllWayPoints.Select(p => p.WayPoint), wp);
                wpvm.LayerId = Layers.IndexOf(_selectedLayer);
            }
            else
            {
                wpvm = _selectedIOPoint;
                wpvm.CloseView = false;
            }

            wnd.DataContext = wpvm;
            wnd.ShowDialog();
            if (wpvm.DialogResult == true)
            {
                if (createNew)
                {
                    AllWayPoints.Add(wpvm);
                    OnPropertyChanged("IOPoints");
                }
            }
        }

        private void AddLayer()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Векторный рисунок (*.svg)|*.svg";
            if (ofd.ShowDialog() == true)
            {
                BusyContent = "Загрузка карты";
                IsBusy = true;
                BackgroundWorker worker = new BackgroundWorker();
                worker.RunWorkerCompleted += (s, e) =>
                    {
                        IsBusy = false;
                        if (e.Error != null)
                        {
                            MessageBox.Show("При создании слоя возникла ошибка:\r\n" + e.Error.ToString());
                            return;
                        }
                        var newLayer = (LayerViewModel)e.Result;
                        Layers.Add(newLayer);
                        SelectedLayer = newLayer;
                    };
                worker.DoWork += (s, e) =>
                    {
                        using (StreamReader sr = new StreamReader(ofd.FileName))
                        {
                            string content = sr.ReadToEnd();
                            using (Enviroment.IO.MapReader reader = new Enviroment.IO.MapReader(content))
                            {
                                if (reader.Read())
                                {
                                    var layer = new LayerViewModel(reader.MapSource, "Новый слой", 1.0D);
                                    e.Result = layer;
                                }
                            }
                        }
                    };
                worker.RunWorkerAsync();
            }
        }

        private void RemoveLayer()
        {
            if (MessageBox.Show("Вы действительно хотите удалить выбранный слой?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Layers.Remove(SelectedLayer);
            }
        }

        private void Save()
        {
            BusyContent = "Создание карты транспортной сети...";
            IsBusy = true;
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (s, e) =>
            {
                DialogResult = true;
                IsBusy = false;
                CloseView = true;
            };
            worker.DoWork += (s, e) =>
            {
                Map = new Map();
                foreach (LayerViewModel lvm in Layers)
                {
                    var maskInfo = new Dictionary<string, byte>();
                    foreach (var pair in lvm.MaskInfo)
                    {
                        maskInfo.Add(pair.Key, pair.Value);
                    }
                    var layer = new Enviroment.Model.Layer(lvm.Name, lvm.MaskSource, lvm.Scale, maskInfo);
                    Map.Add(layer);
                }
                Map.Init();
            };
            worker.RunWorkerAsync();
        }
    }

    public class LayerViewModel : ViewModelBase
    {
        private string _name;
        private double _scale;

        public LayerViewModel(string maskSource, string name, double scale)
        {
            _name = name;
            _scale = scale;

            MaskSource = maskSource;
            MaskInfo = new List<MapInfoVM>();

            using (MapReader reader = new MapReader(maskSource))
            {
                if (reader.Read())
                {
                    foreach (var color in reader.Colors)
                    {
                        string colorHtmlName = string.Format("#FF{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B).ToUpper();
                        MaskInfo.Add(new MapInfoVM(colorHtmlName, 0x00));
                    }

                    System.Drawing.Bitmap bitmap = reader.CreateBitmapFromSvg((float)_scale);
                    Background = Helpers.Imaging.ImageManager.BitmapToBitmapImage(bitmap);
                }
            }
        }

        public LayerViewModel(Enviroment.Model.Layer layer)
            :this(layer.MaskSource, layer.Name, layer.Scale)
        {           
            MaskInfo = new List<MapInfoVM>();

            foreach (var pair in layer.MaskInfo)
            {
                MaskInfo.Add(new MapInfoVM(pair.Key, pair.Value));
            }
        }

        public List<MapInfoVM> MaskInfo { get; private set; }
        public string MaskSource { get; private set; }

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        public double Scale
        {
            get { return _scale; }
            set { _scale = value; OnPropertyChanged("Scale"); }
        }

        public double Width
        {
            get { return Background.Width; }
        }

        public double Height
        {
            get { return Background.Height; }
        }

        public ImageSource Background { get; private set; }
    }
}
