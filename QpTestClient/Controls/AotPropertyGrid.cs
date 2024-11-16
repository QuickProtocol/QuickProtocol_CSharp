using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QpTestClient.Controls
{
    public partial class AotPropertyGrid : UserControl
    {
        private List<Label> propertyLabelList = new List<Label>();

        public AotPropertyGrid()
        {
            InitializeComponent();
        }

        public void RegisterGroup(string groupName)
        {
            var groupLabel = new Label();
            groupLabel.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            groupLabel.Dock = DockStyle.Top;
            groupLabel.Margin = new Padding(0);
            groupLabel.BorderStyle = BorderStyle.FixedSingle;
            groupLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            groupLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            groupLabel.Location = new System.Drawing.Point(0, 0);
            groupLabel.Name = "label1";
            groupLabel.Size = new System.Drawing.Size(878, 40);
            groupLabel.TabIndex = 1;
            groupLabel.Text = $"∇ {groupName}";
            groupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            var collapse = false;
            groupLabel.Click += (_, _) =>
            {
                collapse = !collapse;
                if (collapse)
                    groupLabel.Text = $"▷ {groupName}";
                else
                    groupLabel.Text = $"∇ {groupName}";

                var enterGroup = false;
                foreach (Control control in flp.Controls)
                {
                    if (enterGroup)
                    {
                        if (control is Label)
                            break;
                        control.Visible = !collapse;
                    }
                    else
                    {
                        if (control == groupLabel)
                            enterGroup = true;
                    }
                }
            };


            flp.Controls.Add(groupLabel);
        }

        private Label createPropertyLabel(string propertyName, string propertyDescription)
        {
            var label = new Label();
            label.BackColor = System.Drawing.SystemColors.ControlLightLight;
            label.BorderStyle = BorderStyle.FixedSingle;
            label.Dock = DockStyle.Left;
            label.Location = new System.Drawing.Point(0, 0);
            label.Margin = new Padding(0);
            label.MinimumSize = new System.Drawing.Size(180, 30);
            label.Size = new System.Drawing.Size(180, 38);
            label.TabIndex = 0;
            label.Text = propertyName;
            label.Click += (_, _) =>
            {
                lblPropertyName.Text = propertyName;
                lblPropertyDescription.Text = propertyDescription;
            };
            return label;
        }

        private TextBox createPropertyTextBox(string propertyName, string propertyDescription)
        {
            var textBox = new TextBox();
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Dock = DockStyle.Fill;
            textBox.Location = new Point(180, 0);
            textBox.Margin = new Padding(0);
            textBox.Size = new Size(698, 38);
            textBox.GotFocus += (_, _) =>
            {
                lblPropertyName.Text = propertyName;
                lblPropertyDescription.Text = propertyDescription;
            };
            return textBox;
        }

        private CheckBox createPropertyCheckBox(string propertyName, string propertyDescription)
        {
            var checkBox=new CheckBox();
            checkBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            checkBox.Dock = System.Windows.Forms.DockStyle.Fill;
            checkBox.Location = new System.Drawing.Point(0, 0);
            checkBox.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            checkBox.Size = new System.Drawing.Size(696, 36);
            checkBox.UseVisualStyleBackColor = false;
            return checkBox;
        }


        private ComboBox createPropertyComboBox(string propertyName, string propertyDescription)
        {
            var comboBox=new ComboBox();
            comboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            comboBox.FormattingEnabled = true;
            comboBox.Location = new System.Drawing.Point(0, 0);
            comboBox.Size = new System.Drawing.Size(696, 39);
            return comboBox;
        }

        private void addPropertyControl(Label label1, Control control1)
        {
            propertyLabelList.Add(label1);
            LinkControl(label1, control1);

            var panel1 = new Panel();
            panel1.Margin = new Padding(0);
            panel1.Controls.Add(control1);
            panel1.Controls.Add(label1);
            panel1.Padding = new Padding(0);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 40);
            panel1.Size = new System.Drawing.Size(878, 38);

            flp.Controls.Add(panel1);
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
