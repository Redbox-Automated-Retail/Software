namespace Redbox.KioskDataServices.Proxy.ComponentModel;

public class Constants
{
	public class Messages
	{
		public const string AddKioskSession = "AddKioskSession";

		public const string AddKioskSessionJson = "AddKioskSessionJson";
	}

	public class ApiUrls
	{
		public const string AddKioskSession = "api/KioskData/AddKioskSession";

		public const string AddKioskSessionJson = "api/KioskData/AddKioskSessionJson";
	}

	public class RequestHeaderKeys
	{
		public const string RedboxKioskId = "x-redbox-kioskid";

		public const string RedboxActivityId = "x-redbox-activityid";
	}
}
