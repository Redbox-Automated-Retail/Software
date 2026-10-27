using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PerksOfferDetailsView")]
public partial class PerksOfferDetailsViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private PerksOfferDetailsViewModel PerksOfferDetailsViewModel => base.DataContext as PerksOfferDetailsViewModel;

	public PerksOfferDetailsViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return PerksOfferDetailsViewModel?.ProcessGetSpeechControl();
	}

	private void ApplyTheme()
	{
		(ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme)?.SetStyle(CancelButton);
	}

	private void PerksOfferDetailsViewUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		PopulateProgressStackPanel();
	}

	private void PopulateProgressStackPanel()
	{
		if (PerksOfferDetailsViewModel.CurrentValue.HasValue && PerksOfferDetailsViewModel.MaxValue.HasValue && PerksOfferDetailsViewModel.MaxValue.Value > 0 && PerksOfferDetailsViewModel.MaxValue.Value <= 7)
		{
			ProgressStackPanel.Width = PerksOfferDetailsViewModel.MaxValue.Value * 60;
			for (int i = 0; i < PerksOfferDetailsViewModel.MaxValue.Value; i++)
			{
				string resourceKey = ((i < PerksOfferDetailsViewModel.CurrentValue.Value) ? "style_circle_checkmark_image" : "style_circle_empty_image");
				ProgressStackPanel.Children.Add(new Image
				{
					Style = (FindResource(resourceKey) as Style)
				});
			}
		}
	}

	private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		PerksOfferDetailsViewModel?.ProcessOnCancelButtonClicked();
	}
}
