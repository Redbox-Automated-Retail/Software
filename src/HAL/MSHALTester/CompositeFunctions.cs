using System;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.MSHALTester;

public static class CompositeFunctions
{
	public static void VendDisk(HardwareService service, OutputBox box)
	{
		using VendDiskInPickerJob vendDiskInPickerJob = new VendDiskInPickerJob(service);
		vendDiskInPickerJob.Run();
		foreach (ProgramResult result in vendDiskInPickerJob.Results)
		{
			box.Write(result.Code);
			if (result.Code.Equals("ItemVended", StringComparison.CurrentCultureIgnoreCase))
			{
				box.Write("Disk was taken.");
				return;
			}
			if (result.Code.Equals("HardwareError", StringComparison.CurrentCultureIgnoreCase))
			{
				box.Write(result.Message);
			}
		}
		box.Write("Disk not taken - pulled back in.");
	}

	public static bool GetItem(int deck, int slot, OutputBox box, HardwareService service)
	{
		using GetAndCenterResult getAndCenterResult = new GetAndCenterResult(service, deck, slot, center: false);
		getAndCenterResult.Run();
		int errors = 0;
		getAndCenterResult.Results.ForEach(delegate(ProgramResult result)
		{
			if (result.Code == "ErrorMessage")
			{
				int num = errors + 1;
				errors = num;
			}
			box.Write(result.Message);
		});
		box.Write("GetDVD ended with status {0}", getAndCenterResult.EndStatus.ToString());
		return errors == 0;
	}

	public static bool PutItem(HardwareService service, int deck, int slot, OutputBox box)
	{
		using PutInLocationResult putInLocationResult = new PutInLocationResult(service, deck, slot);
		putInLocationResult.Run();
		int num = 0;
		foreach (ProgramResult result in putInLocationResult.Results)
		{
			if (result.Code == "JobError")
			{
				num++;
			}
			box.Write(result.Message);
		}
		return num == 0;
	}
}
