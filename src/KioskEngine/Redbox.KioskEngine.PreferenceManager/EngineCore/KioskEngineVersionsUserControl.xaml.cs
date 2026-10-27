using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.EngineCore;

[PreferencePage(Name = "Kiosk Versions", Path = "Engine Core/Versions", TreeOrder = 105)]
public partial class KioskEngineVersionsUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public KioskEngineVersionsUserControl()
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
		APIResponse<EngineCoreKioskEngineVersionResponse> aPIResponse = await KioskEngineApiEngineCoreClient.GetKioskEngineVersions();
		base.DataContext = aPIResponse?.Response?.Content;
		PreferencePageHeaderUserControl.SetStatusText(aPIResponse);
	}
}
