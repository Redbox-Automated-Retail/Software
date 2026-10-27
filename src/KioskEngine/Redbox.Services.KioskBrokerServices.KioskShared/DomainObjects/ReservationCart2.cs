using System;
using System.Collections.Generic;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class ReservationCart2
{
	public int KioskID { get; set; }

	public long ReferenceNumber { get; set; }

	public string CardID { get; set; }

	public string Email { get; set; }

	public DateTime ReserveDate { get; set; }

	public List<ReservationTitle2> Titles { get; set; }

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
}
