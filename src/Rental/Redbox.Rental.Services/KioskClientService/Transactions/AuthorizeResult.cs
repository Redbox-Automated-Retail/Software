using System;
using DeviceService.ComponentModel;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class AuthorizeResult : BaseResponse, IAuthorizeResult, IMessageScrub
{
	public Guid MessageId { get; set; }

	public Guid SessionId { get; set; }

	public AuthorizeType AuthorizeType { get; set; }

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

	public object Scrub()
	{
		try
		{
			AuthorizeResult authorizeResult = JsonConvert.DeserializeObject<AuthorizeResult>(JsonConvert.SerializeObject(this));
			authorizeResult?.ScrubData();
			return JsonConvert.SerializeObject(authorizeResult, Formatting.Indented);
		}
		catch (Exception arg)
		{
			return $"Exception in AuthorizeResult.Scrub: {arg}";
		}
	}

	public override void ScrubData()
	{
		base.ScrubData();
		IObfuscationService service = ServiceLocator.Instance.GetService<IObfuscationService>();
		FirstName = service?.ObfuscateStringData(FirstName, 1, 0);
		LastName = service?.ObfuscateStringData(LastName, 2, 0);
		Email = service?.ObfuscateStringData(Email, 1, 0);
	}
}
