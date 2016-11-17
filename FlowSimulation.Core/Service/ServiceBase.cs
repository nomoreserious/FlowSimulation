using System;
using System.Collections.Generic;
using System.Text;
using FlowSimulation.Agents;
using FlowSimulation.SimulationScenario;
using System.Drawing;
using System.Xml.Serialization;
using FlowSimulation.Enviroment;

namespace FlowSimulation.Service
{
    public abstract class ServiceBase
    {
        public enum ServedState
        {
            NotInService = 0,
            MoveToQueue = 1,
            InQueue = 2,
            Served = 3
        }
        [XmlAttribute]
        public int ID { get; set; }
        [XmlIgnore]
        public Scenario scenario { get; set; }
        [XmlIgnore]
        protected List<int> agentsList;
        [XmlIgnore]
        protected Queue<int> agentsQueue;
        [XmlAttribute]
        public int MaxServedTime { get; set; }
        [XmlAttribute]
        public int MinServedTime { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        protected int add_count = 0;
        [XmlIgnore]    
        public int AddCount
        {
            get
            {
                int temp = add_count;
                add_count = 0;
                return temp;
            }
        }

        public ServiceBase()
        {
            ID = -1;
            agentsList = new List<int>();
            agentsQueue = new Queue<int>();
        }

        public abstract void DoStep();
        public abstract WayPoint GetAgentDirection(int agentID, System.Windows.Point position);
        public abstract ServedState GetServedState(int agentID);
        internal abstract void Initialize();

        public virtual void AddAgentToService(int agentID)
        {
            if (!agentsList.Contains(agentID))
            {
                agentsList.Add(agentID);
            }
        }

        protected virtual bool DeleteAgentFromService(int agentID)
        {
            return agentsList.Remove(agentID);
        }

        public virtual bool AddAgentToQueue(int agentID, System.Windows.Point location)
        {
            if (!agentsQueue.Contains(agentID))
            {
                agentsQueue.Enqueue(agentID);
                return true;
            }
            return false;
        }

        protected virtual int RemoveFirstAgentFromQueue()
        {
            return agentsQueue.Dequeue();
        }
    }
}
