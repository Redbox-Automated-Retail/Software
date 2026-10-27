using System;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskEngine.Environment.KioskServices;

public class AuthorizeResult : IAuthorizeResult
{
	public Guid SessionId { get; set; }

	public AuthorizeType AuthorizeType { get; set; }

	public bool Success { get; set; }

	public bool IsOnline { get; set; }

	public bool StandAlone { get; set; }

	public string ServerName { get; set; }

	public string Response { get; set; }

	public string Email { get; set; }

	public string ReferenceNumber { get; set; }

	public bool HasProfile { get; set; }

	public bool IsRBI { get; set; }

	public string FirstName { get; set; }

	public string LastName { get; set; }

	public string ConfirmationStatus { get; set; }

	public string AccountNumber { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string StoreNumber { get; set; }

	public bool GiftCardExists { get; set; }

	public GiftCardStatus GiftCardStatus { get; set; }

	public decimal GiftCardBalance { get; set; }

	public int GiftCardMaxDiscs { get; set; }

	public int GiftCardMaxGames { get; set; }

	public bool KioskTransactionSuccess { get; set; }

	public bool SubscriptionTransactionSuccess { get; set; }
}
