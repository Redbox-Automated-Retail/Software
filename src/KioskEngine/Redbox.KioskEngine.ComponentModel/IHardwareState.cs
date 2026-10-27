using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.HardwareServices;

namespace Redbox.KioskEngine.ComponentModel;

public interface IHardwareState
{
	int CurrentCameraGeneration { get; }

	int BarcodeDecoder { get; }

	int MaxDeck { get; }

	int MaxSlot { get; }

	AirExchangerStatus ExchangerStatus { get; }

	string AirExchangerFanStatus { get; }

	IEnumerable<Tuple<int, int>> ExcludedLocations { get; }

	bool VMZConfigured { get; }

	bool IsDenseMachine { get; }

	int BufferSlot { get; }

	int? QlmDeckAsNumber { get; }

	string QlmDeckAsString { get; }

	string MerchandizeType { get; }

	bool IsInitialized { get; }

	void Initialize();

	void Refresh();
}
