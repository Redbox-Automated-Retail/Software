using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "BrowseView")]
public partial class BrowseViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private BrowseViewModel _browseViewModel;

	public BrowseViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return _browseViewModel?.ProcessGetSpeechControl();
	}

	private void ApplyTheme()
	{
		_ = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
	}

	private void ResetIdleTimerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		HandleWPFHit();
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_browseViewModel = base.DataContext as BrowseViewModel;
	}
}
