using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.Rental.Model.Personalization;

public class RecommendedTitleId : IRecommendedTitleId
{
	private List<string> _personalizationTitleTags;

	public long Id { get; set; }

	public RecommendedTitlesIdType IdType { get; set; }

	public bool IsWishList { get; private set; }

	public List<string> PersonalizationTitleTags
	{
		get
		{
			return _personalizationTitleTags;
		}
		set
		{
			_personalizationTitleTags = value;
			IsWishList = _personalizationTitleTags.Any((string x) => x.Equals("wishlist", StringComparison.CurrentCultureIgnoreCase));
		}
	}
}
