using System;
using Redbox.NetCore.Middleware.Abstractions;

namespace Redbox.NetCore.Middleware.Metrics;

public class CommonMetricsFactory : ICommonMetricsFactory
{
	private readonly IServiceProvider _provider;

	public CommonMetricsFactory(IServiceProvider provider)
	{
		_provider = provider;
	}

	public ICommonMetrics GetImplementation<T>()
	{
		return _provider.GetService(typeof(T)) as ICommonMetrics;
	}
}
