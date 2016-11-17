using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using FlowSimulation.Contracts.Services.Metadata;

namespace FlowSimulation.Contracts.Services.Attributes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ServiceManagerMetadata: ExportAttribute, IServiceManagerMetadata
    {
        private string _name;
        private string _code;
        private string _unikey;

        public ServiceManagerMetadata(string fansyName, string code, string unikey)
        {
            this._code = code;
            this._name = fansyName;
            this._unikey = unikey;
        }

        public string Code { get { return _code; } }
        public string FancyName { get { return _name; } }
        public string UniKey { get { return _unikey; } }
    }
}
