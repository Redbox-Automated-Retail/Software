using Microsoft.AspNetCore.Mvc;
using Redbox.NetCore.Middleware.Filter;
using Redbox.NetCore.Middleware.Middleware;

namespace Redbox.NetCore.Logging.Extensions;

public static class MvcOptionsExtensions
{
	public static void AddApiMetricsFilter(this MvcOptions options)
	{
		options.Filters.Add(typeof(MetricsActionFilter));
	}

	public static void AddValidateModelFilter(this MvcOptions options)
	{
		options.Filters.Add(typeof(ValidateModelAttribute));
	}
}
