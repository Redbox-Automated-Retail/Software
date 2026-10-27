using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public partial class StartViewRentAndBuyTestUserControl : TextToSpeechUserControl, IComponentConnector
{
	private StartViewModel StartViewModel => base.DataContext as StartViewModel;

	public StartViewRentAndBuyTestUserControl()
	{
		InitializeComponent();
		ApplyTheme();
		PostHealthActivity();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(rent_and_buy_movies_button);
		obj?.SetStyle(pick_up_button);
		obj?.SetStyle(return_button);
		obj?.SetStyle(handicap_button);
		obj?.SetStyle(help_button);
		obj?.SetStyle(sign_in_button);
		obj?.SetStyle(espanol_button);
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

	private void RentAndBuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessRentAndBuyMoviesButtonClick();
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
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessToggleLanguage();
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
