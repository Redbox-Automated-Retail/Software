using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Redbox.NetCore.Middleware.Http;

public class ClientWrapper : IClientWrapper
{
	private readonly HttpClient _httpClient;

	private readonly ILogger<ClientWrapper> _logger;

	private int _timeout;

	public int Timeout
	{
		get
		{
			return _timeout;
		}
		set
		{
			_httpClient.Timeout = TimeSpan.FromMilliseconds(value);
			_timeout = value;
		}
	}

	public ClientWrapper(HttpClient client, ILoggerFactory loggerFactory)
	{
		_httpClient = client;
		_logger = loggerFactory.CreateLogger<ClientWrapper>();
	}

	public async Task<BaseResponse> SendRequest(HttpRequestMessage request)
	{
		_ = 1;
		try
		{
			HttpResponseMessage message = await _httpClient.SendAsync(request);
			string content = await (message.Content?.ReadAsStringAsync());
			return new BaseResponse
			{
				Content = content,
				StatusCode = (int)message.StatusCode
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An unhandled exception occurred when sending a message to an external API");
			return new BaseResponse
			{
				Errors = 
				{
					new Error
					{
						Code = HttpStatusCode.InternalServerError.ToString(),
						Message = ex.Message
					}
				}
			};
		}
	}
}
