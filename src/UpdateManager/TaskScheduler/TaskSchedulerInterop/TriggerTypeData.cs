using System.Runtime.InteropServices;

namespace TaskSchedulerInterop;

[StructLayout(LayoutKind.Explicit)]
public struct TriggerTypeData
{
	[FieldOffset(0)]
	public Daily daily;

	[FieldOffset(0)]
	public Weekly weekly;

	[FieldOffset(0)]
	public MonthlyDate monthlyDate;

	[FieldOffset(0)]
	public MonthlyDOW monthlyDOW;
}
