using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using RBA_SDK_ComponentModel;

namespace RBA_SDK_Simulator;

/// <summary>
/// In-process stand-in for the Ingenico RBA SDK (RBA_SDK_STANDARD.RBA_API + native RBA_SDK.dll) that emulates an
/// iUC285 terminal closely enough for DeviceService's IUC285Proxy: request/response parameters, unsolicited terminal
/// messages raised through <see cref="pinpadHandler" />, and the card read / EMV message sequences.
/// Customer actions come from <see cref="ICommandQueue" /> (see SimulatorController).
/// </summary>
public class RBA_API_Simulator : IRBA_API, IDisposable
{
	private enum EmvState
	{
		None,
		CardRead,
		Initiated,
		AwaitingAuthorization
	}

	private static readonly HashSet<PARAMETER_ID> MultiValueParameters = new HashSet<PARAMETER_ID> { PARAMETER_ID.P17_RES_VAS_DATA };

	private readonly ISimulatorDataProvider _data;

	private readonly ICommandQueue _commands;

	private readonly ILogger<RBA_API_Simulator> _logger;

	private readonly object _sync = new object();

	private readonly Dictionary<PARAMETER_ID, List<string>> _params = new Dictionary<PARAMETER_ID, List<string>>();

	private readonly Dictionary<MESSAGE_ID, Queue<KeyValuePair<int, byte[]>>> _tags = new Dictionary<MESSAGE_ID, Queue<KeyValuePair<int, byte[]>>>();

	private readonly BlockingCollection<Action> _terminalMessages = new BlockingCollection<Action>();

	private readonly ConcurrentQueue<string> _history = new ConcurrentQueue<string>();

	private readonly CancellationTokenSource _stop = new CancellationTokenSource();

	private DisconnectHandler _disconnectHandler;

	private bool _initialized;

	private bool _connected;

	private bool _rebooting;

	private MESSAGE_ID? _pendingRead;

	private SimulatedCard _activeCard;

	private bool _cardInserted;

	private EmvState _emvState;

	private Thread _messageThread;

	private Thread _commandThread;

	public LogHandler logHandler { get; set; }

	public PinPadMessageHandler pinpadHandler { get; set; }

	/// <summary>
	/// The most recently initialized simulator. DeviceService.WebApi's Startup creates IUC285Proxy from a second
	/// ServiceProvider (built on a background task), so the terminal the proxy talks to is not the IRBA_API
	/// singleton the MVC container hands to SimulatorController. The controller drives this instance instead.
	/// </summary>
	public static RBA_API_Simulator Active { get; private set; }

	internal ISimulatorDataProvider Data => _data;

	internal ICommandQueue Commands => _commands;

	public bool IsConnected => _connected;

	public bool IsCardInserted => _cardInserted;

	public string PendingRead => _pendingRead?.ToString();

	public string EmvTransactionState => _emvState.ToString();

	public IReadOnlyList<string> History => _history.ToArray();

	public RBA_API_Simulator(ISimulatorDataProvider data, ICommandQueue commands, ILogger<RBA_API_Simulator> logger)
	{
		_data = data;
		_commands = commands;
		_logger = logger;
	}

	public ERROR_ID Initialize()
	{
		lock (_sync)
		{
			if (!_initialized)
			{
				_messageThread = new Thread(MessageLoop)
				{
					IsBackground = true,
					Name = "RBA simulator terminal messages"
				};
				_messageThread.Start();
				_commandThread = new Thread(CommandLoop)
				{
					IsBackground = true,
					Name = "RBA simulator customer actions"
				};
				_commandThread.Start();
				_initialized = true;
			}
		}
		Active = this;
		Trace("Initialize");
		return ERROR_ID.RESULT_SUCCESS;
	}

	public void SetDefaultLogLevel(LOG_LEVEL level)
	{
	}

	public ERROR_ID Connect(string comPort)
	{
		Trace("Connect " + comPort);
		if (!_initialized)
		{
			return ERROR_ID.RESULT_ERROR_NOT_INITIALIZED;
		}
		if (!_data.DeviceAttached || _rebooting)
		{
			return ERROR_ID.RESULT_ERROR_NOT_CONNECTED;
		}
		if (_connected)
		{
			return ERROR_ID.RESULT_ERROR_ALREADY_CONNECTED;
		}
		_connected = true;
		return ERROR_ID.RESULT_SUCCESS;
	}

	public ERROR_ID Disconnect()
	{
		Trace("Disconnect");
		_connected = false;
		return ERROR_ID.RESULT_SUCCESS;
	}

	public ERROR_ID SetNotifyRbaDisconnected(DisconnectHandler handler)
	{
		_disconnectHandler = handler;
		return ERROR_ID.RESULT_SUCCESS;
	}

	public ERROR_ID SetCommTimeouts(SETTINGS_COMM_TIMEOUTS timeouts)
	{
		return ERROR_ID.RESULT_SUCCESS;
	}

	public string GetVersion_NonStatic()
	{
		return "RBA_SDK_Simulator " + typeof(RBA_API_Simulator).Assembly.GetName().Version;
	}

	public ERROR_ID SendCustomMessage(string RawData, bool bWaitResponce)
	{
		Trace("Custom message: " + RawData);
		if (!_connected)
		{
			return ERROR_ID.RESULT_ERROR_NOT_CONNECTED;
		}
		return ERROR_ID.RESULT_SUCCESS;
	}

	#region Parameters

	public ERROR_ID SetParam(PARAMETER_ID id, byte[] data)
	{
		return SetParam(id, data == null ? string.Empty : Encoding.ASCII.GetString(data));
	}

	public ERROR_ID SetParam(PARAMETER_ID id, string data)
	{
		lock (_params)
		{
			_params[id] = new List<string> { data ?? string.Empty };
		}
		return ERROR_ID.RESULT_SUCCESS;
	}

	public ERROR_ID AddParam(PARAMETER_ID id, string v)
	{
		lock (_params)
		{
			if (!_params.TryGetValue(id, out List<string> value))
			{
				value = (_params[id] = new List<string>());
			}
			value.Add(v ?? string.Empty);
		}
		return ERROR_ID.RESULT_SUCCESS;
	}

	public ERROR_ID ResetParam(PARAMETER_ID id)
	{
		lock (_params)
		{
			if (id == PARAMETER_ID.P_ALL_PARAMS)
			{
				_params.Clear();
			}
			else
			{
				_params.Remove(id);
			}
		}
		return ERROR_ID.RESULT_SUCCESS;
	}

	public string GetParam(PARAMETER_ID id)
	{
		lock (_params)
		{
			if (!_params.TryGetValue(id, out List<string> value) || value.Count == 0)
			{
				return string.Empty;
			}
			string result = value[0];
			if (MultiValueParameters.Contains(id))
			{
				value.RemoveAt(0);
			}
			return result;
		}
	}

	public int GetParamLen(PARAMETER_ID id)
	{
		lock (_params)
		{
			if (!_params.TryGetValue(id, out List<string> value) || value.Count == 0)
			{
				return -1;
			}
			return value[0].Length;
		}
	}

	public object GetMutableParam(PARAMETER_ID id)
	{
		return GetParam(id);
	}

	private string Request(PARAMETER_ID id)
	{
		lock (_params)
		{
			if (!_params.TryGetValue(id, out List<string> value) || value.Count == 0)
			{
				return string.Empty;
			}
			return value[0];
		}
	}

	private void Respond(PARAMETER_ID id, string value)
	{
		SetParam(id, value);
	}

	private void Respond(IDictionary<string, string> values)
	{
		foreach (KeyValuePair<string, string> value in values)
		{
			if (Enum.TryParse(value.Key, out PARAMETER_ID result))
			{
				Respond(result, value.Value);
			}
		}
	}

	#endregion

	#region EMV tags

	public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, byte[] data)
	{
		lock (_tags)
		{
			if (!_tags.TryGetValue(msg, out Queue<KeyValuePair<int, byte[]>> value))
			{
				value = (_tags[msg] = new Queue<KeyValuePair<int, byte[]>>());
			}
			value.Enqueue(new KeyValuePair<int, byte[]>(tagId, data ?? new byte[0]));
		}
		return ERROR_ID.RESULT_SUCCESS;
	}

	public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, string data)
	{
		return AddTagParam(msg, tagId, Encoding.ASCII.GetBytes(data ?? string.Empty));
	}

	public int GetTagParamLen(MESSAGE_ID msg)
	{
		lock (_tags)
		{
			if (!_tags.TryGetValue(msg, out Queue<KeyValuePair<int, byte[]>> value) || value.Count == 0)
			{
				return 0;
			}
			return value.Peek().Value.Length;
		}
	}

	public int GetTagParam(MESSAGE_ID msg, out byte[] data)
	{
		lock (_tags)
		{
			if (!_tags.TryGetValue(msg, out Queue<KeyValuePair<int, byte[]>> value) || value.Count == 0)
			{
				data = new byte[0];
				return 0;
			}
			KeyValuePair<int, byte[]> keyValuePair = value.Dequeue();
			data = keyValuePair.Value;
			return keyValuePair.Key;
		}
	}

	private List<KeyValuePair<int, byte[]>> TakeTags(MESSAGE_ID msg)
	{
		lock (_tags)
		{
			if (!_tags.TryGetValue(msg, out Queue<KeyValuePair<int, byte[]>> value))
			{
				return new List<KeyValuePair<int, byte[]>>();
			}
			_tags.Remove(msg);
			return value.ToList();
		}
	}

	private void SetTags(MESSAGE_ID msg, IEnumerable<KeyValuePair<int, byte[]>> tags)
	{
		lock (_tags)
		{
			_tags[msg] = new Queue<KeyValuePair<int, byte[]>>(tags);
		}
	}

	#endregion

	#region Messages from the host

	public ERROR_ID ProcessMessage(MESSAGE_ID messageId)
	{
		Trace($"ProcessMessage {messageId}");
		if (!_connected)
		{
			return ERROR_ID.RESULT_ERROR_NOT_CONNECTED;
		}
		switch (messageId)
		{
		case MESSAGE_ID.M00_OFFLINE:
			lock (_sync)
			{
				_pendingRead = null;
				if (!_cardInserted)
				{
					_emvState = EmvState.None;
				}
			}
			RaiseTerminalMessage(MESSAGE_ID.M00_OFFLINE, null);
			break;
		case MESSAGE_ID.M04_SET_PAYMENT_TYPE:
			Respond(PARAMETER_ID.P04_RES_FORCE_PAYMENT_TYPE, "0");
			Respond(PARAMETER_ID.P04_RES_PAYMENT_TYPE, Request(PARAMETER_ID.P04_REQ_PAYMENT_TYPE));
			Respond(PARAMETER_ID.P04_RES_AMOUNT, Request(PARAMETER_ID.P04_REQ_AMOUNT));
			break;
		case MESSAGE_ID.M07_UNIT_DATA:
			Respond(_data.UnitData);
			if (_data.Tampered)
			{
				Respond(PARAMETER_ID.P07_RES_MANUFACTURING_SERIAL_NUMBER, "ALERT");
			}
			break;
		case MESSAGE_ID.M08_HEALTH_STAT:
			Respond(_data.HealthData);
			if (_data.Tampered)
			{
				Respond(PARAMETER_ID.P08_RES_MANUFACTURING_SERIAL_NUMBER, "ALERT");
			}
			break;
		case MESSAGE_ID.M13_AMOUNT:
			lock (_sync)
			{
				// Contact EMV: after 33.00 and the amount, the card asks for online authorization (33.03).
				if (_emvState == EmvState.Initiated)
				{
					_emvState = EmvState.AwaitingAuthorization;
					RaiseAuthorizationRequest();
				}
			}
			break;
		case MESSAGE_ID.M17_MERCHANT_DATA_WRITE:
			Respond(PARAMETER_ID.P17_RES_TYPE, Request(PARAMETER_ID.P17_REQ_TYPE));
			Respond(PARAMETER_ID.P17_RES_STATUS, "0");
			Respond(PARAMETER_ID.P17_RES_IS_ADDITIONAL_DATA, "0");
			Respond(PARAMETER_ID.P17_RES_VAS_RESULT, "0");
			ResetParam(PARAMETER_ID.P17_RES_VAS_DATA);
			break;
		case MESSAGE_ID.M23_CARD_READ:
		case MESSAGE_ID.M87_E2EE_CARD_READ:
			lock (_sync)
			{
				_pendingRead = messageId;
			}
			break;
		case MESSAGE_ID.M28_SET_VARIABLE:
			_data.SetVariable(Request(PARAMETER_ID.P28_REQ_VARIABLE_ID), Request(PARAMETER_ID.P28_REQ_VARIABLE_DATA));
			Respond(PARAMETER_ID.P28_RES_STATUS, "2");
			break;
		case MESSAGE_ID.M29_GET_VARIABLE:
		{
			string variable = _data.GetVariable(Request(PARAMETER_ID.P29_REQ_VARIABLE_ID));
			Respond(PARAMETER_ID.P29_RES_STATUS, variable == null ? "6" : "2");
			Respond(PARAMETER_ID.P29_RES_VARIABLE_DATA, variable ?? string.Empty);
			break;
		}
		case MESSAGE_ID.M33_00_EMV_TRANSACTION_INITIATION:
			lock (_sync)
			{
				if (_cardInserted && _emvState == EmvState.CardRead)
				{
					_emvState = EmvState.Initiated;
					RaiseTerminalMessage(MESSAGE_ID.M33_02_EMV_TRANSACTION_PREPARATION_RESPONSE, () => RespondEmv(MESSAGE_ID.M33_02_EMV_TRANSACTION_PREPARATION_RESPONSE, PARAMETER_ID.P33_02_RES_STATUS, null));
				}
			}
			break;
		case MESSAGE_ID.M33_01_EMV_STATUS:
			Respond(PARAMETER_ID.P33_01_RES_F1_CHIP_CARD, _cardInserted ? "I" : "R");
			break;
		case MESSAGE_ID.M33_04_EMV_AUTHORIZATION_RESPONSE:
		{
			byte[] array = TakeTags(MESSAGE_ID.M33_04_EMV_AUTHORIZATION_RESPONSE).FirstOrDefault((KeyValuePair<int, byte[]> t) => t.Key == 0x8A).Value;
			string text = array == null ? "00" : Encoding.ASCII.GetString(array);
			lock (_sync)
			{
				if (_emvState == EmvState.AwaitingAuthorization)
				{
					bool approved = text == "00" && _activeCard != null && !_activeCard.Decline;
					_emvState = _cardInserted ? EmvState.CardRead : EmvState.None;
					RaiseTerminalMessage(MESSAGE_ID.M33_05_EMV_AUTHORIZATION_CONFIRMATION, () => RespondEmv(MESSAGE_ID.M33_05_EMV_AUTHORIZATION_CONFIRMATION, PARAMETER_ID.P33_05_RES_STATUS, (byte)(approved ? 0x40 : 0x00), text));
				}
			}
			break;
		}
		case MESSAGE_ID.M60_CONFIGURATION_WRITE:
			_data.WriteConfig(Request(PARAMETER_ID.P60_REQ_GROUP_NUM), Request(PARAMETER_ID.P60_REQ_INDEX_NUM), Request(PARAMETER_ID.P60_REQ_DATA_CONFIG_PARAM));
			Respond(PARAMETER_ID.P60_RES_STATUS, "2");
			break;
		case MESSAGE_ID.M61_CONFIGURATION_READ:
		{
			string text2 = _data.ReadConfig(Request(PARAMETER_ID.P61_REQ_GROUP_NUM), Request(PARAMETER_ID.P61_REQ_INDEX_NUM));
			Respond(PARAMETER_ID.P61_RES_STATUS, "2");
			Respond(PARAMETER_ID.P61_RES_DATA_CONFIG_PARAMETER, text2 ?? string.Empty);
			break;
		}
		case MESSAGE_ID.M62_FILE_WRITE:
		case MESSAGE_ID.FILE_WRITE:
			WriteFile(messageId);
			break;
		case MESSAGE_ID.M65_RETRIEVE_FILE:
		{
			if (!_data.Files.TryGetValue(Request(PARAMETER_ID.P65_REQ_FILE_NAME), out string value))
			{
				Respond(PARAMETER_ID.P65_RES_RESULT, "1");
				break;
			}
			Respond(PARAMETER_ID.P65_RES_RESULT, "0");
			Respond(PARAMETER_ID.P65_RES_TOTAL_NUMBER_OF_BLOCKS, "1");
			Respond(PARAMETER_ID.P65_RES_BLOCK_NUMBER, "1");
			Respond(PARAMETER_ID.P65_RES_DATA_TYPE, Request(PARAMETER_ID.P65_REQ_DATA_TYPE));
			Respond(PARAMETER_ID.P65_RES_CRC, "0000");
			Respond(PARAMETER_ID.P65_RES_DATA, value);
			break;
		}
		case MESSAGE_ID.M97_REBOOT:
			SimulateReboot();
			break;
		}
		return ERROR_ID.RESULT_SUCCESS;
	}

	private void WriteFile(MESSAGE_ID messageId)
	{
		string text = Request(PARAMETER_ID.P62_REQ_FILE_NAME);
		string text2 = Request(PARAMETER_ID.P62_REQ_FILE_DATA);
		if (messageId == MESSAGE_ID.FILE_WRITE)
		{
			// Whole-file download from a path on the host.
			string text3 = Request(PARAMETER_ID.P62_REQ_OS_FILE_NAME);
			try
			{
				text2 = System.IO.File.Exists(text3) ? Convert.ToBase64String(System.IO.File.ReadAllBytes(text3)) : string.Empty;
			}
			catch (Exception ex)
			{
				Trace("FILE_WRITE could not read " + text3 + ": " + ex.Message);
				text2 = string.Empty;
			}
		}
		string key = string.IsNullOrEmpty(text) ? "_lastFile" : text;
		if (Request(PARAMETER_ID.P62_REQ_RECORD_TYPE) == "1" && _data.Files.TryGetValue(key, out string value))
		{
			text2 = value + text2;
		}
		_data.Files[key] = text2;
		if (!string.IsNullOrEmpty(text))
		{
			_data.Files["_lastFile"] = text2;
		}
		Respond(PARAMETER_ID.P62_RES_STATUS, "0");
		Respond(PARAMETER_ID.P62_RES_FILE_LENGTH, text2.Length.ToString(CultureInfo.InvariantCulture));
	}

	private void SimulateReboot()
	{
		_data.CountReboot();
		lock (_sync)
		{
			_rebooting = true;
			_pendingRead = null;
			_cardInserted = false;
			_emvState = EmvState.None;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			Thread.Sleep(Math.Max(_data.ResponseDelay, 100));
			_connected = false;
			Trace("Terminal rebooting - disconnected");
			_disconnectHandler?.Invoke();
			Thread.Sleep(5000);
			_rebooting = false;
			Trace("Terminal reboot complete");
		});
	}

	#endregion

	#region Customer actions

	public void SimulateDisconnect()
	{
		_data.DeviceAttached = false;
		if (_connected)
		{
			_connected = false;
			_disconnectHandler?.Invoke();
		}
		Trace("Terminal unplugged");
	}

	public void SimulateAttach()
	{
		_data.DeviceAttached = true;
		Trace("Terminal plugged in");
	}

	private bool CanRun(SimulatorCommand command)
	{
		lock (_sync)
		{
			switch (command.Type)
			{
			case SimulatorCommandType.RemoveCard:
				return _cardInserted;
			case SimulatorCommandType.InsertChip:
				return _pendingRead.HasValue && !_cardInserted;
			default:
				return _pendingRead.HasValue;
			}
		}
	}

	private void CommandLoop()
	{
		while (!_stop.IsCancellationRequested)
		{
			_commands.WaitForCommand(250);
			while (_connected && _commands.TryTake(CanRun, out SimulatorCommand command))
			{
				try
				{
					Run(command);
				}
				catch (Exception exception)
				{
					_logger?.LogError(exception, "RBA simulator failed to run {Command}", command);
				}
			}
		}
	}

	private void Run(SimulatorCommand command)
	{
		Trace("Customer action: " + command);
		SimulatedCard card = (command.Card ?? _data.DefaultCard).Clone();
		MESSAGE_ID readMessage;
		lock (_sync)
		{
			if (command.Type == SimulatorCommandType.RemoveCard)
			{
				_cardInserted = false;
				_emvState = EmvState.None;
				RaiseTerminalMessage(MESSAGE_ID.M09_SET_ALLOWED_PAYMENT, delegate
				{
					Respond(PARAMETER_ID.P09_RES_CARD_TYPE, "02");
					Respond(PARAMETER_ID.P09_RES_CARD_STATUS, "R");
				});
				return;
			}
			readMessage = _pendingRead.Value;
			_pendingRead = null;
			_activeCard = card;
		}
		switch (command.Type)
		{
		case SimulatorCommandType.Cancel:
			RaiseCardRead(readMessage, "2", string.Empty, card, includeData: false);
			break;
		case SimulatorCommandType.BadRead:
			RaiseCardRead(readMessage, "1", string.Empty, card, includeData: false);
			break;
		case SimulatorCommandType.Swipe:
			_data.CountSwipe();
			RaiseCardRead(readMessage, "0", "M", card, includeData: true);
			break;
		case SimulatorCommandType.TapMsd:
			RaiseCardRead(readMessage, "0", "C", card, includeData: true);
			break;
		case SimulatorCommandType.InsertChip:
			lock (_sync)
			{
				_cardInserted = true;
				_emvState = EmvState.CardRead;
			}
			RaiseTerminalMessage(MESSAGE_ID.M09_SET_ALLOWED_PAYMENT, delegate
			{
				Respond(PARAMETER_ID.P09_RES_CARD_TYPE, "02");
				Respond(PARAMETER_ID.P09_RES_CARD_STATUS, "I");
			});
			RaiseCardRead(readMessage, "0", "S", card, includeData: false);
			break;
		case SimulatorCommandType.Tap:
			lock (_sync)
			{
				_emvState = EmvState.AwaitingAuthorization;
			}
			RaiseCardRead(readMessage, "0", "E", card, includeData: false);
			RaiseAuthorizationRequest();
			break;
		}
	}

	private void RaiseCardRead(MESSAGE_ID readMessage, string exitType, string source, SimulatedCard card, bool includeData)
	{
		RaiseTerminalMessage(readMessage, delegate
		{
			if (readMessage == MESSAGE_ID.M23_CARD_READ)
			{
				Respond(PARAMETER_ID.P23_RES_EXIT_TYPE, exitType);
				Respond(PARAMETER_ID.P23_RES_CARD_SOURCE, source);
				Respond(PARAMETER_ID.P23_RES_TRACK1, includeData ? card.Track1 : string.Empty);
				Respond(PARAMETER_ID.P23_RES_TRACK2, includeData ? card.Track2 : string.Empty);
				Respond(PARAMETER_ID.P23_RES_TRACK3, string.Empty);
			}
			else
			{
				Respond(PARAMETER_ID.P87_RES_EXIT_TYPE, exitType);
				Respond(PARAMETER_ID.P87_RES_CARD_SOURCE, source);
				Respond(PARAMETER_ID.P87_RES_CARD_DATA, includeData ? OnGuardCardData.Build(card) : string.Empty);
			}
		});
	}

	/// <summary>
	/// 33.03: the card's first cryptogram. ARQC (go online) normally; a card set to decline answers AAC here,
	/// which is how a chip refuses a transaction before authorization.
	/// </summary>
	private void RaiseAuthorizationRequest()
	{
		RaiseTerminalMessage(MESSAGE_ID.M33_03_EMV_AUTHORIZATION_REQUEST, delegate
		{
			bool flag = (_activeCard ?? _data.DefaultCard).Decline;
			RespondEmv(MESSAGE_ID.M33_03_EMV_AUTHORIZATION_REQUEST, PARAMETER_ID.P33_03_REQ_STATUS, (byte)(flag ? 0x00 : 0x80));
		});
	}

	/// <summary>Sets the status parameter and EMV tags of an unsolicited 33.xx message.</summary>
	private void RespondEmv(MESSAGE_ID messageId, PARAMETER_ID statusParameter, byte? cryptogramInformation, string authorizationResponseCode = null)
	{
		Respond(statusParameter, "00");
		SimulatedCard card = _activeCard ?? _data.DefaultCard;
		List<KeyValuePair<int, byte[]>> list = EmvTags.ForCard(card, card.MobileWallet && !_cardInserted);
		if (cryptogramInformation.HasValue)
		{
			list.Add(EmvTags.Tag(0x9F27, new byte[1] { cryptogramInformation.Value }));
		}
		if (authorizationResponseCode != null)
		{
			list.Add(EmvTags.Tag(0x8A, Encoding.ASCII.GetBytes(authorizationResponseCode)));
		}
		SetTags(messageId, list);
	}

	#endregion

	#region Unsolicited terminal messages

	private void RaiseTerminalMessage(MESSAGE_ID messageId, Action setResponse)
	{
		_terminalMessages.Add(delegate
		{
			Thread.Sleep(_data.ResponseDelay);
			setResponse?.Invoke();
			Trace($"Terminal message {messageId}");
			pinpadHandler?.Invoke(messageId);
		});
	}

	private void MessageLoop()
	{
		try
		{
			foreach (Action item in _terminalMessages.GetConsumingEnumerable(_stop.Token))
			{
				try
				{
					item();
				}
				catch (Exception exception)
				{
					_logger?.LogError(exception, "RBA simulator terminal message handler failed");
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	#endregion

	private void Trace(string message)
	{
		string text = $"{DateTime.Now:HH:mm:ss.fff} {message}";
		_history.Enqueue(text);
		while (_history.Count > 200 && _history.TryDequeue(out string _))
		{
		}
		logHandler?.Invoke("[RBA_SDK_Simulator] " + message);
		_logger?.LogDebug("[RBA_SDK_Simulator] {Message}", message);
	}

	public void Dispose()
	{
		_stop.Cancel();
		_terminalMessages.CompleteAdding();
	}
}
