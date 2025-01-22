using Grpc.Net.Client;

namespace TagTool.App.Services;

public interface ITagToolBackendConnectionFactory
{
    public GrpcChannel Create();
}

public class GrpcChannelFactory : ITagToolBackendConnectionFactory
{
    public GrpcChannel Create() => UnixDomainSocketConnectionFactory.CreateChannel();
}
