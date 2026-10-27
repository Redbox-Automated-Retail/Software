using System;
using System.Runtime.InteropServices;

namespace TaskSchedulerInterop;

[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("148BD527-A2AB-11CE-B11F-00AA00530503")]
public interface ITaskScheduler
{
	void SetTargetComputer([In][MarshalAs(UnmanagedType.LPWStr)] string Computer);

	void GetTargetComputer(out IntPtr Computer);

	void Enum([MarshalAs(UnmanagedType.Interface)] out IEnumWorkItems EnumWorkItems);

	void Activate([In][MarshalAs(UnmanagedType.LPWStr)] string Name, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object obj);

	void Delete([In][MarshalAs(UnmanagedType.LPWStr)] string Name);

	void NewWorkItem([In][MarshalAs(UnmanagedType.LPWStr)] string TaskName, [In] ref Guid rclsid, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object obj);

	void AddWorkItem([In][MarshalAs(UnmanagedType.LPWStr)] string TaskName, [In][MarshalAs(UnmanagedType.Interface)] ITask WorkItem);

	void IsOfType([In][MarshalAs(UnmanagedType.LPWStr)] string TaskName, [In] ref Guid riid);
}
