using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace FlowSimulation.Helpers.MVVM
{
    public class CloseViewBehavior
    {
        // Регистрируем вложенное свойство
        public static readonly DependencyProperty CloseViewProperty =
            DependencyProperty.RegisterAttached("CloseView", typeof(bool), typeof(CloseViewBehavior),
            new PropertyMetadata(false, OnCloseViewChanged));

        public static bool GetCloseView(Window obj)
        {
            return (bool)obj.GetValue(CloseViewProperty);
        }
        public static void SetCloseView(Window obj, bool value)
        {
            obj.SetValue(CloseViewProperty, value);
        }

        private static void OnCloseViewChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs args)
        {
            Window win = dpo as Window;
            bool close = (bool)args.NewValue;

            if (win != null)
            {
                // Если свойство устанавливается в истину - закрыть окно
                if (close)
                {
                    win.Close();
                }
            }
        }
    }
}
