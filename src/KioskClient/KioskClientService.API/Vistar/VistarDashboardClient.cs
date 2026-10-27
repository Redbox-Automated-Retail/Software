using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Vistar;

public class VistarDashboardClient : IVistarDashboardClient
{
	private readonly IHttpService _httpService;

	private readonly string _serviceUrl;

	private ILogger<VistarDashboardClient> _logger;

	public VistarDashboardClient(IHttpService httpService, ILogger<VistarDashboardClient> logger, string serviceUrl)
	{
		_httpService = httpService;
		_serviceUrl = serviceUrl;
		_logger = logger;
	}

	public async Task<APIResponse<VistarAuthenticateResponse>> Authenticate(string email, string password)
	{
		VistarAuthenticateRequest requestObject = new VistarAuthenticateRequest
		{
			email = email,
			password = password
		};
		return await _httpService.SendRequest<VistarAuthenticateResponse>(_serviceUrl, "session", requestObject, HttpMethod.Post);
	}

	public async Task<APIResponse<List<object>>> GetAllVenues()
	{
		return await _httpService.SendRequest<List<object>>(_serviceUrl, "venue/", null, HttpMethod.Get);
	}
}
