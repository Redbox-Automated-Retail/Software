using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "StartView")]
public partial class StartViewContainerUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private StartViewModel StartViewModel => base.DataContext as StartViewModel;

	public StartViewContainerUserControl()
	{
		InitializeComponent();
		RegisterRoutedCommand("RentOrBuyMoviesCommand", Redbox.Rental.UI.Commands.RentOrBuyMoviesCommand);
		RegisterRoutedCommand("BuyMoviesCommand", Redbox.Rental.UI.Commands.BuyMoviesCommand);
		RegisterRoutedCommand("PickupProductCommand", Redbox.Rental.UI.Commands.PickupProductCommand);
		RegisterRoutedCommand("ReturnProductCommand", Redbox.Rental.UI.Commands.ReturnProductCommand);
		RegisterRoutedCommand("HelpCommand", Redbox.Rental.UI.Commands.HelpCommand);
		RegisterRoutedCommand("SignInCommand", Redbox.Rental.UI.Commands.SignInCommand);
	}

	private void RentOrBuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessRentMovieButtonClick();
		HandleWPFHit();
	}

	private void RentOrBuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
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
		e.CanExecute = StartViewModel?.BuyMoviesEnabled ?? false;
	}

	private void BuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessBuyMoviesButtonClick();
		HandleWPFHit();
	}

	private void PickupProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessPickupButtonClick();
		HandleWPFHit();
	}

	private void ReturnProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessReturnButtonClicked();
		HandleWPFHit();
	}

	private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessHelpButtonClicked();
		HandleWPFHit();
	}

	private void ToggleADAModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessToggleADAMode();
		HandleWPFHit();
	}

	private void ExitBurnInViewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessExitBurnInView();
		HandleWPFHit();
	}

	private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
	}

	private void BannerClickCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel.ProcessBannerClicked();
		HandleWPFHit();
	}

	private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessSignInButtonClicked();
		HandleWPFHit();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return StartViewModel?.ProcessGetSpeechControl();
	}
}
