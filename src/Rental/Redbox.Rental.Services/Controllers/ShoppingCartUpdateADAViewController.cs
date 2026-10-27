using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ShoppingCartUpdateADAViewController : BaseViewController
{
	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.StopIdleTimer("idle_timer");
			ShoppingCartUpdateADAViewModel shoppingCartUpdateADAViewModel = new ShoppingCartUpdateADAViewModel();
			ConfigureTextAndVisibility(shoppingCartUpdateADAViewModel);
			ConfigureCommands(shoppingCartUpdateADAViewModel);
			ConfigureShoppingCartDisplay(shoppingCartUpdateADAViewModel);
			ConfigureTTS(shoppingCartUpdateADAViewModel);
			viewUserControl.DataContext = shoppingCartUpdateADAViewModel;
			CommandManager.InvalidateRequerySuggested();
			OrchestrationService.StartIdleTimer(OrchestrationService.GetIdleTimerTimeout(), IdleTimeOut, "shopping_cart_update_ada_idle_timer");
		}
	}

	private static void ConfigureTTS(ShoppingCartUpdateADAViewModel shoppingCartUpdateADAViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			shoppingCartUpdateADAViewModel.ProcessGetSpeechControl = () => ShoppingCartUpdateADAViewTextToSpeech.GetSpeechControl(GetShoppingCartViewModelFromView);
		}
	}

	private static ShoppingCartUpdateADAViewModel GetShoppingCartViewModelFromView()
	{
		FrameworkElement shoppingCartViewUserControl = BaseViewController.GetViewUserControl();
		if (shoppingCartViewUserControl != null)
		{
			return shoppingCartViewUserControl.Dispatcher.Invoke(() => shoppingCartViewUserControl?.DataContext as ShoppingCartUpdateADAViewModel);
		}
		return null;
	}

	private static void ConfigureShoppingCartDisplay(ShoppingCartUpdateADAViewModel shoppingCartUpdateADAViewModel)
	{
		shoppingCartUpdateADAViewModel.DisplayProductModels = CreateDisplayProductModelsFromCartItems();
	}

	private static List<DisplayProductModel> CreateDisplayProductModelsFromCartItems()
	{
		List<DisplayProductModel> list = new List<DisplayProductModel>();
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (rentalShoppingCart != null && service != null)
		{
			foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
			{
				service.GetProduct(titleItem.ProductId, excludeTitleRollup: true, out var kioskProduct);
				if (kioskProduct != null)
				{
					DisplayProductModel displayProductModel = new DisplayProductModel
					{
						Name = kioskProduct.ProductName,
						Data = titleItem,
						Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
					};
					displayProductModel.ConfigureForUpdateADACart();
					ITitleProduct titleProduct = kioskProduct as ITitleProduct;
					BannerType productBannerType = BrowseFunctions.GetProductBannerType(titleProduct);
					BrowseFunctions.ConfigureBanner(displayProductModel, titleProduct, hideBanner: false, productBannerType, BaseViewController.GetViewUserControl());
					list.Add(displayProductModel);
				}
			}
		}
		return list;
	}

	private static void ConfigureTextAndVisibility(ShoppingCartUpdateADAViewModel shoppingCartUpdateADAViewModel)
	{
		OrchestrationService.SetCurrentUICulture();
		shoppingCartUpdateADAViewModel.Title = Resources.update_your_bag;
		shoppingCartUpdateADAViewModel.Message = Resources.this_is_your_chance;
		shoppingCartUpdateADAViewModel.OkButtonText = Resources.okay;
		shoppingCartUpdateADAViewModel.AddMovieButtonText = Resources.add_movie_button_text;
		IRentalShoppingCartService rentalShoppingCartService = BaseViewController.RentalShoppingCartService;
		shoppingCartUpdateADAViewModel.AddMovieButtonVisibility = ((rentalShoppingCartService == null || rentalShoppingCartService.CurrentRentalShoppingCart.IsFull) ? Visibility.Collapsed : Visibility.Visible);
	}

	private static void IdleTimeOut()
	{
		OrchestrationService.ViewGoBack("shopping_cart_update_ada_view", popOnly: false);
	}

	public static void LeaveView()
	{
		OrchestrationService.RemoveIdleTimer("shopping_cart_update_ada_idle_timer");
		OrchestrationService.StartIdleTimer("idle_timer");
	}

	private static void ConfigureCommands(ShoppingCartUpdateADAViewModel shoppingCartUpdateADAViewModel)
	{
		if (shoppingCartUpdateADAViewModel != null)
		{
			shoppingCartUpdateADAViewModel.OnOkButtonPress = ProcessGoBackCommand;
			shoppingCartUpdateADAViewModel.OnAddMovieButtonPress = ShowBrowseViewMovies;
			shoppingCartUpdateADAViewModel.OnCancelBrowseItemModel = RemoveItemFromShoppingCart;
		}
	}

	private static void RemoveItemFromShoppingCart(DisplayProductModel displayProductModel, object parameter)
	{
		BaseViewController.ProcessWPFHit();
		if (displayProductModel?.Data is IRentalShoppingCartTitleItem rentalShoppingCartItem)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartUpdateADADelete").AddData(ProductData.ToProductData(rentalShoppingCartItem));
			ShoppingCartViewFunctions.RemoveItemFromShoppingCart(displayProductModel, parameter);
		}
	}

	private static void ShowBrowseViewMovies()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartUpdateADAAddMovie");
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.SetDefaultBrowseStateSet();
		BaseViewController.GetViewService()?.UnwindToSafe("browse_view", discard: true, null);
	}

	private static void ProcessGoBackCommand()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartUpdateADAOkay");
		BaseViewController.ProcessWPFHit();
		OrchestrationService.ViewGoBack("shopping_cart_update_ada_view", popOnly: false);
	}
}
