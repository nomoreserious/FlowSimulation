using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Helpers.MVVM;

namespace FlowSimulation.Services.BusStop.Config
{
    public class ConfigViewModel : ViewModelBase, IConfigContext
    {
        private int _input_time_ms;

        public Dictionary<string, object> Settings
        {
            get
            {
                var dict = new Dictionary<string, object>();
                dict.Add("input_time_ms", InputTimeMs);
                return dict;
            }
            set
            {
                InputTimeMs = (int)value["input_time_ms"];
            }
        }

        public int InputTimeMs
        {
            get { return _input_time_ms; }
            set { _input_time_ms = value; OnPropertyChanged("InputTimeMs"); }
        }
        
    }
}
