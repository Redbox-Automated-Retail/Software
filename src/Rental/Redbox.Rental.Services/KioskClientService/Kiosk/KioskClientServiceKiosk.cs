using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Kiosk;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskClientService.Kiosk;

public class KioskClientServiceKiosk : BaseKioskClientService, IKioskClientServiceKiosk
{
	private const string _apiPath = "api/kiosk";

	private List<NearbyKiosk> _nearbyKiosks;

	private NearbyKiosk _dualKiosk;

	public List<NearbyKiosk> NearbyKiosks => _nearbyKiosks;

	public NearbyKiosk DualKiosk => _dualKiosk;

	public bool HasDual => _dualKiosk != null;

	public string KioskLabel
	{
		get
		{
			if (string.IsNullOrEmpty(_dualKiosk?.Label) || !(_dualKiosk.Label == "A"))
			{
				return "A";
			}
			return "B";
		}
	}

	public void LoadNearbyKiosks()
	{
		LogHelper.Instance.Log("KCS.Kiosk.KioskClientServiceKiosk.LoadNearbyKiosks");
		long num = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0;
		_nearbyKiosks = ((num > 0) ? GetNearbyKiosks(num) : null)?.Kiosks?.OrderBy((NearbyKiosk nk) => nk.DistanceMiles).ThenBy((NearbyKiosk nk) => nk.KioskId).ToList();
		_dualKiosk = _nearbyKiosks?.Where((NearbyKiosk nk) => nk.IsDual).Select((NearbyKiosk nk) => nk).FirstOrDefault();
	}

	public IKioskClientServiceNearbyKiosksResponse GetNearbyKiosks(long kioskId)
	{
		KioskClientServiceNearbyKiosksResponse kioskClientServiceNearbyKiosksResponse = new KioskClientServiceNearbyKiosksResponse
		{
			MessageId = Guid.NewGuid()
		};
		try
		{
			var anon = new
			{
				MessageId = kioskClientServiceNearbyKiosksResponse.MessageId,
				KioskId = kioskId
			};
			LogHelper.Instance.Log("KioskClientServiceKiosk.GetNearbyKiosks - sending request of " + anon.ToJson());
			WebResult<KioskClientServiceNearbyKiosksResponse> webResult = ExecuteWebRequest<KioskClientServiceNearbyKiosksResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/kiosk/nearby"), anon, "POST", Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Kiosk.NearbyKiosksTimeout);
			LogHelper.Instance.Log("KioskClientServiceKiosk.GetNearbyKiosks - response: " + webResult.ToJson());
			if (webResult.ExceptionStatus == WebExceptionStatus.Success)
			{
				LogHelper.Instance.Log("KioskClientServiceKiosk.GetNearbyKiosk - api call successful");
				kioskClientServiceNearbyKiosksResponse.Kiosks.AddRange(webResult.Response.Kiosks);
			}
			else
			{
				string text = $"api call returned result code: {webResult.ExceptionStatus}";
				LogHelper.Instance.Log("KioskClientServiceKiosk.GetNearbyKiosks - " + text);
				kioskClientServiceNearbyKiosksResponse.Error = text;
			}
		}
		catch (Exception e)
		{
			string text2 = "KioskClientServiceKiosk.GetNearbyKiosks - An unhandled exception was raised!";
			LogHelper.Instance.Log(text2, e);
			kioskClientServiceNearbyKiosksResponse.Error = text2;
		}
		return kioskClientServiceNearbyKiosksResponse;
	}

	public async Task<IKioskClientServiceNearbyKiosksResponse> GetNearbyKiosksAsync(long kioskId)
	{
		return await Task.Run(() => GetNearbyKiosks(kioskId));
	}
}
