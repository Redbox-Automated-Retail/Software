using System.Runtime.InteropServices;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EConsoleTool
{
	internal const int AttachParentProcess = -1;

	public static bool AttachConsoleToParentProcess()
	{
		return AttachConsole(-1);
	}

	public static bool FreeAttachedConsole()
	{
		return FreeConsole();
	}

	public static void SetConsoleControlHandler(Redbox_002EMacros1309_002EControlCallback handler)
	{
		SetConsoleCtrlHandler(handler, add: true);
	}

	[DllImport("kernel32.dll")]
	private static extern bool AttachConsole(int dwProcessId);

	[DllImport("kernel32.dll")]
	private static extern bool FreeConsole();

	[DllImport("kernel32")]
	private static extern bool SetConsoleCtrlHandler(Redbox_002EMacros1309_002EControlCallback handler, bool add);
}
