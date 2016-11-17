using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Agents;
using System.ComponentModel.Composition;
using FlowSimulation.Contracts.Agents.Attributes;

namespace FlowSimulation.Agents.Vehicle
{
    [Export(typeof(IAgentManager))]
    [AgentManagerMetadata("vehicle", "Маршрутный транспорт", "lsdvshuiqwcfiqwhfbwrgwehbrwfjvuwhaegvbjwrev")]
    public class VehicleManager : IAgentManager
    {
        public AgentBase GetInstance(Enviroment.Map map, IEnumerable<Contracts.Services.AgentServiceBase> services, Dictionary<string, object> settings)
        {
            var agent = new Vehicle(map, services);
            agent.Initialize(settings);
            return agent;
        }

        public Dictionary<string, Contracts.Configuration.ParamDescriptor> CreateSettings()
        {
            return new Dictionary<string, Contracts.Configuration.ParamDescriptor>();
        }
    }
}
