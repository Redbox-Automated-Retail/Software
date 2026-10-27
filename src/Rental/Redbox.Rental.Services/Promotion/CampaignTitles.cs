using System.Collections.Generic;
using System.Linq;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.Promotion;

public class CampaignTitles : ICampaignTitles
{
	public bool Include { get; set; }

	public List<long> Titles { get; set; }

	public bool HasTitles => (Titles?.Count() ?? 0) > 0;
}
