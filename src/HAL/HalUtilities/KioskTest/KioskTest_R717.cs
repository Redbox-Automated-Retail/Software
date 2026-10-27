using System.Collections.Generic;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities.KioskTest;

internal sealed class KioskTest_R717 : AbstractKioskTest
{
	private bool RunDiskAroundBinTest;

	private bool ExcludeSlotsAroundDumpBin;

	protected override JobExecutor GetTransferExecutor(TestLocation start, TestLocation end, bool vend, int notUsed)
	{
		return new KioskTestExecutor(Service, start, end, vend);
	}

	protected override bool CanVisit(TestLocation tl)
	{
		if (tl.Deck != 8)
		{
			return true;
		}
		if (tl.Slot == 82 || tl.Slot == 83 || tl.Slot == 84)
		{
			return false;
		}
		if (ExcludeSlotsAroundDumpBin && (tl.Slot == 81 || tl.Slot == 85))
		{
			return false;
		}
		return true;
	}

	protected override int OnPreTest()
	{
		if (RunDiskAroundBinTest)
		{
			LogHelper.Instance.Log(" ** scheduling disk adjacent to bin test ** ");
			using AdjacentBinTest adjacentBinTest = new AdjacentBinTest(Service);
			adjacentBinTest.Run();
			LogHelper.Instance.Log(" AdjacentBinTest job ended with status {0}", adjacentBinTest.EndStatus);
			return (adjacentBinTest.EndStatus != HardwareJobStatus.Completed) ? (-3) : 0;
		}
		return 0;
	}

	protected override int OnCleanup(TestLocation diskSource)
	{
		LogHelper.Instance.Log("## Schedule load bin job ##");
		List<Location> list = new List<Location>();
		list.Add(new Location
		{
			Deck = diskSource.Deck,
			Slot = diskSource.Slot
		});
		if (!ScheduleAndBlockLoadBinJob(list))
		{
			return -9;
		}
		return 0;
	}

	protected override void OnProcessArgument(string argument)
	{
		if (argument.StartsWith("--excludeAroundDump"))
		{
			ExcludeSlotsAroundDumpBin = CommandLineOption.GetOptionVal(argument, ExcludeSlotsAroundDumpBin);
		}
		else if (argument.StartsWith("--testDisksAroundBin"))
		{
			RunDiskAroundBinTest = CommandLineOption.GetOptionVal(argument, RunDiskAroundBinTest);
		}
	}

	protected override bool SingleTestNeeded()
	{
		return RunDiskAroundBinTest;
	}

	internal KioskTest_R717(HardwareService s)
		: base(s, KioskConfiguration.R717)
	{
	}

	private bool ScheduleAndBlockLoadBinJob(List<Location> locs)
	{
		using ClientHelper clientHelper = new ClientHelper(Service);
		HardwareJob job = null;
		try
		{
			if (!Service.LoadBin(locs, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.High
			}, out job).Success)
			{
				LogHelper.Instance.Log("**ERROR** Failed to create bin job");
				return false;
			}
			if (!clientHelper.WaitForJob(job, out var endStatus))
			{
				LogHelper.Instance.Log("**ERROR** Wait for load bin job failed");
				return false;
			}
			return endStatus == HardwareJobStatus.Completed;
		}
		finally
		{
			job?.Trash();
		}
	}
}
