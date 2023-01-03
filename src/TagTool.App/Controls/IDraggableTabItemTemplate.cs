using Avalonia.Media;

namespace TagTool.App.Controls;

public interface IDraggableTabItemTemplate
{
    public bool IsClosable { get; }
    public IImage? Icon { get; }
}
