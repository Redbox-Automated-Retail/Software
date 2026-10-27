using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using TaskSchedulerInterop;

namespace TaskScheduler;

public class Task : IDisposable
{
	[Flags]
	public enum PropPages
	{
		Task = 1,
		Schedule = 2,
		Settings = 4
	}

	private ITask iTask;

	private string name;

	private TriggerList triggers;

	public string Name => name;

	public TriggerList Triggers
	{
		get
		{
			if (triggers == null)
			{
				triggers = new TriggerList(iTask);
			}
			return triggers;
		}
	}

	public string ApplicationName
	{
		get
		{
			iTask.GetApplicationName(out var ApplicationName);
			return CoTaskMem.LPWStrToString(ApplicationName);
		}
		set
		{
			iTask.SetApplicationName(value);
		}
	}

	public string AccountName
	{
		get
		{
			IntPtr AccountName = IntPtr.Zero;
			iTask.GetAccountInformation(out AccountName);
			return CoTaskMem.LPWStrToString(AccountName);
		}
	}

	public string Comment
	{
		get
		{
			iTask.GetComment(out var Comment);
			return CoTaskMem.LPWStrToString(Comment);
		}
		set
		{
			iTask.SetComment(value);
		}
	}

	public string Creator
	{
		get
		{
			iTask.GetCreator(out var Creator);
			return CoTaskMem.LPWStrToString(Creator);
		}
		set
		{
			iTask.SetCreator(value);
		}
	}

	private short ErrorRetryCount
	{
		get
		{
			iTask.GetErrorRetryCount(out var RetryCount);
			return (short)RetryCount;
		}
		set
		{
			iTask.SetErrorRetryCount((ushort)value);
		}
	}

	private short ErrorRetryInterval
	{
		get
		{
			iTask.GetErrorRetryInterval(out var RetryInterval);
			return (short)RetryInterval;
		}
		set
		{
			iTask.SetErrorRetryInterval((ushort)value);
		}
	}

	public int ExitCode
	{
		get
		{
			uint ExitCode = 0u;
			iTask.GetExitCode(out ExitCode);
			return (int)ExitCode;
		}
	}

	public TaskFlags Flags
	{
		get
		{
			iTask.GetFlags(out var Flags);
			return (TaskFlags)Flags;
		}
		set
		{
			iTask.SetFlags((uint)value);
		}
	}

	public short IdleWaitMinutes
	{
		get
		{
			iTask.GetIdleWait(out var IdleMinutes, out var _);
			return (short)IdleMinutes;
		}
		set
		{
			ushort deadlineMinutes = (ushort)IdleWaitDeadlineMinutes;
			iTask.SetIdleWait((ushort)value, deadlineMinutes);
		}
	}

	public short IdleWaitDeadlineMinutes
	{
		get
		{
			iTask.GetIdleWait(out var _, out var DeadlineMinutes);
			return (short)DeadlineMinutes;
		}
		set
		{
			ushort idleMinutes = (ushort)IdleWaitMinutes;
			iTask.SetIdleWait(idleMinutes, (ushort)value);
		}
	}

	public TimeSpan MaxRunTime
	{
		get
		{
			iTask.GetMaxRunTime(out var MaxRunTimeMS);
			return new TimeSpan((long)MaxRunTimeMS * 10000L);
		}
		set
		{
			double totalMilliseconds = value.TotalMilliseconds;
			if (totalMilliseconds >= 4294967295.0)
			{
				iTask.SetMaxRunTime(uint.MaxValue);
			}
			else
			{
				iTask.SetMaxRunTime((uint)totalMilliseconds);
			}
		}
	}

	public bool MaxRunTimeLimited
	{
		get
		{
			iTask.GetMaxRunTime(out var MaxRunTimeMS);
			return MaxRunTimeMS == uint.MaxValue;
		}
		set
		{
			if (value)
			{
				iTask.GetMaxRunTime(out var MaxRunTimeMS);
				if (MaxRunTimeMS == uint.MaxValue)
				{
					iTask.SetMaxRunTime(25920000u);
				}
			}
			else
			{
				iTask.SetMaxRunTime(uint.MaxValue);
			}
		}
	}

	public DateTime MostRecentRunTime
	{
		get
		{
			SystemTime LastRun = default(SystemTime);
			iTask.GetMostRecentRunTime(ref LastRun);
			if (LastRun.Year == 0)
			{
				return DateTime.MinValue;
			}
			return new DateTime(LastRun.Year, LastRun.Month, LastRun.Day, LastRun.Hour, LastRun.Minute, LastRun.Second, LastRun.Milliseconds);
		}
	}

	public DateTime NextRunTime
	{
		get
		{
			SystemTime NextRun = default(SystemTime);
			iTask.GetNextRunTime(ref NextRun);
			if (NextRun.Year == 0)
			{
				return DateTime.MinValue;
			}
			return new DateTime(NextRun.Year, NextRun.Month, NextRun.Day, NextRun.Hour, NextRun.Minute, NextRun.Second, NextRun.Milliseconds);
		}
	}

	public string Parameters
	{
		get
		{
			iTask.GetParameters(out var Parameters);
			return CoTaskMem.LPWStrToString(Parameters);
		}
		set
		{
			iTask.SetParameters(value);
		}
	}

	public ProcessPriorityClass Priority
	{
		get
		{
			iTask.GetPriority(out var Priority);
			return (ProcessPriorityClass)Priority;
		}
		set
		{
			if (value == ProcessPriorityClass.AboveNormal || value == ProcessPriorityClass.BelowNormal)
			{
				throw new ArgumentException("Unsupported Priority Level");
			}
			iTask.SetPriority((uint)value);
		}
	}

	public TaskStatus Status
	{
		get
		{
			iTask.GetStatus(out var Status);
			return (TaskStatus)Status;
		}
	}

	private int FlagsEx
	{
		get
		{
			iTask.GetTaskFlags(out var Flags);
			return (int)Flags;
		}
		set
		{
			iTask.SetTaskFlags((uint)value);
		}
	}

	public string WorkingDirectory
	{
		get
		{
			iTask.GetWorkingDirectory(out var WorkingDirectory);
			return CoTaskMem.LPWStrToString(WorkingDirectory);
		}
		set
		{
			iTask.SetWorkingDirectory(value);
		}
	}

	public bool Hidden
	{
		get
		{
			return (Flags & TaskFlags.Hidden) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= TaskFlags.Hidden;
			}
			else
			{
				Flags &= ~TaskFlags.Hidden;
			}
		}
	}

	public object Tag
	{
		get
		{
			iTask.GetWorkItemData(out var DataLen, out var Data);
			byte[] array = new byte[DataLen];
			Marshal.Copy(Data, array, 0, DataLen);
			MemoryStream serializationStream = new MemoryStream(array, writable: false);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			return binaryFormatter.Deserialize(serializationStream);
		}
		set
		{
			if (!value.GetType().IsSerializable)
			{
				throw new ArgumentException("Objects set as Data for Tasks must be serializable", "value");
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, value);
			iTask.SetWorkItemData((ushort)memoryStream.Length, memoryStream.GetBuffer());
		}
	}

	internal Task(ITask iTask, string taskName)
	{
		this.iTask = iTask;
		if (taskName.EndsWith(".job"))
		{
			name = taskName.Substring(0, taskName.Length - 4);
		}
		else
		{
			name = taskName;
		}
		triggers = null;
		Hidden = GetHiddenFileAttr();
	}

	private void SetHiddenFileAttr(bool set)
	{
		IPersistFile persistFile = (IPersistFile)iTask;
		persistFile.GetCurFile(out var ppszFileName);
		FileAttributes attributes = File.GetAttributes(ppszFileName);
		attributes = ((!set) ? (attributes & ~FileAttributes.Hidden) : (attributes | FileAttributes.Hidden));
		File.SetAttributes(ppszFileName, attributes);
	}

	private bool GetHiddenFileAttr()
	{
		IPersistFile persistFile = (IPersistFile)iTask;
		persistFile.GetCurFile(out var ppszFileName);
		try
		{
			FileAttributes attributes = File.GetAttributes(ppszFileName);
			return (attributes & FileAttributes.Hidden) != 0;
		}
		catch
		{
			return false;
		}
	}

	public DateTime NextRunTimeAfter(DateTime after)
	{
		after = after.AddSeconds(1.0);
		SystemTime Begin = new SystemTime
		{
			Year = (ushort)after.Year,
			Month = (ushort)after.Month,
			Day = (ushort)after.Day,
			DayOfWeek = (ushort)after.DayOfWeek,
			Hour = (ushort)after.Hour,
			Minute = (ushort)after.Minute,
			Second = (ushort)after.Second
		};
		SystemTime systemTime = default(SystemTime);
		systemTime = Begin;
		systemTime.Year = (ushort)DateTime.MaxValue.Year;
		systemTime.Month = 1;
		ushort Count = 1;
		iTask.GetRunTimes(ref Begin, ref systemTime, ref Count, out var TaskTimes);
		if (Count == 1)
		{
			SystemTime systemTime2 = default(SystemTime);
			systemTime2 = (SystemTime)Marshal.PtrToStructure(TaskTimes, typeof(SystemTime));
			Marshal.FreeCoTaskMem(TaskTimes);
			return new DateTime(systemTime2.Year, systemTime2.Month, systemTime2.Day, systemTime2.Hour, systemTime2.Minute, systemTime2.Second);
		}
		return DateTime.MinValue;
	}

	public void Run()
	{
		iTask.Run();
	}

	public void Save()
	{
		IPersistFile persistFile = (IPersistFile)iTask;
		persistFile.Save(null, fRemember: false);
		SetHiddenFileAttr(Hidden);
	}

	public void Save(string name)
	{
		IPersistFile persistFile = (IPersistFile)iTask;
		persistFile.GetCurFile(out var ppszFileName);
		string pszFileName = Path.GetDirectoryName(ppszFileName) + Path.DirectorySeparatorChar + name + Path.GetExtension(ppszFileName);
		persistFile.Save(pszFileName, fRemember: true);
		persistFile.SaveCompleted(pszFileName);
		this.name = name;
		SetHiddenFileAttr(Hidden);
	}

	public void Close()
	{
		if (triggers != null)
		{
			triggers.Dispose();
		}
		Marshal.ReleaseComObject(iTask);
		iTask = null;
	}

	public void DisplayForEdit()
	{
		iTask.EditWorkItem(0u, 0u);
	}

	public bool DisplayPropertySheet()
	{
		return DisplayPropertySheet(PropPages.Task | PropPages.Schedule | PropPages.Settings);
	}

	public bool DisplayPropertySheet(PropPages pages)
	{
		PropSheetHeader psh = default(PropSheetHeader);
		IProvideTaskPage provideTaskPage = (IProvideTaskPage)iTask;
		IntPtr[] array = new IntPtr[3];
		int num = 0;
		IntPtr phPage;
		if ((pages & PropPages.Task) != 0)
		{
			provideTaskPage.GetPage(0, fPersistChanges: false, out phPage);
			array[num++] = phPage;
		}
		if ((pages & PropPages.Schedule) != 0)
		{
			provideTaskPage.GetPage(1, fPersistChanges: false, out phPage);
			array[num++] = phPage;
		}
		if ((pages & PropPages.Settings) != 0)
		{
			provideTaskPage.GetPage(2, fPersistChanges: false, out phPage);
			array[num++] = phPage;
		}
		if (num == 0)
		{
			throw new ArgumentException("No Property Pages to display");
		}
		psh.dwSize = (uint)Marshal.SizeOf((object)psh);
		psh.dwFlags = 128u;
		psh.pszCaption = Name;
		psh.nPages = (uint)num;
		GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		psh.phpage = gCHandle.AddrOfPinnedObject();
		int num2 = PropertySheetDisplay.PropertySheet(ref psh);
		gCHandle.Free();
		if (num2 < 0)
		{
			throw new Exception("Property Sheet failed to display");
		}
		return num2 > 0;
	}

	public void SetAccountInformation(string accountName, string password)
	{
		IntPtr intPtr = Marshal.StringToCoTaskMemUni(password);
		iTask.SetAccountInformation(accountName, intPtr);
		Marshal.FreeCoTaskMem(intPtr);
	}

	public void SetAccountInformation(string accountName, SecureString password)
	{
		IntPtr intPtr = Marshal.SecureStringToCoTaskMemUnicode(password);
		iTask.SetAccountInformation(accountName, intPtr);
		Marshal.ZeroFreeCoTaskMemUnicode(intPtr);
	}

	public void Terminate()
	{
		iTask.Terminate();
	}

	public override string ToString()
	{
		return $"{name} (\"{ApplicationName}\" {Parameters})";
	}

	public void Dispose()
	{
		Close();
	}
}
