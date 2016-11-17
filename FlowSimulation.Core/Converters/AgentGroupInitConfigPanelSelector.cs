using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace FlowSimulation.Converters
{
    class AgentGroupInitConfigPanelSelector : DataTemplateSelector
    {
        public DataTemplate Network { get; set; }
        public DataTemplate Count { get; set; }
        public DataTemplate Distribution { get; set; }
        public DataTemplate TimeTable { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item is Scenario.Model.AgentsGroup)
            {
                switch ((item as Scenario.Model.AgentsGroup).Type)
                {
                    case Scenario.Model.AgentGroupInitType.Count:
                        return Count;
                    case Scenario.Model.AgentGroupInitType.Distribution:
                        return Distribution;
                    case Scenario.Model.AgentGroupInitType.Network:
                        return Network;
                    case Scenario.Model.AgentGroupInitType.TimeTable:
                        return TimeTable;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
