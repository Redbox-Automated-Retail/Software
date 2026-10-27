using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

public sealed class TableTypeFactory : ITableTypeFactory
{
	public ILocation NewLocation(int deck, int slot)
	{
		return new Location(deck, slot);
	}

	public ILocation NewLocation(int deck, int slot, string id, DateTime? returnTime, bool excluded, int stuck, MerchFlags flags)
	{
		return new Location(deck, slot)
		{
			ReturnDate = returnTime,
			Excluded = excluded,
			StuckCount = stuck,
			Flags = flags,
			ID = id
		};
	}

	public IPersistentCounter NewCounter(string name, int defaultValue)
	{
		return new PersistentCounter
		{
			Name = name,
			Value = defaultValue
		};
	}

	public IDumpBinInventoryItem NewBinItem(string barcode, DateTime timestamp)
	{
		return new DumpbinItem(barcode, timestamp);
	}

	public IPersistentOption NewPersistentOption(string name, string value)
	{
		return new PersistentOption
		{
			Key = name,
			Value = value
		};
	}

	public IHardwareCorrectionStatistic NewStatistic(HardwareCorrectionStatistic stat, string programName, bool correctionOk, DateTime timestamp)
	{
		return new HardwareCorrectionStat
		{
			Statistic = stat,
			ProgramName = programName,
			CorrectionOk = correctionOk,
			CorrectionTime = timestamp
		};
	}

	public IKioskFunctionCheckData NewCheckData(string verticalSlotTestResult, string initResult, string vendDoorTestResult, string trackTestResult, string snapDecodeTestResult, string touchscreenDriverTestResult, string cameraDriverTestResult, DateTime timestamp, string userIdentifier)
	{
		return new KioskFunctionCheckData
		{
			VerticalSlotTestResult = verticalSlotTestResult,
			InitTestResult = initResult,
			VendDoorTestResult = vendDoorTestResult,
			TrackTestResult = trackTestResult,
			SnapDecodeTestResult = snapDecodeTestResult,
			TouchscreenDriverTestResult = touchscreenDriverTestResult,
			CameraDriverTestResult = cameraDriverTestResult,
			Timestamp = timestamp,
			UserIdentifier = userIdentifier
		};
	}
}
