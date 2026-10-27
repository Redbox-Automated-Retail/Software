using System;
using System.IO;
using System.Threading;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class InventoryHelper : IDisposable
{
	private bool Disposed;

	private int completedSyncs;

	private readonly ManualResetEvent Event = new ManualResetEvent(initialState: false);

	private readonly HardwareService Service;

	private readonly ClientHelper Helper;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Helper.Dispose();
			Event.Close();
		}
	}

	internal void DumpInventoryToFile(string path)
	{
		HardwareCommandResult inventoryState = Service.GetInventoryState();
		if (!inventoryState.Errors.ContainsError())
		{
			File.WriteAllText(path, inventoryState.CommandMessages[0]);
		}
		else
		{
			inventoryState.Errors.Dump(Console.Out);
		}
	}

	internal bool ImportInventoryFromFile(string fileName)
	{
		if (!File.Exists(fileName))
		{
			Console.WriteLine("The file '{0}' doesn't exist.", fileName);
			return false;
		}
		try
		{
			return Service.SetInventoryState(File.ReadAllText(fileName)).Success;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			return false;
		}
	}

	internal void CompareSyncResults(HardwareJob syncJob, bool outputSuccessfulCompare)
	{
		if (!syncJob.GetResults(out var results).Success)
		{
			LogHelper.Instance.Log("Can't get job results for {0}", syncJob.ID);
			return;
		}
		int inventoryMatches = 0;
		int inventoryMismatches = 0;
		Array.ForEach(results, delegate(ProgramResult _r)
		{
			if (_r.Code == "InventoryState")
			{
				if (InventoryItemsEqual(_r, outputSuccessfulCompare))
				{
					int num = inventoryMatches + 1;
					inventoryMatches = num;
				}
				else
				{
					int num = inventoryMismatches + 1;
					inventoryMismatches = num;
				}
			}
		});
		LogHelper.Instance.Log("Analysis summary:");
		LogHelper.Instance.Log("  {0} matches.", inventoryMatches);
		LogHelper.Instance.Log("  {0} mismatches.", inventoryMismatches);
		if (inventoryMatches + inventoryMismatches != 717)
		{
			LogHelper.Instance.Log("Wrong analysis - expected 717, found {0}", inventoryMatches + inventoryMismatches);
		}
	}

	internal bool FixType(SyncType type)
	{
		return type switch
		{
			SyncType.Empty => Service.ServiceEmptySync().Success, 
			SyncType.Unknown => Service.ServiceUnknownSync().Success, 
			_ => false, 
		};
	}

	internal bool ScheduleFullSync()
	{
		return ScheduleFullSync(SyncOptions.None);
	}

	internal bool ScheduleFullSync(SyncOptions options)
	{
		LogHelper.Instance.Log("Schedule full sync with options: ");
		if (options == SyncOptions.None)
		{
			LogHelper.Instance.Log("  None");
		}
		else
		{
			foreach (SyncOptions value in Enum.GetValues(typeof(SyncOptions)))
			{
				if ((options & value) != SyncOptions.None)
				{
					LogHelper.Instance.Log("  {0}", value.ToString());
				}
			}
		}
		Thread thread = null;
		if ((options & SyncOptions.AsyncSoftSync) != SyncOptions.None)
		{
			thread = new Thread(RunSoftSync);
			thread.Start();
		}
		HardwareCommandResult hardwareCommandResult = Service.FullSync();
		if (!hardwareCommandResult.Success)
		{
			return false;
		}
		if ((options & SyncOptions.Attach) != SyncOptions.None)
		{
			string[] array = hardwareCommandResult.CommandMessages[0].Split('|');
			if (Service.GetJob(array[0], out var job).Success)
			{
				job.EventRaised += delegate(HardwareJob hardwareJob, DateTime eventTime, string eventMessage)
				{
					LogHelper.Instance.Log("{0}: {1}", eventTime, eventMessage);
				};
				Helper.WaitForJob(job, out var endStatus);
				LogHelper.Instance.Log("{0} Sync job {1} ended with status {2}", DateTime.Now, job.ID, endStatus.ToString());
				if ((options & SyncOptions.Analyze) != SyncOptions.None)
				{
					CompareSyncResults(job, (options & SyncOptions.OutputSuccessfulSyncCompare) != 0);
				}
				if (thread != null)
				{
					Event.Set();
					LogHelper.Instance.Log(" ** Completed {0} soft syncs ** ", completedSyncs);
					thread.Join(5000);
				}
			}
		}
		return hardwareCommandResult.Success;
	}

	internal bool RestoreInventory()
	{
		bool result = true;
		HardwareCommandResult hardwareCommandResult = Service.ScheduleJob("restore-inventory-from-backup", "Utilities restore", enableDebugging: false, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (hardwareCommandResult.Success)
		{
			Helper.WaitForJob(job, out var endStatus);
			Console.WriteLine("Restore job {0} ended with status {1}", job.ID, endStatus);
			if (HardwareJobStatus.Completed != endStatus)
			{
				Console.WriteLine("Restore job failed.");
				if (job.GetErrors(out var errors).Success)
				{
					errors.Dump(Console.Out);
				}
				result = false;
			}
			job.Trash();
		}
		else
		{
			Console.WriteLine("Failed to schedule job.");
			hardwareCommandResult.Errors.Dump(Console.Out);
			result = false;
		}
		return result;
	}

	internal bool RebuildInventory(bool force)
	{
		bool flag = true;
		using (HardwareStatusExecutor hardwareStatusExecutor = new HardwareStatusExecutor(Service))
		{
			hardwareStatusExecutor.Run();
			if (hardwareStatusExecutor.InventoryError | force)
			{
				using RebuildInventoryExecutor rebuildInventoryExecutor = new RebuildInventoryExecutor(Service);
				rebuildInventoryExecutor.Run();
				Console.WriteLine("Rebuild job {0} ended with status {1}", rebuildInventoryExecutor.ID, rebuildInventoryExecutor.EndStatus);
				flag = HardwareJobStatus.Completed == rebuildInventoryExecutor.EndStatus;
				if (!flag)
				{
					Console.WriteLine("Rebuild job failed.");
					rebuildInventoryExecutor.Errors.Dump(Console.Out);
				}
			}
		}
		return flag;
	}

	internal InventoryHelper(HardwareService service)
	{
		Service = service;
		Helper = new ClientHelper(service);
	}

	private void RunSoftSync()
	{
		HardwareJobSchedule schedule = new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.High
		};
		using ClientHelper clientHelper = new ClientHelper(Service);
		while (!Event.WaitOne(30000))
		{
			if (Service.SoftSync(schedule, out var job).Success)
			{
				job.Pend();
				clientHelper.WaitForJob(job, out var endStatus);
				if (HardwareJobStatus.Completed == endStatus)
				{
					completedSyncs++;
				}
				job.Trash();
			}
		}
	}

	private bool InventoryItemsEqual(ProgramResult result, bool logSuccess)
	{
		if (result.ItemID.ToString() == result.PreviousItem.ToString())
		{
			if (logSuccess)
			{
				LogHelper.Instance.Log("Sync: inventory at deck {0} slot {1} ID = {2} was the same", result.Deck, result.Slot, result.PreviousItem.ToString());
			}
			return true;
		}
		LogHelper.Instance.Log("Sync: inventory changed at deck {0} slot {1} ( went from {2} -> {3} )", result.Deck, result.Slot, result.PreviousItem.ToString(), result.ItemID.ToString());
		return false;
	}
}
