using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Reactive;

namespace TagTool.App.Controls;

public class DraggableTabItemContainerGenerator : ItemContainerGenerator<DraggableTabItem>
{
    public DraggableTabItemContainerGenerator(
        IControl owner,
        AvaloniaProperty contentProperty,
        AvaloniaProperty contentTemplateProperty,
        AvaloniaProperty iconProperty,
        AvaloniaProperty isClosableProperty) : base(owner, contentProperty, contentTemplateProperty)
    {
        _iconProperty = iconProperty;
        _isClosableProperty = isClosableProperty;
    }

    private readonly AvaloniaProperty _isClosableProperty, _iconProperty;

    private IControl CreateContainer<T>(object item) where T : class, IControl, new()
    {
        if (item is T container)
        {
            return container;
        }

        var result = new T();

        if (ContentTemplateProperty != null)
        {
            result.SetValue(ContentTemplateProperty, ItemTemplate, BindingPriority.Style);
        }

        result.SetValue(ContentProperty, item, BindingPriority.Style);

        if (item is not IControl)
        {
            result.DataContext = item;
        }

        return result;
    }

    protected override IControl CreateContainer(object item)
    {
        var tabItem = (DraggableTabItem)CreateContainer<DraggableTabItem>(item);

        tabItem.Bind(TabItem.TabStripPlacementProperty, new OwnerBinding<Dock>(tabItem, TabControl.TabStripPlacementProperty));

        if (tabItem.HeaderTemplate == null)
        {
            tabItem.Bind(
                HeaderedContentControl.HeaderTemplateProperty,
                new OwnerBinding<IDataTemplate>(
                    tabItem,
                    ItemsControl.ItemTemplateProperty!));
        }

        if (tabItem.Header == null)
        {
            if (item is IHeadered headered)
            {
                tabItem.Header = headered.Header;
            }
            else if (tabItem.DataContext is not IControl)
            {
                tabItem.Header = tabItem.DataContext;
            }
        }

        if (tabItem.Content is not IControl)
        {
            tabItem.Bind(
                ContentControl.ContentTemplateProperty,
                new OwnerBinding<IDataTemplate>(
                    tabItem,
                    TabControl.ContentTemplateProperty!));
        }

        if (item is IDraggableTabItemTemplate tab)
        {
            if (tab.Icon is not null)
            {
                tabItem.SetValue(_iconProperty, tab.Icon, BindingPriority.Style);
            }

            tabItem.SetValue(_isClosableProperty, tab.IsClosable, BindingPriority.Style);
        }

        return tabItem;
    }

    private class OwnerBinding<T> : SingleSubscriberObservableBase<T>
    {
        private readonly TabItem _item;
        private readonly StyledProperty<T> _ownerProperty;
        private IDisposable? _ownerSubscription;
        private IDisposable? _propertySubscription;

        public OwnerBinding(TabItem item, StyledProperty<T> ownerProperty)
        {
            _item = item;
            _ownerProperty = ownerProperty;
        }

        protected override void Subscribed()
        {
            _ownerSubscription = ControlLocator.Track(_item, 0, typeof(TabControl)).Subscribe(OwnerChanged);
        }

        protected override void Unsubscribed()
        {
            _ownerSubscription?.Dispose();
            _ownerSubscription = null;
        }

        private void OwnerChanged(ILogical? c)
        {
            _propertySubscription?.Dispose();
            _propertySubscription = null;

            if (c is TabControl tabControl)
            {
                _propertySubscription = tabControl.GetObservable(_ownerProperty).Subscribe(PublishNext);
            }
        }
    }
}
