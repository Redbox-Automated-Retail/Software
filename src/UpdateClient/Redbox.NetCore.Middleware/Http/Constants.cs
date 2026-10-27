namespace Redbox.NetCore.Middleware.Http;

internal static class Constants
{
	internal static class HeaderKeys
	{
		public const string ApiKey = "x-api-key";

		public const string AppName = "x-redbox-app-name";

		public const string Authorization = "authorization";

		public const string CorrelationId = "X-Correlation-ID";

		public const string OldAppName = "app-name";

		public const string RedboxActivityId = "x-redbox-activityid";

		public const string RedboxKioskId = "x-redbox-kioskid";
	}
}
