using Avalonia.Controls;
using Avalonia.Interactivity;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class TaskView : UserControl
{
    private TaskViewModel ViewModel => (TaskViewModel)DataContext!;

    public TaskView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        // workaround (probably the better one exists)
        // ComboBox was not updating after loading, because the value was set before view initialization.
        // And maybe also unusual binding of ItemsSource has something to do with it... 
        JobComboBox.SelectedItem = ViewModel.SelectedJob;
        base.OnLoaded(e);
    }
}
