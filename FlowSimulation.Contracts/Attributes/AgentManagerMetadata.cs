using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using FlowSimulation.Contracts.Agents.Metadata;

namespace FlowSimulation.Contracts.Agents.Attributes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class AgentManagerMetadata : ExportAttribute, IAgentManagerMetadata
    {
        string _code, _fansyName, _uniKey;

        public AgentManagerMetadata(string code, string fansyName, string uniKey):base()
        {
            _code = code;
            _fansyName = fansyName;
            _uniKey = uniKey;
        }

        public string Code
        {
            get { return _code; }
        }

        public string FancyName
        {
            get { return _fansyName; }
        }

        public string UniKey
        {
            get { return _uniKey; }
        }
    }
}
