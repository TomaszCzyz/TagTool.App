using Avalonia.Media;

namespace TagTool.App.Controls.Logic;

public interface IDraggableTabItemTemplate
{
    public bool IsClosable { get; }

    public IImage? Icon { get; }
}
