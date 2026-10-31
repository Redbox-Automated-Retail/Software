using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.Core;

public static class TimeoutHelper
{
	private const int TimerInterval = 25;

	private static readonly Timer m_timer;

	private static volatile bool m_isProcessing;

	private static readonly object m_lockObject;

	private static readonly List<TimeoutEntry> TimeoutEntries;

	public static void AddSession(TimeoutEntry session)
	{
		lock (typeof(TimeoutHelper))
		{
			TimeoutEntries.Add(session);
		}
	}

	public static void RemoveSession(ITimeoutSink timeOutSink)
	{
		lock (typeof(TimeoutHelper))
		{
			TimeoutEntry timeoutEntry = TimeoutEntries.Find((TimeoutEntry each) => each.TimeoutSink == timeOutSink);
			if (timeoutEntry != null)
			{
				TimeoutEntries.Remove(timeoutEntry);
			}
		}
	}

	static TimeoutHelper()
	{
		m_lockObject = new object();
		TimeoutEntries = new List<TimeoutEntry>();
		m_timer = new Timer(delegate
		{
			if (m_isProcessing)
			{
				return;
			}
			try
			{
				m_isProcessing = true;
				lock (m_lockObject)
				{
					DateTime now = DateTime.Now;
					for (int i = 0; i < TimeoutEntries.Count; i++)
					{
						TimeoutEntry timeoutEntry = TimeoutEntries[i];
						if (!(now < timeoutEntry.Timeout))
						{
							TimeoutEntries.RemoveAt(i);
							timeoutEntry.TimeoutSink.RaiseTimeout();
						}
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unhandled exception raised in TimeoutHelper.", e);
			}
			finally
			{
				m_isProcessing = false;
			}
		}, null, 25, 25);
	}
}
