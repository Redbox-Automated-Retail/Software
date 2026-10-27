using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Profile;

public class Title
{
	public long ProductId { get; set; }

	public string RunningTime { get; set; }

	public List<string> Starring { get; set; }

	public int ProductTypeId { get; set; }

	public long? RatingId { get; set; }

	public string LongName { get; set; }

	public string SortName { get; set; }

	public string ImageFile { get; set; }

	public string Description { get; set; }

	public List<long> Genres { get; set; }

	public DateTime? ReleaseDate { get; set; }

	public bool SellThru { get; set; }

	public bool SellThruNew { get; set; }

	public decimal BoxOfficeGross { get; set; }

	public List<string> Directors { get; set; }

	public int ComingSoonDays { get; set; }

	public DateTime? SortDate { get; set; }

	public DateTime? NationalStreetDate { get; set; }

	public bool ClosedCaptioned { get; set; }

	public DateTime? MerchandiseDate { get; set; }

	public long? SubPlatformId { get; set; }

	public string NumberOfPlayersText { get; set; }

	public DateTime? DoNotRentDate { get; set; }

	public DateTime? SellThruDate { get; set; }

	public decimal? SellThruPrice { get; set; }

	public int? DiscNumber { get; set; }

	public decimal? DigitalSDRentPrice { get; set; }

	public decimal? DigitalHDRentPrice { get; set; }

	public string Studio { get; set; }

	public ProductGroup ProductGroup { get; set; }

	public int SortOrderPosition => ProductGroup?.SortOrderPosition ?? 0;

	public bool IsEmptyCase => ProductId == 1139;

	public bool IsRentable
	{
		get
		{
			if (IsEmptyCase)
			{
				return false;
			}
			if (DoNotRent)
			{
				return false;
			}
			return IsAvailable;
		}
	}

	public DateTime? RedboxPlusEligibleDate { get; set; }

	public bool IsRedboxPlusEligible
	{
		get
		{
			if (!RedboxPlusEligibleDate.HasValue)
			{
				return false;
			}
			return RedboxPlusEligibleDate <= DateTime.Now;
		}
	}

	public bool DoNotRent
	{
		get
		{
			if (!DoNotRentDate.HasValue)
			{
				return false;
			}
			return DoNotRentDate <= DateTime.Now;
		}
	}

	public bool IsAvailable => DateTime.Now > (MerchandiseDate ?? DateTime.Today);

	public DiscountRestriction DiscountRestriction { get; set; }

	public override string ToString()
	{
		return $"MovieTitle:{ProductId} {SortName} SortOrder: {SortOrderPosition} Group: {ProductGroup?.ProductGroupId} {ProductGroup?.ProductGroupName}";
	}
}
