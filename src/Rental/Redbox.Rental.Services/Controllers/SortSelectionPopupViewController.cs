using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class SortSelectionPopupViewController : BaseViewController
{
	private static SortButtonModel _selectedSortButtonModel;

	public static void ConfigureSortSelectionPopupView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			CreateAndStartTimer();
			_selectedSortButtonModel = null;
			OrchestrationService.SetCurrentUICulture();
			SortSelectionPopupViewModel sortSelectionPopupViewModel = new SortSelectionPopupViewModel();
			sortSelectionPopupViewModel.HeaderText = Resources.sort_selection_popup_header;
			sortSelectionPopupViewModel.DescriptionText = Resources.sort_selection_popup_description;
			sortSelectionPopupViewModel.CancelButtonText = Resources.cancel_button.ToUpper();
			sortSelectionPopupViewModel.ApplyButtonText = Resources.apply_button.ToUpper();
			List<SortButtonModel> sortButtonModels = new List<SortButtonModel>
			{
				CreateSortButtonModel(BrowseSort.SortDate),
				CreateSortButtonModel(BrowseSort.MerchandiseDateDescending),
				CreateSortButtonModel(BrowseSort.Alpha),
				CreateSortButtonModel(BrowseSort.InitialNightPrice)
			};
			sortSelectionPopupViewModel.SortButtonModels = sortButtonModels;
			sortSelectionPopupViewModel.OnCancelButtonClicked += SortSelectionPopupViewModel_OnCancelButtonClicked;
			sortSelectionPopupViewModel.OnApplyButtonClicked += SortSelectionPopupViewModel_OnApplyButtonClicked;
			viewUserControl.DataContext = sortSelectionPopupViewModel;
		}
	}

	public static void LeaveSortSelectionPopupView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	private static void CreateAndStartTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		int timeOut = 20000;
		IIdleTimer obj = service?.CreateIdleTimer("SortSelectionPopupViewTimer");
		obj?.Initialize(timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("SortSelectionPopupViewController time out", popOnly: false);
		});
		obj?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		(service?.GetIdleTimer("SortSelectionPopupViewTimer"))?.Stop();
		service?.RemoveIdleTimer("SortSelectionPopupViewTimer");
	}

	private static SortButtonModel CreateSortButtonModel(BrowseSort sort)
	{
		bool flag = BrowseViewController.GetBrowseSort() == sort;
		SortButtonModel sortButtonModel = new SortButtonModel(WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "button_selectable_unselected_style"), WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "button_selectable_selected_style"))
		{
			Sort = sort,
			Visibility = Visibility.Visible,
			IsSelected = flag,
			Margin = new Thickness(8.0, 8.0, 8.0, 8.0),
			Text = GetSortSelectionPopupButtonText(sort)
		};
		sortButtonModel.OnClicked += SortButtonModel_OnClicked;
		if (flag)
		{
			_selectedSortButtonModel = sortButtonModel;
		}
		return sortButtonModel;
	}

	private static string GetSortSelectionPopupButtonText(BrowseSort sort)
	{
		return sort switch
		{
			BrowseSort.Alpha => Resources.sort_alphabetical, 
			BrowseSort.MerchandiseDate => Resources.sort_old_arrivals, 
			BrowseSort.SortDate => Resources.sort_trending.ToUpper(), 
			BrowseSort.InitialNightPrice => Resources.sort_price_low_high, 
			BrowseSort.MerchandiseDateDescending => Resources.sort_new_arrivals, 
			_ => Resources.sort_selection_popup_button_text, 
		};
	}

	private static void SortButtonModel_OnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		BaseViewController.ProcessWPFHit();
		if (checkMarkButtonModel is SortButtonModel sortButtonModel)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("SortSelectionPopupSort", sortButtonModel.Sort.ToString());
			if (_selectedSortButtonModel != null)
			{
				_selectedSortButtonModel.IsSelected = false;
			}
			sortButtonModel.IsSelected = true;
			_selectedSortButtonModel = sortButtonModel;
		}
	}

	private static void SortSelectionPopupViewModel_OnCancelButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SortSelectionPopupCancel");
		OrchestrationService.ViewGoBackSafe("SortSelectionPopupViewController cancel", popOnly: false);
	}

	private static void SortSelectionPopupViewModel_OnApplyButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SortSelectionPopupApply");
		if (_selectedSortButtonModel != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.Sort = _selectedSortButtonModel.Sort;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.PageNumber = 1;
		}
		OrchestrationService.ViewGoBackSafe("SortSelectionPopupViewController apply", popOnly: false);
	}
}
