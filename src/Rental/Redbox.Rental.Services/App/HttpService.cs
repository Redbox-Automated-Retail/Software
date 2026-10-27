using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redbox.Core;

namespace Redbox.Rental.Services.App;

public class HttpService : IHttpService
{
	private readonly LogHelper _logger;

	private readonly HttpClient _httpClient;

	public int Timeout { get; set; } = 20000;

	public HttpService(LogHelper logger, HttpClient httpClient)
	{
		_logger = logger;
		_httpClient = httpClient;
	}

	public HttpRequestMessage GenerateRequest(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null, bool isForm = false)
	{
		Uri requestUri;
		if (string.IsNullOrEmpty(baseUrl))
		{
			requestUri = new Uri(endpoint);
		}
		else
		{
			baseUrl = baseUrl.TrimEnd('/');
			requestUri = new Uri(baseUrl + "/" + endpoint);
		}
		HttpRequestMessage httpRequest = new HttpRequestMessage(method, requestUri);
		headers?.ForEach(delegate(Header header)
		{
			httpRequest.Headers.Add(header.Key, header.Value);
		});
		if (requestObject != null)
		{
			httpRequest.Content = new StringContent(JsonConvert.SerializeObject(requestObject, Formatting.None, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			}), Encoding.UTF8, "application/json");
		}
		if (isForm)
		{
			httpRequest.Content = requestObject as MultipartFormDataContent;
		}
		return httpRequest;
	}

	public async Task<APIResponse<T>> SendRequestAsync<T>(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null, bool isForm = false, int? timeout = null, [CallerMemberName] string callerMethod = "", [CallerFilePath] string callerLocation = "", object originalRequestObject = null, bool logRequest = true, bool logResponse = true)
	{
		using HttpRequestMessage genRequest = GenerateRequest(baseUrl, endpoint, requestObject, method, headers, isForm);
		return await SendRequestAsync<T>(genRequest, timeout, callerMethod, callerLocation, requestObject, logRequest, logResponse);
	}

	public async Task<APIResponse<T>> SendRequestAsync<T>(HttpRequestMessage request, int? timeout = null, [CallerMemberName] string callerMethod = "", [CallerFilePath] string callerLocation = "", object originalRequestObject = null, bool logRequest = true, bool logResponse = true)
	{
		using (request)
		{
			CancellationTokenSource timeoutTokenSource = new CancellationTokenSource();
			timeoutTokenSource.CancelAfter(timeout ?? Timeout);
			APIResponse<T> apiResponse = new APIResponse<T>(request, _logger);
			Stopwatch sw = new Stopwatch();
			sw.Start();
			try
			{
				string text = ((request.Content == null) ? null : (await request.Content.ReadAsStringAsync()));
				string value = text;
				if (originalRequestObject != null && originalRequestObject is IMessageScrub)
				{
					apiResponse.RequestContent = ((IMessageScrub)originalRequestObject).Scrub()?.ToString();
				}
				else if (!string.IsNullOrEmpty(value))
				{
					apiResponse.RequestContent = JsonConvert.SerializeObject(value, Formatting.None);
				}
				using HttpResponseMessage result = await _httpClient.SendAsync(request, timeoutTokenSource.Token);
				sw.Stop();
				apiResponse.SetAPIResponse(requestTime: (int)sw.ElapsedMilliseconds, responseMessage: result, content: await (result.Content?.ReadAsStringAsync()));
			}
			catch (Exception exception)
			{
				sw.Stop();
				apiResponse.SetException(exception, (int)sw.ElapsedMilliseconds);
			}
			apiResponse.Log(logRequest, logResponse);
			return apiResponse;
		}
	}

	public async Task<APIResponse<byte[]>> DownloadFile(string url, int? timeout, [CallerMemberName] string callerMethod = "", [CallerFilePath] string callerLocation = "")
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.CancelAfter(timeout ?? Timeout);
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
		APIResponse<byte[]> apiResponse = new APIResponse<byte[]>(request, _logger);
		Stopwatch sw = new Stopwatch();
		sw.Start();
		try
		{
			sw.Stop();
			using HttpResponseMessage result = await _httpClient.GetAsync(url, cancellationTokenSource.Token);
			if (result.IsSuccessStatusCode)
			{
				apiResponse.SetAPIResponse(result, await result.Content.ReadAsByteArrayAsync(), (int)sw.ElapsedMilliseconds);
			}
		}
		catch (Exception exception)
		{
			sw.Stop();
			apiResponse.SetException(exception, (int)sw.ElapsedMilliseconds);
		}
		apiResponse.Log(logRequest: true, logResponse: false);
		return apiResponse;
	}
}
