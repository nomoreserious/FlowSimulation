using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulation.Enviroment
{
    public sealed class Generator
    {
        private ulong _id;

        public Generator(ulong initValue = 0UL)
        {
            _id = initValue;
        }

        public void Reset(ulong initValue = 0UL)
        {
            _id = initValue;
        }

        public ulong GetID()
        {
            return ++_id;
        }
    }
}
