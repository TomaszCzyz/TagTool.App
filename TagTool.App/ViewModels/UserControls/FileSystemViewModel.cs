using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;

namespace TagTool.App.ViewModels.UserControls;

public partial class FileSystemViewModel : ObservableObject
{
    private readonly Stack<Folder> _navigationHistoryBack = new();
    private readonly Stack<Folder> _navigationHistoryForward = new();

    public ObservableCollection<Core.Models.File> Files { get; set; } = new();

    public ObservableCollection<AddressSegmentViewModel> AddressSegments { get; set; } = new();

    [ObservableProperty]
    private Folder _currentFolder;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _address = string.Empty;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [ObservableProperty]
    private string _textBoxAddress = string.Empty;

    public bool CanNavigateBack => _navigationHistoryBack.Count > 0;

    public bool CanNavigateForward => _navigationHistoryForward.Count > 0;

    public FileSystemViewModel()
    {
        _currentFolder = new Folder(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));

        Files.AddRange(_exampleFiles);
        AddressSegments.Add(new AddressSegmentViewModel(new Folder(new DirectoryInfo(@"C:\"))));
        AddressSegments.Add(new AddressSegmentViewModel(new Folder(new DirectoryInfo(@"C:\Users"))));
        AddressSegments.Add(new AddressSegmentViewModel(new Folder(new DirectoryInfo(@"C:\Users\tczyz"))));
        AddressSegments.Add(new AddressSegmentViewModel(new Folder(new DirectoryInfo(@"C:\Users\tczyz\MyFiles"))));
    }

    [RelayCommand]
    private void ChangeAddress(string address) => Address = address;

    [RelayCommand]
    public void CancelNavigation()
    {
        TextBoxAddress = Address;
        IsEditing = false;
    }

    [RelayCommand]
    public void CommitNavigation()
    {
        Address = TextBoxAddress;
        IsEditing = false;
    }

    [RelayCommand]
    public void NavigateUp()
    {
        if (CurrentFolder.Parent is { } parent)
        {
            NavigateTo(parent);
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    public void NavigateBack()
    {
        if (!_navigationHistoryBack.TryPop(out var directory)) return;

        _navigationHistoryForward.Push(CurrentFolder);

        NavigateBackCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();

        NavigateTo(directory, true);
    }

    [RelayCommand(CanExecute = nameof(CanNavigateForward))]
    public void NavigateForward()
    {
        if (!_navigationHistoryForward.TryPop(out var directory)) return;

        _navigationHistoryBack.Push(CurrentFolder);

        NavigateBackCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();

        NavigateTo(directory, true);
    }

    partial void OnAddressChanged(string value)
    {
        TextBoxAddress = value;
        AddressSegments.Clear();
        AddressSegments.AddRange(GetAddressSegments(value));
        NavigateToAddress(value);
    }

    partial void OnCurrentFolderChanged(Folder value)
    {
        Address = value.FullPath;
    }

    private void NavigateTo(Folder folder, bool isHistoryNavigation = false)
    {
        if (!CurrentFolder.Equals(folder) && !isHistoryNavigation)
        {
            _navigationHistoryBack.Push(CurrentFolder);
            NavigateBackCommand.NotifyCanExecuteChanged();

            _navigationHistoryForward.Clear();
            NavigateForwardCommand.NotifyCanExecuteChanged();
        }

        CurrentFolder = folder;
    }

    private IEnumerable<AddressSegmentViewModel> GetAddressSegments(string address)
    {
        if (!Directory.Exists(address))
        {
            return Enumerable.Empty<AddressSegmentViewModel>();
        }

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

    private void NavigateToAddress(string address)
    {
        if (Directory.Exists(address))
        {
            var path = Path.GetFullPath(address);
            var folder = new Folder(new DirectoryInfo(path));

            NavigateTo(folder);

            return;
        }

        if (System.IO.File.Exists(address))
        {
            using var process = new Process();

            process.StartInfo.FileName = address;
            process.StartInfo.UseShellExecute = true;

            process.Start();
        }
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
