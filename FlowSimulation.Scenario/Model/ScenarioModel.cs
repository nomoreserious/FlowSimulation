using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Media;
using FlowSimulation.Helpers.Graph;
using System.Windows.Markup;
using System.Text;
using System.IO;
using FlowSimulation.Enviroment;

namespace FlowSimulation.Scenario.Model
{
    [Serializable]
    public class ScenarioModel : ISerializable
    {
        private ExperimentConfiguration _scenarioConfig;
        private Graph<WayPoint, double> _transitionGraph;
        private Graph<WayPoint, string> _roadGraph;
        private Dictionary<string, Dictionary<string, object>> _modulesSettings;

        public ScenarioModel()
        {
            _transitionGraph = new Graph<WayPoint, double>();
            _roadGraph = new Graph<WayPoint, string>();
            _scenarioConfig = new ExperimentConfiguration();
            _modulesSettings = new Dictionary<string, Dictionary<string, object>>();

            Map = new Enviroment.Map();
            AgentGroups = new List<AgentsGroup>();
            Services = new List<ServiceModel>();
            
            StartTime = DateTime.Now;
            EndTime = DateTime.Now.AddHours(5);
        }

        public ScenarioModel(SerializationInfo info, StreamingContext context)
            : this()
        {
            try
            {
                foreach (var pv in info)
                {
                    switch (pv.Name)
                    {
                        //case "StartTime":
                        //    this.StartTime = (DateTime)pv.Value;
                        //    break;
                        //case "EndTime":
                        //    this.EndTime = (DateTime)pv.Value;
                        //    break;
                        case "Map":
                            this.Map = (Map)pv.Value;
                            break;
                        case "InputOutputPoints":
                            this.InputOutputPoints = (List<WayPoint>)pv.Value;
                            break;
                        case "AgentGroups":
                            this.AgentGroups = (List<AgentsGroup>)pv.Value;
                            break;
                        case "Services":
                            this.Services = (List<ServiceModel>)pv.Value;
                            break;
                        case "TransitionGraph":
                            this.TransitionGraph = (Graph<WayPoint, double>)pv.Value;
                            break;
                        case "RoadGraph":
                            this.RoadGraph = (Graph<WayPoint, string>)pv.Value;
                            break;
                        case "ModulesSettings":
                            this.ModulesSettings = (Dictionary<string, Dictionary<string, object>>)pv.Value;
                            break;

                        default:
                            Console.WriteLine("Неизвестный атрибут в ScenarioModel: " + pv.Name);
                            break;
                    }
                }
            }
            catch (SerializationException ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Тип: {0} Ошибка:{1} Сообщение:{2}", this.GetType().Name, ex.GetType().Name, ex.Message));
            }
        }

        public Enviroment.Map Map { get; set; }
        public List<WayPoint> InputOutputPoints { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<AgentsGroup> AgentGroups { get; set; }
        public List<ServiceModel> Services { get; set; }
        
        public Dictionary<string, Dictionary<string, object>> ModulesSettings
        {
            get { return _modulesSettings; }
            set { _modulesSettings = value; }
        }

        public Graph<WayPoint, double> TransitionGraph
        {
            get { return _transitionGraph; }
            set { _transitionGraph = value; }
        }

        public Graph<WayPoint, string> RoadGraph
        {
            get { return _roadGraph; }
            set { _roadGraph = value; }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ModulesSettings", ModulesSettings);
            info.AddValue("StartTime", StartTime);
            info.AddValue("EndTime", EndTime);
            info.AddValue("Map", Map);
            info.AddValue("InputOutputPoints", InputOutputPoints);
            info.AddValue("AgentGroups", AgentGroups);
            info.AddValue("Services", Services);
            info.AddValue("TransitionGraph", TransitionGraph);
            info.AddValue("RoadGraph", RoadGraph);
        }
    }

    [Serializable]
    public class MapInfoVM : ISerializable
    {
        public string Key { get; set; }
        public byte Value { get; set; }

        public MapInfoVM(string key, byte value)
        {
            Key = key;
            Value = value;
        }

        public MapInfoVM(SerializationInfo info, StreamingContext context)
        {
            try
            {
                Key = info.GetString("Key");
                Value = info.GetByte("Value");
            }
            catch (SerializationException ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Тип: {0} Ошибка:{1} Сообщение:{2}", this.GetType().Name, ex.GetType().Name, ex.Message));
                Key = string.Empty;
                Value = 0x00;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Key", Key);
            info.AddValue("Value", Value);
        }
    }
}
