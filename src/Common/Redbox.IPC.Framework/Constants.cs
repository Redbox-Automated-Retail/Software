namespace Redbox.IPC.Framework;

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

	public static class Communications
	{
		public const int Timeout = 30000;
	}

	public static class ProtocolDefaults
	{
		public const string NamedPipe = "halservice";

		public const int TcpPort = 7001;
	}

	public const string RemotingIPKey = "RemoteHostIP";

	public const string RemotingHostKey = "RemotingHost";
}
