using System;
using System.Collections.Generic;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Model.Reservation;

public interface IReservationDetails
{
	string ReferenceNumber { get; set; }

	bool IsMultiNightPrice { get; set; }

	decimal TaxRate { get; set; }

	bool AppliedHiveOnlinePromo { get; set; }

	decimal SubTotal { get; set; }

	DateTime TransactionDate { get; set; }

	string PromoCode { get; set; }

	bool IsMultiDiscVend { get; set; }

	string ZipCode { get; set; }

	bool IsGiftCard { get; set; }

	bool AuthorizeAtPickup { get; set; }

	string CustomerNumber { get; set; }

	decimal GrandTotal { get; set; }

	List<IReservedTitle> ReservedTitles { get; set; }

	decimal Tax { get; set; }

	decimal HiveOnlinePromoDiscount { get; set; }

	string EmailAddress { get; set; }

	int ReservationVersion { get; set; }

	List<int> HistoryTitleIds { get; set; }

	List<string> CreditCardIds { get; set; }

	decimal DiscountedSubTotal { get; set; }

	bool IsLoyaltyRedemption { get; set; }

	bool AppliedTitleMarketing { get; set; }

	List<string> GiftCardIds { get; set; }

	decimal DefaultServiceFee { get; set; }

	decimal ActualServiceFee { get; set; }

	IPricingSet PricingSet { get; set; }
}
