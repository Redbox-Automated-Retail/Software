using System.Collections.Generic;
using System.Diagnostics;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class StopwatchFunctions
{
	private static readonly IDictionary<string, Stopwatch> m_stopwatches = new Dictionary<string, Stopwatch>();

	[KernelFunction(Name = "kernel.stopwatchcreate")]
	internal static void CreateStopwatch(string name)
	{
		if (!m_stopwatches.ContainsKey(name))
		{
			m_stopwatches[name] = new Stopwatch();
		}
	}

	[KernelFunction(Name = "kernel.stopwatchremove")]
	internal static void RemoveStopwatch(string name)
	{
		if (m_stopwatches.ContainsKey(name))
		{
			m_stopwatches[name].Stop();
			m_stopwatches.Remove(name);
		}
	}

	[KernelFunction(Name = "kernel.stopwatchstart")]
	internal static void StartStopwatch(string name)
	{
		if (m_stopwatches.ContainsKey(name))
		{
			m_stopwatches[name].Start();
		}
	}

	[KernelFunction(Name = "kernel.stopwatchstop")]
	internal static void StopStopwatch(string name)
	{
		if (m_stopwatches.ContainsKey(name))
		{
			m_stopwatches[name].Stop();
		}
	}

	[KernelFunction(Name = "kernel.stopwatchisrunning")]
	internal static bool IsStopwatchRunning(string name)
	{
		if (m_stopwatches.ContainsKey(name))
		{
			return m_stopwatches[name].IsRunning;
		}
		return false;
	}

	[KernelFunction(Name = "kernel.stopwatchgetmilliseconds")]
	internal static long GetMilliseconds(string name)
	{
		if (!m_stopwatches.ContainsKey(name))
		{
			return 0L;
		}
		return m_stopwatches[name].ElapsedMilliseconds;
	}

	[KernelFunction(Name = "kernel.stopwatchformat")]
	internal static string GetFormattedTimespan(string name)
	{
		if (!m_stopwatches.ContainsKey(name))
		{
			return null;
		}
		return m_stopwatches[name].Elapsed.ToString();
	}
}
