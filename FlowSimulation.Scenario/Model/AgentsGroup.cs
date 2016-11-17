using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using FlowSimulation.Enviroment;
using FlowSimulation.Helpers.Graph;
using FlowSimulation.Helpers.DesignPatterns;

namespace FlowSimulation.Scenario.Model
{
    [Serializable]
    public sealed class AgentsGroup : ISerializable
    {
        #region Private Members
        private AgentGroupInitType _type;
        private string _agentCode;

        private int _port;
        private string _host;
        private string _name;
        private int _count;
        private Dictionary<DayOfWeek, int[]> _agentsDistibution;
        private Dictionary<DayOfWeek, TimeSpan[]> _timeTable;

        private WayPoint _sourcePoint;
        private WayPoint _targetPoint; 
        #endregion

        #region Ctors
        public AgentsGroup(ulong id, string name)
        {
            Id = id;
            Name = name;

            _type = AgentGroupInitType.Count;
            _agentsDistibution = new Dictionary<DayOfWeek, int[]>();
            _timeTable = new Dictionary<DayOfWeek, TimeSpan[]>();
        }
        #endregion

        #region Public properties
        public ulong Id { get; private set; }

        public Dictionary<DayOfWeek, int[]> AgentsDistibution
        {
            get { return _agentsDistibution; }
            set { _agentsDistibution = value; }
        }

        public Dictionary<DayOfWeek, TimeSpan[]> TimeTable
        {
            get { return _timeTable; }
            set { _timeTable = value; }
        }

        public string AgentTypeCode
        {
            get { return _agentCode; }
            set { _agentCode = value; }
        }

        public AgentGroupInitType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public WayPoint SourcePoint
        {
            get { return _sourcePoint; }
            set { _sourcePoint = value; }
        }

        public WayPoint TargetPoint
        {
            get { return _targetPoint; }
            set { _targetPoint = value; }
        } 
        #endregion

        #region Serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("SourcePoint", SourcePoint);
            info.AddValue("TargetPoint", TargetPoint);
            info.AddValue("Name", Name);
            info.AddValue("AgentTypeCode", AgentTypeCode);
            info.AddValue("Type", Type);
            switch (Type)
            {
                case AgentGroupInitType.Network:
                    info.AddValue("Host", Host);
                    info.AddValue("Port", Port);
                    break;
                case AgentGroupInitType.Distribution:
                    info.AddValue("AgentDistibution", AgentsDistibution);
                    break;
                case AgentGroupInitType.Count:
                    info.AddValue("Count", Count);
                    break;
                case AgentGroupInitType.TimeTable:
                    info.AddValue("TimeTable", TimeTable);
                    break;
            }
        }

        public AgentsGroup(SerializationInfo info, StreamingContext context)
        {
            try
            {
                foreach (var pv in info)
                {
                    switch (pv.Name)
                    {
                        case "Id":
                            this.Id = (ulong)pv.Value;
                            break;
                        case "Name":
                            this.Name = (string)pv.Value;
                            break;
                        //Устарел, теперь SourcePoint
                        case "StartPoint":
                            this.SourcePoint = (WayPoint)pv.Value;
                            break;
                        case "SourcePoint":
                            this.SourcePoint = (WayPoint)pv.Value;
                            break;
                        case "TargetPoint":
                            this.TargetPoint = (WayPoint)pv.Value;
                            break;
                        case "AgentTypeCode":
                            this.AgentTypeCode = (string)pv.Value;
                            break;
                        case "Type":
                            this.Type = (AgentGroupInitType)pv.Value;
                            break;
                        case "Host":
                            this.Host = (string)pv.Value;
                            break;
                        case "Port":
                            this.Port = (int)pv.Value;
                            break;
                        case "Count":
                            this.Count = (int)pv.Value;
                            break;
                        case "AgentDistibution":
                            this.AgentsDistibution = (Dictionary<DayOfWeek, int[]>)pv.Value;
                            break;
                        case "TimeTable":
                            this.TimeTable = (Dictionary<DayOfWeek, TimeSpan[]>)pv.Value;
                            break;
                        default:
                            Console.WriteLine("Неизвестный атрибут: " + pv.Name);
                            break;
                    }
                }
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine(string.Format("Тип: {0} Ошибка:{1} Сообщение:{2}", this.GetType().Name, ex.GetType().Name, ex.Message));
            }
        }  
        #endregion
    }

    public enum AgentGroupInitType
    {
        [StringValue("По сети")]
        Network,
        [StringValue("По распределению")]
        Distribution,
        [StringValue("По расписанию")]
        TimeTable,
        [StringValue("По количеству")]
        Count
    }
}
