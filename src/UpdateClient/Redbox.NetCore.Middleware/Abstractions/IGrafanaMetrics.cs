using System;
using System.Threading.Tasks;

namespace Redbox.NetCore.Middleware.Abstractions;

public interface IGrafanaMetrics
{
	void CaptureError(Exception ex);

	Task<T> CaptureExternalServiceAsync<T>(string service, string method, Func<Task<T>> action);

	void CaptureExternalError(string service, string method, Exception ex);

	void CaptureResponseCode(string route, string method, string action, string code);

	void CaptureResponseTime(string route, string method, string action, string code, long responseTime);
}
