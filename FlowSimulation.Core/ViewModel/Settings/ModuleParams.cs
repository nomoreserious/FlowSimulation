using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FlowSimulation.Contracts.Configuration;
using FlowSimulation.Helpers.MVVM;

namespace FlowSimulation.ViewModel.Settings
{
    public class ModuleParams : ViewModelBase
    {
        public ModuleParams(object metadata, Dictionary<string, ParamDescriptor> moduleParams, Dictionary<string, object> existingConfig = null)
        {
            if(metadata is Contracts.Agents.Metadata.IAgentManagerMetadata)
            {
                var agentModule = (Contracts.Agents.Metadata.IAgentManagerMetadata)metadata;
                GroupName = "Менеджеры агентов";
                ModeluleName = agentModule.FancyName;
                ModuleCode = agentModule.Code;
            }
            else if (metadata is Contracts.Services.Metadata.IServiceManagerMetadata)
            {
                var serviceModule = (Contracts.Services.Metadata.IServiceManagerMetadata)metadata;
                GroupName = "Менеджеры сервисов";
                ModeluleName = serviceModule.FancyName;
                ModuleCode = serviceModule.Code;
            }
            else if (metadata is Contracts.ViewPort.Metadata.IViewPortMetadata)
            {
                var viewPort = (Contracts.ViewPort.Metadata.IViewPortMetadata)metadata;
                GroupName = "Средства визуализации";
                ModeluleName = viewPort.FancyName;
                ModuleCode = viewPort.Code;
            }
            else 
            {
                throw new ArgumentException("Неизвестный поставщик метаданных");
            }

            Params = new ObservableCollection<ModuleParam>();

            foreach (var set in moduleParams)
            {
                if (existingConfig != null && existingConfig.ContainsKey(set.Key))
                {
                    Params.Add(new ModuleParam(set.Value, existingConfig[set.Key]));
                }
                else
                {
                    Params.Add(new ModuleParam(set.Value, set.Value.DefaultValue));
                }
            }
        }

        public string GroupName { get; set; }
        public string ModeluleName { get; set; }
        public string ModuleCode { get; set; }
        public Dictionary<string, object> FinalParams
        {
            get
            {
                var dic = new Dictionary<string, object>();
                foreach (var prm in Params)
                {
                    dic.Add(prm.Code, prm.Value);
                }
                return dic;
            }
        }

        public ObservableCollection<ModuleParam> Params { get; private set; }
    }

    public class ModuleParam : ViewModelBase
    {
        private object _value;
        
        public ModuleParam(ParamDescriptor desc, object value = null)
        {
            _value = value;
            Name = desc.FancyName;
            Code = desc.Code;
            Description = desc.Description;
        }

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string StringValue
        {
            get { return (string)_value; }
            set { _value = value; }
        }

        public int IntegerValue
        {
            get { return (int)_value; }
            set { _value = value; }
        }

        public double DoubleValue
        {
            get { return (double)_value; }
            set { _value = value; }
        }

        public bool BooleanValue
        {
            get { return (bool)_value; }
            set { _value = value; }
        }

        public string Name { get; private set; }
        public string Code { get; private set; }
        public string Description { get; private set; }
    }
}
