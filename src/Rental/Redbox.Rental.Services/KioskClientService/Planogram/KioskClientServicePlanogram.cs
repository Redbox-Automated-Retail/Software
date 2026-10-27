using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Model.KioskClientService.Planogram;
using Redbox.Rental.Model.Planogram;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Planogram;

public class KioskClientServicePlanogram : BaseKioskClientService, IKioskClientServicePlanogram
{
	private const string _controllerPrefix = "api/planogram";

	public List<Redbox.Rental.Model.Planogram.Planogram> GetActivePlanograms()
	{
		List<Redbox.Rental.Model.Planogram.Planogram> result = new List<Redbox.Rental.Model.Planogram.Planogram>();
		LogHelper.Instance.Log("Getting active planograms.");
		WebResult<List<Redbox.Rental.Model.Planogram.Planogram>> webResult = ExecuteWebRequest<List<Redbox.Rental.Model.Planogram.Planogram>>(new Uri(base.KioskClientServiceBaseUrl + "/api/planogram"), null, "GET", 2000);
		if (webResult != null && webResult.IsSuccessStatusCode && webResult.Response.Count > 0)
		{
			result = webResult.Response;
		}
		else
		{
			LogHelper.Instance.Log("KCS Get Planograms returned " + (webResult?.StatusCode.ToString() ?? "null"));
		}
		return result;
	}

	public Redbox.Rental.Model.Planogram.Planogram GetSelectedPlanogram()
	{
		Redbox.Rental.Model.Planogram.Planogram result = null;
		LogHelper.Instance.Log("Getting selected planogram.");
		WebResult<Redbox.Rental.Model.Planogram.Planogram> webResult = ExecuteWebRequest<Redbox.Rental.Model.Planogram.Planogram>(new Uri(base.KioskClientServiceBaseUrl + "/api/planogram/selected"), null, "GET", 2000);
		if (webResult != null && webResult.IsSuccessStatusCode && webResult.Response != null)
		{
			result = webResult.Response;
		}
		else
		{
			LogHelper.Instance.Log("KCS Get Selected Planogram returned " + (webResult?.StatusCode.ToString() ?? "null"));
		}
		return result;
	}

	public void SaveSelectedPlanogram(Redbox.Rental.Model.Planogram.Planogram planogram)
	{
		LogHelper.Instance.Log("Saving selected planogram " + planogram.Name + ".");
		ExecuteWebRequest<Redbox.Rental.Model.Planogram.Planogram>(new Uri(base.KioskClientServiceBaseUrl + "/api/planogram/selected?planogramName=" + planogram.Name), null, "POST", 2000);
	}

	public bool SyncPlanograms(bool forceRefresh)
	{
		LogHelper.Instance.Log($"Syncing planograms, force = {forceRefresh}");
		return ExecuteWebRequest<ApiBaseResponse>(new Uri(string.Format("{0}/{1}/sync?forceOverwrite={2}", base.KioskClientServiceBaseUrl, "api/planogram", forceRefresh)), null, "POST", 10000)?.IsSuccessStatusCode ?? false;
	}
}
