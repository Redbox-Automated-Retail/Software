using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Rental.Model.Reservation;

namespace Redbox.KioskEngine.PreferenceManager.UpdateClientService;

public partial class AddReservationTitleWindow : Window, IComponentConnector
{
	private class DisplayReservationTitleType
	{
		public ReservationTitleType ReservationTitleType { get; set; }

		public override string ToString()
		{
			return ReservationTitleType.ToString();
		}
	}

	private class DisplayReservationDiscountType
	{
		public ReservationDiscountType ReservationDiscountType { get; set; }

		public override string ToString()
		{
			return ReservationDiscountType.ToString();
		}
	}

	public List<DisplayReservableTitle> DisplayReservableTitles { get; set; }

	public ReservationRequestTitle ReservationRequestTitle { get; set; }

	public AddReservationTitleWindow()
	{
		InitializeComponent();
		List<DisplayReservationTitleType> list = new List<DisplayReservationTitleType>();
		foreach (ReservationTitleType item in Enum.GetValues(typeof(ReservationTitleType)).Cast<ReservationTitleType>())
		{
			list.Add(new DisplayReservationTitleType
			{
				ReservationTitleType = item
			});
		}
		TypeComboBox.ItemsSource = list;
		List<DisplayReservationDiscountType> list2 = new List<DisplayReservationDiscountType>();
		foreach (ReservationDiscountType item2 in Enum.GetValues(typeof(ReservationDiscountType)).Cast<ReservationDiscountType>())
		{
			list2.Add(new DisplayReservationDiscountType
			{
				ReservationDiscountType = item2
			});
		}
		ReservationDiscountTypeComboBox.ItemsSource = list2;
	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
	}

	private void OkButton_Click(object sender, RoutedEventArgs e)
	{
		if (ReservationRequestTitle.TitleID == 0)
		{
			SimpleMessageWindow.Show(this, "Add Title Error", "Title is required.");
			return;
		}
		base.DialogResult = true;
		Close();
	}

	private void UpdateDiscountedPrice()
	{
		if (decimal.TryParse(DiscountTextBox.Text, out var result) && decimal.TryParse(PriceTextBox.Text, out var result2))
		{
			ReservationRequestTitle.DiscountedPrice = result2 - result;
			LabelDiscountedPrice.Content = ReservationRequestTitle?.DiscountedPrice.ToString("f2");
		}
	}

	private void DiscountTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		UpdateDiscountedPrice();
	}

	private void PriceTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		UpdateDiscountedPrice();
	}

	private void ProductIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		UpdatePrice();
	}

	private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		UpdatePrice();
	}

	private void UpdatePrice()
	{
		DisplayReservableTitle displayReservableTitle = ProductIdComboBox.SelectedItem as DisplayReservableTitle;
		if (displayReservableTitle?.ReservableTitle != null)
		{
			ReservableTitle reservableTitle = displayReservableTitle.ReservableTitle;
			switch (ReservationRequestTitle.ReservationType)
			{
			case ReservationTitleType.Rental:
				ReservationRequestTitle.Price = reservableTitle.OneNightRentalPrice;
				PriceTextBox.Text = ReservationRequestTitle.Price.ToString("f2");
				break;
			case ReservationTitleType.Purchase:
				ReservationRequestTitle.Price = reservableTitle.PurchasePrice;
				PriceTextBox.Text = ReservationRequestTitle.Price.ToString("f2");
				break;
			case ReservationTitleType._Unspecified:
				break;
			}
		}
	}
}
