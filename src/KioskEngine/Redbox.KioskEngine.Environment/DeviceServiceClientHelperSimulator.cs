using System;
using System.Collections.Generic;
using System.IO;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.KioskEngine.Environment;

public class DeviceServiceClientHelperSimulator : IDeviceServiceClientHelper
{
	private OnDeviceServiceCanShutDown _onDeviceServiceCanShutDownHandler;

	private ReadCardJob _readCardJob;

	private Version _deviceServiceVersion;

	private bool _statusChecked;

	private string _fileDir;

	private DeviceServiceHelperSimulatorModel _model;

	public static DeviceServiceClientHelperSimulator Instance => Singleton<DeviceServiceClientHelperSimulator>.Instance;

	public bool IsConnectedToDeviceService
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Device Service Simulator", "IsConnectedToDeviceService", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Device Service Simulator", "IsConnectedToDeviceService", value);
		}
	}

	public bool IsCardReaderConnected
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Device Service Simulator", "IsCardReaderConnected", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Device Service Simulator", "IsCardReaderConnected", value);
		}
	}

	public bool IsCardReaderTampered
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Device Service Simulator", "IsCardReaderTampered", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Device Service Simulator", "IsCardReaderTampered", value);
		}
	}

	public bool UseSimulatedCardReader
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Device Service Simulator", "UseSimulatedCardReader", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Device Service Simulator", "UseSimulatedCardReader", value);
		}
	}

	public bool SupportsEMV => true;

	public bool SupportsVas => true;

	public Version DeviceServiceVersion => _deviceServiceVersion;

	public Version DeviceServiceClientVersion => _deviceServiceVersion;

	public IDeviceServiceShutDownInfo DeviceServiceShutDownInfo => new DeviceServiceShutDownInfo();

	public bool ConnectToDeviceService()
	{
		return IsConnectedToDeviceService;
	}

	public void RegisterOnDeviceServiceCanShutDownHandler(OnDeviceServiceCanShutDown onDeviceServiceCanShutDown)
	{
		_onDeviceServiceCanShutDownHandler = onDeviceServiceCanShutDown;
	}

	public void PollForCardReaderConnectionAndValidVersion()
	{
	}

	public bool ProcessKey(char keyChar)
	{
		if (_readCardJob != null && _readCardJob.IsExecuting && _model.StoredCardData.ContainsKey(keyChar.ToString()))
		{
			_readCardJob.ReadCardEventHandler(new SimpleResponseEvent(null, "CardProcessingStartedResponseEvent")
			{
				RequestId = _readCardJob.RequestId
			});
			CardData cardData = _model.StoredCardData[keyChar.ToString()];
			CardSourceType cardSourceType = ParseCardSourceType(cardData.CardSource);
			BaseResponseEvent obj;
			switch (cardSourceType)
			{
			case CardSourceType.Swipe:
			case CardSourceType.MSDContactless:
				obj = CreateSwipeResponse(cardData, cardSourceType, _readCardJob);
				break;
			case CardSourceType.EMVContact:
			case CardSourceType.QuickChip:
			case CardSourceType.EMVContactless:
			case CardSourceType.Mobile:
			case CardSourceType.VASOnly:
				obj = CreateEMVResponse(cardData, cardSourceType);
				break;
			default:
				return false;
			}
			_readCardJob.ReadCardResponseHandler(obj);
			return true;
		}
		return false;
	}

	private BaseResponseEvent CreateEMVResponse(CardData cardData, CardSourceType sourceType)
	{
		EMVCardReadModel eMVCardReadModel = new EMVCardReadModel();
		if (sourceType != CardSourceType.VASOnly)
		{
			eMVCardReadModel = (EMVCardReadModel)ParseCardData(cardData, sourceType);
		}
		eMVCardReadModel.VasData = cardData.VasData;
		return new EMVCardReadResponseEvent(null)
		{
			Data = eMVCardReadModel,
			EventName = "EMVCardReadResponseEvent",
			Success = true,
			RequestId = _readCardJob.RequestId
		};
	}

	private BaseResponseEvent CreateSwipeResponse(CardData cardData, CardSourceType sourceType, ReadCardJob readCardJob)
	{
		Base87CardReadModel base87CardReadModel = ParseCardData(cardData, sourceType);
		if (base87CardReadModel != null && base87CardReadModel.EncryptedFlag)
		{
			return new EncryptedCardReadResponseEvent(null)
			{
				Data = (EncryptedCardReadModel)base87CardReadModel,
				Success = (base87CardReadModel != null),
				EventName = "EncryptedCardReadResponseEvent",
				RequestId = (readCardJob?.RequestId ?? default(Guid))
			};
		}
		return new UnencryptedCardReadResponseEvent(null)
		{
			Data = (UnencryptedCardReadModel)base87CardReadModel,
			Success = (base87CardReadModel != null),
			EventName = "UnencryptedCardReadResponseEvent",
			RequestId = (readCardJob?.RequestId ?? default(Guid))
		};
	}

	public IReadCardJob StartCardRead(DeviceInputType deviceInputType, Action<BaseResponseEvent> readCardResponseHandler, Action<BaseResponseEvent> readCardEventHandler, int commandTimeout, VasMode vasMode)
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IShoppingSession currentSession = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetCurrentSession();
		IEnumerable<CardBrandAndSource> deactivatedCardBrandSources = GetDeactivatedCardBrandSources();
		decimal valueOrDefault = (service?.CurrentRentalShoppingCart?.Totals.GrandTotal).GetValueOrDefault();
		valueOrDefault = Math.Round(valueOrDefault, 2);
		CardReadRequest cardReadRequest = new CardReadRequest
		{
			InputType = deviceInputType,
			VasMode = vasMode,
			Amount = valueOrDefault,
			Timeout = commandTimeout + 5000,
			ExcludeCardBrandBySource = deactivatedCardBrandSources,
			SessionId = currentSession.Id
		};
		_readCardJob = new ReadCardJob(cardReadRequest, readCardResponseHandler, readCardEventHandler, commandTimeout);
		_readCardJob.Execute();
		LogHelper.Instance.Log($"Starting DeviceServiceClient.ReadCard with id {_readCardJob?.RequestId}");
		return _readCardJob;
	}

	public void CancelCommand(Guid id)
	{
		LogHelper.Instance.Log($"Cancelling DeviceService command with id {id}");
		_readCardJob = null;
	}

	public void ReportAuthorizeResult(bool authorizeSuccessful)
	{
		LogHelper.Instance.Log($"Reporting Authorize Result of {authorizeSuccessful} to DeviceService");
	}

	private IEnumerable<CardBrandAndSource> GetDeactivatedCardBrandSources()
	{
		List<CardBrandAndSource> list = new List<CardBrandAndSource>();
		Dictionary<CardBrandAndSource, bool> dictionary = new Dictionary<CardBrandAndSource, bool>();
		DeviceServiceConfiguration deviceServiceConfiguration = ServiceLocator.Instance.GetService<IConfiguration>()?.KioskSession?.DeviceService;
		dictionary.Add(CardBrandAndSource.VisaTap, deviceServiceConfiguration.ContactlessVisaEnabled);
		dictionary.Add(CardBrandAndSource.AmexTap, deviceServiceConfiguration.ContactlessAmexEnabled);
		dictionary.Add(CardBrandAndSource.MasterCardTap, deviceServiceConfiguration.ContactlessMasterCardEnabled);
		dictionary.Add(CardBrandAndSource.DiscoverTap, deviceServiceConfiguration.ContactlessDiscoverEnabled);
		dictionary.Add(CardBrandAndSource.VisaChip, deviceServiceConfiguration.VisaInsertChipEnabled);
		dictionary.Add(CardBrandAndSource.AmexChip, deviceServiceConfiguration.AmexInsertChipEnabled);
		dictionary.Add(CardBrandAndSource.MasterCardChip, deviceServiceConfiguration.MasterCardInsertChipEnabled);
		dictionary.Add(CardBrandAndSource.DiscoverChip, deviceServiceConfiguration.AmexInsertChipEnabled);
		foreach (KeyValuePair<CardBrandAndSource, bool> item in dictionary)
		{
			if (!item.Value)
			{
				list.Add(item.Key);
			}
		}
		return list;
	}

	private void CheckIfCardReaderIsConnectedAndValid(bool cardReaderConnectionAlreadyVerified)
	{
		bool flag = false;
		if (cardReaderConnectionAlreadyVerified || IsCardReaderConnected)
		{
			flag = true;
		}
		if (flag)
		{
			RaiseValidDeviceServiceCardReader();
		}
	}

	private void RaiseValidDeviceServiceCardReader()
	{
		CheckDeviceActivation();
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseValidDeviceServiceCardReader();
	}

	public void CheckDeviceActivation()
	{
		LogHelper.Instance.Log("CheckDeviceActivation simulated.");
	}

	public void CheckDeviceStatus()
	{
		if (_statusChecked)
		{
			LogHelper.Instance.Log("CheckDeviceStatus: Status already checked. Skipping.");
			return;
		}
		ServiceLocator.Instance.GetService<IConfiguration>();
		if (IsConnectedToDeviceService)
		{
			LogHelper.Instance.Log("CheckDeviceStatus: simulating CheckDeviceStatus command.");
			_statusChecked = true;
		}
		else
		{
			LogHelper.Instance.Log("CheckDeviceStatus: Device Service not connected.");
		}
	}

	public void AddLogText(string logline)
	{
		LogHelper.Instance.Log(logline, LogEntryType.Info);
	}

	public ValidateVersionModel ValidateVersion()
	{
		ValidateVersionModel validateVersionModel = new ValidateVersionModel
		{
			DeviceServiceVersion = new Version("0.0.0.0"),
			IsCompatible = true
		};
		_deviceServiceVersion = validateVersionModel.DeviceServiceVersion;
		return validateVersionModel;
	}

	public void SendTamperedCardReaderKioskAlertMessage()
	{
		try
		{
			long num = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0;
			ServiceLocator.Instance.GetService<IHealthServices>().SendAlert((ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0).ToString(), "ApplicationCrash", "mm:TamperedCardReaderError, 0099CDRR", string.Format("<h2>Kiosk Engine Card Reader Tampered Error</h2><p>Card Reader Tampering detected on Kiosk: {0}!<br><br>Placing Kiosk into Maintenance Mode.<br>{1}</p>", num, "0099CDRR"), DateTime.Now, null, null, skipQueue: true);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in DeviceServiceClientHelper.SendTamperdCardReaderKioskAlertMessage!", e);
		}
	}

	public void GetCardInsertedStatus(Action<GetCardInsertedStatusResponseEvent> completeCallback)
	{
		GetCardInsertedStatusResponseEvent obj = new GetCardInsertedStatusResponseEvent(null)
		{
			CardInsertedStatus = (_readCardJob.IsCardInserted ? InsertedStatus.Inserted : InsertedStatus.Removed),
			RequestId = _readCardJob.RequestId
		};
		completeCallback?.Invoke(obj);
	}

	public bool ShutDownDeviceService(bool forceShutdown, ShutDownReason shutDownReason)
	{
		return true;
	}

	private DeviceServiceClientHelperSimulator()
	{
		IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
		_fileDir = service.DataPath;
		ReadConfigFile();
	}

	private void ReadConfigFile()
	{
		string path = Path.Combine(_fileDir, "KioskEngineSimulatedCardReaderData.json");
		if (File.Exists(path))
		{
			_model = JsonConvert.DeserializeObject<DeviceServiceHelperSimulatorModel>(File.ReadAllText(path));
		}
	}

	private CardSourceType ParseCardSourceType(string sourceType)
	{
		if (sourceType != null)
		{
			switch (sourceType.Length)
			{
			case 1:
				switch (sourceType[0])
				{
				case '1':
				case 'M':
					return CardSourceType.Swipe;
				case '5':
				case 'E':
					return CardSourceType.EMVContactless;
				case '4':
				case 'C':
					return CardSourceType.MSDContactless;
				case '2':
				case 'S':
					return CardSourceType.EMVContact;
				case '3':
				case 'Q':
					return CardSourceType.QuickChip;
				case '6':
				case 'm':
					return CardSourceType.Mobile;
				case '7':
				case 'c':
					return CardSourceType.VASOnly;
				}
				break;
			case 2:
				if (!(sourceType == "cq"))
				{
					break;
				}
				return CardSourceType.VASOnly;
			}
		}
		return CardSourceType.Unknown;
	}

	private Base87CardReadModel ParseCardData(CardData cardData, CardSourceType sourceType)
	{
		if (cardData == null)
		{
			LogHelper.Instance.Log("ParseCardData -> cardData contains no data.");
			return null;
		}
		try
		{
			if (cardData.EncryptedFlag)
			{
				if (sourceType == CardSourceType.Swipe)
				{
					LogHelper.Instance.Log("Creating Encrypted swipe model");
					return new EncryptedCardReadModel
					{
						FirstSix = cardData.FirstSix,
						LastFour = cardData.LastFour,
						PANLength = cardData.PANLength,
						Mod10CheckFlag = cardData.Mod10CheckFlag,
						Expiry = cardData.Expiry,
						ServiceCode = cardData.ServiceCode,
						LanguageCode = cardData.LanguageCode,
						EncryptedFlag = cardData.EncryptedFlag,
						Name = cardData.Name,
						NameLength = cardData.NameLength,
						EncFormat = cardData.EncFormat,
						KSN = cardData.KSN,
						ICEncDataLength = cardData.ICEncDataLength,
						ICEncData = cardData.ICEncData,
						AESPANLength = cardData.AESPANLength,
						AESPAN = cardData.AESPAN,
						LSEncDataLength = cardData.LSEncDataLength,
						LSEncData = cardData.LSEncData,
						ExtLangCode = cardData.ExtLangCode,
						Status = ResponseStatus.Success,
						MfgSerialNumber = cardData.MfgSerialNumber,
						InjectedSerialNumber = "Simulated Injected Serial Number",
						CardSource = sourceType,
						FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter
					};
				}
				LogHelper.Instance.Log("Creating ENV model");
				return new EMVCardReadModel
				{
					FirstSix = cardData.FirstSix,
					LastFour = cardData.LastFour,
					PANLength = cardData.PANLength,
					Mod10CheckFlag = cardData.Mod10CheckFlag,
					Expiry = cardData.Expiry,
					ServiceCode = cardData.ServiceCode,
					LanguageCode = cardData.LanguageCode,
					EncryptedFlag = cardData.EncryptedFlag,
					Name = cardData.Name,
					NameLength = cardData.NameLength,
					EncFormat = cardData.EncFormat,
					KSN = cardData.KSN,
					ICEncDataLength = cardData.ICEncDataLength,
					ICEncData = cardData.ICEncData,
					AESPANLength = cardData.AESPANLength,
					AESPAN = cardData.AESPAN,
					LSEncDataLength = cardData.LSEncDataLength,
					LSEncData = cardData.LSEncData,
					ExtLangCode = cardData.ExtLangCode,
					Tags = cardData.Tags,
					Status = ResponseStatus.Success,
					MfgSerialNumber = cardData.MfgSerialNumber,
					InjectedSerialNumber = "Simulated Injected Serial Number",
					CardSource = sourceType,
					FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter
				};
			}
			return new UnencryptedCardReadModel
			{
				FirstSix = cardData.FirstSix,
				LastFour = cardData.LastFour,
				PANLength = cardData.PANLength,
				Mod10CheckFlag = cardData.Mod10CheckFlag,
				Expiry = cardData.Expiry,
				ServiceCode = cardData.ServiceCode,
				LanguageCode = cardData.LanguageCode,
				EncryptedFlag = cardData.EncryptedFlag,
				Name = cardData.Name,
				Track1 = cardData.Track1,
				Track2 = cardData.Track2,
				Status = ResponseStatus.Success,
				FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter
			};
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("Parse Card Data", e);
		}
		return null;
	}
}
