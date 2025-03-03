using Grpc.Net.Client;
using TagTool.BackendNew.Services.Grpc;

namespace TagTool.App.Services;

public interface ITagToolBackend
{
    public TagsService.TagsServiceClient GetTagService();
}

public class TagToolBackend : ITagToolBackend
{
    private readonly ITagToolBackendConnectionFactory _connectionFactory;

    public TagToolBackend(ITagToolBackendConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // public TagService.TagServiceClient GetTagService() => new(_connectionFactory.Create());
    public TagsService.TagsServiceClient GetTagService() => new(GrpcChannel.ForAddress("http://localhost:5280"));
}
