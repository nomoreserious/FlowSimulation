using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using FlowSimulation.Agents;
using FlowSimulation.Map.Model;

namespace FlowSimulation.Service
{
    public class QueueService : ServiceBase
    {
        [XmlIgnore]
        private List<List<int>> AgentsQueuesList;
        
        [XmlIgnore]
        private Dictionary<int, System.Windows.Point> AgentsLocationsDictionary;
        
        [XmlIgnore]
        private List<int> Times;

        [XmlArray]
        [XmlArrayItem(typeof(WayPoint))]
        public List<WayPoint> InputPoints { get; set; }

        /// <summary>
        /// Not parametred constructor
        /// </summary>
        public QueueService()
        {
            InputPoints = new List<WayPoint>();
        }

        public QueueService(List<WayPoint> servicePoint)
        {
            InputPoints = servicePoint;
        }

        public double AverangeQueueLenght { get { return (from queue in AgentsQueuesList select queue.Count).Average(); } }

        internal override void Initialize()
        {
            AgentsLocationsDictionary = new Dictionary<int, System.Windows.Point>();
            AgentsQueuesList = new List<List<int>>(InputPoints.Count);
            for (int i = 0; i < InputPoints.Count; i++)
            {
                AgentsQueuesList.Add(new List<int>());
            }
            Times = new List<int>(InputPoints.Count);
            for (int i = 0; i < InputPoints.Count; i++)
            {
                Times.Add(0);
            }
        }

        public override void DoStep()
        {

        }

        public override WayPoint GetAgentDirection(int agentID, System.Windows.Point position)
        {
            WayPoint wp = new WayPoint();
            Random rand = new Random(agentID);
            int index = AgentsQueuesList.FindIndex(delegate(List<int> queue) { return queue.Contains(agentID); });
            if (index == -1)
            {
                index = 0;
                int count = AgentsQueuesList[index].Count;
                for (int i = 1; i < AgentsQueuesList.Count; i++)
                {
                    if (AgentsQueuesList[i].Count < count)
                    {
                        index = i;
                        count = AgentsQueuesList[i].Count;
                    }
                }
                if (AgentsQueuesList[index].Count == 0)
                {
                    wp.LocationPoint = InputPoints[index].LocationPoint;   
                }
                else
                {
                    System.Windows.Point last_in_queue = AgentsLocationsDictionary[AgentsQueuesList[index][AgentsQueuesList[index].Count-1]];
                    System.Windows.Vector vect = last_in_queue - position;
                    if (Math.Abs(vect.X) + Math.Abs(vect.Y) == 1)
                    {
                        wp.LocationPoint = position;
                    }
                    else if (Math.Abs(vect.X) + Math.Abs(vect.Y) == 2)
                    {
                        if (vect.X == 0 || vect.Y == 0)
                        {
                            if (rand.Next(0, 100) < 50)
                            {
                                wp.LocationPoint = position;
                            }
                            else
                            {
                                wp.LocationPoint = last_in_queue;
                            }
                        }
                        else
                        {
                            wp.LocationPoint = position;
                        }
                    }
                    else
                    {
                        wp.LocationPoint = last_in_queue;
                    }
                }
                AgentsQueuesList[index].Add(agentID);
                AgentsLocationsDictionary.Add(agentID, position);
            }
            else
            {
                AgentsLocationsDictionary[agentID] = position;
                int id = AgentsQueuesList[index].FindIndex(delegate(int item) { return item == agentID; });
                if (id == 0)
                {
                    wp.LocationPoint = InputPoints[index].LocationPoint;
                }
                else
                {
                    System.Windows.Point previos = AgentsLocationsDictionary[AgentsQueuesList[index][id - 1]];
                    System.Windows.Vector vect = previos - position;
                    if (Math.Abs(vect.X) + Math.Abs(vect.Y) == 1)
                    {
                        wp.LocationPoint = position;
                    }
                    else if (Math.Abs(vect.X) + Math.Abs(vect.Y) == 2)
                    {
                        if (vect.X == 0 || vect.Y == 0)
                        {
                            if (rand.Next(0, 100) < 50)
                            {
                                wp.LocationPoint = position;
                            }
                            else
                            {
                                wp.LocationPoint = previos;
                            }
                        }
                        else
                        {
                            wp.LocationPoint = position;
                        }
                    }
                    else
                    {
                        wp.LocationPoint = previos;
                    }
                }
            }
            wp.PointHeight = 1;
            wp.PointWidth = 1;
            wp.IsServicePoint = true;
            wp.ServiceID = ID;
            wp.IsWaitPoint = true;
            wp.MinWait = rand.Next(MinServedTime, MaxServedTime);
            return wp;
        }

        public override ServiceBase.ServedState GetServedState(int agentID)
        {
            if (!agentsList.Contains(agentID))
            {
                return ServedState.NotInService;
            }
            else
            {
                int index = AgentsQueuesList.FindIndex(delegate(List<int> queue) { return queue.Count != 0 && queue.Contains(agentID); });
                if (index == -1)
                {
                    return ServedState.MoveToQueue;
                }
                if(AgentsLocationsDictionary[AgentsQueuesList[index][0]] != InputPoints[index].LocationPoint)
                {
                    return ServedState.InQueue;
                }
                agentsList.Remove(agentID);
                AgentsQueuesList[index].Remove(agentID);
                AgentsLocationsDictionary.Remove(agentID);
                return ServedState.Served;
            }
        }

        public override void AddAgentToService(int agentID)
        {
            agentsList.Add(agentID);
        }

        public override bool AddAgentToQueue(int agentID, System.Windows.Point location)
        {
            int index = new Random(agentID).Next(0, InputPoints.Count);
            AgentsQueuesList[index].Add(agentID);
            AgentsLocationsDictionary.Add(agentID, location);
            return true;
        }
    }
}
