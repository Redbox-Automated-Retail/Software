using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "TitleDetailsDevView")]
public partial class TitleDetailsDevViewUserControl : RentalUserControl, IWPFActor, IComponentConnector
{
	private TitleDetailDevModel Model => base.DataContext as TitleDetailDevModel;

	public TitleDetailsDevViewUserControl()
	{
		InitializeComponent();
		base.Loaded += delegate
		{
			Keyboard.Focus(this);
			title_rollup_button.IsEnabled = false;
			dvd_button.IsEnabled = Model.CurrentProduct.IsDvd;
			bluray_button.IsEnabled = Model.CurrentProduct.IsBluRay;
			digital_button.IsEnabled = Model.CurrentProduct.IsDigitalTitle;
			uhd_button.IsEnabled = Model.CurrentProduct.Is4KUhdTitle;
		};
	}

	private void close_button_Click(object sender, RoutedEventArgs e)
	{
		if (Model?.OnCloseButtonClicked != null)
		{
			Model.OnCloseButtonClicked();
		}
	}

	private void dvd_button_Click(object sender, RoutedEventArgs e)
	{
		dvd_button.IsEnabled = false;
		if (Model?.OnDvdButtonClicked != null)
		{
			Model.OnDvdButtonClicked();
		}
		title_rollup_button.IsEnabled = Model.CurrentProduct.HasRollup;
		bluray_button.IsEnabled = Model.CurrentProduct.IsBluRay;
		digital_button.IsEnabled = Model.CurrentProduct.IsDigitalTitle;
		uhd_button.IsEnabled = Model.CurrentProduct.Is4KUhdTitle;
	}

	private void bluray_button_Click(object sender, RoutedEventArgs e)
	{
		title_rollup_button.IsEnabled = Model.CurrentProduct.HasRollup;
		dvd_button.IsEnabled = Model.CurrentProduct.IsDvd;
		bluray_button.IsEnabled = false;
		digital_button.IsEnabled = Model.CurrentProduct.IsDigitalTitle;
		uhd_button.IsEnabled = Model.CurrentProduct.Is4KUhdTitle;
		if (Model?.OnBlurayButtonClicked != null)
		{
			Model.OnBlurayButtonClicked();
		}
	}

	private void digital_button_Click(object sender, RoutedEventArgs e)
	{
		title_rollup_button.IsEnabled = Model.CurrentProduct.HasRollup;
		dvd_button.IsEnabled = Model.CurrentProduct.IsDvd;
		bluray_button.IsEnabled = Model.CurrentProduct.IsBluRay;
		uhd_button.IsEnabled = Model.CurrentProduct.Is4KUhdTitle;
		digital_button.IsEnabled = false;
		if (Model?.OnDigitalButtonClicked != null)
		{
			Model.OnDigitalButtonClicked();
		}
	}

	private void title_rollup_button_Click(object sender, RoutedEventArgs e)
	{
		title_rollup_button.IsEnabled = false;
		dvd_button.IsEnabled = Model.CurrentProduct.IsDvd;
		bluray_button.IsEnabled = Model.CurrentProduct.IsBluRay;
		digital_button.IsEnabled = Model.CurrentProduct.IsDigitalTitle;
		uhd_button.IsEnabled = Model.CurrentProduct.Is4KUhdTitle;
		if (Model?.OnTitleRollupButtonClicked != null)
		{
			Model.OnTitleRollupButtonClicked();
		}
	}

	private void uhd_button_Click(object sender, RoutedEventArgs e)
	{
		title_rollup_button.IsEnabled = Model.CurrentProduct.HasRollup;
		dvd_button.IsEnabled = Model.CurrentProduct.IsDvd;
		bluray_button.IsEnabled = Model.CurrentProduct.IsBluRay;
		digital_button.IsEnabled = Model.CurrentProduct.IsDigitalTitle;
		uhd_button.IsEnabled = false;
		if (Model?.On4KUhdButtonClicked != null)
		{
			Model.On4KUhdButtonClicked();
		}
	}

	private void DisplayAddCustomPriceInfo(object sender, RoutedEventArgs e)
	{
		ToggleAddCustomPrice(showPrice: true);
	}

	private void Add_product_with_custom_price_button_Click(object sender, RoutedEventArgs e)
	{
		if (Model?.CurrentProduct != null)
		{
			decimal? num = (decimal.TryParse(price_input.Text, out var result) ? new decimal?(result) : ((decimal?)null));
			if (num.HasValue)
			{
				Model?.OnAddProductWithCustomPriceButtonClicked?.Invoke(Model.CurrentProduct.ProductId, num.Value);
			}
		}
		ToggleAddCustomPrice(showPrice: false);
	}

	private void ToggleAddCustomPrice(bool showPrice)
	{
		Model.CurrentProduct.AddProductWithCustomPriceButtonVisibility = ((!showPrice) ? Visibility.Collapsed : Visibility.Visible);
		Model.CurrentProduct.DisplayAddProductWithCustomPriceButtonVisibility = (showPrice ? Visibility.Collapsed : Visibility.Visible);
	}
}
