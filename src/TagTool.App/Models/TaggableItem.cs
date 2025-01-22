using Avalonia.Media.Imaging;
using TagTool.App.Contracts;

namespace TagTool.App.Core.Models;

public record TaggableItem(Guid Id, string DisplayName, Bitmap? Icon, ISet<Tag> Tags);
