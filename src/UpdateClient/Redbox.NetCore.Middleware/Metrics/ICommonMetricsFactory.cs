using Redbox.NetCore.Middleware.Abstractions;

namespace Redbox.NetCore.Middleware.Metrics;

public interface ICommonMetricsFactory
{
	ICommonMetrics GetImplementation<T>();
}
