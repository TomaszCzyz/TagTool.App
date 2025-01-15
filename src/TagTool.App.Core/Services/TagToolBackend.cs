using TagTool.BackendNew;

namespace TagTool.App.Core.Services;

public interface ITagToolBackend
{
    public TagService.TagServiceClient GetTagService();
}

public class TagToolBackend : ITagToolBackend
{
    private readonly ITagToolBackendConnectionFactory _connectionFactory;

    public TagToolBackend(ITagToolBackendConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public TagService.TagServiceClient GetTagService() => new(_connectionFactory.Create());
}
