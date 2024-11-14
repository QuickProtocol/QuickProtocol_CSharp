using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QpTestClient.Controls.ClientOptions
{
    public class ClientOptionsControlUtils
    {

        public static void BindString(TextBox textBox, Func<string> getValueHandler, Action<string> setValueHandler)
        {
            textBox.Text = getValueHandler();
            textBox.TextChanged += (sender2, e2) => setValueHandler(textBox.Text);
        }

        public static void BindInt32(TextBox textBox, Func<int> getValueHandler, Action<int> setValueHandler)
        {
            textBox.Text = getValueHandler().ToString();
            textBox.TextChanged += (sender2, e2) =>
            {
                if (int.TryParse(textBox.Text, out var v))
                    setValueHandler(v);
                else
                    textBox.Text = getValueHandler().ToString();
            };
        }

        public static void BindInt32(TextBox textBox, Func<int?> getValueHandler, Action<int?> setValueHandler)
        {
            textBox.Text = getValueHandler()?.ToString();
            textBox.TextChanged += (sender2, e2) =>
            {
                var text = textBox.Text;
                if (string.IsNullOrEmpty(text))
                    setValueHandler(null);
                else if (int.TryParse(text, out var v))
                    setValueHandler(v);
                else
                    textBox.Text = getValueHandler().ToString();
            };
        }

        private static void ActiveLabel(Label label)
        {
            label.ForeColor = SystemColors.HighlightText;
            label.BackColor = SystemColors.Highlight;
        }

        private static void DeactiveLabel(Label label)
        {
            label.ForeColor = SystemColors.ControlText;
            label.BackColor = SystemColors.ControlLightLight;
        }

        public static void LinkControl(Label label, TextBox control)
        {
            label.Click += (sender, e) =>
            {
                ActiveLabel(label);
                if (!control.Focused)
                {
                    control.SelectionStart = control.Text.Length;
                    control.Focus();
                }
            };
            control.GotFocus += (sender, e) =>
            {
                ActiveLabel(label);
            };
            control.LostFocus += (sender, e) =>
            {
                DeactiveLabel(label);
            };
        }
    }
}
