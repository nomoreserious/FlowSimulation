using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace FlowSimulation.ViewModel.Settings
{
    public class ParamValueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RegularTemplate { get; set; }
        public DataTemplate DoubleTemplate { get; set; }
        public DataTemplate IntegerTemplate { get; set; }
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate BooleanTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item is ModuleParam)
            {
                if (((ModuleParam)item).Value is int)
                {
                    return IntegerTemplate;
                }
                if (((ModuleParam)item).Value is double)
                {
                    return DoubleTemplate;
                }
                if (((ModuleParam)item).Value is string)
                {
                    return StringTemplate;
                }
                if (((ModuleParam)item).Value is bool)
                {
                    return BooleanTemplate;
                }
            }
            return RegularTemplate;
        }
    }
}
