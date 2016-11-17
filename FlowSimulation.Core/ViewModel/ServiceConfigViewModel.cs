using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Helpers.MVVM;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Contracts.Services.Metadata;
using System.Windows.Input;
using FlowSimulation.Scenario.Model;
using System.Windows.Media;
using FlowSimulation.Core;

namespace FlowSimulation.ViewModel
{
    public class ServiceConfigViewModel : ViewModelBase, ICloseable
    {
        #region Private members
        private Generator _generator = new Generator();

        private IServiceManagerMetadata _selectedServiceType;
        private System.Windows.Visibility _childWindowVisibility = System.Windows.Visibility.Hidden;
        private string _newServiceName = string.Empty;
        private ObservableCollection<ServiceModel> _servicesOnMap = new ObservableCollection<ServiceModel>();
        private ServiceModel _selectedService;
        private UserControl _configControl;
        private IConfigContext _configContext;
        private double _width;
        private double _height;
        private Brush _background;
        #endregion

        public ServiceConfigViewModel(ScenarioModel scenario)
        {
            if (scenario != null)
            {
                if (scenario.Map != null)
                {
                    try
                    {
                        Width = scenario.Map[0].Width;
                        Height = scenario.Map[0].Height;
                        if (scenario.Map[0].Substrate != null)
                        {
                            Background = new ImageBrush(Helpers.Imaging.ImageManager.BitmapToBitmapImage(scenario.Map[0].Substrate));
                        }
                    }
                    catch (NullReferenceException)
                    {
                        Width = 800;
                        Height = 600;
                        Background = Brushes.LightCoral;
                    }
                }
                if (scenario.Services != null)
                {
                    _servicesOnMap = new ObservableCollection<ServiceModel>(scenario.Services);
                    if (_servicesOnMap.Count > 0)
                    {
                        _generator.Init(_servicesOnMap.Max(s => s.Id));
                    }
                }
            }
        }

        public ICommand SaveCommand
        {
            get { return new DelegateCommand(() => { DialogResult = true; CloseView = true; }); }
        }

        public ICommand AddServiceCommand
        {
            get
            {
                return new DelegateCommand(
                    () =>
                    {
                        NewServiceName = string.Empty;
                        ChildWindowVisibility = System.Windows.Visibility.Visible;
                    });
            }
        }
        public ICommand CreateNewCommand
        {
            get
            {
                return new DelegateCommand(() =>
                    {
                        var svm = new ServiceModel(_generator.GetId(),_newServiceName, _selectedServiceType, new Dictionary<string, object>());
                        var settings = new Dictionary<string, object>();
                        foreach (var pair in Managers.ServiceManager.Instance.GetManagerByInnerCode(_selectedServiceType.Code).CreateSettings())
                        {
                            settings.Add(pair.Key, pair.Value.DefaultValue);
                        }
                        svm.Settings = settings;
                        ServicesOnMap.Add(svm);
                        ChildWindowVisibility = System.Windows.Visibility.Hidden;
                    }, () => !string.IsNullOrEmpty(_newServiceName) && _selectedServiceType != null);
            }
        }
        public ICommand CancelCreatingCommand { get { return new DelegateCommand(() => ChildWindowVisibility = System.Windows.Visibility.Hidden); } }

        public double Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged("Width"); }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged("Height"); }
        }

        public Brush Background
        {
            get { return _background; }
            set { _background = value; OnPropertyChanged("Background"); }
        }

        public System.Windows.Visibility ChildWindowVisibility
        {
            get { return _childWindowVisibility; }
            set { _childWindowVisibility = value; OnPropertyChanged("ChildWindowVisibility"); }
        }


        public IServiceManagerMetadata SelectedServiceType
        {
            get { return _selectedServiceType; }
            set { _selectedServiceType = value; }
        }

        public string NewServiceName
        {
            get { return _newServiceName; }
            set { _newServiceName = value; }
        }

        /// <summary>
        /// Доступные типы сервисов
        /// </summary>
        public IEnumerable<IServiceManagerMetadata> AvailableServices { get { return Managers.ServiceManager.Instance.ServiceManagersMetadata; } }

        /// <summary>
        /// Созданные сервисы
        /// </summary>
        public ObservableCollection<ServiceModel> ServicesOnMap
        {
            get { return _servicesOnMap; }
            set { _servicesOnMap = value; }
        }

        /// <summary>
        /// Выбранный сервис для настройки
        /// </summary>
        public ServiceModel SelectedService
        {
            get { return _selectedService; }
            set
            {
                if (_selectedService == value)
                    return;

                if (_selectedService != null && _configContext != null)
                {
                    _selectedService.Settings = _configContext.Settings;
                }

                if (_selectedService == null || _selectedService.ManagerCode != value.ManagerCode)
                {
                    var serviceManager = Managers.ServiceManager.Instance.ServiceManagers.FirstOrDefault(s => s.Metadata.Code == value.ManagerCode);
                    if (serviceManager != null)
                    {
                        ConfigControl = serviceManager.Value.ConfigControl;
                        ConfigContext = serviceManager.Value.ConfigContext;
                        
                    }
                }
                ConfigContext.Settings = value.Settings;
                ConfigControl.DataContext = ConfigContext;

                _selectedService = value;
                OnPropertyChanged("SelectedService");
            }
        }

        /// <summary>
        /// Панелька настройки выбранного сервиса
        /// </summary>
        public UserControl ConfigControl
        {
            get { return _configControl; }
            set { _configControl = value; OnPropertyChanged("ConfigControl"); }
        }

        /// <summary>
        /// Контекст панельки настройки сервиса
        /// </summary>
        public IConfigContext ConfigContext
        {
            get { return _configContext; }
            set { _configContext = value; OnPropertyChanged("ConfigContext"); }
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
    }

    //public class ServiceViewModel : ViewModelBase
    //{
    //    private Dictionary<string,object> _settings;
    //    public Dictionary<string,object> Settings
    //    {
    //        get { return _settings; }
    //        set { _settings = value; }
    //    }

    //    private string _name;
    //    public string Name
    //    {
    //        get { return _name; }
    //        set { _name = value; }
    //    }

    //    private string _managerCode;
    //    public string ManagerCode
    //    {
    //        get { return _managerCode; }
    //        set { _managerCode = value; }
    //    }
    //}
}
