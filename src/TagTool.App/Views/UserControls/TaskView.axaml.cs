using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class TaskView : UserControl
{
    private readonly ObservableCollection<TaskTriggerView> _taskTriggerViews = new(new[] { new TaskTriggerView() });

    public TaskView()
    {
        InitializeComponent();
        DataContext = AppTemplate.Current.Services.GetRequiredService<TaskViewModel>();
        TriggersItemsControl.ItemsSource = _taskTriggerViews;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        _taskTriggerViews.Add(new TaskTriggerView());
    }

    private void JobComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count != 1)
        {
            return;
        }

        var jonInfo = (JobInfo)e.AddedItems[0]!;
        JobArgumentsTextBox.Text = jonInfo.ArgumentsExample;
    }
}
