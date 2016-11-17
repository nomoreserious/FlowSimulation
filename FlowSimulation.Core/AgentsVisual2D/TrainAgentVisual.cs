using System.Windows;
using System.Windows.Media;
using FlowSimulation.Agents;

namespace FlowSimulation.AgentsVisual2D
{
    class TrainAgentVisual : AgentVisualBase
    {
        public TrainAgentVisual(AgentBase agentBase) : base(agentBase) { }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            System.Windows.Media.Media3D.Size3D size = (agentBase as TrainAgent).Size;
            for (int i = 0; i < (agentBase as TrainAgent).Positions.Length; i++)
            {
                if ((agentBase as TrainAgent).NeedDraw[i] == true)
                {
                    Point position = (agentBase as TrainAgent).Positions[i];
                    drawingContext.PushTransform(new TranslateTransform(position.X, position.Y));
                    drawingContext.PushTransform(new RotateTransform((agentBase as TrainAgent).Angles[i]));
                    drawingContext.DrawRectangle(GetGroupColor(agentBase.Group), null, new Rect(-size.X / 2, -size.Y / 2, size.X, size.Y));
                    drawingContext.Pop();
                    drawingContext.Pop();
                }
            }
        }
    }
}
