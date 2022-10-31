using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagTool.App.Core.Models;

namespace TagTool.App.ViewModels.Dialogs;

public partial class AdvancedTaggingDialogViewModel : ViewModelBase, IDisposable
{
    private readonly Node _root = new();

    public ObservableCollection<Node> Items { get; }

    public ObservableCollection<Node> SelectedItems { get; } = new();

    [ObservableProperty]
    private bool _isTagsLoading;

    public AdvancedTaggingDialogViewModel()
    {
        var child = new Node(new DirectoryInfo(@"C:\Users\tczyz\MyFiles\FromOec"));
        _root.AddItem(child);
        Items = _root.Children;
    }

    [RelayCommand]
    private void AddItem(DirectoryInfo directoryInfo)
    {
        var child = new Node(directoryInfo);

        _root.AddItem(child);
    }

    [RelayCommand]
    private void RemoveItem()
    {
        while (SelectedItems.Count > 0)
        {
            var item = SelectedItems[0];
            RecursiveRemove(Items, item);
            SelectedItems.RemoveAt(0);
        }

        bool RecursiveRemove(ICollection<Node> items, Node selectedItem)
        {
            return items.Remove(selectedItem) || items.Any(item => item.AreChildrenInitialized && RecursiveRemove(item.Children, selectedItem));
        }
    }

    [RelayCommand]
    private void CancelAllTagsLoading()
    {
        _root.CancelTagsLoading();
    }

    public void Dispose()
    {
        CancelAllTagsLoading();
        GC.SuppressFinalize(this);
    }

    public class Node
    {
        public FileSystemInfo Item { get; init; }

        public Node? Parent { get; init; }

        private ObservableCollection<Node>? _children;

        public ObservableCollection<Node> Children => _children ??= InitializeChildren();

        public ObservableCollection<Tag> Tags { get; set; } = new();

        public string Header { get; }

        public bool AreChildrenInitialized => _children != null;

        public Node()
        {
            Item = null!;
            Header = "NoItem";
        }

        public Node(FileSystemInfo item)
        {
            Item = item;
            Header = item.Name;
        }

        private Node(Node parent, FileSystemInfo item) : this(item)
        {
            Parent = parent;
        }

        public void CancelTagsLoading()
        {
            foreach (var child in Children)
            {
                child.CancelTagsLoading();
            }

            _cts.Cancel();
        }

        private readonly CancellationTokenSource _cts = new();

        private ObservableCollection<Node> InitializeChildren()
        {
            if (Item is not DirectoryInfo directoryInfo) return new ObservableCollection<Node>();

            var array = directoryInfo
                .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                .Select(info => new Node(this, info))
                .OrderByDescending(node => node.Item, FileSystemInfoComparer.Comparer)
                .ToArray();

            // todo: safeguard for large directories as TreeView cannot handle too many entries;
            // desired solution: shows first 50 files/folders and button to load next 50
            // also disabling loading Tags would be nice 

            foreach (var node in array)
            {
                LoadTags(node);
            }

            return new ObservableCollection<Node>(array);
        }

        public void AddItem(Node child)
        {
            Children.Add(child);
            LoadTags(child);
        }

        public void RemoveItem(Node child) => Children.Remove(child);

        private void LoadTags(Node node)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                foreach (var i in Enumerable.Range(1, Random.Shared.Next(1, 8)))
                {
                    if (_cts.IsCancellationRequested)
                    {
                        return;
                    }

                    await Task.Delay(Random.Shared.Next(500, 2000));
                    node.Tags.Add(new Tag($"Tag{i}"));
                }
            });
        }

        public override string ToString() => Header;
    }
}
