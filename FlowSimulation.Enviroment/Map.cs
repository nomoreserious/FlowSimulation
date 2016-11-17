using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FlowSimulation.Enviroment.FindPathMethods;
using FlowSimulation.Enviroment.Model;
using QuickGraph;
using QuickGraph.Algorithms;

namespace FlowSimulation.Enviroment
{
    [Serializable]
    public class Map : List<Layer>
    {
        public Map()
            : base()
        { }

        public Map(IEnumerable<Layer> layers)
            : base(layers)
        { }

        [Obsolete]
        public bool? TryHoldPosition(Point position, int layerId, double weight, bool useCompression = false)
        {
            return this[layerId].TryHoldPosition(position, weight, useCompression);
        }

        [Obsolete]
        /// <summary>
        /// Освобождение занятой клетки
        /// </summary>
        public void ReleasePosition(Point position, int layerId, double weight)
        {
            this[layerId].ReleasePosition(position, weight);
        }

        public void Init()
        {
            for (int i = 0; i < this.Count; i++)
            {
                using (Enviroment.IO.MapReader reader = new Enviroment.IO.MapReader(this[i].MaskSource))
                {
                    if (reader.Read())
                    {
                        this[i] = reader.InitLayer(this[i].MaskInfo, this[i].Scale, this[i].Name);
                        this[i].CreatePatensyGraph(i);
                    }
                }
            }
        }
    }
}
