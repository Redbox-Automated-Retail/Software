using System;
using System.Runtime.InteropServices;

namespace TaskSchedulerInterop;

[Guid("148BD524-A2AB-11CE-B11F-00AA00530503")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITask
{
	void CreateTrigger(out ushort NewTriggerIndex, [MarshalAs(UnmanagedType.Interface)] out ITaskTrigger Trigger);

	void DeleteTrigger([In] ushort TriggerIndex);

	void GetTriggerCount(out ushort Count);

	void GetTrigger([In] ushort TriggerIndex, [MarshalAs(UnmanagedType.Interface)] out ITaskTrigger Trigger);

	void GetTriggerString([In] ushort TriggerIndex, out IntPtr TriggerString);

	void GetRunTimes([In][MarshalAs(UnmanagedType.Struct)] ref SystemTime Begin, [In][MarshalAs(UnmanagedType.Struct)] ref SystemTime End, ref ushort Count, out IntPtr TaskTimes);

	void GetNextRunTime([In][Out][MarshalAs(UnmanagedType.Struct)] ref SystemTime NextRun);

	void SetIdleWait([In] ushort IdleMinutes, [In] ushort DeadlineMinutes);

	void GetIdleWait(out ushort IdleMinutes, out ushort DeadlineMinutes);

	void Run();

	void Terminate();

	void EditWorkItem([In] uint hParent, [In] uint dwReserved);

	void GetMostRecentRunTime([In][Out][MarshalAs(UnmanagedType.Struct)] ref SystemTime LastRun);

	void GetStatus([MarshalAs(UnmanagedType.Error)] out int Status);

	void GetExitCode(out uint ExitCode);

	void SetComment([In][MarshalAs(UnmanagedType.LPWStr)] string Comment);

	void GetComment(out IntPtr Comment);

	void SetCreator([In][MarshalAs(UnmanagedType.LPWStr)] string Creator);

	void GetCreator(out IntPtr Creator);

	void SetWorkItemData([In] ushort DataLen, [In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 0)] byte[] Data);

	void GetWorkItemData(out ushort DataLen, out IntPtr Data);

	void SetErrorRetryCount([In] ushort RetryCount);

	void GetErrorRetryCount(out ushort RetryCount);

	void SetErrorRetryInterval([In] ushort RetryInterval);

	void GetErrorRetryInterval(out ushort RetryInterval);

	void SetFlags([In] uint Flags);

	void GetFlags(out uint Flags);

	void SetAccountInformation([In][MarshalAs(UnmanagedType.LPWStr)] string AccountName, [In] IntPtr Password);

	void GetAccountInformation(out IntPtr AccountName);

	void SetApplicationName([In][MarshalAs(UnmanagedType.LPWStr)] string ApplicationName);

	void GetApplicationName(out IntPtr ApplicationName);

	void SetParameters([In][MarshalAs(UnmanagedType.LPWStr)] string Parameters);

	void GetParameters(out IntPtr Parameters);

	void SetWorkingDirectory([In][MarshalAs(UnmanagedType.LPWStr)] string WorkingDirectory);

	void GetWorkingDirectory(out IntPtr WorkingDirectory);

	void SetPriority([In] uint Priority);

	void GetPriority(out uint Priority);

	void SetTaskFlags([In] uint Flags);

	void GetTaskFlags(out uint Flags);

	void SetMaxRunTime([In] uint MaxRunTimeMS);

	void GetMaxRunTime(out uint MaxRunTimeMS);
}
