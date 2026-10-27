using System.Net;
using System.Threading.Tasks;

namespace Redbox.NetCore.Middleware.Abstractions;

public interface IApplicationMetrics
{
	Task CaptureEndpointExecutionAsync(string controllerName, string actionName, long responseMilliseconds, HttpStatusCode statusCode);

	Task CaptureUrlExecutionAsync(string url, long responseMilliseconds, HttpStatusCode statusCode);
}
