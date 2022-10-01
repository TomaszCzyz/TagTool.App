﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TagTool.App.Extensions;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class FileView : UserControl
{
    public FileView()
    {
        InitializeComponent();
        DataContext = Application.Current?.CreateInstance<FileViewModel>();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
