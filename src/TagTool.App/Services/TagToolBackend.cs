using TagTool.Backend;

namespace TagTool.App.Services;

public interface ITagToolBackend
{
    public TagService.TagServiceClient GetTagService();

    public FileActionsService.FileActionsServiceClient GetFileActionsService();

    public FolderActionsService.FolderActionsServiceClient GetFolderActionsService();

    public SearchService.SearchServiceClient GetFileSystemSearchService();
}

public class TagToolBackend : ITagToolBackend
{
    private readonly ITagToolBackendConnectionFactory _connectionFactory;

    public TagToolBackend(ITagToolBackendConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public TagService.TagServiceClient GetTagService() => new(_connectionFactory.Create());

    public FileActionsService.FileActionsServiceClient GetFileActionsService() => new(_connectionFactory.Create());

    public FolderActionsService.FolderActionsServiceClient GetFolderActionsService() => new(_connectionFactory.Create());

    public SearchService.SearchServiceClient GetFileSystemSearchService() => new(_connectionFactory.Create());
}
