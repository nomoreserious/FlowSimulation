using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Agents;

namespace FlowSimulation.Contracts
{
    class Generator
    {
        protected static ulong _lastAgentId = 0;
        protected static ulong _lastServiceId = 0;
        
        internal static ulong GetAgentId()
        {
            return _lastAgentId++;
        }

        internal static ulong GetServiceId()
        {
            return _lastServiceId++;
        }
    }
}
