using System;
using System.Runtime.InteropServices;

namespace Redbox.Core;

public class ThreadExecutionTimer
{
	private long? m_endTimeStamp;

	private long m_startTimeStamp;

	public TimeSpan Elapsed
	{
		get
		{
			long num = m_endTimeStamp ?? GetThreadTimes();
			long num2 = num - m_startTimeStamp;
			return TimeSpan.FromMilliseconds(num2 / 10000);
		}
	}

	public bool IsRunning { get; private set; }

	public void Start()
	{
		IsRunning = true;
		m_startTimeStamp = GetThreadTimes();
	}

	public void Stop()
	{
		IsRunning = false;
		m_endTimeStamp = GetThreadTimes();
	}

	public void Reset()
	{
		m_startTimeStamp = 0L;
		m_endTimeStamp = null;
	}

	[DllImport("kernel32.dll")]
	private static extern long GetThreadTimes(IntPtr threadHandle, out long createionTime, out long exitTime, out long kernelTime, out long userTime);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetCurrentThread();

	private static long GetThreadTimes()
	{
		IntPtr currentThread = GetCurrentThread();
		long threadTimes = GetThreadTimes(currentThread, out long createionTime, out createionTime, out var kernelTime, out var userTime);
		if (!Convert.ToBoolean(threadTimes))
		{
			throw new Exception($"failed to get timestamp. error code: {threadTimes}");
		}
		return kernelTime + userTime;
	}
}
