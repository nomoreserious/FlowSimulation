using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulation.Helpers.Graph
{
    public interface IGraphNode<T> : IEquatable<T>
    {
        int Value { get; set; }
        bool IsChecked { get; set; }
        T ParentNode { get; set; }
    }
}
