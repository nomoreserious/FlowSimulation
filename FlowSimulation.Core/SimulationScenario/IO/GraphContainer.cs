using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using FlowSimulation.Agents;
using FlowSimulation.Map.Model;
using FlowSimulation.Helpers.Graph;

namespace FlowSimulation.SimulationScenario.IO
{
    public class GraphContainer
    {
        [XmlArray]
        [XmlArrayItem(typeof(WayPoint))]
        public List<WayPoint> VertecesList { get; set; }
        [XmlArray]
        [XmlArrayItem(typeof(EdgeContainer))]
        public List<EdgeContainer> EdgesList { get; set; }

        public GraphContainer() { }
        public GraphContainer(Graph<WayPoint, System.Windows.Media.PathFigure> graph)
        {
            VertecesList = new List<WayPoint>();
            EdgesList = new List<EdgeContainer>();
            foreach (var node in graph.Nodes)
            {
                VertecesList.Add(node);
            }
            foreach (var edge in graph.Edges)
            {
                EdgeContainer ec = new EdgeContainer()
                {
                    data = edge.Data,
                    from_id = VertecesList.IndexOf(edge.Start),
                    to_id = VertecesList.IndexOf(edge.End)
                };
                EdgesList.Add(ec);
            }
        }
    }

    [XmlInclude(typeof(System.Windows.Media.LineSegment))]
    [XmlInclude(typeof(System.Windows.Media.PolyLineSegment))]
    [XmlInclude(typeof(System.Windows.Media.BezierSegment))]
    public class EdgeContainer
    {
        [XmlAttribute]
        public int from_id { get; set; }
        [XmlAttribute]
        public int to_id { get; set; }

        public System.Windows.Media.PathFigure data { get; set; }
    }
}
