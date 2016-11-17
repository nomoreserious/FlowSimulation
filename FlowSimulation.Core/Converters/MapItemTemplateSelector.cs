using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using FlowSimulation.Enviroment;

namespace FlowSimulation.Converters
{
    public class MapItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PointTemplate { get; set; }
        public DataTemplate ServiceTemplate { get; set; }
        public DataTemplate EdgeTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item is WayPoint)
            {
                if (((WayPoint)item).IsServicePoint)
                {
                    return ServiceTemplate;
                }
                return PointTemplate;
            }
            return EdgeTemplate;
        }
    }
}
