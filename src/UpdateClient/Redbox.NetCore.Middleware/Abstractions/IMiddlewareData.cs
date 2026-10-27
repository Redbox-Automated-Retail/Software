namespace Redbox.NetCore.Middleware.Abstractions;

public interface IMiddlewareData
{
	string ActivityId { get; set; }

	long? KioskId { get; set; }
}
