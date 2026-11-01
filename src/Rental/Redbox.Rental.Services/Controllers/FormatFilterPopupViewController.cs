using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class FormatFilterPopupViewController : BaseViewController
{
	private static bool IsDigitalCodeButtonEnabled
	{
		get
		{
			IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
			List<IKioskProduct> products = null;
			dataService?.GetProducts(BrowseViewFilter.ForSale, TitleFamily.Movies, TitleType.DigitalCode, BrowseSort.MerchandiseDate, null, null, null, null, isADAView: false, null, null, out products);
			if (products != null)
			{
				return products.Count > 0;
			}
			return false;
		}
	}

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	public static void ConfigureFormatFilterPopupView()
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			FormatSelectionPopupViewModel formatSelectionPopupViewModel = GetViewModel();
			if (formatSelectionPopupViewModel == null)
			{
				formatSelectionPopupViewModel = new FormatSelectionPopupViewModel();
			}
			formatSelectionPopupViewModel.HeaderText = Resources.format_filter_popup_text;
			formatSelectionPopupViewModel.CancelButtonText = Resources.cancel_button.ToUpper();
			formatSelectionPopupViewModel.OnCancelButtonClicked = delegate
			{
				ProcessButtonClick(TitleType.All, "cancel");
			};
			Style style = WPFFunctions.GetStyle(viewUserControl, "button_light_large_primary_style");
			int visibleFormatCount = 0;
			if (IsTitleTypeAvailable(TitleType.DVD))
			{
				formatSelectionPopupViewModel.ButtonData.Add(new FormatButtonData(Resources.dvd, TitleType.DVD, style));
				visibleFormatCount++;
			}
			if (IsTitleTypeAvailable(TitleType.Bluray))
			{
				formatSelectionPopupViewModel.ButtonData.Add(new FormatButtonData(Resources.blu_ray.ToUpper(), TitleType.Bluray, style));
				visibleFormatCount++;
			}
			if (IsTitleTypeAvailable(TitleType._4KUHD))
			{
				formatSelectionPopupViewModel.ButtonData.Add(new FormatButtonData(Resources.four_k_uhd, TitleType._4KUHD, style));
				visibleFormatCount++;
			}
			if (IsDigitalCodeButtonEnabled)
			{
				formatSelectionPopupViewModel.ButtonData.Add(new FormatButtonData(Resources.digital_code.ToUpper(), TitleType.DigitalCode, style));
				visibleFormatCount++;
			}
			formatSelectionPopupViewModel.OnFormatButtonClicked = delegate(TitleType type)
			{
				ProcessButtonClick(type, type.GetDisplayName());
			};
			if (visibleFormatCount == 3)
			{
				formatSelectionPopupViewModel.DialogWidth = 900;
			}
			else
			{
				formatSelectionPopupViewModel.DialogWidth = 750;
			}
			ConfigureTTS(formatSelectionPopupViewModel);
			viewUserControl.DataContext = formatSelectionPopupViewModel;
		}
	}

	private static void ConfigureTTS(FormatSelectionPopupViewModel viewModel)
	{
		ITextToSpeechService textToSpeechService = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (textToSpeechService != null && textToSpeechService.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => FormatFilterPopupViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	private static FormatSelectionPopupViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as FormatSelectionPopupViewModel;
		}
		return null;
	}

	private static bool IsTitleTypeAvailable(TitleType titleType)
	{
		IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
		List<IKioskProduct> products = null;
		dataService?.GetProducts(BrowseViewFilter.All, TitleFamily.Movies, titleType, BrowseSort.MerchandiseDate, null, null, null, null, isADAView: false, null, null, out products);
		if (products != null)
		{
			return products.Count > 0;
		}
		return false;
	}

	private static void ProcessButtonClick(TitleType filterType, string analysisText)
	{
		if (!BaseViewController.IsADAMode && filterType == TitleType.All)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("FormatFilterPopupFilter", "Cancel");
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("FormatFilterPopupFilter", filterType.ToString());
			BrowseState.TitleType = filterType;
			BrowseState.BrowseSelectedSort = TitleTypeToSelectedSort(filterType);
			BrowseState.BrowseMode = BrowseFunctions.GetBrowseMode(filterType);
			BrowseState.PageNumber = 1;
			BrowseState.BrowsePriceRangeFilter = null;
			BrowseState.BrowseStudio = null;
		}
		OrchestrationService.ViewGoBackSafe("FormatFilterPopupViewController " + analysisText, popOnly: false);
	}

	private static BrowseSelectedSort TitleTypeToSelectedSort(TitleType filterType)
	{
		return filterType switch
		{
			TitleType.Bluray => BrowseSelectedSort.Bluray, 
			TitleType.DVD => BrowseSelectedSort.Dvd, 
			TitleType.DigitalCode => BrowseSelectedSort.DigitalCode, 
			TitleType._4KUHD => BrowseSelectedSort._4KUHD, 
			_ => BrowseSelectedSort.Newest, 
		};
	}

	private static void CreateAndStartTimer()
	{
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = OrchestrationService.TTSGetTimeout("medium", 15000);
		ServiceLocator.Instance.GetService<IViewService>()?.GetCurrentViewAnalyticsName();
		(timerService?.CreateTimer("FormatFilterPopupViewTimer", timeOut, timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("FormatFilterPopupViewController time out", popOnly: false);
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		(timerService?.GetTimer("FormatFilterPopupViewTimer"))?.Stop();
		timerService?.RemoveTimer("FormatFilterPopupViewTimer");
	}

	public static void LeaveFormatFilterPopupView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}
}
