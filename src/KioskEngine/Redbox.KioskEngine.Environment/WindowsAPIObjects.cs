using System;

namespace Redbox.KioskEngine.Environment;

public class WindowsAPIObjects
{
	public struct POINT
	{
		public int x;

		public int y;
	}

	public struct MSLLHOOKSTRUCT
	{
		public POINT pt;

		public uint mouseData;

		public uint flags;

		public uint time;

		public IntPtr dwExtraInfo;
	}

	public const int WH_MOUSE_LL = 14;
}
