using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Contracts.Agents;
using System.Threading;

namespace FlowSimulation.Core
{
    public class SimulationTask
    {
        private ManualResetEvent _readyEvent;
        private double _step_time_ms;

        public SimulationTask(ManualResetEvent readyEvent, double step_time_ms)
        {
            if (readyEvent == null)
            {
                throw new ArgumentNullException("readyEvent is null in work thread");
            }
            _readyEvent = readyEvent;
            _step_time_ms = step_time_ms;
        }

        public void ThreadPoolCallback(object context)
        {
            if (context is IEnumerable<IAgent>)
            {
                var tasks = (IEnumerable<IAgent>)context;
                foreach (var task in tasks)
                {
                    try
                    {
                        task.DoStep(_step_time_ms);
                    }
                    catch (Exception ex)
                    {
                        task.RouteList.Clear();
                        Console.WriteLine(string.Format("Ошибка в потоке {0} :{1}", System.Threading.Thread.CurrentContext.ContextID, ex.Message));
                    }
                }
            }
            _readyEvent.Set();
        }
    }
}
