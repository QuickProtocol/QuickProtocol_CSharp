using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace QpTestClient;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var viewModel = new ViewModels.MainWindowViewModel(this);
        DataContext = viewModel;
        InitializeComponent();
    }

    private T FindVisualTreeChild<T>(Visual visual) where T : Visual
    {
        foreach (var child in visual.GetVisualChildren())
        {
            if (visual is T t)
                return t;
            var childT = FindVisualTreeChild<T>(child);
            if (childT != null)
                return childT;
        }
        return null;
    }
}