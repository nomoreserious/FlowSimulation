using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows.Markup;
using System.Windows.Media;
using System.IO;
using System.Windows;

namespace FlowSimulation.Scenario.IO
{
    sealed class PathFigureSerializationSurrogate : ISerializationSurrogate
    {
        // Save
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var s = XamlWriter.Save((PathFigure)obj);
            byte[] byteArray = Encoding.ASCII.GetBytes(s);
            info.AddValue("PathFigureData", byteArray);
        }

        // Load
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            byte[] byteArray = (byte[])info.GetValue("PathFigureData", typeof(byte[]));
            var stream = new MemoryStream(byteArray);

            var pathFigure = XamlReader.Load(stream);

            return (PathFigure)pathFigure;
        }
    }
}
