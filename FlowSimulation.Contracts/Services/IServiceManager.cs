using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Configuration;
using System.Windows.Controls;
using FlowSimulation.Enviroment;

namespace FlowSimulation.Contracts.Services
{
    public interface IServiceManager : IConfigSource
    {
        UserControl ConfigControl { get; }
        IConfigContext ConfigContext { get; }
        ServiceBase GetInstance(ulong serviceId, Map map, WayPoint location, Dictionary<string, object> settings);
    }
}
