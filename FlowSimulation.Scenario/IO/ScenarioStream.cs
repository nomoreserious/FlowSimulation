using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace FlowSimulation.Scenario.IO
{
    public class ScenarioStream : IDisposable
    {
        private string _path;

        public ScenarioStream(string path)
        {
            _path = path;
        }

        public void Write(Scenario.Model.ScenarioModel scenario)
        {
            using (Stream stream = File.Open(_path, FileMode.Create))
            {
                var ss = new SurrogateSelector();
                ss.AddSurrogate(typeof(System.Windows.Media.PathFigure), new StreamingContext(StreamingContextStates.All), new PathFigureSerializationSurrogate());
                
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.SurrogateSelector = ss;

                bformatter.Serialize(stream, scenario);
            }
        }

        public Scenario.Model.ScenarioModel Read()
        {
            using (Stream stream = File.Open(_path, FileMode.Open))
            {
                var ss = new SurrogateSelector();
                ss.AddSurrogate(typeof(System.Windows.Media.PathFigure), new StreamingContext(StreamingContextStates.All), new PathFigureSerializationSurrogate());
                
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.SurrogateSelector = ss;
                Scenario.Model.ScenarioModel sc = (Scenario.Model.ScenarioModel)bformatter.Deserialize(stream);
                return sc;
            }
        }

        public void Dispose()
        {
            
        }
    }
}
