using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace FlowSimulation.Contracts.Services.Metadata
{
    public interface IServiceManagerMetadata
    {
        string Code { get; }
        string FancyName { get; }
        string UniKey { get; }
    }
}
