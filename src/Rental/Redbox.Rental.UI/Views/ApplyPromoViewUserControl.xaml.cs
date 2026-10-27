using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ApplyPromoView")]
public partial class ApplyPromoViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private ApplyPromoViewModel Model => base.DataContext as ApplyPromoViewModel;

	public ApplyPromoViewUserControl()
	{
		InitializeComponent();
		base.Loaded += ApplyPromoViewUserControl_Loaded;
	}

	private void ApplyPromoViewUserControl_Loaded(object sender, RoutedEventArgs e)
	{
		ApplyTheme();
		Model?.OnLoadedCommand.Execute(null);
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(CancelButton);
		obj?.SetStyle(EnterPromoButton);
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.OnGetSpeechControl();
	}
}
