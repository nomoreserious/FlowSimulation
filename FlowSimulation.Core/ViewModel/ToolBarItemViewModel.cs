using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace FlowSimulation.ViewModel
{
    public class ToolBarItemViewModel
    {
        public ToolBarItemViewModel(string code, string name, string iconUri, ICommand selectionChagedCommand)
        {
            Code = code;
            Name = name;
            if (string.IsNullOrEmpty(iconUri))
            {
                Icon = new BitmapImage();
            }
            else
            {
                try
                {
                    Icon = new BitmapImage(new Uri(iconUri, UriKind.RelativeOrAbsolute));
                }
                catch
                {
                    Icon = new BitmapImage();
                }
            }
            CheckedCommand = selectionChagedCommand;
        }

        public string Name { get; private set; }
        public BitmapSource Icon { get; private set; }
        public string Code { get; private set; }

        public ICommand CheckedCommand { get; private set; }
    }
}
