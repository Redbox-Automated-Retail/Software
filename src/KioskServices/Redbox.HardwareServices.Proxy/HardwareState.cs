using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.HardwareServices;

namespace Redbox.HardwareServices.Proxy;

public class HardwareState : IHardwareState
{
	private CameraGeneration _cameraGeneration;

	private BarcodeServices _barcodeServices;

	private Redbox.HAL.Component.Model.AirExchangerStatus _airExchangerStatus;

	private ExchangerFanStatus _exchangerFanStatus;

	private string _doorSensorStatus;

	private List<Location> _excludedSlots = new List<Location>();

	private int _maxDeck;

	private int _maxSlot;

	private bool _vmzConfigured;

	private bool _dense;

	private int _bufferSlot = 1;

	private int? _qlmDeckAsNumber;

	private string _qlmDeckAsString;

	private string _merchType = "QLM";

	private int _isRunning;

	public int CurrentCameraGeneration
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return (int)_cameraGeneration;
		}
	}

	public int BarcodeDecoder
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return (int)_barcodeServices;
		}
	}

	public Redbox.KioskEngine.ComponentModel.HardwareServices.AirExchangerStatus ExchangerStatus
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return (Redbox.KioskEngine.ComponentModel.HardwareServices.AirExchangerStatus)_airExchangerStatus;
		}
	}

	public string AirExchangerFanStatus
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _exchangerFanStatus.ToString();
		}
	}

	public IEnumerable<Tuple<int, int>> ExcludedLocations
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _excludedSlots.Select((Location x) => Tuple.Create(x.Deck, x.Slot));
		}
	}

	public int MaxDeck
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _maxDeck;
		}
	}

	public int MaxSlot
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _maxSlot;
		}
	}

	public bool VMZConfigured
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _vmzConfigured;
		}
	}

	public bool IsDenseMachine
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _dense;
		}
	}

	public int BufferSlot
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _bufferSlot;
		}
	}

	public int? QlmDeckAsNumber
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _qlmDeckAsNumber;
		}
	}

	public string QlmDeckAsString
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _qlmDeckAsString;
		}
	}

	public string MerchandizeType
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _merchType;
		}
	}

	public bool IsInitialized { get; private set; }

	public void Initialize()
	{
		Redbox.Core.LogHelper.Instance.Log("Initialize Hardware State.", Redbox.HAL.Component.Model.LogEntryType.Info);
		Refresh();
	}

	public void Refresh()
	{
		if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
		{
			Redbox.Core.LogHelper.Instance.Log("Refresh Hardware State is already running.", Redbox.HAL.Component.Model.LogEntryType.Info);
		}
		try
		{
			HardwareService.Instance.RefreshHardwareState(RefreshPropertiesCallback);
		}
		finally
		{
			_isRunning = 0;
		}
	}

	private void RefreshPropertiesCallback(MachineConfiguration mc)
	{
		try
		{
			if (mc.IsNotValid())
			{
				Redbox.Core.LogHelper.Instance.Log("Bypassing update of hardware state properties since the MachineConfiguration is not valid.  Setting initialized flag to false.");
				IsInitialized = false;
				return;
			}
			_cameraGeneration = mc.CurrentCameraGeneration;
			Redbox.Core.LogHelper.Instance.Log($"Camera Generation: {_cameraGeneration}");
			_barcodeServices = mc.BarcodeDecoder;
			Redbox.Core.LogHelper.Instance.Log($"Barcode Decoder: {_barcodeServices}");
			_airExchangerStatus = mc.AirExchangerStatus;
			Redbox.Core.LogHelper.Instance.Log($"Air Exchanger Status: {_airExchangerStatus}");
			_exchangerFanStatus = mc.AirExchangerFanStatus;
			Redbox.Core.LogHelper.Instance.Log($"Air Exchanger Fan Status: {_exchangerFanStatus}");
			_doorSensorStatus = mc.DoorSensorStatus;
			Redbox.Core.LogHelper.Instance.Log("Door Sensor Status: " + _doorSensorStatus);
			_excludedSlots = ((mc.ExcludedSlots == null) ? new List<Location>() : mc.ExcludedSlots.ToList());
			Redbox.Core.LogHelper.Instance.Log($"Excluded Slots Count: {_excludedSlots.Count()}");
			_maxDeck = mc.MaxDeck;
			Redbox.Core.LogHelper.Instance.Log($"Max Deck: {_maxDeck}");
			_maxSlot = mc.MaxSlot;
			Redbox.Core.LogHelper.Instance.Log($"Max Slot: {_maxSlot}");
			_vmzConfigured = mc.VMZConfigured;
			Redbox.Core.LogHelper.Instance.Log($"VMZConfigured: {_vmzConfigured}");
			_dense = mc.IsDenseMachine;
			Redbox.Core.LogHelper.Instance.Log($"Is Dense Machine: {_dense}");
			_bufferSlot = mc.BufferSlot;
			Redbox.Core.LogHelper.Instance.Log($"Buffer Slot: {_bufferSlot}");
			_qlmDeckAsNumber = (_vmzConfigured ? ((int?)null) : new int?(_maxDeck + 1));
			_qlmDeckAsString = (_qlmDeckAsNumber.HasValue ? _qlmDeckAsNumber.ToString() : null);
			Redbox.Core.LogHelper.Instance.Log($"QLM Deck: {_qlmDeckAsNumber}, {_qlmDeckAsString}");
			_merchType = (_vmzConfigured ? "VMZ2" : "QLM");
			Redbox.Core.LogHelper.Instance.Log("Merchandize Type: " + _merchType);
			Redbox.Core.LogHelper.Instance.Log("End Processing Refresh Hardware State.  Setting initialized flag to true.");
			IsInitialized = true;
		}
		catch (Exception e)
		{
			Redbox.Core.LogHelper.Instance.LogException("Exception running the RefreshePropertiesCallback for HardwareState refresh!  Setting initialized flag to false.", e);
			IsInitialized = false;
		}
	}
}
