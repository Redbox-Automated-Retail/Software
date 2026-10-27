using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.App;

public class HttpService : IHttpService
{
	private delegate HttpContent ContentGenerator(object source);

	private readonly ILogger<HttpService> _logger;

	private readonly HttpClient _httpClient;

	public int Timeout { get; set; } = 20000;

	public HttpService(ILogger<HttpService> logger, HttpClient httpClient)
	{
		_logger = logger;
		_httpClient = httpClient;
	}

	private HttpRequestMessage GenerateRequest(ContentGenerator generator, string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null)
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
			httpRequest.Content = generator(requestObject);
		}
		return httpRequest;
	}

	public async Task<APIResponse<T>> SendRequest<T>(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null, bool logResponseContent = true)
	{
		using HttpRequestMessage genRequest = GenerateRequest(HttpExtensions.ToStandardPayloadContent, baseUrl, endpoint, requestObject, method, headers);
		return await SendRequest<T>(genRequest, logResponseContent);
	}

	public async Task<APIResponse<T>> SendGZipRequest<T>(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null, bool logResponseContent = true)
	{
		using HttpRequestMessage genRequest = GenerateRequest(HttpExtensions.ToGZipPayloadContent, baseUrl, endpoint, requestObject, method, headers);
		return await SendRequest<T>(genRequest, logResponseContent);
	}

	public async Task<APIResponse<T>> SendDeflateRequest<T>(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null, bool logResponseContent = true)
	{
		using HttpRequestMessage genRequest = GenerateRequest(HttpExtensions.ToDeflatePayloadContent, baseUrl, endpoint, requestObject, method, headers);
		return await SendRequest<T>(genRequest, logResponseContent);
	}

	public async Task<HttpResponseMessage> SendRequest(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null, bool logResponseContent = true)
	{
		using HttpRequestMessage genRequest = GenerateRequest(HttpExtensions.ToStandardPayloadContent, baseUrl, endpoint, requestObject, method, headers);
		return await SendRequest(genRequest, logResponseContent);
	}

	public async Task<APIResponse<T>> SendRequest<T>(HttpRequestMessage request, bool logResponseContent = true)
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.CancelAfter(Timeout);
		APIResponse<T> apiResponse = new APIResponse<T>(request, _logger);
		Stopwatch sw = new Stopwatch();
		sw.Start();
		try
		{
			if (request.Content == null)
			{
				apiResponse.RequestContent = null;
			}
			else
			{
				apiResponse.RequestContent = ((!(request.Content is IMessageScrub)) ? JsonConvert.SerializeObject(request.Content, Formatting.None) : (request.Content as IMessageScrub).Scrub()?.ToString());
			}
			HttpResponseMessage result = await _httpClient.SendAsync(request, cancellationTokenSource.Token);
			sw.Stop();
			apiResponse.SetAPIResponse(requestTime: (int)sw.ElapsedMilliseconds, responseMessage: result, content: await (result.Content?.ReadAsStringAsync()));
		}
		catch (Exception exception)
		{
			sw.Stop();
			apiResponse.SetException(exception, (int)sw.ElapsedMilliseconds);
		}
		apiResponse.Log(logResponseContent);
		return apiResponse;
	}

	public async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, bool logResponseContent = true)
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.CancelAfter(Timeout);
		HttpResponseMessage result = null;
		Stopwatch sw = new Stopwatch();
		sw.Start();
		StringBuilder builder = new StringBuilder();
		try
		{
			if (request != null)
			{
				builder.Append("Request: ").Append(request.Method).AppendLine($", Uri: {request.RequestUri}");
			}
			result = await _httpClient.SendAsync(request, cancellationTokenSource.Token);
			if (result != null)
			{
				builder.Append("Status: ").Append(result.StatusCode);
				if (result.Content != null)
				{
					string text = await result.Content.ReadAsStringAsync();
					builder.Append(", Response Length: ").Append(text.Length);
					if (logResponseContent)
					{
						builder.Append(", Content: ").Append(text);
					}
				}
			}
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Exception occured!");
		}
		finally
		{
			sw.Stop();
			builder.AppendLine($"Time: {sw.ElapsedMilliseconds}");
			_logger.LogInformation(builder.ToString());
		}
		return result;
	}

	public async Task<APIResponse<byte[]>> DownloadFile(string url)
	{
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
		APIResponse<byte[]> apiResponse = new APIResponse<byte[]>(request, _logger);
		Stopwatch sw = new Stopwatch();
		sw.Start();
		try
		{
			using HttpResponseMessage result = await _httpClient.GetAsync(url);
			if (result.IsSuccessStatusCode)
			{
				apiResponse.SetAPIResponse(result, await result.Content.ReadAsByteArrayAsync(), (int)sw.ElapsedMilliseconds);
			}
		}
		catch (Exception exception)
		{
			apiResponse.SetException(exception, (int)sw.ElapsedMilliseconds);
		}
		apiResponse.Log(responseContentLoggingEnabled: false);
		return apiResponse;
	}
}
