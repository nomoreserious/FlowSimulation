using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Agents;
using System.Drawing;

namespace FlowSimulation.Contracts.Services
{
    public abstract class AgentServiceBase : ServiceBase
    {
        public abstract bool TryGetDirection(AgentBase agent, out Point direction);
        public abstract void AddAgent(AgentBase agent);
        public abstract bool ContainsAgent(ulong Id);

        public override abstract void DoStep(double step_interval);
    }
}
