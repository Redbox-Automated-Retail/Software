using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Personalization;

public class TitleInfo
{
	public long Id { get; set; }

	public RecommendedTitlesIdType IdType { get; set; }

	public List<string> Tags { get; set; }
}
