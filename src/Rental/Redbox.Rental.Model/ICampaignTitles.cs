using System.Collections.Generic;

namespace Redbox.Rental.Model;

public interface ICampaignTitles
{
	bool Include { get; set; }

	List<long> Titles { get; set; }

	bool HasTitles { get; }
}
