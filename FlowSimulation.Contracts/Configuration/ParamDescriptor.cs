using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulation.Contracts.Configuration
{
    public class ParamDescriptor
    {
        public object DefaultValue { get; set; }
        public string FancyName { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        public ParamDescriptor(string code, string fancyName, string description, object defaultValue = null)
        {
            this.Code = code;
            this.FancyName = fancyName;
            this.Description = description;
            this.DefaultValue = defaultValue;
        }
    }
}
