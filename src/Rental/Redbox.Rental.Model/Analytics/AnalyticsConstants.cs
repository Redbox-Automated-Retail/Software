namespace Redbox.Rental.Model.Analytics;

public static class AnalyticsConstants
{
	public static class EventTypes
	{
		public const string ButtonPress = "ButtonPress";

		public const string CheckBoxPressed = "CheckBoxPressed";

		public const string CardRead = "CardRead";

		public const string Timeout = "Timeout";

		public const string OfferDisplayed = "offerDisplayed";

		public const string Login = "Login";

		public const string DiscountRestriction = "DiscountRestriction";
	}

	public static class SessionProperties
	{
		public const string CustomerProfileNumber = "CustomerProfileNumber";

		public const string AccountNumber = "AccountNumber";

		public const string TransactionId = "TransactionId";

		public const string LoggedInCustomerProfileNumber = "LoggedInCustomerProfileNumber";

		public const string Abandon = "Abandon";

		public const string Market = "Market";

		public const string Banner = "Banner";

		public const string DefaultCulture = "DefaultCulture";

		public const string Application = "Application";

		public const string ApplicationVersion = "ApplicationVersion";

		public const string CurrentCulture = "CurrentCulture";

		public const string SessionType = "SessionType";

		public const string IsAda = "IsADA";

		public const string IsAbe = "IsABE";

		public const string SortOrderType = "SortOrderType";

		public const string IsZeroTouch = "IsZeroTouch";
	}

	public static class EventProperties
	{
		public const string ViewMessage = "ViewMessage";

		public const string ShowMessageActorTag = "ShowMessageActorTag";
	}

	public enum SessionType
	{
		None,
		Browse,
		Pickup,
		Return
	}

	public static class ButtonNames
	{
		public const string AnimatedDiscReturnGotIt = "AnimatedDiscReturnGotIt";

		public const string ApplyPromoCancel = "ApplyPromoCancel";

		public const string ApplyPromoEnterPromo = "ApplyPromoEnterPromo";

		public const string BrowseFunctionsBuy = "BrowseFunctionsBuy";

		public const string BrowseFunctionsShoppingCartAddItem = "BrowseFunctionsShoppingCartAddItem";

		public const string BrowseFunctionsRelatedMoviesGotit = "BrowseFunctionsRelatedMoviesGotit";

		public const string BrowseFunctionsOkay = "BrowseFunctionsOkay";

		public const string BrowseFunctionsCancel = "BrowseFunctionsCancel";

		public const string BrowseFunctionsGotit = "BrowseFunctionsGotit";

		public const string BrowseFunctionsRemove = "BrowseFunctionsRemove";

		public const string BrowseFunctionsCancelDigitalCodeIncart = "BrowseFunctionsCancelDigitalCodeIncart";

		public const string BrowseFunctionsOkDigitalCodeIncart = "BrowseFunctionsOkDigitalCodeIncart";

		public const string BrowseAddItem = "BrowseAddItem";

		public const string BrowseViewDisplayProductButton = "BrowseViewDisplayProductButton";

		public const string BrowsePageBack = "BrowsePageBack";

		public const string BrowsePageNext = "BrowsePageNext";

		public const string BrowseMoreCollections = "BrowseMoreCollections";

		public const string BrowseFilterChange = "browse-filter-change";

		public const string BrowseHome = "BrowseHome";

		public const string BrowseQuit = "BrowseQuit";

		public const string BrowseFormat = "BrowseFormat";

		public const string BrowsePriceFilter = "BrowsePriceFilter";

		public const string BrowseCancel = "BrowseCancel";

		public const string BrowseFilter = "BrowseFilter";

		public const string BrowseReserveOnlineRentBack = "BrowseReserveOnlineRentBack";

		public const string BrowseReserveOnlineRentGotit = "BrowseReserveOnlineRentGotit";

		public const string BrowseSignIn = "BrowseSignIn";

		public const string BrowseSignOut = "BrowseSignOut";

		public const string BrowseMyPerks = "BrowseMyPerks";

		public const string BrowseBack = "BrowseBack";

		public const string BrowseCheckOut = "BrowseCheckOut";

		public const string BrowseAddToCart = "BrowseAddToCart";

		public const string BrowseOutOfStockCancel = "BrowseOutOfStockCancel";

		public const string BrowseOutOfStockRelatedMovies = "BrowseOutOfStockRelatedMovies";

		public const string BrowseShoppingCartRemoveItem = "BrowseShoppingCartRemoveItem";

		public const string BrowseBackToAllMovies = "BrowseBackToAllMovies";

		public const string ViewRedboxPlusMovies = "ViewRedboxPlusMovies";

		public const string Continue = "Continue";

		public const string BrowseRedboxPlusTooltip = "BrowseRedboxPlusTooltip";

		public const string BrowseBrowseMessagePopupInfo = "BrowseBrowseMessagePopupInfo";

		public const string BrowseStarzBrowseMessagePopupInfo = "BrowseStarzBrowseMessagePopupInfo";

		public const string BrowseBrowseMessagePopupClose = "BrowseBrowseMessagePopupClose";

		public const string BrowseStarzBrowseMessagePopupClose = "BrowseStarzBrowseMessagePopupClose";

		public const string BrowseQuitMessagePopupKeepBrowsing = "BrowseQuitMessagePopupKeepBrowsing";

		public const string BrowseQuitMessagePopupQuit = "BrowseQuitMessagePopupQuit";

		public const string BrowseWatchOptions = "BrowseWatchOptions";

		public const string BrowseGenreSelectionPopup = "BrowseGenreSelectionPopup";

		public const string BrowseRatingSelectionPopup = "BrowseRatingSelectionPopup";

		public const string BrowseSortSelectionPopup = "BrowseSortSelectionPopup";

		public const string BulletListPopupReserveOnlineGotit = "BulletListPopupReserveOnlineGotit";

		public const string BulletListPopupRedboxPerksGotit = "BulletListPopupRedboxPerksGotit";

		public const string CollectionMessageCancel = "CollectionMessageCancel";

		public const string CollectionMessageSeeAll = "CollectionMessageSeeAll";

		public const string CollectionMessageView = "CollectionMessageView";

		public const string FormatFilterPopupFilter = "FormatFilterPopupFilter";

		public const string HelpBack = "HelpBack";

		public const string HelpPageChange = "HelpPageChange";

		public const string HelpSelectDocument = "HelpSelectDocument";

		public const string KeyboardLogicSignIn = "KeyboardLogicStartBrowsing";

		public const string KeyboardLogicStartSignIn = "KeyboardLogicSignIn";

		public const string KeyboardLogicSignInForgotPassword = "KeyboardLogicSignInForgotPassword";

		public const string KeyboardOk = "KeyboardOk";

		public const string KeyboardCancel = "KeyboardCancel";

		public const string LoyaltySignUpJoinPerks = "LoyaltySignUpJoinPerks";

		public const string LoyaltySignUpCreateAcccount = "LoyaltySignUpCreateAcccount";

		public const string LoyaltySignUpCancel = "LoyaltySignUpCancel";

		public const string LoyaltySignUpTerms = "LoyaltySignUpTerms";

		public const string LoyaltySignUpJoinPerksMarketingOptin = "LoyaltySignUpJoinPerksMarketingOptin";

		public const string LoyaltySignUpBack = "LoyaltySignUpBack";

		public const string MemberPerksViewSeeRedboxPlusMovies = "MemberPerksViewSeeRedboxPlusMovies";

		public const string MemberPerksViewTerms = "MemberPerksViewTerms";

		public const string MemberPerksLogicRedboxPlus = "MemberPerksLogicRedboxPlus";

		public const string MemberPerksLogicPerks = "MemberPerksLogicPerks";

		public const string MemberPerksLogicPromo = "MemberPerksLogicPromo";

		public const string MemberPerksLogicBack = "MemberPerksLogicBack";

		public const string MemberPerksLogicButtonValue = "MemberPerksLogicButtonValue";

		public const string MemberPointsLogicCancel = "MemberPointsLogicCancel";

		public const string MemberPointsLogicOkay = "MemberPointsLogicOkay";

		public const string MemberPointsLogicRedeem = "MemberPointsLogicRedeem";

		public const string MemberPointsLogicCancelRedeem = "MemberPointsLogicCancelRedeem";

		public const string MessagePopupButton1 = "MessagePopupButton1";

		public const string MessagePopupButton2 = "MessagePopupButton2";

		public const string MultiDiscVendOkay = "MultiDiscVendOkay";

		public const string MultiNightPopupOneNightRental = "MultiNightPopupOneNightRental";

		public const string MultiNightPopupMultiNightRental = "MultiNightPopupMultiNightRental";

		public const string MultiNightPopupPurchase = "MultiNightPopupPurchase";

		public const string MultiNightPopupCancel = "MultiNightPopupCancel";

		public const string PerksConfirmAcceptAndSignUpButton = "PerksConfirmAcceptAndSignUpButton";

		public const string PerksConfirmBackButton = "PerksConfirmBackButton";

		public const string PerksConfirmTermsButton = "PerksConfirmTermsButton";

		public const string PersonalizationFunctionsSignOutBack = "PersonalizationFunctionsSignOutBack";

		public const string PersonalizationFunctionsSignOut = "PersonalizationFunctionsSignOut";

		public const string PersonalizationFunctionsNotEnoughPointsGotit = "PersonalizationFunctionsNotEnoughPointsGotit";

		public const string PersonalizationFunctionsPerksPassErrorGotIt = "PersonalizationFunctionsPerksPassErrorGotIt";

		public const string PhoneAndPinStartBrowsing = "PhoneAndPinStartBrowsing";

		public const string PhoneAndPinSignIn = "PhoneAndPinSignIn";

		public const string PhoneAndPinTerms = "PhoneAndPinTerms";

		public const string PhoneAndPinMaybeLater = "PhoneAndPinMaybeLater";

		public const string PhoneAndPinSignUp = "PhoneAndPinSignUp";

		public const string PhoneAndPinTextClubOptin = "PhoneAndPinTextClubOptin";

		public const string PresentOffersContinue = "PresentOffersContinue";

		public const string PresentOffersCancel = "PresentOffersCancel";

		public const string PromoListLogicPageChangeNumber = "PromoListLogicPageChangeNumber";

		public const string PromoListLogicPageBack = "PromoListLogicPageBack";

		public const string PromoListLogicPageNext = "PromoListLogicPageNext";

		public const string PromoListLogicCancelRedeem = "PromoListLogicCancelRedeem";

		public const string PromoListLogicRedeem = "PromoListLogicRedeem";

		public const string PerksOfferDetailsCancel = "PerksOfferDetailsCancel";

		public const string PromoConfirmationCancel = "PromoConfirmationCancel";

		public const string PromoConfirmationApplyPromo = "PromoConfirmationApplyPromo";

		public const string PurchaseFormatSelectionPopupAnalysisText = "PurchaseFormatSelectionPopupAnalysisText";

		public const string PurchaseFormatSelectionPopupReserveOnlineRentBack = "PurchaseFormatSelectionPopupReserveOnlineRentBack";

		public const string PurchaseFormatSelectionPopupReserveOnlineRentGotit = "PurchaseFormatSelectionPopupReserveOnlineRentGotit";

		public const string PurchaseFormatSelectionPopupPurchase = "PurchaseFormatSelectionPopupPurchase";

		public const string PurchaseFormatSelectionPopupCancel = "PurchaseFormatSelectionPopupCancel";

		public const string PurchaseFormatSelectionPopupDigitalCodeInformation = "PurchaseFormatSelectionPopupDigitalCodeInformation";

		public const string PurchaseFormatSelectionPopupOutOfStockCancel = "PurchaseFormatSelectionPopupOutOfStockCancel";

		public const string PurchaseFormatSelectionPopupOutOfStockAddFormat = "PurchaseFormatSelectionPopupOutOfStockAddFormat";

		public const string PurchaseFormatSelectionPopupOutOfStockRelatedMovies = "PurchaseFormatSelectionPopupOutOfStockRelatedMovies";

		public const string PurchaseFormatSelectionPopupCancelWhatDigitalCode = "PurchaseFormatSelectionPopupCancelWhatDigitalCode";

		public const string PurchaseFormatSelectionPopupBuyWhatDigitalCode = "PurchaseFormatSelectionPopupBuyWhatDigitalCode";

		public const string RecommendationOnPickupKeepBrowsing = "RecommendationOnPickupKeepBrowsing";

		public const string RecommendationOnPickupPickup = "RecommendationOnPickupPickup";

		public const string RecommendationOnPickupPay = "RecommendationOnPickupPay";

		public const string RecommendationOnPickupShoppingCartRemoveItem = "RecommendationOnPickupShoppingCartRemoveItem";

		public const string RecommendedTitlesPopupRelatedMoviesTitleDetails = "RecommendedTitlesPopupRelatedMoviesTitleDetails";

		public const string RecommendedTitlesPopupRelatedMoviesAddTitle = "RecommendedTitlesPopupRelatedMoviesAddTitle";

		public const string RecommendedTitlesPopupRecommendedAddItem = "RecommendedTitlesPopupRecommendedAddItem";

		public const string RecommendedTitlesPopupRelatedHowPointsWork = "RecommendedTitlesPopupRelatedHowPointsWork";

		public const string RecommendedTitlesPopupRelatedPickup = "RecommendedTitlesPopupRelatedPickup";

		public const string RecommendedTitlesPopupRelatedNoThanks = "RecommendedTitlesPopupRelatedNoThanks";

		public const string RecommendedTitlesPopupRecommendedHowPointsWork = "RecommendedTitlesPopupRecommendedHowPointsWork";

		public const string RecommendedTitlesPopupRecommendedPickup = "RecommendedTitlesPopupRecommendedPickup";

		public const string RecommendedTitlesPopupRecommendedNoThanks = "RecommendedTitlesPopupRecommendedNoThanks";

		public const string ReservationDetailsOkay = "ReservationDetailsOkay";

		public const string ShoppingCartUpdateADADelete = "ShoppingCartUpdateADADelete";

		public const string ShoppingCartUpdateADAAddGame = "ShoppingCartUpdateADAAddGame";

		public const string ShoppingCartUpdateADAAddMovie = "ShoppingCartUpdateADAAddMovie";

		public const string ShoppingCartUpdateADAOkay = "ShoppingCartUpdateADAOkay";

		public const string ShoppingCartPayNow = "ShoppingCartPayNow";

		public const string ShoppingCartUsePoints = "ShoppingCartUsePoints";

		public const string ShoppingCartPointsBalanceInfo = "ShoppingCartPointsBalanceInfo";

		public const string ShoppingCartSignIn = "ShoppingCartSignIn";

		public const string ShoppingCartDelete = "ShoppingCartDelete";

		public const string ShoppingCartAddPromos = "ShoppingCartAddPromos";

		public const string ShoppingCartRemovePromo = "ShoppingCartRemovePromo";

		public const string ShoppingCartKeepPromo = "ShoppingCartKeepPromo";

		public const string ShoppingCartTermsAndPrivacy = "ShoppingCartTermsAndPrivacy";

		public const string ShoppingCartBack = "ShoppingCartBack";

		public const string ShoppingCartAddMovie = "ShoppingCartAddMovie";

		public const string ShoppingCartAddGame = "ShoppingCartAddGame";

		public const string ShoppingCartUpdateBag = "ShoppingCartUpdateBag";

		public const string ShoppingCartViewStarzOffer = "ShoppingCartViewStarzOffer";

		public const string ShoppingCartViewOfferDetails = "ShoppingCartViewOfferDetails";

		public const string ShoppingCartScanStarzQRCode = "ShoppingCartScanStarzQRCode";

		public const string ShoppingCartScanRedboxPlusQRCode = "ShoppingCartScanRedboxPlusQRCode";

		public const string ShoppingCartViewRedboxPlusOffer = "ShoppingCartViewRedboxPlusOffer";

		public const string ShoppingCartRedboxPlusInfo = "ShoppingCartRedboxPlusInfo";

		public const string ShoppingCartViewFunctionsInCartPromoDecline = "ShoppingCartViewFunctionsInCartPromoDecline";

		public const string ShoppingCartViewFunctionsInCartPromoAccept = "ShoppingCartViewFunctionsInCartPromoAccept";

		public const string ShoppingCartViewFunctionsInCartRemoveItemCancel = "ShoppingCartViewFunctionsInCartRemoveItemCancel";

		public const string ShoppingCartViewFunctionsInCartRemoveItemConfirm = "ShoppingCartViewFunctionsInCartRemoveItemConfirm";

		public const string ShoppingCartViewFunctionsRemoveItem = "ShoppingCartViewFunctionsRemoveItem";

		public const string SignInEmail = "SignInEmail";

		public const string SignInPhone = "SignInPhone";

		public const string SignInCancel = "SignInCancel";

		public const string SignUp = "SignUp";

		public const string SignUpInfo = "SignUpInfo";

		public const string MobilePerksPass = "MobilePerksPass";

		public const string StartStartBrowsing = "StartStartBrowsing";

		public const string StartADAOn = "StartADAOn";

		public const string StartADAOff = "StartADAOff";

		public const string StartReturn = "StartReturn";

		public const string StartPressedButton = "StartPressedButton";

		public const string StartLanguageToggle = "StartLanguageToggle";

		public const string StartHelp = "StartHelp";

		public const string StartTermsAndPrivacy = "StartTermsAndPrivacy";

		public const string StartCarouselCancelAddItem = "StartCarouselCancelAddItem";

		public const string StartAddToCart = "StartAddToCart";

		public const string StartCarouselArt = "StartCarouselArt";

		public const string StartSignOut = "StartSignOut";

		public const string StartSignIn = "StartSignIn";

		public const string StartSignInHome = "StartSignInHome";

		public const string StartSignInTooltip = "StartSignInTooltip";

		public const string StartBannerBluRays = "StartBannerBluRays";

		public const string StartBannerGames = "StartBannerGames";

		public const string StartBannerCollections = "StartBannerCollections";

		public const string StartMovies = "StartMovies";

		public const string StartBuyMovies = "StartBuyMovies";

		public const string StartRentAndBuyMovies = "StartRentAndBuyMovies";

		public const string StartGames = "StartGames";

		public const string StartBuyGames = "StartBuyGames";

		public const string StartRentAndBuyGames = "StartRentAndBuyGames";

		public const string StartScreenSaver99CentsMovies = "StartScreenSaver99CentsMovies";

		public const string StartScreenSaverGames = "StartScreenSaverGames";

		public const string StartScreenSaverTitleDetails = "StartScreenSaverTitleDetails";

		public const string StartScreenSaverBrowsePromotion = "StartScreenSaverBrowsePromotion";

		public const string StartScreenSaverBrowseBrowseFilter = "StartScreenSaverBrowseBrowseFilter";

		public const string SwipeViewMessageButton = "SwipeViewMessageButton";

		public const string SwipeCardSwipeBack = "SwipeCardSwipeBack";

		public const string TitleDetailsRentButton = "TitleDetailsRentButton";

		public const string TitleDetailsOutofStockAddFormat = "TitleDetailsOutofStockAddFormat";

		public const string TitleDetailsOutofStockRelatedMovies = "TitleDetailsOutofStockRelatedMovies";

		public const string TitleDetailsReserveOnlineRentBack = "TitleDetailsReserveOnlineRentBack";

		public const string TitleDetailsReserveOnlineRentGotit = "TitleDetailsReserveOnlineRentGotit";

		public const string TitleDetailsSeeRelatedCollection = "TitleDetailsSeeRelatedCollection";

		public const string TitleDetailsOutofStockCancel = "TitleDetailsOutofStockCancel";

		public const string TitleDetailsBuy = "TitleDetailsBuy";

		public const string TitleDetailsBack = "TitleDetailsBack";

		public const string TitleDetailsRedboxPlusInfo = "TitleDetailsRedboxPlusInfo";

		public const string TitleDetailsPageNext = "TitleDetailsPageNext";

		public const string TitleDetailsPageBack = "TitleDetailsPageBack";

		public const string TitleDetailsMoreLikeThis = "TitleDetailsMoreLikeThis";

		public const string TitleDetailsDualInStockLearnMore = "TitleDetailsDualInStockLearnMore";

		public const string TitleDetailsDualInStockLearnMoreClose = "TitleDetailsDualInStockLearnMoreClose";

		public const string TitleDetailsFullDetailsPopupClose = "TitleDetailsFullDetailsPopupClose";

		public const string TitleDetailsWatchOptionsPopupMoreLikeThis = "TitleDetailsWatchOptionsPopupMoreLikeThis";

		public const string TitleDetailsWatchOptionsPopupSeeFullDetails = "TitleDetailsWatchOptionsPopupSeeFullDetails";

		public const string TitleDetailsWatchOptionsPopupClose = "TitleDetailsWatchOptionsPopupClose";

		public const string UpsellUpgrade = "UpsellUpgrade";

		public const string UpsellBack = "UpsellBack";

		public const string UpsellNoThanks = "UpsellNoThanks";

		public const string EmailOptInRejectOptIn = "EmailOptInRejectOptIn";

		public const string EmailOptInAcceptOptIn = "EmailOptInAcceptOptIn";

		public const string NewCartConfirmRemovePMNPOffer = "NewCartConfirmRemovePMNPOfferToProduct";

		public const string NewCartConfirmAddPMNPOffer = "NewCartConfirmAddPMNPOfferToProduct";

		public const string NewCartConfirmModifyCart = "NewCartConfirmModifyCart";

		public const string NewCartConfirmRemoveProductOffer = "NewCartConfirmRemoveProductOffer";

		public const string NewCartConfirmSubmit = "NewCartConfirmSubmit";

		public const string NewCartConfirmSkipOffer = "NewCartConfirmSkipOffer";

		public const string NewCartConfirmADAAddProductOffer = "NewCartConfirmADAAddProductOffer";

		public const string NewCartConfirmADARemoveProductOffer = "NewCartConfirmADARemoveProductOffer";

		public const string NewCartConfirmADAOK = "NewCartConfirmADAOK";

		public const string OfferAbandonConfirmOK = "OfferAbandonConfirmOK";

		public const string PerksSignupInfoPopupOK = "PerksSignupInfoPopupOK";

		public const string TextMessageAlertOK = "TextMessageAlertOK";

		public const string StarzOfferDetailBackButton = "StarzOfferDetailBackButton";

		public const string StarzOfferDetailAcceptButton = "StarzOfferDetailAcceptButton";

		public const string StarzOfferScanQRCodeButton = "StarzOfferScanQRCodeButton";

		public const string StarzOfferTermsButton = "StarzOfferTermsButton";

		public const string StarzOfferViewInBagButton = "StarzOfferViewInBagButton";

		public const string StarzTermsAcceptanceCheckMark = "StarzTermsAcceptanceCheckMark";

		public const string StarzTermsAcceptanceAcceptButton = "StarzTermsAcceptanceAcceptButton";

		public const string StarzTermsAcceptanceBackButton = "StarzTermsAcceptanceBackButton";

		public const string StarzTermsAcceptanceTermsAndPrivacyButton = "StarzTermsAcceptanceTermsAndPrivacyButton";

		public const string RedboxPlusInfoPopupClose = "RedboxPlusInfoPopupClose";

		public const string SubscriptionSignUpConfirmationGotItButton = "SubscriptionSignUpConfirmationGotItButton";

		public const string RedboxPlusOfferSelectionBackButton = "RedboxPlusOfferSelectionBackButton";

		public const string RedboxPlusOfferSelectionTermsButton = "RedboxPlusOfferSelectionTermsButton";

		public const string RedboxPlusOfferSelectionTier1AcceptButton = "RedboxPlusOfferSelectionTier1AcceptButton";

		public const string RedboxPlusOfferSelectionTier2AcceptButton = "RedboxPlusOfferSelectionTier2AcceptButton";

		public const string RedboxPlusOfferSelectionScanQRCodeButton = "RedboxPlusOfferSelectionScanQRCodeButton";

		public const string RedboxPlusOfferDetailBackButton = "RedboxPlusOfferDetailBackButton";

		public const string RedboxPlusOfferDetailTermsButton = "RedboxPlusOfferDetailTermsButton";

		public const string RedboxPlusOfferDetailAcceptButton = "RedboxPlusOfferDetailAcceptButton";

		public const string RedboxPlusOfferDetailViewInBagButton = "RedboxPlusOfferDetailViewInBagButton";

		public const string RedboxPlusTermsAcceptanceCheckMark = "RedboxPlusTermsAcceptanceCheckMark";

		public const string RedboxPlusTermsAcceptanceAcceptButton = "RedboxPlusTermsAcceptanceAcceptButton";

		public const string RedboxPlusTermsAcceptanceBackButton = "RedboxPlusTermsAcceptanceBackButton";

		public const string RedboxPlusTermsAcceptanceTermsAndPrivacyButton = "RedboxPlusTermsAcceptanceTermsAndPrivacyButton";

		public const string AvodLeadGenerationOfferNoButton = "AvodLeadGenerationOfferNoButton";

		public const string AvodLeadGenerationOfferEmailButton = "AvodLeadGenerationOfferEmailButton";

		public const string AvodLeadGenerationOfferQRCodeButton = "AvodLeadGenerationOfferQRCodeButton";

		public const string RedboxPlusLeadGenerationOfferNoButton = "RedboxPlusLeadGenerationOfferNoButton";

		public const string RedboxPlusLeadGenerationOfferEmailButton = "RedboxPlusLeadGenerationOfferEmailButton";

		public const string RedboxPlusLeadGenerationOfferQRCodeButton = "RedboxPlusLeadGenerationOfferQRCodeButton";

		public const string LeadGenerationOfferConfirmationGotItButton = "LeadGenerationOfferConfirmationGotItButton";

		public const string ReturnVisitPromoOfferClose = "ReturnVisitPromoOfferClose";

		public const string ReturnVisitPromoOfferUseNow = "ReturnVisitPromoOfferUseNow";

		public const string ReturnSuccessWithBrowseClose = "ReturnSuccessWithBrowseClose";

		public const string ReturnSuccessWithBrowseSeeNewMovies = "ReturnSuccessWithBrowseSeeNewMovies";

		public const string GenreSelectionPopupGenre = "GenreSelectionPopupGenre";

		public const string GenreSelectionPopupCancel = "GenreSelectionPopupCancel";

		public const string GenreSelectionPopupApply = "GenreSelectionPopupApply";

		public const string RatingSelectionPopupRating = "RatingSelectionPopupRating";

		public const string RatingSelectionPopupCancel = "RatingSelectionPopupCancel";

		public const string RatingSelectionPopupApply = "RatingSelectionPopupApply";

		public const string SortSelectionPopupSort = "SortSelectionPopupSort";

		public const string SortSelectionPopupCancel = "SortSelectionPopupCancel";

		public const string SortSelectionPopupApply = "SortSelectionPopupApply";
	}

	public static class DataTypeNames
	{
		public const string Pricing = "Pricing";

		public const string RentalPricing = "RentalPricing";

		public const string PurchasePricing = "PurchasePricing";

		public const string Product = "Product";

		public const string ROPRecommendedProducts = "ROPRecommendedProducts";

		public const string ShoppingCart = "ShoppingCart";

		public const string Authorization = "Authorization";

		public const string ShoppingCartItem = "ShoppingCartItem";

		public const string NameValue = "NameValue";

		public const string Collection = "Collection";

		public const string MultiNightPopup = "MultiNightPopup";

		public const string Offer = "Offer";

		public const string Swipe = "Swipe";
	}

	public static class NameValueNames
	{
		public const string CollectionsPopupReason = "CollectionsPopupReason";

		public const string MultipNightPopupShowBuyButton = "MultipNightPopupShowBuyButton";

		public const string PromoCode = "PromoCode";
	}
}
