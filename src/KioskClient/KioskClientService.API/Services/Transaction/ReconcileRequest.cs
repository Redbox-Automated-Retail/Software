using System;
using System.Collections.Generic;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Transaction;

public class ReconcileRequest : MessageBase
{
	public long? ReferenceNumber { get; set; }

	public bool Blocklisted { get; set; }

	public string Email { get; set; }

	public DateTimeOffset? TransactionDate { get; set; }

	public CreditCard CreditCard { get; set; }

	public long? ParentTransactionId { get; set; }

	public ShoppingCart ShoppingCart { get; set; }

	public bool IsOffline { get; set; }

	public bool PlayPassPromptAccepted { get; set; }

	public int PlayPassPointsEarned { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string UtcOffset { get; set; }

	public List<DiscOfferCode> DiscOfferCodes { get; set; }

	public bool IsEarlyId { get; set; }

	public bool IsEarlyIdApple { get; set; }

	public bool IsEarlyIdGoogle { get; set; }

	public RedeemedOffer RedeemedCustomerOffer { get; set; }

	public override object Scrub()
	{
		try
		{
			ReconcileRequest reconcileRequest = JsonConvert.DeserializeObject<ReconcileRequest>(JsonConvert.SerializeObject(this));
			reconcileRequest?.ScrubData();
			return JsonConvert.SerializeObject(reconcileRequest, Formatting.Indented);
		}
		catch (Exception arg)
		{
			return $"Exception in ReconcileRequest.Scrub: {arg}";
		}
	}

	public override void ScrubData()
	{
		base.ScrubData();
		Email = ObfuscationHelper.ObfuscateStringData(Email, 1, 0);
		CreditCard?.ScrubData();
	}
}
