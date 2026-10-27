using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Reservation;

public interface IReservationRequest
{
	bool AuthorizeAtPickup { get; set; }

	decimal ActualServiceFeeAmount { get; set; }

	decimal DefaultServiceFeeAmount { get; set; }

	bool LoyaltyRedemption { get; set; }

	bool IsMdv { get; set; }

	bool IsMnp { get; set; }

	string Zip { get; set; }

	decimal TaxRate { get; set; }

	List<int> HistoryTitleIDs { get; set; }

	bool AppliedTitleMarketing { get; set; }

	bool AppliedHiveOnlinePromotion { get; set; }

	string CustomerNumber { get; set; }

	string PromoCode { get; set; }

	decimal HiveOnlinePromotionDiscount { get; set; }

	decimal GrandTotal { get; set; }

	decimal Tax { get; set; }

	decimal SubTotal { get; set; }

	decimal DiscountedSubTotal { get; set; }

	List<ReservationRequestTitle> Titles { get; set; }

	DateTime ReserveDate { get; set; }

	string Email { get; set; }

	List<string> CcIds { get; set; }

	List<string> GcIds { get; set; }

	bool IsGc { get; set; }

	string CardID { get; set; }

	long ReferenceNumber { get; set; }

	int KioskID { get; set; }

	ReservationRequestCardHash ReservationCardHash { get; set; }

	List<ReservationRequestCardHash> AlternateCardHashes { get; set; }
}
