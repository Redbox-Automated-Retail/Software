using Microsoft.AspNetCore.Mvc;
using Redbox.NetCore.Logging.Filter;

namespace Redbox.NetCore.Logging.Extensions;

public static class MvcOptionsExtensions
{
	public static void AddLoggingFilter(this MvcOptions options)
	{
		options.Filters.Add(typeof(LoggingActionFilter));
	}
}
