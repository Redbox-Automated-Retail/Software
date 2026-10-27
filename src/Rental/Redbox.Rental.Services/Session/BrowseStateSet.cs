using System.Collections.Generic;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class BrowseStateSet : IBrowseStateSet
{
	private BrowseSelectedSort _browseSelectedSort;

	public bool IsNonBrowseView { get; set; }

	public BrowseViewFilter BrowseMode { get; set; }

	public TitleFamily TitleFamily { get; set; }

	public Genres? Genre { get; set; }

	public Ratings? Rating { get; set; }

	public BrowseSort? Sort { get; set; }

	public int PageNumber { get; set; }

	public TitleType TitleType { get; set; }

	public string SelectedAlphaGroupName { get; set; }

	public BrowseSelectedSort BrowseSelectedSort
	{
		get
		{
			return _browseSelectedSort;
		}
		set
		{
			_browseSelectedSort = value;
		}
	}

	public List<IBrowseItemModel> BrowseItemModels { get; set; }

	public List<IKioskProduct> BrowseProductsShown { get; set; }

	public List<IBrowseItemModel> TTSBrowseItemModelsShown { get; set; }
}
