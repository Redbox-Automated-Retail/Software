using System;
using Redbox.Core;

namespace Redbox.IPC.Framework;

public class Statistics
{
	public long NumberOfBytesSent;

	public DateTime ServerStartTime;

	public long NumberOfBytesReceived;

	public long NumberOfCommandsExecuted;

	public long NumberServerBusyResponses;

	public TimeSpan TotalCommandExecutionTime;

	public TimeSpan MinimumCommandExecutionTime;

	public TimeSpan MaximumCommandExecutionTime;

	private readonly object m_syncObject = new object();

	public static Statistics Instance => Singleton<Statistics>.Instance;

	public string Host => Environment.MachineName;

	public TimeSpan ServerUpTime => DateTime.Now - ServerStartTime;

	public long AverageNumberOfBytesSent
	{
		get
		{
			long result = 0L;
			if (NumberOfCommandsExecuted > 0 && NumberOfBytesSent > 0)
			{
				result = NumberOfBytesSent / NumberOfCommandsExecuted;
			}
			return result;
		}
	}

	public long AverageNumberOfBytesReceived
	{
		get
		{
			long result = 0L;
			if (NumberOfCommandsExecuted > 0 && NumberOfBytesReceived > 0)
			{
				result = NumberOfBytesReceived / NumberOfCommandsExecuted;
			}
			return result;
		}
	}

	public TimeSpan AverageCommandExecutionTime
	{
		get
		{
			long ticks = 0L;
			if (NumberOfCommandsExecuted > 0)
			{
				ticks = TotalCommandExecutionTime.Ticks / NumberOfCommandsExecuted;
			}
			return new TimeSpan(ticks);
		}
	}

	public void TrackCommandStatistics(TimeSpan executionTime)
	{
		lock (m_syncObject)
		{
			NumberOfCommandsExecuted++;
			TotalCommandExecutionTime = TotalCommandExecutionTime.Add(executionTime);
			if (executionTime < MinimumCommandExecutionTime)
			{
				MinimumCommandExecutionTime = executionTime;
			}
			else if (executionTime > MaximumCommandExecutionTime)
			{
				MaximumCommandExecutionTime = executionTime;
			}
		}
	}

	private Statistics()
	{
	}
}
