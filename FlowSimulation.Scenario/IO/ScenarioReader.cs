using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, scenario);
            }
        }

        public Scenario.Model.ScenarioModel ReadScenario()
        {
            using (Stream stream = File.Open(_path, FileMode.Open))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                Scenario.Model.ScenarioModel sc = (Scenario.Model.ScenarioModel)bformatter.Deserialize(stream);
                return sc;
            }
        }

        public void Dispose()
        {
            
        }
    }
}
