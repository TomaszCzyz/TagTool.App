using Grpc.Net.Client;

namespace TagTool.App.Core.Services;

public interface ITagToolBackendConnectionFactory
{
    public GrpcChannel Create();
}

public class GrpcChannelFactory : ITagToolBackendConnectionFactory
{
    public GrpcChannel Create() => UnixDomainSocketConnectionFactory.CreateChannel();
}
