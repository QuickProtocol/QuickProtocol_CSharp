using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QpTestClient.Controls
{
    public partial class AotPropertyGrid : UserControl
    {
        private List<Control> pnlPropertyControls = new List<Control>();
        private List<Label> propertyLabelList = new List<Label>();

        private bool _ReadOnly = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReadOnly
        {
            get { return _ReadOnly; }
            set
            {
                _ReadOnly = value;
                travelChild(pnlProperty, control =>
                {
                    if (control is TextBoxBase textBoxBase)
                    {
                        textBoxBase.ReadOnly = value;
                    }
                    else if (control is ButtonBase buttonBase)
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

        private void travelChild(Control control, Action<Control> action)
        {
            action(control);
            foreach (Control child in control.Controls)
            {
                travelChild(child, action);
            }
        }

        public AotPropertyGrid()
        {
            InitializeComponent();
        }

        public void RegisterGroup(string groupName)
        {
            var control = new Label();
            control.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            control.Dock = DockStyle.Top;
            control.Margin = new Padding(0);
            control.BorderStyle = BorderStyle.FixedSingle;
            control.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            control.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            control.Location = new System.Drawing.Point(0, 0);
            control.Name = "label1";
            control.Size = new System.Drawing.Size(878, 40);
            control.TabIndex = 1;
            control.Text = $"∇ {groupName}";
            control.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            var collapse = false;
            control.Click += (_, _) =>
            {
                collapse = !collapse;
                if (collapse)
                    control.Text = $"▷ {groupName}";
                else
                    control.Text = $"∇ {groupName}";

                var enterGroup = false;
                foreach (Control child in pnlPropertyControls)
                {
                    if (enterGroup)
                    {
                        if (child is Label)
                            break;
                        child.Visible = !collapse;
                    }
                    else
                    {
                        if (child == control)
                            enterGroup = true;
                    }
                }
            };
            pnlPropertyControls.Add(control);
        }

        private Label createPropertyLabel(string propertyName, string propertyDescription)
        {
            var control = new Label();
            control.BackColor = SystemColors.ControlLightLight;
            control.BorderStyle = BorderStyle.None;
            control.Dock = DockStyle.Left;
            control.Margin = new Padding(0);
            control.Size = new Size(280, 38);
            control.Text = propertyName;
            control.Click += (_, _) =>
            {
                lblPropertyName.Text = propertyName;
                lblPropertyDescription.Text = propertyDescription;
            };
            return control;
        }

        private TextBox createPropertyTextBox(string propertyName, string propertyDescription)
        {
            var control = new TextBox();
            control.BorderStyle = BorderStyle.None;
            control.Dock = DockStyle.Top;
            control.Margin = new Padding(0);
            control.Size = new Size(698, 38);
            control.GotFocus += (_, _) =>
            {
                lblPropertyName.Text = propertyName;
                lblPropertyDescription.Text = propertyDescription;
            };
            return control;
        }

        private CheckBox createPropertyCheckBox(string propertyName, string propertyDescription)
        {
            var control = new CheckBox();
            control.BackColor = SystemColors.ControlLightLight;
            control.Dock = DockStyle.Top;
            control.Location = new Point(0, 0);
            control.Padding = new Padding(10, 0, 0, 0);
            control.Size = new Size(696, 36);
            control.UseVisualStyleBackColor = false;
            return control;
        }


        private ComboBox createPropertyComboBox(string propertyName, string propertyDescription)
        {
            var control = new ComboBox();
            control.Dock = DockStyle.Top;
            control.DropDownStyle = ComboBoxStyle.DropDownList;
            control.FlatStyle = FlatStyle.Flat;
            control.FormattingEnabled = true;
            control.Location = new Point(0, 0);
            control.Size = new Size(696, 39);
            return control;
        }

        private void addPropertyControl(Label label1, Control control1)
        {
            propertyLabelList.Add(label1);
            LinkControl(label1, control1);

            var panel1 = new Panel();
            panel1.AutoSize = true;
            panel1.Margin = new Padding(0);
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(control1);
            panel1.Controls.Add(new Splitter() { BackColor = SystemColors.ControlDark, Width = 2 });
            panel1.Controls.Add(label1);
            panel1.Padding = new Padding(0);
            panel1.Dock = DockStyle.Top;
            panel1.Size = new Size(878, 38);

            pnlPropertyControls.Add(panel1);
        }


        private void ActiveLabel(Label label)
        {
            foreach (var item in propertyLabelList)
            {
                if (item == label)
                {
                    item.ForeColor = SystemColors.HighlightText;
                    item.BackColor = SystemColors.Highlight;
                }
                else
                {
                    item.ForeColor = SystemColors.ControlText;
                    item.BackColor = SystemColors.ControlLightLight;
                }
            }
        }


        private void LinkControl(Label label, Control control)
        {
            label.Click += (_, _) =>
            {
                ActiveLabel(label);
                if (control.CanFocus)
                {
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
        }

        public void RegisterProperty(string propertyName, string propertyDescription, Func<string> getValueHandler, Action<string> setValueHandler)
        {
            var textBox = createPropertyTextBox(propertyName, propertyDescription);
            textBox.Text = getValueHandler();
            textBox.TextChanged += (_, _) => setValueHandler(textBox.Text);
            addPropertyControl(createPropertyLabel(propertyName, propertyDescription), textBox);
        }

        public void RegisterProperty(string propertyName, string propertyDescription, Func<int> getValueHandler, Action<int> setValueHandler)
        {
            var textBox = createPropertyTextBox(propertyName, propertyDescription);
            textBox.Text = getValueHandler().ToString();
            textBox.TextChanged += (_, _) =>
            {
                if (int.TryParse(textBox.Text, out var v))
                    setValueHandler(v);
                else
                    textBox.Text = getValueHandler().ToString();
            };
            addPropertyControl(createPropertyLabel(propertyName, propertyDescription), textBox);
        }

        public void RegisterProperty(string propertyName, string propertyDescription, Func<int?> getValueHandler, Action<int?> setValueHandler)
        {
            var textBox = createPropertyTextBox(propertyName, propertyDescription);
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
            addPropertyControl(createPropertyLabel(propertyName, propertyDescription), textBox);
        }

        public void RegisterProperty(string propertyName, string propertyDescription, Func<bool> getValueHandler, Action<bool> setValueHandler)
        {
            var checkBox = createPropertyCheckBox(propertyName, propertyDescription);
            checkBox.Checked = getValueHandler();
            checkBox.CheckedChanged += (_, _) => setValueHandler(checkBox.Checked);
            addPropertyControl(createPropertyLabel(propertyName, propertyDescription), checkBox);
        }

        /// <summary>
        /// 生成控件
        /// </summary>
        public void GenerateControls()
        {
            for (var i = pnlPropertyControls.Count - 1; i >= 0; i--)
            {
                pnlProperty.Controls.Add(pnlPropertyControls[i]);
            }
        }

        private void AotPropertyGrid_Load(object sender, EventArgs e)
        {
            
        }

        public void RegisterProperty<TEnum>(string propertyName, string propertyDescription, Func<TEnum> getValueHandler, Action<TEnum> setValueHandler)
            where TEnum : struct, Enum
        {
            var comboBox = createPropertyComboBox(propertyName, propertyDescription);
            foreach (var item in Enum.GetValues<TEnum>())
                comboBox.Items.Add(item);
            comboBox.SelectedItem = getValueHandler();
            comboBox.SelectedIndexChanged += (_, _) => setValueHandler((TEnum)comboBox.SelectedItem);
            addPropertyControl(createPropertyLabel(propertyName, propertyDescription), comboBox);
        }
    }
}
