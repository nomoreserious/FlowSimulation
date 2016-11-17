using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Documents;
using System.Drawing;
using System.Runtime.Serialization;

namespace FlowSimulation.Helpers.Graph
{
    [Serializable]
    public class Graph<T, E> : ICollection<T>, ISerializable where T : IEquatable<T>, ICloneable, ISerializable
    {
        private List<T> _nodes = new List<T>();
        private List<Edge<T, E>> _edges = new List<Edge<T, E>>();
        private List<T> _viewedNodes;

        private readonly bool _isDirected;
        private readonly bool _allowsReflexivity;
        /// <summary>
        /// Returns whether the graph is directed
        /// </summary>
        public bool IsDirected
        {
            get
            {
                return _isDirected;
            }
        }
        /// <summary>
        /// Returns whether the graph allows reflexivity (nodes connected to themselves)
        /// </summary>
        public bool AllowsReflexivity
        {
            get
            {
                return _allowsReflexivity;
            }
        }
        /// <summary>
        /// Returns the set of nodes in the graph
        /// </summary>
        public IEnumerable<T> Nodes
        {
            get
            {
                foreach (T node in _nodes)
                    yield return node;
            }
        }
        /// <summary>
        /// Returns the set of edges in the graph
        /// </summary>
        public IEnumerable<Edge<T, E>> Edges
        {
            get
            {
                foreach (Edge<T, E> e in _edges)
                    yield return e;
            }
        }
        ///// <summary>
        ///// Returns the set of nodes in the graph
        ///// </summary>
        //public List<T> Nodes
        //{
        //    get
        //    {
        //        return nodes;
        //    }
        //}
        ///// <summary>
        ///// Returns the set of edges in the graph
        ///// </summary>
        //public List<Edge<T, E>> Edges
        //{
        //    get
        //    {
        //        return edges;
        //    }
        //}
        /// <summary>
        /// Adds a node to the graph
        /// </summary>
        /// <param name="node">A node</param>
        public void Add(T node)
        {
            if (_nodes.Contains(node))
                throw new ArgumentException("The specified node is already in the graph");
            _nodes.Add(node);
        }
        /// <summary>
        /// Adds an edge between two existing nodes in the graph. Throws ArgumentException
        /// if the edge already exists, either node does not exist in the graph, of if the edge is illegal.
        /// </summary>
        /// <param name="start">The start node for the edge</param>
        /// <param name="end">The end node for the edge</param>
        /// <param name="data">Additional data to attach to the edge</param>
        public void AddEdge(T start, T end, E data)
        {
            if (!_allowsReflexivity && start.Equals(end))
                throw new ArgumentException("Reflexivity is not allowed");
            if (!_nodes.Contains(start))
                throw new ArgumentException("The start node is not in the graph");
            if (!_nodes.Contains(end))
                throw new ArgumentException("The end node is not in the graph");
            if (!ContainsEdge(start, end))
                _edges.Add(new Edge<T, E>(start, end, data));
            //throw new ArgumentException("The edge is already in the graph");
            
        }
        /// <summary>
        /// Determines whether the graph contains an edge starting at start, and ending at end
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <returns></returns>
        public bool ContainsEdge(T start, T end)
        {
            if (_isDirected)
            {
                foreach (Edge<T, E> p in _edges)
                {
                    if (!p.Start.Equals(start))
                        continue;
                    if (!p.End.Equals(end))
                        continue;
                    return true;
                }
                return false;
            }
            else
            {
                foreach (Edge<T, E> p in _edges)
                {
                    if (!p.Start.Equals(start) && !p.Start.Equals(end))
                        continue;
                    if (!p.End.Equals(start) && !p.End.Equals(end))
                        continue;
                    if (start.Equals(end))
                        return true;
                    if (!p.Start.Equals(p.End))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Возвращает Данные с ребра
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public E GetEdgeData(T start, T end)
        {
            if (_isDirected)
            {
                foreach (Edge<T, E> p in _edges)
                {
                    if (!p.Start.Equals(start))
                        continue;
                    if (!p.End.Equals(end))
                        continue;
                    return p.Data;
                }
                return default(E);
            }
            else
            {
                foreach (Edge<T, E> p in _edges)
                {
                    if (!p.Start.Equals(start) && !p.Start.Equals(end))
                        continue;
                    if (!p.End.Equals(start) && !p.End.Equals(end))
                        continue;
                    if (start.Equals(end))
                        return p.Data;
                    if (!p.Start.Equals(p.End))
                        return p.Data;
                }
                return default(E);
            }
        }

        /// <summary>
        /// Removes the edge from start to end, if it exists. If an edge is removed, returns true.
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <returns>True iff an edge was removed</returns>
        public bool RemoveEdge(T start, T end)
        {
            for (int edge = _edges.Count - 1; edge >= 0; edge--)
            {
                if (_edges[edge].Start.Equals(start) && _edges[edge].End.Equals(end))
                {
                    _edges.RemoveAt(edge);
                    return true;
                }
                if (!_isDirected && _edges[edge].End.Equals(start) && _edges[edge].Start.Equals(end))
                {
                    _edges.RemoveAt(edge);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns the set of edges from the given node (ie that have it as their start)
        /// </summary>
        /// <param name="start">The start node</param>
        /// <returns>The set of edges beginning at start</returns>
        public IEnumerable<Edge<T, E>> GetEdgesFrom(T start)
        {
            foreach (Edge<T, E> e in _edges)
            {
                if (e.Start.Equals(start))
                {
                    yield return e;
                    continue;
                }
                if (!_isDirected && e.End.Equals(start))
                {
                    yield return e;
                    continue;
                }
            }
        }
        /// <summary>
        /// Returns the set of edges ending at the given node
        /// </summary>
        /// <param name="end">The ending node</param>
        /// <returns>The set of edges ending at the given node</returns>
        public IEnumerable<Edge<T, E>> GetEdgesTo(T end)
        {
            foreach (Edge<T, E> e in _edges)
            {
                if (e.End.Equals(end))
                {
                    yield return e;
                    continue;
                }
                if (!_isDirected && e.Start.Equals(end))
                {
                    yield return e;
                    continue;
                }
            }
        }
        /// <summary>
        /// Returns the degree of the specified node; IE the number of edges connected to it.
        /// </summary>
        /// <param name="node">The node to find the degree of</param>
        /// <returns>The degree of the node</returns>
        public int GetDegree(T node)
        {
            int degree = 0;
            foreach (Edge<T, E> e in GetEdgesTo(node))
                degree++;
            if (_isDirected)
                foreach (Edge<T, E> e in GetEdgesFrom(node))
                    degree++;
            return degree;
        }
        /// <summary>
        /// Returns the set of nodes directly connected to the given node, where the edge starts
        /// at the given node.
        /// </summary>
        /// <param name="node">A node</param>
        /// <returns>The set of nodes</returns>
        public IEnumerable<T> GetNodesFrom(T node)
        {
            if (_isDirected)
                foreach (Edge<T, E> e in GetEdgesFrom(node))
                    yield return e.End;
            else
                foreach (Edge<T, E> e in GetEdgesFrom(node))
                {
                    if (e.Start.Equals(e.End))
                    {
                        yield return node;
                        continue;
                    }
                    if (e.Start.Equals(node))
                        yield return e.End;
                    if (e.End.Equals(node))
                        yield return e.Start;
                }
        }
        /// <summary>
        /// Returns the set of nodes directly connected to the given node, where the edge ends
        /// at the given node.
        /// </summary>
        /// <param name="node">A node</param>
        /// <returns>The set of nodes</returns>
        public IEnumerable<T> GetNodesTo(T node)
        {
            if (_isDirected)
                foreach (Edge<T, E> e in GetEdgesFrom(node))
                    yield return e.Start;
            else
                foreach (Edge<T, E> e in GetEdgesFrom(node))
                {
                    if (e.Start.Equals(e.End))
                    {
                        yield return node;
                        continue;
                    }
                    if (e.Start.Equals(node))
                        yield return e.End;
                    if (e.End.Equals(node))
                        yield return e.Start;
                }
        }
        #region ICollection<T> Members
        /// <summary>
        /// Removes all edges and nodes
        /// </summary>
        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
        }
        /// <summary>
        /// Determines if the graph contains the specified node
        /// </summary>
        /// <param name="node">The node to look for</param>
        /// <returns>True if the node is in the graph</returns>
        public bool Contains(T node)
        {
            return _nodes.Contains(node);
        }
        /// <summary>
        /// Copies the nodes to an array. Not supported.
        /// </summary>
        /// <param name="array">An array to copy to</param>
        /// <param name="arrayIndex">The array index to begin at</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException("CopyTo(T[],int) is not supported on Graph<T,E>");
        }
        /// <summary>
        /// Returns the number of nodes in the graph
        /// </summary>
        public int Count
        {
            get { return _nodes.Count; }
        }
        /// <summary>
        /// Returns whether the graph is readonly
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }
        /// <summary>
        /// Removes the specified node from the graph, along with all edges connected to it.
        /// </summary>
        /// <param name="node">The node to remove</param>
        /// <returns>True iff the node is removed</returns>
        public bool Remove(T node)
        {
            if (!_nodes.Remove(node))
                return false;
            for (int edge = _edges.Count - 1; edge >= 0; edge--)
            {
                if (_edges[edge].Start.Equals(node) || _edges[edge].End.Equals(node))
                    _edges.RemoveAt(edge);
            }
            return true;
        }
        #endregion
        #region IEnumerable<T> Members
        /// <summary>
        /// Returns an enumerator over the nodes.
        /// </summary>
        /// <returns>An enumerator of nodes</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }
        #endregion
        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator
        /// </summary>
        /// <returns>An enumerator of nodes</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }
        #endregion
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        /// <param name="directed">Whether the graph should be directed</param>
        /// <param name="allowsReflexivity">Whether the graph should allow reflexivity</param>
        public Graph(bool directed, bool allowsReflexivity)
        {
            this._isDirected = directed;
            this._allowsReflexivity = allowsReflexivity;
        }
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        /// <param name="directed">Whether the graph should be directed</param>
        public Graph(bool directed)
            : this(directed, true)
        { }
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        public Graph()
            : this(true, true)
        { }

        public Graph(SerializationInfo info, StreamingContext context)
        {

            _allowsReflexivity = info.GetBoolean("AllowsReflexivity");
            _isDirected = info.GetBoolean("IsDirected");

            _nodes = (List<T>)info.GetValue("Nodes", typeof(List<T>));
            _edges = (List<Edge<T, E>>)info.GetValue("Edges", typeof(List<Edge<T, E>>));
        }

        public bool HasChildNodes(T node)
        {
            var en = GetNodesFrom(node).GetEnumerator();
            if (en.MoveNext())
            {
                return true;
            }
            return false;
        }


        public List<T> GetNodesWay(T from, T to)
        {
            _viewedNodes = new List<T>();
            return GetNodesWayRecursive(from, to);
        }

        private List<T> GetNodesWayRecursive(T from, T to)
        {
            List<T> list = new List<T>();
            list.Add(from);
            if (_viewedNodes.Contains(from))
            {
                return new List<T>();
            }
            else
            {
                _viewedNodes.Add(from);
            }
            if (HasChildNodes(list[list.Count - 1]))
            {
                foreach (T node in GetNodesFrom(list[list.Count - 1]))
                {
                    if (node.Equals(to))
                    {
                        list.Add(node);
                        return list;
                    }
                    else
                    {
                        List<T> temp = GetNodesWayRecursive(node, to);
                        if (temp.Count == 0)
                        {
                            continue;
                        }
                        else
                        {
                            temp.Insert(0, list[list.Count - 1]);
                            return temp;
                        }
                    }
                }
            }
            return new List<T>();
        }

        public object Clone()
        {
            Graph<T, E> clone = new Graph<T, E>();
            foreach (var node in this.Nodes)
            {
                clone.Add((T)node.Clone());
            }
            foreach (var edge in Edges)
            {
                clone.AddEdge(edge.Start, edge.End, edge.Data);
            }
            return clone;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AllowsReflexivity", _allowsReflexivity);
            info.AddValue("IsDirected", _isDirected);

            info.AddValue("Nodes", _nodes, typeof(List<T>));
            info.AddValue("Edges", _edges, typeof(List<Edge<T, E>>));
        }
    }
    /// <summary>
    /// Implements a graph with nodes of type T, and no attached edge data.
    /// If you need to attach data to edges, use Graph&lt;T,E&gt; instead.
    /// </summary>
    /// <typeparam name="T">The node type</typeparam>
    public class Graph<T> : Graph<T, object> where T : IEquatable<T>, IGraphNode<T>, ICloneable, ISerializable
    {
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        /// <param name="directed">Whether the graph should be directed</param>
        /// <param name="allowsReflexivity">Whether the graph should allow reflexivity</param>
        public Graph(bool directed, bool allowsReflexivity)
            : base(directed, allowsReflexivity)
        { }
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        /// <param name="directed">Whether the graph should be directed</param>
        public Graph(bool directed)
            : base(directed)
        { }
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        public Graph()
            : base()
        { }
        /// <summary>
        /// Adds an edge between start and end. Throws ArgumentException if the edge is invalid.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void AddEdge(T start, T end)
        {
            base.AddEdge(start, end, null);
        }
    }
}
