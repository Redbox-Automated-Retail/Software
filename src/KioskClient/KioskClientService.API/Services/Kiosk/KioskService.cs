using System;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Kiosk;

internal class KioskService : IKioskService
{
	private readonly ILogger<KioskService> _logger;

	private readonly IProxyApi _proxyApi;

	public KioskService(ILogger<KioskService> logger, IProxyApi proxyApi)
	{
		_logger = logger;
		_proxyApi = proxyApi;
	}

	public async Task<NearbyKiosksResponse> GetNearbyKiosks(NearbyKiosksRequest request)
	{
		NearbyKiosksResponse result = new NearbyKiosksResponse
		{
			MessageId = request.MessageId
		};
		try
		{
			_logger.LogInfoWithSource($"calling nearby kiosks for kiosk: {request.KioskId}", "GetNearbyKiosks");
			APIResponse<NearbyKiosksResponse> aPIResponse = await _proxyApi.GetNearbyKiosks(request);
			if (aPIResponse == null)
			{
				string text = "no response returned from nearby kiosks lookup";
				_logger.LogErrorWithSource(text, "GetNearbyKiosks");
				result.StatusCode = HttpStatusCode.InternalServerError;
				result.Error = text;
			}
			else if (aPIResponse.IsSuccessStatusCode)
			{
				result.StatusCode = HttpStatusCode.OK;
				result.Kiosks.AddRange(aPIResponse.Response.Kiosks);
				_logger.LogInfoWithSource("found the following kiosks: " + result.Kiosks.ToJson(), "GetNearbyKiosks");
			}
			else
			{
				_logger.LogInfoWithSource($"nearby kiosks failed with the following status code: {aPIResponse.StatusCode}, error: {aPIResponse.Errors.ToJson()}", "GetNearbyKiosks");
				aPIResponse.CopyTo(result);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception during get nearby kiosks", "GetNearbyKiosks");
			result.StatusCode = HttpStatusCode.InternalServerError;
			result.Error = ex.Message;
		}
		return result;
	}
}
