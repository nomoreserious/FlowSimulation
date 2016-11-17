using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulation.Helpers.MVVM
{
    public interface ICloseable
    {
        bool CloseView { get; set; }
        bool? DialogResult { get; set; }
    }
}
