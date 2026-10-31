using System;
using System.Threading;
using Microsoft.Win32.TaskScheduler;

namespace KioskAdmin;

public static class DellBios
{
	public static void RunDellBios()
	{
		string appPath = "c:\\temp\\dellbios.exe";
		DateTime startTime = DateTime.Now.AddSeconds(65.0);
		SetSchedule("dellbios", appPath, startTime);
		WaitForTime(startTime.AddMinutes(5.0));
	}

	private static void SetSchedule(string taskName, string appPath, DateTime startTime)
	{
		try
		{
			using TaskService taskService = new TaskService(null);
			taskService.AddTask(taskName, new TimeTrigger(startTime), new ExecAction(appPath));
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	public static void WaitForTime(DateTime dt)
	{
		if (!(dt > DateTime.Now))
		{
			return;
		}
		TimeSpan timeout = dt - DateTime.Now;
		if (!(timeout.TotalMilliseconds <= 0.0))
		{
			using (ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false))
			{
				manualResetEvent.WaitOne(timeout);
				manualResetEvent.Close();
			}
		}
	}
}
