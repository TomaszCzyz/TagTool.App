using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TagTool.App.Core.Models;

namespace TagTool.App.ViewModels.Dialogs;

public partial class AdvancedTaggingDialogViewModel : ViewModelBase
{
    private readonly Node _root = new();

    public ObservableCollection<Node> Items { get; }

    public ObservableCollection<Node> SelectedItems { get; } = new();

    public AdvancedTaggingDialogViewModel()
    {
        _root.AddItem(new Node(new DirectoryInfo(@"C:\Users\tczyz\MyFiles")));
        Items = _root.Children;
    }

    // [RelayCommand]
    // private void AddItem()
    // {
    //     var parentItem = SelectedItems.Count > 0 ? SelectedItems[0] : _root;
    //     parentItem.AddItem();
    // }

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

    public class Node
    {
        public FileSystemInfo Item { get; init; }

        public Node? Parent { get; init; }

        private ObservableCollection<Node>? _children;

        public ObservableCollection<Node> Children => _children ??= InitializeChildren();

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

        private ObservableCollection<Node> InitializeChildren()
        {
            if (Item is not DirectoryInfo directoryInfo) return new ObservableCollection<Node>();

            var array = directoryInfo
                .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                .Select(info => new Node(this, info))
                .OrderByDescending(node => node.Item, FileSystemInfoComparer.Comparer)
                .ToArray();

            return new ObservableCollection<Node>(array);
        }

        public void AddItem(Node child) => Children.Add(child);

        public void RemoveItem(Node child) => Children.Remove(child);

        public override string ToString() => Header;
    }
}
