using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class BrowseCheckOutBarUserControl : UserControl, IWPFActor, IComponentConnector
{
	public IActor Actor { get; set; }

	private BrowseViewModel BrowseViewModel => base.DataContext as BrowseViewModel;

	public event WPFHitHandler OnWPFHit;

	public BrowseCheckOutBarUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(checkout_button);
		obj?.SetStyle(SwitchProductFamilyButton1);
	}

	private void HandleWPFHit()
	{
		ServiceLocator.Instance.GetService<IRenderingService>()?.ActiveScene?.PrcoessWPFHit();
	}

	private void CheckOutCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseViewModel?.ProcessOnCheckOutButtonClicked();
		HandleWPFHit();
	}

	private void CheckOutCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void ToggleTitleFamily1CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseViewModel?.ProcessOnToggleTitleFamilyButton1Clicked();
		HandleWPFHit();
	}

	private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseViewModel?.ProcessOnSignInButtonClicked();
		HandleWPFHit();
	}

	private void SignOutCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseViewModel?.ProcessOnSignOutButtonClicked();
		HandleWPFHit();
	}

	private void MyPerksCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseViewModel?.ProcessOnMyPerksButtonClicked();
		HandleWPFHit();
	}
}
