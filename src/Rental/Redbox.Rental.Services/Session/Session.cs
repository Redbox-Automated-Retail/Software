using System;
using System.Collections.Generic;
using System.Linq;
using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment.CardDataReader;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.Transaction;

namespace Redbox.Rental.Services.Session;

public class Session : ISession
{
	private bool _isInAddProductFlow;

	public static ISession Instance { get; set; }

	public Guid KioskSessionId { get; set; } = Guid.NewGuid();

	public IBrowseState BrowseState { get; set; }

	public IHelpState HelpState { get; set; }

	public IPersonalizationSession PersonalizationSession { get; set; }

	public IEmailConfirmResult EmailConfirmResult { get; set; }

	public ITrackData TrackData { get; set; }

	public IUserInfo UserInfo { get; set; }

	public IPromoOfferState PromoOfferState { get; set; }

	public string CustomerProfileNumber
	{
		get
		{
			string text = null;
			if (PersonalizationSession != null && !string.IsNullOrEmpty(PersonalizationSession.CustomerProfileNumber))
			{
				return PersonalizationSession.CustomerProfileNumber;
			}
			return UserInfo?.CustomerProfileNumber;
		}
	}

	public List<IReservation> ValidatedReservations { get; set; }

	public ICardReadAttemptCollection CardReadAttemptCollection { get; }

	public bool LastCardReadAuthorizeSuccess
	{
		get
		{
			bool result = false;
			ICardReadAttempt cardReadAttempt = CardReadAttemptCollection?.GetMostRecent();
			if (cardReadAttempt != null)
			{
				result = cardReadAttempt?.AuthorizeResult?.Success == true;
			}
			return result;
		}
	}

	public bool LastCardReadAttemptSuccess
	{
		get
		{
			bool result = false;
			ICardReadAttempt cardReadAttempt = CardReadAttemptCollection?.GetMostRecent();
			if (cardReadAttempt != null)
			{
				result = cardReadAttempt.CardReadAttemptStatus == CardReadAttemptStatus.Success;
			}
			return result;
		}
	}

	public int CardReadTechnicalFallbackCount { get; set; }

	public FallbackType? LastFallbackType { get; set; }

	public bool HasReceivedUserInteraction => SessionStartTriggerActions.Any();

	public bool HasReceivedUserInteractionThatStartsTimedSession => SessionStartTriggerActions.Any((ISessionStartTriggerAction x) => x.StartedTimedSession);

	public string SessionStartTriggerAction { get; set; }

	public ISessionStartTriggerActionList SessionStartTriggerActions { get; private set; }

	public bool WaitingForCardReaderConnection { get; set; }

	public IPreauthorizeResult PreauthorizeResult { get; set; }

	public ITransactionData TransactionData { get; set; }

	public TimeSpan RentalReturnTime { get; set; }

	public bool IsInZeroTouchMode { get; set; }

	public bool IsVendProcessStarted { get; set; }

	public bool IsInReservationFlow { get; set; }

	public bool IsInAddProductFlow
	{
		get
		{
			return _isInAddProductFlow;
		}
		set
		{
			if (value != IsInAddProductFlow)
			{
				_isInAddProductFlow = value;
				LogHelper.Instance.Log($"IsInAddProductFlow set to {value}");
			}
		}
	}

	public Session()
	{
		BrowseState = new BrowseState();
		HelpState = new HelpState();
		UserInfo = new UserInfo();
		ValidatedReservations = null;
		CardReadAttemptCollection = new CardReadAttemptCollection();
		TransactionData = new TransactionData();
		RentalReturnTime = TimeSpan.FromHours(21.0);
		SessionStartTriggerActions = new SessionStartTriggerActionList();
		LogHelper.Instance.Log($"Created KisokSession with id: {KioskSessionId}");
	}

	public void ClearTrackData()
	{
		LogHelper.Instance.Log("Clearing track data for session");
		TrackData = null;
		if (CardReadAttemptCollection == null)
		{
			return;
		}
		foreach (ICardReadAttempt item in CardReadAttemptCollection)
		{
			item.TrackData = null;
		}
	}
}
