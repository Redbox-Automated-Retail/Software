using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class GenreSelectionPopupViewController : BaseViewController
{
	private static GenreButtonModel _selectedGenreButtonModel;

	public static void ConfigureGenreSelectionPopupView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			CreateAndStartTimer();
			_selectedGenreButtonModel = null;
			OrchestrationService.SetCurrentUICulture();
			GenreSelectionPopupViewModel genreSelectionPopupViewModel = new GenreSelectionPopupViewModel();
			genreSelectionPopupViewModel.HeaderText = Resources.genre_selection_popup_header;
			genreSelectionPopupViewModel.DescriptionText = Resources.genre_selection_popup_description;
			genreSelectionPopupViewModel.CancelButtonText = Resources.cancel_button.ToUpper();
			genreSelectionPopupViewModel.ApplyButtonText = Resources.apply_button.ToUpper();
			List<GenreButtonModel> genreButtonModels = new List<GenreButtonModel>
			{
				CreateGenreButtonModel(Genres.ActionAndAdventure),
				CreateGenreButtonModel(Genres.Comedy),
				CreateGenreButtonModel(Genres.Drama),
				CreateGenreButtonModel(Genres.FamilyMovies),
				CreateGenreButtonModel(Genres.Horror),
				CreateGenreButtonModel(Genres.Kids),
				CreateGenreButtonModel(Genres.Romance),
				CreateGenreButtonModel(Genres.SciFiAndFantasy),
				CreateGenreButtonModel(Genres.Suspense),
				CreateGenreButtonModel(Genres.War),
				CreateGenreButtonModel(Genres.Western)
			};
			genreSelectionPopupViewModel.GenreButtonModels = genreButtonModels;
			genreSelectionPopupViewModel.OnCancelButtonClicked += GenreSelectionPopupViewModel_OnCancelButtonClicked;
			genreSelectionPopupViewModel.OnApplyButtonClicked += GenreSelectionPopupViewModel_OnApplyButtonClicked;
			viewUserControl.DataContext = genreSelectionPopupViewModel;
		}
	}

	public static void LeaveGenreSelectionPopupView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	private static void CreateAndStartTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		int timeOut = 20000;
		IIdleTimer obj = service?.CreateIdleTimer("GenreSelectionPopupViewTimer");
		obj?.Initialize(timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("GenreSelectionPopupViewController time out", popOnly: false);
		});
		obj?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		(service?.GetIdleTimer("GenreSelectionPopupViewTimer"))?.Stop();
		service?.RemoveIdleTimer("GenreSelectionPopupViewTimer");
	}

	private static GenreButtonModel CreateGenreButtonModel(Genres genre)
	{
		bool flag = Redbox.Rental.Services.Session.Session.Instance.BrowseState.Genre.HasValue && Redbox.Rental.Services.Session.Session.Instance.BrowseState.Genre == genre;
		GenreButtonModel genreButtonModel = new GenreButtonModel(WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "button_selectable_unselected_style"), WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "button_selectable_selected_style"))
		{
			Genre = genre,
			Visibility = Visibility.Visible,
			IsSelected = flag,
			Margin = new Thickness(8.0, 8.0, 8.0, 8.0),
			Text = Resources.ResourceManager.GetString(string.Format($"genre_{(long)genre}"))
		};
		genreButtonModel.OnClicked += GenreButtonModel_OnClicked;
		if (flag)
		{
			_selectedGenreButtonModel = genreButtonModel;
		}
		return genreButtonModel;
	}

	private static void GenreButtonModel_OnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		BaseViewController.ProcessWPFHit();
		if (checkMarkButtonModel is GenreButtonModel genreButtonModel)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("GenreSelectionPopupGenre", genreButtonModel.Genre.ToString());
			if (_selectedGenreButtonModel != null)
			{
				_selectedGenreButtonModel.IsSelected = false;
			}
			genreButtonModel.IsSelected = true;
			_selectedGenreButtonModel = genreButtonModel;
		}
	}

	private static void GenreSelectionPopupViewModel_OnCancelButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("GenreSelectionPopupCancel");
		OrchestrationService.ViewGoBackSafe("GenreSelectionPopupViewController cancel", popOnly: false);
	}

	private static void GenreSelectionPopupViewModel_OnApplyButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("GenreSelectionPopupApply");
		if (_selectedGenreButtonModel != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.Genre = _selectedGenreButtonModel.Genre;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.PageNumber = 1;
		}
		OrchestrationService.ViewGoBackSafe("GenreSelectionPopupViewController apply", popOnly: false);
	}
}
