using TagTool.App.Contracts;
using TagTool.App.Views;

namespace TagTool.App;

public static class DesignData
{
    public static TaggableItemViewModel TaggableItemViewModelSample
        => new(
            new TaggableFile.TaggableFile
            {
                Id = Guid.NewGuid(),
                Path = "",
                Tags = new HashSet<Tag>
                {
                    new() { Id = 0, Text = "Tag" },
                    new() { Id = 0, Text = "LongTag" },
                    new() { Id = 0, Text = "VeryLongTag" },
                    new() { Id = 0, Text = "Tag2" },
                    new() { Id = 0, Text = "Tag3" },
                    new() { Id = 0, Text = "Tag4" },
                    new() { Id = 0, Text = "Tag5" }
                }
            },
            null,
            "Test display name"
        );
}
