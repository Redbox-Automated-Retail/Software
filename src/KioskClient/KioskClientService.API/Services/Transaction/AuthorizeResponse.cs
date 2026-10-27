using System;
using System.Collections.Generic;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Transaction;

public class AuthorizeResponse : BaseResponse, IMessageScrub
{
	public Guid MessageId { get; set; }

	public string Email { get; set; }

	public long ReferenceNumber { get; set; }

	public string CustomerProfileLastName { get; set; }

	public string CustomerProfileFirstName { get; set; }

	public List<RentalLimit> Limits { get; set; }

	public bool HasProfile { get; set; }

	public string CustomerProfileNumber { get; set; }

	public ConfirmationStatus ConfirmationStatus { get; set; }

	public string AccountNumber { get; set; }

	public bool KioskTransactionSuccess { get; set; }

	public bool SubscriptionTransactionSuccess { get; set; }

	public AuthorizeResponse()
		: base(200)
	{
	}

	public AuthorizeResponse(int statusCode)
		: base(statusCode)
	{
	}

	public override object Scrub()
	{
		try
		{
			AuthorizeResponse authorizeResponse = JsonConvert.DeserializeObject<AuthorizeResponse>(this.ToJson());
			authorizeResponse?.ScrubData();
			return authorizeResponse?.ToJsonIndented();
		}
		catch (Exception arg)
		{
			return $"Exception in AuthorizeResponse.Scrub: {arg}";
		}
	}

	public override void ScrubData()
	{
		base.ScrubData();
		CustomerProfileFirstName = ObfuscationHelper.ObfuscateStringData(CustomerProfileFirstName, 1, 0);
		CustomerProfileLastName = ObfuscationHelper.ObfuscateStringData(CustomerProfileLastName, 2, 0);
		Email = ObfuscationHelper.ObfuscateStringData(Email, 1, 0);
	}
}
