using System;
using System.Runtime.InteropServices;

namespace TaskSchedulerInterop;

[Guid("4086658a-cbbb-11cf-b604-00c04fd8d565")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IProvideTaskPage
{
	void GetPage([In] int tpType, [In] bool fPersistChanges, out IntPtr phPage);
}
