using TagTool.App.Contracts;
using TagTool.App.Core.Models;

namespace TagTool.App.Core;

public static class DesignData
{
    public static TaggableItem TaggableItemSample
        => new(Guid.NewGuid(), "Test display name", null, new HashSet<Tag>
        {
            new() { Id = 0, Text = "Tag" },
            new() { Id = 0, Text = "LongTag" },
            new() { Id = 0, Text = "VeryLongTag" },
            new() { Id = 0, Text = "Tag2" },
            new() { Id = 0, Text = "Tag3" },
            new() { Id = 0, Text = "Tag4" },
            new() { Id = 0, Text = "Tag5" }
        });
}
