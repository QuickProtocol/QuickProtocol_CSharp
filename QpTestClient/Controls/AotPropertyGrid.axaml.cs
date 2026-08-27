using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace QpTestClient.Controls
{
    public partial class AotPropertyGrid : UserControl
    {
        private readonly List<Control> pnlPropertyControls = new();
        private readonly List<TextBlock> propertyLabelList = new();

        private bool _ReadOnly = false;
        public bool ReadOnly
        {
            get => _ReadOnly;
            set
            {
                _ReadOnly = value;
                TravelChild(PnlProperty, control =>
                {
                    if (control is TextBox textBox)
                        textBox.IsReadOnly = value;
                    else if (control is Button button)
                        button.IsEnabled = !value;
                    else if (control is CheckBox checkBox)
                        checkBox.IsEnabled = !value;
                    else if (control is ComboBox comboBox)
                        comboBox.IsEnabled = !value;
                    else if (control is NumericUpDown numericUpDown)
                        numericUpDown.IsEnabled = !value;
                });
            }
        }

        private static void TravelChild(Control control, Action<Control> action)
        {
            action(control);
            if (control is Panel panel)
            {
                foreach (var child in panel.Children)
                    TravelChild(child, action);
            }
            else if (control is Border border)
            {
                TravelChild(border.Child, action);
            }
            else if (control is ContentControl contentControl && contentControl.Content is Control child1)
            {
                TravelChild(child1, action);
            }
        }

        public AotPropertyGrid()
        {
            InitializeComponent();
        }

        public void RegisterGroup(string groupName)
        {
            var collapse = false;
            var control = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#FFE0E0E0")),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Avalonia.Thickness(0, 0, 0, 1),
                Padding = new Avalonia.Thickness(8, 6, 8, 6),
                Child = new TextBlock
                {
                    Text = $"∇ {groupName}",
                    FontWeight = FontWeight.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            control.PointerPressed += (_, _) =>
            {
                collapse = !collapse;
                ((TextBlock)control.Child!).Text = collapse ? $"▷ {groupName}" : $"∇ {groupName}";

                var enterGroup = false;
                foreach (var child in pnlPropertyControls)
                {
                    if (enterGroup)
                    {
                        if (child is Border border && border.Child is TextBlock)
                            break;
                        child.IsVisible = !collapse;
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

        private TextBlock CreatePropertyLabel(string propertyName, string propertyDescription)
        {
            var control = new TextBlock
            {
                Text = propertyName,
                Width = 280,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Avalonia.Thickness(8, 4, 8, 4)
            };
            control.PointerPressed += (_, _) =>
            {
                LblPropertyName.Text = propertyName;
                LblPropertyDescription.Text = propertyDescription;
            };
            return control;
        }

        private TextBox CreatePropertyTextBox(string propertyName, string propertyDescription)
        {
            var control = new TextBox
            {
                IsReadOnly = ReadOnly,
                Margin = new Avalonia.Thickness(0),
                MinHeight = 30
            };
            control.GotFocus += (_, _) =>
            {
                LblPropertyName.Text = propertyName;
                LblPropertyDescription.Text = propertyDescription;
            };
            return control;
        }

        private CheckBox CreatePropertyCheckBox(string propertyName, string propertyDescription)
        {
            var control = new CheckBox
            {
                IsEnabled = !ReadOnly,
                Padding = new Avalonia.Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            return control;
        }

        private ComboBox CreatePropertyComboBox(string propertyName, string propertyDescription)
        {
            var control = new ComboBox
            {
                IsEnabled = !ReadOnly,
                MinHeight = 30
            };
            return control;
        }

        private NumericUpDown CreatePropertyNumericUpDown(string propertyName, string propertyDescription)
        {
            var control = new NumericUpDown
            {
                IsEnabled = !ReadOnly,
                Minimum = int.MinValue,
                Maximum = int.MaxValue,
                MinHeight = 30
            };
            return control;
        }

        private void AddPropertyControl(TextBlock label, Control valueControl)
        {
            propertyLabelList.Add(label);
            LinkControl(label, valueControl);

            var panel = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("280,4,*"),
                Margin = new Avalonia.Thickness(0),
                MinHeight = 34
            };

            Grid.SetColumn(label, 0);
            panel.Children.Add(label);

            var splitter = new Border
            {
                Background = new SolidColorBrush(Colors.Gray),
                Width = 2
            };
            Grid.SetColumn(splitter, 1);
            panel.Children.Add(splitter);

            Grid.SetColumn(valueControl, 2);
            panel.Children.Add(valueControl);

            var wrapper = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.LightGray),
                BorderThickness = new Avalonia.Thickness(0, 0, 0, 1),
                Child = panel
            };

            pnlPropertyControls.Add(wrapper);
        }

        private void ActiveLabel(TextBlock label)
        {
            foreach (var item in propertyLabelList)
            {
                if (item == label)
                {
                    item.Foreground = new SolidColorBrush(Colors.White);
                    item.Background = new SolidColorBrush(Color.Parse("#FF0078D4"));
                }
                else
                {
                    item.Foreground = new SolidColorBrush(Colors.Black);
                    item.Background = new SolidColorBrush(Colors.White);
                }
            }
        }

        private void LinkControl(TextBlock label, Control control)
        {
            label.PointerPressed += (_, _) =>
            {
                ActiveLabel(label);
                control.Focus();
            };
            control.GotFocus += (_, _) =>
            {
                ActiveLabel(label);
            };
        }

        public void RegisterProperty(string propertyName, string propertyDescription, Func<string> getValueHandler, Action<string> setValueHandler)
        {
            var textBox = CreatePropertyTextBox(propertyName, propertyDescription);
            textBox.Text = getValueHandler();
            textBox.TextChanged += (_, _) => setValueHandler(textBox.Text ?? "");
            AddPropertyControl(CreatePropertyLabel(propertyName, propertyDescription), textBox);
        }

        public void RegisterProperty(string propertyName, string propertyDescription, Func<int> getValueHandler, Action<int> setValueHandler)
        {
            var numericUpDown = CreatePropertyNumericUpDown(propertyName, propertyDescription);
            numericUpDown.Value = getValueHandler();
            numericUpDown.ValueChanged += (_, _) =>
            {
                if (numericUpDown.Value.HasValue)
                    setValueHandler(Convert.ToInt32(numericUpDown.Value.Value));
            };
            AddPropertyControl(CreatePropertyLabel(propertyName, propertyDescription), numericUpDown);
        }

        public void RegisterProperty(string propertyName, string propertyDescription, Func<int?> getValueHandler, Action<int?> setValueHandler)
        {
            var numericUpDown = CreatePropertyNumericUpDown(propertyName, propertyDescription);
            numericUpDown.Value = getValueHandler();
            numericUpDown.ValueChanged += (_, _) =>
            {
                if (numericUpDown.Value.HasValue)
                    setValueHandler(Convert.ToInt32(numericUpDown.Value.Value));
                else
                    setValueHandler(null);
            };
            AddPropertyControl(CreatePropertyLabel(propertyName, propertyDescription), numericUpDown);
        }

        public void RegisterProperty(string propertyName, string propertyDescription, Func<bool> getValueHandler, Action<bool> setValueHandler)
        {
            var checkBox = CreatePropertyCheckBox(propertyName, propertyDescription);
            checkBox.IsChecked = getValueHandler();
            checkBox.IsCheckedChanged += (_, _) => setValueHandler(checkBox.IsChecked ?? false);
            AddPropertyControl(CreatePropertyLabel(propertyName, propertyDescription), checkBox);
        }

        public void RegisterProperty<TEnum>(string propertyName, string propertyDescription, Func<TEnum> getValueHandler, Action<TEnum> setValueHandler)
            where TEnum : struct, Enum
        {
            var comboBox = CreatePropertyComboBox(propertyName, propertyDescription);
            foreach (var item in Enum.GetValues<TEnum>())
                comboBox.Items.Add(item);
            comboBox.SelectedItem = getValueHandler();
            comboBox.SelectionChanged += (_, _) =>
            {
                if (comboBox.SelectedItem is TEnum val)
                    setValueHandler(val);
            };
            AddPropertyControl(CreatePropertyLabel(propertyName, propertyDescription), comboBox);
        }

        /// <summary>
        /// 生成控件
        /// </summary>
        public void GenerateControls()
        {
            for (var i = 0; i < pnlPropertyControls.Count; i++)
            {
                PnlProperty.Children.Add(pnlPropertyControls[i]);
            }
        }
    }
}
