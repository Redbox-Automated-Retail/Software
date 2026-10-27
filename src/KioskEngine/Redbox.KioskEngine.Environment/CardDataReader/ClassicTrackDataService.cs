using System;
using System.Security;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.Session;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public class ClassicTrackDataService : IClassicTrackDataService, ITrackDataService
{
	private Action<CardReadRequestInfo, CardReadCompleteDetails> _onCardReadComplete;

	private CardReadRequestInfo _cardReadRequest;

	private const char TrackEnd = '?';

	private const char TrackStart = '%';

	private const string StartSentinel = ";";

	private bool _allowTrackDataParsing;

	private bool _receivingTrackData;

	public bool IsOlderReader => true;

	public bool IsNewReader => false;

	public bool AllowTrackDataParsing => _allowTrackDataParsing;

	private SecureString _trackDataBuffer { get; set; }

	private void ClearData()
	{
		_trackDataBuffer?.Clear();
	}

	public void StartCardRead(CardReadRequestInfo request, Action<IReadCardJob> cardReadStartedAction, Action<Guid> onCardProcessing, Action<CardReadRequestInfo, CardReadCompleteDetails> onCardReadComplete)
	{
		_cardReadRequest = request;
		_onCardReadComplete = onCardReadComplete;
		LogHelper.Instance.Log("ClassicTrackDataService start card read");
		_allowTrackDataParsing = true;
		ClearData();
	}

	public void StopCardRead(CancelReason cancelReason)
	{
		LogHelper.Instance.Log("ClassicTrackDataService stop card read");
		if (_allowTrackDataParsing)
		{
			_allowTrackDataParsing = false;
			ICCReaderHealth service = ServiceLocator.Instance.GetService<ICCReaderHealth>();
			ITrackData trackData = (ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession())?.TrackData;
			if (cancelReason == CancelReason.LeavingSwipeView)
			{
				bool flag = trackData != null || (trackData?.HasValidData() ?? false);
				bool hasError = !flag;
				service?.EventOccurred(flag, hasError);
			}
		}
	}

	public void StopAllSessionCardReads(Guid sessionId, CancelReason cancelReason)
	{
	}

	public bool ProcessKey(char keyChar, Action onTrackEnd)
	{
		bool result = false;
		if (_allowTrackDataParsing)
		{
			if (keyChar == '%')
			{
				_receivingTrackData = true;
				_trackDataBuffer = new SecureString();
				LogHelper.Instance.Log("ClassicTrackDataService card read: track start detected");
			}
			if (_receivingTrackData)
			{
				_trackDataBuffer?.AppendChar(keyChar);
				if (keyChar == '?' && TrackDataBufferContainsStartSentinel())
				{
					LogHelper.Instance.Log("ClassicTrackDataService card read: track end detected");
					_receivingTrackData = false;
					onTrackEnd?.Invoke();
					IUnencryptedTrackData unencryptedTrackData = CardHelper.Parse(_trackDataBuffer);
					if (unencryptedTrackData != null)
					{
						unencryptedTrackData.CardSourceType = CardSourceType.Unknown;
					}
					if (unencryptedTrackData != null && unencryptedTrackData.HasValidData())
					{
						ITrackDataEncryptionService service = ServiceLocator.Instance.GetService<ITrackDataEncryptionService>();
						unencryptedTrackData.EncryptionCerificateKeyId = service.KeyId;
						unencryptedTrackData.EncryptedAccountNumber = service.GetEncryptedAccountNumber(unencryptedTrackData);
						unencryptedTrackData.EncryptedTrack2 = service.GetEncryptedTrack2(unencryptedTrackData);
					}
					unencryptedTrackData?.LockData();
					_onCardReadComplete?.Invoke(_cardReadRequest, new CardReadCompleteDetails
					{
						TrackData = unencryptedTrackData,
						CardRemoved = true,
						Success = (unencryptedTrackData?.HasValidData() ?? false)
					});
					_trackDataBuffer.Clear();
				}
				result = true;
			}
		}
		return result;
	}

	private bool TrackDataBufferContainsStartSentinel()
	{
		if (_trackDataBuffer != null)
		{
			return CardHelper.ExtractFromSecure(_trackDataBuffer).IndexOf(";") > 0;
		}
		return false;
	}
}
