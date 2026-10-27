namespace Redbox.HAL.Client;

public static class Constants
{
	public static class ExecutionContexts
	{
		public static readonly string ImmediateModeContext = "IMMEDIATE";
	}

	public static class HALIPCStrings
	{
		public static readonly string TcpServer = "rcp://127.0.0.1:7001";

		public static readonly string PipeServer = "rcp-p://HALService:7001";
	}
}
