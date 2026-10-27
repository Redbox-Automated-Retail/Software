using System;
using System.Collections.Generic;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class ReservationCart3
{
	public int KioskID { get; set; }

	public long ReferenceNumber { get; set; }

	public string CardID { get; set; }

	public bool IsGc { get; set; }

	public List<string> GcIds { get; set; }

	public List<string> CcIds { get; set; }

	public string Email { get; set; }

	public DateTime ReserveDate { get; set; }

	public List<ReservationTitle3> Titles { get; set; }

	public decimal DiscountedSubTotal { get; set; }

	public decimal SubTotal { get; set; }

	public decimal Tax { get; set; }

	public decimal GrandTotal { get; set; }

	public decimal HiveOnlinePromotionDiscount { get; set; }

	public int NumberOfCreditsRemaining { get; set; }

	public string PromoCode { get; set; }

	public string CustomerNumber { get; set; }

	public bool AppliedCredit { get; set; }

	public bool AppliedHiveOnlinePromotion { get; set; }

	public bool AppliedTitleMarketing { get; set; }

	public List<int> HistoryTitleIDs { get; set; }

	public decimal TaxRate { get; set; }

	public string Zip { get; set; }

	public bool IsMnp { get; set; }

	public bool IsMdv { get; set; }

	public bool LoyaltyRedemption { get; set; }

	public decimal DefaultServiceFeeAmount { get; set; }

	public decimal ActualServiceFeeAmount { get; set; }

	public bool AuthorizeAtPickup { get; set; }

	public ReservationCardHash ReservationCardHash { get; set; }

	public List<ReservationCardHash> AlternateCardHashes { get; set; }
}
