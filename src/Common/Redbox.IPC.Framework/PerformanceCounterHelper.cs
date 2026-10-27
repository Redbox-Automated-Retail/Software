using System;
using System.Diagnostics;
using Redbox.Core;

namespace Redbox.IPC.Framework;

public class PerformanceCounterHelper
{
	private string m_groupName;

	private volatile bool m_countersEnabled;

	public static PerformanceCounterHelper Instance => Singleton<PerformanceCounterHelper>.Instance;

	public void Initialize(string groupName)
	{
		m_countersEnabled = true;
		m_groupName = groupName;
	}

	public void IncrementCommandPerSecond(string command)
	{
		if (!m_countersEnabled)
		{
			return;
		}
		try
		{
			using (PerformanceCounter performanceCounter = new PerformanceCounter(m_groupName, FormatCountName(command), readOnly: false))
			{
				performanceCounter.Increment();
			}
			using PerformanceCounter performanceCounter2 = new PerformanceCounter(m_groupName, "Total Commands/second", readOnly: false);
			performanceCounter2.Increment();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception raised in IncrementCounter: ", e);
			m_countersEnabled = false;
		}
	}

	public void IncrementCommandExecutionTime(string command, long ticks)
	{
		if (!m_countersEnabled)
		{
			return;
		}
		try
		{
			using (PerformanceCounter performanceCounter = new PerformanceCounter(m_groupName, FormatExecutionTimeName(command), readOnly: false))
			{
				performanceCounter.IncrementBy(ticks);
			}
			using PerformanceCounter performanceCounter2 = new PerformanceCounter(m_groupName, FormatExecutionTimeBaseName(command), readOnly: false);
			performanceCounter2.IncrementBy(1L);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception raised in IncrementCommandExecutionTime: ", e);
			m_countersEnabled = false;
		}
	}

	public static string FormatCountName(string command)
	{
		return $"{command}/second";
	}

	public static string FormatCountDescription(string command)
	{
		return $"{command} calls per second";
	}

	public static string FormatExecutionTimeName(string command)
	{
		return $"{command} Execution Time (seconds)";
	}

	public static string FormatExecutionTimeDescription(string command)
	{
		return $"{command} execution time in seconds";
	}

	public static string FormatExecutionTimeBaseName(string command)
	{
		return $"{command} Execution Time Base";
	}

	public static string FormatExecutionTimeBaseDescription(string command)
	{
		return $"{command} execution time in seconds base";
	}

	private PerformanceCounterHelper()
	{
	}
}
