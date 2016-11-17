using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FlowSimulation.Agents;
using Microsoft.Win32;
using FlowSimulation.Service;
using System.Windows.Shapes;
using FlowSimulation.Helpers.Graph;
using FlowSimulation.Map.Model;

namespace FlowSimulation.SimulationScenario.IO
{
    public class ScenarioReader
    {
        private string _path;

        /// <summary>
        /// Constructor ScenarioReader
        /// </summary>
        /// <param name="path">Path to scenario folder</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public ScenarioReader(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException();
            _path = path;
        }

        internal Scenario ReadScenario()
        {
            Scenario sim = new Scenario();
            try
            {
                using (StreamReader test = new StreamReader(_path + "scenario.scn"))
                {}
            }
            catch (Exception)
            {
                return null;
            }
            try
            {
                using (StreamReader sr = new StreamReader(_path + "map.svg"))
                {
                    sim.StringMap = sr.ReadToEnd();
                    XmlTextReader reader = new XmlTextReader(_path + "map.svg");
                    MapReader mapreader = new MapReader(reader);
                    byte[,] temp = mapreader.GetMap();
                    sim.map = new MapOld(temp);
                    for (int i = 0; i < temp.GetLength(0); i++)
                    {
                        for (int j = 0; j < temp.GetLength(1); j++)
                        {
                            if ((temp[i, j] & 0x80) != 0x80)
                            {
                                sim.MapSquere += MapOld.CellSize * MapOld.CellSize;
                            }
                        }
                    }
                    sim.paintObjectList = mapreader.GetPaintObjectList();
                    sim.Image = mapreader.GetImage();
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                sim.map = new MapOld(600, 300);
                sim.paintObjectList = new List<PaintObject>();
                sim.Image = new System.Windows.Media.Imaging.BitmapImage();
            }

            try
            {
                using (StreamReader reader = new StreamReader(_path + "AgentGroups.xml"))
                {
                    XmlSerializer sw = new XmlSerializer(typeof(AgentsGroup[]));
                    AgentsGroup[] groups = (AgentsGroup[])sw.Deserialize(reader);
                    sim.agentGroups = new List<AgentsGroup>(groups);
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                sim.agentGroups = new List<AgentsGroup>();
            }

            try
            {
                using (StreamReader reader = new StreamReader(_path + "Services.xml"))
                {
                    XmlSerializer sw = new XmlSerializer(typeof(ServiceBase[]), new Type[] { typeof(StopService), typeof(TurnstileService), typeof(QueueService), typeof(System.Windows.Media.LineSegment), typeof(System.Windows.Media.PolyLineSegment), typeof(System.Windows.Media.BezierSegment) });
                    ServiceBase[] services = (ServiceBase[])sw.Deserialize(reader);
                    sim.ServicesList = new List<ServiceBase>(services);
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                sim.ServicesList = new List<ServiceBase>();
            }

            try
            {
                using (StreamReader reader = new StreamReader(_path + "RoadGraph.xml"))
                {
                    XmlSerializer sw = new XmlSerializer(typeof(GraphContainer));
                    GraphContainer graph = (GraphContainer)sw.Deserialize(reader);
                    Graph<WayPoint, System.Windows.Media.PathFigure> roadGraph = new Graph<WayPoint, System.Windows.Media.PathFigure>();
                    for (int i = 0; i < graph.VertecesList.Count; i++)
                    {
                        roadGraph.Add(graph.VertecesList[i]);
                    }
                    for (int i = 0; i < graph.EdgesList.Count; i++)
                    {
                        roadGraph.AddEdge(graph.VertecesList[graph.EdgesList[i].from_id], graph.VertecesList[graph.EdgesList[i].to_id], graph.EdgesList[i].data);
                    }
                    sim.RoadGraph = roadGraph;
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("Can't load Road Graph");
                sim.RoadGraph = new Graph<WayPoint, System.Windows.Media.PathFigure>();
            }

            sim.agentsList = new List<AgentBase>();

            int last_agent_id = 0, last_service_id = 0, last_group_id = 0;
            for (int i = 0; i < sim.agentGroups.Count; i++)
            {
                if (last_group_id < sim.agentGroups[i].ID)
                {
                    last_group_id = sim.agentGroups[i].ID;
                }
            }

            for (int i = 0; i < sim.ServicesList.Count; i++)
            {
                if (last_service_id < sim.ServicesList[i].ID)
                {
                    last_service_id = sim.ServicesList[i].ID;
                }
                sim.ServicesList[i].scenario = sim;
                if (sim.ServicesList[i] is StopService)
                {
                    try
                    {
                        if (last_group_id < (sim.ServicesList[i] as StopService).PassengersGroup.ID)
                        {
                            last_group_id = (sim.ServicesList[i] as StopService).PassengersGroup.ID;
                        }
                    }
                    catch { }
                }
            }
            AgentIDEnumerator.Init(last_agent_id);
            GroupIDEnumerator.Init(last_group_id);
            ServiceIDEnumerator.Init(last_service_id);
            return sim;
        }

        //internal bool WriteScenario(bool rewrite, Scenario scn)
        //{
        //    string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //    if (rewrite || string.IsNullOrEmpty(Properties.Settings.Default.ScenarioPath))
        //    {
        //        wndProgectName wnd = new wndProgectName();
        //        if (wnd.ShowDialog().GetValueOrDefault())
        //        {
        //            Properties.Settings.Default.ScenarioName = wnd.tbName.Text;
        //            Properties.Settings.Default.ScenarioPath = path + @"\Scenarios\" + wnd.tbName.Text + @"\";
        //        }
        //        else
        //        {
        //            return;
        //        }
        //    }
        //    if (!string.IsNullOrEmpty(Properties.Settings.Default.ScenarioPath))
        //    {
        //        if (!System.IO.Directory.Exists(Properties.Settings.Default.ScenarioPath))
        //        {
        //            System.IO.Directory.CreateDirectory(Properties.Settings.Default.ScenarioPath);
        //        }
        //        StreamWriter writer = new StreamWriter(Properties.Settings.Default.ScenarioPath + Properties.Settings.Default.ScenarioName + ".scn", false);
        //        writer.WriteLine(Properties.Settings.Default.ScenarioName);
        //        writer.Close();
        //        //Пишем карту
        //        if (!string.IsNullOrEmpty(scn.StringMap))
        //        {
        //            writer = new System.IO.StreamWriter(Properties.Settings.Default.ScenarioPath + "Map.svg", false);
        //            writer.Write(scn.StringMap);
        //            writer.Close();
        //        }
        //        //Пишем группы агентов
        //        if (scn.agentGroups != null && scn.agentGroups.Count > 0)
        //        {
        //            try
        //            {
        //                writer = new System.IO.StreamWriter(Properties.Settings.Default.ScenarioPath + "AgentGroups.xml", false);
        //                XmlSerializer sw = new XmlSerializer(typeof(AgentsGroup[]));
        //                sw.Serialize(writer, scn.agentGroups.ToArray());
        //            }
        //            catch (NotSupportedException)
        //            { }
        //            writer.Close();
        //        }
        //        //Пишем дорожную сеть
        //        if (scn.RoadGraph.Nodes != null)
        //        {
        //            try
        //            {
        //                writer = new System.IO.StreamWriter(Properties.Settings.Default.ScenarioPath + "RoadGraph.xml", false);
        //                XmlSerializer sw = new XmlSerializer(typeof(GraphContainer));
        //                sw.Serialize(writer, new GraphContainer(scn.RoadGraph));
        //            }
        //            catch (NotSupportedException)
        //            { }
        //            writer.Close();
        //        }
        //        //Пишем сервисы
        //        if (scn.ServicesList != null && scn.ServicesList.Count > 0)
        //        {
        //            try
        //            {
        //                ServiceBase[] sb = scn.ServicesList.ToArray();
        //                writer = new System.IO.StreamWriter(Properties.Settings.Default.ScenarioPath + "Services.xml", false);
        //                XmlSerializer sw = new XmlSerializer(typeof(ServiceBase[]), new Type[] { typeof(StopService), typeof(TurnstileService), typeof(QueueService), typeof(System.Windows.Media.LineSegment), typeof(System.Windows.Media.PolyLineSegment), typeof(System.Windows.Media.BezierSegment) });
        //                sw.Serialize(writer, sb);
        //            }
        //            catch (NotSupportedException)
        //            { }
        //            writer.Close();
        //        }
        //    }
        //}

        //internal Scenario ReadScenario()
        //{
        //    Scenario sim = new Scenario();
        //    try
        //    {
        //        StreamReader test = new StreamReader(Properties.Settings.Default.ScenarioPath + Properties.Settings.Default.ScenarioName + ".scn");
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //    try
        //    {
        //        StreamReader sr = new StreamReader(Properties.Settings.Default.ScenarioPath + "Map.svg");
        //        sim.StringMap = sr.ReadToEnd();
        //        XmlTextReader reader = new XmlTextReader(Properties.Settings.Default.ScenarioPath + "Map.svg");
        //        MapReader mapreader = new MapReader(reader);
        //        byte[,] temp = mapreader.GetMap();
        //        sim.map = new Map(temp);
        //        for (int i = 0; i < temp.GetLength(0); i++)
        //        {
        //            for (int j = 0; j < temp.GetLength(1); j++)
        //            {
        //                if ((temp[i, j] & 0x80) != 0x80)
        //                {
        //                    sim.MapSquere += Map.CellSize * Map.CellSize;
        //                }
        //            }
        //        } 
        //        sim.paintObjectList = mapreader.GetPaintObjectList();
        //        sim.Image = mapreader.GetImage();
        //    }
        //    catch (System.IO.FileNotFoundException)
        //    {
        //        sim.map = new Map(600, 300);
        //        sim.paintObjectList = new List<PaintObject>();
        //        sim.Image = new System.Windows.Media.Imaging.BitmapImage();
        //    }

        //    try
        //    {
        //        StreamReader reader = new StreamReader(Properties.Settings.Default.ScenarioPath + "AgentGroups.xml");
        //        XmlSerializer sw = new XmlSerializer(typeof(AgentsGroup[]));
        //        AgentsGroup[] groups = (AgentsGroup[])sw.Deserialize(reader);
        //        sim.agentGroups = new List<AgentsGroup>(groups);
        //    }
        //    catch (System.IO.FileNotFoundException)
        //    {
        //        sim.agentGroups = new List<AgentsGroup>();
        //    }

        //    try
        //    {
        //        StreamReader reader = new StreamReader(Properties.Settings.Default.ScenarioPath + "Services.xml");
        //        XmlSerializer sw = new XmlSerializer(typeof(ServiceBase[]), new Type[] { typeof(StopService), typeof(TurnstileService), typeof(QueueService), typeof(System.Windows.Media.LineSegment), typeof(System.Windows.Media.PolyLineSegment), typeof(System.Windows.Media.BezierSegment) });
        //        ServiceBase[] services = (ServiceBase[])sw.Deserialize(reader);
        //        sim.ServicesList = new List<ServiceBase>(services);
        //    }
        //    catch (System.IO.FileNotFoundException)
        //    {
        //        sim.ServicesList = new List<ServiceBase>();
        //    }
            
        //    try
        //    {
        //        StreamReader reader = new StreamReader(Properties.Settings.Default.ScenarioPath + "RoadGraph.xml");
        //        XmlSerializer sw = new XmlSerializer(typeof(GraphContainer));
        //        GraphContainer graph = (GraphContainer)sw.Deserialize(reader);
        //        Graph<WayPoint, System.Windows.Media.PathFigure> roadGraph = new Graph<WayPoint, System.Windows.Media.PathFigure>();
        //        for (int i = 0; i < graph.VertecesList.Count; i++)
        //        {
        //            roadGraph.Add(graph.VertecesList[i]);
        //        }
        //        for (int i = 0; i < graph.EdgesList.Count; i++)
        //        {
        //            roadGraph.AddEdge(graph.VertecesList[graph.EdgesList[i].from_id], graph.VertecesList[graph.EdgesList[i].to_id], graph.EdgesList[i].data);
        //        }
        //        sim.RoadGraph = roadGraph;
        //    }
        //    catch (System.IO.FileNotFoundException)
        //    {
        //        Console.WriteLine("Can't load Road Graph");
        //        sim.RoadGraph = new Graph<WayPoint, System.Windows.Media.PathFigure>();
        //    }

        //    sim.agentsList = new List<AgentBase>();

        //    int last_agent_id = 0, last_service_id = 0, last_group_id = 0;
        //    for (int i = 0; i < sim.agentGroups.Count; i++)
        //    {
        //        if (last_group_id < sim.agentGroups[i].ID)
        //        {
        //            last_group_id = sim.agentGroups[i].ID;
        //        }
        //    }

        //    for (int i = 0; i < sim.ServicesList.Count; i++)
        //    {
        //        if (last_service_id < sim.ServicesList[i].ID)
        //        {
        //            last_service_id = sim.ServicesList[i].ID;
        //        }
        //        sim.ServicesList[i].scenario = sim;
        //        if (sim.ServicesList[i] is StopService)
        //        {
        //            try
        //            {
        //                if (last_group_id < (sim.ServicesList[i] as StopService).PassengersGroup.ID)
        //                {
        //                    last_group_id = (sim.ServicesList[i] as StopService).PassengersGroup.ID;
        //                }
        //            }
        //            catch { }
        //        }
        //    }
        //    AgentIDEnumerator.Init(last_agent_id);
        //    GroupIDEnumerator.Init(last_group_id);
        //    ServiceIDEnumerator.Init(last_service_id);
        //    return sim;
        //}
    }

    

    
}
    