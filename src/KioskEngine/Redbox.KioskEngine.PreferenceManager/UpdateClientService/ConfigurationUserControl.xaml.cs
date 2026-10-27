using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.PreferenceManager.Controls;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Configuration;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model.UpdateClientService.Configuration;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.UpdateClientService;

[PreferencePage(Name = "Get Configuration Changes", Path = "Update Client Service/Configuration/Get Configuration Changes", TreeOrder = 210)]
public partial class ConfigurationUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public ConfigurationUserControl()
	{
		InitializeComponent();
	}

	public async Task<bool> SaveValues(IPreferencePage preferencePage)
	{
		return await Task.Run(() => true);
	}

	public async void LoadValues(IPreferencePage preferencePage)
	{
		await Task.Run(delegate
		{
		});
	}

	private async void GetConfigChangesButton_Click(object sender, RoutedEventArgs e)
	{
		PreferencePageHeaderUserControl.SetStatusText("Getting Configuration Setting Changes via Update Client Service...");
		APIResponse<GetConfigurationChangesResponse> aPIResponse = await KioskEngineApiConfigurationClient.GetConfigurationChanges();
		PreferencePageHeaderUserControl.SetStatusText(null);
		string message = "Get Configuration Changes succeeded.";
		if (aPIResponse == null || aPIResponse.StatusCode != HttpStatusCode.OK)
		{
			message = $"Get Configuration Changes failed. Status code: {aPIResponse?.StatusCode}";
		}
		SimpleMessageWindow.Show(ControlUtilities.GetParentWindow(this), "Get Configuration Changes", message);
	}
}
