using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System;
using System.Xml.Serialization;
using FlowSimulation.SimulationScenario;
using FlowSimulation.Enviroment;

namespace FlowSimulation.Agents
{
    public class AgentTypesNames
    {
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public Type Type { get; set; }
    }

    public abstract class AgentBase
    {   
        public static AgentTypesNames[] AgentsTypesList =  new AgentTypesNames[3] { 
                new AgentTypesNames() { TypeID = 0, Type = typeof(HumanAgent), TypeName = "Человек" }, 
                new AgentTypesNames() { TypeID = 1, Type = typeof(BusAgent), TypeName = "Автобус" }, 
                new AgentTypesNames() { TypeID = 2, Type = typeof(TrainAgent), TypeName = "Поезд" } };
        
        protected int _id;
        protected bool _break;
        protected int _group;
        protected int _maxspeed;

        protected SimulationScenario.Scenario scenario;
        
        protected Thread lifeThread;

        protected System.Drawing.Point cell;
        protected System.Drawing.Point prePosition;
        protected System.Windows.Point position;

        public List<WayPoint> WayPointsList { get; set; }
        public double SpeedRatio { get; set; }

        public int ID 
        {
            get { return _id; }
        }

        public int Group
        {
            get { return _group; }
        }

        public int MaxSpeed
        {
            get { return _maxspeed; }
        }

        public int CurrentSpeed { get; set; }

        public int RealSpeed { get; set; }

        public AgentBase(int id, Scenario scenario, int group, int speed)
        {
            this._id = id;
            this.scenario = scenario;
            this._group = group;
            this._maxspeed = speed;
            this.WayPointsList = new List<WayPoint>();
            SpeedRatio = 1.0;
            cell = new Point(-1, -1);
            position = new System.Windows.Point(-1, -1);
            lifeThread = new Thread(Life);
        }

        public System.Drawing.Point GetCell()
        {
            return cell;
        }

        public System.Windows.Point GetPosition()
        {
            return position;
        }

        public abstract void Life();

        public abstract void Step();

        public virtual void Remove()
        {
            WayPointsList.Clear();
            scenario.map.SetMapCellTake(false, cell.X, cell.Y);
        }

        public bool Sleep(int millisecondsTimeout)
        {
            try
            {
                this.lifeThread.Join(millisecondsTimeout);
                return true;
            }
            catch
            {}
            return false;
        }

        public bool Start()
        {
            try
            {
                this.lifeThread.Start();
                return true;
            }
            catch
            { }
            return false;
        }

        public bool Abort()
        {
            try
            {
                _break = true;
                if (lifeThread.ThreadState == ThreadState.WaitSleepJoin || lifeThread.ThreadState == ThreadState.Suspended)
                {
                    lifeThread.Resume();
                }
                return true;
            }
            catch
            { }
            return false;
        }

        public bool Pause()
        {
            try
            {
                this.lifeThread.Suspend();
                return true;
            }
            catch
            { }
            return false;
        }

        public bool Resume()
        {
            try
            {
                this.lifeThread.Resume();
                return true;
            }
            catch
            { }
            return false;
        }

        public ThreadState LifeStatus()
        {
            return lifeThread.ThreadState;
        }

        public void SetThreadStateBackground()
        {
            lifeThread.IsBackground = true;
        }
    }

    public class AgentsGroup
    {
        [XmlAttribute]
        public int ID { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public bool IsNetworkGroup { get; set; }
        [XmlAttribute]
        public bool IsServiceGroup { get; set; }
        [XmlAttribute]
        public int ServiceID { get; set; }
        [XmlAttribute]
        public string Address { get; set; }
        [XmlAttribute]
        public int Port { get; set; }
        [XmlArray]
        [XmlArrayItem(typeof(int))]
        public List<int> AgentDistribution { get; set; }
        [XmlArray]
        [XmlArrayItem(typeof(AgentTemplate))]
        public List<AgentTemplate> AgentTemplateList { get; set; }

        public AgentsGroup()
        {
            AgentDistribution = new List<int>(1440);
            AgentTemplateList = new List<AgentTemplate>();
        }
    }

    public class AgentTemplate
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public double Length { get; set; }
        [XmlAttribute]
        public double Width { get; set; }
        [XmlAttribute]
        public double Height { get; set; }
        [XmlAttribute]
        public int NumberOfCarriges { get; set; }
        [XmlAttribute]
        public int Capasity { get; set; }
        [XmlAttribute]
        public double InputFactor { get; set; }
        [XmlAttribute]
        public double OutputFactor { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public double MaxSpeed { get; set; }
        [XmlAttribute]
        public double MinSpeed { get; set; }
        [XmlAttribute]
        public int Persent { get; set; }
        [XmlArray]
        [XmlArrayItem(typeof(WayPoint))]
        public List<WayPoint> WayPointsList { get; set; }

        public AgentTemplate()
        { }
    }
}
