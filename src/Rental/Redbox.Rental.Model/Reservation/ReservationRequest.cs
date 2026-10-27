using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Redbox.Rental.Model.Reservation;

public class ReservationRequest : IReservationRequest
{
	public bool AuthorizeAtPickup { get; set; }

	public decimal ActualServiceFeeAmount { get; set; }

	public decimal DefaultServiceFeeAmount { get; set; }

	public bool LoyaltyRedemption { get; set; }

	public bool IsMdv { get; set; }

	public bool IsMnp { get; set; }

	public string Zip { get; set; }

	public decimal TaxRate { get; set; }

	public List<int> HistoryTitleIDs { get; set; }

	public bool AppliedTitleMarketing { get; set; }

	public bool AppliedHiveOnlinePromotion { get; set; }

	public string CustomerNumber { get; set; }

	public string PromoCode { get; set; }

	public decimal HiveOnlinePromotionDiscount { get; set; }

	public decimal GrandTotal { get; set; }

	public decimal Tax { get; set; }

	public decimal SubTotal { get; set; }

	public decimal DiscountedSubTotal { get; set; }

	public List<ReservationRequestTitle> Titles { get; set; }

	public DateTime ReserveDate { get; set; }

	public string Email { get; set; }

	public List<string> CcIds { get; set; } = new List<string>();

	public List<string> GcIds { get; set; } = new List<string>();

	public bool IsGc { get; set; }

	public string CardID { get; set; }

	[Required]
	public long ReferenceNumber { get; set; }

	public int KioskID { get; set; }

	public ReservationRequestCardHash ReservationCardHash { get; set; }

	public List<ReservationRequestCardHash> AlternateCardHashes { get; set; }
}
