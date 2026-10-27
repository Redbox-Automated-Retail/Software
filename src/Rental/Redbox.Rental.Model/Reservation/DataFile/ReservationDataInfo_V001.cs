using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Reservation.DataFile;

public class ReservationDataInfo_V001
{
	private List<ReservationTitle_V001> _reservationTitles = new List<ReservationTitle_V001>();

	private List<int> _historyTitlesIds = new List<int>();

	private List<string> _creditCardIds = new List<string>();

	private List<string> _giftCardIds = new List<string>();

	public int ReservationVersion { get; set; }

	public string ReferenceNumber { get; set; }

	public DateTime TransactionDate { get; set; }

	public bool IsMultiNightPrice { get; set; }

	public bool IsMultiDiscVend { get; set; }

	public bool IsGiftCard { get; set; }

	public bool AuthorizeAtPickup { get; set; }

	public bool IsLoyaltyRedemption { get; set; }

	public bool AppliedTitleMarketing { get; set; }

	public bool AppliedHiveOnlinePromo { get; set; }

	public decimal SubTotal { get; set; }

	public decimal DiscountedSubTotal { get; set; }

	public decimal HiveOnlinePromoDiscount { get; set; }

	public decimal Tax { get; set; }

	public decimal TaxRate { get; set; }

	public decimal DefaultServiceFee { get; set; }

	public decimal ActualServiceFee { get; set; }

	public decimal GrandTotal { get; set; }

	public string PromoCode { get; set; }

	public string ZipCode { get; set; }

	public string CustomerNumber { get; set; }

	public string EmailAddress { get; set; }

	public List<ReservationTitle_V001> ReservationTitles
	{
		get
		{
			return _reservationTitles;
		}
		set
		{
			_reservationTitles = value;
		}
	}

	public List<int> HistoryTitleIds
	{
		get
		{
			return _historyTitlesIds;
		}
		set
		{
			_historyTitlesIds = value;
		}
	}

	public List<string> CreditCardIds
	{
		get
		{
			return _creditCardIds;
		}
		set
		{
			_creditCardIds = value;
		}
	}

	public List<string> GiftCardIds
	{
		get
		{
			return _giftCardIds;
		}
		set
		{
			_giftCardIds = value;
		}
	}

	public ReservationPricingSet_V001 PricingSet { get; set; }
}
