using System;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class MovieTitle
{
	private Title _title;

	private TitleRollupProductGroup _productGroup;

	public TitleRollupProductGroup ProductGroup => _productGroup;

	public long ProductId => _title.ProductId;

	public int SortOrderPosition => ProductGroup?.SortOrderPosition ?? 0;

	public bool IsEmptyCase => _title.ProductId == 1139;

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

	public bool DoNotRent
	{
		get
		{
			if (!_title.DoNotRentDate.HasValue)
			{
				return false;
			}
			return _title.DoNotRentDate <= DateTime.Now;
		}
	}

	public bool IsAvailable => DateTime.Now > (_title.MerchandiseDate ?? DateTime.Today);

	public MovieTitle(Title title, ProductGroup pg)
	{
		_title = title;
		_productGroup = TitleRollupProductGroup.CopyFrom(pg);
	}

	public override string ToString()
	{
		return $"MovieTitle:{ProductId} {_title.SortName} SortOrder: {SortOrderPosition} Group: {ProductGroup?.ProductGroupId} {ProductGroup?.ProductGroupName}";
	}
}
