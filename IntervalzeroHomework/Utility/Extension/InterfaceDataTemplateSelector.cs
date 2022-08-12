using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Utility.Extension
{
    public class InterfaceDataTemplateSelector : DataTemplateSelector
    {
        public static DataTemplateSelector Instance { get; } = new InterfaceDataTemplateSelector();
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            #region Require

            if (item == null) throw new ArgumentNullException();
            if (container == null) throw new ArgumentNullException();

            #endregion

            // Result
            DataTemplate itemDataTemplate = null;

            // Base DataTemplate
            itemDataTemplate = base.SelectTemplate(item, container);
            if (itemDataTemplate != null) return itemDataTemplate;

            // Interface DataTemplate
            FrameworkElement itemContainer = container as FrameworkElement;
            if (itemContainer == null) return null;

            foreach (Type itemInterface in item.GetType().GetInterfaces())
            {
                itemDataTemplate = itemContainer.TryFindResource(new DataTemplateKey(itemInterface)) as DataTemplate;
                if (itemDataTemplate != null) break;
            }

            // Return
            return itemDataTemplate;
        }
    }
}
