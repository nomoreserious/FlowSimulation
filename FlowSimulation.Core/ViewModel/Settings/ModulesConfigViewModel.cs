using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Helpers.MVVM;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace FlowSimulation.ViewModel.Settings
{
    public class ModuleSettingsViewModel : ViewModelBase, ICloseable
    {
        #region Private fields
        private Scenario.Model.ScenarioModel _scenario;
        private ObservableCollection<ModuleParams> _modules;
        private ModuleParams _selectedModule;
        #endregion

        #region Ctor

        /// <summary>
        /// Конструктор ModuleSettingsViewModel
        /// </summary>
        public ModuleSettingsViewModel(Scenario.Model.ScenarioModel scenario)
        {
            _scenario = scenario;
            _modules = new ObservableCollection<ModuleParams>();
            foreach (var mng in Managers.AgentManager.Instance.AgentManagers)
            {
                if (scenario.ModulesSettings.ContainsKey(mng.Metadata.Code))
                {
                    _modules.Add(new ModuleParams(mng.Metadata, mng.Value.CreateSettings(), scenario.ModulesSettings[mng.Metadata.Code]));
                }
                else
                {
                    _modules.Add(new ModuleParams(mng.Metadata, mng.Value.CreateSettings()));
                }
            }
            foreach (var mng in Managers.ServiceManager.Instance.ServiceManagers)
            {
                if (scenario.ModulesSettings.ContainsKey(mng.Metadata.Code))
                {
                    _modules.Add(new ModuleParams(mng.Metadata, mng.Value.CreateSettings(), scenario.ModulesSettings[mng.Metadata.Code]));
                }
                else
                {
                    _modules.Add(new ModuleParams(mng.Metadata, mng.Value.CreateSettings()));
                }
            }
            foreach (var mng in Managers.ViewPortManager.Instance.ViewPorts)
            {
                if (scenario.ModulesSettings.ContainsKey(mng.Metadata.Code))
                {
                    _modules.Add(new ModuleParams(mng.Metadata, mng.Value.CreateSettings(), scenario.ModulesSettings[mng.Metadata.Code]));
                }
                else
                {
                    _modules.Add(new ModuleParams(mng.Metadata, mng.Value.CreateSettings()));
                }
            }
        }

        #endregion

        #region Properties
        /// <summary>
        /// Список модулей
        /// </summary>
        public ObservableCollection<ModuleParams> Modules
        {
            get { return _modules; }
            set
            {
                _modules = value;
                base.OnPropertyChanged("Modules");
            }
        }

        /// <summary>
        /// Текущий выбранный для настройки модуль
        /// </summary>
        public ModuleParams SelectedModule
        {
            get { return _selectedModule; }
            set
            {
                _selectedModule = value;
                base.OnPropertyChanged("SelectedModule");
            }
        }

        public ICommand SaveCommand
        {
            get { return new DelegateCommand(Save); }
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(() => { DialogResult = false; CloseView = true; }); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Метод сохранения настроек
        /// </summary>
        private void Save()
        {
            foreach (var cnfg in Modules)
            {
                _scenario.ModulesSettings[cnfg.ModuleCode] = cnfg.FinalParams;
            }
            DialogResult = true;
            CloseView = true;
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
    }
}
