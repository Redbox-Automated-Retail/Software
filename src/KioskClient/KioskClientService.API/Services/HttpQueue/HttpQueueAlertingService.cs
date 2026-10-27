using System;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace KioskClientService.API.Services.HttpQueue;

internal abstract class HttpQueueAlertingService : BackgroundService
{
	private const int HttpQueueServiceAlertType = 9;

	private readonly string _url;

	private readonly IHttpService _httpService;

	protected readonly long _kioskId;

	public HttpQueueAlertingService(IOptions<AppSettings> options, IStoreService store, IHttpService httpSservice)
	{
		_url = options.Value.ServiceUrl;
		_kioskId = store.KioskId;
		_httpService = httpSservice;
	}

	protected async Task SendAlert(string subAlertType, string message)
	{
		DateTime now = DateTime.Now;
		DataModels.KioskAlertRequest requestObject = new DataModels.KioskAlertRequest
		{
			UniqueId = Guid.NewGuid(),
			KioskId = _kioskId,
			CreatedOn = now.ToUniversalTime(),
			CreatedOnLocal = now.ToString(),
			AlertType = 9,
			SubAlertType = subAlertType,
			AlertMessage = message
		};
		await _httpService.SendRequest<ApplicationResponse>(_url, "api/data/kioskalert", requestObject, HttpMethod.Post);
	}
}
