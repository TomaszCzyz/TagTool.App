using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;

namespace TagTool.App.ViewModels.UserControls;

public partial class FileSystemViewModel : ObservableObject
{
    public ObservableCollection<Core.Models.File> Files { get; set; } = new();

    public ObservableCollection<AddressSegmentViewModel> AddressSegments { get; set; } = new();

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _address = string.Empty;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [ObservableProperty]
    private string _textBoxAddress = string.Empty;

    public FileSystemViewModel()
    {
        Files.AddRange(_exampleFiles);
        AddressSegments.Add(new AddressSegmentViewModel(new Folder(new DirectoryInfo(@"C:\"))));
        AddressSegments.Add(new AddressSegmentViewModel(new Folder(new DirectoryInfo(@"C:\Users"))));
        AddressSegments.Add(new AddressSegmentViewModel(new Folder(new DirectoryInfo(@"C:\Users\tczyz"))));
        AddressSegments.Add(new AddressSegmentViewModel(new Folder(new DirectoryInfo(@"C:\Users\tczyz\MyFiles"))));
    }

    [RelayCommand]
    private void NavigateToAddress(string address) => Address = address;

    partial void OnAddressChanged(string value)
    {
        _textBoxAddress = value;
        AddressSegments.AddRange(GetAddressSegments(value));
    }

    public void CancelNavigation()
    {
        TextBoxAddress = Address;
        IsEditing = false;
    }

    public void CommitNavigation()
    {
        Address = TextBoxAddress;
        IsEditing = false;
    }

    private IEnumerable<AddressSegmentViewModel> GetAddressSegments(string address)
    {
        if (!Directory.Exists(address)) return Enumerable.Empty<AddressSegmentViewModel>();

        var path = Path.GetFullPath(address);
        var directory = new DirectoryInfo(path);

        return GetAddressSegments(new Folder(directory));
    }

    private IEnumerable<AddressSegmentViewModel> GetAddressSegments(Folder folder)
    {
        if (folder.Parent is { } parent)
        {
            foreach (var segment in GetAddressSegments(parent))
            {
                yield return segment;
            }
        }

        yield return new AddressSegmentViewModel(folder);
    }

    private static readonly Core.Models.File[] _exampleFiles =
    {
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File2.txt", 12311111114, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File3.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File4.txt", 1212312334, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File5.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File6.txt", 1222234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1212334, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(1, "File1.txt", 1234, new DateTime(2022, 12, 12), new DateTime(2022, 12, 12), @"C:\Program Files"),
        new(2, "File2.txt", 1234, new DateTime(1999, 1, 1), new DateTime(1999, 1, 1), @"C:\Users\tczyz\Source\repos\LayersTraversing"),
        new(3, "File3.txt", 144234, new DateTime(2022, 2, 12), null, @"C:\Program Files"),
        new(4, "FileFile4", 13234, new DateTime(202, 12, 30), null, @"C:\Users\tczyz\Source"),
        new(5, "File5", 122334, new DateTime(1990, 12, 30), null, @"C:\Users\tczyz\Source\repos\LayersTraversing\file.txt")
    };
}
