namespace Redbox.NetCore.Middleware.Http;

public interface IVersionedResponse
{
	int Version { get; }
}
