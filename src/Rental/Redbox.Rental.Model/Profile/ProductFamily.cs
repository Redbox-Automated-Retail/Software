using System.Collections.Generic;

namespace Redbox.Rental.Model.Profile;

public class ProductFamily
{
	public long ProductFamilyId { get; set; }

	public string ProductFamilyName { get; set; }

	public List<long> Genres { get; set; }
}
