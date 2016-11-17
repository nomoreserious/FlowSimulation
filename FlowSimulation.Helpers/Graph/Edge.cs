using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FlowSimulation.Helpers.Graph
{
    /// <summary>
    /// Represents additional data which is attached to an edge.
    /// </summary>
    /// <typeparam name="T">The node type</typeparam>
    /// <typeparam name="E">The edge type</typeparam>
    [Serializable]
    public struct Edge<T, E> : ISerializable where T : IEquatable<T>, ISerializable
    {
        private E data;
        private T start;
        private T end;
        /// <summary>
        /// The attached data
        /// </summary>
        public E Data
        {
            get { return data; }
            set { data = value; }
        }
        /// <summary>
        /// The start node
        /// </summary>
        public T Start
        {
            get { return start; }
        }
        /// <summary>
        /// The end node
        /// </summary>
        public T End
        {
            get { return end; }
        }
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Edge<T, E>))
                return false;
            Edge<T, E> edge = (Edge<T, E>)obj;
            return (edge.start.Equals(start) && edge.end.Equals(end));
        }
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return start.GetHashCode() ^ end.GetHashCode();
        }
        /// <summary>
        /// Creates a new instance of the Edge structure
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <param name="data">The attached data</param>
        internal Edge(T start, T end, E data)
        {
            this.start = start;
            this.end = end;
            this.data = data;
        }

        public Edge(SerializationInfo info, StreamingContext context)
        {
            this.data = (E)info.GetValue("Data", typeof(E));
            this.start = (T)info.GetValue("Start", typeof(T));
            this.end = (T)info.GetValue("End", typeof(T));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Data", this.data);
            info.AddValue("Start", this.start);
            info.AddValue("End", this.end);
        }
    }
}
