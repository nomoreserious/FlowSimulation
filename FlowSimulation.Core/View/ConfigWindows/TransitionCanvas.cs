using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using FlowSimulation.Helpers.Graph;
using System.Windows;

namespace FlowSimulation.View.ConfigWindows
{
    public class TransitionCanvas : Canvas
    {
        public Graph<ViewModel.TransitionNode,ViewModel.TransitionEdge> TransitionGraph
        {
            get { return (Graph<ViewModel.TransitionNode, ViewModel.TransitionEdge>)GetValue(TransitionGraphProperty); }
            set { SetValue(TransitionGraphProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TransitionGraph.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransitionGraphProperty =
            DependencyProperty.Register("TransitionGraphProperty", typeof(Graph<ViewModel.TransitionNode, ViewModel.TransitionEdge>), typeof(TransitionCanvas), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, TransitionGraphChanged));

        private static void TransitionGraphChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((TransitionCanvas)obj).InvalidateVisual();
        }


        public TransitionCanvas()
        {

        }

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            base.OnRender(dc);
            //int row = 0, column = 0;
            if (TransitionGraph != null)
            {

            }
        }
    }
}
