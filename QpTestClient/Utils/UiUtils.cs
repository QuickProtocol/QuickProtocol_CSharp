using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QpTestClient.Utils
{
    public class UiUtils
    {
        public static Control GetPropertyGridControl(object obj)
        {
            TypeDescriptor.AddAttributes(obj, new Attribute[] { new ReadOnlyAttribute(true) });

            var control = new PropertyGrid();
            control.SelectedObject = obj;
            control.ToolbarVisible = false;
            control.PropertySort = PropertySort.NoSort;
            return control;
        }
    }
}
