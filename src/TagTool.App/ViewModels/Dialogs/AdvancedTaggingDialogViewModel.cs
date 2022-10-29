using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace TagTool.App.ViewModels.Dialogs;

public partial class AdvancedTaggingDialogViewModel : ViewModelBase
{
    private readonly Node _root = new();

    public ObservableCollection<Node> Items { get; }

    public ObservableCollection<Node> SelectedItems { get; } = new();

    public AdvancedTaggingDialogViewModel()
    {
        Items = _root.Children;
    }

    [RelayCommand]
    private void AddItem()
    {
        var parentItem = SelectedItems.Count > 0 ? SelectedItems[0] : _root;
        parentItem.AddItem();
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

        bool RecursiveRemove(ObservableCollection<Node> items, Node selectedItem)
        {
            return items.Remove(selectedItem)
                   || items.Any(item => item.AreChildrenInitialized && RecursiveRemove(item.Children, selectedItem));
        }
    }

    public class Node
    {
        private int _childIndex = 10;

        private ObservableCollection<Node>? _children;

        public string Header { get; }

        public Node? Parent { get; }

        public Node()
        {
            Header = "Item";
        }

        public Node(Node parent, int index)
        {
            Parent = parent;
            Header = parent.Header + ' ' + index;
        }

        public bool AreChildrenInitialized => _children != null;

        public ObservableCollection<Node> Children => _children ??= CreateChildren();

        public void AddItem() => Children.Add(new Node(this, _childIndex++));

        public void RemoveItem(Node child) => Children.Remove(child);

        public override string ToString() => Header;

        private ObservableCollection<Node> CreateChildren()
        {
            return new ObservableCollection<Node>(Enumerable.Range(0, 10).Select(i => new Node(this, i)));
        }
    }
}
