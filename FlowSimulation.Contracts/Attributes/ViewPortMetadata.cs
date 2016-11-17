using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using FlowSimulation.Contracts.ViewPort.Metadata;

namespace FlowSimulation.Contracts.Attributes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ViewPortMetadata : ExportAttribute, IViewPortMetadata
    {
        private string _code;
        private string _name;
        private string _icon;

        public ViewPortMetadata(string code, string name, string icon = "")
            : base()
        {
            _code = code;
            _name = name;
            _icon = icon;
        }

        public string Code
        {
            get { return _code; }
        }

        public string FancyName
        {
            get { return _name; }
        }

        public string IconUri
        {
            get { return _icon; }
        }
    }
}
