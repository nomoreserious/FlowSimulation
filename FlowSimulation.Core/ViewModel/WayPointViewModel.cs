using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using FlowSimulation.Enviroment;
using FlowSimulation.Helpers.MVVM;
using FlowSimulation.Scenario.Model;

namespace FlowSimulation.ViewModel
{
    public class WayPointViewModel : ViewModelBase, ICloseable
    {
        private WayPoint _wayPoint;

        public WayPointViewModel(IEnumerable<ServiceModel> services, IEnumerable<WayPoint> links, WayPoint wayPoint = null)
        {
            if (wayPoint == null)
            {
                wayPoint = new WayPoint(0, 0, 0, 5, 5);
            }

            _services = services;
            _links = links.Where(wp => wp.LayerId != wayPoint.LayerId);

            _wayPoint = wayPoint;
            _x = wayPoint.X;
            _y = wayPoint.Y;
            _width = wayPoint.Width;
            _height = wayPoint.Height;
            _name = wayPoint.Name;
            _isInput = wayPoint.IsInput;
            _isOutput = wayPoint.IsOutput;
            _isService = wayPoint.IsServicePoint;
            if (_isService)
            {
                SelectedService = services.FirstOrDefault(s => s.Id == wayPoint.ServiceId);
            }
            _isLinked = wayPoint.IsLinked;
            if (_isLinked)
            {
                SelectedLink = wayPoint.LinkedPoint;
            }
        }

        public System.Windows.Media.Brush Background
        {
            get
            {
                if (IsService)
                    return System.Windows.Media.Brushes.LightGreen;
                if (IsLinked)
                    return System.Windows.Media.Brushes.Goldenrod;
                return System.Windows.Media.Brushes.DeepPink;
            }
        }

        public WayPoint WayPoint { get { return _wayPoint; } }
        
        public int LayerId
        {
            get { return _wayPoint.LayerId; }
            set { _wayPoint.LayerId = value; }
        }

        private int _x;
        public int X
        {
            get { return _x; }
            set { _x = value; OnPropertyChanged("X"); }
        }

        private int _y;
        public int Y
        {
            get { return _y; }
            set { _y = value; OnPropertyChanged("Y"); }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged("Width"); }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged("Height"); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        private bool _isInput;
        public bool IsInput
        {
            get { return _isInput; }
            set { _isInput = value; OnPropertyChanged("IsInput"); }
        }

        private bool _isOutput;
        public bool IsOutput
        {
            get { return _isOutput; }
            set { _isOutput = value; OnPropertyChanged("IsOutput"); }
        }

        private bool _isService;
        public bool IsService
        {
            get { return _isService; }
            set { _isService = value; OnPropertyChanged("IsService"); OnPropertyChanged("Background"); }
        }

        private bool _isLinked;
        public bool IsLinked
        {
            get { return _isLinked; }
            set { _isLinked = value; OnPropertyChanged("IsLinked"); OnPropertyChanged("Background"); }
        }

        private ulong? _serviceId;
        public ulong? ServiceId
        {
            get { return _serviceId; }
            set { _serviceId = value; OnPropertyChanged("ServiceId"); }
        }

        private IEnumerable<ServiceModel> _services;
        public IEnumerable<ServiceModel> Services
        {
            get { return _services; }
        }

        private IEnumerable<WayPoint> _links;
        public IEnumerable<WayPoint> Links
        {
            get { return _links; }
        }

        private WayPoint _selectedLink;
        public WayPoint SelectedLink
        {
            get { return _selectedLink; }
            set
            {
                if (_selectedLink == value)
                    return;
                _selectedLink = value;
                OnPropertyChanged("SelectedLink");
            }
        }

        private ServiceModel _selectedService;
        public ServiceModel SelectedService
        {
            get { return _selectedService; }
            set
            {
                if (_selectedService == value)
                    return; 
                _selectedService = value;
                if (_selectedService != null)
                {
                    _serviceId = _selectedService.Id;
                }
                OnPropertyChanged("SelectedService");
            }
        }
        
        public ICommand SaveCommand 
        {
            get
            {
                return new DelegateCommand(() =>
                    {
                        _wayPoint.X = X;
                        _wayPoint.Y = Y;
                        _wayPoint.Width = Width;
                        _wayPoint.Height = Height;
                        _wayPoint.Name = Name;
                        _wayPoint.IsInput = IsInput;
                        _wayPoint.IsOutput = IsOutput;
                        _wayPoint.IsServicePoint = IsService;
                        _wayPoint.ServiceId = IsService ? ServiceId : null;
                        _wayPoint.IsLinked = IsLinked;
                        if (_wayPoint.LinkedPoint != null)
                        {
                            _wayPoint.LinkedPoint.IsLinked = false;
                            _wayPoint.LinkedPoint.LinkedPoint = null;
                        }
                        if (IsLinked)
                        {
                            _wayPoint.LinkedPoint = SelectedLink;
                            _selectedLink.IsLinked = true;
                            _selectedLink.LinkedPoint = _wayPoint;
                        }
                        else
                        {
                            _wayPoint.LinkedPoint = null;
                        }

                        DialogResult = true;
                        CloseView = true;
                    }, () => (!IsService || ServiceId.HasValue) && (!IsLinked || SelectedLink != null));
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
    }
}
