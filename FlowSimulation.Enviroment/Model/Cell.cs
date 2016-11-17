using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Contexts;

namespace FlowSimulation.Enviroment.Model
{
    public class Cell
    {
        /// <summary>
        /// initial cell value
        /// 0 - closed
        /// 1 - мах open
        /// 2 - 255 - partly closed
        /// </summary>
        public byte StaticValue { get; private set; }

        /// <summary>
        /// current cell value
        /// </summary>
        public byte CurrentValue { get; set; }

        public bool TemporarilyClosed { get; set; }
        public bool HasAgent { get; set; }
        public double Width { get; set; }

        public Cell(byte value)
        {
            this.TemporarilyClosed = false;
            this.HasAgent = false;
            this.StaticValue = value;
            this.CurrentValue = value;
            this.Width = 0;
        }
    }
}
