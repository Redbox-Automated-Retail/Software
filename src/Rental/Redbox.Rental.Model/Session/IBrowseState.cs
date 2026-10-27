using System.Collections.Generic;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Session;

public interface IBrowseState
{
	bool ProductSelectedFromCarousel { get; set; }

	bool ProductAddedFromCarousel { get; set; }

	bool MovieMatureWarningShown { get; set; }

	bool _4kWarningShown { get; set; }

	bool SubscriptionRemoved { get; set; }

	bool MatureGameWarningShown { get; set; }

	bool IsShoppingCartFullMessagesOpen { get; set; }

	bool ShoppingCartFullMessagesShown { get; set; }

	bool ReturnVisitIneligibleCartWarningShown { get; set; }

	ISignupFlowState SignupFlowState { get; }

	int PaddleClickCount { get; set; }

	BrowseViewFilter BrowseMode { get; set; }

	BrowseSelectedSort BrowseSelectedSort { get; set; }

	string SelectedAlphaGroupName { get; set; }

	TitleFamily TitleFamily { get; set; }

	TitleType TitleType { get; set; }

	BrowsePriceRangeParameter BrowsePriceRangeFilter { get; set; }

	int PageNumber { get; set; }

	Genres? Genre { get; set; }

	Ratings? Rating { get; set; }

	BrowseSort? Sort { get; set; }

	string BrowseStudio { get; set; }

	List<IBrowseItemModel> BrowseItemModels { get; set; }

	IKioskProduct CenterCarouselProduct { get; set; }

	IKioskProduct BrowseSelectionSelectedProduct { get; set; }

	string ProductDetailShownFromView { get; set; }

	string ItemAddedFromView { get; set; }

	bool IsNonBrowseView { get; set; }

	List<IKioskProduct> BrowseProductsShown { get; set; }

	List<IBrowseItemModel> TTSBrowseItemModelsShown { get; set; }

	bool InReturnFlow { get; set; }

	bool NeedMarketingConfirm { get; set; }

	bool EnableBrowseMessagePopup { get; set; }

	int BrowseMessagePopupDuration { get; set; }

	bool ShowBrowseMessagePopup { get; set; }

	bool ShowingBrowseMessagePopup { get; set; }

	bool ShowRedboxPlusTooltip { get; set; }

	bool IsRedboxPlusTermsAcceptanceCheckMarkChecked { get; set; }

	void PushBrowseStateSet();

	IBrowseStateSet PopBrowseStateSet();

	void UnwindBrowseStateSetStack();

	void PushNonBrowseViewBrowseStateSet();

	void SetDefaultBrowseStateSet();
}
