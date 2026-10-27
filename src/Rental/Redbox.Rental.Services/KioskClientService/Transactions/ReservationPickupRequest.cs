using System;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Transactions;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ReservationPickupRequest : MessageBase
{
	public long? ReferenceNumber { get; set; }

	public bool Blocklisted { get; set; }

	public string Email { get; set; }

	public CreditCard CreditCard { get; set; }

	public MessageShoppingCart ShoppingCart { get; set; }

	public string CustomerProfileNumber { get; set; }

	public bool CartWasModified { get; set; }

	public bool IsEarlyId { get; set; }

	public bool IsEarlyIdApple { get; set; }

	public bool IsEarlyIdGoogle { get; set; }

	public RedeemedOffer RedeemedCustomerOffer { get; set; }

	public DateTime PickupDateTime { get; set; }

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
		IObfuscationService service = ServiceLocator.Instance.GetService<IObfuscationService>();
		Email = service.ObfuscateStringData(Email, 1, 0);
		CreditCard.ScrubData();
	}
}
