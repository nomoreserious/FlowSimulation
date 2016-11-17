using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Services;
using System.ComponentModel.Composition;
using FlowSimulation.Contracts.Services.Attributes;
using System.Windows.Controls;
using FlowSimulation.Enviroment;

namespace FlowSimulation.Services.Queue
{
    [Export(typeof(IServiceManager))]
    [ServiceManagerMetadata("Очередь", "queue", "queue")]
    public class QueueManager : IServiceManager
    {
        private UserControl _cuc;
        private IConfigContext _cdc;

        public QueueManager()
        {
            _cuc = new ConfigControl();
            _cdc = new ConfigContext();
        }

        public UserControl ConfigControl
        {
            get { return _cuc; }
        }

        public IConfigContext ConfigContext
        {
            get { return _cdc; }
        }

        public ServiceBase GetInstance(ulong serviceId, Map map, WayPoint location, Dictionary<string, object> settings)
        {
            var service = new Queue(serviceId, map, location);
            service.Initialize(settings);
            return service;
        }

        public Dictionary<string, Contracts.Configuration.ParamDescriptor> CreateSettings()
        {
            var dict = new Dictionary<string, Contracts.Configuration.ParamDescriptor>();
            dict.Add("direction", new Contracts.Configuration.ParamDescriptor("direction", "direction","", 1));
            dict.Add("minTime", new Contracts.Configuration.ParamDescriptor("minTime", "minTime", "", 600));
            dict.Add("maxTime", new Contracts.Configuration.ParamDescriptor("maxTime", "maxTime", "", 1000));
            return dict;
        }
    }
}
