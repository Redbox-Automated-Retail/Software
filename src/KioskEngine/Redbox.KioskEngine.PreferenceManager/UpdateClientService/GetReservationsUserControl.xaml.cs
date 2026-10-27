using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.PreferenceManager.Controls;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.UpdateClientService;

[PreferencePage(Name = "Current Reservations", Path = "Update Client Service/Reservations/Current Reservations", TreeOrder = 235)]
public partial class GetReservationsUserControl : UserControl, IPreferencePageHost, IComponentConnector, IStyleConnector
{
	private IPreferencePage _preferencePage;

	public GetReservationsUserControl()
	{
		InitializeComponent();
	}

	public async Task<bool> SaveValues(IPreferencePage preferencePage)
	{
		return await Task.Run(() => true);
	}

	public async void LoadValues(IPreferencePage preferencePage)
	{
		_preferencePage = preferencePage;
		base.DataContext = null;
		PreferencePageHeaderUserControl.SetStatusText("Loading...");
		APIResponse<Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.GetReservationsResponse> aPIResponse = await KioskEngineApiReservationClient.GetReservations();
		base.DataContext = aPIResponse?.Response?.Content;
		if (aPIResponse.StatusCode == HttpStatusCode.NotFound)
		{
			PreferencePageHeaderUserControl.SetStatusText("There are no existing reservations.");
		}
		else
		{
			PreferencePageHeaderUserControl.SetStatusText(aPIResponse);
		}
	}

	private void ReservationsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.Reservation reservation = ReservationsDataGrid.SelectedItem as Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.Reservation;
		ReservationDetailsStackPanel.Visibility = ((reservation == null) ? Visibility.Collapsed : Visibility.Visible);
		ReservationDetailsStackPanel.DataContext = reservation?.ReservationDetails;
	}

	private void RefreshData()
	{
		LoadValues(_preferencePage);
	}

	private async void CancelReservationButton_Click(object sender, RoutedEventArgs e)
	{
		if (ReservationsDataGrid.SelectedItem is Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.Reservation reservation && MessageBox.Show("Are you sure you want to cancel reservation " + reservation?.ReferenceNumber + "?", "Cancel Reservation Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
		{
			APIResponse<CancelReservationResponse> aPIResponse = await KioskEngineApiReservationClient.CancelReservations(reservation.ReferenceNumber);
			string text = (aPIResponse.IsSuccessStatusCode ? "Succeeded." : $"failed.  StatusCode: {(int)aPIResponse.StatusCode} {aPIResponse.StatusCode}");
			SimpleMessageWindow.Show(ControlUtilities.GetParentWindow(this), "Cancel Reservation Confirmation", "Cancel " + text);
			if (aPIResponse.IsSuccessStatusCode)
			{
				RefreshData();
			}
		}
	}
}
