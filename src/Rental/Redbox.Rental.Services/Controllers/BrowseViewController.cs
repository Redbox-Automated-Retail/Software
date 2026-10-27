using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class BrowseViewController : BaseViewController
{
	private static RentalShoppingCartChanged ShoppingCartChangeEventHandler;

	private static List<long> _configuredBrowseItemModels = new List<long>();

	private static List<long> _configuredADAMiniCartBrowseItemModels = new List<long>();

	private const decimal _lowRangeTop = 3.99m;

	private const decimal _midRangeBottom = 4.99m;

	private const decimal _midRangeTop = 5.99m;

	private const decimal _topRangeBottom = 6.99m;

	private const decimal _lowCutoffPrice = 3.99m;

	private const decimal _highCutoffPrice = 5.99m;

	private const decimal _rentalPrice = 0.99m;

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	private static ITimerService TimerService => ServiceLocator.Instance.GetService<ITimerService>();

	private static bool InTitleMarketingMode => BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.InCartTitleMarketing == true;

	private static NotifyInputEventHandler NotifiyInputEventHandler { get; set; }

	private static Action RemovePreNotifyInputHandler { get; set; }

	public static void CacheProductImages()
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<IKioskProduct> products = null;
		service?.GetProducts(BrowseViewFilter.All, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, null, null, null, null, isADAView: false, null, null, out products);
		foreach (IKioskProduct item in products)
		{
			OrchestrationService.GetProductBitmapImage(item);
		}
		service?.GetProducts(BrowseViewFilter.ComingSoon, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, null, null, null, null, isADAView: false, null, null, out products);
		foreach (IKioskProduct item2 in products)
		{
			OrchestrationService.GetProductBitmapImage(item2);
		}
	}

	public static void ConfigureBrowseView(BrowseViewParameters parameters)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			BrowseViewModel browseViewModel = new BrowseViewModel();
			ConfigureBrowseControl(browseViewModel);
			LogHelper.Instance.Log($"ConfigureBrowseView ConfigureBrowseControl stopwatch: {stopwatch.Elapsed}");
			ConfigureMiniCartControl(browseViewModel);
			ConfigureShoppingCartChangeHandler();
			ConfigureTopMenu(browseViewModel);
			ConfigureBrowseFilterBar(viewUserControl, browseViewModel);
			ConfigureCheckOutBar(browseViewModel);
			ConfigureBrowseMessagePopupUserControl(browseViewModel);
			ConfigureBrowseRedboxPlusTooltipUserControl(browseViewModel);
			ConfigureTTS(browseViewModel);
			viewUserControl.DataContext = browseViewModel;
			CommandManager.InvalidateRequerySuggested();
			(IdleTimerService?.GetIdleTimer("idle_timer"))?.Start();
			stopwatch.Stop();
			LogHelper.Instance.Log($"ConfigureBrowseView stopwatch: {stopwatch.Elapsed}");
		}
	}

	private static void ConfigureShoppingCartChangeHandler()
	{
		if (ShoppingCartChangeEventHandler == null)
		{
			ShoppingCartChangeEventHandler = CurrentRentalShoppingCart_OnChange;
			BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.OnChange += ShoppingCartChangeEventHandler;
		}
	}

	public static void CleanupBrowseView()
	{
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && ShoppingCartChangeEventHandler != null)
		{
			try
			{
				rentalShoppingCart.OnChange -= ShoppingCartChangeEventHandler;
			}
			catch
			{
			}
			ShoppingCartChangeEventHandler = null;
		}
	}

	private static void ConfigureTopMenu()
	{
		ConfigureTopMenu(BaseViewController.GetViewModel<BrowseViewModel>());
	}

	private static void ConfigureTopMenu(BrowseViewModel browseViewModel)
	{
		if (browseViewModel == null)
		{
			return;
		}
		OrchestrationService.SetCurrentUICulture();
		TopMenuModel topMenuModel = new TopMenuModel();
		if (!BaseViewController.IsADAMode && !InTitleMarketingMode)
		{
			CreateMenuButtons(topMenuModel);
		}
		topMenuModel.MenuBackgroundStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), (!BaseViewController.IsADAMode) ? "browse_top_menu_white_background_style" : (BrowseFunctions.IsRedboxPlusBrowseMode ? "browse_top_menu_redbox_plus_background_style" : "browse_top_menu_background_style"));
		topMenuModel.MenuBackgroundText = null;
		if (BaseViewController.IsADAMode || InTitleMarketingMode)
		{
			if (BaseViewController.IsADAMode)
			{
				if (BrowseFunctions.IsRedboxPlusBrowseMode)
				{
					string text = ((RedboxPlusFunctions.RedboxPlusSubscriptionTier == RedboxPlusSubscriptionTier.Tier1) ? Resources.redbox_plus_ada_cart_unavailable_tier1_message_text : Resources.redbox_plus_ada_cart_unavailable_tier2_message_text);
					topMenuModel.MenuBackgroundText = ((!RedboxPlusFunctions.IsFreeOneNightRentalAvailable) ? text : ((BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.HasRedboxPlusFreeOneNightRentalSelected == true) ? Resources.redbox_plus_mini_cart_title_selected : (BrowseFunctions.CanAddProductToCart ? Resources.redbox_plus_ada_cart_empty_message_text : Resources.redbox_plus_ada_cart_full_message_text)));
				}
				else
				{
					topMenuModel.MenuBackgroundText = ((BrowseState.TitleFamily == TitleFamily.Games) ? Resources.browse_family_games : Resources.browse_family_movies);
				}
				topMenuModel.MenuTextStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), BrowseFunctions.IsRedboxPlusBrowseMode ? "browse_top_menu_text_ada_redbox_plus_style" : "browse_top_menu_text_ada_style");
			}
		}
		else if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons && !BaseViewController.IsADAMode && BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
		{
			topMenuModel.AZMenuBackgroundVisibility = Visibility.Visible;
			topMenuModel.AZMenuBorderMargin = (OrchestrationService.IsCultureSpanish() ? new Thickness(40.0, 24.0, 187.0, 0.0) : new Thickness(40.0, 24.0, 161.0, 0.0));
			topMenuModel.AZMenuText = Resources.az_menu_text;
			topMenuModel.AZFiltersMessageText = Resources.az_filters_message_text;
		}
		topMenuModel.RedboxPlusLogoVisiblity = ((!BaseViewController.IsADAMode || !BrowseFunctions.IsRedboxPlusBrowseMode) ? Visibility.Collapsed : Visibility.Visible);
		topMenuModel.OnBrowseMenuButtonClicked += HandleBrowseMenuButtonClick;
		browseViewModel.BrowseTopMenuModel = topMenuModel;
	}

	private static void HandleBrowseMenuButtonClick(BrowseMenuButton browseMenuButton)
	{
		if (browseMenuButton == null)
		{
			return;
		}
		BrowseState.SelectedAlphaGroupName = null;
		BrowseProductFamilyMenuButton browseProductFamilyMenuButton = browseMenuButton as BrowseProductFamilyMenuButton;
		if (browseMenuButton is BrowseGenreMenuButton browseGenreMenuButton)
		{
			BrowseState.PageNumber = 1;
			if (browseGenreMenuButton.FilterValue == BrowseState.Genre && browseGenreMenuButton.IsDeselectable)
			{
				BrowseState.Genre = null;
				BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", string.Format(browseGenreMenuButton.Text + ",DESELECT"));
			}
			else
			{
				BrowseState.Genre = browseGenreMenuButton.FilterValue;
				BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", browseGenreMenuButton.Text);
			}
			ConfigureBrowseControl();
			if (BaseViewController.IsADAMode)
			{
				ConfigureMiniCartControl();
			}
		}
		else if (browseProductFamilyMenuButton != null)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", browseProductFamilyMenuButton.Text);
			BrowseState.TitleFamily = browseProductFamilyMenuButton.FilterValue;
			BrowseState.TitleType = TitleType.All;
			BrowseState.PageNumber = 1;
			BrowseState.BrowsePriceRangeFilter = null;
			BrowseState.BrowseStudio = null;
			BrowseState.Genre = null;
			if (BrowseState.TitleFamily == TitleFamily.Games)
			{
				if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Bluray || BrowseState.BrowseSelectedSort == BrowseSelectedSort.DigitalCode || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Dvd || BrowseState.BrowseSelectedSort == BrowseSelectedSort._4KUHD || BrowseState.BrowseSelectedSort == BrowseSelectedSort._99Cents || BrowseState.BrowseSelectedSort == BrowseSelectedSort.SonySweepstakes)
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.ForSale;
					BrowseState.BrowseMode = BrowseViewFilter.ForSale;
				}
				else if (BrowseState.BrowseMode == BrowseViewFilter.ComingSoon)
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.ForSale;
					BrowseState.BrowseMode = BrowseViewFilter.ForSale;
				}
				else if (BrowseState.BrowseMode == BrowseViewFilter.ForSale)
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
					BrowseState.BrowseMode = BrowseViewFilter.All;
				}
				else
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.ForSale;
					BrowseState.BrowseMode = BrowseViewFilter.ForSale;
				}
			}
			else if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Xbox360 || BrowseState.BrowseSelectedSort == BrowseSelectedSort.XboxOne || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Ps2 || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Ps3 || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Ps4 || BrowseState.BrowseSelectedSort == BrowseSelectedSort.NintendoDS || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Wii || BrowseState.BrowseSelectedSort == BrowseSelectedSort.WiiU || BrowseState.BrowseSelectedSort == BrowseSelectedSort.NintendoSwitch)
			{
				BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
			}
			else if (BrowseState.BrowseMode == BrowseViewFilter.ForSale)
			{
				if (OrchestrationService.GetCanSellUsedMovies())
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.ForSale;
					BrowseState.BrowseMode = BrowseViewFilter.ForSale;
				}
				else
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
					BrowseState.BrowseMode = BrowseViewFilter.All;
				}
			}
			ConfigureBrowseView(BrowseFunctions.GetBrowseViewparameters());
		}
		else if (browseMenuButton is SignOutMenuButton)
		{
			HandleSignOutButtonClicked();
		}
		BrowseViewModel viewModel = BaseViewController.GetViewModel<BrowseViewModel>();
		if (viewModel?.BrowseTopMenuModel?.MenuButtons != null)
		{
			foreach (BrowseMenuButton menuButton in viewModel.BrowseTopMenuModel.MenuButtons)
			{
				if (menuButton is BrowseProductFamilyMenuButton || menuButton is SignOutMenuButton)
				{
					menuButton.IsSelected = false;
					continue;
				}
				BrowseGenreMenuButton browseGenreMenuButton2 = menuButton as BrowseGenreMenuButton;
				menuButton.IsSelected = browseMenuButton == menuButton || (browseProductFamilyMenuButton != null && browseProductFamilyMenuButton.FilterValue == TitleFamily.Movies && browseGenreMenuButton2 != null && !browseGenreMenuButton2.FilterValue.HasValue) || (browseProductFamilyMenuButton != null && browseProductFamilyMenuButton.FilterValue == TitleFamily.Games && browseGenreMenuButton2 != null && !browseGenreMenuButton2.FilterValue.HasValue);
			}
		}
		ConfigureTopMenu();
		UpdateQuitButtonsText();
	}

	private static void GenreSelectionPopupButtonModel_OnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		BaseViewController.ProcessWPFHit();
		if (checkMarkButtonModel != null && checkMarkButtonModel.IsSelected)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseGenreSelectionPopup", "Deselect");
			BrowseState.Genre = null;
			BrowseState.PageNumber = 1;
			ConfigureTopMenu();
			ConfigureBrowseControl();
			ConfigureBrowseFilterBar();
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseGenreSelectionPopup");
			OrchestrationService.ViewShow("genre_selection_popup_view");
		}
	}

	private static void RatingSelectionPopupButtonModel_OnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		BaseViewController.ProcessWPFHit();
		if (checkMarkButtonModel != null && checkMarkButtonModel.IsSelected)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseRatingSelectionPopup", "Deselect");
			BrowseState.Rating = null;
			BrowseState.PageNumber = 1;
			ConfigureTopMenu();
			ConfigureBrowseControl();
			ConfigureBrowseFilterBar();
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseRatingSelectionPopup");
			OrchestrationService.ViewShow("rating_selection_popup_view");
		}
	}

	private static void FormatSelectionPopupButtonModel_OnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		BaseViewController.ProcessWPFHit();
		if (checkMarkButtonModel != null && checkMarkButtonModel.IsSelected)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseFormat", "Deselect");
			BrowseState.TitleType = TitleType.All;
			BrowseState.BrowseMode = ((BrowseState.BrowseMode == BrowseViewFilter.RedboxPlus) ? BrowseViewFilter.RedboxPlus : BrowseViewFilter.All);
			BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
			BrowseState.PageNumber = 1;
			ConfigureTopMenu();
			ConfigureBrowseControl();
			ConfigureBrowseFilterBar();
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseFormat");
			OrchestrationService.ViewShow("format_filter_popup_view");
		}
	}

	private static string GetFormatSelectionPopupButtonText()
	{
		return BrowseState.TitleType switch
		{
			TitleType.DVD => Resources.dvd, 
			TitleType.Bluray => Resources.blu_ray.ToUpper(), 
			TitleType._4KUHD => Resources.four_k_uhd, 
			TitleType.DigitalCode => Resources.digital_code.ToUpper(), 
			_ => Resources.format_selection_popup_button_text, 
		};
	}

	private static void DealsFilterButtonModel_OnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		BaseViewController.ProcessWPFHit();
		if (checkMarkButtonModel != null && checkMarkButtonModel.IsSelected)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", "browse-filter-family,Deals,Deselect");
			BrowseState.BrowseMode = ((BrowseState.BrowseMode == BrowseViewFilter.RedboxPlus) ? BrowseViewFilter.RedboxPlus : BrowseViewFilter.All);
			BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", "browse-filter-family,Deals");
			BrowseState.BrowseMode = BrowseViewFilter.Deals;
			BrowseState.BrowseSelectedSort = BrowseSelectedSort.Deals;
		}
		BrowseState.TitleType = TitleType.All;
		BrowseState.PageNumber = 1;
		ConfigureTopMenu();
		ConfigureBrowseControl();
		ConfigureBrowseFilterBar();
	}

	private static void ToBuyFilterButtonModel_OnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		BaseViewController.ProcessWPFHit();
		if (checkMarkButtonModel != null && checkMarkButtonModel.IsSelected)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", "browse-filter-family,for-sale,Deselect");
			BrowseState.BrowseMode = ((BrowseState.BrowseMode == BrowseViewFilter.RedboxPlus) ? BrowseViewFilter.RedboxPlus : BrowseViewFilter.All);
			BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", "browse-filter-family,for-sale");
			BrowseState.BrowseMode = BrowseViewFilter.ForSale;
			BrowseState.BrowseSelectedSort = BrowseSelectedSort.ForSale;
		}
		BrowseState.TitleType = TitleType.All;
		BrowseState.PageNumber = 1;
		ConfigureTopMenu();
		ConfigureBrowseControl();
		ConfigureBrowseFilterBar();
	}

	public static BrowseSort GetBrowseSort()
	{
		if (!ShowSortSelectionPopupButton() || !BrowseState.Sort.HasValue || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
		{
			return BrowseState.BrowseSelectedSort.ConvertToBrowseSort(BrowseState.BrowseMode);
		}
		return BrowseState.Sort.Value;
	}

	private static string GetSortSelectionPopupButtonText(BrowseSort sort)
	{
		return sort switch
		{
			BrowseSort.Alpha => Resources.sort_alpha, 
			BrowseSort.MerchandiseDate => Resources.sort_oldest, 
			BrowseSort.SortDate => Resources.sort_trending, 
			BrowseSort.InitialNightPrice => Resources.sort_price, 
			BrowseSort.MerchandiseDateDescending => Resources.sort_newest, 
			_ => Resources.sort_selection_popup_button_text, 
		};
	}

	private static void SortSelectionPopupButtonModel_OnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseSortSelectionPopup");
		OrchestrationService.ViewShow("sort_selection_popup_view");
	}

	private static void TopMenuModel_OnAZButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", "browse-filter-family,alpha,Deselect");
			BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", "browse-filter-family,alpha");
			BrowseState.BrowseSelectedSort = BrowseSelectedSort.Alpha;
		}
		BrowseState.TitleType = TitleType.All;
		BrowseState.BrowseMode = ((BrowseState.BrowseMode == BrowseViewFilter.RedboxPlus) ? BrowseViewFilter.RedboxPlus : BrowseViewFilter.All);
		BrowseState.Genre = null;
		BrowseState.Rating = null;
		BrowseState.Sort = null;
		BrowseState.SelectedAlphaGroupName = null;
		BrowseState.PageNumber = 1;
		ConfigureTopMenu();
		ConfigureBrowseControl();
		ConfigureBrowseFilterBar();
	}

	private static void CreateMenuButtons(TopMenuModel topMenuModel)
	{
		bool highlightSelectedGenre = true;
		if (BrowseState.TitleFamily == TitleFamily.Movies)
		{
			if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons)
			{
				Style style = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "button_selectable_unselected_style");
				Style style2 = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "button_selectable_selected_style");
				bool flag = BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha;
				bool isRedboxPlusBrowseMode = BrowseFunctions.IsRedboxPlusBrowseMode;
				bool hasValue = BrowseState.Genre.HasValue;
				CheckMarkButtonModel checkMarkButtonModel = new CheckMarkButtonModel(style, style2)
				{
					Visibility = ((!ShowGenreSelectionPopupButton() || flag) ? Visibility.Collapsed : Visibility.Visible),
					IsSelected = hasValue,
					Text = (hasValue ? Resources.ResourceManager.GetString(string.Format($"genre_{(long)BrowseState.Genre.Value}")) : Resources.genre_selection_popup_button_text),
					ShowArrow = !hasValue,
					TextMaxWidth = (isRedboxPlusBrowseMode ? 220 : ((!hasValue) ? 104 : 128))
				};
				checkMarkButtonModel.OnClicked += GenreSelectionPopupButtonModel_OnClicked;
				bool hasValue2 = BrowseState.Rating.HasValue;
				RatingsProperties.Properties.TryGetValue(BrowseState.Rating.GetValueOrDefault(), out var value);
				CheckMarkButtonModel checkMarkButtonModel2 = new CheckMarkButtonModel(style, style2)
				{
					Visibility = ((!ShowRatingSelectionPopupButton() || flag) ? Visibility.Collapsed : Visibility.Visible),
					IsSelected = hasValue2,
					Text = ((!hasValue2) ? Resources.rating_selection_popup_button_text : (value?.Name ?? "None")),
					ShowArrow = !hasValue2,
					TextMaxWidth = (isRedboxPlusBrowseMode ? 220 : ((!hasValue2) ? 104 : 128))
				};
				checkMarkButtonModel2.OnClicked += RatingSelectionPopupButtonModel_OnClicked;
				bool flag2 = BrowseState.TitleType == TitleType.DVD || BrowseState.TitleType == TitleType.Bluray || BrowseState.TitleType == TitleType._4KUHD || BrowseState.TitleType == TitleType.DigitalCode;
				CheckMarkButtonModel checkMarkButtonModel3 = new CheckMarkButtonModel(style, style2)
				{
					Visibility = ((!ShowTopFormatFilterButton() || flag) ? Visibility.Collapsed : Visibility.Visible),
					IsSelected = flag2,
					Text = GetFormatSelectionPopupButtonText(),
					ShowArrow = !flag2,
					TextMaxWidth = (isRedboxPlusBrowseMode ? 220 : ((!flag2) ? 104 : 128))
				};
				checkMarkButtonModel3.OnClicked += FormatSelectionPopupButtonModel_OnClicked;
				CheckMarkButtonModel checkMarkButtonModel4 = new CheckMarkButtonModel(style, style2)
				{
					Visibility = ((!ShowTopDealsFilterButton() || flag) ? Visibility.Collapsed : Visibility.Visible),
					IsSelected = (BrowseState.BrowseMode == BrowseViewFilter.Deals),
					Text = Resources.deals_button,
					TextMaxWidth = 128
				};
				checkMarkButtonModel4.OnClicked += DealsFilterButtonModel_OnClicked;
				CheckMarkButtonModel checkMarkButtonModel5 = new CheckMarkButtonModel(style, style2)
				{
					Visibility = ((!ShowTopForSaleButton() || flag) ? Visibility.Collapsed : Visibility.Visible),
					IsSelected = (BrowseState.BrowseMode == BrowseViewFilter.ForSale),
					Text = Resources.to_buy_button,
					TextMaxWidth = 128
				};
				checkMarkButtonModel5.OnClicked += ToBuyFilterButtonModel_OnClicked;
				BrowseSort browseSort = GetBrowseSort();
				CheckMarkButtonModel checkMarkButtonModel6 = new CheckMarkButtonModel(style, style2)
				{
					Visibility = ((!ShowSortSelectionPopupButton() || flag) ? Visibility.Collapsed : Visibility.Visible),
					IsSelected = false,
					Text = Resources.sort_selection_popup_button_text,
					SmallText = GetSortSelectionPopupButtonText(browseSort),
					TextMaxWidth = (isRedboxPlusBrowseMode ? 220 : 128),
					ShowDownArrow = true
				};
				checkMarkButtonModel6.OnClicked += SortSelectionPopupButtonModel_OnClicked;
				List<CheckMarkButtonModel> checkMarkButtonModels = new List<CheckMarkButtonModel> { checkMarkButtonModel, checkMarkButtonModel2, checkMarkButtonModel3, checkMarkButtonModel4, checkMarkButtonModel5, checkMarkButtonModel6 };
				topMenuModel.CheckMarkButtonModels = checkMarkButtonModels;
				topMenuModel.AZButtonVisibility = ((!ShowTopAZButton()) ? Visibility.Collapsed : Visibility.Visible);
				bool flag3 = BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha;
				topMenuModel.AZButtonIconVisibility = (flag3 ? Visibility.Collapsed : Visibility.Visible);
				topMenuModel.AZButtonExitIconVisibility = ((!flag3) ? Visibility.Collapsed : Visibility.Visible);
				topMenuModel.AZButtonText = (flag3 ? Resources.abc_exit_button : Resources.abc_button);
				topMenuModel.OnAZButtonClicked += TopMenuModel_OnAZButtonClicked;
			}
			else
			{
				bool flag4 = PersonalizationFunctions.IsSignedIn && RedboxPlusFunctions.CanShowRedboxPlusRedemption;
				bool isDeselectable = !BaseViewController.IsADAMode;
				BrowseMenuButton browseMenuButton = null;
				int width = (OrchestrationService.IsCultureSpanish() ? 142 : 121);
				browseMenuButton = CreateGenreMenuButton(Genres.Top20Movies, highlightSelectedGenre, width, isDeselectable);
				topMenuModel.MenuButtons.AddRange(new List<BrowseMenuButton>
				{
					CreateGenreMenuButton(Genres.ActionAndAdventure, highlightSelectedGenre, flag4 ? 101 : 121, isDeselectable),
					CreateGenreMenuButton(Genres.Comedy, highlightSelectedGenre, flag4 ? 101 : 121, isDeselectable),
					CreateGenreMenuButton(Genres.FamilyMovies, highlightSelectedGenre, flag4 ? 101 : 121, isDeselectable),
					CreateGenreMenuButton(Genres.Horror, highlightSelectedGenre, flag4 ? 101 : 121, isDeselectable),
					CreateGenreMenuButton(Genres.Drama, highlightSelectedGenre, flag4 ? 101 : 121, isDeselectable),
					browseMenuButton,
					CreateGenreMenuButton(null, highlightSelectedGenre, OrchestrationService.IsCultureSpanish() ? 121 : 142)
				});
				if (flag4)
				{
					topMenuModel.MenuButtons.Add(CreateSignOutMenuButton());
				}
			}
		}
		else
		{
			topMenuModel.MenuButtons.AddRange(new List<BrowseMenuButton>
			{
				CreateGenreMenuButton(null, highlightSelectedGenre, 184),
				CreateProductFamilyMenuButton(TitleFamily.Movies, ArrowDirection.Right, 792)
			});
		}
	}

	private static BrowseProductFamilyMenuButton CreateProductFamilyMenuButton(TitleFamily productFamily, ArrowDirection arrowDirection)
	{
		return CreateProductFamilyMenuButton(productFamily, arrowDirection, -1);
	}

	private static SignOutMenuButton CreateSignOutMenuButton()
	{
		return new SignOutMenuButton
		{
			Text = Resources.sign_out_button,
			IsSelected = false,
			Width = 101
		};
	}

	private static BrowseProductFamilyMenuButton CreateProductFamilyMenuButton(TitleFamily productFamily, ArrowDirection arrowDirection, int x)
	{
		string text = ((productFamily == TitleFamily.Movies) ? Resources.To_Movies : Resources.To_Games);
		BrowseProductFamilyMenuButton browseProductFamilyMenuButton = new BrowseProductFamilyMenuButton
		{
			Text = text,
			FilterValue = productFamily,
			ArrowDirection = arrowDirection,
			IsButtonEnabled = true
		};
		if (x > -1)
		{
			browseProductFamilyMenuButton.X = x;
			browseProductFamilyMenuButton.FixedPosition = true;
		}
		return browseProductFamilyMenuButton;
	}

	private static BrowseGenreMenuButton CreateGenreMenuButton(Genres? genre, bool highlightSelectedGenre, int width, bool isDeselectable = false)
	{
		Genres? genre2 = BrowseState.Genre;
		BrowseGenreMenuButton browseGenreMenuButton = new BrowseGenreMenuButton
		{
			FilterValue = genre,
			IsDeselectable = isDeselectable
		};
		browseGenreMenuButton.IsSelected = highlightSelectedGenre && genre2 == genre;
		if (browseGenreMenuButton.IsSelected && browseGenreMenuButton.IsDeselectable)
		{
			browseGenreMenuButton.IsButtonEnabled = true;
		}
		string name = "Games_ALL";
		if (genre.HasValue)
		{
			name = $"genre_{(int)genre.Value}";
		}
		else if (BrowseState.TitleFamily == TitleFamily.Movies)
		{
			name = "Movies_ALL";
		}
		browseGenreMenuButton.Text = Resources.ResourceManager.GetString(name);
		browseGenreMenuButton.Width = width;
		return browseGenreMenuButton;
	}

	private static void ConfigureTTS(BrowseViewModel browseViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			browseViewModel.OnGetSpeechControl = () => BrowseViewTextToSpeech.GetSpeechControl(BaseViewController.GetViewModel<BrowseViewModel>);
		}
	}

	private static void ConfigureCheckOutBar(BrowseViewModel browseViewModel)
	{
		if (browseViewModel == null)
		{
			return;
		}
		OrchestrationService.SetCurrentUICulture();
		browseViewModel.OnCheckOutButtonClicked = delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseCheckOut");
			BrowseState.UnwindBrowseStateSetStack();
			BrowseState.SetDefaultBrowseStateSet();
			if (BrowseState.ShowingBrowseMessagePopup)
			{
				CloseBrowseMessagePopupUserControl();
			}
			ShoppingCartViewFunctions.ViewShoppingCart();
		};
		browseViewModel.OnToggleTitleFamilyButton1Clicked = delegate
		{
			BrowseState.TitleFamily = ((BrowseState.TitleFamily != TitleFamily.Movies) ? TitleFamily.Movies : TitleFamily.Games);
			BrowseState.TitleType = TitleType.All;
			BrowseState.BrowseMode = BrowseViewFilter.ForSale;
			BrowseState.BrowseSelectedSort = BrowseSelectedSort.ForSale;
			BrowseState.Genre = null;
			BrowseState.Rating = null;
			BrowseState.Sort = null;
			BrowseState.PageNumber = 1;
			BrowseState.BrowsePriceRangeFilter = null;
			BrowseState.BrowseStudio = null;
			BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", BrowseState.TitleFamily.ToString());
			ConfigureBrowseView(BrowseFunctions.GetBrowseViewparameters());
		};
		browseViewModel.OnSignInButtonClicked = delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseSignIn");
			if (BrowseState.ShowingBrowseMessagePopup)
			{
				CloseBrowseMessagePopupUserControl();
			}
			PersonalizationFunctions.ShowRegularSignInSignUpDialog();
		};
		browseViewModel.OnSignOutButtonClicked = HandleSignOutButtonClicked;
		browseViewModel.OnMyPerksButtonClicked = HandleMyPerksButtonClick;
		browseViewModel.BackToAllMoviesCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), HandleBackToAllMoviesCommand);
		browseViewModel.ViewRedboxPlusMoviesCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), HandleViewRedboxPlusMoviesCommand);
		browseViewModel.ContinueCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), HandleContinueCommand);
		string text = null;
		Style style = null;
		string checkOutButtonCountText = null;
		int valueOrDefault = (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.Count).GetValueOrDefault();
		if (ShowNoThanks())
		{
			style = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "classic_gray_button_style");
			text = Resources.no_thanks_button;
			checkOutButtonCountText = string.Empty;
		}
		else
		{
			style = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "warm_purple_no_border_button_style");
			text = Resources.check_out_button_text;
			string check_out_button_count = Resources.check_out_button_count;
			if (check_out_button_count.Contains("{0}"))
			{
				checkOutButtonCountText = string.Format(check_out_button_count, valueOrDefault);
			}
		}
		browseViewModel.IsADAAccessible = BaseViewController.IsADAMode;
		browseViewModel.CheckOutButtonText = text;
		browseViewModel.CheckOutButtonCountText = checkOutButtonCountText;
		browseViewModel.CheckOutButtonStyle = style;
		browseViewModel.CheckOutButtonVisibility = (BrowseFunctions.IsRedboxPlusBrowseMode ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.SwitchProductFamilyButton1Visibility = Visibility.Collapsed;
		browseViewModel.SwitchProductFamilyButton1Text = ((BrowseState.TitleFamily == TitleFamily.Movies) ? Resources.games_button_text : Resources.movies_button_text);
		browseViewModel.AddToADACartLabelText = Resources.add_to_cart_ada_label;
		browseViewModel.AddToADACartLabelVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.DontMissYourPerksText = Resources.earn_free_rentals;
		browseViewModel.SignInButtonText = Resources.button_sign_in;
		browseViewModel.SignUpButtonText = Resources.button_sign_up;
		browseViewModel.SignInUpButtonSeparator = Resources.button_sign_in_sign_up_separator;
		browseViewModel.SignInButtonVisibility = ((!PersonalizationFunctions.IsSignInEnabled) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.SignOutButtonText = Resources.sign_out_button;
		browseViewModel.SignOutButtonVisibility = (((!PersonalizationFunctions.IsSignedIn || RedboxPlusFunctions.CanShowRedboxPlusRedemption) && !ShowBottomRedboxPlusMoviesButton()) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.ADARedboxPlusSignOutGridVisibility = ((!PersonalizationFunctions.IsSignedIn || !RedboxPlusFunctions.CanShowRedboxPlusRedemption || !BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.MyPerksButtonText = Resources.view_my_perks;
		browseViewModel.MyPerksButtonVisibility = ((!PersonalizationFunctions.IsLoyaltySignedIn || RedboxPlusFunctions.CanShowRedboxPlusRedemption) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.MyPerksRedboxPlusButtonVisibility = ((!PersonalizationFunctions.IsLoyaltySignedIn || !RedboxPlusFunctions.CanShowRedboxPlusRedemption) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.ContinueButtonText = Resources.continue_button;
		browseViewModel.ContinueButtonVisibility = ((!BrowseFunctions.IsRedboxPlusBrowseMode) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.BackToAllMoviesButtonText = Resources.back_to_all_movies;
		browseViewModel.BackToAllMoviesButtonVisibility = ((!BrowseFunctions.IsRedboxPlusBrowseMode || ShowBottomRedboxPlusMoviesButton()) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.ViewRedboxPlusMoviesButtonText = Resources.view_redbox_plus_movies;
		browseViewModel.ViewRedboxPlusMoviesButtonVisibility = ((BrowseState.BrowseMode == BrowseViewFilter.RedboxPlus || !RedboxPlusFunctions.CanShowRedboxPlusRedemption || ShowBottomRedboxPlusMoviesButton()) ? Visibility.Collapsed : Visibility.Visible);
	}

	private static void HandleSignOutButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseSignOut");
		PersonalizationFunctions.ShowLogoutDialog();
	}

	private static void HandleViewRedboxPlusMoviesCommand()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ViewRedboxPlusMovies");
		HandleFilterButtonClick(new BrowseFilter
		{
			ButtonType = BrowseFilterType.RedboxPlus
		});
		BrowseFunctions.ShowRedboxPlusBrowseWarnings();
	}

	private static void BottomRedboxPlusMoviesButtonModel_OnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		BaseViewController.ProcessWPFHit();
		if (checkMarkButtonModel != null && checkMarkButtonModel.IsSelected)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("ViewRedboxPlusMovies", "Deselect");
			BrowseState.TitleType = TitleType.All;
			BrowseState.BrowseMode = BrowseViewFilter.All;
			BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
			BrowseState.PageNumber = 1;
			ConfigureTopMenu();
			ConfigureBrowseControl();
			ConfigureBrowseFilterBar();
		}
		else
		{
			HandleViewRedboxPlusMoviesCommand();
		}
	}

	private static void HandleBackToAllMoviesCommand()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseBackToAllMovies");
		HandleFilterButtonClick(new BrowseFilter
		{
			ButtonType = BrowseFilterType.BackToAll
		});
	}

	private static void HandleContinueCommand()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("Continue");
		HandleFilterButtonClick(new BrowseFilter
		{
			ButtonType = BrowseFilterType.BackToAll
		});
	}

	private static void HandleMyPerksButtonClick()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseMyPerks");
		if (BrowseState.ShowingBrowseMessagePopup)
		{
			CloseBrowseMessagePopupUserControl();
		}
		PersonalizationFunctions.ShowMemberPerks();
	}

	private static bool ShowNoThanks()
	{
		bool result = false;
		if (BaseViewController.RentalShoppingCartService != null)
		{
			IRentalShoppingCart currentRentalShoppingCart = BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart;
			if (currentRentalShoppingCart != null)
			{
				result = currentRentalShoppingCart.InCartPromoOfferMade && !PromotionService.Instance.IsFreeInCartOfferRunning && !PromotionService.Instance.IsRentTwoInCartOfferRunning && !currentRentalShoppingCart.InCartPromoOfferCancel && !currentRentalShoppingCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => x.Sequence == RentalShoppingCartItemSequence.post_offer);
			}
		}
		return result;
	}

	private static void ConfigureBrowseFilterBar()
	{
		BrowseViewModel viewModel = BaseViewController.GetViewModel<BrowseViewModel>();
		ConfigureBrowseFilterBar(BaseViewController.GetViewUserControl(), viewModel);
	}

	private static void ConfigureBrowseFilterBar(FrameworkElement browseViewUserControl, BrowseViewModel browseViewModel)
	{
		if (browseViewModel == null)
		{
			return;
		}
		OrchestrationService.SetCurrentUICulture();
		FilterBarModel filterBarModel = new FilterBarModel();
		filterBarModel.OnStartOverButtonClicked += FilterBarModel_OnQuitButtonsClicked;
		filterBarModel.OnBackButtonClicked += delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseBack");
			BrowseState?.PopBrowseStateSet();
			if (BrowseState.IsNonBrowseView)
			{
				BrowseState?.PopBrowseStateSet();
				IViewService service = ServiceLocator.Instance.GetService<IViewService>();
				service?.GoBackSafe("back_buton_press", popOnly: false);
				service?.Show();
			}
			else
			{
				UpdateDisplayProductModels(browseViewModel.BrowseProductControlModel.BrowseItems, isADAMiniCart: false);
				ConfigureBrowseControl();
				ConfigureTopMenu(browseViewModel);
				ConfigureBrowseFilterBar();
				ConfigureMiniCartControl(browseViewModel);
				ConfigureCheckOutBar(browseViewModel);
				CommandManager.InvalidateRequerySuggested();
			}
		};
		filterBarModel.OnHomeButtonClicked += FilterBarModel_OnQuitButtonsClicked;
		filterBarModel.OnFormatFilterButtonClicked += delegate
		{
			if (!BaseViewController.IsADAMode && (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Dvd || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Bluray || BrowseState.BrowseSelectedSort == BrowseSelectedSort._4KUHD || BrowseState.BrowseSelectedSort == BrowseSelectedSort.DigitalCode))
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseFormat", "Deselect");
				BrowseState.TitleType = TitleType.All;
				BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
				BrowseState.BrowseMode = ((BrowseState.BrowseMode == BrowseViewFilter.RedboxPlus) ? BrowseViewFilter.RedboxPlus : BrowseViewFilter.All);
				BrowseState.PageNumber = 1;
				BrowseState.BrowsePriceRangeFilter = null;
				BrowseState.BrowseStudio = null;
				if (browseViewModel != null)
				{
					browseViewModel.BrowseFilterBarModel.SelectedPriceRange = null;
				}
				ConfigureBrowseControl();
				ConfigureTopMenu(browseViewModel);
				ConfigureBrowseFilterBar();
				ConfigureMiniCartControl(browseViewModel);
				ConfigureCheckOutBar(browseViewModel);
				CommandManager.InvalidateRequerySuggested();
			}
			else
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseFormat");
				OrchestrationService.ViewShow("format_filter_popup_view");
			}
		};
		filterBarModel.OnPriceRangeFilterButtonClicked += delegate
		{
			ShowPriceFilterDialog();
		};
		filterBarModel.HomeButtonText = (ShowHomeButton() ? Resources.home_button.ToUpper() : Resources.quit_button.ToUpper());
		filterBarModel.HomeButtonVisibility = ((!PersonalizationFunctions.IsSignedIn || InTitleMarketingMode) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.StartOverButtonVisibility = ((PersonalizationFunctions.IsSignedIn || InTitleMarketingMode) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.StartOverButtonText = (ShowHomeButton() ? Resources.home_button.ToUpper() : Resources.quit_button.ToUpper());
		filterBarModel.BackButtonVisibility = Visibility.Collapsed;
		filterBarModel.BackButtonText = Resources.back_button.ToUpper();
		CheckMarkButtonModel checkMarkButtonModel = new CheckMarkButtonModel(WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "button_selectable_small_unselected_style"), WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "button_selectable_small_selected_style"))
		{
			Visibility = ((!ShowBottomRedboxPlusMoviesButton()) ? Visibility.Collapsed : Visibility.Visible),
			IsSelected = BrowseFunctions.IsRedboxPlusBrowseMode,
			Text = Resources.redbox_plus_movies_button,
			CheckMarkImageMargin = new Thickness(-35.0, -20.0, 0.0, 0.0)
		};
		checkMarkButtonModel.OnClicked += BottomRedboxPlusMoviesButtonModel_OnClicked;
		filterBarModel.BottomRedboxPlusMoviesButtonModel = checkMarkButtonModel;
		filterBarModel.FilterBarTextVisibility = Visibility.Collapsed;
		filterBarModel.FormatFilterButtonVisibility = ((!ShowBottomFormatFilterButton()) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.FormatFilterButtonStrings = new Dictionary<TitleType, string>();
		filterBarModel.FormatFilterButtonStrings.Add(TitleType.All, Resources.format_button);
		filterBarModel.FormatFilterButtonStrings.Add(TitleType.DVD, Resources.dvd);
		filterBarModel.FormatFilterButtonStrings.Add(TitleType.Bluray, Resources.blu_ray.ToUpper());
		filterBarModel.FormatFilterButtonStrings.Add(TitleType.DigitalCode, Resources.digital_code.ToUpper());
		filterBarModel.FormatFilterButtonStrings.Add(TitleType._4KUHD, Resources.four_k_uhd);
		filterBarModel.FilterButtonSelectedStyle = WPFFunctions.GetStyle(browseViewUserControl, "purple_with_purple_border_button_style_disabled");
		filterBarModel.FilterButtonUnselectedStyle = WPFFunctions.GetStyle(browseViewUserControl, "purple_with_purple_border_button_style");
		filterBarModel.SelectedFormat = BrowseState.TitleType;
		filterBarModel.PriceRangeButtonDefaultText = Resources.price_range_select_button;
		filterBarModel.PriceRangeFilterButtonVisibility = ((!ShowPriceRangeFilterButton()) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.SelectedPriceRange = BrowseState.BrowsePriceRangeFilter;
		IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
		List<BrowseFilter> filters = null;
		if (!InTitleMarketingMode)
		{
			filters = CreateBrowseFilters(instance);
		}
		filterBarModel.Filters = filters;
		filterBarModel.FilterMargin = new Thickness((!BaseViewController.IsADAMode && BrowseState.TitleFamily != TitleFamily.Games) ? 16 : (OrchestrationService.IsCultureSpanish() ? 6 : 8), 0.0, 0.0, 0.0);
		filterBarModel.OnBrowseFilterButtonClicked += delegate(BrowseFilter browseFilter)
		{
			foreach (BrowseFilter filter in filterBarModel.Filters)
			{
				filter.IsSelected = browseFilter == filter;
			}
			HandleFilterButtonClick(browseFilter);
			if (BrowseState.TitleType == TitleType.All)
			{
				filterBarModel.SelectedFormat = TitleType.All;
			}
		};
		browseViewModel.BrowseFilterBarModel = filterBarModel;
	}

	private static void FilterBarModel_OnQuitButtonsClicked()
	{
		IViewService viewService = BaseViewController.GetViewService();
		if (viewService != null && ShowHomeButton())
		{
			BaseViewController.ProcessWPFHit();
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseHome");
			if (BrowseState.ShowingBrowseMessagePopup)
			{
				CloseBrowseMessagePopupUserControl();
			}
			BrowseState.SetDefaultBrowseStateSet();
			viewService.Show();
			return;
		}
		BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseQuit");
		if (BrowseState.ShowingBrowseMessagePopup)
		{
			CloseBrowseMessagePopupUserControl();
		}
		Action startOverAction = delegate
		{
			ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(null);
		};
		Action signOutAction = delegate
		{
			Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LogOut();
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult = null;
			}
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart.ClearAllLoyaltyPointsRecords();
			LogHelper.Instance.Log("User has signed out.  Closing session and returning to welcome screen.");
			ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(null);
		};
		bool isSignedIn = PersonalizationFunctions.IsSignedIn;
		IRentalShoppingCartService rentalShoppingCartService = BaseViewController.RentalShoppingCartService;
		bool flag = rentalShoppingCartService != null && rentalShoppingCartService.CurrentRentalShoppingCart?.Count > 0;
		if (isSignedIn)
		{
			if (flag)
			{
				MessagePopupViewController.ShowSignedInItemInCartQuitMessagePopupView(delegate
				{
					signOutAction();
				}, delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseQuitMessagePopupKeepBrowsing");
					OrchestrationService.ViewGoBackSafe("QuitMessagePopup KeepBrowsing", popOnly: false);
				}, delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseQuitMessagePopupQuit");
					signOutAction();
				});
			}
			else
			{
				MessagePopupViewController.ShowSignedInEmptyCartQuitMessagePopupView(delegate
				{
					signOutAction();
				}, delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseQuitMessagePopupKeepBrowsing");
					OrchestrationService.ViewGoBackSafe("QuitMessagePopup KeepBrowsing", popOnly: false);
				}, delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseQuitMessagePopupQuit");
					signOutAction();
				});
			}
		}
		else if (flag)
		{
			MessagePopupViewController.ShowItemInCartQuitMessagePopupView(delegate
			{
				startOverAction();
			}, delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseQuitMessagePopupKeepBrowsing");
				OrchestrationService.ViewGoBackSafe("QuitMessagePopup KeepBrowsing", popOnly: false);
			}, delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseQuitMessagePopupQuit");
				startOverAction();
			});
		}
		else
		{
			startOverAction();
		}
	}

	private static bool ShowHomeButton()
	{
		OrchestrationService.SetCurrentUICulture();
		if (BrowseState.PageNumber <= 1 && BrowseState.BrowseMode == BrowseViewFilter.All && BrowseState.BrowseSelectedSort == BrowseSelectedSort.Newest && !BrowseState.Genre.HasValue && !BrowseState.Rating.HasValue)
		{
			return BrowseState.Sort.HasValue;
		}
		return true;
	}

	private static void UpdateQuitButtonsText()
	{
		BrowseViewModel viewModel = BaseViewController.GetViewModel<BrowseViewModel>();
		if (viewModel?.BrowseFilterBarModel != null)
		{
			viewModel.BrowseFilterBarModel.HomeButtonText = (ShowHomeButton() ? Resources.home_button.ToUpper() : Resources.quit_button.ToUpper());
			viewModel.BrowseFilterBarModel.StartOverButtonText = (ShowHomeButton() ? Resources.home_button.ToUpper() : Resources.quit_button.ToUpper());
		}
	}

	private static void ShowPriceFilterDialog()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowsePriceFilter", "price_filter");
		OptionSelectionDialogInfo optionSelectionDialogInfo = new OptionSelectionDialogInfo
		{
			HeaderText = Resources.price_filter_title,
			CancelButtonText = Resources.cancel_button.ToUpper(),
			ViewAnalyticsName = "filter_by_price_view",
			ViewAnalyticsContext = "FilterByPriceDialog"
		};
		optionSelectionDialogInfo.CancelCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseCancel");
			OrchestrationService.ViewGoBackSafe("FilterByPriceDialog_cancel", popOnly: false);
		}, (Func<bool>)null);
		DynamicRoutedCommand command = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate(BrowseFilter filter)
		{
			BrowsePriceRangeParameter browsePriceRangeParameter = (BrowsePriceRangeParameter)filter.Parameter;
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseFilter", browsePriceRangeParameter.analtyicsString);
			HandleFilterButtonClick(filter);
			OrchestrationService.ViewGoBackSafe("FilterByPriceDialog_filter_selected", popOnly: false);
		});
		string buttonText = string.Format(Resources.price_range_low_button_short, 3.99m.ToString("0.00"));
		BrowseFilter browseFilter = new BrowseFilter
		{
			ButtonType = BrowseFilterType.PriceRange,
			Name = "price_range_low_button",
			Parameter = new BrowsePriceRangeParameter
			{
				PriceRangeHigh = 3.99m,
				analtyicsString = "buy399range",
				buttonText = buttonText
			}
		};
		browseFilter.Text = string.Format(Resources.price_range_low_button, 3.99m.ToString("0.00"));
		optionSelectionDialogInfo.ButtonData.Add(new ButtonData(browseFilter.Text, command, browseFilter));
		buttonText = string.Format(Resources.price_range_mid_button_short, 4.99m.ToString("0.00"));
		browseFilter = new BrowseFilter
		{
			ButtonType = BrowseFilterType.PriceRange,
			Name = "price_range_mid_button",
			Parameter = new BrowsePriceRangeParameter
			{
				PriceRangeLow = 4.99m,
				PriceRangeHigh = 5.99m,
				analtyicsString = "buymidrange",
				buttonText = buttonText
			}
		};
		browseFilter.Text = string.Format(Resources.price_range_mid_button, 4.99m.ToString("0.00"), 5.99m.ToString("0.00"));
		optionSelectionDialogInfo.ButtonData.Add(new ButtonData(browseFilter.Text, command, browseFilter, isEnabled: true, "$" + 4.99m.ToString("0.00") + " to $" + 5.99m.ToString("0.00")));
		buttonText = string.Format(Resources.price_range_high_button_short, 6.99m.ToString("0.00"));
		browseFilter = new BrowseFilter
		{
			ButtonType = BrowseFilterType.PriceRange,
			Name = "price_range_high_button",
			Parameter = new BrowsePriceRangeParameter
			{
				PriceRangeLow = 6.99m,
				analtyicsString = "buy699range",
				buttonText = buttonText
			}
		};
		browseFilter.Text = string.Format(Resources.price_range_high_button, 6.99m.ToString("0.00"));
		optionSelectionDialogInfo.ButtonData.Add(new ButtonData(browseFilter.Text, command, browseFilter));
		OrchestrationService.ViewShow("option_selection_dialog_view", optionSelectionDialogInfo);
	}

	private static bool ShowBottomRedboxPlusMoviesButton()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons && !BaseViewController.IsADAMode && PersonalizationFunctions.IsSignedIn)
		{
			return RedboxPlusFunctions.CanShowRedboxPlusRedemption;
		}
		return false;
	}

	private static bool ShowGenreSelectionPopupButton()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons)
		{
			return !BaseViewController.IsADAMode;
		}
		return false;
	}

	private static bool ShowRatingSelectionPopupButton()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons)
		{
			return !BaseViewController.IsADAMode;
		}
		return false;
	}

	private static bool ShowSortSelectionPopupButton()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons)
		{
			return !BaseViewController.IsADAMode;
		}
		return false;
	}

	private static bool CanShowFormatFilterButton()
	{
		if (BrowseState.TitleFamily == TitleFamily.Movies)
		{
			return !InTitleMarketingMode;
		}
		return false;
	}

	private static bool ShowBottomFormatFilterButton()
	{
		if (CanShowFormatFilterButton())
		{
			return !ShowTopFormatFilterButton();
		}
		return false;
	}

	private static bool ShowTopFormatFilterButton()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons && !BaseViewController.IsADAMode)
		{
			return CanShowFormatFilterButton();
		}
		return false;
	}

	private static bool ShowPriceRangeFilterButton()
	{
		if (BrowseState.TitleFamily == TitleFamily.Movies && BrowseState.BrowseSelectedSort == BrowseSelectedSort.ForSale && Redbox.Rental.Services.Configuration.Configuration.Instance.PriceFiltersOnBrowseABTestVersion == "A")
		{
			return BaseViewController.IsADAMode;
		}
		return false;
	}

	private static bool ShowPriceRangeFilterButtons()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons && !BaseViewController.IsADAMode)
		{
			return false;
		}
		return BrowseState.BrowseSelectedSort == BrowseSelectedSort.ForSale;
	}

	private static bool CanShowForSaleButton()
	{
		IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
		if (service != null && service.CanSellUsedMovies && BrowseState.TitleFamily == TitleFamily.Movies)
		{
			return !BrowseFunctions.IsRedboxPlusBrowseMode;
		}
		return false;
	}

	private static bool ShowBottomForSaleButton()
	{
		if (CanShowForSaleButton() && !ShowTopForSaleButton() && BrowseState.BrowseSelectedSort != BrowseSelectedSort.ForSale)
		{
			return !ShowPriceRangeFilterButton();
		}
		return false;
	}

	private static bool ShowTopForSaleButton()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons && !BaseViewController.IsADAMode)
		{
			return CanShowForSaleButton();
		}
		return false;
	}

	private static bool CanShowLastFilterButton()
	{
		if (!Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons && !BaseViewController.IsADAMode)
		{
			return !BrowseFunctions.IsRedboxPlusBrowseMode;
		}
		return false;
	}

	private static bool ShowTopDealsFilterButton()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons && Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowDealsBrowseFilterButton && !BaseViewController.IsADAMode)
		{
			return !BrowseFunctions.IsRedboxPlusBrowseMode;
		}
		return false;
	}

	private static bool ShouldShowDeals()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowDealsBrowseFilterButton)
		{
			return CanShowLastFilterButton();
		}
		return false;
	}

	private static bool ShouldShowSonySweepstakes()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.EnableSonySweepstakes && CanShowLastFilterButton())
		{
			return !ShouldShowDeals();
		}
		return false;
	}

	private static bool ShouldShow99Cents()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.Show99CentsBrowseFilterButton && CanShowLastFilterButton() && !ShouldShowDeals())
		{
			return !ShouldShowSonySweepstakes();
		}
		return false;
	}

	private static bool ShouldShowComingSoon()
	{
		if (CanShowLastFilterButton() && !ShouldShowDeals() && !ShouldShowSonySweepstakes())
		{
			return !ShouldShow99Cents();
		}
		return false;
	}

	private static bool ShowBottomAZButton()
	{
		if (!ShowTopAZButton())
		{
			return BrowseState.TitleFamily == TitleFamily.Movies;
		}
		return false;
	}

	private static bool ShowTopAZButton()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons && !BaseViewController.IsADAMode)
		{
			return BrowseState.TitleFamily == TitleFamily.Movies;
		}
		return false;
	}

	private static bool ShowBottomPageNumberText()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTopFilterButtons && BrowseState.BrowseSelectedSort != BrowseSelectedSort.Alpha)
		{
			return !BaseViewController.IsADAMode;
		}
		return false;
	}

	internal static void HandleFilterButtonClick(BrowseFilter browseFilter)
	{
		BrowseFilterType buttonType = browseFilter.ButtonType;
		bool isDeselectable = browseFilter.IsDeselectable;
		if (BaseViewController.IsADAMode)
		{
			BrowseState.Genre = null;
		}
		switch (buttonType)
		{
		case BrowseFilterType.BackToAll:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Newest, TitleType.All, "browse-filter-family,back-to-all", 1);
			break;
		case BrowseFilterType.AllNewest:
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(TitleType.All), BrowseSelectedSort.Newest, TitleType.All, "browse-filter-family,Newest", 1);
			break;
		case BrowseFilterType.BluRay:
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(TitleType.Bluray), BrowseSelectedSort.Bluray, TitleType.Bluray, "browse-filter-family,Blu-ray", 1);
			break;
		case BrowseFilterType.Dvd:
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(TitleType.DVD), BrowseSelectedSort.Dvd, TitleType.DVD, "browse-filter-family,DVD", 1);
			break;
		case BrowseFilterType._4KUHD:
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(TitleType._4KUHD), BrowseSelectedSort._4KUHD, TitleType._4KUHD, "browse-filter-family,4K-UHD", 1);
			break;
		case BrowseFilterType.ForSale:
			SetBrowseFilters(BrowseViewFilter.ForSale, BrowseSelectedSort.ForSale, TitleType.All, "browse-filter-family,for-sale", 1);
			if (ShowPriceRangeFilterButton())
			{
				browseFilter.IsVisible = false;
			}
			break;
		case BrowseFilterType.ComingSoon:
			SetBrowseFilters(BrowseViewFilter.ComingSoon, BrowseSelectedSort.ComingSoon, TitleType.All, "browse-filter-family,Coming-soon", 1, null, null, (BrowseState.BrowseSelectedSort == BrowseSelectedSort.ComingSoon) & isDeselectable);
			break;
		case BrowseFilterType.Deals:
			SetBrowseFilters(BrowseViewFilter.Deals, BrowseSelectedSort.Deals, TitleType.All, "browse-filter-family,Deals", 1, null, null, (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Deals) & isDeselectable);
			break;
		case BrowseFilterType.AtoZ:
			BrowseState.SelectedAlphaGroupName = null;
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(TitleType.All), BrowseSelectedSort.Alpha, TitleType.All, "browse-filter-family,alpha", 1, null, null, (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha) & isDeselectable);
			break;
		case BrowseFilterType.DigitalCode:
			SetBrowseFilters(BrowseViewFilter.ForSale, BrowseSelectedSort.DigitalCode, TitleType.DigitalCode, "browse-filter-family,Digital Code", 1);
			break;
		case BrowseFilterType.XboxOne:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.XboxOne, TitleType.XboxOne, "browse-filter-family,Xbox One", 1);
			break;
		case BrowseFilterType.Xbox360:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Xbox360, TitleType.Xbox360, "browse-filter-family,Xbox 360", 1);
			break;
		case BrowseFilterType.PS3:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Ps3, TitleType.PS3, "browse-filter-family,PS3", 1);
			break;
		case BrowseFilterType.PS4:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Ps4, TitleType.PS4, "browse-filter-family,PS4", 1);
			break;
		case BrowseFilterType.Wii:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Wii, TitleType.Wii, "browse-filter-family,Wii", 1);
			break;
		case BrowseFilterType.WiiU:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.WiiU, TitleType.WiiU, "browse-filter-family,Wii U", 1);
			break;
		case BrowseFilterType.NintendoSwitch:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.NintendoSwitch, TitleType.NintendoSwitch, "browse-filter-family,Nintendo Switch", 1);
			break;
		case BrowseFilterType.Top20:
			BrowseState.Genre = Genres.Top20Movies;
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(), BrowseSelectedSort.Top20, BrowseState.TitleType, "browse-filter-family,Top 20", 1);
			break;
		case BrowseFilterType.Action:
			BrowseState.Genre = Genres.ActionAndAdventure;
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(), BrowseSelectedSort.Action, BrowseState.TitleType, "browse-filter-family,Action", 1);
			break;
		case BrowseFilterType.Drama:
			BrowseState.Genre = Genres.Drama;
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(), BrowseSelectedSort.Drama, BrowseState.TitleType, "browse-filter-family,Drama", 1);
			break;
		case BrowseFilterType.Horror:
			BrowseState.Genre = Genres.Horror;
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(), BrowseSelectedSort.Horror, BrowseState.TitleType, "browse-filter-family,Horror", 1);
			break;
		case BrowseFilterType.Comedy:
			BrowseState.Genre = Genres.Comedy;
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(), BrowseSelectedSort.Comedy, BrowseState.TitleType, "browse-filter-family,Comedy", 1);
			break;
		case BrowseFilterType.Family:
			BrowseState.Genre = Genres.FamilyMovies;
			SetBrowseFilters(BrowseFunctions.GetBrowseMode(), BrowseSelectedSort.Family, BrowseState.TitleType, "browse-filter-family,Family", 1);
			break;
		case BrowseFilterType.PriceRange:
		{
			if (!(browseFilter.Parameter is BrowsePriceRangeParameter { PriceRangeHigh: var priceRangeHigh } browsePriceRangeParameter))
			{
				break;
			}
			decimal num = 3.99m;
			int num2;
			decimal? priceRangeHigh2;
			if ((priceRangeHigh.GetValueOrDefault() == num) & priceRangeHigh.HasValue)
			{
				BrowsePriceRangeParameter browsePriceRangeFilter = BrowseState.BrowsePriceRangeFilter;
				if (browsePriceRangeFilter == null)
				{
					num2 = 0;
				}
				else
				{
					priceRangeHigh2 = browsePriceRangeFilter.PriceRangeHigh;
					num = 3.99m;
					num2 = (((priceRangeHigh2.GetValueOrDefault() == num) & priceRangeHigh2.HasValue) ? 1 : 0);
				}
			}
			else
			{
				num2 = 0;
			}
			priceRangeHigh2 = browsePriceRangeParameter.PriceRangeHigh;
			num = 5.99m;
			int num3;
			if ((priceRangeHigh2.GetValueOrDefault() == num) & priceRangeHigh2.HasValue)
			{
				BrowsePriceRangeParameter browsePriceRangeFilter2 = BrowseState.BrowsePriceRangeFilter;
				if (browsePriceRangeFilter2 == null)
				{
					num3 = 0;
				}
				else
				{
					priceRangeHigh2 = browsePriceRangeFilter2.PriceRangeHigh;
					num = 5.99m;
					num3 = (((priceRangeHigh2.GetValueOrDefault() == num) & priceRangeHigh2.HasValue) ? 1 : 0);
				}
			}
			else
			{
				num3 = 0;
			}
			bool flag = (byte)num3 != 0;
			priceRangeHigh2 = browsePriceRangeParameter.PriceRangeLow;
			num = 6.99m;
			int num4;
			if ((priceRangeHigh2.GetValueOrDefault() == num) & priceRangeHigh2.HasValue)
			{
				BrowsePriceRangeParameter browsePriceRangeFilter3 = BrowseState.BrowsePriceRangeFilter;
				if (browsePriceRangeFilter3 == null)
				{
					num4 = 0;
				}
				else
				{
					priceRangeHigh2 = browsePriceRangeFilter3.PriceRangeLow;
					num = 6.99m;
					num4 = (((priceRangeHigh2.GetValueOrDefault() == num) & priceRangeHigh2.HasValue) ? 1 : 0);
				}
			}
			else
			{
				num4 = 0;
			}
			bool flag2 = (byte)num4 != 0;
			bool flag3 = (byte)((uint)num2 | (flag ? 1u : 0u) | (flag2 ? 1u : 0u)) != 0;
			SetBrowseFilters(BrowseViewFilter.ForSale, BrowseSelectedSort.ForSale, TitleType.All, "browse-filter-family," + browsePriceRangeParameter.analtyicsString, 1, browsePriceRangeParameter, null, (BrowseState.BrowseSelectedSort == BrowseSelectedSort.ForSale) & isDeselectable & flag3, flag3);
			break;
		}
		case BrowseFilterType._99Cents:
			if (browseFilter.Parameter is BrowsePriceRangeParameter priceRange)
			{
				SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort._99Cents, TitleType.All, "browse-filter-family,$.99", 1, priceRange, null, (BrowseState.BrowseSelectedSort == BrowseSelectedSort._99Cents) & isDeselectable);
			}
			break;
		case BrowseFilterType.SonySweepstakes:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.SonySweepstakes, TitleType.All, "browse-filter-family,Sony Sweepstakes", 1, null, "Sony", (BrowseState.BrowseSelectedSort == BrowseSelectedSort.SonySweepstakes) & isDeselectable);
			break;
		case BrowseFilterType.RedboxPlus:
			SetBrowseFilters(BrowseViewFilter.RedboxPlus, BrowseSelectedSort.Newest, TitleType.All, "browse-filter-family,Redbox+", 1);
			break;
		}
		BrowseViewModel viewModel = BaseViewController.GetViewModel<BrowseViewModel>();
		if (viewModel?.BrowseFilterBarModel == null)
		{
			return;
		}
		if (viewModel?.BrowseFilterBarModel?.Filters != null)
		{
			foreach (BrowseFilter filter in viewModel.BrowseFilterBarModel.Filters)
			{
				if (filter.ButtonType == BrowseFilterType.ForSale)
				{
					if (Redbox.Rental.Services.Configuration.Configuration.Instance.PriceFiltersOnBrowseABTestVersion != "B")
					{
						filter.IsVisible = ShowBottomForSaleButton();
					}
				}
				else if (filter.ButtonType == BrowseFilterType.PriceRange)
				{
					filter.IsVisible = ShowPriceRangeFilterButtons();
				}
				else if (filter.ButtonType == BrowseFilterType.Dvd || filter.ButtonType == BrowseFilterType.BluRay || filter.ButtonType == BrowseFilterType._4KUHD)
				{
					filter.IsVisible = false;
					viewModel.BrowseFilterBarModel.SelectedFormat = BrowseState.TitleType;
				}
			}
		}
		viewModel.BrowseFilterBarModel.FormatFilterButtonVisibility = ((!ShowBottomFormatFilterButton()) ? Visibility.Collapsed : Visibility.Visible);
		viewModel.BrowseFilterBarModel.PriceRangeFilterButtonVisibility = ((!ShowPriceRangeFilterButton()) ? Visibility.Collapsed : Visibility.Visible);
	}

	internal static void SetBrowseFilters(BrowseViewFilter browseMode, BrowseSelectedSort browseSelectedSort, TitleType titleType, string analyticAttributeValue, int? pageNumber, BrowsePriceRangeParameter priceRange = null, string studio = null, bool deselect = false, bool deselectPriceRange = false)
	{
		BrowseViewModel viewModel = BaseViewController.GetViewModel<BrowseViewModel>();
		if (deselect)
		{
			if (deselectPriceRange)
			{
				BrowseState.BrowsePriceRangeFilter = null;
				BrowseState.PageNumber = 1;
				if (!string.IsNullOrEmpty(analyticAttributeValue))
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", string.Format(analyticAttributeValue + ",deselect"));
				}
				if (viewModel != null)
				{
					viewModel.BrowseFilterBarModel.SelectedPriceRange = null;
				}
			}
			else
			{
				BrowseState.BrowseMode = ((browseMode == BrowseViewFilter.RedboxPlus) ? BrowseViewFilter.RedboxPlus : BrowseViewFilter.All);
				BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
				BrowseState.TitleType = TitleType.All;
				BrowseState.BrowsePriceRangeFilter = null;
				BrowseState.PageNumber = 1;
				BrowseState.BrowseStudio = null;
				if (!string.IsNullOrEmpty(analyticAttributeValue))
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", string.Format(analyticAttributeValue + ",deselect"));
				}
				if (viewModel != null)
				{
					viewModel.BrowseFilterBarModel.SelectedPriceRange = null;
				}
			}
		}
		else
		{
			BrowseState.BrowseMode = browseMode;
			BrowseState.BrowseSelectedSort = browseSelectedSort;
			BrowseState.TitleType = titleType;
			BrowseState.BrowsePriceRangeFilter = priceRange;
			if (pageNumber.HasValue)
			{
				BrowseState.PageNumber = pageNumber.Value;
			}
			BrowseState.BrowseStudio = studio;
			if (!string.IsNullOrEmpty(analyticAttributeValue))
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("browse-filter-change", analyticAttributeValue);
			}
			if (viewModel != null)
			{
				viewModel.BrowseFilterBarModel.SelectedPriceRange = priceRange;
			}
		}
		ConfigureBrowseControl(viewModel);
		ConfigureMiniCartControl(viewModel);
		ConfigureBrowseFilterBar();
		ConfigureCheckOutBar(viewModel);
		ConfigureTopMenu(viewModel);
	}

	private static void ConfigureBrowseControl()
	{
		ConfigureBrowseControl(BaseViewController.GetViewModel<BrowseViewModel>());
	}

	private static void ConfigureBrowseControl(BrowseViewModel browseViewModel)
	{
		if (browseViewModel == null)
		{
			return;
		}
		OrchestrationService.SetCurrentUICulture();
		browseViewModel.BackgroundImageVisibility = Visibility.Collapsed;
		BrowseControlModel browseProductControl = new BrowseControlModel();
		browseProductControl.SetPageNumber(Math.Max(BrowseState.PageNumber, 1), PageChangeSource.None);
		string pageNumberText = Resources.browse_view_control_page_mask;
		if (BrowseFunctions.IsRedboxPlusBrowseMode)
		{
			IBrowseState browseState = BrowseState;
			if (browseState == null || browseState.BrowseSelectedSort != BrowseSelectedSort.Alpha)
			{
				pageNumberText = ((ShowBottomRedboxPlusMoviesButton() && OrchestrationService.IsCultureSpanish()) ? Resources.browse_view_control_page_mask : Resources.redbox_plus_browse_view_control_page_mask);
				goto IL_00bb;
			}
		}
		if (BrowseState.BrowseMode == BrowseViewFilter.Deals)
		{
			pageNumberText = ((ShowBottomRedboxPlusMoviesButton() && OrchestrationService.IsCultureSpanish()) ? Resources.browse_view_control_page_mask : Resources.deals_browse_view_control_page_mask);
		}
		goto IL_00bb;
		IL_00bb:
		browseProductControl.PageNumberText = pageNumberText;
		browseProductControl.NoResultsText = Resources.filter_games_empty;
		IBrowseState browseState2 = BrowseState;
		if (browseState2 != null && browseState2.TitleFamily == TitleFamily.Movies)
		{
			browseProductControl.NoResultsText = Resources.filter_movies_empty;
		}
		browseProductControl.ShowNoResultsMessage = true;
		browseProductControl.BorderShadowVisibility = Visibility.Collapsed;
		DisplayGroupSelectionModel displayGroupSelectionModel = new DisplayGroupSelectionModel();
		displayGroupSelectionModel.OnGroupSelectionItemClicked = BrowseViewModel_OnGroupSelectionItemClicked;
		browseProductControl.ShowBorderItems = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort != BrowseSelectedSort.Alpha;
		browseProductControl.OnNotifyBrowseItemsShown += BrowseProductControl_OnNotifyBrowseItemsShown;
		browseProductControl.OnBrowseItemModelSelected += BrowseProductControl_OnDisplayProductModelSelected;
		browseProductControl.OnAddBrowseItemModel += BrowseProductControl_OnAddDisplayProductModel;
		browseProductControl.OnPageChanged += delegate(int prevPageNumber, int newPageNumber, PageChangeSource pageChangeSource)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent((newPageNumber < prevPageNumber) ? "BrowsePageBack" : "BrowsePageNext");
			BrowseState.PageNumber = newPageNumber;
			if (BaseViewController.IsADAMode)
			{
				browseViewModel.MiniCartControlModel.SetPageNumber(newPageNumber, PageChangeSource.None);
			}
			int pageNumber = ((newPageNumber < prevPageNumber) ? (newPageNumber - 1) : (newPageNumber + 1));
			ConfigureDisplayProductModelsForPage(browseProductControl, pageNumber, isADAMiniCart: false, _configuredBrowseItemModels);
			if (BaseViewController.IsADAMode)
			{
				ConfigureDisplayProductModelsForPage(browseViewModel.MiniCartControlModel, pageNumber, isADAMiniCart: true, _configuredADAMiniCartBrowseItemModels);
			}
			ConfigureBrowseMessagePopupUserControl(browseViewModel);
			UpdateQuitButtonsText();
		};
		if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
		{
			browseProductControl.LayoutDirection = BrowseLayoutDirection.TopToBottomThenLeftToRight;
			browseProductControl.BrowseItemMargin = new Thickness(12.0, 0.0, 10.0, 0.0);
			browseProductControl.ShowBorderItems = false;
			displayGroupSelectionModel.BrowseGroupSelectionVisibility = Visibility.Visible;
			browseProductControl.PaddleOverlapWidth = 0.0;
			browseProductControl.PageNumberVisibility = Visibility.Collapsed;
			if (BaseViewController.IsADAMode)
			{
				browseProductControl.RowCount = 5;
				browseProductControl.ColumnCount = 2;
				browseProductControl.Margin = new Thickness(0.0, 0.0, 0.0, 50.0);
				displayGroupSelectionModel.BrowseGroupSelectionMargin = new Thickness(38.0, 517.0, 38.0, 0.0);
			}
			else
			{
				browseProductControl.RowCount = 6;
				browseProductControl.ColumnCount = 3;
				browseProductControl.Margin = new Thickness(0.0, 50.0, 0.0, 4.0);
				displayGroupSelectionModel.BrowseGroupSelectionMargin = new Thickness(38.0, 88.0, 38.0, 0.0);
			}
		}
		else
		{
			displayGroupSelectionModel.BrowseGroupSelectionVisibility = Visibility.Collapsed;
			browseProductControl.BottomPageNumberVisibility = ((!ShowBottomPageNumberText()) ? Visibility.Collapsed : Visibility.Visible);
			if (browseProductControl.BottomPageNumberVisibility == Visibility.Visible)
			{
				browseProductControl.PageNumberVisibility = Visibility.Collapsed;
			}
			bool show10Titles = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.Show10Titles;
			bool flag = (BrowseState.BrowseMode == BrowseViewFilter.ForSale && BrowseState.TitleFamily == TitleFamily.Movies) || Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTitleText;
			int num = -8;
			Thickness margin = new Thickness(0.0, 15.0, 0.0, 33.0);
			Thickness browseItemMargin = new Thickness(0.0, 40.0, 0.0, 0.0);
			if (show10Titles)
			{
				if (flag)
				{
					num = 24;
					margin = new Thickness(0.0, 26.0, 0.0, 0.0);
					browseItemMargin = new Thickness(0.0, 26.0, 0.0, 0.0);
				}
				else
				{
					num = 10;
					margin = new Thickness(0.0, 21.0, 0.0, 7.0);
					browseItemMargin = new Thickness(0.0, 36.0, 0.0, 0.0);
				}
			}
			else if (flag)
			{
				num = 12;
				margin = new Thickness(0.0, 19.0, 0.0, 37.0);
				browseItemMargin = new Thickness(0.0, 32.0, 0.0, 0.0);
			}
			browseProductControl.PaddleOverlapWidth = num;
			browseProductControl.Margin = margin;
			browseProductControl.BrowseItemMargin = browseItemMargin;
			browseProductControl.RowCount = 2;
			browseProductControl.ColumnCount = (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.Show10Titles ? 5 : 6);
		}
		BaseViewController.ApplicationState.BrowseProducts = GetBrowseKioskProducts();
		BrowseControlModel browseControlModel = browseProductControl;
		List<IKioskProduct> browseProducts = BaseViewController.ApplicationState.BrowseProducts;
		IBrowseState browseState3 = BrowseState;
		browseControlModel.BrowseItems = CreateBrowseItemsFromKioskProducts(browseProducts, browseState3 != null && browseState3.BrowseSelectedSort == BrowseSelectedSort.Alpha, isADAMiniCart: false, browseProductControl.VisibleItemCount, browseProductControl.PageNumber, _configuredBrowseItemModels);
		if (BrowseState != null)
		{
			BrowseState.BrowseItemModels = browseProductControl.BrowseItems;
		}
		for (int num2 = 0; num2 < browseProductControl.RowCount * browseProductControl.ColumnCount * 3; num2++)
		{
			if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
			{
				browseProductControl.BrowseControls.Add(new DisplayProductAToZUserControl
				{
					Name = $"product_{num2 + 1}"
				});
			}
			else if ((BrowseState.BrowseMode == BrowseViewFilter.ForSale && BrowseState.TitleFamily == TitleFamily.Movies) || Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTitleText)
			{
				browseProductControl.BrowseControls.Add(new DisplayProductWithPriceBrowseUserControl
				{
					Name = $"product_{num2 + 1}",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				});
			}
			else
			{
				browseProductControl.BrowseControls.Add(new DisplayProductBrowseUserControl
				{
					Name = $"product_{num2 + 1}",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				});
			}
		}
		if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
		{
			displayGroupSelectionModel.GroupSelectionItems = CreateAToZGroups(browseProductControl.BrowseItems);
		}
		if (browseProductControl.BrowseItems != null && browseProductControl.BrowseItems.Count == 0)
		{
			browseProductControl.PageNumberVisibility = Visibility.Collapsed;
			browseProductControl.BottomPageNumberVisibility = Visibility.Collapsed;
		}
		browseViewModel.BrowseGroupSelection = displayGroupSelectionModel;
		browseViewModel.BrowseProductControlModel = browseProductControl;
	}

	private static void BrowseProductControl_OnNotifyBrowseItemsShown(List<IBrowseItemModel> browseItemModels, object parameter)
	{
		List<IKioskProduct> list = new List<IKioskProduct>();
		List<IBrowseItemModel> list2 = new List<IBrowseItemModel>();
		if (browseItemModels != null)
		{
			foreach (IBrowseItemModel browseItemModel in browseItemModels)
			{
				if (browseItemModel?.Data is IKioskProduct item)
				{
					list.Add(item);
				}
				if (!browseItemModel.IsBorderItem)
				{
					list2.Add(browseItemModel);
				}
			}
		}
		if (!AreProductListsIdentical(list, BrowseState.BrowseProductsShown) || !AreTTSBrowseItemModelsIdentical(list2, BrowseState.TTSBrowseItemModelsShown))
		{
			_ = list.Count;
			_ = 0;
			BrowseState.BrowseProductsShown = list;
			BrowseState.TTSBrowseItemModelsShown = list2;
		}
	}

	private static bool AreTTSBrowseItemModelsIdentical(List<IBrowseItemModel> newBrowseItemModels, List<IBrowseItemModel> oldBrowseItemModels)
	{
		bool flag = newBrowseItemModels != null && oldBrowseItemModels != null && newBrowseItemModels.Count == oldBrowseItemModels.Count;
		if (flag)
		{
			for (int i = 0; i < newBrowseItemModels.Count; i++)
			{
				ITitleProduct titleProduct = newBrowseItemModels[i]?.Data as ITitleProduct;
				ITitleProduct titleProduct2 = oldBrowseItemModels[i]?.Data as ITitleProduct;
				flag = titleProduct != null && titleProduct2 != null && titleProduct.ProductId == titleProduct2.ProductId;
				if (!flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	private static bool AreProductListsIdentical(List<IKioskProduct> shownKioskProducts, List<IKioskProduct> browseProductsShown)
	{
		bool flag = shownKioskProducts?.Count == browseProductsShown?.Count;
		if (flag)
		{
			for (int i = 0; i < shownKioskProducts.Count; i++)
			{
				flag = shownKioskProducts[i] is ITitleProduct && browseProductsShown[i] is ITitleProduct && (shownKioskProducts[i] as ITitleProduct)?.ProductId == (browseProductsShown[i] as ITitleProduct).ProductId;
				if (!flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	private static void BrowseViewModel_OnGroupSelectionItemClicked(DisplayGroupSelectionItemModel displayGroupSelectionItemModel)
	{
		BrowseViewModel viewModel = BaseViewController.GetViewModel<BrowseViewModel>();
		if (viewModel?.BrowseGroupSelection?.GroupSelectionItems == null || displayGroupSelectionItemModel?.Name == null)
		{
			return;
		}
		BrowseState.SelectedAlphaGroupName = displayGroupSelectionItemModel.Name;
		foreach (DisplayGroupSelectionItemModel groupSelectionItem in viewModel.BrowseGroupSelection.GroupSelectionItems)
		{
			groupSelectionItem.IsSelected = groupSelectionItem.IsEnabled && groupSelectionItem.Name == displayGroupSelectionItemModel?.Name;
		}
		foreach (IBrowseItemModel item2 in viewModel.BrowseProductControlModel.BrowseItems.Where((IBrowseItemModel x) => x is DisplayGroupModel))
		{
			if (item2 is DisplayGroupModel displayGroupModel)
			{
				displayGroupModel.IsSelected = displayGroupModel.Name == displayGroupSelectionItemModel.Name;
			}
		}
		DisplayGroupModel item = viewModel?.BrowseProductControlModel?.BrowseItems.FirstOrDefault((IBrowseItemModel x) => x is DisplayGroupModel && (x as DisplayGroupModel).Name == displayGroupSelectionItemModel.Name) as DisplayGroupModel;
		int num = viewModel?.BrowseProductControlModel?.BrowseItems?.IndexOf(item) ?? (-1);
		if (num > -1)
		{
			viewModel.BrowseProductControlModel.SetPageNumber(viewModel?.BrowseProductControlModel.GetPageOfBrowseItemIndex(num) ?? 1, PageChangeSource.GroupSelection);
		}
	}

	private static List<DisplayGroupSelectionItemModel> CreateAToZGroups(List<IBrowseItemModel> browseItems)
	{
		List<DisplayGroupSelectionItemModel> list = new List<DisplayGroupSelectionItemModel>();
		List<string> list2 = new List<string> { "#" };
		for (int i = 65; i < 91; i++)
		{
			list2.Add(((char)i).ToString());
		}
		LinearGradientBrush resource = GetResource<LinearGradientBrush>(BaseViewController.GetViewUserControl(), "group_selection_item_unselected_background");
		LinearGradientBrush resource2 = GetResource<LinearGradientBrush>(BaseViewController.GetViewUserControl(), "group_selection_item_selected_background");
		foreach (string eachGroupName in list2)
		{
			bool isEnabled = browseItems.Any((IBrowseItemModel x) => x is DisplayGroupModel && ((DisplayGroupModel)x).Name == eachGroupName);
			list.Add(new DisplayGroupSelectionItemModel
			{
				Name = eachGroupName,
				EnableTextStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "group_selection_item_enabled"),
				DisabledTextStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "group_selection_item_disabled"),
				SelectedBackgroundBrush = resource2,
				UnselectedBackgroundBrusch = resource,
				IsEnabled = isEnabled,
				IsSelected = (eachGroupName == BrowseState.SelectedAlphaGroupName)
			});
		}
		return list;
	}

	private static void BrowseProductControl_OnDisplayProductModelSelected(IBrowseItemModel browseItemModel, object parameter)
	{
		if (!(browseItemModel.Data is IKioskProduct kioskProduct) || BrowseFunctions.IsRedboxPlusBrowseMode)
		{
			return;
		}
		BrowseState.BrowseSelectionSelectedProduct = kioskProduct;
		KioskProductType kioskProductType = kioskProduct.KioskProductType;
		if (kioskProductType == KioskProductType.None || (uint)(kioskProductType - 1) > 2u || !(kioskProduct is ITitleProduct titleProduct))
		{
			return;
		}
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (BrowseState != null && service != null)
		{
			BrowseState.ProductDetailShownFromView = service.Peek();
		}
		PrepareToLeaveBrowseView();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseViewDisplayProductButton")?.AddData(ProductData.ToProductData(titleProduct));
		if (OrchestrationService.PeekView() == "browse_view")
		{
			if (BrowseState.ShowingBrowseMessagePopup)
			{
				CloseBrowseMessagePopupUserControl();
				BrowseState.ShowingBrowseMessagePopup = true;
			}
			if (browseItemModel is DisplayProductModel displayProductModel && (displayProductModel.WatchOptionsVisibility == Visibility.Visible || displayProductModel.WatchOptionsAZVisibility == Visibility.Visible))
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseWatchOptions")?.AddData(ProductData.ToProductData(titleProduct));
				displayProductModel.ConfigureForTitleDetail();
				displayProductModel.IsTitleDetailsFlag = false;
				TitleDetailsController.PopulateTitleDetailProperties(displayProductModel, loadAd: false, loadMoreLikeThis: false);
				OrchestrationService.ViewShow("title_details_watch_options_popup_view", new TitleDetailsWatchOptionsPopupViewParameter
				{
					DisplayProductModel = displayProductModel
				});
			}
			else
			{
				OrchestrationService.ViewShow("product_detail_view");
			}
		}
		else
		{
			LogHelper.Instance.Log("Not opening view product_detail_view because the browse_view is not the current view.");
		}
	}

	private static void PrepareToLeaveBrowseView()
	{
		BaseViewController.GetViewModel<BrowseViewModel>();
	}

	private static void BrowseProductControl_OnAddDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (parameter?.ToString() == BrowseFunctions.Constants.ComingSoon)
		{
			ProcessComingSoonOptin(browseItemModel);
		}
		else
		{
			if (!(browseItemModel.Data is IKioskProduct kioskProduct) || BaseViewController.GetViewModel<BrowseViewModel>() == null)
			{
				return;
			}
			switch (kioskProduct.KioskProductType)
			{
			case KioskProductType.Title:
			case KioskProductType.MDV:
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseAddItem").AddData(ProductData.ToProductData(kioskProduct as ITitleProduct));
				ITitleProduct titleProduct = kioskProduct as ITitleProduct;
				if ((titleProduct?.ProductId ?? 0) == 1139)
				{
					BrowseFunctions.AddProductToCart(titleProduct, isPurchase: true);
				}
				else
				{
					ProcessAddTitleProduct(titleProduct);
				}
				break;
			}
			case KioskProductType.TitleRollup:
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseAddItem").AddData(ProductData.ToProductData(kioskProduct as ITitleProduct));
				TitleRollupProduct titleRollupProduct = kioskProduct as TitleRollupProduct;
				if ((titleRollupProduct?.ProductId ?? 0) == 1139)
				{
					BrowseFunctions.AddProductToCart(titleRollupProduct, isPurchase: true);
				}
				else if (!BrowseFunctions.IsPurchase && titleRollupProduct.KioskProducts.Count == 1)
				{
					ProcessAddTitleProduct(kioskProduct as ITitleProduct);
				}
				else if (BrowseFunctions.IsPurchase)
				{
					BrowseFunctions.ShowPurchasePopupView(titleRollupProduct, BrowseFunctions.IsPurchase);
				}
				else
				{
					BrowseFunctions.ShowRentalPopupView(titleRollupProduct, BrowseFunctions.IsPurchase, null, !BrowseFunctions.IsRedboxPlusBrowseMode, null, showPurchaseButton: true);
				}
				break;
			}
			case KioskProductType.None:
				break;
			}
		}
	}

	private static void ProcessAddTitleProduct(ITitleProduct titleProduct)
	{
		if (titleProduct.InStock || BrowseFunctions.IsPurchase)
		{
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			bool flag = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
			if ((titleProduct.HasMultiNightPrice || (titleProduct.IsSellableAndInStock && (titleProduct.IsMovie || titleProduct.IsGame))) && !BrowseFunctions.IsPurchase && !flag && !BrowseFunctions.IsRedboxPlusBrowseMode)
			{
				BrowseFunctions.AddMultiNightPricedProductToCart(titleProduct, "browse_view");
				return;
			}
			BrowseFunctions.SetRedboxPlusFreeOneNightRentalOrAddProductToCart(new AddProductToCartParameters
			{
				TitleProduct = titleProduct,
				IsPurchase = BrowseFunctions.IsPurchase,
				PricingRecord = null,
				ItemSourceType = ItemSourceType.Browse,
				RedboxPlusBenefitId = (BrowseFunctions.IsRedboxPlusBrowseMode ? new int?(2) : ((int?)null))
			});
		}
		else
		{
			ShowOutOfStockOptionsDialog(titleProduct);
		}
	}

	private static void ShowOutOfStockOptionsDialog(ITitleProduct kioskProduct)
	{
		OptionSelectionDialogInfo optionSelectionDialogInfo = new OptionSelectionDialogInfo
		{
			ViewAnalyticsName = "message_popup_view",
			ViewAnalyticsContext = "OutOfStockDialog",
			HeaderText = kioskProduct.ProductName,
			CancelButtonText = Resources.cancel_button
		};
		optionSelectionDialogInfo.CancelCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseOutOfStockCancel");
			ServiceLocator.Instance.GetService<IViewService>()?.GoBack("OutOfStockDialogCancel", popOnly: false);
		}, (Func<bool>)null);
		optionSelectionDialogInfo.DescriptionText = Resources.out_of_stock_movie_message;
		ObservableCollection<ButtonData> observableCollection = new ObservableCollection<ButtonData>();
		observableCollection.Add(new ButtonData(Resources.related_movies, DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseOutOfStockRelatedMovies");
			BrowseFunctions.ShowRelatedProductsDialog(kioskProduct);
		}, (Func<bool>)null)));
		optionSelectionDialogInfo.ButtonData = observableCollection;
		OrchestrationService.ViewShow("option_selection_dialog_view", optionSelectionDialogInfo);
	}

	private static void ProcessComingSoonOptin(IBrowseItemModel browseItemModel)
	{
		IKioskProduct kioskProduct = browseItemModel.Data as IKioskProduct;
		ITitleProduct titleProduct = kioskProduct as ITitleProduct;
		IComingSoonOptinInfo comingSoonOptInInfo = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.ComingSoonOptInInfo;
		if (titleProduct == null || comingSoonOptInInfo == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(comingSoonOptInInfo.EmailAddress))
		{
			if (comingSoonOptInInfo.OptInItems?.FirstOrDefault((IOptInItem x) => x.ItemId == titleProduct.ProductId) == null)
			{
				comingSoonOptInInfo.AddProduct(titleProduct.ProductId);
			}
			else
			{
				comingSoonOptInInfo.RemoveProduct(titleProduct.ProductId);
			}
			if (browseItemModel is DisplayProductModel browseItemModel2)
			{
				ConfigureDisplayProductModel(kioskProduct, browseItemModel2, isADAMiniCart: false, GetDisplayProductModelConstants());
			}
			return;
		}
		OrchestrationService.ViewShow("keyboard_email_coming_soon_view", new KeyboardInfo
		{
			MaxLength = 64,
			KeyboardText = comingSoonOptInInfo.EmailAddress,
			CancelAction = delegate
			{
				BaseViewController.GetViewService()?.GoBack("go back", popOnly: false);
			},
			IsValidEntry = ValidationServices.IsValidEmailAddress,
			ContinueAction = delegate(string keyboardText)
			{
				if (comingSoonOptInInfo != null && BaseViewController.GetViewService() != null)
				{
					comingSoonOptInInfo.EmailAddress = keyboardText;
					comingSoonOptInInfo.AddProduct(titleProduct.ProductId);
					BaseViewController.GetViewService().UnwindTo("browse_view", discard: true, "KeyboardView: KeyboardMode.EMAIL_COMING_SOON  - NEXT button");
				}
			}
		});
	}

	private static void ConfigureMiniCartControl()
	{
		ConfigureMiniCartControl(BaseViewController.GetViewModel<BrowseViewModel>());
	}

	private static void ConfigureMiniCartControl(BrowseViewModel browseViewModel)
	{
		if (browseViewModel != null)
		{
			OrchestrationService.SetCurrentUICulture();
			BrowseControlModel browseControlModel = new BrowseControlModel();
			browseControlModel.OnAddBrowseItemModel += BrowseProductControl_OnAddDisplayProductModel;
			browseControlModel.BackgroundColor = Colors.Transparent;
			browseControlModel.ShowBorderItems = false;
			browseControlModel.PageNumberVisibility = Visibility.Collapsed;
			browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
			browseControlModel.Margin = (BaseViewController.IsADAMode ? new Thickness(0.0, 0.0, 0.0, 20.0) : new Thickness(0.0, 0.0, 0.0, 0.0));
			browseControlModel.BrowseItemMargin = new Thickness(0.0, 12.0, 0.0, 0.0);
			browseControlModel.RowCount = ((!BaseViewController.IsADAMode) ? 1 : 2);
			browseControlModel.ColumnCount = ((BrowseFunctions.IsRedboxPlusBrowseMode && !BaseViewController.IsADAMode) ? 4 : ((!Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.Show10Titles && BaseViewController.IsADAMode) ? 6 : 5));
			int num = ((!BaseViewController.IsADAMode) ? 5 : (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.Show10Titles ? 10 : 12));
			for (int i = 0; i < num; i++)
			{
				browseControlModel.BrowseControls.Add(new DisplayProductWithCommandsUserControl
				{
					Name = $"minicart_product_{i + 1}",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				});
			}
			browseControlModel.OnCancelBrowseItemModel += MiniCartControl_OnCancelDisplayProductModel;
			if (BaseViewController.IsADAMode)
			{
				List<IKioskProduct> browseProducts = BaseViewController.ApplicationState.BrowseProducts;
				IBrowseState browseState = BrowseState;
				browseControlModel.BrowseItems = CreateBrowseItemsFromKioskProducts(browseProducts, browseState != null && browseState.BrowseSelectedSort == BrowseSelectedSort.Alpha, isADAMiniCart: true, num, 1, _configuredADAMiniCartBrowseItemModels);
				browseControlModel.SetPageNumber(BrowseState.PageNumber, PageChangeSource.None);
			}
			else
			{
				browseControlModel.BrowseItems = CreateBrowseItemsFromCartItems(BrowseFunctions.IsRedboxPlusBrowseMode);
			}
			browseViewModel.MiniCartControlModel = browseControlModel;
			browseViewModel.MiniCartVisibility = Visibility.Visible;
			browseViewModel.EmptyCartSpacesVisibility = (BaseViewController.IsADAMode ? Visibility.Collapsed : Visibility.Visible);
			browseViewModel.EmptyCartAddItemMessageVisibility = ((BaseViewController.IsADAMode || BrowseFunctions.IsRedboxPlusBrowseMode || (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.Count).GetValueOrDefault() != 0) ? Visibility.Collapsed : Visibility.Visible);
			string text = ((RedboxPlusFunctions.RedboxPlusSubscriptionTier == RedboxPlusSubscriptionTier.Tier1) ? Resources.redbox_plus_mini_cart_unavailable_tier1_message_text : Resources.redbox_plus_mini_cart_unavailable_tier2_message_text);
			browseViewModel.RedboxPlusMiniCartMessageText = ((!RedboxPlusFunctions.IsFreeOneNightRentalAvailable) ? text : ((BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.HasRedboxPlusFreeOneNightRentalSelected == true) ? Resources.redbox_plus_mini_cart_title_selected : (BrowseFunctions.CanAddProductToCart ? Resources.redbox_plus_mini_cart_empty_message_text : Resources.redbox_plus_mini_cart_full_message_text)));
			browseViewModel.RedboxPlusMiniCartVisiblity = ((!BrowseFunctions.IsRedboxPlusBrowseMode || BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
			browseViewModel.RedboxPlusEmptyCartSpaceVisiblity = ((!BrowseFunctions.IsRedboxPlusBrowseMode || !RedboxPlusFunctions.IsFreeOneNightRentalAvailable) ? Visibility.Collapsed : Visibility.Visible);
			browseViewModel.AddToCartLabel1Text = Resources.add_to_cart_label1;
			browseViewModel.AddToCartLabel2Text = Resources.add_to_cart_label2;
		}
	}

	private static void MiniCartControl_OnCancelDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel == null)
		{
			return;
		}
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = browseItemModel.Data as IRentalShoppingCartTitleItem;
		IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem = browseItemModel.Data as IRentalShoppingCartSubscriptionItem;
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart == null || (rentalShoppingCartTitleItem == null && rentalShoppingCartSubscriptionItem == null))
		{
			return;
		}
		IBaseRentalShoppingCartItem baseRentalShoppingCartItem = rentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == rentalShoppingCartTitleItem?.ProductId);
		if (baseRentalShoppingCartItem == null)
		{
			baseRentalShoppingCartItem = rentalShoppingCart.SubscriptionItems.FirstOrDefault((IRentalShoppingCartSubscriptionItem x) => x != null && x.SubscriptionProduct.CompareTo(rentalShoppingCartSubscriptionItem?.SubscriptionProduct) == 0);
		}
		if (baseRentalShoppingCartItem != null)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseShoppingCartRemoveItem");
			BrowseFunctions.PromptToRemoveItem(baseRentalShoppingCartItem, userRemoved: true);
		}
	}

	private static void CurrentRentalShoppingCart_OnChange(IRentalShoppingCart rentalShoppingCart)
	{
		BrowseViewModel viewModel = BaseViewController.GetViewModel<BrowseViewModel>();
		if (viewModel == null)
		{
			return;
		}
		if (!BaseViewController.IsADAMode)
		{
			ConfigureMiniCartControl(viewModel);
		}
		ConfigureCheckOutBar(viewModel);
		if (BaseViewController.IsADAMode)
		{
			ConfigureTopMenu(viewModel);
		}
		if (viewModel?.BrowseProductControlModel?.BrowseItems != null)
		{
			UpdateDisplayProductModels(viewModel.BrowseProductControlModel.BrowseItems, isADAMiniCart: false);
			if (BaseViewController.IsADAMode)
			{
				UpdateDisplayProductModels(viewModel.MiniCartControlModel.BrowseItems, isADAMiniCart: true);
			}
		}
		if (BrowseState.ShowingBrowseMessagePopup && rentalShoppingCart != null && rentalShoppingCart.Count > 0)
		{
			CloseBrowseMessagePopupUserControl();
		}
		ConfigureBrowseMessagePopupUserControl(viewModel);
	}

	private static void UpdateDisplayProductModels(List<IBrowseItemModel> displayProductModels, bool isADAMiniCart)
	{
		DisplayProductModelConstants displayProductModelConstants = GetDisplayProductModelConstants();
		foreach (IBrowseItemModel displayProductModel2 in displayProductModels)
		{
			if (displayProductModel2 is DisplayProductModel displayProductModel)
			{
				ConfigureDisplayProductModel(displayProductModel.Data as IKioskProduct, displayProductModel, isADAMiniCart, displayProductModelConstants);
			}
		}
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromCartItems(bool onlyRedboxPlusFreeOneNightRental)
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (rentalShoppingCart != null && service != null)
		{
			foreach (IBaseRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				IKioskProduct kioskProduct = null;
				IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = item as IRentalShoppingCartTitleItem;
				IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem = item as IRentalShoppingCartSubscriptionItem;
				if (rentalShoppingCartTitleItem != null)
				{
					service.GetProduct(rentalShoppingCartTitleItem.ProductId, excludeTitleRollup: true, out kioskProduct);
				}
				else if (rentalShoppingCartSubscriptionItem != null)
				{
					kioskProduct = rentalShoppingCartSubscriptionItem.SubscriptionProduct;
				}
				if ((onlyRedboxPlusFreeOneNightRental && (rentalShoppingCartTitleItem == null || rentalShoppingCartTitleItem == null || !rentalShoppingCartTitleItem.IsRedboxPlusFreeOneNightRental)) || kioskProduct == null)
				{
					continue;
				}
				DisplayProductModel displayProductModel = new DisplayProductModel
				{
					Name = kioskProduct.ProductName,
					Data = item,
					Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
				};
				displayProductModel.ConfigureForMiniCart();
				displayProductModel.MiniCartCancelButtonVisibility = ((InTitleMarketingMode && item.Sequence == RentalShoppingCartItemSequence.pre_offer) ? Visibility.Collapsed : Visibility.Visible);
				IKioskProduct kioskProduct2 = null;
				if (rentalShoppingCartTitleItem != null)
				{
					service.GetProduct(rentalShoppingCartTitleItem.ProductId, excludeTitleRollup: false, out kioskProduct2);
				}
				MDVProduct mDVProduct = kioskProduct2 as MDVProduct;
				bool flag = item.Action == RentalShoppingCartItemAction.purchase;
				if (flag)
				{
					displayProductModel.Banner = BannerCollection.Instance.FirstOrDefault((Banner x) => x.BannerType == BannerType.Purchase);
				}
				else if (mDVProduct != null)
				{
					displayProductModel.Banner = BannerCollection.Instance.FirstOrDefault((Banner x) => x.BannerType == BannerType.MultipleDisc);
				}
				else if (rentalShoppingCartSubscriptionItem?.SubscriptionProduct != null)
				{
					displayProductModel.Banner = BannerCollection.Instance.FirstOrDefault((Banner x) => x.BannerType == BannerType.Plan);
				}
				else
				{
					displayProductModel.Banner = null;
				}
				displayProductModel.BannerVisibility = ((displayProductModel.Banner == null) ? Visibility.Collapsed : Visibility.Visible);
				if (flag && displayProductModel.Banner != null)
				{
					displayProductModel.BannerText = Resources.ResourceManager.GetString(displayProductModel.Banner.TextResourceName);
				}
				else if (mDVProduct != null)
				{
					displayProductModel.BannerText = string.Format("{1} {0}", Resources.ResourceManager.GetString(displayProductModel.Banner.TextResourceName), mDVProduct.TitleProducts.Count);
				}
				else if (rentalShoppingCartSubscriptionItem != null)
				{
					displayProductModel.BannerText = Resources.ResourceManager.GetString(displayProductModel.Banner.TextResourceName);
				}
				else
				{
					displayProductModel.BannerText = null;
				}
				displayProductModel.BannerBackgroundBrush = (((flag || mDVProduct != null || rentalShoppingCartSubscriptionItem != null) && displayProductModel.Banner != null) ? WPFFunctions.GetBrush(BaseViewController.GetViewUserControl(), displayProductModel.Banner.BannerStyleName) : null);
				list.Add(displayProductModel);
			}
		}
		return list;
	}

	private static DisplayProductModelConstants GetDisplayProductModelConstants()
	{
		DisplayProductModelConstants obj = new DisplayProductModelConstants
		{
			StarringLabel = Resources.starring_label,
			GenresLabel = Resources.title_details_genres_text,
			OneNightPriceLabel = Resources.one_night_price_label,
			OneNightReturnTimeLabel = Resources.return_time_label,
			MultiNightPriceLabel = Resources.multi_night_price_label,
			MultiNightReturnTimeLabel = Resources.return_time_label,
			BuyText = Resources.title_detail_buy,
			TitleDetailFormatButtonTextValues = new TitleRollupUtilities.TitleDetailFormatRentalButtonTextValues(),
			Add = Resources.add,
			ComingSoonLabel = Resources.coming_label,
			OutOfStockLine1Text = Resources.out_of_stock_line1,
			OutOfStockLine2Text = Resources.out_of_stock_line2,
			OutOfStockLine3Text = Resources.out_of_stock_line3,
			OutOfStockLine4Text = Resources.out_of_stock_line4,
			Movies = Resources.Movies,
			Games = Resources.Games,
			Movie = Resources.Movie.ToLower(),
			Game = Resources.game.ToLower(),
			FontMontserrat16Style = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_montserrat_extrabold_16"),
			FontMontserrat20Style = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_montserrat_medium_20"),
			PriceFor = Resources.price_for,
			Night = Resources.night,
			PricingService = ServiceLocator.Instance.GetService<IPricingService>(),
			RentalShoppingCartService = BaseViewController.RentalShoppingCartService
		};
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		obj.IsInCartBrowse = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
		obj.OutOfStockTitleDetailsABTestVersion = Redbox.Rental.Services.Configuration.Configuration.Instance.OutOfStockTitleDetailsABTestVersion;
		obj.EnableRedboxPlus = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableRedboxPlusRedemption;
		obj.KioskClientService = ServiceLocator.Instance.GetService<IKioskClientService>();
		obj.ShowWatchOptions = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowWatchOptions;
		obj.ShowDualInStock = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowDualInStock;
		return obj;
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromKioskProducts(List<IKioskProduct> kioskProducts, bool createAlphaGroups, bool isADAMiniCart, int visibleItemCount, int pageNumber, List<long> configuredBrowseItemModels)
	{
		DisplayGroupModel displayGroupModel = null;
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		configuredBrowseItemModels?.Clear();
		if (kioskProducts != null)
		{
			DisplayProductModelConstants displayProductModelConstants = GetDisplayProductModelConstants();
			int num = Math.Max((pageNumber - 2) * visibleItemCount, 0);
			int num2 = num + visibleItemCount * 3;
			if (createAlphaGroups)
			{
				num = 0;
				num2 = kioskProducts.Count;
			}
			int num3 = 0;
			foreach (IKioskProduct kioskProduct in kioskProducts)
			{
				ITitleProduct titleProduct = kioskProduct as ITitleProduct;
				string productDisplayName = BrowseFunctions.GetProductDisplayName(titleProduct);
				string text = null;
				if (titleProduct.IsGame)
				{
					TitleTypeProperty value = null;
					TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value);
					text = " " + value?.GetName(Resources.ResourceManager);
				}
				string text2 = ((titleProduct is MDVProduct mDVProduct) ? $" ({mDVProduct.TitleProducts.Count} {Resources.multi_disc_vend_discs_text})" : null);
				string header = productDisplayName + text + text2;
				DisplayProductModel displayProductModel;
				if (BrowseState.BrowseMode != BrowseViewFilter.ForSale || BrowseState.TitleFamily != TitleFamily.Movies)
				{
					displayProductModel = ((!Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.ShowTitleText) ? new DisplayProductModel
					{
						Name = kioskProduct.ProductName,
						Data = kioskProduct,
						Image = OrchestrationService.GetProductBitmapImage(kioskProduct),
						LockAspectRatio = true,
						Header = header,
						DisplayProductModelConstants = displayProductModelConstants
					} : new DisplayProductWithPriceModel
					{
						Name = kioskProduct.ProductName,
						Data = kioskProduct,
						Image = OrchestrationService.GetProductBitmapImage(kioskProduct),
						LockAspectRatio = true,
						Header = header,
						DisplayProductModelConstants = displayProductModelConstants,
						TitleText = kioskProduct.ProductName,
						TitleTextWidth = (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.Show10Titles ? 114 : 103)
					});
				}
				else
				{
					string purchasePriceText = BrowseFunctions.GetPurchasePriceText(titleProduct);
					purchasePriceText = (string.IsNullOrEmpty(purchasePriceText) ? "" : (Resources.browse_view_rent_and_buy_purchase_text + ": " + purchasePriceText));
					displayProductModel = new DisplayProductWithPriceModel
					{
						Name = kioskProduct.ProductName,
						Data = kioskProduct,
						Image = OrchestrationService.GetProductBitmapImage(kioskProduct),
						LockAspectRatio = true,
						Header = header,
						DisplayProductModelConstants = displayProductModelConstants,
						PriceText = (purchasePriceText ?? "")
					};
				}
				if (num3 >= num && num3 < num2)
				{
					ConfigureDisplayProductModel(kioskProduct, displayProductModel, isADAMiniCart, displayProductModelConstants);
					configuredBrowseItemModels?.Add(titleProduct.ProductId);
				}
				num3++;
				if (createAlphaGroups)
				{
					string alphaGroupCharacter = GetAlphaGroupCharacter(kioskProduct as ITitleProduct);
					if (displayGroupModel == null || displayGroupModel.Name != alphaGroupCharacter)
					{
						displayGroupModel = new DisplayGroupModel
						{
							Name = alphaGroupCharacter,
							SeparatorLineStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "separator_line_for_group"),
							UnselectedTitleStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "browse_group_header"),
							SelectedTitleStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "browse_group_header_selected"),
							IsSelected = (alphaGroupCharacter == BrowseState.SelectedAlphaGroupName)
						};
						list.Add(displayGroupModel);
					}
					displayProductModel.SeparatorLineStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "separator_line_for_product");
					displayProductModel.TitleStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_montserrat_regular_16");
				}
				list.Add(displayProductModel);
			}
		}
		return list;
	}

	private static void ConfigureDisplayProductModelsForPage(BrowseControlModel browseControlModel, int pageNumber, bool isADAMiniCart, List<long> configuredBrowseItemModels)
	{
		if (browseControlModel?.BrowseItems == null || pageNumber < 0 || pageNumber > browseControlModel.TotalPages)
		{
			return;
		}
		int num = (pageNumber - 1) * browseControlModel.VisibleItemCount;
		int num2 = num + browseControlModel.VisibleItemCount;
		DisplayProductModelConstants displayProductModelConstants = null;
		for (int i = num; i < num2; i++)
		{
			if (i < 0 || i > browseControlModel.BrowseItems.Count - 1)
			{
				continue;
			}
			IBrowseItemModel browseItemModel = browseControlModel.BrowseItems[i];
			IKioskProduct kioskProduct = browseItemModel?.Data as IKioskProduct;
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			DisplayProductModel browseItemModel2 = browseItemModel as DisplayProductModel;
			if (titleProduct != null && (configuredBrowseItemModels == null || !configuredBrowseItemModels.Contains(titleProduct.ProductId)))
			{
				if (displayProductModelConstants == null)
				{
					displayProductModelConstants = GetDisplayProductModelConstants();
				}
				ConfigureDisplayProductModel(kioskProduct, browseItemModel2, isADAMiniCart, displayProductModelConstants);
				configuredBrowseItemModels?.Add(titleProduct.ProductId);
			}
		}
	}

	private static string GetAlphaGroupCharacter(ITitleProduct browseItem)
	{
		char c = ((browseItem != null && browseItem.SortName != null) ? browseItem.SortName.ToUpper()[0] : ' ');
		string result = "#";
		if (c >= 'A' && c <= 'Z')
		{
			result = c.ToString();
		}
		return result;
	}

	public static void ConfigureDisplayProductModel(IKioskProduct eachKioskProduct, DisplayProductModel browseItemModel, bool isADAMiniCart, DisplayProductModelConstants displayProductModelConstants)
	{
		if (browseItemModel == null || eachKioskProduct == null)
		{
			return;
		}
		ITitleProduct titleProduct = eachKioskProduct as ITitleProduct;
		bool isEmptyCase = titleProduct?.IsEmptyCase ?? false;
		MDVProduct mDVProduct = titleProduct as MDVProduct;
		browseItemModel.MDVFieldVisibility = ((mDVProduct == null) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.MDVNumberOfDiscsText = ((mDVProduct != null) ? $"({mDVProduct.TitleProducts.Count} {Resources.discs_text})" : null);
		browseItemModel.MDVDiscVendText = ((mDVProduct != null) ? $"{mDVProduct.TitleProducts.Count} {Resources.disc_vend}" : null);
		BannerType bannerType = BannerType.None;
		browseItemModel.ComingSoonButtonVisibility = Visibility.Collapsed;
		if (BrowseState.BrowseSelectedSort != BrowseSelectedSort.Alpha)
		{
			bannerType = ((titleProduct == null || !titleProduct.ComingSoon) ? BrowseFunctions.GetProductBannerType(titleProduct, BrowseState.TitleType != TitleType.All) : BannerType.ComingSoon);
		}
		bool flag = AreAllProductFormatsInCart(titleProduct, displayProductModelConstants);
		bool flag2 = (!(displayProductModelConstants.OutOfStockTitleDetailsABTestVersion == "B") || titleProduct.TitleFamily != TitleFamily.Movies) && !titleProduct.InStock;
		_ = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		IBrowseState browseState = BrowseState;
		if ((browseState == null || browseState.BrowseMode != BrowseViewFilter.ComingSoon) && ((!BrowseFunctions.CanAddProductToCart || !titleProduct.IsAvailable || (flag && (!BrowseFunctions.IsRedboxPlusBrowseMode || (BaseViewController.IsADAMode && RedboxPlusFunctions.IsTitleProductSelectedRedboxPlusFreeOneNightRental(titleProduct))))) | flag2))
		{
			browseItemModel.ImageOpacity = 0.4;
		}
		else
		{
			browseItemModel.ImageOpacity = 1.0;
		}
		BrowseFunctions.ConfigureBanner(browseItemModel, titleProduct, isADAMiniCart, bannerType, BaseViewController.GetViewUserControl());
		ConfigureWatchOptionsAndDualInStock(browseItemModel, titleProduct, displayProductModelConstants);
		ProductFlags browseProductFlag = GetBrowseProductFlag(titleProduct, isEmptyCase, flag);
		BrowseFunctions.ConfigureProductFlag(browseItemModel, browseProductFlag, isADAMiniCart, BaseViewController.GetViewUserControl(), titleProduct, displayProductModelConstants);
		bool flag3 = (displayProductModelConstants.OutOfStockTitleDetailsABTestVersion == "B" && titleProduct.TitleFamily == TitleFamily.Movies) || (titleProduct?.InStock ?? false);
		bool flag4 = (BrowseFunctions.CanAddProductToCart && titleProduct != null && titleProduct.IsAvailable && (!flag || (BrowseFunctions.IsRedboxPlusBrowseMode && (!BaseViewController.IsADAMode || !RedboxPlusFunctions.IsTitleProductSelectedRedboxPlusFreeOneNightRental(titleProduct))))) & flag3;
		browseItemModel.CanAdd = flag4 || BrowseState.BrowseMode == BrowseViewFilter.ComingSoon;
		Visibility visibility = ((!flag4 && !BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.CornerAddButtonVisibility = ((!isADAMiniCart) ? visibility : Visibility.Collapsed);
		browseItemModel.RightAddButtonVisibility = ((visibility == Visibility.Collapsed) ? Visibility.Hidden : visibility);
		browseItemModel.ADAMiniCartAddButtonVisibility = ((!isADAMiniCart) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AddButtonPlusSignVisibility = (BaseViewController.IsADAMode ? Visibility.Hidden : Visibility.Visible);
		browseItemModel.AddButtonTextVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AddButtonStyle = (BaseViewController.IsADAMode ? WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "rubine_white_border_button_style") : WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "classic_rubine_button_style"));
		if (isADAMiniCart)
		{
			browseItemModel.ConfigureForADAMiniCart();
		}
		else if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
		{
			browseItemModel.ConfigureForAToZBrowse();
		}
		else
		{
			browseItemModel.ConfigureForBrowse();
		}
		bool flag5 = titleProduct is TitleRollupProduct && titleProduct.KioskProductType == KioskProductType.TitleRollup;
		Visibility visibility2 = ((BrowseFunctions.IsShoppingCartFull || (!titleProduct.IsSellableAndInStock && !titleProduct.IsEmptyCase) || !(flag5 ? (!((TitleRollupProduct)titleProduct).KioskProducts.All((ITitleProduct x) => BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.HasTitleItem(x.ProductId))) : (!BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.HasTitleItem(titleProduct.ProductId)))) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.ShowBuy = ((visibility2 != Visibility.Visible || titleProduct.TitleFamily == TitleFamily.Games) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.ButtonData.Clear();
		if (!titleProduct.IsEmptyCase)
		{
			if (flag5)
			{
				foreach (ITitleProduct item in ((TitleRollupProduct)titleProduct).KioskProducts.OrderBy((ITitleProduct p) => p.TitleType))
				{
					AddFormatButtonData(browseItemModel, item, flag5, displayProductModelConstants);
				}
			}
			else
			{
				AddFormatButtonData(browseItemModel, titleProduct, flag5, displayProductModelConstants);
			}
		}
		browseItemModel.ComingSoonVisibility = Visibility.Collapsed;
	}

	public static void UpdateTitles()
	{
		DisplayProductModelConstants displayProductModelConstants = GetDisplayProductModelConstants();
		(BaseViewController.GetViewModel<BrowseViewModel>()?.BrowseProductControlModel?.BrowseItems?.Where((IBrowseItemModel bi) => bi is DisplayProductModel).Select((IBrowseItemModel bi) => bi as DisplayProductModel).ToList())?.Where((DisplayProductModel dpm) => dpm.Data is ITitleProduct && !(dpm.Data as ITitleProduct).InStock).ForEach(delegate(DisplayProductModel dpm)
		{
			ConfigureWatchOptionsAndDualInStock(dpm, dpm.Data as ITitleProduct, displayProductModelConstants);
		});
		(BaseViewController.GetViewModel<TitleDetailModel>()?.BrowseProductControlModel?.BrowseItems?.Where((IBrowseItemModel bi) => bi is DisplayProductModel).Select((IBrowseItemModel bi) => bi as DisplayProductModel).ToList())?.Where((DisplayProductModel dpm) => dpm.Data is ITitleProduct && !(dpm.Data as ITitleProduct).InStock).ForEach(delegate(DisplayProductModel dpm)
		{
			ConfigureWatchOptionsAndDualInStock(dpm, dpm.Data as ITitleProduct, displayProductModelConstants);
			dpm.WatchOptionsVisibility = Visibility.Collapsed;
			dpm.DualInStockVisibility = Visibility.Collapsed;
		});
	}

	private static void ConfigureWatchOptionsAndDualInStock(DisplayProductModel browseItemModel, ITitleProduct titleProduct, DisplayProductModelConstants displayProductModelConstants)
	{
		browseItemModel.WatchOptionsVisibility = Visibility.Collapsed;
		browseItemModel.WatchOptionsAZVisibility = Visibility.Collapsed;
		browseItemModel.DualInStockVisibility = Visibility.Collapsed;
		browseItemModel.DualInStockAZVisibility = Visibility.Collapsed;
		browseItemModel.OutOfStockTextAZVisibility = Visibility.Collapsed;
		bool flag = !titleProduct.InStock && BrowseState.BrowseMode != BrowseViewFilter.ComingSoon && !BaseViewController.IsADAMode && !titleProduct.IsEmptyCase;
		bool flag2 = displayProductModelConstants.ShowWatchOptions & flag;
		bool valueOrDefault = displayProductModelConstants.KioskClientService?.Kiosk?.HasDual == true;
		bool flag3 = false;
		if (valueOrDefault)
		{
			flag3 = ((!(titleProduct is TitleRollupProduct titleRollupProduct)) ? (displayProductModelConstants.KioskClientService?.Dual?.IsInStockInOther(titleProduct.ProductId) == true) : (titleRollupProduct.KioskProducts?.Any((ITitleProduct tp) => displayProductModelConstants.KioskClientService?.Dual?.IsInStockInOther(tp.ProductId) == true) ?? false));
		}
		bool flag4 = displayProductModelConstants.ShowDualInStock & flag & flag3;
		if (flag4)
		{
			string arg = displayProductModelConstants.KioskClientService?.Kiosk?.DualKiosk?.Label ?? "A";
			if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
			{
				browseItemModel.DualInStockAZVisibility = Visibility.Visible;
				browseItemModel.OutOfStockTextAZVisibility = Visibility.Visible;
				browseItemModel.DualInStockTextAZKioskText = string.Format(Resources.dual_in_stock_text_az_kiosk_text, arg);
			}
			else
			{
				browseItemModel.DualInStockVisibility = Visibility.Visible;
				browseItemModel.DualInStockTextKioskText = string.Format(Resources.dual_in_stock_text_kiosk_text, arg);
			}
		}
		else if (flag2)
		{
			if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
			{
				browseItemModel.WatchOptionsAZVisibility = Visibility.Visible;
				browseItemModel.OutOfStockTextAZVisibility = Visibility.Visible;
			}
			else
			{
				browseItemModel.WatchOptionsVisibility = Visibility.Visible;
			}
		}
		bool flag5 = (titleProduct.TitleFamily == TitleFamily.Movies && displayProductModelConstants.OutOfStockTitleDetailsABTestVersion == "B") || titleProduct.InStock;
		browseItemModel.AddButtonBarVisibility = ((!((!BrowseFunctions.IsShoppingCartFull && !titleProduct.ComingSoon) & flag5)) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.OutOfStockVisibility = ((flag5 || flag4 || titleProduct.ComingSoon) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.DualInStockTitleDetailsVisibility = ((!flag4) ? Visibility.Collapsed : Visibility.Visible);
	}

	private static void AddFormatButtonData(DisplayProductModel browseItemModel, ITitleProduct product, bool isTitleRollup, DisplayProductModelConstants displayProductModelConstants)
	{
		bool showOutOfStock = product.TitleFamily == TitleFamily.Games || displayProductModelConstants.OutOfStockTitleDetailsABTestVersion == "A";
		TitleRollupFormatButtonProperties rollupFormatButtonProperties = TitleRollupUtilities.GetRollupFormatButtonProperties(product, isPurchase: false, displayProductModelConstants.TitleDetailFormatButtonTextValues, showOutOfStock);
		if (rollupFormatButtonProperties.Visibility == Visibility.Visible)
		{
			TilteDetailRentalFormatButtonData tilteDetailRentalFormatButtonData = new TilteDetailRentalFormatButtonData(rollupFormatButtonProperties.ButtonText, rollupFormatButtonProperties.TitleType, rollupFormatButtonProperties.ButtonTextLine2, rollupFormatButtonProperties.IsEnabled);
			if (!product.IsMovie || product.IsEmptyCase)
			{
				tilteDetailRentalFormatButtonData.Line1Text = displayProductModelConstants.Add;
			}
			browseItemModel.ButtonData.Add(tilteDetailRentalFormatButtonData);
		}
	}

	private static bool AreAllProductFormatsInCart(ITitleProduct titleProduct, DisplayProductModelConstants displayProductModelConstants)
	{
		bool flag = true;
		if (titleProduct != null)
		{
			if (titleProduct is TitleRollupProduct titleRollupProduct)
			{
				foreach (ITitleProduct kioskProduct in titleRollupProduct.KioskProducts)
				{
					if ((BrowseState.BrowseMode == BrowseViewFilter.ForSale && kioskProduct.IsSellable) || (BrowseState.BrowseMode != BrowseViewFilter.ForSale && kioskProduct.IsRentable))
					{
						bool flag2 = (!(displayProductModelConstants.OutOfStockTitleDetailsABTestVersion == "B") || kioskProduct.TitleFamily != TitleFamily.Movies) && !kioskProduct.InStock;
						flag = flag && (flag2 || (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.HasTitleItem(kioskProduct.ProductId) ?? true));
					}
				}
			}
			else
			{
				flag = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.HasTitleItem(titleProduct.ProductId) ?? true;
			}
		}
		return flag;
	}

	private static ProductFlags GetBrowseProductFlag(ITitleProduct titleProduct, bool isEmptyCase, bool areAllProductFormatsInCart)
	{
		ProductFlags result = ProductFlags.None;
		if (titleProduct != null && !BrowseFunctions.IsShoppingCartFull && !InTitleMarketingMode)
		{
			if (isEmptyCase && !titleProduct.InStock)
			{
				result = ProductFlags.OutOfStock;
			}
			else if (!titleProduct.IsAvailable)
			{
				result = ProductFlags.ComingSoon;
			}
			else if (BrowseState.BrowseMode != BrowseViewFilter.ForSale && titleProduct.ShowOutOfStock)
			{
				result = ProductFlags.OutOfStock;
			}
			else if (!areAllProductFormatsInCart)
			{
				result = BrowseFunctions.GetProductFlag(titleProduct);
			}
		}
		return result;
	}

	private static List<IKioskProduct> GetBrowseKioskProducts()
	{
		IBrowseState browseState = Redbox.Rental.Services.Session.Session.Instance.BrowseState;
		LogHelper.Instance.Log("RentalGetProducts by browseMode: {0}, family: {1}, familyType: {2}, selectedBrowseSort: {3}, selectedGenre: {4}, selectedRating: {5}, selectedSort: {6}, isADAView: {7}, priceFilterLow: {8}, priceFilterHigh {9}, studio {10}", browseState.BrowseMode, browseState.TitleFamily, browseState.TitleType, browseState.BrowseSelectedSort, browseState.Genre, browseState.Rating, browseState.Sort, BaseViewController.IsADAMode, browseState.BrowsePriceRangeFilter?.PriceRangeLow, browseState.BrowsePriceRangeFilter?.PriceRangeHigh, browseState.BrowseStudio);
		try
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			BrowseSort browseSort = GetBrowseSort();
			List<long> include = new List<long>();
			List<long> exclude = new List<long>();
			List<TitleType> excludeTitleTypes = new List<TitleType>();
			bool excludeDiscountRestriction = false;
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			if (rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel)
			{
				include = PromotionService.Instance.GetInCartIncludeTitleList();
				exclude = PromotionService.Instance.GetInCartExcludeTitleList();
				excludeTitleTypes = PromotionService.Instance.GetInCartExcludeTitleTypeList();
				excludeDiscountRestriction = PromotionService.Instance.GetInCartExcludeDiscountRestriction();
			}
			ErrorList products = service.GetProducts(browseState.BrowseMode, browseState.TitleFamily, browseState.TitleType, browseSort, (long?)browseState.Genre, (long?)browseState.Rating, include, exclude, BaseViewController.IsADAMode, browseState.BrowsePriceRangeFilter?.PriceRangeLow, browseState.BrowsePriceRangeFilter?.PriceRangeHigh, out var products2, browseState.BrowseStudio, excludeTitleTypes, excludeDiscountRestriction);
			if (products.Count > 0)
			{
				products.ToLogHelper();
				return null;
			}
			if (products2 == null)
			{
				LogHelper.Instance.Log("Error: RentalDataFunctions.RentalGetProducts return null");
				return null;
			}
			LogHelper.Instance.Log("RentalGetProducts by browseViewfilter count: {0}", products2.Count);
			return products2;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.RentalGetProducts", e);
			return null;
		}
	}

	private static bool IsTitleTypeAvailable(TitleType titleType)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<IKioskProduct> products = null;
		service?.GetProducts(BrowseViewFilter.All, TitleFamily.Movies, titleType, BrowseSort.MerchandiseDate, null, null, null, null, isADAView: false, null, null, out products);
		if (products == null)
		{
			return false;
		}
		return products.Count > 0;
	}

	private static T GetResource<T>(FrameworkElement frameworkElement, string resourceName)
	{
		T result = default(T);
		if (!string.IsNullOrEmpty(resourceName))
		{
			object obj = frameworkElement?.FindResource(resourceName);
			if (obj != null && obj is T result2)
			{
				return result2;
			}
		}
		return result;
	}

	private static List<BrowseFilter> CreateBrowseFilters(IConfiguration configuration)
	{
		List<BrowseFilter> list = new List<BrowseFilter>();
		IBrowseState browseState = BrowseState;
		int height = 32;
		int width = (BaseViewController.IsADAMode ? 77 : 96);
		bool isDeselectable = !BaseViewController.IsADAMode;
		bool isVisible = ShowBottomAZButton();
		list.Add(new BrowseFilter
		{
			ButtonType = BrowseFilterType.AtoZ,
			Name = "abc_button",
			Height = height,
			Width = (BaseViewController.IsADAMode ? 56 : 93),
			IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.Alpha),
			IsDeselectable = isDeselectable,
			IsVisible = isVisible
		});
		bool isVisible2 = ShowBottomForSaleButton();
		list.Add(new BrowseFilter
		{
			ButtonType = BrowseFilterType.ForSale,
			Name = "for_sale_button",
			Height = height,
			Width = width,
			IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.ForSale),
			IsVisible = isVisible2
		});
		if (browseState.TitleFamily == TitleFamily.Movies)
		{
			bool isVisible3 = ShowPriceRangeFilterButtons();
			switch (configuration.PriceFiltersOnBrowseABTestVersion)
			{
			case "A":
			{
				if (BaseViewController.IsADAMode)
				{
					break;
				}
				BrowseFilter obj2 = new BrowseFilter
				{
					ButtonType = BrowseFilterType.PriceRange,
					Name = "price_range_low_button",
					Height = height,
					Width = 136
				};
				int isSelected2;
				if (browseState.BrowseSelectedSort == BrowseSelectedSort.ForSale)
				{
					BrowsePriceRangeParameter browsePriceRangeFilter2 = browseState.BrowsePriceRangeFilter;
					if (browsePriceRangeFilter2 == null)
					{
						isSelected2 = 0;
					}
					else
					{
						decimal? priceRangeHigh = browsePriceRangeFilter2.PriceRangeHigh;
						decimal num = 3.99m;
						isSelected2 = (((priceRangeHigh.GetValueOrDefault() == num) & priceRangeHigh.HasValue) ? 1 : 0);
					}
				}
				else
				{
					isSelected2 = 0;
				}
				obj2.IsSelected = (byte)isSelected2 != 0;
				obj2.IsVisible = isVisible3;
				obj2.Parameter = new BrowsePriceRangeParameter
				{
					PriceRangeHigh = 3.99m,
					analtyicsString = "buy399range"
				};
				obj2.IsDeselectable = isDeselectable;
				BrowseFilter browseFilter2 = obj2;
				browseFilter2.Text = string.Format(Resources.price_range_low_button, 3.99m.ToString("0.00"));
				list.Add(browseFilter2);
				BrowseFilter obj3 = new BrowseFilter
				{
					ButtonType = BrowseFilterType.PriceRange,
					Name = "price_range_mid_button",
					Height = height,
					Width = 127
				};
				int isSelected3;
				if (browseState.BrowseSelectedSort == BrowseSelectedSort.ForSale)
				{
					BrowsePriceRangeParameter browsePriceRangeFilter3 = browseState.BrowsePriceRangeFilter;
					if (browsePriceRangeFilter3 == null)
					{
						isSelected3 = 0;
					}
					else
					{
						decimal? priceRangeHigh = browsePriceRangeFilter3.PriceRangeHigh;
						decimal num = 5.99m;
						isSelected3 = (((priceRangeHigh.GetValueOrDefault() == num) & priceRangeHigh.HasValue) ? 1 : 0);
					}
				}
				else
				{
					isSelected3 = 0;
				}
				obj3.IsSelected = (byte)isSelected3 != 0;
				obj3.IsVisible = isVisible3;
				obj3.Parameter = new BrowsePriceRangeParameter
				{
					PriceRangeLow = 4.99m,
					PriceRangeHigh = 5.99m,
					analtyicsString = "buymidrange"
				};
				obj3.IsDeselectable = isDeselectable;
				browseFilter2 = obj3;
				browseFilter2.Text = string.Format(Resources.price_range_mid_button, 4.99m.ToString("0.00"), 5.99m.ToString("0.00"));
				list.Add(browseFilter2);
				BrowseFilter obj4 = new BrowseFilter
				{
					ButtonType = BrowseFilterType.PriceRange,
					Name = "price_range_high_button",
					Height = height,
					Width = width
				};
				int isSelected4;
				if (browseState.BrowseSelectedSort == BrowseSelectedSort.ForSale)
				{
					BrowsePriceRangeParameter browsePriceRangeFilter4 = browseState.BrowsePriceRangeFilter;
					if (browsePriceRangeFilter4 == null)
					{
						isSelected4 = 0;
					}
					else
					{
						decimal? priceRangeHigh = browsePriceRangeFilter4.PriceRangeLow;
						decimal num = 6.99m;
						isSelected4 = (((priceRangeHigh.GetValueOrDefault() == num) & priceRangeHigh.HasValue) ? 1 : 0);
					}
				}
				else
				{
					isSelected4 = 0;
				}
				obj4.IsSelected = (byte)isSelected4 != 0;
				obj4.IsVisible = isVisible3;
				obj4.Parameter = new BrowsePriceRangeParameter
				{
					PriceRangeLow = 6.99m,
					analtyicsString = "buy699range"
				};
				obj4.IsDeselectable = isDeselectable;
				browseFilter2 = obj4;
				browseFilter2.Text = string.Format(Resources.price_range_high_button, 6.99m.ToString("0.00"));
				list.Add(browseFilter2);
				break;
			}
			case "C":
			{
				BrowseFilter obj5 = new BrowseFilter
				{
					ButtonType = BrowseFilterType.PriceRange,
					Name = "price_cutoff_low_button",
					Height = height,
					Width = 136
				};
				int isSelected5;
				if (browseState.BrowseSelectedSort == BrowseSelectedSort.ForSale)
				{
					BrowsePriceRangeParameter browsePriceRangeFilter5 = browseState.BrowsePriceRangeFilter;
					if (browsePriceRangeFilter5 == null)
					{
						isSelected5 = 0;
					}
					else
					{
						decimal? priceRangeHigh = browsePriceRangeFilter5.PriceRangeHigh;
						decimal num = 3.99m;
						isSelected5 = (((priceRangeHigh.GetValueOrDefault() == num) & priceRangeHigh.HasValue) ? 1 : 0);
					}
				}
				else
				{
					isSelected5 = 0;
				}
				obj5.IsSelected = (byte)isSelected5 != 0;
				obj5.IsVisible = isVisible3;
				obj5.Parameter = new BrowsePriceRangeParameter
				{
					PriceRangeHigh = 3.99m,
					analtyicsString = "buy399"
				};
				BrowseFilter browseFilter3 = obj5;
				browseFilter3.Text = string.Format(Resources.price_range_low_button, 3.99m.ToString("0.00"));
				list.Add(browseFilter3);
				break;
			}
			case "D":
			{
				BrowseFilter obj = new BrowseFilter
				{
					ButtonType = BrowseFilterType.PriceRange,
					Name = "price_cutoff_high_button",
					Height = height,
					Width = 136
				};
				int isSelected;
				if (browseState.BrowseSelectedSort == BrowseSelectedSort.ForSale)
				{
					BrowsePriceRangeParameter browsePriceRangeFilter = browseState.BrowsePriceRangeFilter;
					if (browsePriceRangeFilter == null)
					{
						isSelected = 0;
					}
					else
					{
						decimal? priceRangeHigh = browsePriceRangeFilter.PriceRangeHigh;
						decimal num = 5.99m;
						isSelected = (((priceRangeHigh.GetValueOrDefault() == num) & priceRangeHigh.HasValue) ? 1 : 0);
					}
				}
				else
				{
					isSelected = 0;
				}
				obj.IsSelected = (byte)isSelected != 0;
				obj.IsVisible = isVisible3;
				obj.Parameter = new BrowsePriceRangeParameter
				{
					PriceRangeHigh = 5.99m,
					analtyicsString = "buy599"
				};
				BrowseFilter browseFilter = obj;
				browseFilter.Text = string.Format(Resources.price_range_low_button, 5.99m.ToString("0.00"));
				list.Add(browseFilter);
				break;
			}
			}
		}
		if (BaseViewController.IsADAMode && BrowseState.TitleFamily == TitleFamily.Movies)
		{
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Top20,
				Name = "top_20_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Top20, Genres.Top20Movies)
			});
		}
		if (BaseViewController.IsADAMode && BrowseState.TitleFamily == TitleFamily.Movies)
		{
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Action,
				Name = "action_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Action, Genres.ActionAndAdventure)
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Drama,
				Name = "drama_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Drama, Genres.Drama)
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Comedy,
				Name = "comedy_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Comedy, Genres.Comedy)
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Horror,
				Name = "horror_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Horror, Genres.Horror)
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Family,
				Name = "family_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Family, Genres.FamilyMovies)
			});
		}
		list.Add(new BrowseFilter
		{
			ButtonType = BrowseFilterType.ComingSoon,
			Name = "coming_soon_button",
			Height = height,
			Width = 135,
			IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.ComingSoon),
			IsVisible = ShouldShowComingSoon(),
			IsDeselectable = isDeselectable
		});
		list.Add(new BrowseFilter
		{
			ButtonType = BrowseFilterType._99Cents,
			Name = "_99_cents_filter_button",
			Height = height,
			Width = width,
			IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort._99Cents),
			Parameter = new BrowsePriceRangeParameter
			{
				PriceRangeLow = 0.99m,
				PriceRangeHigh = 0.99m
			},
			IsVisible = ShouldShow99Cents(),
			IsDeselectable = isDeselectable
		});
		list.Add(new BrowseFilter
		{
			ButtonType = BrowseFilterType.SonySweepstakes,
			Name = "sony_sweepstakes_filter_button",
			Height = height,
			Width = width,
			IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.SonySweepstakes),
			IsVisible = ShouldShowSonySweepstakes(),
			IsDeselectable = isDeselectable
		});
		list.Add(new BrowseFilter
		{
			ButtonType = BrowseFilterType.Deals,
			Name = "deals_button",
			Height = height,
			Width = 93,
			IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.Deals),
			IsVisible = ShouldShowDeals(),
			IsDeselectable = isDeselectable
		});
		foreach (BrowseFilter item in list)
		{
			if (item.IsSelected && item.IsDeselectable)
			{
				item.IsButtonEnabled = true;
			}
			if (item.ButtonType != BrowseFilterType.PriceRange)
			{
				item.Text = Resources.ResourceManager.GetString(item.Name).ToUpper();
			}
		}
		return list;
	}

	private static bool IsGenreButtonSelected(BrowseSelectedSort genreSort, Genres genre)
	{
		if (BrowseState.BrowseSelectedSort != genreSort)
		{
			if (BrowseState.BrowseSelectedSort != BrowseSelectedSort.Alpha && BrowseState.BrowseSelectedSort != BrowseSelectedSort.ForSale && BaseViewController.IsADAMode)
			{
				return BrowseState.Genre == genre;
			}
			return false;
		}
		return true;
	}

	private static string GetRentalPriceText(ITitleProduct titleProduct)
	{
		List<ITitleProduct> list = ((titleProduct is ITitleRollupProduct) ? ((ITitleRollupProduct)titleProduct).KioskProducts : new List<ITitleProduct> { titleProduct }).Where((ITitleProduct tp) => tp.IsRentableAndInStock).ToList();
		if (list.Count == 0)
		{
			return string.Empty;
		}
		decimal? price = list.Min((ITitleProduct tp) => (ServiceLocator.Instance.GetService<IPricingService>()?.GetOneDayProductPricingRecord(tp.ProductId))?.InitialNight);
		bool flag = list.Count == 1 || list.All(delegate(ITitleProduct tp)
		{
			IPricingRecord obj = ServiceLocator.Instance.GetService<IPricingService>()?.GetOneDayProductPricingRecord(tp.ProductId);
			if (obj == null)
			{
				return !price.HasValue;
			}
			decimal initialNight = obj.InitialNight;
			decimal? num = price;
			return (initialNight == num.GetValueOrDefault()) & num.HasValue;
		});
		return string.Format("${0:F2}{1}", price, flag ? "" : "+");
	}

	private static void ConfigureBrowseMessagePopupUserControl(BrowseViewModel browseViewModel)
	{
		if (browseViewModel == null || !BrowseState.EnableBrowseMessagePopup)
		{
			return;
		}
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (((!BrowseState.ShowBrowseMessagePopup || BrowseState.PageNumber < 2) && !BrowseState.ShowingBrowseMessagePopup) || rentalShoppingCart == null || rentalShoppingCart.Count != 0)
		{
			return;
		}
		if (BrowseState.ShowBrowseMessagePopup)
		{
			BaseViewController.AnalyticsService?.AddViewEvent("View", "browse_message_popup_view");
		}
		BrowseMessagePopupModel browseMessagePopupModel = new BrowseMessagePopupModel();
		browseMessagePopupModel.Text1 = Resources.browse_message_popup_text1;
		browseMessagePopupModel.Text2 = Resources.browse_message_popup_text2;
		browseMessagePopupModel.InfoButtonText = Resources.browse_message_popup_text3;
		browseMessagePopupModel.OnBrowseMessagePopupInfoClicked = (Action)Delegate.Combine(browseMessagePopupModel.OnBrowseMessagePopupInfoClicked, (Action)delegate
		{
			OnBrowseMessagePopupInfoClicked();
		});
		browseMessagePopupModel.OnBrowseMessagePopupCloseClicked = (Action)Delegate.Combine(browseMessagePopupModel.OnBrowseMessagePopupCloseClicked, (Action)delegate
		{
			OnBrowseMessagePopupCloseClicked();
		});
		browseMessagePopupModel.BrowseMessagePopupUserControlVisibility = Visibility.Visible;
		browseViewModel.BrowseMessagePopupModel = browseMessagePopupModel;
		if (TimerService?.GetTimer("browse_message_popup") == null)
		{
			(TimerService?.CreateTimer("browse_message_popup", BrowseState.BrowseMessagePopupDuration, BrowseState.BrowseMessagePopupDuration, delegate
			{
				BaseViewController.AnalyticsService?.AddTimeoutEvent("browse_message_popup_view", BrowseState.BrowseMessagePopupDuration);
				CloseBrowseMessagePopupUserControl();
			}))?.Start();
		}
		BrowseState.ShowBrowseMessagePopup = false;
		BrowseState.ShowingBrowseMessagePopup = true;
	}

	private static void CloseBrowseMessagePopupUserControl()
	{
		(TimerService?.GetTimer("browse_message_popup"))?.Stop();
		TimerService?.RemoveTimer("browse_message_popup");
		BrowseViewModel viewModel = BaseViewController.GetViewModel<BrowseViewModel>();
		if (viewModel != null && viewModel.BrowseMessagePopupModel != null)
		{
			viewModel.BrowseMessagePopupModel.BrowseMessagePopupUserControlVisibility = Visibility.Collapsed;
		}
		BrowseState.ShowBrowseMessagePopup = false;
		BrowseState.ShowingBrowseMessagePopup = false;
	}

	private static void ConfigureBrowseRedboxPlusTooltipUserControl(BrowseViewModel browseViewModel)
	{
		BrowseRedboxPlusTooltipModel browseRedboxPlusTooltipModel = new BrowseRedboxPlusTooltipModel();
		browseRedboxPlusTooltipModel.BrowseRedboxPlusTooltipUserControlVisibility = ((!RedboxPlusFunctions.CanShowRedboxPlusRedemption || !BrowseState.ShowRedboxPlusTooltip || OrchestrationService.TTSEnabled) ? Visibility.Collapsed : Visibility.Visible);
		browseRedboxPlusTooltipModel.Text = Resources.redbox_plus_tooltip_text;
		browseRedboxPlusTooltipModel.BrowseRedboxPlusTooltipUserControlMargin = (ShowBottomRedboxPlusMoviesButton() ? new Thickness(118.0, 0.0, 0.0, 186.0) : new Thickness(28.0, 0.0, 0.0, 138.0));
		browseRedboxPlusTooltipModel.OnBrowseRedboxPlusTooltipUserControlClicked = (Action<BrowseRedboxPlusTooltipModel>)Delegate.Combine(browseRedboxPlusTooltipModel.OnBrowseRedboxPlusTooltipUserControlClicked, new Action<BrowseRedboxPlusTooltipModel>(OnBrowseRedboxPlusTooltipUserControlClicked));
		if (BrowseState.ShowRedboxPlusTooltip)
		{
			InputManager inputManager = InputManager.Current;
			if (inputManager != null)
			{
				NotifiyInputEventHandler = ProcessInputManagerPreNotifyInput;
				inputManager.PreNotifyInput += NotifiyInputEventHandler;
				RemovePreNotifyInputHandler = delegate
				{
					if (NotifiyInputEventHandler != null)
					{
						inputManager.PreNotifyInput -= NotifiyInputEventHandler;
						NotifiyInputEventHandler = null;
					}
				};
			}
		}
		browseViewModel.BrowseRedboxPlusTooltipModel = browseRedboxPlusTooltipModel;
	}

	private static void ProcessInputManagerPreNotifyInput(object sender, NotifyInputEventArgs e)
	{
		if (e?.StagingItem?.Input?.RoutedEvent == Mouse.PreviewMouseDownEvent && e?.StagingItem?.Input is MouseEventArgs { LeftButton: MouseButtonState.Pressed } && OrchestrationService.PeekView() == "browse_view" && NotifiyInputEventHandler != null)
		{
			BrowseViewModel viewModel = BaseViewController.GetViewModel<BrowseViewModel>();
			if (viewModel?.BrowseRedboxPlusTooltipModel != null)
			{
				HideRedboxPlusToolTip(viewModel.BrowseRedboxPlusTooltipModel);
			}
		}
	}

	private static void HideRedboxPlusToolTip(BrowseRedboxPlusTooltipModel browseRedboxPlusTooltipModel)
	{
		BrowseState.ShowRedboxPlusTooltip = false;
		browseRedboxPlusTooltipModel.BrowseRedboxPlusTooltipUserControlVisibility = Visibility.Collapsed;
		RemovePreNotifyInputHandler?.Invoke();
	}

	private static void OnBrowseMessagePopupInfoClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseBrowseMessagePopupInfo");
		MessagePopupViewController.ShowBrowseMessagePopupLearnMorePopup(delegate
		{
			OrchestrationService.ViewGoBackSafe("OnBrowseMessagePopupInfoClicked timeOutAction", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("OnBrowseMessagePopupInfoClicked continueAction", popOnly: false);
		});
	}

	private static void OnBrowseMessagePopupCloseClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseBrowseMessagePopupClose");
		CloseBrowseMessagePopupUserControl();
	}

	private static void OnBrowseRedboxPlusTooltipUserControlClicked(BrowseRedboxPlusTooltipModel browseRedboxPlusTooltipModel)
	{
		LogHelper.Instance.Log("Closing Redbox+ tooltip because user clicked on it.");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("BrowseRedboxPlusTooltip");
		HideRedboxPlusToolTip(browseRedboxPlusTooltipModel);
	}
}
