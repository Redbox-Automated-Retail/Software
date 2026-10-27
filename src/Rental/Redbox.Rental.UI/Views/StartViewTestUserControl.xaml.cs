using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public partial class StartViewTestUserControl : TextToSpeechUserControl, IComponentConnector
{
	private StartViewModel StartViewModel => base.DataContext as StartViewModel;

	public StartViewTestUserControl()
	{
		InitializeComponent();
		ApplyTheme();
		PostHealthActivity();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(rent_movies_button);
		obj?.SetStyle(buy_movies_button);
		obj?.SetStyle(return_button);
		obj?.SetStyle(pick_up_button);
		obj?.SetStyle(handicap_button);
		obj?.SetStyle(espanol_button);
		obj?.SetStyle(help_button);
	}

	private void RentOrBuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessRentMovieButtonClick();
		HandleWPFHit();
	}

	private void RentOrBuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = rent_movies_button.Visibility == Visibility.Visible;
	}

	private void ToggleLanguageModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessToggleLanguage();
		HandleWPFHit();
	}

	private void ToggleLanguageModeCommandBingind_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void BuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = buy_movies_button.Visibility == Visibility.Visible;
	}

	private void BuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessBuyMoviesButtonClick();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void PickupProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessPickupButtonClick();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void ReturnProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessReturnButtonClicked();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessHelpButtonClicked();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void ToggleADAModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessToggleADAMode();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void ExitBurnInViewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessExitBurnInView();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void BannerClickCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel.ProcessBannerClicked();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessSignInButtonClicked();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}
}
