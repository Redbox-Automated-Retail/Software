using System;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IAuthorizeResult
{
	Guid SessionId { get; set; }

	AuthorizeType AuthorizeType { get; set; }

	bool Success { get; set; }

	bool IsOnline { get; set; }

	bool StandAlone { get; set; }

	string ServerName { get; set; }

	string Response { get; set; }

	string Email { get; set; }

	string ReferenceNumber { get; set; }

	bool HasProfile { get; set; }

	bool IsRBI { get; set; }

	string FirstName { get; set; }

	string LastName { get; set; }

	string ConfirmationStatus { get; set; }

	string AccountNumber { get; set; }

	string CustomerProfileNumber { get; set; }

	string StoreNumber { get; set; }

	bool GiftCardExists { get; set; }

	GiftCardStatus GiftCardStatus { get; set; }

	decimal GiftCardBalance { get; set; }

	int GiftCardMaxDiscs { get; set; }

	int GiftCardMaxGames { get; set; }

	bool KioskTransactionSuccess { get; set; }

	bool SubscriptionTransactionSuccess { get; set; }
}
