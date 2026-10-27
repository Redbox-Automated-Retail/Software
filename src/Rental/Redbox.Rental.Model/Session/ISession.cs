using System;
using System.Collections.Generic;
using DeviceService.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.Transaction;

namespace Redbox.Rental.Model.Session;

public interface ISession
{
	Guid KioskSessionId { get; }

	IBrowseState BrowseState { get; set; }

	IHelpState HelpState { get; set; }

	IPersonalizationSession PersonalizationSession { get; set; }

	IEmailConfirmResult EmailConfirmResult { get; set; }

	IUserInfo UserInfo { get; set; }

	ITrackData TrackData { get; set; }

	string CustomerProfileNumber { get; }

	List<IReservation> ValidatedReservations { get; set; }

	ICardReadAttemptCollection CardReadAttemptCollection { get; }

	bool LastCardReadAuthorizeSuccess { get; }

	bool LastCardReadAttemptSuccess { get; }

	int CardReadTechnicalFallbackCount { get; set; }

	bool HasReceivedUserInteraction { get; }

	bool HasReceivedUserInteractionThatStartsTimedSession { get; }

	string SessionStartTriggerAction { get; set; }

	ISessionStartTriggerActionList SessionStartTriggerActions { get; }

	bool WaitingForCardReaderConnection { get; set; }

	IPreauthorizeResult PreauthorizeResult { get; set; }

	IPromoOfferState PromoOfferState { get; set; }

	FallbackType? LastFallbackType { get; set; }

	ITransactionData TransactionData { get; set; }

	bool IsInZeroTouchMode { get; set; }

	bool IsInReservationFlow { get; set; }

	bool IsVendProcessStarted { get; set; }

	bool IsInAddProductFlow { get; set; }

	TimeSpan RentalReturnTime { get; set; }

	void ClearTrackData();
}
