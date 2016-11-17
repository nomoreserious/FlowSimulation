using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulation.Contracts.Configuration
{
    public interface IConfigSource
    {
        Dictionary<string, ParamDescriptor> CreateSettings();
    }

    public interface IConfigTarget
    {
        void Initialize(Dictionary<string, object> settings);
    }
}
