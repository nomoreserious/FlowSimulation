using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using FlowSimulation.Contracts.Agents;
using FlowSimulation.Contracts.Configuration;
using FlowSimulation.Enviroment;
using System.Windows.Shapes;

namespace FlowSimulation.Contracts.Services
{
    public interface IService : IConfigTarget
    {
        ulong Id { get; }
        string Name { get; }
        WayPoint Dislocation { get; }

        Shape GetServiceShape();
        void DoStep(double step_interval);

    }
}
