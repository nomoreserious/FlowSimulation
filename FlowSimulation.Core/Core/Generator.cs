using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulation.Core
{
    internal sealed class Generator
    {
        private static ulong id = 0UL;
        internal void Init(ulong first_id)
        {
            id = first_id;
        }
        internal void Restart()
        {
            id = 0UL;
        }
        internal ulong GetId()
        {
            return ++id;
        }
    }
}
