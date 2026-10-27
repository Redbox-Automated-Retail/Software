using System.Runtime.InteropServices;

namespace TaskSchedulerInterop;

public class PropertySheetDisplay
{
	[DllImport("comctl32.dll")]
	public static extern int PropertySheet([In][MarshalAs(UnmanagedType.Struct)] ref PropSheetHeader psh);
}
