using Redbox.NetCore.Middleware.Abstractions;

namespace Redbox.NetCore.Middleware.Middleware;

public class MiddlewareData : IMiddlewareData
{
	public string ActivityId { get; set; }

	public long? KioskId { get; set; }
}
