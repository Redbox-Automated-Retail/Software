using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.Session;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public class DeviceServiceTrackDataService : IDeviceServiceTrackDataService, ITrackDataService
{
	private class ReadCardJobEntry
	{
		public IReadCardJob ReadCardJob { get; set; }

		public bool AttemptedCancel { get; set; }

		public bool IsZeroTouchRead { get; set; }

		public Guid SessionId { get; set; }
	}

	private List<ReadCardJobEntry> _readCardJobs = new List<ReadCardJobEntry>();

	private object _lockObject = new object();

	private IReadCardJob _readCardJob;

	private bool _cardReadAborted;

	public bool IsOlderReader => false;

	public bool IsNewReader => true;

	public bool IsInTechnicalFallback
	{
		get
		{
			bool result = false;
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			if (service == null || service.KioskSession.DeviceService.CardReadTechnicalFallbackEnabled)
			{
				int num = (ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession())?.CardReadTechnicalFallbackCount ?? 0;
				int num2 = service?.KioskSession.DeviceService.CardReadTechnicalFallbackCountLimit ?? 3;
				result = num >= num2;
			}
			return result;
		}
	}

	public FallbackType? NextFallbackReason { get; set; }

	private bool IsCardReadInProgress => _readCardJob != null;

	public void StartCardRead(CardReadRequestInfo request, Action<IReadCardJob> onReadJobStart, Action<Guid> onCardProcessing, Action<CardReadRequestInfo, CardReadCompleteDetails> onComplete)
	{
		IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		if (deviceServiceClientHelper == null)
		{
			return;
		}
		_cardReadAborted = false;
		Task.Run(delegate
		{
			try
			{
				Action<BaseResponseEvent, CardReadRequestInfo, Action<CardReadRequestInfo, CardReadCompleteDetails>> cardReadHandler = null;
				if (VasMode.VasOnly == request.VasMode)
				{
					cardReadHandler = CardReadVasOnlyResponseHandler;
				}
				else
				{
					cardReadHandler = CardReadResponseHandler;
				}
				IReadCardJob readCardJob = deviceServiceClientHelper.StartCardRead(request.InputType, delegate(BaseResponseEvent result)
				{
					cardReadHandler?.Invoke(result, request, onComplete);
				}, delegate(BaseResponseEvent result)
				{
					CardReadEventHandler(result, onCardProcessing);
				}, request.Timeout, request.VasMode);
				lock (_lockObject)
				{
					_readCardJobs.Add(new ReadCardJobEntry
					{
						ReadCardJob = readCardJob,
						SessionId = request.SessionId,
						IsZeroTouchRead = request.IsZeroTouchRead
					});
					LogHelper.Instance.Log($"Adding ReadCardJobEntry {readCardJob.RequestId}.  list count= {_readCardJobs?.Count}");
				}
				onReadJobStart?.Invoke(readCardJob);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in DeviceServiceTrackDataService.StartCardRead.", e);
			}
		});
	}

	public void StopCardRead(CancelReason cancelReason)
	{
		lock (_lockObject)
		{
			ReadCardJobEntry activeCardReadJobEntry = GetActiveCardReadJobEntry();
			if (activeCardReadJobEntry != null)
			{
				CancelReadCardJobEntry(cancelReason, activeCardReadJobEntry);
			}
			else
			{
				LogHelper.Instance.Log($"Failed to StopCardRead with cancel reason: {cancelReason} because no active card read was available.  Possible that card read has already returned data, please report auth result.");
			}
		}
	}

	private static void CancelReadCardJobEntry(CancelReason cancelReason, ReadCardJobEntry activeCardReadJobEntry)
	{
		ICardReadAttempt cardReadAttempt = (ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession())?.CardReadAttemptCollection?.GetByCardReadJobCommandRequestId(activeCardReadJobEntry.ReadCardJob.RequestId);
		if (cardReadAttempt != null)
		{
			cardReadAttempt.AttemptedCancel = true;
			cardReadAttempt.CancelReason = cancelReason;
			LogHelper.Instance.Log($"Setting CardReadAttempt.AttemptedCancel = true for card read command {activeCardReadJobEntry.ReadCardJob.RequestId}.  CancelReason = {cancelReason}");
		}
		activeCardReadJobEntry.AttemptedCancel = true;
		Task.Run(delegate
		{
			try
			{
				activeCardReadJobEntry.ReadCardJob.Cancel(delegate(BaseResponseEvent baseResponseEvent)
				{
					LogHelper.Instance.Log($"DeviceServiceTrackDataService.StopCardRead - cancel card read success = {(baseResponseEvent as CancelCommandResponseEvent)?.Success ?? false}");
				});
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in DeviceServiceTrackDataService.StopCardRead.", e);
			}
		});
	}

	public void StopAllSessionCardReads(Guid sessionId, CancelReason cancelReason)
	{
		lock (_lockObject)
		{
			if (_readCardJobs == null)
			{
				return;
			}
			List<ReadCardJobEntry> readCardJobEntriesToCancelAndRemove = new List<ReadCardJobEntry>();
			_readCardJobs.Where((ReadCardJobEntry x) => x.SessionId == sessionId).ForEach(delegate(ReadCardJobEntry x)
			{
				readCardJobEntriesToCancelAndRemove.Add(x);
			});
			if (readCardJobEntriesToCancelAndRemove.Count <= 0)
			{
				return;
			}
			LogHelper.Instance.Log($"Removing all Card Read Jobs for session {sessionId}");
			foreach (ReadCardJobEntry item in readCardJobEntriesToCancelAndRemove)
			{
				if (!item.AttemptedCancel)
				{
					CancelReadCardJobEntry(cancelReason, item);
				}
				RemoveReadCardJobEntry(item);
			}
		}
	}

	private ReadCardJobEntry GetActiveCardReadJobEntry()
	{
		ReadCardJobEntry result = null;
		lock (_lockObject)
		{
			foreach (ReadCardJobEntry readCardJob in _readCardJobs)
			{
				if (!readCardJob.AttemptedCancel && readCardJob.ReadCardJob != null)
				{
					result = readCardJob;
					break;
				}
			}
		}
		return result;
	}

	private void CardReadEventHandler(BaseResponseEvent baseResponseEvent, Action<Guid> OnCardProcessing)
	{
		if (!_cardReadAborted && IsCardReadInProgress && baseResponseEvent?.EventName == "CardProcessingStartedResponseEvent")
		{
			OnCardProcessing?.Invoke(baseResponseEvent.RequestId);
		}
	}

	private ReadCardJobEntry RemoveReadCardJobEntry(Guid requestId)
	{
		ReadCardJobEntry readCardJobEntry = null;
		lock (_lockObject)
		{
			ReadCardJobEntry readCardJobEntry2 = _readCardJobs.FirstOrDefault((ReadCardJobEntry x) => x.ReadCardJob != null && x.ReadCardJob.RequestId == requestId);
			if (readCardJobEntry2 != null)
			{
				readCardJobEntry = readCardJobEntry2;
				RemoveReadCardJobEntry(readCardJobEntry2);
			}
		}
		if (readCardJobEntry == null)
		{
			LogHelper.Instance.Log($"Unable to find/remove ReadCardJobEntry {requestId}.  list count= {_readCardJobs?.Count}");
		}
		return readCardJobEntry;
	}

	private void RemoveReadCardJobEntry(ReadCardJobEntry readCardJobEntry)
	{
		lock (_lockObject)
		{
			if (readCardJobEntry != null)
			{
				_readCardJobs.Remove(readCardJobEntry);
				LogHelper.Instance.Log($"Removing ReadCardJobEntry  RequestId: {readCardJobEntry.ReadCardJob.RequestId},  SessionId: {readCardJobEntry.SessionId}, IsZeroTouch: {readCardJobEntry.IsZeroTouchRead},  list count= {_readCardJobs?.Count}");
			}
		}
	}

	private void CardReadResponseHandler(BaseResponseEvent baseResponseEvent, CardReadRequestInfo request, Action<CardReadRequestInfo, CardReadCompleteDetails> onComplete)
	{
		_readCardJob = null;
		ReadCardJobEntry readEntry = RemoveReadCardJobEntry(baseResponseEvent.RequestId);
		bool chipCardRemovedFromReader = true;
		ITrackData trackDataFromResponseEvent = GetTrackDataFromResponseEvent(baseResponseEvent, ref chipCardRemovedFromReader);
		UpdateTechnicalFallbackState(trackDataFromResponseEvent);
		DeclineChipEnabledCardThatWasSwiped(trackDataFromResponseEvent);
		DeclineAmexFallback(trackDataFromResponseEvent);
		DeclineReservationMobileWallet(trackDataFromResponseEvent);
		UpdateCardReadTechnicalFallbackInfo(trackDataFromResponseEvent);
		UpdateNextCardReadTechnicalFallbackState(trackDataFromResponseEvent);
		CardHelper.CheckForTrackDataErrors(trackDataFromResponseEvent);
		UpdateCreditCardHealthService(trackDataFromResponseEvent, baseResponseEvent, readEntry);
		if (onComplete == null)
		{
			LogHelper.Instance.Log("Unable to Process OnCardReadComplete, no actions are registered.");
		}
		else
		{
			onComplete(request, new CardReadCompleteDetails
			{
				TrackData = trackDataFromResponseEvent,
				VasData = (baseResponseEvent as ICardReadResponseEvent)?.GetBase87CardReadModel()?.VasData,
				CardRemoved = chipCardRemovedFromReader,
				RequestId = baseResponseEvent?.RequestId,
				Success = (baseResponseEvent?.Success ?? false)
			});
		}
		if (trackDataFromResponseEvent != null && !trackDataFromResponseEvent.HasValidData())
		{
			ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>().ReportAuthorizeResult(authorizeSuccessful: false);
		}
	}

	private void CardReadVasOnlyResponseHandler(BaseResponseEvent baseResponseEvent, CardReadRequestInfo request, Action<CardReadRequestInfo, CardReadCompleteDetails> onComplete)
	{
		_readCardJob = null;
		bool chipCardRemovedFromReader = false;
		ITrackData trackDataFromResponseEvent = GetTrackDataFromResponseEvent(baseResponseEvent, ref chipCardRemovedFromReader);
		Base87CardReadModel base87CardReadModel = (baseResponseEvent as ICardReadResponseEvent)?.GetBase87CardReadModel();
		bool success = baseResponseEvent != null && baseResponseEvent.Success && base87CardReadModel != null && base87CardReadModel.Status == ResponseStatus.Success && base87CardReadModel.HasVasData;
		if (!_cardReadAborted)
		{
			onComplete?.Invoke(request, new CardReadCompleteDetails
			{
				RequestId = baseResponseEvent.RequestId,
				Success = success,
				TrackData = trackDataFromResponseEvent,
				VasData = base87CardReadModel?.VasData,
				CardRemoved = true
			});
		}
	}

	private static void UpdateCreditCardHealthService(ITrackData trackData, BaseResponseEvent responseEvent, ReadCardJobEntry readEntry)
	{
		if (ResponseEventIsTimedOut(responseEvent) || (readEntry != null && readEntry.IsZeroTouchRead) || GetCardReadAttemptCancelled(responseEvent) || GetCardReadResponseCancelledOrBlocked(responseEvent))
		{
			LogHelper.Instance.Log("Card Read Cancelled or Blocked or ZeroTouch or Timed Out, Don't Notify Health Service.");
			return;
		}
		ICCReaderHealth service = ServiceLocator.Instance.GetService<ICCReaderHealth>();
		bool flag = trackData?.HasValidData() ?? false;
		bool hasError = !flag;
		service?.EventOccurred(flag, hasError);
	}

	private static bool ResponseEventIsTimedOut(BaseResponseEvent responseEvent)
	{
		return responseEvent is TimeoutResponseEvent;
	}

	private static bool GetCardReadAttemptCancelled(BaseResponseEvent responseEvent)
	{
		bool result = false;
		if (responseEvent != null)
		{
			result = ((ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession())?.CardReadAttemptCollection?.GetByCardReadJobCommandRequestId(responseEvent.RequestId))?.AttemptedCancel ?? false;
		}
		return result;
	}

	private static bool GetCardReadResponseCancelledOrBlocked(BaseResponseEvent responseEvent)
	{
		Base87CardReadModel base87CardReadModel = (responseEvent as ICardReadResponseEvent)?.GetBase87CardReadModel();
		if (base87CardReadModel != null)
		{
			if (base87CardReadModel.Status != ResponseStatus.Cancelled)
			{
				return base87CardReadModel.Status == ResponseStatus.Blocked;
			}
			return true;
		}
		return false;
	}

	private ITrackData GetTrackDataFromEMVCardReadResponseEvent(BaseResponseEvent baseResponseEvent, ref bool chipCardRemovedFromReader)
	{
		ITrackData result = null;
		EMVCardReadModel eMVCardReadModel = (baseResponseEvent as EMVCardReadResponseEvent)?.Data;
		if (eMVCardReadModel != null)
		{
			chipCardRemovedFromReader = eMVCardReadModel.CardRemoved;
			EMVTrackData eMVTrackData = new EMVTrackData();
			PopulateEncryptedTrackDataFields(eMVTrackData, eMVCardReadModel);
			eMVTrackData.Tags = new Dictionary<string, string>(eMVCardReadModel.Tags);
			eMVTrackData.AID = eMVCardReadModel.AID;
			result = eMVTrackData;
		}
		return result;
	}

	private static void UpdateCardReadTechnicalFallbackInfo(ITrackData trackData)
	{
		if (trackData == null)
		{
			return;
		}
		IRentalSessionService service = ServiceLocator.Instance.GetService<IRentalSessionService>();
		IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
		ISession session = service?.GetCurrentSession();
		switch (trackData.FallbackStatusAction)
		{
		case FallbackStatusAction.IncrementFallbackCounter:
			if (session != null)
			{
				session.CardReadTechnicalFallbackCount++;
				session.LastFallbackType = trackData?.FallbackReason;
			}
			break;
		case FallbackStatusAction.ResetFallbackCounter:
			if (session != null)
			{
				session.CardReadTechnicalFallbackCount = 0;
				if (!trackData.IsInTechnicalFallback)
				{
					session.LastFallbackType = null;
				}
			}
			break;
		case FallbackStatusAction.ImmediateFallback:
			if (session != null && service2 != null)
			{
				session.CardReadTechnicalFallbackCount = service2.KioskSession.DeviceService.CardReadTechnicalFallbackCountLimit;
				session.LastFallbackType = trackData?.FallbackReason;
			}
			break;
		case FallbackStatusAction.NoAction:
			break;
		}
	}

	private void UpdateTechnicalFallbackState(ITrackData trackData)
	{
		if (trackData != null)
		{
			trackData.IsInTechnicalFallback = IsInTechnicalFallback;
		}
	}

	private void UpdateNextCardReadTechnicalFallbackState(ITrackData trackData)
	{
		if (trackData != null)
		{
			trackData.NextCardReadIsInTechnicalFallback = IsInTechnicalFallback;
		}
	}

	private void DeclineAmexFallback(ITrackData trackData)
	{
		if (trackData != null && trackData.IsInTechnicalFallback && trackData.CardSourceType == CardSourceType.Swipe && trackData.CardHasChip && trackData.CardType == Redbox.Core.CardType.AmericanExpress)
		{
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			if (trackData != null && trackData.ChipEnabledAndSupportsEmv && (service == null || service.KioskSession.DeviceService.CardReadTechnicalFallbackEnabled))
			{
				trackData.Errors.Add(CardHelper.ErrorCodes.NewAmexCannotFallbackError());
				trackData.FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter;
			}
		}
	}

	private void DeclineChipEnabledCardThatWasSwiped(ITrackData trackData)
	{
		if (trackData != null && !trackData.IsInTechnicalFallback && trackData.CardSourceType == CardSourceType.Swipe && trackData.CardHasChip && trackData != null && trackData.ChipEnabledAndSupportsEmv)
		{
			trackData.Errors.Add(CardHelper.ErrorCodes.NewSwipedCardIsChipEnabledError());
		}
	}

	private void DeclineReservationMobileWallet(ITrackData trackData)
	{
		ISession session = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		if (trackData != null && session != null && !session.IsInZeroTouchMode && session.IsInReservationFlow && trackData.CardSourceType == CardSourceType.Mobile)
		{
			trackData.Errors.Add(CardHelper.ErrorCodes.NewReservationMobileDeviceError());
		}
	}

	private ITrackData GetTrackDataFromEncryptedCardReadResponseEvent(BaseResponseEvent baseResponseEvent)
	{
		ITrackData trackData = null;
		EncryptedCardReadModel encryptedCardReadModel = (baseResponseEvent as EncryptedCardReadResponseEvent)?.Data;
		if (encryptedCardReadModel != null)
		{
			trackData = new EncryptedTrackData();
			PopulateEncryptedTrackDataFields(trackData as IEncryptedTrackData, encryptedCardReadModel);
		}
		return trackData;
	}

	private void PopulateEncryptedTrackDataFields(IEncryptedTrackData encryptedTrackData, IEncryptedCardReadModel encryptedCardReadModel)
	{
		if (encryptedTrackData != null)
		{
			if (encryptedCardReadModel != null)
			{
				encryptedTrackData.PANLength = encryptedCardReadModel.PANLength;
				encryptedTrackData.ExtLangCode = encryptedCardReadModel.ExtLangCode;
				encryptedTrackData.LSEncData = encryptedCardReadModel.LSEncData;
				encryptedTrackData.LSEncDataLength = encryptedCardReadModel.LSEncDataLength;
				encryptedTrackData.AESPAN = encryptedCardReadModel.AESPAN;
				encryptedTrackData.AESPANLength = encryptedCardReadModel.AESPANLength;
				encryptedTrackData.ICEncData = encryptedCardReadModel.ICEncData;
				encryptedTrackData.ICEncDataLength = encryptedCardReadModel.ICEncDataLength;
				encryptedTrackData.KSN = encryptedCardReadModel.KSN;
				encryptedTrackData.MfgSerialNumber = encryptedCardReadModel.MfgSerialNumber;
				encryptedTrackData.EncFormat = encryptedCardReadModel.EncFormat;
				encryptedTrackData.Name = encryptedCardReadModel.Name;
				encryptedTrackData.NameLength = encryptedCardReadModel.NameLength;
				encryptedTrackData.LanguageCode = encryptedCardReadModel.LanguageCode;
				encryptedTrackData.ServiceCode = encryptedCardReadModel.ServiceCode;
				encryptedTrackData.Expiry = encryptedCardReadModel.Expiry;
				encryptedTrackData.Mod10CheckFlag = encryptedCardReadModel.Mod10CheckFlag;
				encryptedTrackData.EncryptedFlag = encryptedCardReadModel.EncryptedFlag;
				encryptedTrackData.InjectedSerialNumber = encryptedCardReadModel.InjectedSerialNumber;
				PopulateBaseCardReadProperties(encryptedTrackData, encryptedCardReadModel as Base87CardReadModel);
			}
			else
			{
				LogHelper.Instance.Log("DeviceServiceTrackDataService.PopulateEncryptedTrackDataFields error: encrypted card read model is null.");
			}
			UpdateDeviceSerialNumber(encryptedTrackData.MfgSerialNumber);
		}
		else
		{
			LogHelper.Instance.Log("DeviceServiceTrackDataService.PopulateEncryptedTrackDataFields error: encrypted track data is null.");
		}
	}

	public bool IsInNumericRange(string data, int start, int? end)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			return false;
		}
		if (!int.TryParse(data, out var result))
		{
			return false;
		}
		if (start > result)
		{
			return false;
		}
		if (end.HasValue && end < result)
		{
			return false;
		}
		return true;
	}

	private ITrackData GetTrackDataFromResponseEvent(BaseResponseEvent baseResponseEvent, ref bool chipCardRemovedFromReader)
	{
		ITrackData trackData = GetTrackDataFromEMVCardReadResponseEvent(baseResponseEvent, ref chipCardRemovedFromReader);
		if (trackData == null)
		{
			trackData = GetTrackDataFromEncryptedCardReadResponseEvent(baseResponseEvent);
			if (trackData == null)
			{
				trackData = GetTrackDataFromUnencryptedCardReadResponseEvent(baseResponseEvent);
				if (trackData == null)
				{
					trackData = GetTrackDataFromCancelResponseEvent(baseResponseEvent);
					if (trackData == null)
					{
						trackData = GetTrackDataFromCardReadUnencryptedResponseEvent(baseResponseEvent);
					}
				}
			}
		}
		return trackData;
	}

	private ITrackData GetTrackDataFromCancelResponseEvent(BaseResponseEvent baseResponseEvent)
	{
		ITrackData result = null;
		if (baseResponseEvent is CancelCommandResponseEvent)
		{
			result = new UnencryptedTrackData
			{
				ReadStatus = ResponseStatus.Cancelled
			};
		}
		return result;
	}

	private ITrackData GetTrackDataFromUnencryptedCardReadResponseEvent(BaseResponseEvent baseResponseEvent)
	{
		ITrackData trackData = null;
		if (baseResponseEvent is UnencryptedCardReadResponseEvent { Data: var data })
		{
			if (data != null)
			{
				if (data.FirstSix == "601056" && data.Track1 == null)
				{
					data.Track1 = "REDBOX";
				}
				trackData = GenerateTrackDataFromTrack1AndTrack2(data.Track1, data.Track2);
				IUnencryptedTrackData unencryptedTrackData = trackData as IUnencryptedTrackData;
				if (unencryptedTrackData != null && unencryptedTrackData.HasValidData())
				{
					unencryptedTrackData.CardSourceType = data.CardSource;
					ITrackDataEncryptionService service = ServiceLocator.Instance.GetService<ITrackDataEncryptionService>();
					unencryptedTrackData.EncryptionCerificateKeyId = service.KeyId;
					unencryptedTrackData.EncryptedAccountNumber = service.GetEncryptedAccountNumber(unencryptedTrackData);
					unencryptedTrackData.EncryptedTrack2 = service.GetEncryptedTrack2(unencryptedTrackData);
				}
				unencryptedTrackData?.LockData();
			}
			PopulateBaseCardReadProperties(trackData, data);
		}
		return trackData;
	}

	private void PopulateBaseCardReadProperties(ITrackData trackData, Base87CardReadModel baseCardReadModel)
	{
		if (trackData == null || baseCardReadModel == null)
		{
			return;
		}
		trackData.FallbackStatusAction = baseCardReadModel.FallbackStatusAction;
		trackData.FallbackReason = baseCardReadModel.FallbackReason;
		trackData.CardHasChip = baseCardReadModel.HasChip;
		trackData.WalletType = baseCardReadModel.WalletFormat;
		trackData.CardSourceType = baseCardReadModel.CardSource;
		trackData.FirstSix = baseCardReadModel?.FirstSix;
		trackData.LastFour = baseCardReadModel?.LastFour;
		baseCardReadModel?.Errors?.ForEach(delegate(DeviceService.ComponentModel.Error x)
		{
			trackData.Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DeviceServiceError", x.Code, x.Message));
		});
		trackData.ReadStatus = baseCardReadModel.Status;
		if (string.IsNullOrEmpty(trackData.LastName))
		{
			CardHelper.ParseNameData(baseCardReadModel?.Name, trackData);
		}
		if (!trackData.CardType.HasValue && baseCardReadModel != null && baseCardReadModel.FirstSix != null && baseCardReadModel != null && baseCardReadModel.LastFour != null)
		{
			string text = string.Empty.PadRight(Math.Max(baseCardReadModel.PANLength - 10, 0), 'X');
			string cardNumber = baseCardReadModel.FirstSix + text + baseCardReadModel.LastFour;
			CardHelper.AssignCardType(trackData, cardNumber);
			if (trackData.CardType == Redbox.Core.CardType.RedboxGiftCard)
			{
				CardHelper.AssignRedboxGiftCardAccountNumber(trackData);
			}
		}
		if (string.IsNullOrEmpty(trackData.ExpiryYear) || string.IsNullOrEmpty(trackData.ExpiryMonth))
		{
			trackData.ExpiryYear = ((baseCardReadModel != null && baseCardReadModel.Expiry != null && baseCardReadModel.Expiry.Length == 4) ? baseCardReadModel.Expiry.Substring(0, 2) : null);
			trackData.ExpiryMonth = ((baseCardReadModel != null && baseCardReadModel.Expiry != null && baseCardReadModel.Expiry.Length == 4) ? baseCardReadModel.Expiry.Substring(2, 2) : null);
		}
		if (baseCardReadModel.HasVasData)
		{
			trackData.VasIdentifier = baseCardReadModel.VasData;
		}
		AddTamperedCardReadErrorsToTrackData(trackData, baseCardReadModel);
	}

	private void AddTamperedCardReadErrorsToTrackData(ITrackData trackData, Base87CardReadModel baseCardReadModel)
	{
		if (trackData != null && baseCardReadModel != null)
		{
			trackData.Errors.AddRange(baseCardReadModel.GetTamperedErrors());
		}
	}

	private ITrackData GetTrackDataFromCardReadUnencryptedResponseEvent(BaseResponseEvent baseResponseEvent)
	{
		ITrackData result = null;
		if (baseResponseEvent is UnencryptedCardReadResponseEvent { Data: not null } unencryptedCardReadResponseEvent)
		{
			result = GenerateTrackDataFromTrack1AndTrack2(unencryptedCardReadResponseEvent.Data.Track1, unencryptedCardReadResponseEvent.Data.Track2);
		}
		return result;
	}

	private ITrackData GenerateTrackDataFromTrack1AndTrack2(string track1, string track2)
	{
		ITrackData result = null;
		if (track1 != null || track2 != null)
		{
			SecureString secureString = new SecureString();
			string text = track1 + ";" + track2 + "?";
			foreach (char c in text)
			{
				secureString.AppendChar(c);
			}
			result = CardHelper.Parse(secureString);
			secureString.Dispose();
		}
		return result;
	}

	private void UpdateDeviceSerialNumber(string current)
	{
		if (!string.IsNullOrWhiteSpace(current))
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Store", "ReaderSerialNumber", current);
		}
	}
}
