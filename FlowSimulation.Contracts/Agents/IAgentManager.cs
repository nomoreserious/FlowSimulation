using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Enviroment;
using FlowSimulation.Contracts.Services;
using System.Windows.Media.Media3D;
using FlowSimulation.Contracts.Configuration;

namespace FlowSimulation.Contracts.Agents
{
    public interface IAgentManager : IConfigSource
    {
        AgentBase GetInstance(Map map, IEnumerable<AgentServiceBase> services, Dictionary<string,object> settings);
    }
}
