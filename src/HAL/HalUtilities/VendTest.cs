using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class VendTest : IDisposable
{
	private readonly AutoResetEvent Waiter = new AutoResetEvent(initialState: false);

	private readonly HardwareService Service;

	private readonly ClientHelper Helper;

	private readonly int Iterations;

	public void Dispose()
	{
		Helper.Dispose();
	}

	internal VendTest(int iter, HardwareService service)
	{
		Iterations = iter;
		Helper = new ClientHelper(service);
		Service = service;
	}

	internal void Execute(IConsole console, int? startAt)
	{
		if (startAt.HasValue && startAt.Value != -1)
		{
			new RedboxTimer("Vend Test", TimerFired).ScheduleAtNext(startAt.Value, 0);
			Waiter.WaitOne();
		}
		console.WriteLine("Starting test at {0} on {1}", DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString());
		List<Location> list = new List<Location>();
		for (int i = 0; i < Iterations; i++)
		{
			console.WriteLine("Starting iteration {0}", i);
			using RandomInventorySelector randomInventorySelector = new RandomInventorySelector(Service);
			IInventoryLocation inventoryLocation = null;
			do
			{
				inventoryLocation = randomInventorySelector.Select();
				if (inventoryLocation == null)
				{
					break;
				}
				list.Add(inventoryLocation.Location);
				if (!Service.Vend(list, new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.High
				}, out var job).Success)
				{
					Console.WriteLine("Failed to schedule vend job.");
					return;
				}
				job.Pend();
				Helper.WaitForJob(job, out var endStatus);
				list.Clear();
				if (endStatus != HardwareJobStatus.Completed)
				{
					console.WriteLine("Vend job {0} failed with status {1}.", job.ID, endStatus.ToString());
					return;
				}
				if (!job.GetResults(out var _).Success)
				{
					console.WriteLine("Failed to get program results.");
					return;
				}
			}
			while (inventoryLocation != null);
		}
	}

	private void TimerFired(object source, ElapsedEventArgs e)
	{
		Waiter.Set();
	}
}
