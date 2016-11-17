using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowSimulation.Helpers.MVVM
{
    public class MVVMHelper
    {
        public static System.Windows.Window GetActiveWindow()
        {
            foreach (System.Windows.Window win in System.Windows.Application.Current.Windows)
            {
                if (win.IsActive)
                {
                    return win;
                }
            }
            return null;
        }
    }
}
