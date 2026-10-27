namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;

public class PreferencePageConstants
{
	public class EngineCoreGeneral
	{
		public class EnvironmentValues
		{
			public const string Development = "Development";

			public const string QA = "QA";

			public const string UAT = "UAT";

			public const string Production = "Production";
		}

		public const string PageName = "EngineCore_General";

		public const string Environment = "Environment";
	}

	public class StoreService
	{
		public const string SettingPath = "Store";

		public const string StoreNumber = "ID";

		public const string RunInitialSync = "RunInitialSync";

		public const string InitialSyncType = "InitialSyncType";

		public const string SynchronizeTimeToServer = "SynchronizeTimeToServer";

		public const string IsStandAlone = "IsStandAlone";
	}

	public class HealthService
	{
		public const string SettingPath = "Remote Services\\Health Services";

		public const string LiveCall = "LiveCall";

		public const string PingPeriod = "PingPeriod";

		public const string SmtpServer = "SmtpServer";

		public const string Recipients = "Recipients";
	}
}
