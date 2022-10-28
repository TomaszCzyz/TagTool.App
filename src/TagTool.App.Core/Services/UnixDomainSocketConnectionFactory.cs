using System.Net;
using System.Net.Sockets;
using Grpc.Net.Client;
using TagTool.Backend;

namespace TagTool.App.Core.Services;

public class TagSearchServiceFactory
{
    public TagSearchService.TagSearchServiceClient Create()
    {
        var grpcChannel = UnixDomainSocketConnectionFactory.CreateChannel();
        return new TagSearchService.TagSearchServiceClient(grpcChannel);
    }
}

public class TagServiceFactory
{
    public TagService.TagServiceClient Create()
    {
        var grpcChannel = UnixDomainSocketConnectionFactory.CreateChannel();
        return new TagService.TagServiceClient(grpcChannel);
    }
}

public class UnixDomainSocketConnectionFactory
{
    private static readonly string _socketPath = Path.Combine(Path.GetTempPath(), "socket.tmp");

    private readonly EndPoint _endPoint;

    private UnixDomainSocketConnectionFactory(EndPoint endPoint)
    {
        _endPoint = endPoint;
    }

    public static GrpcChannel CreateChannel()
    {
        var udsEndPoint = new UnixDomainSocketEndPoint(_socketPath);
        var connectionFactory = new UnixDomainSocketConnectionFactory(udsEndPoint);
        var socketsHttpHandler = new SocketsHttpHandler { ConnectCallback = connectionFactory.ConnectAsync };
        var grpcChannelOptions = new GrpcChannelOptions { HttpHandler = socketsHttpHandler, ThrowOperationCanceledOnCancellation = false };

        return GrpcChannel.ForAddress("http://localhost", grpcChannelOptions);
    }

    private async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _,
        CancellationToken cancellationToken = default)
    {
        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

        try
        {
            await socket.ConnectAsync(_endPoint, cancellationToken).ConfigureAwait(false);
            return new NetworkStream(socket, true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}
