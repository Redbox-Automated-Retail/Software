using System;
using System.Diagnostics;

namespace Redbox.HAL.Core;

public class ExecutionTimer : IDisposable
{
	public TimeSpan Elapsed => Stopwatch.Elapsed;

	public long ElapsedTicks => Stopwatch.ElapsedTicks;

	public long ElapsedMilliseconds => Stopwatch.ElapsedMilliseconds;

	private Stopwatch Stopwatch { get; set; }

	public ExecutionTimer()
	{
		Stopwatch = new Stopwatch();
		Stopwatch.Start();
	}

	public void Stop()
	{
		Stopwatch.Stop();
	}

	public void Start()
	{
		Stopwatch.Start();
	}

	public void StartNew()
	{
		Stopwatch.StartNew();
	}

	public void Dispose()
	{
		if (Stopwatch.IsRunning)
		{
			Stopwatch.Stop();
		}
	}
}
