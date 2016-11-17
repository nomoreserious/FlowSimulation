using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulation.Contracts.Services
{
    public interface IConfigContext
    {
        Dictionary<string, object> Settings { get; set; }
    }
}
