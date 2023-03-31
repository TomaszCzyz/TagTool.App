using TagTool.Backend;

namespace TagTool.App.Core.Services;

public interface ITagToolBackend
{
    public FileActionsService.FileActionsServiceClient GetFileActionsService();

    public TagService.TagServiceClient GetTagService();
}

public class TagToolBackend : ITagToolBackend
{
    private readonly ITagToolBackendConnectionFactory _connectionFactory;

    public TagToolBackend(ITagToolBackendConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public FileActionsService.FileActionsServiceClient GetFileActionsService() => new(_connectionFactory.Create());

    public TagService.TagServiceClient GetTagService() => new(_connectionFactory.Create());
}
