using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Drawing;
using System.Windows.Shapes;
using FlowSimulation.Contracts.Configuration;
using FlowSimulation.Enviroment;

namespace FlowSimulation.Contracts.Agents
{
    public interface IAgent : IConfigTarget
    {
        Point Position { get; set; }
        Size3D Size { get; set; }
        List<WayPoint> RouteList { get; set; }

        void DoStep(double msInterval);
        void DoStepAsync(double msInterval);
        Shape GetAgentShape();
        void AddAgentGeometry(double zCoordiante, ref MeshGeometry3D baseGeometry);
    }
}
