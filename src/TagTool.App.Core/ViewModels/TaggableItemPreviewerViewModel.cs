using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TagTool.App.Core.ViewModels;

public partial class TaggableItemPreviewerViewModel : ViewModelBase
{
    [ObservableProperty]
    private IImage? _image;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggableItemPreviewerViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        Image = new Bitmap(@"C:\Users\tczyz\Pictures\2022 - Aply\20220326_124320.jpg");
    }
}
