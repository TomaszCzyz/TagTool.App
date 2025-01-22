using Avalonia.Media.Imaging;
using TagTool.App.Contracts;
using TagTool.App.Core.Models;

namespace TagTool.App.Core.Views;

public record TaggableItemModel(Guid Id, string DisplayName, Bitmap? Icon, ISet<Tag> Tags)
{
    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggableItemModel() : this(Guid.NewGuid(), "Test display name", null, new HashSet<Tag> { new() { Id = 0, Text = "Tag" } })
    {
        // if (!Design.IsDesignMode)
        // {
        //     Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        // }
    }
}
