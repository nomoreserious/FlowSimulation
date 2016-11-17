using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows;
using FlowSimulation.Agents;

namespace FlowSimulation.AgentsVisual2D
{
    class BusAgentVisual : AgentVisualBase
    {
        public BusAgentVisual(AgentBase agentBase) : base(agentBase) { }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            System.Windows.Media.Media3D.Size3D size = (agentBase as BusAgent).Size;
            drawingContext.PushTransform(new TranslateTransform(Location.X - size.X / 4 , Location.Y));
            drawingContext.PushTransform(new RotateTransform((agentBase as BusAgent).Angle));
            drawingContext.DrawRectangle(GetGroupColor(agentBase.Group), null, new Rect(-size.X / 4, -size.Y / 2, size.X, size.Y));
            drawingContext.DrawRectangle(Brushes.LightBlue, null, new Rect(size.X / 4 * 3 - 2, -size.Y / 2, 2, size.Y));
            drawingContext.Pop();
            drawingContext.Pop();
        }
    }
}
