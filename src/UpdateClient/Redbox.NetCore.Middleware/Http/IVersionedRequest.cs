namespace Redbox.NetCore.Middleware.Http;

public interface IVersionedRequest
{
	int Version { get; set; }
}
