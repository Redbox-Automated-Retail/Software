using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.App;

public static class MvcOptionsExtensions
{
	public static void AddLoggingActionFilter(this MvcOptions options)
	{
		options.Filters.Add(typeof(LoggingActionFilter));
	}
}
