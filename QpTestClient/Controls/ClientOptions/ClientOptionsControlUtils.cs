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
            textBox.TextChanged += (_, _) => setValueHandler(textBox.Text);
        }

        public static void BindInt32(TextBox textBox, Func<int> getValueHandler, Action<int> setValueHandler)
        {
            textBox.Text = getValueHandler().ToString();
            textBox.TextChanged += (_, _) =>
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
            textBox.TextChanged += (_, _) =>
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

        public static void BindBoolean(CheckBox checkBox, Func<bool> getValueHandler, Action<bool> setValueHandler)
        {
            checkBox.Checked = getValueHandler();
            checkBox.CheckedChanged += (_, _) => setValueHandler(checkBox.Checked);
        }

        public static void BindEnum<TEnum>(ComboBox comboBox, Func<TEnum> getValueHandler, Action<TEnum> setValueHandler)
             where TEnum : struct, Enum
        {
            foreach (var item in Enum.GetValues<TEnum>())
                comboBox.Items.Add(item);
            comboBox.SelectedItem = getValueHandler();
            comboBox.SelectedIndexChanged += (_, _) => setValueHandler((TEnum)comboBox.SelectedItem);
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

        public static void LinkControl(Label label, Control control)
        {
            label.Click += (_, _) =>
            {
                if (control.CanFocus)
                {
                    ActiveLabel(label);
                    if (!control.Focused)
                    {
                        control.Focus();
                    }
                }
            };
            control.GotFocus += (_, _) =>
            {
                ActiveLabel(label);
            };
            control.LostFocus += (_, _) =>
            {
                DeactiveLabel(label);
            };
        }
    }
}
