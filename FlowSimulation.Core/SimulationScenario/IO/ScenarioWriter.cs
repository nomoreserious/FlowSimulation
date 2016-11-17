using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using FlowSimulation.Agents;
using FlowSimulation.SimulationScenario.IO;
using FlowSimulation.Service;

namespace FlowSimulation.SimulationScenario
{
    class ScenarioWriter
    {
        private string _path;
        private bool _rewrite;

        public ScenarioWriter(string path, bool rewrite)
        {
            if(!Directory.Exists(_path))
                throw new DirectoryNotFoundException();
        }

        internal bool WriteScenario(Scenario scn)
        {
            if (!string.IsNullOrEmpty(_path))
            {
                using (StreamWriter writer = new StreamWriter(_path + "scenario.scn", false))
                {
                    writer.WriteLine(1);
                }
                //Пишем карту
                if (!string.IsNullOrEmpty(scn.StringMap))
                {
                    using (StreamWriter writer = new System.IO.StreamWriter(_path + "map.svg", false))
                    {
                        writer.Write(scn.StringMap);
                    }
                }
                //Пишем группы агентов
                if (scn.agentGroups != null && scn.agentGroups.Count > 0)
                {
                    try
                    {
                        using (StreamWriter writer = new System.IO.StreamWriter(_path + "AgentGroups.xml", false))
                        {
                            XmlSerializer sw = new XmlSerializer(typeof(AgentsGroup[]));
                            sw.Serialize(writer, scn.agentGroups.ToArray());
                        }
                    }
                    catch (NotSupportedException) { return false; }
                }
                //Пишем дорожную сеть
                if (scn.RoadGraph.Nodes != null)
                {
                    try
                    {
                        using (StreamWriter writer = new System.IO.StreamWriter(_path + "RoadGraph.xml", false))
                        {
                            XmlSerializer sw = new XmlSerializer(typeof(GraphContainer));
                            sw.Serialize(writer, new GraphContainer(scn.RoadGraph));
                        }
                    }
                    catch (NotSupportedException) { return false; }
                }
                //Пишем сервисы
                if (scn.ServicesList != null && scn.ServicesList.Count > 0)
                {
                    try
                    {
                        ServiceBase[] sb = scn.ServicesList.ToArray();
                        using (StreamWriter writer = new System.IO.StreamWriter(_path + "Services.xml", false))
                        {
                            XmlSerializer sw = new XmlSerializer(typeof(ServiceBase[]), new Type[] { typeof(StopService), typeof(TurnstileService), typeof(QueueService), typeof(System.Windows.Media.LineSegment), typeof(System.Windows.Media.PolyLineSegment), typeof(System.Windows.Media.BezierSegment) });
                            sw.Serialize(writer, sb);
                        }
                    }
                    catch (NotSupportedException) { return false; }
                }
                return true;
            }
            return false;
        }
    }
}
