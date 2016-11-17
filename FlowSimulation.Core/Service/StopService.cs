using System;
using System.Collections.Generic;
using System.Drawing;
using FlowSimulation.Agents;
using FlowSimulation.SimulationScenario;
using System.Xml.Serialization;
using FlowSimulation.Map.Model;

namespace FlowSimulation.Service
{
    public class StopService : ServiceBase
    {
        [XmlArray]
        [XmlArrayItem(typeof(WayPoint))]
        public List<WayPoint> InputPoints { get; set; }

        [XmlArray]
        [XmlArrayItem(typeof(WayPoint))]
        public List<WayPoint> OutputPoints { get; set; }

        [XmlElement]
        public AgentsGroup PassengersGroup { get; set; }

        [XmlAttribute]
        public int InputTimeMs { get; set; }

        [XmlAttribute]
        public int OutputTimeMs { get; set; }

        [XmlIgnore]
        private VehicleAgentBase IOAgent;

        [XmlIgnore]
        private bool IsIOReady;

        [XmlIgnore]
        private TimeSpan startTime;

        [XmlIgnore]
        private int max_input_count;

        [XmlIgnore]
        private int output_count;

        [XmlIgnore]
        public int input_time_helper;

        [XmlIgnore]
        public int output_time_helper;

        public StopService()
        {}

        public void OpenInputPoints(int agentID)
        {
            if (IOAgent != null)
            {
                CloseInputPoints();
                IOAgent.Go();
            }
            this.IOAgent = (VehicleAgentBase)scenario.agentsList.Find(delegate(AgentBase ab) { return ab.ID == agentID; });
            IOAgent.CurrentAgentCount = PassengersGroup.AgentDistribution[Convert.ToInt32(scenario.currentTime.TotalMinutes)] * IOAgent.MaxCapasity / 100;
            IOAgent.InputFactor = 1.0;
            IOAgent.OutputFactor = 1.0;
            startTime = scenario.currentTime;
            if (IOAgent != null)
            {
                output_count = Convert.ToInt32(IOAgent.OutputFactor * IOAgent.CurrentAgentCount);
             
                //max_input_count = agentsList.Count;
                max_input_count = IOAgent.MaxCapasity - IOAgent.CurrentAgentCount + output_count;
            }
            for (int i = 0; i < InputPoints.Count; i++)
            {
                scenario.map.SetMapFrameFlag(MapOld.CellState.Input, false, InputPoints[i].X, InputPoints[i].Y, InputPoints[i].PointWidth, InputPoints[i].PointHeight);
            }
            IsIOReady = true;
            output_time_helper = 0;
            input_time_helper = 0;
        }

        public void CloseInputPoints()
        {
            for (int i = 0; i < InputPoints.Count; i++)
            {
                scenario.map.SetMapFrameFlag(MapOld.CellState.Input, true, InputPoints[i].X, InputPoints[i].Y, InputPoints[i].PointWidth, InputPoints[i].PointHeight);
            }
            IsIOReady = false;
        }

        public override WayPoint GetAgentDirection(int agentID, System.Windows.Point position)
        {
            WayPoint point = InputPoints[new Random(agentID + 13).Next(0, InputPoints.Count)];
            point.IsServicePoint = true;
            point.ServiceID = ID;
            return point;
        }

        public override ServiceBase.ServedState GetServedState(int agentID)
        {
            if (!base.agentsList.Contains(agentID))
            {
                return ServedState.NotInService;
            }
            else if (!base.agentsQueue.Contains(agentID))
            {
                return ServedState.MoveToQueue;
            }
            else
            {
                return ServedState.InQueue;
            }
        }

        public override void DoStep()
        {
            //проверяем прибыл ли транспорт
            if (!IsIOReady)
            {
                return;
            }
            output_time_helper += Scenario.STEP_TIME_MS;
            input_time_helper += Scenario.STEP_TIME_MS;
            Random rand = new Random();
            //Добавляем Агентов
            while (output_time_helper > OutputTimeMs / OutputPoints.Count)
            {
                if (output_count > 0)
                {
                    AgentBase agent = Scenario.GetRandomAgentTemplateFromGroup(PassengersGroup.AgentTemplateList, PassengersGroup.ID, scenario);
                    agent.WayPointsList.Insert(0, OutputPoints[rand.Next(0, OutputPoints.Count)]);
                    scenario.agentsList.Add(agent);
                    add_count++;
                    output_count--;
                    IOAgent.CurrentAgentCount--;
                }
                output_time_helper -= OutputTimeMs / OutputPoints.Count;
            }
            //Если для входа и выхода одна и та же дверь, то сначала выходят все
            if (output_count > 0)
            {
                return;
            }

            //Поглощаем Агентов
            while (input_time_helper > InputTimeMs / InputPoints.Count)
            {
                //Если есть место, подходящий маршрут, агенты в очереди и не истекло время.
                if (IOAgent.CurrentAgentCount < IOAgent.MaxCapasity && max_input_count > 0 && agentsQueue.Count > 0)
                {
                    int id = base.RemoveFirstAgentFromQueue();
                    base.DeleteAgentFromService(id);
                    foreach (var agent in scenario.agentsList.FindAll(delegate(AgentBase ab) { return ab.ID == id; }))
                    {
                        agent.Remove();
                    }
                    IOAgent.CurrentAgentCount++;
                    max_input_count--;
                }
                input_time_helper -= InputTimeMs / InputPoints.Count;
            }
            if ((scenario.currentTime - startTime).TotalMilliseconds > MinServedTime && 
                ((scenario.currentTime - startTime).TotalMilliseconds > MaxServedTime ||
                ((IOAgent.CurrentAgentCount < IOAgent.MaxCapasity || max_input_count > 0) && output_count == 0)))
            {
                CloseInputPoints();
                IOAgent.Go();
                IOAgent = null;
            }
        }

        internal override void Initialize()
        {
            CloseInputPoints();
        }
    }
}
