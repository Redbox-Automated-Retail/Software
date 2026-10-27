using System;
using System.Collections.Generic;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Model.KioskProduct;

public interface ITitleProduct : IKioskProduct
{
	long ProductId { get; }

	string RunningTime { get; }

	List<string> Starring { get; }

	int ProductTypeId { get; }

	TitleType TitleType { get; }

	TitleFamily TitleFamily { get; }

	Dictionary<string, BarcodeProduct> BarCodeProducts { get; }

	long? RatingId { get; }

	string LongName { get; }

	string ImageFile { get; }

	string Description { get; }

	List<long> Genres { get; }

	DateTime? ReleaseDate { get; }

	decimal BoxOfficeGross { get; }

	List<string> Directors { get; }

	int ComingSoonDays { get; }

	DateTime? NationalStreetDate { get; }

	bool ClosedCaptioned { get; }

	DateTime MerchandiseDate { get; }

	DateTime? DoNotRentDate { get; }

	long? SubPlatformId { get; }

	string NumberOfPlayersText { get; }

	int? DiscNumber { get; }

	bool IsGame { get; }

	bool IsMovie { get; }

	bool IsSellable { get; }

	bool IsSellableAndInStock { get; }

	bool IsRentable { get; }

	bool IsRentableAndInStock { get; }

	bool IsEmptyCase { get; }

	decimal PurchasePrice { get; }

	IPricingRecord PurchasePriceRecord { get; }

	IList<IPricingRecord> RentalPriceRecords { get; }

	bool SellThru { get; }

	bool SellThruNew { get; }

	DateTime? SellThruDate { get; }

	decimal? SellThruPrice { get; }

	bool DoNotRent { get; }

	bool IsAvailable { get; }

	bool ComingSoon { get; }

	bool InStock { get; }

	bool ShowOutOfStock { get; }

	bool HasBeenInStock { get; }

	bool HasNeverBeenInStockAndSortOrderPosition { get; }

	bool HasDeal { get; }

	bool HasMultiNightPrice { get; }

	string SortAlpha { get; }

	DateTime SortMerchandise { get; }

	DateTime SortDate { get; }

	decimal SortBoxOfficeGross { get; }

	string SortName { get; }

	long TitleRollupProductGroupId { get; }

	int SortOrderType { get; set; }

	int SortOrderPosition { get; set; }

	decimal? DigitalRentPrice { get; }

	List<long> RequiredAdditionalTitles { get; }

	string TivoQueryId { get; set; }

	string Studio { get; }

	DateTime? RedboxPlusEligibleDate { get; }

	bool IsRedboxPlusEligible { get; }

	DiscountRestriction DiscountRestriction { get; }

	string ProductWebPageUrl { get; }
}
