using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Helpers.MVVM;
using FlowSimulation.Scenario.Model;
using System.Windows.Input;
using System.Collections.ObjectModel;
using FlowSimulation.Enviroment;
using Xceed.Wpf.Toolkit;
using System.Windows;

namespace FlowSimulation.ViewModel
{
    public class AgentGroupConfigViewModel : ViewModelBase,ICloseable
    {
        private AgentGroupViewModel _selectedGroup;
        private ObservableCollection<AgentGroupViewModel> _agentGroups;
        private IEnumerable<WayPoint> _ioPoints;
        private Generator _idGenerator;

        public AgentGroupConfigViewModel(IEnumerable<WayPoint> ioPoints, List<AgentsGroup> groups)
        {
            if (groups != null && groups.Count > 0)
            {
                _agentGroups = new ObservableCollection<AgentGroupViewModel>(from g in groups select new AgentGroupViewModel(g, AgentTypes.FirstOrDefault(t => t.Code == g.AgentTypeCode)));
                _idGenerator = new Generator(_agentGroups.Max(a => a.Group.Id));
            }
            else
            {
                _agentGroups = new ObservableCollection<AgentGroupViewModel>();
                _idGenerator = new Generator();
            }
            _ioPoints = ioPoints;
        }

        public IEnumerable<WayPoint> InputPoints { get { return _ioPoints.Where(p => p.IsInput); } }

        public IEnumerable<WayPoint> OutputPoints { get { return _ioPoints.Where(p => p.IsOutput); } }

        public IEnumerable<AgentGroupInitType> AgentInitTypes
        {
            get { return Enum.GetValues(typeof(AgentGroupInitType)).Cast<AgentGroupInitType>(); }
        }

        public IEnumerable<Contracts.Agents.Metadata.IAgentManagerMetadata> AgentTypes
        {
            get { return Managers.AgentManager.Instance.AgentManagersMetadata; }
        }

        public ObservableCollection<AgentGroupViewModel> AgentGroups
        {
            get { return _agentGroups; }
            set
            {
                _agentGroups = value;
                OnPropertyChanged("AgentGroups");
            }
        }

        public AgentGroupViewModel SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (_selectedGroup == value)
                    return;
                _selectedGroup = value;
                OnPropertyChanged("SelectedGroup");
            }
        }

        #region Commands
        public ICommand AddCommand
        {
            get { return new DelegateCommand(Add); }
        }
        public ICommand RemoveCommand
        {
            get
            {
                return new DelegateCommand(
                    () =>
                    {
                        if (Xceed.Wpf.Toolkit.MessageBox.Show("Вы дейтвительно хотите удалить группу агентов?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            AgentGroups.Remove(SelectedGroup);
                        }
                    },
                    () => SelectedGroup != null);
            }
        }
        public ICommand AgentVolumeConfigCommand
        {
            get
            {
                return new DelegateCommand(AgentVolumeConfig, () => SelectedGroup != null);
            }
        }
        public ICommand SaveCommand
        {
            get { return new DelegateCommand(() => { DialogResult = true; CloseView = true; }, CanSave); }
        } 
        #endregion

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

        private void Add()
        {
            ulong id = _idGenerator.GetID();
            AgentsGroup ag = new AgentsGroup(id, "Новая группа " + id);
            var groupVM = new AgentGroupViewModel(ag, AgentTypes.FirstOrDefault());
            AgentGroups.Add(groupVM);
            SelectedGroup = groupVM;
        }

        private void AgentVolumeConfig()
        {
            var dialog = new View.ConfigWindows.AgentsVolumeConfig(_selectedGroup.AgentsDistibution);
            if (dialog.ShowDialog() == true)
            {
                _selectedGroup.AgentsDistibution = new Dictionary<DayOfWeek, int[]>();
                foreach (var item in dialog.DaysOfWeekDistribution)
                {
                    _selectedGroup.AgentsDistibution.Add(item.Day, item.Distribution);
                }
            }
        }

        private bool CanSave()
        {
            //Все заполнены и имеют уникальные имена
            //TODO проверить остальное
            return AgentGroups.All(g => g.CanSave) && AgentGroups.Select(g => g.Name).Distinct().Count() == AgentGroups.Count;
        }
    }

    public class AgentGroupViewModel : ViewModelBase
    {
        private Contracts.Agents.Metadata.IAgentManagerMetadata _selectedAgentType;
        private AgentsGroup _group;
        private bool _hasTargetPoint;

        public AgentGroupViewModel(AgentsGroup group, Contracts.Agents.Metadata.IAgentManagerMetadata selectedAgentType)
        {
            _group = group;
            _hasTargetPoint = group.TargetPoint != null;
            SelectedAgentType = selectedAgentType;
        }


        public bool HasTargetPoint
        {
            get { return _hasTargetPoint; }
            set
            {
                _hasTargetPoint = value;
                if(value)
                {
                    OnPropertyChanged("TargetPoint");
                }
                else
                {
                    TargetPoint = null;
                }
                OnPropertyChanged("HasTargetPoint");
            }
        }

        public AgentsGroup Group { get { return _group; } }

        public Dictionary<DayOfWeek, int[]> AgentsDistibution
        {
            get { return _group.AgentsDistibution; }
            set { _group.AgentsDistibution = value; }
        }

        public Dictionary<DayOfWeek, TimeSpan[]> TimeTable
        {
            get { return _group.TimeTable; }
            set { _group.TimeTable = value; }
        }

        public string Name
        {
            get { return _group.Name; }
            set { _group.Name = value; OnPropertyChanged("Name"); }
        }

        public WayPoint SourcePoint
        {
            get { return _group.SourcePoint; }
            set
            {
                _group.SourcePoint = value;
                OnPropertyChanged("SourcePoint");
            }
        }

        public WayPoint TargetPoint
        {
            get { return _group.TargetPoint; }
            set
            {
                _group.TargetPoint = value;
                OnPropertyChanged("TargetPoint");
            }
        } 

        public AgentGroupInitType Type
        {
            get { return _group.Type; }
            set
            {
                _group.Type = value;
                OnPropertyChanged("Type");
                OnPropertyChanged("Group");
            }
        }

        public Contracts.Agents.Metadata.IAgentManagerMetadata SelectedAgentType
        {
            get { return _selectedAgentType; }
            set
            {
                _selectedAgentType = value;
                if (value != null)
                {
                    _group.AgentTypeCode = value.Code;
                }
                OnPropertyChanged("SelectedAgentType");
            }
        }
        /// <summary>
        /// TODO Допилить
        /// </summary>
        public bool CanSave { get { return SourcePoint != null && !string.IsNullOrWhiteSpace(Name); } }
    }
}
