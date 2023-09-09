using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using TagTool.App.Core.Comparers;
using TagTool.App.Core.ViewModels;
using TagTool.App.Models;

namespace TagTool.App.ViewModels.Dialogs;

public partial class AdvancedTaggingDialogViewModel : ViewModelBase, IDisposable
{
    private static int _tagsLoadingTaskCounter;
    private readonly Node _root = new();

    [ObservableProperty]
    private string _rowDescription = "Existing Tags: ";

    [ObservableProperty]
    private int _tagsLoadingCounter;

    public ObservableCollection<Node> Items { get; }

    public ObservableCollection<Node> SelectedItems { get; } = new();

    public ObservableCollection<Tag> ExistingTags { get; set; } = new();

    public ObservableCollection<Tag> ImplicitTags { get; set; } = new(new Tag[] { new("Audio"), new("Text"), new("Date"), new("Zip") });

    public AdvancedTaggingDialogViewModel()
    {
        var child = new Node(new DirectoryInfo(@"C:\Users\tczyz\MyFiles\FromOec"));
        _root.AddItem(child);
        Items = _root.Children;

        SelectedItems.CollectionChanged += (_, _) =>
        {
            ExistingTags.Clear();

            switch (SelectedItems.Count)
            {
                case 0:
                    return;
                case 1:
                    RowDescription = "Existing Tags: ";
                    ExistingTags.AddRange(SelectedItems.SelectMany(node => node.Tags));
                    break;
                default:
                    RowDescription = "Intersection: ";
                    var observableCollections = SelectedItems.Select(node => node.Tags).ToArray();

                    var intersection = observableCollections
                        .Skip(1)
                        .Aggregate(
                            new HashSet<Tag>(observableCollections.First()),
                            (set, tags) =>
                            {
                                set.IntersectWith(tags);
                                return set;
                            }
                        );

                    ExistingTags.AddRange(intersection);
                    break;
            }
        };

        Task.Run(async () =>
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(750));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                TagsLoadingCounter = _tagsLoadingTaskCounter;
            }
        });
    }

    public void Dispose()
    {
        CancelAllTagsLoading();
        GC.SuppressFinalize(this);
    }

    [RelayCommand]
    private void AddItem(FileSystemInfo directoryInfo)
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
    private void CancelAllTagsLoading() => _root.CancelTagsLoading();

    public class Node : IDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private ObservableCollection<Node>? _children;

        public FileSystemInfo Item { get; init; }

        public Node? Parent { get; init; }

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

        public void Dispose()
        {
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }

        public void CancelTagsLoading()
        {
            foreach (var child in Children)
            {
                child.CancelTagsLoading();
            }

            _cts.Cancel();
        }

        private ObservableCollection<Node> InitializeChildren()
        {
            if (Item is not DirectoryInfo directoryInfo)
            {
                return new ObservableCollection<Node>();
            }

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
            if (Children.Contains(child))
            {
                return;
            }

            Children.Add(child);
            LoadTags(child);
        }

        public void RemoveItem(Node child) => Children.Remove(child);

        private void LoadTags(Node node)
            => Dispatcher.UIThread.InvokeAsync(async () =>
            {
                // todo: add upper limit for concurrent loadings (especially important for "expand all" functionality; NOTE: the order of expanding matters.. should be like BFS)
                Interlocked.Increment(ref _tagsLoadingTaskCounter);

                foreach (var i in Enumerable.Range(1, Random.Shared.Next(1, 6)))
                {
                    if (_cts.IsCancellationRequested)
                    {
                        return;
                    }

                    await Task.Delay(Random.Shared.Next(500, 2000));
                    node.Tags.Add(new Tag($"Tag{i}"));
                }

                Interlocked.Decrement(ref _tagsLoadingTaskCounter);
            });

        private bool Equals(Node other) => Item.FullName.Equals(other.Item.FullName, StringComparison.Ordinal);

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((Node)obj);
        }

        public override int GetHashCode() => Item.GetHashCode();

        public override string ToString() => Header;
    }
}
