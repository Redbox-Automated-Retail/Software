using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002ETimeoutHelper
{
	private const int TimerInterval = 25;

	private static readonly Timer m_timer;

	private static volatile bool m_isProcessing;

	private static readonly object m_lockObject;

	private static readonly List<Redbox_002EMacros1309_002ETimeoutEntry> TimeoutEntries;

	public static void AddSession(Redbox_002EMacros1309_002ETimeoutEntry session)
	{
		lock (typeof(Redbox_002EMacros1309_002ETimeoutHelper))
		{
			TimeoutEntries.Add(session);
		}
	}

	public static void RemoveSession(Redbox_002EMacros1309_002EITimeoutSink timeOutSink)
	{
		lock (typeof(Redbox_002EMacros1309_002ETimeoutHelper))
		{
			Redbox_002EMacros1309_002ETimeoutEntry entry = TimeoutEntries.Find((Redbox_002EMacros1309_002ETimeoutEntry each) => each.TimeoutSink == timeOutSink);
			if (entry != null)
			{
				TimeoutEntries.Remove(entry);
			}
		}
	}

	static Redbox_002EMacros1309_002ETimeoutHelper()
	{
		m_lockObject = new object();
		TimeoutEntries = new List<Redbox_002EMacros1309_002ETimeoutEntry>();
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
						Redbox_002EMacros1309_002ETimeoutEntry entry = TimeoutEntries[i];
						if (!(now < entry.Timeout))
						{
							TimeoutEntries.RemoveAt(i);
							entry.TimeoutSink.RaiseTimeout();
						}
					}
				}
			}
			catch (Exception e)
			{
				Redbox_002EMacros1309_002ELogHelper.Instance.Log("Unhandled exception raised in TimeoutHelper.", e);
			}
			finally
			{
				m_isProcessing = false;
			}
		}, null, 25, 25);
	}
}
