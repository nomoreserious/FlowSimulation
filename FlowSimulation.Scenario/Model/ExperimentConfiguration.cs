using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FlowSimulation.Scenario.Model
{
    [Serializable]
    public class ExperimentConfiguration : ISerializable
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan StopTime { get; set; }

        public bool AverangeQueueLenght { get; set; }
        public bool AgentAverangeLenght { get; set; }
        public bool AgentAverangeLenghtByGroup { get; set; }
        public bool AgentAverangeSpeed { get; set; }
        public bool AgentAverangeSpeedByGroup { get; set; }
        public bool AgentAverangeTime { get; set; }
        public bool AgentAverangeTimeByGroup { get; set; }
        public bool AgentCountOnMap { get; set; }
        public bool AgentCountOnMapByGroup { get; set; }
        public bool AgentInputOutput { get; set; }
        public bool AgentInputOutputByGroup { get; set; }
        public bool SpectralDensity { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            
        }

        public ExperimentConfiguration()
        {

        }

        public ExperimentConfiguration(SerializationInfo info, StreamingContext context)
        {

        }
    }
}
