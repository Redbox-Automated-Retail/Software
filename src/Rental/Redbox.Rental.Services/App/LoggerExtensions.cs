using Redbox.Core;

namespace Redbox.Rental.Services.App;

public static class LoggerExtensions
{
	public static void LogInformation(this LogHelper logHelper, string message)
	{
		logHelper.Log(message);
	}
}
