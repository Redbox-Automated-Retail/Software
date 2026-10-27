using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.PreferenceManager.Controls;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.KioskHealth;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model.UpdateClientService.KioskHealth;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.UpdateClientService;

[PreferencePage(Name = "Ping Service", Path = "Update Client Service/Kiosk Health/Ping Service", TreeOrder = 220)]
public partial class PingUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public PingUserControl()
	{
		InitializeComponent();
	}

	public async Task<bool> SaveValues(IPreferencePage preferencePage)
	{
		return await Task.Run(() => true);
	}

	public async void LoadValues(IPreferencePage preferencePage)
	{
		PreferencePageHeaderUserControl.SetStatusText("Loading...");
		SendPingButton.IsEnabled = false;
		APIResponse<PingServiceInfo> aPIResponse = await KioskEngineApiKioskHealthClient.GetPingServerInfo();
		PreferencePageHeaderUserControl.SetStatusText(aPIResponse);
		SendPingButton.IsEnabled = aPIResponse?.Response?.Enabled == true;
		base.DataContext = aPIResponse?.Response;
	}

	private async void SendPingButton_Click(object sender, RoutedEventArgs e)
	{
		PreferencePageHeaderUserControl.SetStatusText("Sending ping...");
		APIResponse<PingResponse> aPIResponse = await KioskEngineApiKioskHealthClient.SendPing();
		PreferencePageHeaderUserControl.SetStatusText(null);
		string message = "Ping sent successfully.";
		if (!aPIResponse.IsSuccessStatusCode)
		{
			message = "Ping failed.  Error sending message to KioskEngine API.  StatusCode=" + aPIResponse?.StatusCode.ToString();
		}
		else if ((aPIResponse?.Response?.StatusCode ?? 500) != 200)
		{
			message = $"Ping failed.  StatusCode={aPIResponse?.Response?.StatusCode}";
		}
		SimpleMessageWindow.Show(ControlUtilities.GetParentWindow(this), "Send Ping", message);
	}

	private Window GetParentWindow()
	{
		Window window = null;
		FrameworkElement frameworkElement = this;
		while (window == null && frameworkElement?.Parent != null)
		{
			frameworkElement = frameworkElement.Parent as FrameworkElement;
			if (frameworkElement is Window)
			{
				window = frameworkElement as Window;
			}
		}
		return window;
	}
}
