using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class TimerFunctions
{
	[KernelFunction(Name = "kernel.timerwait")]
	internal static void TimerWait(int milliseconds)
	{
		ServiceLocator.Instance.GetService<ITimerService>().Wait(milliseconds);
	}

	[KernelFunction(Name = "kernel.timercreate")]
	internal static void CreateTimer(string name, object dueTime, object period, TimerFiredHandler timerHandler)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int? dueTime2 = null;
		if (dueTime != null)
		{
			dueTime2 = Convert.ToInt32(dueTime);
		}
		int? period2 = null;
		if (period != null)
		{
			period2 = Convert.ToInt32(period);
		}
		service.CreateTimer(name, dueTime2, period2, delegate
		{
			timerHandler();
		});
	}

	[KernelFunction(Name = "kernel.timerstop")]
	internal static void StopTimer(string name)
	{
		ServiceLocator.Instance.GetService<ITimerService>().GetTimer(name)?.Stop();
	}

	[KernelFunction(Name = "kernel.timerstart")]
	internal static void StartTimer(string name)
	{
		ServiceLocator.Instance.GetService<ITimerService>().GetTimer(name)?.Start();
	}

	[KernelFunction(Name = "kernel.timerstartall")]
	internal static void StartAllTimers()
	{
		ServiceLocator.Instance.GetService<ITimerService>().StartAll();
	}

	[KernelFunction(Name = "kernel.timerstopall")]
	internal static void StopAllTimers()
	{
		ServiceLocator.Instance.GetService<ITimerService>().StopAll();
	}

	[KernelFunction(Name = "kernel.timerremove")]
	internal static void RemoveTimer(string name)
	{
		ServiceLocator.Instance.GetService<ITimerService>().RemoveTimer(name);
	}

	[KernelFunction(Name = "kernel.timerexists")]
	internal static bool TimerExists(string name)
	{
		return ServiceLocator.Instance.GetService<ITimerService>().GetTimer(name) != null;
	}

	[KernelFunction(Name = "kernel.gettimerperiod")]
	internal static int? GetTimerPeriod(string name)
	{
		int? result = 0;
		ITimer timer = ServiceLocator.Instance.GetService<ITimerService>().GetTimer(name);
		if (timer != null)
		{
			return timer.Period;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.set_timer_period_and_duetime")]
	internal static void GetTimerPeriod(string timerName, int? period, int? dueTime)
	{
		ITimer timer = ServiceLocator.Instance.GetService<ITimerService>().GetTimer(timerName);
		if (timer != null)
		{
			timer.Period = period;
			timer.DueTime = dueTime;
		}
	}

	internal static void Reset()
	{
		ServiceLocator.Instance.GetService<ITimerService>().Reset();
	}
}
