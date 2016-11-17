using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Contracts.Services.Attributes;
using System.Windows.Controls;

namespace FlowSimulation.Services.BusStop
{
    [Export(typeof(IServiceManager))]
    [ServiceManagerMetadata("Остановка общественного транспорта", "bus_stop", "fdsjnDFBD:Fsenfuef;SEFNuirASJNF")]
    public class BusStopManager : IServiceManager
    {
        private Config.ConfigView _cv;
        private Config.ConfigViewModel _cvm;

        public BusStopManager()
        {
            _cv = new Config.ConfigView();
            _cvm = new Config.ConfigViewModel();
        }

        public System.Windows.Controls.UserControl ConfigControl
        {
            get { return _cv; }
        }

        public IConfigContext ConfigContext
        {
            get { return _cvm; }
        }

        public Dictionary<string, Contracts.Configuration.ParamDescriptor> CreateSettings()
        {
            Dictionary<string, Contracts.Configuration.ParamDescriptor> dict = new Dictionary<string, Contracts.Configuration.ParamDescriptor>();
            dict.Add("input_time_ms", new Contracts.Configuration.ParamDescriptor("input_time_ms", "Время входа","", 2500));
            return dict;
        }

        public ServiceBase GetInstance(ulong serviceId, Enviroment.Map map, Enviroment.WayPoint location, Dictionary<string, object> settings)
        {
            BusStop service = new BusStop(serviceId, map, location);
            service.Initialize(settings);
            return service;
        }
    }
}
