using System;

namespace Redbox.HAL.Component.Model;

public interface ITableTypeFactory
{
	ILocation NewLocation(int deck, int slot);

	ILocation NewLocation(int deck, int slot, string id, DateTime? returnTime, bool excluded, int stuck, MerchFlags flags);

	IPersistentCounter NewCounter(string name, int defaultValue);

	IDumpBinInventoryItem NewBinItem(string barcode, DateTime timestamp);

	IPersistentOption NewPersistentOption(string name, string value);

	IHardwareCorrectionStatistic NewStatistic(HardwareCorrectionStatistic stat, string programName, bool correctionOk, DateTime timestamp);

	IKioskFunctionCheckData NewCheckData(string verticalSlotTestResult, string initResult, string vendDoorTestResult, string trackTestResult, string snapDecodeTestResult, string touchscreenDriverTestResult, string cameraDriverTestResult, DateTime timestamp, string userIdentifier);
}
