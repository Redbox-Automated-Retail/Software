using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class RatingSelectionPopupViewController : BaseViewController
{
	private static RatingButtonModel _selectedRatingButtonModel;

	public static void ConfigureRatingSelectionPopupView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			CreateAndStartTimer();
			_selectedRatingButtonModel = null;
			OrchestrationService.SetCurrentUICulture();
			RatingSelectionPopupViewModel ratingSelectionPopupViewModel = new RatingSelectionPopupViewModel();
			ratingSelectionPopupViewModel.HeaderText = Resources.rating_selection_popup_header;
			ratingSelectionPopupViewModel.DescriptionText = Resources.rating_selection_popup_description;
			ratingSelectionPopupViewModel.CancelButtonText = Resources.cancel_button.ToUpper();
			ratingSelectionPopupViewModel.ApplyButtonText = Resources.apply_button.ToUpper();
			List<RatingButtonModel> ratingButtonModels = new List<RatingButtonModel>
			{
				CreateRatingButtonModel(Ratings.G),
				CreateRatingButtonModel(Ratings.PG),
				CreateRatingButtonModel(Ratings.PG13),
				CreateRatingButtonModel(Ratings.R),
				CreateRatingButtonModel(Ratings.NR)
			};
			ratingSelectionPopupViewModel.RatingButtonModels = ratingButtonModels;
			ratingSelectionPopupViewModel.OnCancelButtonClicked += RatingSelectionPopupViewModel_OnCancelButtonClicked;
			ratingSelectionPopupViewModel.OnApplyButtonClicked += RatingSelectionPopupViewModel_OnApplyButtonClicked;
			viewUserControl.DataContext = ratingSelectionPopupViewModel;
		}
	}

	public static void LeaveRatingSelectionPopupView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	private static void CreateAndStartTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		int timeOut = 20000;
		IIdleTimer obj = service?.CreateIdleTimer("RatingSelectionPopupViewTimer");
		obj?.Initialize(timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("RatingSelectionPopupViewController time out", popOnly: false);
		});
		obj?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		(service?.GetIdleTimer("RatingSelectionPopupViewTimer"))?.Stop();
		service?.RemoveIdleTimer("RatingSelectionPopupViewTimer");
	}

	private static RatingButtonModel CreateRatingButtonModel(Ratings rating)
	{
		bool flag = Redbox.Rental.Services.Session.Session.Instance.BrowseState.Rating.HasValue && Redbox.Rental.Services.Session.Session.Instance.BrowseState.Rating == rating;
		RatingsProperties.Properties.TryGetValue(rating, out var value);
		RatingButtonModel ratingButtonModel = new RatingButtonModel(WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "button_selectable_unselected_style"), WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "button_selectable_selected_style"))
		{
			Rating = rating,
			Visibility = Visibility.Visible,
			IsSelected = flag,
			Margin = new Thickness(8.0, 8.0, 8.0, 8.0),
			Text = (value?.Name ?? "None")
		};
		ratingButtonModel.OnClicked += RatingButtonModel_OnClicked;
		if (flag)
		{
			_selectedRatingButtonModel = ratingButtonModel;
		}
		return ratingButtonModel;
	}

	private static void RatingButtonModel_OnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		BaseViewController.ProcessWPFHit();
		if (checkMarkButtonModel is RatingButtonModel ratingButtonModel)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("RatingSelectionPopupRating", ratingButtonModel.Rating.ToString());
			if (_selectedRatingButtonModel != null)
			{
				_selectedRatingButtonModel.IsSelected = false;
			}
			ratingButtonModel.IsSelected = true;
			_selectedRatingButtonModel = ratingButtonModel;
		}
	}

	private static void RatingSelectionPopupViewModel_OnCancelButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RatingSelectionPopupCancel");
		OrchestrationService.ViewGoBackSafe("RatingSelectionPopupViewController cancel", popOnly: false);
	}

	private static void RatingSelectionPopupViewModel_OnApplyButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RatingSelectionPopupApply");
		if (_selectedRatingButtonModel != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.Rating = _selectedRatingButtonModel.Rating;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.PageNumber = 1;
		}
		OrchestrationService.ViewGoBackSafe("RatingSelectionPopupViewController apply", popOnly: false);
	}
}
