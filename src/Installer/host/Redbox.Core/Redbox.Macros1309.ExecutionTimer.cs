using System;
using System.Diagnostics;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002EExecutionTimer : IDisposable
{
	public TimeSpan Elapsed => Stopwatch.Elapsed;

	public long ElapsedTicks => Stopwatch.ElapsedTicks;

	public long ElapsedMilliseconds => Stopwatch.ElapsedMilliseconds;

	private Stopwatch Stopwatch { get; set; }

	public Redbox_002EMacros1309_002EExecutionTimer()
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
