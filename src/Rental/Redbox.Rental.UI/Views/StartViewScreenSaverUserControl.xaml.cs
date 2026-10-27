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

public partial class StartViewScreenSaverUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private StartViewModel StartViewModel => base.DataContext as StartViewModel;

	public StartViewScreenSaverUserControl()
	{
		InitializeComponent();
		ApplyTheme();
		RegisterRoutedCommand("BannerClickCommand", Redbox.Rental.UI.Commands.BannerClickCommand);
		RegisterRoutedCommand("RentOrBuyMoviesCommand", Redbox.Rental.UI.Commands.RentOrBuyMoviesCommand);
		RegisterRoutedCommand("BuyMoviesCommand", Redbox.Rental.UI.Commands.BuyMoviesCommand);
		RegisterRoutedCommand("ReturnProductCommand", Redbox.Rental.UI.Commands.ReturnProductCommand);
		RegisterRoutedCommand("PickupProductCommand", Redbox.Rental.UI.Commands.PickupProductCommand);
		RegisterRoutedCommand("ToggleADAModeCommand", Redbox.Rental.UI.Commands.ToggleADAModeCommand);
		RegisterRoutedCommand("SignInCommand", Redbox.Rental.UI.Commands.SignInCommand);
		RegisterRoutedCommand("ToggleLanguageModeCommand", Redbox.Rental.UI.Commands.ToggleLanguageModeCommand);
		RegisterRoutedCommand("HelpCommand", Redbox.Rental.UI.Commands.HelpCommand);
		RegisterRoutedCommand("ExitBurnInViewCommand", Redbox.Rental.UI.Commands.ExitBurnInViewCommand);
		PostHealthActivity();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(all_movies_button);
		obj?.SetStyle(buy_movies_button);
		obj?.SetStyle(online_pickup_button);
		obj?.SetStyle(return_button);
		obj?.SetStyle(ada_button);
		obj?.SetStyle(sign_in_button);
		obj?.SetStyle(espanol_button);
		obj?.SetStyle(help_button);
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

	private void RentOrBuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = all_movies_button.Visibility == Visibility.Visible;
	}

	private void RentOrBuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			StartViewModel?.ProcessRentMovieButtonClick();
			HandleWPFHit();
		});
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

	private void ToggleLanguageModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessToggleLanguage();
		HandleWPFHit();
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
}
