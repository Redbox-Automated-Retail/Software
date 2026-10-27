using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.FileUpdate;
using DeviceService.ComponentModel.KDS;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Newtonsoft.Json;
using RBA_SDK_ComponentModel;

namespace DeviceService.Domain;

public class IUC285Proxy : IIUC285Proxy, ITestIUC285Proxy, IFileUpdater
{
	public class Constants
	{
		public const string EMVClessFileName = "emvcless.xml";

		public const string EMVContactFileName = "emvcontact.xml";

		public const string DeviceFilesPath = "/HOST/";
	}

	private enum RetrieveFileDataFormat
	{
		Text,
		Base64
	}

	private class ConfigDefault
	{
		public string Group { get; set; }

		public string Index { get; set; }

		public string Value { get; set; }
	}

	private class EncodedData
	{
		public byte[] EncData { get; set; }

		public int DataLength { get; set; }

		public EncodedResult Result { get; set; }
	}

	private enum EncodedResult
	{
		Success,
		EndOfFile,
		Error
	}

	private enum WriteFileRecordType
	{
		NewFileFull = 0,
		NewFileInitialMulti = 1,
		Continuation = 2,
		LastRecord = 3,
		SetRKIVersion = 4,
		AbortRequest = 9
	}

	private enum UnpackFlag
	{
		UnpackAndVerifyAfterSending,
		WriteFileDirectly
	}

	private static ILogger<IUC285Proxy> _logger;

	private static IRBA_API _rba_api;

	private static int _attemptingReconnect = 0;

	private int _connectAttempts;

	private bool? _isConnected;

	private bool _isInitialized;

	private readonly System.Timers.Timer _reconnectTimer;

	private readonly System.Timers.Timer _healthTimer;

	private UnitDataModel _unitData;

	private static int _continuousResultTimeouts = 0;

	private readonly KioskData _kioskData;

	private readonly IKioskDataServiceClient _statusClient;

	private readonly IApplicationSettings _settings;

	private RebootStatus _currentRebootStatus = new RebootStatus();

	private RebootType _rebootType;

	private readonly IIUC285Notifier _iuc285Notifier;

	private readonly IOptionsMonitor<DeviceServiceConfig> _deviceConfig;

	private bool _isVasInitialized;

	private static TimeSpan REBOOT_THRESHOLD = TimeSpan.FromMinutes(3.0);

	private static string _keys;

	private const string RKI_VAS_KEY_VALID_PROD = "KCV_VKBPK=713F7E";

	private const string RKI_VAS_KEY_VALID_QA_PREFIX = "KCV_";

	private const string RKI_VAS_KEY_INVALID_PROD = "KCV_VKBPK=483320";

	private const string RKI_VAS_KEY_INTERMEDIATE = "KCV_VKBPK=D668B7";

	private static int _connectInProgress = 0;

	private readonly AutoResetEvent _areWaitForReconnect = new AutoResetEvent(initialState: false);

	private readonly IEnumerable<ConfigDefault> _configDefaults = new List<ConfigDefault>
	{
		new ConfigDefault
		{
			Group = "0007",
			Index = "0003",
			Value = "1"
		},
		new ConfigDefault
		{
			Group = "0019",
			Index = "0001",
			Value = "1"
		},
		new ConfigDefault
		{
			Group = "0019",
			Index = "0002",
			Value = "1"
		},
		new ConfigDefault
		{
			Group = "0019",
			Index = "0003",
			Value = "3"
		},
		new ConfigDefault
		{
			Group = "0008",
			Index = "0001",
			Value = "9"
		},
		new ConfigDefault
		{
			Group = "0003",
			Index = "0010",
			Value = "1"
		},
		new ConfigDefault
		{
			Group = "0008",
			Index = "0003",
			Value = "1"
		},
		new ConfigDefault
		{
			Group = "0013",
			Index = "0014",
			Value = "1"
		},
		new ConfigDefault
		{
			Group = "0019",
			Index = "0021",
			Value = "1"
		},
		new ConfigDefault
		{
			Group = "0008",
			Index = "0017",
			Value = "86400"
		},
		new ConfigDefault
		{
			Group = "0007",
			Index = "0014",
			Value = "25"
		},
		new ConfigDefault
		{
			Group = "0007",
			Index = "0001",
			Value = "0"
		},
		new ConfigDefault
		{
			Group = "0007",
			Index = "0039",
			Value = "0"
		},
		new ConfigDefault
		{
			Group = "0007",
			Index = "0022",
			Value = "0"
		}
	};

	private const string _vendorId = "0B00";

	private const string _productId = "0057";

	private static ManualResetEvent _mre = new ManualResetEvent(initialState: false);

	private static ManualResetEvent _mreInsertCard = new ManualResetEvent(initialState: false);

	private static ManualResetEvent _mre3302TransactionPrepareResponse = new ManualResetEvent(initialState: false);

	private static ManualResetEvent _mre3303AuthorizationRequest = new ManualResetEvent(initialState: false);

	private static ManualResetEvent _mre3305AuthorizationConfirmation = new ManualResetEvent(initialState: false);

	private static ManualResetEvent _mre3304AuthorizationResponse = new ManualResetEvent(initialState: false);

	private static ManualResetEvent _mre09CardRemoved = new ManualResetEvent(initialState: true);

	private static ManualResetEvent _mre166VasResponse = new ManualResetEvent(initialState: false);

	private static ManualResetEvent _mre17MerchantDataWriteResponse = new ManualResetEvent(initialState: false);

	private static AutoResetEvent _areOfflineResponse = new AutoResetEvent(initialState: false);

	public CardSourceType CardSource;

	private static bool _authSuccessful;

	private static readonly List<string> _ingenicoTags = new List<string>
	{
		"1000", "1001", "1002", "1003", "1004", "1005", "1006", "1007", "1008", "1009",
		"100A", "100B", "100C", "100D", "100E", "100F", "1010", "1011", "1012", "1013",
		"1014", "1015", "1016", "1017", "1018", "1019", "101A", "101B", "101C", "101D",
		"101E", "101F", "1020", "FF1D", "FF1E", "FF1F", "FF20", "FF21", "9000", "9001"
	};

	private const string PASS_TYPE_IDENTIFIER_QA = "pass.com.vibes.RedboxQAEnvironment";

	private const string PASS_TYPE_IDENTIFIER_PROD = "pass.com.vibes.redbox";

	private const string PASS_TYPE_IDENTIFIER_TEST = "pass.com.redbox.testing";

	private const string PASS_TYPE_IDENTIFIER_INGENICO = "pass.com.ingenico.us.vas.test";

	private const string PASS_TYPE_IDENTIFIER_APPLE_RedBox_Testing = "pass.com.redbox.kiosk";

	private const string PASS_TYPE_GOOGLE_MERCHANT_ID_INGENICO = "70779451";

	private const string PASS_TYPE_GOOGLE_MERCHANT_ID_VIBES_QA = "90833929";

	private const string PASS_TYPE_GOOGLE_MERCHANT_ID_VIBES_PROD = "44813557";

	private List<string> VasBuilds = new List<string> { "23kA", "23kG", "23kK" };

	private const int P62_REQ_FILE_DATA_MAX_SIZE = 4064;

	private DateTime? _deviceRebootTime;

	private Task _rebootTask;

	private static object _getKeysLock = new object();

	public Version EMVClessVersion { get; private set; }

	public Version EMVContactVersion { get; private set; }

	public UnitDataModel UnitData
	{
		get
		{
			if (!IsConnected)
			{
				return null;
			}
			if (_unitData == null)
			{
				_unitData = GetUnitData();
			}
			return _unitData;
		}
		set
		{
			_unitData = value;
		}
	}

	public bool IsConnected
	{
		get
		{
			return _isConnected == true;
		}
		set
		{
			_keys = null;
			_isVasInitialized = false;
			if (_isConnected != value)
			{
				_isConnected = value;
				Log($"IsConnected: {value}");
				if (value)
				{
					ConnectionState = ConnectionState.Connected;
					_reconnectTimer.Stop();
					OnConnectedHandler?.Invoke();
				}
				else
				{
					ConnectionState = ConnectionState.Disconnected;
					ErrorState = ErrorState.None;
					_reconnectTimer.Start();
					OnDisconnectedHandler?.Invoke(null);
				}
			}
			else if (!value && !_reconnectTimer.Enabled)
			{
				_reconnectTimer.Start();
			}
			else if (value && _reconnectTimer.Enabled)
			{
				_reconnectTimer.Stop();
			}
		}
	}

	public bool SupportsEMV
	{
		get
		{
			bool result = false;
			if (TryGetCurrentTgzVersion(out var version))
			{
				result = version != null && (version.Major != 0 || version.Minor != 0);
			}
			return result;
		}
	}

	public bool SupportsVas
	{
		get
		{
			if (!IsConnected)
			{
				return false;
			}
			IOptionsMonitor<DeviceServiceConfig> deviceConfig = _deviceConfig;
			if (deviceConfig != null && !deviceConfig.CurrentValue.EnableVasCommands)
			{
				return false;
			}
			if (_unitData?.ApplicationVersion != null && VasBuilds.Contains(_unitData.ApplicationVersion) && HasValidVasRKI)
			{
				return _isInitialized;
			}
			return false;
		}
	}

	public bool HasValidVasRKI
	{
		get
		{
			string rki = (IsProdEnvironment ? "KCV_VKBPK=713F7E" : "KCV_");
			return HasRKIKey(rki);
		}
	}

	public bool HasInvalidVasRKI
	{
		get
		{
			if (IsProdEnvironment)
			{
				return HasRKIKey("KCV_VKBPK=483320");
			}
			return false;
		}
	}

	public bool HasIntermediateVasRepairKey
	{
		get
		{
			if (IsProdEnvironment)
			{
				return HasRKIKey("KCV_VKBPK=D668B7");
			}
			return false;
		}
	}

	public ConnectionState ConnectionState { get; set; }

	public ErrorState ErrorState { get; set; }

	private ReadCardContext ReadCardContextData { get; set; }

	private bool IsProdEnvironment => (Registry.LocalMachine.OpenSubKey("Software\\Redbox\\REDS\\Kiosk Engine\\Store")?.GetValue("Environment"))?.ToString() == "Production";

	private DateTime DeviceRebootTime
	{
		get
		{
			if (!_deviceRebootTime.HasValue)
			{
				if (int.TryParse(GetVariable_29("500"), out var result))
				{
					_deviceRebootTime = DateTime.Now.AddSeconds(result);
				}
				if (!_deviceRebootTime.HasValue)
				{
					return DateTime.MaxValue;
				}
			}
			return _deviceRebootTime.Value;
		}
	}

	public event OnConnected OnConnectedHandler;

	public event OnDisconnected OnDisconnectedHandler;

	public IUC285Proxy(IRBA_API rba_api, ILogger<IUC285Proxy> logger, IIUC285Notifier iuc285Notifier, IKioskDataServiceClient statusClient, IApplicationSettings settings, IOptionsMonitor<DeviceServiceConfig> deviceConfig)
	{
		_rba_api = rba_api;
		_logger = logger;
		_iuc285Notifier = iuc285Notifier;
		_statusClient = statusClient;
		_settings = settings;
		_deviceConfig = deviceConfig;
		if (_settings != null)
		{
			_kioskData = DataFileHelper.GetFileData<KioskData>(settings.DataFilePath, "KioskData.json", LogException);
		}
		_reconnectTimer = new System.Timers.Timer
		{
			Interval = 10000.0,
			AutoReset = true,
			Enabled = false
		};
		_reconnectTimer.Elapsed += OnReconnect;
		_healthTimer = new System.Timers.Timer
		{
			Interval = 900000.0,
			AutoReset = true,
			Enabled = false
		};
		_healthTimer.Elapsed += OnHealthTimer;
		OnConnectedHandler += OnStartup;
		OnDisconnectedHandler += OnDisconnect;
		_deviceConfig.OnChange(OnConfigChange);
	}

	private void OnConfigChange(DeviceServiceConfig config, string data)
	{
		if (IsConnected)
		{
			_iuc285Notifier?.SendCardReaderStateEvent(GetCardReaderState());
		}
	}

	private void OnStartup()
	{
		_continuousResultTimeouts = 0;
		_deviceRebootTime = null;
		UnitData = GetUnitData();
		EMVClessVersion = GetXMLVersion("emvcless.xml");
		EMVContactVersion = GetXMLVersion("emvcontact.xml");
		_rebootTask = StartRebootCountdown();
		RebootStatus rebootStatus = GetRebootStatus();
		_statusClient?.PostRebootStatus(rebootStatus);
		_rebootType = RebootType.Disconnect;
		_iuc285Notifier?.SendCardReaderConnectedEvent();
		Log($"Connected after {_connectAttempts} attempts.");
		StartHealthTimer();
		_areWaitForReconnect.Set();
		UnitDataModel unitData = UnitData;
		if (unitData != null && unitData.IsTampered)
		{
			_iuc285Notifier?.SendDeviceTamperedEvent();
		}
		_iuc285Notifier?.SendCardReaderStateEvent(GetCardReaderState());
		InitializeVas();
	}

	public CardReaderState GetCardReaderState()
	{
		return new CardReaderState
		{
			SupportsEMV = SupportsEMV,
			IsConnected = IsConnected,
			IsTampered = (UnitData?.IsTampered ?? false),
			SupportsVas = SupportsVas,
			ConnectionState = ConnectionState,
			Version = Assembly.GetExecutingAssembly().GetName().Version,
			ErrorState = ErrorState
		};
	}

	private Version GetXMLVersion(string fileName)
	{
		Version result = null;
		try
		{
			using Stream stream = RetrieveFile(RetrieveFileDataFormat.Text, Path.Combine("/HOST/", fileName));
			if (stream != null)
			{
				StreamReader streamReader = new StreamReader(stream);
				bool flag = false;
				while (!flag && !streamReader.EndOfStream)
				{
					string text = streamReader?.ReadLine()?.Trim();
					if (!string.IsNullOrEmpty(text) && text.StartsWith("<!--") && text.ToUpper().Contains("VERSION") && text.EndsWith("-->"))
					{
						flag = true;
						Version.TryParse(new Regex("\\d+(\\.\\d+)+").Match(text)?.Value, out result);
					}
				}
			}
			else
			{
				Log("Unable to read file " + fileName + " to extract version.");
			}
		}
		catch (Exception arg)
		{
			Log($"Error in GetXMLVersion. {arg}");
		}
		Log($"Extracted version: {result} from XML file: {fileName}");
		return result;
	}

	private void OnReconnect(object s, EventArgs e)
	{
		if (!IsConnected && Interlocked.CompareExchange(ref _attemptingReconnect, 1, 0) == 0)
		{
			Connect();
			_attemptingReconnect = 0;
		}
	}

	private void OnDisconnect(Exception exception)
	{
		RebootStatus currentRebootStatus = _currentRebootStatus;
		bool flag = currentRebootStatus != null && currentRebootStatus.DisconnectTime.HasValue && DateTime.Now >= _currentRebootStatus.DisconnectTime.Value.AddDays(1.0) - REBOOT_THRESHOLD && DateTime.Now <= _currentRebootStatus.DisconnectTime.Value.AddDays(1.0) + REBOOT_THRESHOLD;
		_rebootType = ((_rebootType == RebootType.ServiceStart || _rebootType == RebootType.Scheduled || _rebootType == RebootType.Manual || _rebootType == RebootType.FileUpdate) ? _rebootType : (flag ? RebootType.Automatic : RebootType.Disconnect));
		_currentRebootStatus.DisconnectTime = DateTime.Now;
		if (ReadCardContextData != null)
		{
			ReadCardContextData.State = ReadCardContextState.Canceled;
			ReadCardContextData.Errors.Add(new Error
			{
				Code = "READERDISCONNECT",
				Message = "The Reader was disconnected, automatically cancelling job"
			});
			ReadCancel();
		}
		_iuc285Notifier?.SendCardReaderDisconnectedEvent(exception);
		StopHealthTimer();
		if (ReadCardContextData != null)
		{
			ReadCardContextData.State = ReadCardContextState.Canceled;
			ReadCardContextData.Errors.Add(new Error
			{
				Code = "READERDISCONNECT",
				Message = "The Reader was disconnected, automatically cancelling job"
			});
			ReadCancel();
		}
	}

	public static void Log(string s)
	{
		_logger?.LogInformation(s);
	}

	public static void LogError(string s)
	{
		_logger?.LogError(s);
	}

	public void HandleAndLogResult(ERROR_ID result, string resultName = null, bool includeResultLog = true)
	{
		if (includeResultLog)
		{
			Log($"{resultName} Result: {result}");
		}
		switch (result)
		{
		case ERROR_ID.RESULT_SUCCESS:
			_continuousResultTimeouts = 0;
			break;
		case ERROR_ID.RESULT_ERROR_TIMEOUT:
			_continuousResultTimeouts++;
			break;
		default:
			_continuousResultTimeouts = 0;
			break;
		}
		if (_continuousResultTimeouts >= 10 && _continuousResultTimeouts % 10 == 0)
		{
			Log($"Continuous Timeouts: {_continuousResultTimeouts}");
		}
	}

	public static void LogException(string message, Exception e)
	{
		_logger?.LogError(e, "Unhandled Exception in " + message);
	}

	private void OnHealthTimer(object sender, ElapsedEventArgs e)
	{
		Log("OnHealthTimer Elapsed Begin");
		if (!PingDevice())
		{
			Reconnect();
		}
	}

	private bool HasRKIKey(string rki)
	{
		Log("Begin HasRKI " + rki + " in " + _keys);
		if (string.IsNullOrWhiteSpace(_keys))
		{
			GetKeys();
		}
		if (string.IsNullOrEmpty(_keys))
		{
			return false;
		}
		return _keys.Contains(rki);
	}

	public bool Initialize()
	{
		ERROR_ID eRROR_ID = ERROR_ID.RESULT_SUCCESS;
		try
		{
			_rba_api.logHandler = delegate(string s)
			{
				IOptionsMonitor<DeviceServiceConfig> deviceConfig = _deviceConfig;
				if (deviceConfig != null && deviceConfig.CurrentValue.EnableIngenicoTraceLogging)
				{
					Log(s);
				}
			};
			_rba_api.SetDefaultLogLevel(LOG_LEVEL.TRACE);
			eRROR_ID = _rba_api.Initialize();
			HandleAndLogResult(eRROR_ID, "Initialize RBA API");
			if (eRROR_ID != ERROR_ID.RESULT_SUCCESS)
			{
				IsConnected = false;
				_isInitialized = false;
				return false;
			}
			string version_NonStatic = _rba_api.GetVersion_NonStatic();
			_rba_api.pinpadHandler = TerminalMessageHandler;
			Log("Initialized!");
			Log("RBA_SDK Version: " + version_NonStatic);
			_rba_api.SetNotifyRbaDisconnected(delegate
			{
				Log("**** DISCONNECTED ****");
				IsConnected = false;
			});
			SETTINGS_COMM_TIMEOUTS commTimeouts = new SETTINGS_COMM_TIMEOUTS
			{
				ConnectTimeout = 5000u,
				ReceiveTimeout = 5000u,
				SendTimeout = 5000u
			};
			eRROR_ID = _rba_api.SetCommTimeouts(commTimeouts);
			HandleAndLogResult(eRROR_ID, "Set Comm Timeouts");
		}
		catch (Exception e)
		{
			LogException("Initialize RBA SDK", e);
			eRROR_ID = ERROR_ID.RESULT_ERROR;
		}
		return _isInitialized = eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	public bool Connect()
	{
		if (Interlocked.CompareExchange(ref _connectInProgress, 1, 0) == 1)
		{
			return false;
		}
		ConnectionState = ConnectionState.Connecting;
		ErrorState = ErrorState.None;
		_connectAttempts++;
		ERROR_ID eRROR_ID = ERROR_ID.RESULT_ERROR;
		string text = string.Empty;
		try
		{
			if (!_isInitialized && !Initialize())
			{
				IsConnected = false;
				ErrorState = ErrorState.FailedToInitializeConnection;
				return false;
			}
			foreach (string item in ComPortNames())
			{
				text = item;
				eRROR_ID = _rba_api.Connect(text);
				if (eRROR_ID == ERROR_ID.RESULT_SUCCESS)
				{
					break;
				}
			}
		}
		catch (Exception e)
		{
			IsConnected = false;
			ErrorState = ErrorState.Unknown;
			LogException("Connect", e);
			return false;
		}
		finally
		{
			if (ErrorState == ErrorState.None)
			{
				bool flag = eRROR_ID == ERROR_ID.RESULT_SUCCESS || eRROR_ID == ERROR_ID.RESULT_ERROR_ALREADY_CONNECTED;
				bool flag2 = UpdateDeviceConfiguration();
				IsConnected = flag & flag2;
				if (IsConnected)
				{
					HandleAndLogResult(eRROR_ID, "Connect to " + text);
					_connectAttempts = 0;
				}
				else if (flag && !flag2)
				{
					ErrorState = ErrorState.FailedToUpdateDevice;
					UnitDataModel unitData = GetUnitData();
					if (unitData != null && unitData.IsTampered)
					{
						ErrorState = ErrorState.Tampered;
						_iuc285Notifier?.SendDeviceTamperedEvent();
						IsConnected = true;
					}
					HandleAndLogResult(eRROR_ID, "Connect to " + text, includeResultLog: false);
				}
				else
				{
					ErrorState = ErrorState.ConnectionError;
					HandleAndLogResult(eRROR_ID, "Connect to " + text, includeResultLog: false);
				}
				_connectInProgress = 0;
			}
		}
		return IsConnected;
	}

	public UnitHealthModel GetUnitHealth()
	{
		_rba_api.SetParam(PARAMETER_ID.P08_REQ_REQUEST_TYPE, "0");
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M08_HEALTH_STAT);
		HandleAndLogResult(eRROR_ID, "08 Health Stat");
		if (eRROR_ID != ERROR_ID.RESULT_SUCCESS)
		{
			return null;
		}
		UnitHealthModel unitHealthModel = new UnitHealthModel
		{
			MSRSwipeCount = _rba_api.GetParam(PARAMETER_ID.P08_RES_COUNT_MSR_SWIPES)
		};
		Log("P08_RES_COUNT_MSR_SWIPES: " + unitHealthModel.MSRSwipeCount);
		unitHealthModel.BadTrack1Reads = _rba_api.GetParam(PARAMETER_ID.P08_RES_COUNT_BAD_TRACK1_READS);
		Log("P08_RES_COUNT_BAD_TRACK1_READS: " + unitHealthModel.BadTrack1Reads);
		unitHealthModel.BadTrack2Reads = _rba_api.GetParam(PARAMETER_ID.P08_RES_COUNT_BAD_TRACK2_READS);
		Log("P08_RES_COUNT_BAD_TRACK2_READS: " + unitHealthModel.BadTrack2Reads);
		unitHealthModel.BadTrack3Reads = _rba_api.GetParam(PARAMETER_ID.P08_RES_COUNT_BAD_TRACK3_READS);
		Log("P08_RES_COUNT_BAD_TRACK3_READS: " + unitHealthModel.BadTrack3Reads);
		unitHealthModel.RebootCount = _rba_api.GetParam(PARAMETER_ID.P08_RES_COUNT_REBOOT);
		Log("P08_RES_COUNT_REBOOT: " + unitHealthModel.RebootCount);
		unitHealthModel.DeviceName = _rba_api.GetParam(PARAMETER_ID.P08_RES_DEVICE_NAME);
		Log("P08_RES_DEVICE_NAME: " + unitHealthModel.DeviceName);
		unitHealthModel.SerialNumber = _rba_api.GetParam(PARAMETER_ID.P08_RES_SERIAL_NUMBER);
		Log("P08_RES_SERIAL_NUMBER: " + unitHealthModel.SerialNumber);
		unitHealthModel.OSVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_OS_VERSION);
		Log("P08_RES_OS_VERSION: " + unitHealthModel.OSVersion);
		unitHealthModel.AppVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_APP_VERSION);
		Log("P08_RES_APP_VERSION: " + unitHealthModel.AppVersion);
		unitHealthModel.SecurityLibVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_SECURITY_LIB_VERSION);
		Log("P08_RES_SECURITY_LIB_VERSION: " + unitHealthModel.SecurityLibVersion);
		unitHealthModel.TDAVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_TDA_VERSION);
		Log("P08_RES_TDA_VERSION: " + unitHealthModel.TDAVersion);
		unitHealthModel.EFTLVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_EFTL_VERSION);
		Log("P08_RES_EFTL_VERSION: " + unitHealthModel.EFTLVersion);
		unitHealthModel.EFTPVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_EFTP_VERSION);
		Log("P08_RES_EFTP_VERSION: " + unitHealthModel.EFTPVersion);
		unitHealthModel.RamSize = _rba_api.GetParam(PARAMETER_ID.P08_RES_RAM_SIZE);
		Log("P08_RES_RAM_SIZE: " + unitHealthModel.RamSize);
		unitHealthModel.FlashSize = _rba_api.GetParam(PARAMETER_ID.P08_RES_FLASH_SIZE);
		Log("P08_RES_FLASH_SIZE: " + unitHealthModel.FlashSize);
		unitHealthModel.ManufactureDate = _rba_api.GetParam(PARAMETER_ID.P08_RES_MANUFACTURE_DATE);
		Log("P08_RES_MANUFACTURE_DATE: " + unitHealthModel.ManufactureDate);
		unitHealthModel.CPEMType = _rba_api.GetParam(PARAMETER_ID.P08_RES_CPEM_TYPE);
		Log("P08_RES_CPEM_TYPE: " + unitHealthModel.CPEMType);
		unitHealthModel.AppName = _rba_api.GetParam(PARAMETER_ID.P08_RES_APP_NAME);
		Log("P08_RES_APP_NAME: " + unitHealthModel.AppName);
		unitHealthModel.ManufactureId = _rba_api.GetParam(PARAMETER_ID.P08_RES_MANUFACTURE_ID);
		Log("P08_RES_MANUFACTURE_ID: " + unitHealthModel.ManufactureId);
		unitHealthModel.DigitizerVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_DIGITIZER_VERSION);
		Log("P08_RES_DIGITIZER_VERSION: " + unitHealthModel.DigitizerVersion);
		unitHealthModel.ManufacturingSerialNumber = _rba_api.GetParam(PARAMETER_ID.P08_RES_MANUFACTURING_SERIAL_NUMBER);
		Log("P08_RES_MANUFACTURING_SERIAL_NUMBER: " + unitHealthModel.ManufacturingSerialNumber);
		return unitHealthModel;
	}

	private UnitDataModel GetUnitData()
	{
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M07_UNIT_DATA);
		HandleAndLogResult(eRROR_ID, "07 Unit Data");
		if (eRROR_ID != ERROR_ID.RESULT_SUCCESS)
		{
			return null;
		}
		UnitDataModel unitDataModel = new UnitDataModel
		{
			Manufacture = _rba_api.GetParam(PARAMETER_ID.P07_RES_MANUFACTURE)
		};
		Log("P07_RES_MANUFACTURE: " + unitDataModel.Manufacture);
		unitDataModel.DeviceType = _rba_api.GetParam(PARAMETER_ID.P07_RES_DEVICE);
		Log("P07_RES_DEVICE: " + unitDataModel.DeviceType);
		unitDataModel.UnitSerialNumber = _rba_api.GetParam(PARAMETER_ID.P07_RES_UNIT_SERIAL_NUMBER);
		Log("P07_RES_UNIT_SERIAL_NUMBER: " + unitDataModel.UnitSerialNumber);
		unitDataModel.RamSize = _rba_api.GetParam(PARAMETER_ID.P07_RES_RAM_SIZE);
		Log("P07_RES_RAM_SIZE: " + unitDataModel.RamSize);
		unitDataModel.FlashSize = _rba_api.GetParam(PARAMETER_ID.P07_RES_FLASH_SIZE);
		Log("P07_RES_FLASH_SIZE: " + unitDataModel.FlashSize);
		unitDataModel.DigitizerVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_DIGITIZER_VERSION);
		Log("P07_RES_DIGITIZER_VERSION: " + unitDataModel.DigitizerVersion);
		unitDataModel.SecurityModuleVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_SECURITY_MODULE_VERSION);
		Log("P07_RES_SECURITY_MODULE_VERSION: " + unitDataModel.SecurityModuleVersion);
		unitDataModel.OSVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_OS_VERSION);
		Log("P07_RES_OS_VERSION: " + unitDataModel.OSVersion);
		unitDataModel.ApplicationVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_APPLICATION_VERSION);
		Log("P07_RES_APPLICATION_VERSION: " + unitDataModel.ApplicationVersion);
		unitDataModel.EFTLVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_EFTL_VERSION);
		Log("P07_RES_EFTL_VERSION: " + unitDataModel.EFTLVersion);
		unitDataModel.EFTPVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_EFTP_VERSION);
		Log("P07_RES_EFTP_VERSION: " + unitDataModel.EFTPVersion);
		unitDataModel.ManufacturingSerialNumber = _rba_api.GetParam(PARAMETER_ID.P07_RES_MANUFACTURING_SERIAL_NUMBER);
		Log("P07_RES_MANUFACTURING_SERIAL_NUMBER: " + unitDataModel.ManufacturingSerialNumber);
		unitDataModel.DCKernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_EMV_DC_KERNEL_TYPE);
		Log("P07_RES_EMV_DC_KERNEL_TYPE: " + unitDataModel.DCKernelType);
		unitDataModel.EMVEngineKernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_EMV_ENGINE_KERNEL_TYPE);
		Log("P07_RES_EMV_ENGINE_KERNEL_TYPE: " + unitDataModel.EMVEngineKernelType);
		unitDataModel.CLessDiscoverKernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_DISCOVER_KERNEL_TYPE);
		Log("P07_RES_CLESS_DISCOVER_KERNEL_TYPE: " + unitDataModel.CLessDiscoverKernelType);
		unitDataModel.ClessExpresPayV3KernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_EXPRESSPAY_V3_KERNEL_TYPE);
		Log("P07_RES_CLESS_EXPRESSPAY_V3_KERNEL_TYPE: " + unitDataModel.ClessExpresPayV3KernelType);
		unitDataModel.ClessExpresPayV2KernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_EXPRESSPAY_V2_KERNEL_TYPE);
		Log("P07_RES_CLESS_EXPRESSPAY_V2_KERNEL_TYPE: " + unitDataModel.ClessExpresPayV2KernelType);
		unitDataModel.ClessPayPassV3KernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_PAYPASS_V3_KERNEL_TYPE);
		Log("P07_RES_CLESS_PAYPASS_V3_KERNEL_TYPE: " + unitDataModel.ClessPayPassV3KernelType);
		unitDataModel.ClessPayPassV3AppType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_PAYPASS_V3_APP_TYPE);
		Log("P07_RES_CLESS_PAYPASS_V3_APP_TYPE: " + unitDataModel.ClessPayPassV3AppType);
		unitDataModel.ClessVisaPayWaveKernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_VISA_PAYWAVE_KERNEL_TYPE);
		Log("P07_RES_CLESS_VISA_PAYWAVE_KERNEL_TYPE: " + unitDataModel.ClessVisaPayWaveKernelType);
		unitDataModel.ClessInteracKernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_INTERAC_KERNEL_TYPE);
		Log("P07_RES_CLESS_INTERAC_KERNEL_TYPE: " + unitDataModel.ClessInteracKernelType);
		unitDataModel.ClessInterfaceIsSupported = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_INTERFACE_IS_SUPPORTED);
		Log("P07_RES_CLESS_INTERFACE_IS_SUPPORTED: " + unitDataModel.ClessInterfaceIsSupported);
		return unitDataModel;
	}

	public async Task<bool> ReadCard(ICardReadRequest request, CancellationToken token, Action<Base87CardReadModel> jobCompletedCallback, Action<string, string> eventsCallback)
	{
		Log("ENTER READ CARD");
		return await ReadCardInner(request, token, jobCompletedCallback, eventsCallback);
	}

	private bool IsCancelled(ReadCardContext context)
	{
		if (context.CancelToken.IsCancellationRequested)
		{
			Log("Cancelling...");
			context.Errors.Add(new Error
			{
				Code = "Cancel",
				Message = "Cancelled by client."
			});
			context.ResponseDataModel = new EncryptedCardReadModel(context.Errors)
			{
				Status = ResponseStatus.Cancelled,
				TimeTaken = context.Watch.Elapsed
			};
			context.SendCompletedCallback();
			return true;
		}
		return false;
	}

	private bool SendCardIsBeingProcessed(ReadCardContext context)
	{
		context.SendCardProcessing();
		return true;
	}

	private void PerformActionAndCheckSuccess<T>(Func<ReadCardContext, T> action, Predicate<T> checkSuccess, ReadCardContext context, VasMode vasMode = VasMode.VasAndPay)
	{
		if (typeof(T) == typeof(bool) && checkSuccess == null)
		{
			checkSuccess = (T input) => Convert.ToBoolean(input);
		}
		if (context == null || !context.CanContinue(VasMode.VasAndPay))
		{
			Log($"PerformActionAndCheckSuccess on {action?.Method?.Name} is in state: {context?.State} and can no longer continue.");
			return;
		}
		if (context.Id != ReadCardContextData.Id)
		{
			LogError($"PerformActionAndCheckSuccess on {action?.Method?.Name} for context: {context.Id} does not match ReadcardContextData: {ReadCardContextData?.Id}");
			context.UpdateStopStateFor(VasMode.VasAndPay);
			context.Errors.Add(new Error
			{
				Code = "CNTXTMM",
				Message = "The Context ID for read does not match what is currently running."
			});
			return;
		}
		UnitDataModel unitData = _unitData;
		if (unitData != null && unitData.IsTampered)
		{
			Log("PerformActionAndCheckSuccess on " + action?.Method?.Name + " is Tampered and can no longer continue.");
			context.State = ReadCardContextState.Tampered;
			return;
		}
		if (action == null || checkSuccess == null)
		{
			LogError("PerformActionAndCheckSuccess Action or Check Success Is Null.");
			context.UpdateStopStateFor(VasMode.VasAndPay);
			return;
		}
		if (IsCancelled(context))
		{
			Log("PerformActionAndCheckSuccess on " + action?.Method?.Name + " - context is cancelled and can no longer continue.");
			context.State = ReadCardContextState.Canceled;
		}
		T obj = action(context);
		if (!checkSuccess(obj))
		{
			Log("PerformActionAndCheckSuccess on " + action?.Method?.Name + " did not complete successfully.");
			context.UpdateStopStateFor(vasMode);
		}
	}

	private async Task<bool> ReadCardInner(ICardReadRequest request, CancellationToken cancel, Action<Base87CardReadModel> jobCompletedCallback, Action<string, string> eventsCallback)
	{
		InitializeReadCard();
		IUC285Proxy iUC285Proxy = this;
		ReadCardContext obj = new ReadCardContext(_logger)
		{
			Request = request,
			UserInteractionTimeout = _deviceConfig?.CurrentValue.CardReadUserInteractionTimeout,
			CancelToken = cancel,
			State = ReadCardContextState.Continue,
			JobCompletedCallback = jobCompletedCallback,
			EventsCallback = eventsCallback,
			UnitData = _unitData
		};
		ReadCardContext readCardContext = obj;
		iUC285Proxy.ReadCardContextData = obj;
		ReadCardContext context = readCardContext;
		try
		{
			if (context.Request.Timeout < 30000)
			{
				context.Request.Timeout = 30000;
			}
			if (context.CanContinue(VasMode.VasOnly) && !SupportsVas)
			{
				LogError("Vas Is Not Supported. Requests should not be sent for vas data.");
				context.UpdateStopStateFor(VasMode.VasOnly);
				if (!context.CanContinue(VasMode.PayOnly))
				{
					context.Errors.Add(new Error
					{
						Code = "VASNOTSUPPORTED",
						Message = "Vas is not supported or is not enabled."
					});
				}
			}
			if (context.Request.VasMode == VasMode.PayOnly && context.State == ReadCardContextState.Continue)
			{
				context.State = ReadCardContextState.PayContinue;
			}
			if (context.Request.VasMode == VasMode.VasOnly && context.State == ReadCardContextState.Continue)
			{
				context.State = ReadCardContextState.VasContinue;
			}
			PerformActionAndCheckSuccess(Offline, null, context);
			PerformActionAndCheckSuccess(PrepareVas, null, context);
			PerformActionAndCheckSuccess(SetAmount, null, context);
			PerformActionAndCheckSuccess(CardReadRequestEncrypted, null, context);
			PerformActionAndCheckSuccess(WaitForVasReadStart, null, context, VasMode.VasOnly);
			PerformActionAndCheckSuccess(WaitForCardReadComplete, null, context);
			PerformActionAndCheckSuccess(SendCardIsBeingProcessed, null, context);
			PerformActionAndCheckSuccess(GetVasData, null, context, VasMode.VasOnly);
			if (context.IsPayEnabled)
			{
				switch (context.CardSource)
				{
				case CardSourceType.VASOnly:
					Log("Vas Only Flow");
					if (context.State.HasFlag(ReadCardContextState.VasContinue) && !_mre166VasResponse.WaitOne(context.TimeRemainingUntilTimeout))
					{
						context.State = ReadCardContextState.Timeout;
						Log("Timeout Waiting for data.");
					}
					break;
				case CardSourceType.Swipe:
					Log("Enter Swipe Flow");
					break;
				case CardSourceType.QuickChip:
					Log("Enter Quick Chip Flow");
					if (context.State.HasFlag(ReadCardContextState.PayContinue) && !_mre3305AuthorizationConfirmation.WaitOne(context.TimeRemainingUntilTimeout))
					{
						context.State = ReadCardContextState.Timeout;
						Log("Timeout in Quickchip waiting for data.");
					}
					break;
				case CardSourceType.EMVContactless:
				case CardSourceType.Mobile:
					Log("Enter EMV Contactless Flow");
					PerformActionAndCheckSuccess(EMVResponse, null, context, VasMode.PayOnly);
					break;
				case CardSourceType.MSDContactless:
					Log("Enter MSD Contactless Flow");
					break;
				case CardSourceType.EMVContact:
					Log("Enter EMV Contact Flow");
					if (context.State.HasFlag(ReadCardContextState.PayContinue))
					{
						PerformActionAndCheckSuccess(EMVTransInitiation, null, context, VasMode.PayOnly);
						PerformActionAndCheckSuccess(SetPaymentType_04, null, context, VasMode.PayOnly);
						PerformActionAndCheckSuccess(SetAmount, null, context, VasMode.PayOnly);
						PerformActionAndCheckSuccess(EMVResponse, null, context, VasMode.PayOnly);
					}
					break;
				default:
					Log($"Unhandled Source Type Occurred: {CardSource}");
					context.Errors.Add(new Error
					{
						Code = "SourceInvalid",
						Message = $"An invalid or unhandled source type: {CardSource} was received in ReadCardInner."
					});
					break;
				}
			}
		}
		catch (TaskCanceledException)
		{
			Log("Read Card Cancelled");
		}
		catch (Exception e)
		{
			LogException("Encrypted Read Card", e);
			context.Errors.Add(new Error
			{
				Code = "DS001",
				Message = "Unhandled Exception in ReadCardInner."
			});
			context.ResponseDataModel = new EncryptedCardReadModel
			{
				Status = ResponseStatus.Errored
			};
			context.SendCompletedCallback();
			return false;
		}
		finally
		{
			Offline();
			Log("END READ CARD");
		}
		context.SendCompletedCallback();
		if (context.TimeRemainingUntilTimeout >= 0 && context.State != ReadCardContextState.Timeout && context.State != ReadCardContextState.Canceled)
		{
			if (TryGetCurrentTgzVersion(out var version))
			{
				context.CardStats.TgzVersion = version?.ToString();
			}
			if (_kioskData != null && _kioskData.KioskId != 0L)
			{
				context.CardStats.KioskId = _kioskData.KioskId;
			}
			Task.Run(() => _statusClient?.PostCardStats(context.CardStats));
		}
		return true;
	}

	private bool EMVResponse(ReadCardContext context)
	{
		if (context.State.HasFlag(ReadCardContextState.PayContinue))
		{
			if (!_mre3303AuthorizationRequest.WaitOne(context.TimeRemainingUntilTimeout))
			{
				Log("Timeout Waiting for EMV Response.");
				return false;
			}
			_authSuccessful = false;
			Log("GO AUTH.");
			context.SendCompletedCallback();
			if (context.EventsCallback == null || _mre3304AuthorizationResponse.WaitOne(context.TimeRemainingUntilTimeout))
			{
				SendAuthorizationReponse(context.EventsCallback == null || _authSuccessful);
			}
			else
			{
				Log("Failed to receive auth response");
			}
			Log("GET CONFIRMATION");
			_mre3305AuthorizationConfirmation.WaitOne(context.TimeRemainingUntilTimeout);
		}
		return true;
	}

	public void SetAuthorizationResponse(bool success)
	{
		_authSuccessful = success;
		_mre3304AuthorizationResponse.Set();
	}

	public bool UpdateDeviceConfiguration()
	{
		bool flag = false;
		int num = 3;
		foreach (ConfigDefault configDefault in _configDefaults)
		{
			string Data = null;
			flag = false;
			while (!flag && num > 0)
			{
				flag = ReadConfig(configDefault.Group, configDefault.Index, out Data, sendOffline: false) && Data != null;
				if (!flag)
				{
					num--;
				}
			}
			if (!flag || num <= 0)
			{
				break;
			}
			if (string.Compare(Data, configDefault.Value, ignoreCase: true) != 0)
			{
				flag = WriteConfig(configDefault.Group, configDefault.Index, configDefault.Value, sendOffline: false);
				if (!flag)
				{
					break;
				}
			}
		}
		if (flag)
		{
			flag = UpdateCardConfigurationData();
		}
		SetVariable_28("201", DateTime.Now.TimeOfDay.ToString("hhmmss"));
		SetVariable_28("202", DateTime.Now.Date.ToString("MMddyy"));
		if (_settings != null && DataFileHelper.GetFileData<TimeSpan>(_settings.DataFilePath, "RebootTime.json", LogException) == default(TimeSpan))
		{
			SetRebootTime();
		}
		Offline();
		return flag;
	}

	private void SetRebootTime()
	{
		if (_settings != null)
		{
			int num = new Random().Next(0, 3600);
			TimeSpan data = TimeSpan.FromSeconds(10800 + num);
			DataFileHelper.TryUpdateFileData(_settings.DataFilePath, "RebootTime.json", data);
		}
	}

	public bool UpdateCardConfigurationData()
	{
		bool flag = true;
		string text = "0011";
		for (int i = 1; i <= 16; i++)
		{
			flag = ReadConfig(text, i.ToString(), out var Data, sendOffline: false);
			if (!flag)
			{
				break;
			}
			List<string> list = Data.Split(' ').ToList();
			if (string.Compare(list[0], "1", ignoreCase: true) != 0)
			{
				continue;
			}
			bool flag2 = false;
			int num = 0;
			for (int j = 0; j < list.Count; j++)
			{
				if (!string.IsNullOrWhiteSpace(list[j]))
				{
					if ((num == 6 && list[j] != "0") || (num == 8 && list[j] != "0"))
					{
						list[j] = "0";
						flag2 = true;
					}
					num++;
				}
			}
			string value = string.Join(" ", list);
			if (flag2)
			{
				flag = WriteConfig(text, i.ToString(), value, sendOffline: false);
				if (!flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	private Stream RetrieveFile(RetrieveFileDataFormat dataFormat, string filename)
	{
		Stream stream = null;
		Log("Starting RetrieveFile for filename " + filename);
		try
		{
			string text = "";
			ERROR_ID eRROR_ID = _rba_api.SetParam(PARAMETER_ID.P65_REQ_RECORD_TYPE, "1");
			IRBA_API rba_api = _rba_api;
			int num = (int)dataFormat;
			eRROR_ID = rba_api.SetParam(PARAMETER_ID.P65_REQ_DATA_TYPE, num.ToString());
			eRROR_ID = _rba_api.SetParam(PARAMETER_ID.P65_REQ_FILE_NAME, filename);
			eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M65_RETRIEVE_FILE);
			Log($"ProcessMessage 65. {eRROR_ID}");
			string param = _rba_api.GetParam(PARAMETER_ID.P65_RES_RESULT);
			Log("P65_RES_RESULT: " + param);
			switch (param)
			{
			case "0":
			{
				int num2 = Convert.ToInt32(_rba_api.GetParam(PARAMETER_ID.P65_RES_TOTAL_NUMBER_OF_BLOCKS));
				Log($"Total number of blocks: {num2}");
				Log("CRC: " + _rba_api.GetParam(PARAMETER_ID.P65_RES_CRC));
				Log("Data type: " + _rba_api.GetParam(PARAMETER_ID.P65_RES_DATA_TYPE));
				Log("# of blocks downloaded: " + _rba_api.GetParam(PARAMETER_ID.P65_RES_BLOCK_NUMBER));
				string param2 = _rba_api.GetParam(PARAMETER_ID.P65_RES_DATA);
				text += param2;
				if (num2 > 1)
				{
					for (int i = 1; i < num2; i++)
					{
						eRROR_ID = _rba_api.SetParam(PARAMETER_ID.P65_REQ_RECORD_TYPE, "0");
						IRBA_API rba_api2 = _rba_api;
						num = (int)dataFormat;
						eRROR_ID = rba_api2.SetParam(PARAMETER_ID.P65_REQ_DATA_TYPE, num.ToString());
						eRROR_ID = _rba_api.SetParam(PARAMETER_ID.P65_REQ_FILE_NAME, filename);
						eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M65_RETRIEVE_FILE);
						param = _rba_api.GetParam(PARAMETER_ID.P65_RES_RESULT);
						if (param == "0")
						{
							Log("# of blocks downloaded: " + _rba_api.GetParam(PARAMETER_ID.P65_RES_BLOCK_NUMBER));
							param2 = _rba_api.GetParam(PARAMETER_ID.P65_RES_DATA);
							text += param2;
						}
					}
				}
				try
				{
					stream = new MemoryStream();
					StreamWriter streamWriter = new StreamWriter(stream);
					streamWriter.Write(text);
					streamWriter.Flush();
					stream.Position = 0L;
					Log("File successfully read into memeory stream " + filename);
				}
				catch (Exception arg)
				{
					Log($"Exception: {arg}");
				}
				break;
			}
			case "1":
				Log("File not found: " + filename);
				break;
			case "2":
				Log("Error while converting original data to Base64 format");
				break;
			default:
				Log("Format change error");
				break;
			}
		}
		catch (Exception arg2)
		{
			Log($"Error in RetrieveFile. {arg2}");
		}
		Log("Ending RetrieveFile for filename " + filename);
		return stream;
	}

	public async Task<bool> WriteFile(string fullPath, bool rebootAfterWrite = false)
	{
		if (string.IsNullOrEmpty(fullPath))
		{
			return false;
		}
		return await WriteFileInner(fullPath, rebootAfterWrite);
	}

	public void PrepareToUpdateFiles()
	{
		_iuc285Notifier?.SetCommandQueueState(commandQueuePaused: true);
	}

	public void CompleteFileUpdates()
	{
		_iuc285Notifier?.SetCommandQueueState(commandQueuePaused: false);
	}

	public async Task<bool> WriteStream(Stream stream, string fileName, bool rebootAfterWrite = false, bool waitForReconnect = false, int reconnectTimeout = 180000)
	{
		_areWaitForReconnect.Reset();
		bool flag = await WriteStreamInner(stream, fileName, rebootAfterWrite);
		if ((flag & rebootAfterWrite & waitForReconnect) && !_areWaitForReconnect.WaitOne(reconnectTimeout))
		{
			return false;
		}
		_ = _kioskData?.KioskId;
		return flag;
	}

	public int GetFileUpdateRevisionNumber()
	{
		int result = -1;
		if (IsConnected)
		{
			ReadConfig("14", "10", out var Data, sendOffline: false);
			if (int.TryParse(Data, out var result2))
			{
				result = result2;
			}
		}
		return result;
	}

	public bool SetFileUpdateRevisionNumber(string revisionNumber)
	{
		bool result = false;
		if (IsConnected)
		{
			result = WriteConfig("14", "10", revisionNumber);
		}
		return result;
	}

	public bool TryGetCurrentTgzVersion(out Version version)
	{
		version = new Version();
		if (!IsConnected)
		{
			return false;
		}
		ReadConfig("14", "1", out var Data, sendOffline: false);
		return Version.TryParse(Data, out version);
	}

	private static List<string> ComPortNames()
	{
		Regex regex = new Regex("^VID_0B00.PID_0057", RegexOptions.IgnoreCase);
		List<string> list = new List<string>();
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
		string[] subKeyNames = registryKey.GetSubKeyNames();
		foreach (string name in subKeyNames)
		{
			RegistryKey registryKey2 = registryKey.OpenSubKey(name);
			string[] subKeyNames2 = registryKey2.GetSubKeyNames();
			foreach (string text in subKeyNames2)
			{
				if (!regex.Match(text).Success)
				{
					continue;
				}
				RegistryKey registryKey3 = registryKey2.OpenSubKey(text);
				string[] subKeyNames3 = registryKey3.GetSubKeyNames();
				foreach (string name2 in subKeyNames3)
				{
					RegistryKey? obj = registryKey3?.OpenSubKey(name2);
					_ = (string)obj?.GetValue("LocationInformation");
					RegistryKey registryKey4 = obj.OpenSubKey("Device Parameters");
					string value = (string)registryKey4?.GetValue("PortName");
					if (!string.IsNullOrEmpty(value) && Enumerable.Contains(SerialPort.GetPortNames(), value))
					{
						list.Add((string)registryKey4.GetValue("PortName"));
					}
				}
			}
		}
		if (!list.Any())
		{
			list.Add("COM7");
		}
		return list;
	}

	private static string ByteArrayToString(byte[] data)
	{
		StringBuilder stringBuilder = new StringBuilder(data.Length * 2);
		foreach (byte b in data)
		{
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		return stringBuilder.ToString();
	}

	private string VasMessageResponseText(string response)
	{
		return response switch
		{
			"0" => "Command Successful", 
			"1" => "Command Failed", 
			"2" => "FailedToRetrieveData", 
			"4" => "Invalid Format", 
			"10" => "Vas Returned", 
			"11" => "Vas Returned, Payment Pending", 
			_ => "Unknown Response", 
		};
	}

	private VasResult M17_6SendVasMessage(VasCmd vasCmd, params string[] vasParams)
	{
		VasResult vasResult = new VasResult
		{
			VasCommand = vasCmd
		};
		_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
		_rba_api.SetParam(PARAMETER_ID.P17_REQ_TYPE, "6");
		int num = (int)vasCmd;
		string text = num.ToString().PadLeft(3, '0');
		_rba_api.SetParam(PARAMETER_ID.P17_REQ_VAS_CMD, text);
		if (vasParams.Length != 0)
		{
			foreach (string v in vasParams)
			{
				_rba_api.AddParam(PARAMETER_ID.P17_REQ_VAS_PARAMETER, v);
			}
		}
		vasResult.CommandResult = _rba_api.ProcessMessage(MESSAGE_ID.M17_MERCHANT_DATA_WRITE);
		if (_rba_api.GetParam(PARAMETER_ID.P17_RES_TYPE) != "6")
		{
			return vasResult;
		}
		string param = _rba_api.GetParam(PARAMETER_ID.P17_RES_STATUS);
		vasResult.Status = (Enum.TryParse(typeof(VasStatus), param, out object result) ? ((VasStatus)result) : VasStatus.Unknown);
		vasResult.AdditionalData = _rba_api.GetParam(PARAMETER_ID.P17_RES_IS_ADDITIONAL_DATA);
		string param2 = _rba_api.GetParam(PARAMETER_ID.P17_RES_VAS_RESULT);
		vasResult.VasDataResult = param2;
		List<string> list = new List<string>();
		int paramLen = _rba_api.GetParamLen(PARAMETER_ID.P17_RES_VAS_DATA);
		while (vasResult.CommandResult == ERROR_ID.RESULT_SUCCESS && paramLen >= 0)
		{
			string param3 = _rba_api.GetParam(PARAMETER_ID.P17_RES_VAS_DATA);
			if (!string.IsNullOrWhiteSpace(param3))
			{
				list.Add(param3);
			}
			paramLen = _rba_api.GetParamLen(PARAMETER_ID.P17_RES_VAS_DATA);
		}
		vasResult.Data = list;
		HandleAndLogResult(vasResult.CommandResult, "M17_6SendVasMessage - VasCommand: " + text + "-(" + vasCmd.ToString() + ") - Params: " + string.Join(",", vasParams) + " - " + vasResult.ToString());
		return vasResult;
	}

	private void M09SetAllowedPayment()
	{
		string param = _rba_api.GetParam(PARAMETER_ID.P09_RES_CARD_TYPE);
		string param2 = _rba_api.GetParam(PARAMETER_ID.P09_RES_CARD_STATUS);
		if ((param == "02" || param == "99") && param2 == "I")
		{
			Log("**Card inserted");
			_mre09CardRemoved.Reset();
		}
		else if ((param == "02" || param == "99") && param2 == "R")
		{
			Log("**Card Removed");
			ReadCardContextData?.SendCardRemoved();
			_mre09CardRemoved.Set();
		}
		else if (param2 == "P")
		{
			Log("**Unknown problem with Card Insertion / Removal");
			_mre09CardRemoved.Set();
		}
		else
		{
			Log("**Unknown Card activity");
			_mre09CardRemoved.Set();
		}
	}

	private void M23CardRead()
	{
		string param = _rba_api.GetParam(PARAMETER_ID.P23_RES_EXIT_TYPE);
		Log("P23_RES_EXIT_TYPE: (" + param + ") - " + ExitCodeText(param));
		string param2 = _rba_api.GetParam(PARAMETER_ID.P23_RES_TRACK1);
		Log("P23_RES_TRACK1: " + param2);
		string param3 = _rba_api.GetParam(PARAMETER_ID.P23_RES_TRACK2);
		Log("P23_RES_TRACK2: " + param3);
		string param4 = _rba_api.GetParam(PARAMETER_ID.P23_RES_TRACK3);
		Log("P23_RES_TRACK3: " + param4);
		string param5 = _rba_api.GetParam(PARAMETER_ID.P23_RES_CARD_SOURCE);
		Log("P23_RES_CARD_SOURCE: " + param5);
		CardSourceType cardSource = (ReadCardContextData.CardSource = ParseCardSourceType(param5));
		CardSource = cardSource;
		ReadCardContextData.ExitType = (CardReadExitType)int.Parse(param);
		if (param == "0")
		{
			switch (CardSource)
			{
			case CardSourceType.Swipe:
			case CardSourceType.MSDContactless:
				GetVariable_29("394");
				GetVariable_29("000398");
				ReadCardContextData.ResponseDataModel = new UnencryptedCardReadModel
				{
					Track1 = param2,
					Track2 = param3,
					Track3 = param4
				};
				break;
			case CardSourceType.EMVContact:
			case CardSourceType.QuickChip:
			case CardSourceType.EMVContactless:
			case CardSourceType.Mobile:
			case CardSourceType.VASOnly:
				break;
			}
		}
		else
		{
			CardSource = CardSourceType.Unknown;
			Log("ExitType: " + param);
		}
	}

	public InsertedStatus CheckIfCardInserted()
	{
		_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
		_rba_api.SetParam(PARAMETER_ID.P33_01_REQ_STATUS, "00");
		_rba_api.SetParam(PARAMETER_ID.P33_01_REQ_EMVH_CURRENT_PACKET_NBR, "0");
		_rba_api.SetParam(PARAMETER_ID.P33_01_REQ_EMVH_PACKET_TYPE, "0");
		ERROR_ID result = _rba_api.ProcessMessage(MESSAGE_ID.M33_01_EMV_STATUS);
		HandleAndLogResult(result, "3301CardStatus");
		return M3301Status();
	}

	private InsertedStatus M3301Status()
	{
		string param = _rba_api.GetParam(PARAMETER_ID.P33_01_RES_F1_CHIP_CARD);
		Log("P33_01_RES_F1_CHIP_CARD: " + param);
		if (param != null && param == "I")
		{
			return InsertedStatus.Inserted;
		}
		return InsertedStatus.Removed;
	}

	private void EMVDataFromDevice(MESSAGE_ID messageId, PARAMETER_ID parameter)
	{
		try
		{
			string param = _rba_api.GetParam(parameter);
			EMVCardReadModel eMVCardReadModel = new EMVCardReadModel
			{
				CardSource = CardSource
			};
			byte[] data = new byte[1];
			if (param == "E")
			{
				string text = $"Error when retrieving status when Getting Parameter {parameter} from EMV Message {messageId}";
				eMVCardReadModel.AddError("D001", text);
				Log(text);
				ReadCardContextData.UpdateStopStateFor(VasMode.PayOnly);
			}
			ParseTags parseTags = new ParseTags();
			while (true)
			{
				int tagParamLen = _rba_api.GetTagParamLen(messageId);
				if (tagParamLen <= 0)
				{
					break;
				}
				int tagParam = _rba_api.GetTagParam(messageId, out data);
				string text2 = ByteArrayToString(data);
				string text3 = tagParam.ToString("X");
				if (!_ingenicoTags.Contains(text3))
				{
					eMVCardReadModel.Tags.Add(text3, text2);
				}
				parseTags.ParseEMVTags(text3, tagParamLen, text2, data, CardSource, Log, ReadCardContextData?.Errors);
			}
			if (parseTags.Tag9F27CryptogramDeclined)
			{
				ReadCardContextData.Errors.Add(new Error
				{
					Code = "EMVCardDecline",
					Message = "Card Declined Transaction."
				});
				ReadCardContextData.UpdateStopStateFor(VasMode.PayOnly);
			}
			if (!string.IsNullOrEmpty(parseTags.ErrorCode))
			{
				ReadCardContextData.Errors.Add(new Error
				{
					Code = parseTags.ErrorCode,
					Message = "Error During Read."
				});
				ReadCardContextData.UpdateStopStateFor(VasMode.PayOnly);
			}
			eMVCardReadModel.FirstSix = parseTags.First6;
			eMVCardReadModel.LastFour = parseTags.Last4;
			eMVCardReadModel.AID = parseTags.AID;
			eMVCardReadModel.MfgSerialNumber = UnitData.ManufacturingSerialNumber;
			if (ReadCardContextData.CardSource == CardSourceType.EMVContactless && parseTags.IsMobileWallet)
			{
				ReadCardContext readCardContextData = ReadCardContextData;
				CardSourceType cardSource = (eMVCardReadModel.CardSource = CardSourceType.Mobile);
				readCardContextData.CardSource = cardSource;
			}
			if (ParseOnGuardData(parseTags.FF1FOnGuardCardData) is EncryptedCardReadModel encryptedCardReadModel)
			{
				eMVCardReadModel.AESPAN = encryptedCardReadModel.AESPAN;
				eMVCardReadModel.AESPANLength = encryptedCardReadModel.AESPANLength;
				eMVCardReadModel.EncFormat = encryptedCardReadModel.EncFormat;
				eMVCardReadModel.EncryptedFlag = encryptedCardReadModel.EncryptedFlag;
				IEnumerable<Error> errors = encryptedCardReadModel.Errors;
				if (errors != null && errors.Any())
				{
					eMVCardReadModel.AddErrors(encryptedCardReadModel.Errors);
				}
				eMVCardReadModel.Expiry = encryptedCardReadModel.Expiry;
				eMVCardReadModel.ExtLangCode = encryptedCardReadModel.ExtLangCode;
				eMVCardReadModel.FirstSix = encryptedCardReadModel.FirstSix;
				eMVCardReadModel.ICEncData = encryptedCardReadModel.ICEncData;
				eMVCardReadModel.ICEncDataLength = encryptedCardReadModel.ICEncDataLength;
				eMVCardReadModel.InjectedSerialNumber = encryptedCardReadModel.InjectedSerialNumber;
				eMVCardReadModel.KSN = encryptedCardReadModel.KSN;
				eMVCardReadModel.LanguageCode = encryptedCardReadModel.LanguageCode;
				eMVCardReadModel.LastFour = encryptedCardReadModel.LastFour;
				eMVCardReadModel.LSEncData = encryptedCardReadModel.LSEncData;
				eMVCardReadModel.LSEncDataLength = encryptedCardReadModel.LSEncDataLength;
				eMVCardReadModel.MfgSerialNumber = encryptedCardReadModel.MfgSerialNumber;
				eMVCardReadModel.Mod10CheckFlag = encryptedCardReadModel.Mod10CheckFlag;
				eMVCardReadModel.Name = encryptedCardReadModel.Name;
				eMVCardReadModel.NameLength = encryptedCardReadModel.NameLength;
				eMVCardReadModel.PANLength = encryptedCardReadModel.PANLength;
				eMVCardReadModel.ServiceCode = encryptedCardReadModel.ServiceCode;
			}
			eMVCardReadModel.Name = parseTags.CardHolderName;
			eMVCardReadModel.NameLength = parseTags.CardHolderName?.Length ?? 0;
			eMVCardReadModel.FallbackStatusAction = parseTags.Fallback;
			eMVCardReadModel.FallbackReason = parseTags.FallbackReason;
			eMVCardReadModel.ErrorCode = parseTags.ErrorCode;
			if (!string.IsNullOrWhiteSpace(ReadCardContextData.ResponseDataModel?.Name))
			{
				eMVCardReadModel.Name = ReadCardContextData.ResponseDataModel.Name;
				eMVCardReadModel.NameLength = ReadCardContextData.ResponseDataModel.NameLength;
			}
			ReadCardContextData.ResponseDataModel = eMVCardReadModel;
			ReadCardContextData.CardBrand = parseTags.CardBrand;
			_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
		}
		catch (Exception e)
		{
			LogException("EMVDataFromDevice", e);
			ReadCardContextData.Errors.Add(new Error
			{
				Code = "EMVDataError",
				Message = "Error Reading EMV Data From Device."
			});
			ReadCardContextData.UpdateStopStateFor(VasMode.PayOnly);
		}
	}

	private bool M87e2eeCardRead()
	{
		Log("87 E2EE Card Read Message Received");
		try
		{
			char value = '\u001c';
			string param = _rba_api.GetParam(PARAMETER_ID.P87_RES_EXIT_TYPE);
			ReadCardContextData.ExitType = (int.TryParse(param, out var result) ? ((CardReadExitType)result) : CardReadExitType.InvalidExitCode);
			Log("P87_RES_EXIT_TYPE: (" + param + ") - " + ExitCodeText(param));
			string param2 = _rba_api.GetParam(PARAMETER_ID.P87_RES_CARD_SOURCE);
			CardSourceType cardSource = (ReadCardContextData.CardSource = ParseCardSourceType(param2));
			CardSource = cardSource;
			Log($"P87_RES_CARD_SOURCE: ({param2}) - {ReadCardContextData.CardSource}");
			if (param != "0")
			{
				return false;
			}
			if (string.IsNullOrWhiteSpace(param2))
			{
				Log("Could not detect card source, assuming source as MSD");
			}
			else
			{
				switch (ReadCardContextData.CardSource)
				{
				case CardSourceType.VASOnly:
					_mre09CardRemoved.Set();
					return true;
				case CardSourceType.Swipe:
				case CardSourceType.MSDContactless:
					break;
				case CardSourceType.EMVContactless:
					Log($"CardType: EMV Contactless Data. - {ReadCardContextData.CardSource}");
					_mre09CardRemoved.Set();
					return true;
				case CardSourceType.EMVContact:
				case CardSourceType.QuickChip:
					Log($"CardType: EMV Data. - {ReadCardContextData.CardSource}");
					if (ReadCardContextData.IsVasEnabled && ReadCardContextData.CanContinue(VasMode.VasOnly))
					{
						ReadCardContextData.UpdateStopStateFor(VasMode.VasOnly);
					}
					_mre09CardRemoved.Reset();
					return true;
				default:
					ReadCardContextData.Errors.Add(new Error
					{
						Code = "E2EEICS",
						Message = "Invalid Card Source When Reading Card Read Data."
					});
					_mre09CardRemoved.Set();
					Log($"Unhandled Card Source Type: {ReadCardContextData.CardSource}");
					ReadCardContextData.Errors.Add(new Error
					{
						Code = "UnknownCardSource",
						Message = $"Unhandled Card Source Type: {ReadCardContextData.CardSource}"
					});
					ReadCardContextData.UpdateStopStateFor(VasMode.VasAndPay);
					return false;
				}
				if (ReadCardContextData.IsVasEnabled && ReadCardContextData.CanContinue(VasMode.VasOnly))
				{
					ReadCardContextData.UpdateStopStateFor(VasMode.VasOnly);
				}
				_mre09CardRemoved.Set();
			}
			string text = _rba_api.GetParam(PARAMETER_ID.P87_RES_CARD_DATA);
			if (!string.IsNullOrEmpty(text) && text.IndexOf(value) != -1)
			{
				text = text.Replace(value.ToString(), "");
			}
			Log("P87_RES_CARD_DATA: " + text);
			if (text.Length <= 0)
			{
				ReadCardContextData.Errors.Add(new Error
				{
					Code = "SwipeHasNoData",
					Message = "Card Swipe has no data."
				});
				ReadCardContextData.UpdateStopStateFor(VasMode.PayOnly);
				return false;
			}
			Base87CardReadModel responseDataModel = ParseOnGuardData(text);
			ReadCardContextData.ResponseDataModel = responseDataModel;
			return true;
		}
		catch (Exception ex)
		{
			ReadCardContextData.Errors.Add(new Error
			{
				Code = "E2EEUH",
				Message = "Unhandled Exception When Reading Card Read Data."
			});
			Log("Exception occurred" + ex.ToString());
			ReadCardContextData.Errors.Add(new Error
			{
				Code = "87ReadCardUnhandled",
				Message = "Unhandled exception occured in 87 - Read Card Encrypted Response from device."
			});
			ReadCardContextData.UpdateStopStateFor(VasMode.VasAndPay);
		}
		return false;
	}

	private void TerminalMessageHandler(MESSAGE_ID messageId)
	{
		Log($"Device Message: {messageId}");
		switch (messageId)
		{
		case MESSAGE_ID.M00_OFFLINE:
			_areOfflineResponse.Set();
			break;
		case MESSAGE_ID.M09_SET_ALLOWED_PAYMENT:
			M09SetAllowedPayment();
			break;
		case MESSAGE_ID.M16_CONTACTLESS_MODE:
			M16ContactlessMode();
			_mre166VasResponse.Set();
			break;
		case MESSAGE_ID.M17_MERCHANT_DATA_WRITE:
			M17_MERCHANT_DATA_WRITE();
			_mre17MerchantDataWriteResponse.Set();
			break;
		case MESSAGE_ID.M23_CARD_READ:
			M23CardRead();
			_mreInsertCard.Set();
			break;
		case MESSAGE_ID.M33_01_EMV_STATUS:
			M3301Status();
			break;
		case MESSAGE_ID.M33_02_EMV_TRANSACTION_PREPARATION_RESPONSE:
			EMVDataFromDevice(messageId, PARAMETER_ID.P33_02_RES_STATUS);
			_mre3302TransactionPrepareResponse.Set();
			break;
		case MESSAGE_ID.M33_03_EMV_AUTHORIZATION_REQUEST:
			EMVDataFromDevice(messageId, PARAMETER_ID.P33_03_REQ_STATUS);
			_mre3302TransactionPrepareResponse.Set();
			_mre3303AuthorizationRequest.Set();
			break;
		case MESSAGE_ID.M33_05_EMV_AUTHORIZATION_CONFIRMATION:
			EMVDataFromDevice(messageId, PARAMETER_ID.P33_05_RES_STATUS);
			_mre3302TransactionPrepareResponse.Set();
			_mre3303AuthorizationRequest.Set();
			_mre3305AuthorizationConfirmation.Set();
			break;
		case MESSAGE_ID.M87_E2EE_CARD_READ:
			_mre166VasResponse.Set();
			M87e2eeCardRead();
			_mreInsertCard.Set();
			_mre166VasResponse.Set();
			break;
		default:
			Log($"UNHANDLED MESSAGEID: {messageId}");
			break;
		}
	}

	private void M17_MERCHANT_DATA_WRITE()
	{
		string param = _rba_api.GetParam(PARAMETER_ID.P17_RES_STATUS);
		_rba_api.GetParam(PARAMETER_ID.P17_RES_TYPE);
		int paramLen = _rba_api.GetParamLen(PARAMETER_ID.P17_RES_VAS_DATA);
		_rba_api.GetParamLen(PARAMETER_ID.P17_RES_DATA);
		List<string> list = new List<string>();
		while (param == "0" && paramLen > 0)
		{
			string param2 = _rba_api.GetParam(PARAMETER_ID.P17_RES_VAS_DATA);
			list.Add(param2);
			paramLen = _rba_api.GetParamLen(PARAMETER_ID.P17_RES_VAS_DATA);
		}
		_rba_api.GetParam(PARAMETER_ID.P17_RES_CLESS_CARD_CMD_ID_FOR_NOTIFY);
		_rba_api.GetParam(PARAMETER_ID.P17_RES_CLESS_CARD_CMD_TYPE);
		_rba_api.GetParam(PARAMETER_ID.P17_RES_CLESS_CARD_DATA);
		_rba_api.GetParam(PARAMETER_ID.P17_RES_CLESS_CARD_IDX);
		_rba_api.GetParam(PARAMETER_ID.P17_RES_CLESS_CARD_NEED_NOTIFY);
		_rba_api.GetParam(PARAMETER_ID.P17_RES_IS_ADDITIONAL_DATA);
		_rba_api.GetParam(PARAMETER_ID.P17_REQ_CLESS_CARD_CMD_TYPE);
		Log("M17_MerchDataWrite - " + string.Join(",", list));
	}

	private void M16ContactlessMode()
	{
		ReadCardContext readCardContextData = ReadCardContextData;
		if (readCardContextData == null)
		{
			LogError("M16 Response - No Context Available.");
			return;
		}
		if (!readCardContextData.State.HasFlag(ReadCardContextState.VasContinue))
		{
			LogError("M16 Response - Vas Flow is in Disabled State.");
			return;
		}
		if (_rba_api.GetParam(PARAMETER_ID.P16_RES_TYPE) != "6")
		{
			Log("Not vas.");
		}
		string param = _rba_api.GetParam(PARAMETER_ID.P16_RES_DATA);
		string param2 = _rba_api.GetParam(PARAMETER_ID.P16_RES_IS_ADDITIONAL_DATA);
		string param3 = _rba_api.GetParam(PARAMETER_ID.P16_RES_STATUS);
		VasMessageStatus vasMessageStatus = (Enum.TryParse(typeof(VasMessageStatus), param, out object result) ? ((VasMessageStatus)result) : VasMessageStatus.VAS_ERROR_UNKNOWN);
		vasMessageStatus = (Enum.IsDefined(typeof(VasMessageStatus), vasMessageStatus) ? vasMessageStatus : VasMessageStatus.VAS_ERROR_UNKNOWN);
		if ((!(param3 == "0") && !(param3 == "6")) || (vasMessageStatus != VasMessageStatus.VAS_STATUS_OK && vasMessageStatus != VasMessageStatus.VAS_STATUS_CANCELLED && vasMessageStatus != VasMessageStatus.VAS_STATUS_COMPLETE_TAP && vasMessageStatus != VasMessageStatus.VAS_STATUS_EXPECT_PAYMENT && vasMessageStatus != VasMessageStatus.VAS_ERROR_MOBILE_DATA_NOT_AVAILABLE))
		{
			ReadCardContextData.UpdateStopStateFor(VasMode.VasOnly);
			if (!readCardContextData.IsPayEnabled)
			{
				readCardContextData.Errors.Add(new Error
				{
					Code = "VAS166",
					Message = $"Failed 16 Command with error {param} - {vasMessageStatus}"
				});
			}
		}
		Log($"data = {param} - vas message status = {vasMessageStatus}, additionalData = {param2}, status = {param3}");
	}

	private string ExitCodeText(string exitType)
	{
		return exitType switch
		{
			"0" => "Good Read", 
			"1" => "Bad Read", 
			"2" => "Cancelled", 
			"3" => "Button Pressed", 
			"4" => "Cless Card Floor Limit Exceeded", 
			"5" => "Max Cless FLoor Limit Exceeded", 
			"6" => "Invalid Prompt", 
			"7" => "Encryption Failed", 
			"8" => "Bad Key card", 
			"9" => "Bad Format of 23", 
			"A" => "Amount was not set and the contactless reader was not enabled", 
			"R" => "At least one specified reader is disabled", 
			_ => "Not performed or unknown exit type", 
		};
	}

	private string ExitCodeText(CardReadExitType exitType)
	{
		return exitType switch
		{
			CardReadExitType.GoodRead => "Good Read", 
			CardReadExitType.BadRead => "Bad Read", 
			CardReadExitType.Cancelled => "Cancelled", 
			CardReadExitType.ButtonPressed => "Button Pressed", 
			CardReadExitType.ClessCardFloorLimitExceeded => "Cless Card Floor Limit Exceeded", 
			CardReadExitType.MaxClessFloorLimitExceeded => "Max Cless FLoor Limit Exceeded", 
			CardReadExitType.InvalidPrompt => "Invalid Prompt", 
			CardReadExitType.EncryptionFailed => "Encryption Failed", 
			CardReadExitType.BadKeyCard => "Bad Key card", 
			CardReadExitType.BadFormatOf23 => "Bad Format of 23", 
			CardReadExitType.AmountWasNotSetAndContactlessReaderWasNotEnabled => "Amount was not set and the contactless reader was not enabled", 
			CardReadExitType.AtLeastOneSpecifiedReaderIsDisabled => "At least one specified reader is disabled", 
			_ => "Not performed or unknown exit type", 
		};
	}

	private CardSourceType ParseCardSourceType(string sourceType)
	{
		return sourceType switch
		{
			"M" => CardSourceType.Swipe, 
			"E" => CardSourceType.EMVContactless, 
			"C" => CardSourceType.MSDContactless, 
			"S" => CardSourceType.EMVContact, 
			"Q" => CardSourceType.QuickChip, 
			"m" => CardSourceType.Mobile, 
			"c" => CardSourceType.VASOnly, 
			"cq" => CardSourceType.VASOnly, 
			_ => CardSourceType.Unknown, 
		};
	}

	public Base87CardReadModel ParseOnGuardData(string cardData)
	{
		if (string.IsNullOrWhiteSpace(cardData))
		{
			Log("ParseOnGuardData -> cardData contains no data.");
			return null;
		}
		try
		{
			Log("**********************************************");
			Log("Parsed OnGuard MSD Data");
			Log("**********************************************");
			int num = 0;
			bool flag = false;
			if (cardData.StartsWith('B'))
			{
				flag = true;
				cardData = cardData.Substring(cardData.LastIndexOf(" ") + 1);
			}
			string text = cardData.Substring(num, 6);
			if (cardData.IndexOf("=") > 20 && text != "601056" && !flag)
			{
				num += 6;
				string text2 = cardData.Substring(num, 4);
				num += 4;
				int.TryParse(cardData.Substring(num, 2), out var result);
				num += 2;
				string text3 = cardData.Substring(num, 1);
				num++;
				string text4 = cardData.Substring(num, 4);
				num += 4;
				string text5 = cardData.Substring(num, 3);
				num += 3;
				string text6 = cardData.Substring(num, 1);
				num++;
				if (!int.TryParse(cardData.Substring(num, 2), out var result2))
				{
					Log("ParseOnGuardData -> Could not find valid Name length");
					return null;
				}
				num += 2;
				string name = cardData.Substring(num, result2);
				num += result2;
				string text7 = cardData.Substring(num, 1);
				bool encryptedFlag = text7 == "1";
				num++;
				if (text7.ToString() == "1")
				{
					Log("First 6: " + text);
					Log("Last 4: " + text2);
					Log("PAN Length: " + result);
					Log("Mod10 check flag: " + text3);
					Log("Service Code: " + text5);
					Log("Language Code: " + text6);
					Log("Cardholder Name Length: " + result2);
					Log("Encrypted Flag: " + text7);
					string text8 = cardData.Substring(num, 1);
					Log("Encryption Format: " + text8);
					num++;
					string kSN = cardData.Substring(num, 20);
					Log("4-digit Extension: " + cardData.Substring(num + 20, 4));
					num += 24;
					int num2 = Convert.ToInt32(cardData.Substring(num, 2));
					Log("IC Encrypted Data Length: " + num2);
					num += 2;
					string iCEncData = cardData.Substring(num, num2);
					num += num2;
					int num3 = Convert.ToInt32(cardData.Substring(num, 2));
					Log("AES Pan Length: " + num3);
					num += 2;
					string aESPAN = cardData.Substring(num, num3);
					num += num3;
					int num4 = Convert.ToInt32(cardData.Substring(num, 2));
					Log("LS Encrypted Data Length: " + num4);
					num += 2;
					string lSEncData = cardData.Substring(num, num4);
					num += num4;
					string text9 = cardData.Substring(num, 1);
					Log("Extended Language Code: " + text9);
					num++;
					Log("**********************************************");
					return new EncryptedCardReadModel
					{
						FirstSix = text,
						LastFour = text2,
						PANLength = result,
						Mod10CheckFlag = text3,
						Expiry = text4,
						ServiceCode = text5,
						LanguageCode = text6,
						EncryptedFlag = encryptedFlag,
						Name = name,
						NameLength = result2,
						EncFormat = text8,
						KSN = kSN,
						ICEncDataLength = num2,
						ICEncData = iCEncData,
						AESPANLength = num3,
						AESPAN = aESPAN,
						LSEncDataLength = num4,
						LSEncData = lSEncData,
						ExtLangCode = text9,
						Status = ResponseStatus.Success,
						MfgSerialNumber = UnitData?.ManufacturingSerialNumber,
						InjectedSerialNumber = UnitData?.UnitSerialNumber,
						CardSource = CardSource,
						FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter
					};
				}
				if (text7.ToString() == "0")
				{
					Log("First 6: " + text);
					Log("Last 4: " + text2);
					Log("PAN Length: " + result);
					Log("Mod10 check flag: " + text3);
					Log("Expiry: " + text4);
					Log("Service Code: " + text5);
					Log("Language Code: " + text6);
					Log("Encrypted Flag: " + text7);
					int num5 = Convert.ToInt32(cardData.Substring(num, 2));
					num += 2;
					Log("Track1 length: " + num5);
					string track = null;
					if (num5 > 0)
					{
						track = cardData.Substring(num, num5);
						num += num5;
					}
					int num6 = Convert.ToInt32(cardData.Substring(num, 2));
					num += 2;
					Log("Track2 length: " + num6);
					string track2 = null;
					if (num6 > 0)
					{
						track2 = cardData.Substring(num, num6);
						num += num6;
					}
					string text10 = cardData.Substring(num, 1);
					Log("Extended Language Code: " + text10);
					num++;
					return new UnencryptedCardReadModel
					{
						FirstSix = text,
						LastFour = text2,
						PANLength = result,
						Mod10CheckFlag = text3,
						Expiry = text4,
						ServiceCode = text5,
						LanguageCode = text6,
						EncryptedFlag = encryptedFlag,
						Name = name,
						Track1 = track,
						Track2 = track2,
						Status = ResponseStatus.Success,
						FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter
					};
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(cardData))
				{
					Log("Card data whitelisted using secbin.dat");
					if (text != "601056")
					{
						return null;
					}
					Log("Card Bin Matches Gift Card");
					int num7 = cardData.IndexOf("=");
					return new UnencryptedCardReadModel
					{
						Track2 = cardData,
						PANLength = num7,
						CardType = CardType.RedboxGiftCard,
						FirstSix = text,
						EncryptedFlag = false,
						SwipeMode = SwipeType.Swipe,
						LastFour = cardData.Substring(num7 - 4, 4),
						Expiry = cardData.Substring(num7 + 1, 4),
						ServiceCode = cardData.Substring(num7 + 5, 3),
						Status = ResponseStatus.Success,
						FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter
					};
				}
				Log("Card Data: Empty");
				Log("**********************************************");
			}
		}
		catch (Exception e)
		{
			LogException("Parse on Guard Data", e);
		}
		return null;
	}

	private bool Offline(ReadCardContext context)
	{
		return Offline();
	}

	private bool Offline(int attempts = 3)
	{
		ERROR_ID eRROR_ID = ERROR_ID.RESULT_ERROR;
		List<ERROR_ID> list = new List<ERROR_ID>();
		while (eRROR_ID != ERROR_ID.RESULT_SUCCESS && attempts-- > 0)
		{
			eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M00_OFFLINE);
			HandleAndLogResult(eRROR_ID, "00 Offline");
			list.Add(eRROR_ID);
			int num = 5250;
			if (eRROR_ID == ERROR_ID.RESULT_SUCCESS && !_areOfflineResponse.WaitOne(num))
			{
				Log($"Offline Not received in wait of {num}");
				return false;
			}
		}
		if (list.All((ERROR_ID x) => x == ERROR_ID.RESULT_ERROR_TIMEOUT))
		{
			Reconnect();
		}
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	private bool SetAmount(ReadCardContext context)
	{
		string amount = (((context?.Request?.Amount).GetValueOrDefault() > 0m) ? ((context?.Request.Amount).Value * 100m).ToString("0") : "100");
		if (!SetAmount(amount))
		{
			context.Errors.Add(new Error
			{
				Code = "RBA001",
				Message = "Set Amount Message Failed."
			});
			context.ResponseDataModel = ((context.CardSource == CardSourceType.EMVContact) ? new EMVCardReadModel() : new EncryptedCardReadModel());
			context.ResponseDataModel.Status = ResponseStatus.Errored;
			return false;
		}
		return true;
	}

	public bool SetAmount(string amount)
	{
		_rba_api.SetParam(PARAMETER_ID.P13_REQ_AMOUNT, amount);
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M13_AMOUNT);
		HandleAndLogResult(eRROR_ID, "13 Set Amount: " + amount);
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	private bool WaitForVasData(ReadCardContext context)
	{
		if (context.IsVasEnabled)
		{
			PerformActionAndCheckSuccess(GetVasData, null, context, VasMode.VasOnly);
			if (!context.CanContinue(VasMode.VasAndPay))
			{
				context.Errors.Add(new Error
				{
					Code = "VASDATA001",
					Message = "Error When Attempting To Get Vas Data."
				});
				if (!context.IsPayEnabled)
				{
					context.ResponseDataModel = new EMVCardReadModel
					{
						Status = ResponseStatus.Errored
					};
				}
				return false;
			}
		}
		return true;
	}

	private bool WaitForCardReadComplete(ReadCardContext context)
	{
		if (!_mreInsertCard.WaitOne(context.TimeRemainingUntilTimeout))
		{
			Log("No Device Interaction.");
			context.Errors.Add(new Error
			{
				Code = "TimeOut",
				Message = "No User interaction."
			});
			context.ResponseDataModel = new EncryptedCardReadModel
			{
				Status = ResponseStatus.Errored
			};
			return false;
		}
		if (context.ExitType != CardReadExitType.GoodRead)
		{
			context.Errors.Add(new Error
			{
				Code = "BadExitType",
				Message = $"({context.ExitType}) - {ExitCodeText(context.ExitType)}"
			});
			Log("Exit Type Not Good Read.");
		}
		if (!context.IsPayEnabled || (((context.CardSource != CardSourceType.MSDContactless && context.CardSource != CardSourceType.Swipe) || context.ResponseDataModel == null) && context.CardSource != CardSourceType.QuickChip && context.CardSource != CardSourceType.EMVContact && context.CardSource != CardSourceType.EMVContactless && context.CardSource != CardSourceType.Mobile))
		{
			if (context.CardSource == CardSourceType.VASOnly)
			{
				return context.IsVasEnabled;
			}
			return false;
		}
		return true;
	}

	private bool WaitForVasReadStart(ReadCardContext context)
	{
		if (context.IsVasEnabled && context.CanContinue(VasMode.VasOnly) && !_mre166VasResponse.WaitOne(context.TimeRemainingUntilTimeout))
		{
			Log("No Device Interaction.");
			context.State = ReadCardContextState.Timeout;
			context.Errors.Add(new Error
			{
				Code = "TimeOut",
				Message = "No User interaction."
			});
			context.ResponseDataModel = new EncryptedCardReadModel
			{
				Status = ResponseStatus.Errored
			};
			return false;
		}
		if (!context.CanContinue(VasMode.VasOnly))
		{
			Log("No Longer in Vas Flow.");
			return false;
		}
		context.SendCardProcessing();
		return true;
	}

	private bool CardReadRequestEncrypted(ReadCardContext context)
	{
		_mreInsertCard.Reset();
		if (!CardReadRequestEncrypted(context.Request.InputType))
		{
			Log("Encrypted Read Failed.");
			context.Errors.Add(new Error
			{
				Code = "RBA001",
				Message = "87 Read Card Encrypted Message Failed."
			});
			context.ResponseDataModel = new EncryptedCardReadModel
			{
				Status = ResponseStatus.Errored
			};
			context.SendCompletedCallback();
			return false;
		}
		return true;
	}

	private bool CardReadRequestEncrypted(DeviceInputType? inputs = DeviceInputType.M)
	{
		if (!inputs.HasValue)
		{
			inputs = DeviceInputType.M;
		}
		string text = "COD.K3Z";
		if (!string.IsNullOrEmpty(text))
		{
			_rba_api.SetParam(PARAMETER_ID.P87_REQ_FORM_NAME, text);
		}
		string description = inputs.GetDescription();
		if (!string.IsNullOrEmpty(description))
		{
			_rba_api.SetParam(PARAMETER_ID.P87_REQ_PROMPT_INDEX, description);
		}
		string text2 = inputs.ToString();
		if (!string.IsNullOrEmpty(text2))
		{
			_rba_api.SetParam(PARAMETER_ID.P87_REQ_ENABLE_DEVICES, text2);
		}
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M87_E2EE_CARD_READ);
		HandleAndLogResult(eRROR_ID, "87 OnGuard Card Read");
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	private bool CardReadRequestUnencrypted(DeviceInputType? inputs = DeviceInputType.M)
	{
		if (!inputs.HasValue)
		{
			inputs = DeviceInputType.M;
		}
		string text = "COD.K3Z";
		if (!string.IsNullOrEmpty(text))
		{
			_rba_api.SetParam(PARAMETER_ID.P23_REQ_FORM_NAME, text);
		}
		string description = inputs.GetDescription();
		if (!string.IsNullOrEmpty(description))
		{
			_rba_api.SetParam(PARAMETER_ID.P23_REQ_PROMPT_INDEX, description);
		}
		string text2 = inputs.ToString();
		if (!string.IsNullOrEmpty(text2))
		{
			_rba_api.SetParam(PARAMETER_ID.P23_REQ_ENABLE_DEVICES, text2);
		}
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M23_CARD_READ);
		HandleAndLogResult(eRROR_ID, "23 Card Read");
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	private string VasModeToIngenicoVasMode(VasMode mode)
	{
		string result = null;
		switch (mode)
		{
		case VasMode.VasOnly:
			result = "0";
			break;
		case VasMode.VasAndPay:
			result = "1";
			break;
		case VasMode.PayOnly:
			result = "2";
			break;
		}
		return result;
	}

	private bool EnableVas(bool enable = true)
	{
		_isVasInitialized = enable;
		return M17_6SendVasMessage(VasCmd.SetEnable, enable ? "1" : "0").Success;
	}

	private bool AddAppleVas(string identifier, ReadCardContext context)
	{
		if (context?.Request.AppleVasUrl == null)
		{
			_ = string.Empty;
		}
		return M17_6SendVasMessage(VasCmd.AddAppleMerchantId, identifier, "tbd", "0100000000", "1").Success;
	}

	private bool AddGoogleVas(string identifier, ReadCardContext context)
	{
		return M17_6SendVasMessage(VasCmd.AddGoogleMerchantId, identifier, "PLACE03", "TRMNL01", "Google Store", "en", "1", "0").Success;
	}

	private void InitializeVas()
	{
		if (SupportsVas)
		{
			EnableVas();
			AddAppleVas("pass.com.apple.wallet.vas.prodtest", null);
			AddAppleVas("pass.com.ingenico.us.vas.test", null);
			AddAppleVas("pass.com.redbox.kiosk", null);
			AddGoogleVas("70779451", null);
		}
	}

	private bool PrepareVas(ReadCardContext context)
	{
		if (!SupportsVas)
		{
			if (_isVasInitialized)
			{
				EnableVas(enable: false);
			}
			return true;
		}
		if (!_isVasInitialized && !EnableVas())
		{
			return false;
		}
		string text = VasModeToIngenicoVasMode((context?.Request?.VasMode).GetValueOrDefault());
		if (context != null && context.IsVasEnabled)
		{
			M17_6SendVasMessage(VasCmd.SetMode, text);
			M17_6SendVasMessage(VasCmd.ClearAppleMerchantList);
			AddAppleVas(IsProdEnvironment ? "pass.com.vibes.redbox" : "pass.com.vibes.RedboxQAEnvironment", context);
			VasResult vasResult = M17_6SendVasMessage(VasCmd.GetAppleMerchantCount);
			AddGoogleVas(IsProdEnvironment ? "44813557" : "70779451", context);
			VasResult vasResult2 = M17_6SendVasMessage(VasCmd.GetGoogleMerchantCount);
			Log("Apple: " + vasResult.VasDataResult + " - Google: " + vasResult2.VasDataResult);
		}
		else
		{
			M17_6SendVasMessage(VasCmd.SetMode, text);
		}
		return true;
	}

	private bool GetVasData(ReadCardContext context)
	{
		if (!context.CanContinue(VasMode.VasOnly))
		{
			return true;
		}
		VasResult vasResult = M17_6SendVasMessage(VasCmd.GetData);
		context.VasData = vasResult.GetVasData(context);
		if (!context.IsPayEnabled)
		{
			if (vasResult.Success)
			{
				return !string.IsNullOrWhiteSpace(context.VasData);
			}
			return false;
		}
		return true;
	}

	private void SetVariable_28(string varID, string varData)
	{
		_rba_api.SetParam(PARAMETER_ID.P28_REQ_RESPONSE_TYPE, "1");
		_rba_api.SetParam(PARAMETER_ID.P28_REQ_VARIABLE_ID, varID);
		_rba_api.SetParam(PARAMETER_ID.P28_REQ_VARIABLE_DATA, varData);
		ERROR_ID result = _rba_api.ProcessMessage(MESSAGE_ID.M28_SET_VARIABLE);
		string param = _rba_api.GetParam(PARAMETER_ID.P28_RES_STATUS);
		string statusText = GetStatusText(param);
		HandleAndLogResult(result, "28 Set Variable ID: " + varID + ", Data: " + varData + " - P28_RES_STATUS: " + param + " - " + statusText + " -");
	}

	public string GetVariable_29(string varID)
	{
		_rba_api.SetParam(PARAMETER_ID.P29_REQ_VARIABLE_ID, varID);
		ERROR_ID result = _rba_api.ProcessMessage(MESSAGE_ID.M29_GET_VARIABLE);
		string param = _rba_api.GetParam(PARAMETER_ID.P29_RES_STATUS);
		string statusText = GetStatusText(param);
		string param2 = _rba_api.GetParam(PARAMETER_ID.P29_RES_VARIABLE_DATA);
		HandleAndLogResult(result, "29 Get Variable ID: " + varID + ", Data: " + param2 + " - P29_RES_STATUS: (" + param + ") - " + statusText + " -");
		return param2;
	}

	private static string GetStatusText(string status)
	{
		string result = string.Empty;
		switch (status)
		{
		case "2":
			result = "Success";
			break;
		case "3":
			result = "Error";
			break;
		case "4":
			result = "Insufficient Memory";
			break;
		case "5":
			result = "Invalid Id";
			break;
		case "6":
			result = "No Data";
			break;
		}
		return result;
	}

	private bool EMVTransInitiation(ReadCardContext context)
	{
		if (!EMVTransInitiation() || !_mre3302TransactionPrepareResponse.WaitOne(20000))
		{
			Log("Failed EMV Transaction Initiation or Timed out waiting for response.");
			context.Errors.Add(new Error
			{
				Code = "RBA001",
				Message = "EMV Initiate Message Failed."
			});
			context.ResponseDataModel = new EMVCardReadModel
			{
				Status = ResponseStatus.Errored
			};
			context.SendCompletedCallback();
			return false;
		}
		if (_mre3305AuthorizationConfirmation.WaitOne(0))
		{
			Log("Bad EMV Read Or Error.");
			context.Errors.Add(new Error
			{
				Code = "EMV001",
				Message = "EMV Data Issue."
			});
			context.SendCompletedCallback();
			return false;
		}
		return true;
	}

	private bool EMVTransInitiation()
	{
		_rba_api.SetParam(PARAMETER_ID.P33_00_REQ_STATUS, "00");
		_rba_api.SetParam(PARAMETER_ID.P33_00_REQ_EMVH_CURRENT_PACKET_NBR, "0");
		_rba_api.SetParam(PARAMETER_ID.P33_00_REQ_EMVH_PACKET_TYPE, "0");
		_rba_api.SetParam(PARAMETER_ID.P33_00_REQ_RESEND_TIMER, "5000");
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M33_00_EMV_TRANSACTION_INITIATION);
		HandleAndLogResult(eRROR_ID, "33.00 EMV Transaction Initiation");
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	private bool SetPaymentType_04(ReadCardContext context)
	{
		if (!SetPaymentType_04("B", "00"))
		{
			context.Errors.Add(new Error
			{
				Code = "RBA001",
				Message = "Set Payment Message Failed."
			});
			context.ResponseDataModel = new EMVCardReadModel
			{
				Status = ResponseStatus.Errored
			};
			context.SendCompletedCallback();
			return false;
		}
		return true;
	}

	private bool SetPaymentType_04(string paymentType, string amount)
	{
		_rba_api.SetParam(PARAMETER_ID.P04_REQ_FORCE_PAYMENT_TYPE, "0");
		_rba_api.SetParam(PARAMETER_ID.P04_REQ_PAYMENT_TYPE, paymentType);
		_rba_api.SetParam(PARAMETER_ID.P04_REQ_AMOUNT, amount);
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M04_SET_PAYMENT_TYPE);
		Log($"ProcessMessage 04. {eRROR_ID}");
		string param = _rba_api.GetParam(PARAMETER_ID.P04_RES_FORCE_PAYMENT_TYPE);
		string text = string.Empty;
		switch (param)
		{
		case "0":
			text = "Success";
			break;
		case "1":
			text = "Failed";
			break;
		case "2":
			text = "Changed Customer Selection";
			break;
		}
		Log("P04_RES_FORCE_PAYMENT_TYPE: (" + param + ") - " + text);
		string param2 = _rba_api.GetParam(PARAMETER_ID.P04_RES_PAYMENT_TYPE);
		string text2 = "Unknown";
		switch (param2)
		{
		case "A":
			text2 = "Debit";
			break;
		case "B":
			text2 = "Credit";
			break;
		case "C":
			text2 = "EBT Cash";
			break;
		case "D":
			text2 = "ET Food Stamps";
			break;
		case "E":
			text2 = "Store Charge";
			break;
		case "F":
			text2 = "Loyalty";
			break;
		case "G":
			text2 = "PayPal";
			break;
		}
		Log("P04_RES_PAYMENT_TYPE: (" + param2 + ") - " + text2);
		Log("P04_RES_AMOUNT: " + _rba_api.GetParam(PARAMETER_ID.P04_RES_AMOUNT));
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	private void SendAuthorizationReponse(bool success = true)
	{
		_rba_api.SetParam(PARAMETER_ID.P33_04_RES_STATUS, "00");
		_rba_api.SetParam(PARAMETER_ID.P33_04_RES_EMVH_CURRENT_PACKET_NBR, "0");
		_rba_api.SetParam(PARAMETER_ID.P33_04_RES_EMVH_PACKET_TYPE, "0");
		_rba_api.AddTagParam(MESSAGE_ID.M33_04_EMV_AUTHORIZATION_RESPONSE, 138, success ? "00" : "05");
		ERROR_ID result = _rba_api.ProcessMessage(MESSAGE_ID.M33_04_EMV_AUTHORIZATION_RESPONSE);
		HandleAndLogResult(result, $"33.04 EMV Auth Response: {success}");
	}

	public bool ReadConfig(string group, string index, out string Data, bool sendOffline = true)
	{
		if (sendOffline)
		{
			Offline();
		}
		_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
		_rba_api.SetParam(PARAMETER_ID.P61_REQ_GROUP_NUM, group);
		_rba_api.SetParam(PARAMETER_ID.P61_REQ_INDEX_NUM, index);
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M61_CONFIGURATION_READ);
		string param = _rba_api.GetParam(PARAMETER_ID.P61_RES_STATUS);
		Data = _rba_api.GetParam(PARAMETER_ID.P61_RES_DATA_CONFIG_PARAMETER);
		HandleAndLogResult(eRROR_ID, "61 Configuration Read Group#: " + group + ", Index#: " + index + ", Data: " + Data + " - P61_RES_STATUS: (" + param + ") - " + ConfigStatusText(param) + " -");
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	public bool WriteConfig(string group, string index, string value, bool sendOffline = true)
	{
		if (sendOffline)
		{
			Offline();
		}
		_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
		_rba_api.SetParam(PARAMETER_ID.P60_REQ_GROUP_NUM, group.PadLeft(4, '0'));
		_rba_api.SetParam(PARAMETER_ID.P60_REQ_INDEX_NUM, index.PadLeft(4, '0'));
		_rba_api.SetParam(PARAMETER_ID.P60_REQ_DATA_CONFIG_PARAM, value);
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M60_CONFIGURATION_WRITE);
		string param = _rba_api.GetParam(PARAMETER_ID.P60_RES_STATUS);
		HandleAndLogResult(eRROR_ID, "60 Configuration Write Group#: " + group + ", Index#: " + index + ", Data: " + value + " - P60_RES_STATUS: (" + param + ") - " + ConfigStatusText(param) + " -");
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	private static string ConfigStatusText(string statusNum)
	{
		string result = string.Empty;
		switch (statusNum)
		{
		case "1":
			result = "Unknown Error";
			break;
		case "2":
			result = "Success";
			break;
		case "3":
			result = "Error, One Or More Parameters Have An Invalid Id";
			break;
		case "4":
			result = "Error, One Or More Paramaters Were Not Updated";
			break;
		case "5":
			result = "Message Rejected, Wrong Message Format";
			break;
		case "9":
			result = "Message Rejected, Cannot Be Executed";
			break;
		}
		return result;
	}

	public bool Reboot(RebootType rebootType = RebootType.Manual)
	{
		_rebootType = rebootType;
		return RebootInner();
	}

	private bool RebootInner()
	{
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M97_REBOOT);
		HandleAndLogResult(eRROR_ID, "97 Reboot");
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	private static EncodedData GetEncodedData(Stream ist)
	{
		EncodedData encodedData = new EncodedData();
		List<byte> list = new List<byte>(4074);
		byte item = byte.MaxValue;
		int num = -1;
		do
		{
			byte b;
			try
			{
				int num2 = ist.ReadByte();
				if (num2 == num)
				{
					encodedData.Result = EncodedResult.EndOfFile;
					encodedData.EncData = list.ToArray();
					return encodedData;
				}
				b = (byte)(num2 & 0xFF);
				encodedData.DataLength++;
			}
			catch (IOException)
			{
				encodedData.Result = EncodedResult.Error;
				return encodedData;
			}
			if (b >= 0 && b <= 31)
			{
				b += 32;
				list.Add(item);
				list.Add(b);
			}
			else if (b >= 32 && b <= 254)
			{
				list.Add(b);
			}
			else if (b == byte.MaxValue)
			{
				list.Add(item);
				list.Add(item);
			}
		}
		while (list.Count < 4063);
		encodedData.EncData = list.ToArray();
		return encodedData;
	}

	private bool WriteData(EncodedData data, string fileName, WriteFileRecordType recordType, int segmentNumber, int retryOnTimeout = 3)
	{
		ERROR_ID eRROR_ID = ERROR_ID.RESULT_ERROR;
		UnpackFlag unpackFlag = UnpackFlag.WriteFileDirectly;
		while (eRROR_ID != ERROR_ID.RESULT_SUCCESS && retryOnTimeout-- > 0)
		{
			_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
			int num = (int)recordType;
			string data2 = num.ToString();
			_rba_api.SetParam(PARAMETER_ID.P62_REQ_RECORD_TYPE, data2);
			_rba_api.SetParam(PARAMETER_ID.P62_REQ_ENCODING_FORMAT, "8");
			string data3 = $"{segmentNumber:D2}";
			_rba_api.SetParam(PARAMETER_ID.P62_REQ_SEGMENT_NBR, data3);
			num = (int)unpackFlag;
			string data4 = num.ToString();
			_rba_api.SetParam(PARAMETER_ID.P62_REQ_UNPACK_FLAG, data4);
			_rba_api.SetParam(PARAMETER_ID.P62_REQ_FAST_DOWNLOAD, "1");
			if (recordType == WriteFileRecordType.NewFileFull || recordType == WriteFileRecordType.NewFileInitialMulti)
			{
				_rba_api.SetParam(PARAMETER_ID.P62_REQ_FILE_NAME, fileName);
			}
			_rba_api.SetParam(PARAMETER_ID.P62_REQ_FILE_DATA, data.EncData);
			_rba_api.SetParam(PARAMETER_ID.P62_REQ_LAST_MESSAGE_TIMEOUT_SEC, "180");
			eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.M62_FILE_WRITE);
			HandleAndLogResult(eRROR_ID, $"62 FileWrite: {fileName} - Record Type: {recordType} - Segment: {segmentNumber} - Length: {data.DataLength} - Result Status: {_rba_api.GetParam(PARAMETER_ID.P62_RES_STATUS)}");
		}
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	private WriteFileRecordType GetWriteFileRecordType(bool firstFile, bool lastFile)
	{
		if (firstFile & lastFile)
		{
			return WriteFileRecordType.NewFileFull;
		}
		if (firstFile)
		{
			return WriteFileRecordType.NewFileInitialMulti;
		}
		if (lastFile)
		{
			return WriteFileRecordType.LastRecord;
		}
		return WriteFileRecordType.Continuation;
	}

	private static Stream ReduceRkiStream(StreamReader sr, string serialNumber)
	{
		string text;
		while (sr != null && !sr.EndOfStream && !string.IsNullOrWhiteSpace(text = sr?.ReadLine()))
		{
			if (text.StartsWith(serialNumber))
			{
				return new MemoryStream(Encoding.UTF8.GetBytes(text));
			}
		}
		return null;
	}

	private async Task<bool> WriteStreamInner(Stream stream, string fileName, bool rebootAfterWrite)
	{
		if (string.IsNullOrEmpty(fileName) || fileName.Length > 15)
		{
			LogError($"Must have a filename with 1-15 characters.  FileName: {fileName} has {fileName.Length} characters.");
			return false;
		}
		Log($"Write Steam File: {fileName} - Length: {stream?.Length ?? 0} bytes - reboot: {rebootAfterWrite}");
		if (!stream.CanRead)
		{
			LogError("WriteFileInner Can't Read Stream for " + fileName);
			return false;
		}
		if (Path.GetExtension(fileName).ToUpper() == ".RKI")
		{
			stream = ReduceRkiStream(new StreamReader(stream), UnitData?.UnitSerialNumber);
			if (stream == null)
			{
				return true;
			}
		}
		bool flag = true;
		_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
		bool firstFile = true;
		bool flag2 = true;
		int num = 1;
		long num2 = 0L;
		while (flag2)
		{
			EncodedData encodedData = GetEncodedData(stream);
			num2 += encodedData.DataLength;
			switch (encodedData.Result)
			{
			case EncodedResult.Success:
			{
				WriteFileRecordType writeFileRecordType2 = GetWriteFileRecordType(firstFile, lastFile: false);
				if (!WriteData(encodedData, fileName, writeFileRecordType2, num))
				{
					LogError("WriteData Failed.");
					return false;
				}
				break;
			}
			case EncodedResult.EndOfFile:
			{
				flag2 = false;
				WriteFileRecordType writeFileRecordType = GetWriteFileRecordType(firstFile, lastFile: true);
				if (!WriteData(encodedData, fileName, writeFileRecordType, num))
				{
					LogError("WriteData Failed.");
					return false;
				}
				break;
			}
			case EncodedResult.Error:
				Log("Aborting File Transfer of " + fileName + ".  Error During GetEncodedData.");
				WriteData(encodedData, fileName, WriteFileRecordType.AbortRequest, num);
				return false;
			}
			Log($"{num2} of {stream?.Length ?? 0} bytes Written.");
			firstFile = false;
			num++;
			if (num % 100 == 0)
			{
				num = 1;
			}
		}
		if (rebootAfterWrite)
		{
			_rebootType = RebootType.FileUpdate;
		}
		bool flag3 = RebootInner();
		string arg = ((!rebootAfterWrite) ? string.Empty : $"- Reboot Succesful: {flag3}");
		Log($"File {fileName} Transfered Successfully: {flag} {arg}");
		return flag & flag3;
	}

	private async Task<bool> WriteFileInner(string fullPath, bool rebootAfterWrite)
	{
		Log($"Write File: {fullPath}, reboot: {rebootAfterWrite}");
		string fileName = Path.GetFileName(fullPath);
		if (Path.GetExtension(fullPath).ToUpper() == ".OGZ")
		{
			rebootAfterWrite = false;
		}
		_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
		_rba_api.SetParam(PARAMETER_ID.P62_REQ_RECORD_TYPE, "0");
		_rba_api.SetParam(PARAMETER_ID.P62_REQ_ENCODING_FORMAT, "8");
		_rba_api.SetParam(PARAMETER_ID.P62_REQ_UNPACK_FLAG, $"{1}");
		_rba_api.SetParam(PARAMETER_ID.P62_REQ_FAST_DOWNLOAD, "1");
		_rba_api.SetParam(PARAMETER_ID.P62_REQ_OS_FILE_NAME, fullPath);
		_rba_api.SetParam(PARAMETER_ID.P62_REQ_FILE_NAME, fileName);
		_rba_api.SetParam(PARAMETER_ID.P62_REQ_LAST_MESSAGE_TIMEOUT_SEC, "180");
		ERROR_ID eRROR_ID = _rba_api.ProcessMessage(MESSAGE_ID.FILE_WRITE);
		HandleAndLogResult(eRROR_ID, "62 FileWrite");
		Log("P62_RES_STATUS " + _rba_api.GetParam(PARAMETER_ID.P62_RES_STATUS));
		Log("P62_RES_FILE_LENGTH " + _rba_api.GetParam(PARAMETER_ID.P62_RES_FILE_LENGTH));
		bool flag = true;
		if ((eRROR_ID == ERROR_ID.RESULT_SUCCESS) & rebootAfterWrite)
		{
			flag = RebootInner();
		}
		return (eRROR_ID == ERROR_ID.RESULT_SUCCESS) & flag;
	}

	public bool Disconnect()
	{
		ERROR_ID eRROR_ID;
		try
		{
			eRROR_ID = _rba_api.Disconnect();
		}
		catch (Exception e)
		{
			LogException("Discconnect", e);
			return false;
		}
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	public bool Reconnect()
	{
		Log("Reconnecting to Device.");
		if (IsConnected && !Disconnect())
		{
			Log("Failed To Disconnect");
			return false;
		}
		return Connect();
	}

	public bool PingDevice()
	{
		return SendCustomMessage("PING");
	}

	private bool SendCustomMessage(string message, bool waitForResponse = false)
	{
		ERROR_ID eRROR_ID = _rba_api.SendCustomMessage(message, waitForResponse);
		HandleAndLogResult(eRROR_ID, "SendCustomMessage " + message);
		return eRROR_ID == ERROR_ID.RESULT_SUCCESS;
	}

	public void ReadCancel()
	{
		if (ReadCardContextData.CanContinue(VasMode.VasAndPay))
		{
			ReadCardContextData.State = ReadCardContextState.Canceled;
		}
		_mre.Set();
		_mreInsertCard.Set();
		_mre09CardRemoved.Set();
		_mre3302TransactionPrepareResponse.Set();
		_mre3303AuthorizationRequest.Set();
		_mre3304AuthorizationResponse.Set();
		_mre3305AuthorizationConfirmation.Set();
		_mre166VasResponse.Set();
	}

	private void InitializeReadCard()
	{
		Log("Initialize Read Card");
		_mre.Reset();
		_mreInsertCard.Reset();
		_mre09CardRemoved.Reset();
		_mre3302TransactionPrepareResponse.Reset();
		_mre3303AuthorizationRequest.Reset();
		_mre3304AuthorizationResponse.Reset();
		_mre3305AuthorizationConfirmation.Reset();
		_mre166VasResponse.Reset();
		CardSource = CardSourceType.Unknown;
	}

	private RebootStatus GetRebootStatus()
	{
		DateTime? disconnectTime = _currentRebootStatus?.DisconnectTime;
		_currentRebootStatus = new RebootStatus
		{
			ExpectedTime = GetExpectedRebootTime(),
			DeviceTime = GetDeviceTime(),
			Id = _unitData?.UnitSerialNumber,
			DisconnectTime = disconnectTime,
			Type = _rebootType,
			KioskId = (_kioskData?.KioskId ?? 0)
		};
		_logger?.LogInformation("Reboot Status: " + JsonConvert.SerializeObject(_currentRebootStatus));
		return _currentRebootStatus;
	}

	private DateTime? GetDeviceTime()
	{
		string text = "";
		try
		{
			text = GetVariable_29("202") + GetVariable_29("201");
			return DateTime.ParseExact(text, "MMddyyHHmmss", CultureInfo.InvariantCulture);
		}
		catch
		{
			_logger?.LogInformation("GetDeviceTime: Could not parse date from deviceTimeString - '" + text + "'");
			return null;
		}
	}

	public DateTime? GetExpectedRebootTime()
	{
		DateTime configRebootTime = GetConfigRebootTime();
		DateTime deviceRebootTime = DeviceRebootTime;
		if (configRebootTime == default(DateTime))
		{
			return deviceRebootTime;
		}
		if (deviceRebootTime == default(DateTime))
		{
			return configRebootTime;
		}
		return (DateTime.Compare(configRebootTime, deviceRebootTime) < 0) ? configRebootTime : deviceRebootTime;
	}

	private DateTime GetConfigRebootTime()
	{
		if (_settings == null)
		{
			return DateTime.Today;
		}
		DateTime dateTime = DateTime.Today + DataFileHelper.GetFileData<TimeSpan>(_settings.DataFilePath, "RebootTime.json", LogException);
		DateTime now = DateTime.Now;
		if (dateTime < now - REBOOT_THRESHOLD)
		{
			dateTime = dateTime.AddDays(1.0);
		}
		return dateTime;
	}

	private async Task StartRebootCountdown()
	{
		if (_rebootTask == null || _rebootTask.IsCompleted)
		{
			await Task.Delay((int)(GetConfigRebootTime() - DateTime.Now).TotalMilliseconds);
			Reboot(RebootType.Scheduled);
		}
	}

	public async Task PostDeviceStatus()
	{
		KioskData kioskData = _kioskData;
		int num;
		if (kioskData == null)
		{
			num = 0;
		}
		else
		{
			_ = kioskData.KioskId;
			num = 1;
		}
		DeviceStatus deviceStatus = ((num == 0) ? null : (await CheckDeviceStatus(_kioskData.KioskId)));
		DeviceStatus deviceStatus2 = deviceStatus;
		if (deviceStatus2 != null)
		{
			_statusClient?.PostDeviceStatus(deviceStatus2);
		}
	}

	public async Task<DeviceStatus> CheckDeviceStatus(long kioskId)
	{
		DeviceStatus deviceStatus = new DeviceStatus
		{
			Id = kioskId,
			Serial = _unitData?.UnitSerialNumber,
			MfgSerial = _unitData?.ManufacturingSerialNumber,
			RBA = _unitData?.GetApplicationVersionString(),
			Assembly = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
			EMVClessVersion = EMVClessVersion?.ToString(),
			EMVContactVersion = EMVContactVersion?.ToString(),
			LocalTime = DateTime.Now,
			RevisionNumber = GetFileUpdateRevisionNumber(),
			ErrorState = ErrorState,
			InjectedKeys = GetKeys()
		};
		deviceStatus.SupportsVas = SupportsVas;
		if (TryGetCurrentTgzVersion(out var version))
		{
			deviceStatus.TgzVersion = version?.ToString();
		}
		Log("Status Check Result: " + JsonConvert.SerializeObject(deviceStatus));
		return deviceStatus;
	}

	private string GetKeys()
	{
		Log("Begin GetKeys");
		lock (_getKeysLock)
		{
			if (!IsConnected || _unitData.IsTampered)
			{
				return null;
			}
			if (string.IsNullOrWhiteSpace(_keys))
			{
				_keys = GetVariable_29("810");
			}
			if (string.IsNullOrWhiteSpace(_keys))
			{
				return null;
			}
		}
		return string.Join("|", _keys.Split("\n", StringSplitOptions.RemoveEmptyEntries));
	}

	public void StartHealthTimer()
	{
		Log("Start Health Timer");
		_healthTimer.Start();
	}

	public void StopHealthTimer()
	{
		Log("Stop Health Timer");
		_healthTimer.Stop();
	}

	public bool UpdateFile(string fileName, Stream steam, bool rebootRequired)
	{
		throw new NotImplementedException();
	}
}
