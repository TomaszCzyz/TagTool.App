﻿using Avalonia.Controls;
using Avalonia.Input;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class ItemsTaggingTabView : UserControl
{
    private ItemsTaggingTabViewModel ViewModel => (ItemsTaggingTabViewModel)DataContext!;

    public ItemsTaggingTabView()
    {
        InitializeComponent();
        ItemsListBox.AddHandler(DragDrop.DropEvent, Drop);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.FileNames) || e.Data.GetFileNames() is not { } fullFileNames) return;

        var fileSystemInfos = fullFileNames
            .Select(path => (FileSystemInfo)(Directory.Exists(path)
                ? new DirectoryInfo(path)
                : new FileInfo(path)));

        ViewModel.AddItemsToListCommand.Execute(fileSystemInfos);
    }
}
