using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using KioskClientService.API.App;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace KioskClientService.API.Services.HttpQueue;

public class HttpRequestMetadata
{
	public string Scheme { get; set; }

	public string Host { get; set; }

	public int Port { get; set; }

	public string Path { get; set; }

	public string QueryString { get; set; }

	public string Method { get; set; }

	public List<Header> Headers { get; set; } = new List<Header>();

	public string Body { get; set; }

	public static HttpRequestMetadata FromHttpRequest(HttpRequest source)
	{
		HttpRequestMetadata httpRequestMetadata = new HttpRequestMetadata();
		httpRequestMetadata.Method = source.Method;
		httpRequestMetadata.Scheme = source.Scheme;
		httpRequestMetadata.Host = source.Host.Host;
		httpRequestMetadata.Port = source.Host.Port.GetValueOrDefault();
		httpRequestMetadata.Path = source.Path;
		httpRequestMetadata.QueryString = source.QueryString.Value;
		httpRequestMetadata.Headers.AddRange(source.Headers.Select(delegate(KeyValuePair<string, StringValues> h)
		{
			KeyValuePair<string, StringValues> keyValuePair = h;
			string key = keyValuePair.Key;
			keyValuePair = h;
			return new Header(key, keyValuePair.Value);
		}));
		using StreamReader streamReader = new StreamReader(source.Body);
		streamReader.DiscardBufferedData();
		streamReader.BaseStream.Seek(0L, SeekOrigin.Begin);
		httpRequestMetadata.Body = streamReader.ReadToEnd();
		return httpRequestMetadata;
	}

	public HttpRequestMessage ToHttpRequestMessage()
	{
		HttpRequestMessage result = new HttpRequestMessage();
		result.Method = new HttpMethod(Method);
		UriBuilder uriBuilder = new UriBuilder();
		uriBuilder.Scheme = Scheme;
		uriBuilder.Host = Host;
		uriBuilder.Port = Port;
		uriBuilder.Path = Path;
		uriBuilder.Query = QueryString;
		result.RequestUri = uriBuilder.Uri;
		Headers.ForEach(delegate(Header h)
		{
			result.Headers.TryAddWithoutValidation(h.Key, h.Value);
		});
		result.Content = new StringContent(Body, Encoding.UTF8, "application/json");
		return result;
	}

	private HttpRequestMetadata()
	{
	}
}
