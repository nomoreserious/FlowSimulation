using System.Collections.Generic;
using System.Windows.Media;
using FlowSimulation.SimulationScenario;
using FlowSimulation.Helpers.Graph;
using FlowSimulation.Map.Model;
using FlowSimulation.Enviroment;
using FlowSimulation.Scenario.Model;

namespace FlowSimulation.Agents
{
    abstract class VehicleAgentBase : AgentBase 
    {
        protected List<WayPoint> CheckPointsList;
        protected bool go;
        public Graph<WayPoint, PathFigure> RoadGraph { get; set; }
        public System.Windows.Media.Media3D.Size3D Size { get; set; }
        public int MaxCapasity { get; set; }
        public int CurrentAgentCount { get; set; }
        public double InputFactor { get; set; }
        public double OutputFactor { get; set; }
        public bool ReadyToIOOperation { get; set; }

        public VehicleAgentBase(int id, ScenarioModel scenario, int group, int speed)
            : base(id, scenario, group, speed)
        { }
        public abstract override void Life();

        public abstract override void Step();

        internal virtual void Go()
        {
            go = true;
        }
    }
}
