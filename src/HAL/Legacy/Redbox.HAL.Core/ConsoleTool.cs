using System.Runtime.InteropServices;

namespace Redbox.HAL.Core;

public static class ConsoleTool
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

	public static void SetConsoleControlHandler(ControlCallback handler)
	{
		SetConsoleCtrlHandler(handler, add: true);
	}

	[DllImport("kernel32.dll")]
	private static extern bool AttachConsole(int dwProcessId);

	[DllImport("kernel32.dll")]
	private static extern bool FreeConsole();

	[DllImport("kernel32")]
	private static extern bool SetConsoleCtrlHandler(ControlCallback handler, bool add);
}
