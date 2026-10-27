namespace KioskClientService.API.Services.Configuration;

[Category(Name = "Browse")]
public class BrowseConfiguration : BaseCategorySetting
{
	public bool ShowDealsBrowseFilterButton { get; set; }

	public bool Show99CentsBrowseFilterButton { get; set; }

	public bool EnableSonySweepstakes { get; set; }

	public bool LoadSortOrderProductGroups { get; set; }

	public int NumberOfSortOrderProductGroupsToLoad { get; set; } = 25;

	public int NumberOfOutOfStockProductGroupsToShow { get; set; }

	public bool ShowWatchOptions { get; set; }

	public bool ShowDualInStock { get; set; }

	public bool Show10Titles { get; set; }

	public bool ShowTitleText { get; set; }

	public bool ShowTopFilterButtons { get; set; }
}
