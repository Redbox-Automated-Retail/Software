using System;
using System.Collections.Generic;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities.KioskTest;

internal abstract class AbstractKioskTest : IDisposable
{
	protected readonly KioskConfiguration Configuration;

	protected readonly HardwareService Service;

	internal bool RewriteDataOnSuccess;

	internal int VendFrequency = 5;

	private int Transfers;

	private bool RunOneDiskQuickTest;

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			OnDispose(fromDispose: true);
		}
	}

	protected abstract JobExecutor GetTransferExecutor(TestLocation start, TestLocation end, bool vend, int iteration);

	protected abstract bool CanVisit(TestLocation loc);

	protected abstract int OnPreTest();

	protected abstract int OnCleanup(TestLocation diskSource);

	protected virtual void OnDispose(bool fromDispose)
	{
	}

	protected abstract void OnProcessArgument(string argument);

	protected abstract bool SingleTestNeeded();

	protected bool UpdateGamp()
	{
		if (RewriteDataOnSuccess)
		{
			HardwareCommandResult hardwareCommandResult = Service.ExecuteServiceCommand("SERVICE update-gamp backup-directory: @'c:\\gamp\\PowerhouseBackup'");
			LogHelper.Instance.Log("Update gamp file data: {0}.", hardwareCommandResult.Success ? "OK" : "EPIC FAIL");
			return hardwareCommandResult.Success;
		}
		return true;
	}

	protected AbstractKioskTest(HardwareService service, KioskConfiguration config)
	{
		Service = service;
		Configuration = config;
	}

	internal void AcceptArgument(string argument)
	{
		if (argument.StartsWith("--vend"))
		{
			VendFrequency = CommandLineOption.GetOptionVal(argument, VendFrequency);
		}
		else if (argument.StartsWith("--rewriteDataOnSuccess"))
		{
			RewriteDataOnSuccess = CommandLineOption.GetOptionVal(argument, RewriteDataOnSuccess);
		}
		else if (argument.StartsWith("--oneDiskQuickTest"))
		{
			RunOneDiskQuickTest = CommandLineOption.GetOptionVal(argument, RunOneDiskQuickTest);
		}
		else
		{
			OnProcessArgument(argument);
		}
	}

	internal bool SingleTestOnly()
	{
		if (!RunOneDiskQuickTest)
		{
			return SingleTestNeeded();
		}
		return true;
	}

	internal int Run(TimeSpan? runTime, TestLocation diskSource, MachineInformation config)
	{
		DateTime? dateTime = null;
		if (runTime.HasValue)
		{
			dateTime = DateTime.Now.Add(runTime.Value);
			LogHelper.Instance.Log("Kiosk Test Starting - will run until {0}. Deck details:", dateTime);
		}
		else
		{
			LogHelper.Instance.Log("Kiosk Test Starting - Deck details:");
		}
		LogHelper.Instance.Log("-------------------------------------");
		LogHelper.Instance.Log(" Machine model - {0}", config.Configuration);
		config.DecksConfiguration.ForEach(delegate(IDeckConfig dc)
		{
			LogHelper.Instance.Log("  Deck {0} - Number of Slots {1}", dc.Number, dc.SlotCount);
		});
		LogHelper.Instance.Log("-------------------------------------");
		List<TestLocation> list = PopulateLocations(diskSource, config);
		TestLocation testLocation = diskSource;
		while (list.Count > 0)
		{
			bool flag = VendFrequency > 0 && Transfers % VendFrequency == 0;
			int index = new Random(DateTime.Now.Millisecond).Next(0, list.Count - 1);
			TestLocation testLocation2 = list[index];
			list.RemoveAt(index);
			LogHelper.Instance.Log("## Move Disc From Deck {0}, Slot {1} to Deck {2}, Slot {3} - Vend {4} ##", testLocation.Deck, testLocation.Slot, testLocation2.Deck, testLocation2.Slot, flag);
			if (!RunTransfer(testLocation, testLocation2, flag))
			{
				LogHelper.Instance.Log("Transfer job failed.");
				Environment.Exit(-1);
			}
			testLocation = testLocation2;
			if (dateTime.HasValue)
			{
				LogHelper.Instance.Log("## Time Remaining {0} ##", dateTime - DateTime.Now);
				DateTime now = DateTime.Now;
				DateTime? dateTime2 = dateTime;
				if (now > dateTime2)
				{
					break;
				}
			}
		}
		RunTransfer(testLocation, diskSource, vend: false);
		LogHelper.Instance.Log("## TestComplete - {0} total transfers ##", Transfers);
		OnCleanup(diskSource);
		UpdateGamp();
		LogHelper.Instance.Log("## Kiosk Test Completed ##");
		return 0;
	}

	internal int RunSingleTests(TestLocation start, MachineInformation c)
	{
		if (RunOneDiskQuickTest)
		{
			if (!RunQuickTest(start, c) || !UpdateGamp())
			{
				return -3;
			}
			return 0;
		}
		return OnPreTest();
	}

	private bool RunTransfer(TestLocation start, TestLocation end, bool vend)
	{
		using JobExecutor jobExecutor = GetTransferExecutor(start, end, vend, Transfers);
		jobExecutor.Run();
		if (HardwareJobStatus.Completed == jobExecutor.EndStatus)
		{
			Transfers++;
			return true;
		}
		LogHelper.Instance.Log("**ERROR** Kiosk test job {0} failed with status {1}", jobExecutor.ID, jobExecutor.EndStatus.ToString());
		return false;
	}

	private bool RunQuickTest(TestLocation source, MachineInformation config)
	{
		LogHelper.Instance.Log(" ** scheduling one disk quick deck test ** ");
		int count = config.DecksConfiguration.Count;
		if (config.Configuration != KioskConfiguration.R717)
		{
			count = config.DecksConfiguration.Count - 1;
		}
		using OneDiskQuickTest oneDiskQuickTest = new OneDiskQuickTest(Service, source.Deck, source.Slot, count);
		oneDiskQuickTest.Run();
		LogHelper.Instance.Log(" OneDiskQuickTest ended with status {0}", oneDiskQuickTest.EndStatus);
		return oneDiskQuickTest.EndStatus == HardwareJobStatus.Completed;
	}

	private List<TestLocation> PopulateLocations(TestLocation source, MachineInformation config)
	{
		List<TestLocation> locations = new List<TestLocation>();
		config.DecksConfiguration.ForEach(delegate(IDeckConfig dc)
		{
			if (!dc.IsQlm)
			{
				for (int i = 1; i <= dc.SlotCount; i++)
				{
					if (dc.Number != source.Deck || i != source.Slot)
					{
						TestLocation testLocation = new TestLocation(dc.Number, i);
						if (CanVisit(testLocation))
						{
							locations.Add(testLocation);
						}
					}
				}
			}
		});
		return locations;
	}
}
