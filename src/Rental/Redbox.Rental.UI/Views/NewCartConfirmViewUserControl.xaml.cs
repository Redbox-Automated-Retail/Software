using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "NewCartConfirmView")]
public partial class NewCartConfirmViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private NewCartConfirmViewModel Model => base.DataContext as NewCartConfirmViewModel;

	public NewCartConfirmViewUserControl()
	{
		InitializeComponent();
	}

	private void BrowseItemCancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		DisplayCheckoutProductModel displayProductModel = (e.OriginalSource as FrameworkElement)?.DataContext as DisplayCheckoutProductModel;
		if (Model != null)
		{
			Model.ProcessOnCancelBrowseItemModel(displayProductModel, e.Parameter);
		}
		HandleWPFHit();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model.ProcessGetSpeechControl();
	}
}
