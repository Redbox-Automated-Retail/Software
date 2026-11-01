using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;

public class KioskEngineApiEngineCoreClient : KioskEngineApiBaseClient
{
	private static string _apiPath = "api/enginecore";

	public static async Task<APIResponse<EngineCoreKioskEngineVersionResponse>> GetKioskEngineVersions()
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/getkioskengineversions", null, HttpMethod.Get);
		return await httpService.SendRequestAsync<EngineCoreKioskEngineVersionResponse>(request, null, "GetKioskEngineVersions", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskEngineApiEngineCoreClient.cs");
	}

	public static async Task<APIResponse<GetStoreInfoResponse>> GetStoreInfo()
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/getstoreinfo", null, HttpMethod.Get);
		return await httpService.SendRequestAsync<GetStoreInfoResponse>(request, null, "GetStoreInfo", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskEngineApiEngineCoreClient.cs");
	}

	public static async Task<APIResponse<GetPreferenceValueResponse>> GetPreferenceValue(string pageName, string settingName)
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/getpreferencevalue?pageName=" + pageName + "&settingName=" + settingName, null, HttpMethod.Get);
		return await httpService.SendRequestAsync<GetPreferenceValueResponse>(request, null, "GetPreferenceValue", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskEngineApiEngineCoreClient.cs");
	}

	public static async Task<APIResponse<MachineSettingValueResponse>> GetMachineSettingValue(string settingPath, string settingName)
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/machinesettingvalue?settingPath=" + settingPath + "&settingName=" + settingName, null, HttpMethod.Get);
		return await httpService.SendRequestAsync<MachineSettingValueResponse>(request, null, "GetMachineSettingValue", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskEngineApiEngineCoreClient.cs");
	}

	public static async Task<T> GetMachineSettingValue<T>(string settingPath, string settingName, T defaultValue)
	{
		APIResponse<MachineSettingValueResponse> aPIResponse = await GetMachineSettingValue(settingPath, settingName);
		try
		{
			if (aPIResponse.IsSuccessStatusCode && aPIResponse.Response?.Content != null)
			{
				return (T)ConversionHelper.ChangeType(aPIResponse.Response.Content, typeof(T));
			}
		}
		catch
		{
		}
		return defaultValue;
	}

	public static async Task<APIResponse<MachineSettingValueResponse>> SetMachineSettingValue(string settingPath, string settingName, string value)
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/machinesettingvalue?settingPath=" + settingPath + "&settingName=" + settingName + "&value=" + value, null, HttpMethod.Post);
		return await httpService.SendRequestAsync<MachineSettingValueResponse>(request, null, "SetMachineSettingValue", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskEngineApiEngineCoreClient.cs");
	}

	public static async Task<APIResponse<ApplicationStateValueResponse>> GetApplicationStateValue(string settingName)
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/applicationstatevalue?settingName=" + settingName, null, HttpMethod.Get);
		return await httpService.SendRequestAsync<ApplicationStateValueResponse>(request, null, "GetApplicationStateValue", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskEngineApiEngineCoreClient.cs");
	}

	public static async Task<T> GetApplicationStateValue<T>(string settingName, T defaultValue)
	{
		APIResponse<ApplicationStateValueResponse> aPIResponse = await GetApplicationStateValue(settingName);
		try
		{
			if (aPIResponse.IsSuccessStatusCode && aPIResponse.Response?.Content != null)
			{
				return (T)ConversionHelper.ChangeType(aPIResponse.Response.Content, typeof(T));
			}
		}
		catch
		{
		}
		return defaultValue;
	}

	public static async Task<APIResponse<ApplicationStateValueResponse>> SetApplicationStateValue(string settingName, string value)
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/applicationstatevalue?settingName=" + settingName + "&value=" + value, null, HttpMethod.Post);
		return await httpService.SendRequestAsync<ApplicationStateValueResponse>(request, null, "SetApplicationStateValue", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskEngineApiEngineCoreClient.cs");
	}
}
