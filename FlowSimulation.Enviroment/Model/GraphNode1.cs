using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace FlowSimulation.Enviroment.Model
{
    public class GraphNode1 : Helpers.Graph.IGraphNode<GraphNode1>, ICloneable
    {
        public Point FromArea { get; set; }
        public Point ToArea { get; set; }
        public int L { get; set; }
        public int T { get; set; }
        public int R { get; set; }
        public int B { get; set; }

        public Point Center
        {
            get { return new Point((L + R) / 2, (T + B) / 2); }
        }

        public int Volume { get; set; }

        public int Value { get; set; }
        public bool IsChecked { get; set; }
        public GraphNode1 ParentNode { get; set; }

        public GraphNode1()
        {
            Value = int.MaxValue;
        }

        public GraphNode1(Point fromArea, Point toArea, Rectangle rect)
        {
            this.FromArea = fromArea;
            this.ToArea = toArea;
            this.L = rect.Left;
            this.T = rect.Top;
            this.R = rect.Right;
            this.B = rect.Bottom;
            Value = int.MaxValue;
        }

        public GraphNode1(int fromA, int fromB, int toA, int toB, Rectangle rect)
        {
            this.FromArea = new Point(fromA, fromB);
            this.ToArea = new Point(toA, toB);
            this.L = rect.Left;
            this.T = rect.Top;
            this.R = rect.Right;
            this.B = rect.Bottom;
            Value = int.MaxValue;
        }

        public bool Equals(GraphNode1 other)
        {
            return this.FromArea == other.FromArea && 
                   this.ToArea == other.ToArea && 
                   this.L == other.L &&
                   this.T == other.T &&
                   this.R == other.R &&
                   this.B == other.B;
        }

        public object Clone()
        {
            return new GraphNode1()
            {
                FromArea = this.FromArea,
                L = this.L,
                T = this.T,
                R = this.R,
                B = this.B,
                ToArea = this.ToArea,
                Volume = this.Volume
            };
        }
    }
}
