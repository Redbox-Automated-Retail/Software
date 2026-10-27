using System;
using System.Diagnostics;

namespace Redbox.HAL.Component.Model.Timers;

public sealed class ExecutionTimer : IDisposable
{
	private readonly Stopwatch Stopwatch;

	public TimeSpan Elapsed => Stopwatch.Elapsed;

	public long ElapsedTicks => Stopwatch.ElapsedTicks;

	public long ElapsedMilliseconds => Stopwatch.ElapsedMilliseconds;

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
