using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulation.Contracts.Agents.Metadata
{
    public interface IAgentManagerMetadata
    {
        string Code { get; }
        string FancyName { get; }
        string UniKey { get; }
    }
}
