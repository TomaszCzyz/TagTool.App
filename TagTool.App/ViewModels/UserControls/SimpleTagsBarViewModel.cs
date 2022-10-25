using System.Collections.ObjectModel;
using DynamicData;
using TagTool.App.Core.Models;

namespace TagTool.App.ViewModels.UserControls;

public interface ITagsContainer
{
    void AddTag(Tag tag);

    void RemoveLast();
}

public class SimpleTagsBarViewModel : ViewModelBase, ITagsContainer
{
    public ObservableCollection<object> EnteredTags { get; set; } = new(); // Tag or TagSearchBox

    public SimpleTagsBarViewModel()
    {
        EnteredTags.AddRange(
            new Tag[] { new("Tag1"), new("Audio"), new("Dog"), new("Picture"), new("Colleague"), new("Tag6"), new("LastTag") });

        var tagSearchBoxViewModel = new TagSearchBoxViewModel(this);
        EnteredTags.Add(tagSearchBoxViewModel);
    }

    public void AddTag(Tag tag)
    {
        if (!EnteredTags.Contains(tag))
        {
            // workaround to remove text from AutoCompleteBox when IsTextCompletionEnabled is true and we select item by pressing enter with autocompleted text
            // but with the workaround focus becomes a problem...
            // var tagSearchBoxViewModel = new TagSearchBoxViewModel(this);
            // EnteredTags.RemoveAt(EnteredTags.Count - 1);
            // EnteredTags.Add(tag);
            // EnteredTags.Add(tagSearchBoxViewModel);

            EnteredTags.Insert(EnteredTags.Count - 1, tag);
        }
    }

    public void RemoveLast()
    {
        var lastTag = EnteredTags.LastOrDefault(o => o.GetType() == typeof(Tag));

        if (lastTag is null) return;

        EnteredTags.Remove(lastTag);
    }
}
