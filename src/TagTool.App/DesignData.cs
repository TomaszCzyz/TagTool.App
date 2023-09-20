using TagTool.App.ViewModels.UserControls;

namespace TagTool.App;

public static class DesignData
{
    public static TaggableItemsSearchViewModel TaggableItemsSearchViewModel { get; } = new();

    public static TagsLibraryViewModel TagsLibraryViewModel { get; } = new();

    public static TagsAssociationsViewModel TagsAssociationsViewModel { get; } = new();
}
