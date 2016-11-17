using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Helpers.MVVM;

namespace FlowSimulation.Services.Queue
{
    public class ConfigContext: ViewModelBase, IConfigContext
    {
        public Dictionary<string, object> Settings
        {
            get
            {
                var dict = new Dictionary<string, object>();
                dict.Add("direction", _priorityDirection);
                dict.Add("minTime", _minServiceTime);
                dict.Add("maxTime", _maxServiceTime);
                return dict;
            }
            set
            {
                PriorityDirection = (int)value["direction"];
                MinServiceTime = (int)value["minTime"];
                MaxServiceTime = (int)value["maxTime"];
            }
        }

        private int _priorityDirection;
        public int PriorityDirection
        {
            get { return _priorityDirection; }
            set { _priorityDirection = value; OnPropertyChanged("PriorityDirection"); }
        }

        private int _minServiceTime;
        public int MinServiceTime
        {
            get { return _minServiceTime; }
            set { _minServiceTime = value; OnPropertyChanged("MinServiceTime"); }
        }
        private int _maxServiceTime;
        public int MaxServiceTime
        {
            get { return _maxServiceTime; }
            set { _maxServiceTime = value; OnPropertyChanged("MaxServiceTime"); }
        }
    }
}
