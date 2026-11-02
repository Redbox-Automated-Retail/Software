using System;
using System.Runtime.InteropServices;
using TaskSchedulerInterop;

namespace TaskScheduler;

public class ScheduledTasks : IDisposable
{
	private ITaskScheduler its; // released and cleared by Dispose; using the object afterwards throws, as before

	internal static Guid ITaskGuid;

	internal static Guid CTaskGuid;

	public ScheduledTasks(string? computer)
		: this()
	{
		its.SetTargetComputer(computer);
	}

	public ScheduledTasks()
	{
		CTaskScheduler cTaskScheduler = new CTaskScheduler();
		its = (ITaskScheduler)cTaskScheduler;
	}

	private string[] GrowStringArray(string[] s, uint n)
	{
		string[] grown = new string[s.Length + n];
		for (int i = 0; i < s.Length; i++)
		{
			grown[i] = s[i];
		}
		return grown;
	}

	public string[] GetTaskNames()
	{
		string[] names = new string[0];
		int count = 0;
		its.Enum(out var EnumWorkItems);
		IntPtr Names;
		uint Fetched;
		while (EnumWorkItems.Next(10u, out Names, out Fetched) >= 0 && Fetched != 0)
		{
			names = GrowStringArray(names, Fetched);
			while (Fetched != 0)
			{
				IntPtr ptr = Marshal.ReadIntPtr(Names, (int)(--Fetched) * IntPtr.Size);
				names[count++] = Marshal.PtrToStringUni(ptr)!; // the enumerator returns allocated names
				Marshal.FreeCoTaskMem(ptr);
			}
			Marshal.FreeCoTaskMem(Names);
		}
		return names;
	}

	public Task? CreateTask(string name)
	{
		Task? task = OpenTask(name);
		if (task != null)
		{
			task.Close();
			throw new ArgumentException("The task \"" + name + "\" already exists.");
		}
		try
		{
			its.NewWorkItem(name, ref CTaskGuid, ref ITaskGuid, out var workItem);
			ITask iTask = (ITask)workItem;
			return new Task(iTask, name);
		}
		catch
		{
			return null;
		}
	}

	public bool DeleteTask(string name)
	{
		try
		{
			its.Delete(name);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public Task? OpenTask(string name)
	{
		try
		{
			its.Activate(name, ref ITaskGuid, out var workItem);
			ITask iTask = (ITask)workItem;
			return new Task(iTask, name);
		}
		catch
		{
			return null;
		}
	}

	public void Dispose()
	{
		Marshal.ReleaseComObject(its);
		its = null!;
	}

	static ScheduledTasks()
	{
		ITaskGuid = Marshal.GenerateGuidForType(typeof(ITask));
		CTaskGuid = Marshal.GenerateGuidForType(typeof(CTask));
	}
}
