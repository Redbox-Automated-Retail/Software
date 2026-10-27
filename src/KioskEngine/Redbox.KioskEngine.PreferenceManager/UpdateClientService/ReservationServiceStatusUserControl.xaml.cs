using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Newtonsoft.Json;
using Redbox.KioskEngine.PreferenceManager.Controls;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.UpdateClientService.Reservation;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.UpdateClientService;

[PreferencePage(Name = "Reservation Service Status", Path = "Update Client Service/Reservations/Reservation Service Status", TreeOrder = 230)]
public partial class ReservationServiceStatusUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public ReservationServiceStatusUserControl()
	{
		InitializeComponent();
	}

	public async Task<bool> SaveValues(IPreferencePage preferencePage)
	{
		return await Task.Run(() => true);
	}

	public async void LoadValues(IPreferencePage preferencePage)
	{
		base.DataContext = null;
		PreferencePageHeaderUserControl.SetStatusText("Loading...");
		APIResponse<GetReservationServiceStatusResponse> aPIResponse = await KioskEngineApiReservationClient.GetReservationServiceStatus();
		base.DataContext = aPIResponse?.Response?.Content;
		PreferencePageHeaderUserControl.SetStatusText(aPIResponse);
	}

	private async void RegisterButton_Click(object sender, RoutedEventArgs e)
	{
		PreferencePageHeaderUserControl.SetStatusText("Registering with broker service...");
		APIResponse<RegisterWithBrokerServiceResponse> aPIResponse = await KioskEngineApiReservationClient.RegisterWithBrokerService();
		string text = ((aPIResponse != null && aPIResponse.StatusCode != HttpStatusCode.OK && aPIResponse.StatusCode != HttpStatusCode.Conflict) ? " Retry timer started." : null);
		RegisterWithBrokerServiceResponse registerWithBrokerServiceResponse = null;
		if (aPIResponse.StatusCode != HttpStatusCode.OK)
		{
			try
			{
				registerWithBrokerServiceResponse = JsonConvert.DeserializeObject<RegisterWithBrokerServiceResponse>(aPIResponse.ResponseContent);
			}
			catch
			{
			}
		}
		string message = (((aPIResponse?.StatusCode ?? HttpStatusCode.InternalServerError) == HttpStatusCode.OK) ? "Register successful" : ("register failed.  Status code: " + aPIResponse?.StatusCode.ToString() + ". " + text + GetErrorMessage(registerWithBrokerServiceResponse?.Errors)));
		SimpleMessageWindow.Show(ControlUtilities.GetParentWindow(this), "Register with broker service.", message);
		LoadValues(null);
	}

	private async void UnregisterButton_Click(object sender, RoutedEventArgs e)
	{
		PreferencePageHeaderUserControl.SetStatusText("Unregistering from broker service...");
		APIResponse<UnregisterFromBrokerServiceResponse> aPIResponse = await KioskEngineApiReservationClient.UnregisterFromBrokerService();
		UnregisterFromBrokerServiceResponse unregisterFromBrokerServiceResponse = null;
		if (aPIResponse.StatusCode != HttpStatusCode.OK)
		{
			try
			{
				unregisterFromBrokerServiceResponse = JsonConvert.DeserializeObject<UnregisterFromBrokerServiceResponse>(aPIResponse.ResponseContent);
			}
			catch
			{
			}
		}
		string message = ((aPIResponse.StatusCode == HttpStatusCode.OK) ? "Unregister successful" : ("Unregister failed.  Status code: " + aPIResponse?.StatusCode.ToString() + GetErrorMessage(unregisterFromBrokerServiceResponse?.Errors)));
		SimpleMessageWindow.Show(ControlUtilities.GetParentWindow(this), "Unregister from broker service", message);
		LoadValues(null);
	}

	private string GetErrorMessage(List<Redbox.Rental.Model.Error> errorList)
	{
		string result = null;
		if (errorList != null && errorList.Count > 0)
		{
			string[] value = errorList.Select((Redbox.Rental.Model.Error x) => x.Code + " " + x.Message).ToArray();
			result = " Errors: " + string.Join(", ", value);
		}
		return result;
	}
}
