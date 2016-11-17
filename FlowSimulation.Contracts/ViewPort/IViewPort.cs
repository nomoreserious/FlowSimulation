using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Agents;
using System.Windows.Controls;
using FlowSimulation.Contracts.Configuration;
using FlowSimulation.Enviroment;

namespace FlowSimulation.Contracts.ViewPort
{
    public interface IViewPort : IConfigSource
    {      
        UserControl ViewPort { get; }

        void Update(IEnumerable<AgentBase> agents);
        void Initialize(Map map, Dictionary<string, object> settings);
    }
}
