namespace TagTool.App.Options;

public enum FilePickerStartFolderMode
{
    Previous = 0,
    Fixed = 1
}

public class GeneralOptions
{
    public const string General = "General";

    public FilePickerStartFolderMode FilePickerStartFolderMode { get; set; } = FilePickerStartFolderMode.Previous;

    public string? FilePickerStartFolder { get; set; }
}
