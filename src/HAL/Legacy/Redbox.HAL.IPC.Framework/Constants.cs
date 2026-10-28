namespace Redbox.HAL.IPC.Framework;

public static class Constants
{
	public static class Session
	{
		public const string QuitCommand = "quit";

		public const string SendMessage = "[MSG]";

		public const string GoodbyeMessage = "Goodbye!";

		public const string WelcomeMessage = "Welcome!";

		public const string FailureResponseCode = "545 Command";

		public const string SuccessResponseCode = "203 Command";
	}

	public static class DefaultProtocols
	{
		public const string Pipe = "rcp-p://HAL7001:7001";

		public const string Tcp = "rcp://127.0.0.1:7001";
	}

	public static class Communications
	{
		public const int StreamReadTimeout = 30000;

		public const int StreamWriteTimeout = 30000;
	}

	public static class Configuration
	{
		public const string SettingsKey = "SOFTWARE\\Redbox\\HAL";
	}
}
