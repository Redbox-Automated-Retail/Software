using System.Collections.Generic;

namespace Redbox.Rental.Model.Profile;

public class ProductGroup
{
	public long ProductGroupId { get; set; }

	public string ProductGroupName { get; set; }

	public int ProductGroupTypeId { get; set; }

	public List<long> ProductIds { get; set; }

	public int SortOrderPosition { get; set; }
}
