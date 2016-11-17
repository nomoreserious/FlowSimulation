using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Agents;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace FlowSimulation.Contracts.Services
{
    public abstract class ServiceBase : IService
    {
        public ServiceBase()
        {
            Id = Generator.GetServiceId();
        }

        public Enviroment.WayPoint Dislocation { get; set; }
        public ulong Id { get; protected set; }
        public string Name { get; set; }

        public abstract void DoStep(double step_interval);
        public abstract void Initialize(Dictionary<string, object> settings);
        public virtual Shape GetServiceShape() { throw new NotImplementedException(); }
    }
}
