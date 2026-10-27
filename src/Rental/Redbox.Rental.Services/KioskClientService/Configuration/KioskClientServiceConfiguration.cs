using System;
using System.Net;
using Redbox.Core;
using Redbox.Rental.Model.KioskClientService.Configuration;

namespace Redbox.Rental.Services.KioskClientService.Configuration;

public class KioskClientServiceConfiguration : BaseKioskClientService, IKioskClientServiceConfiguration
{
	private const string _apiPath = "api/configuration";

	public KioskConfiguration GetCurrentKioskConfiguration(string kioskClientServiceBaseUrl)
	{
		WebResult<KioskConfigurationResponse> webResult = ExecuteWebRequest<KioskConfigurationResponse>(new Uri(kioskClientServiceBaseUrl + "/api/configuration/current"), null, "GET");
		if (webResult == null || webResult.StatusCode != HttpStatusCode.OK || webResult?.Response?.Kioskconfiguration == null)
		{
			LogHelper.Instance.Log($"Error: GetCurrentKioskConfiguration failed. StatusCode: {webResult?.StatusCode}");
		}
		return webResult?.Response?.Kioskconfiguration;
	}

	public KioskConfiguration GetKioskSessionKioskConfiguration(string kioskClientServiceBaseUrl)
	{
		WebResult<KioskConfigurationResponse> webResult = ExecuteWebRequest<KioskConfigurationResponse>(new Uri(kioskClientServiceBaseUrl + "/api/configuration/kiosksession"), null, "GET");
		if (webResult == null || webResult.StatusCode != HttpStatusCode.OK || webResult?.Response?.Kioskconfiguration == null)
		{
			LogHelper.Instance.Log($"Error: GetKioskSessionKioskConfiguration failed. StatusCode: {webResult?.StatusCode}");
		}
		return webResult?.Response?.Kioskconfiguration;
	}
}
