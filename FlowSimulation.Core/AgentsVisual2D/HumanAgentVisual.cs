using System.Windows.Media;
using System.Windows;
using System;
using System.Windows.Media.Animation;
using FlowSimulation.Agents;

namespace FlowSimulation.AgentsVisual2D
{
    class HumanAgentVisual : AgentVisualBase
    {
        public HumanAgentVisual(AgentBase agentBase) : base(agentBase) { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double d = new Random(agentBase.ID).NextDouble() / 5;
            drawingContext.DrawEllipse(GetGroupColor(agentBase.Group), null, Location, 0.35 + d, 0.35 + d);
        }
    }
}
