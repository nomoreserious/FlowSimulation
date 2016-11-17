using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace FlowSimulation.Enviroment.Model
{
    public class GraphNode : ICloneable, IEquatable<GraphNode>
    {
        public Point SourceArea { get; set; }
        public Point TargetArea { get; set; }
        public WayPoint SourceWP { get; set; }
        public WayPoint TargetWP { get; set; }

        public int Volume { get; set; }

        public GraphNode()
        { }

        public GraphNode(Point fromArea, Point toArea)
        {
            this.SourceArea = fromArea;
            this.TargetArea = toArea;
        }

        public GraphNode(int fromA, int fromB, int toA, int toB)
            : this(new Point(fromA, fromB), new Point(toA, toB))
        { }

        public bool Equals(GraphNode other)
        {
            return this.SourceArea.Equals(other.SourceArea) &&
                   this.TargetArea.Equals(other.TargetArea) &&
                   this.SourceWP.Equals(other.SourceWP) &&
                   this.TargetWP.Equals(other.TargetWP);
        }

        public object Clone()
        {
            return new GraphNode()
            {
                SourceArea = this.SourceArea,
                TargetArea = this.TargetArea,
                SourceWP = this.SourceWP,
                TargetWP = this.TargetWP,
                Volume = this.Volume
            };
        }
    }
}
