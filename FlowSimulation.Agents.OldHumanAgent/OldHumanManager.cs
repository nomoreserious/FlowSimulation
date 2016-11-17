using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Agents;
using FlowSimulation.Contracts.Agents.Attributes;
using System.ComponentModel.Composition;
using FlowSimulation.Contracts.Configuration;
using FlowSimulation.Enviroment;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace FlowSimulation.Agents.OldHuman
{
    [Export(typeof(Contracts.Agents.IAgentManager))]
    [AgentManagerMetadata("old_passenger", "Человек (старая версия)", "tdPGu45gi7V6lnLKNhOIGoIGEJGS98geS76SEGgH")]
    public class OldHumanManager : IAgentManager
    {
        public AgentBase GetInstance(Enviroment.Map map, IEnumerable<Contracts.Services.AgentServiceBase> services, Dictionary<string, object> settings)
        {
            var agent = new OldHuman(map, services);
            agent.Initialize(settings);
            return agent;
        }

        public Dictionary<string, ParamDescriptor> CreateSettings()
        {
            Dictionary<string, ParamDescriptor> settings = new Dictionary<string, ParamDescriptor>();
            settings.Add("startPosition", new ParamDescriptor("startPosition", "Начальная позиция", string.Empty, new Point()));
            settings.Add("checkPoints", new ParamDescriptor("checkPoints", "Маршрутный лист", string.Empty, new List<WayPoint>()));
            settings.Add("size", new ParamDescriptor("size", "Размер", string.Empty, new Size3D()));
            settings.Add("maxSpeed", new ParamDescriptor("maxSpeed", "Максимальная скорость", string.Empty, 0));
            settings.Add("acceleration", new ParamDescriptor("acceleration", "Коэффициент ускорения", string.Empty, 0));
            settings.Add("deceleration", new ParamDescriptor("deceleration", "Коэффициент замедления", string.Empty, 0));

            return settings;
        }
    }
}
