using System.Collections.Generic;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class TitleRollupProductGroup
{
	public long ProductGroupId { get; set; }

	public string ProductGroupName { get; set; }

	public int ProductGroupTypeId { get; set; }

	public List<long> ProductIds { get; set; }

	public int SortOrderPosition { get; set; }

	public static TitleRollupProductGroup CopyFrom(ProductGroup pg)
	{
		List<long> list = new List<long>();
		list.AddRange(pg.ProductIds);
		return new TitleRollupProductGroup
		{
			ProductGroupId = pg.ProductGroupId,
			ProductGroupName = pg.ProductGroupName,
			ProductGroupTypeId = pg.ProductGroupTypeId,
			ProductIds = list,
			SortOrderPosition = 0
		};
	}
}
