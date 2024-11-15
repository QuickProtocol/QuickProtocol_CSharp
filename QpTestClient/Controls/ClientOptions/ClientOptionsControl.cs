using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace QpTestClient.Controls.ClientOptions
{
    public abstract class ClientOptionsControl : UserControl
    {
        private bool _ReadOnly = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReadOnly
        {
            get { return _ReadOnly; }
            set
            {
                _ReadOnly = value;
                travelChild(this, control =>
                {
                    if(control is TextBoxBase textBoxBase)
                    {
                        textBoxBase.ReadOnly = value;
                    }
                    else if(control is ButtonBase buttonBase)
                    {
                        buttonBase.Enabled = !value;
                    }
                    else if (control is ListControl listControl)
                    {
                        listControl.Enabled = !value;
                    }
                });
            }
        }

        private void travelChild(Control control,Action<Control> action)
        {
            action(control);
            foreach (Control child in control.Controls)
            {
                travelChild(child, action);
            }
        }
    }
}
