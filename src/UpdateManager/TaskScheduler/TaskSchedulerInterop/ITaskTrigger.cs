using System;
using System.Runtime.InteropServices;

namespace TaskSchedulerInterop;

[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("148BD52B-A2AB-11CE-B11F-00AA00530503")]
public interface ITaskTrigger
{
	void SetTrigger([In][Out][MarshalAs(UnmanagedType.Struct)] ref TaskTrigger Trigger);

	void GetTrigger([In][Out][MarshalAs(UnmanagedType.Struct)] ref TaskTrigger Trigger);

	void GetTriggerString(out IntPtr TriggerString);
}
