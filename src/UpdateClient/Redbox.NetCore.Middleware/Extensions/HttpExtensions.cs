using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;

namespace Redbox.NetCore.Middleware.Extensions;

public static class HttpExtensions
{
	public static string GetActivityId(this HttpContext context)
	{
		if (context.Request.Headers.ContainsKey("x-redbox-activityid"))
		{
			return context.Request.Headers["x-redbox-activityid"][0];
		}
		if (context.Items.ContainsKey("ActivityId"))
		{
			return context.Items["ActivityId"].ToString();
		}
		return string.Empty;
	}

	public static string GetKioskId(this HttpContext context)
	{
		if (context.Request.Headers.ContainsKey("x-redbox-kioskid"))
		{
			return context.Request.Headers["x-redbox-kioskid"][0];
		}
		if (context.Items.ContainsKey("KioskId"))
		{
			return context.Items["KioskId"].ToString();
		}
		return string.Empty;
	}

	public static async Task<string> ReadBodyAsync(this HttpRequest request)
	{
		return await ReadStream(request.Body);
	}

	public static HttpRequestMessage AddActivityIdHeader(this HttpRequestMessage message, Guid activityId)
	{
		message?.AddActivityIdHeader(activityId.ToString());
		return message;
	}

	public static HttpRequestMessage AddActivityIdHeader(this HttpRequestMessage message, string activityId)
	{
		message?.Headers.Add("x-redbox-activityid", activityId);
		return message;
	}

	public static List<Header> AddActivityIdHeader(this List<Header> headers, Guid activityId)
	{
		headers?.AddActivityIdHeader(activityId.ToString());
		return headers;
	}

	public static List<Header> AddActivityIdHeader(this List<Header> headers, string activityId)
	{
		if (headers != null)
		{
			headers.Add(new Header("x-redbox-activityid", activityId));
			headers.Add(new Header("X-Correlation-ID", activityId));
		}
		return headers;
	}

	public static HttpRequestMessage AddApiKeyHeader(this HttpRequestMessage message, string apikey)
	{
		if (message != null && !string.IsNullOrEmpty(apikey))
		{
			message.Headers.Add("x-api-key", apikey);
		}
		return message;
	}

	public static List<Header> AddApiKeyHeader(this List<Header> headers, string apikey)
	{
		if (headers != null && !string.IsNullOrEmpty(apikey))
		{
			headers.Add(new Header("x-api-key", apikey));
		}
		return headers;
	}

	public static HttpRequestMessage AddAppNameHeader(this HttpRequestMessage message, string appName)
	{
		if (message != null && appName != null)
		{
			string value = new string(appName.Where((char c) => char.IsLetter(c)).ToArray()).ToLower();
			message.Headers.Add("app-name", value);
			message.Headers.Add("x-redbox-app-name", value);
		}
		return message;
	}

	public static List<Header> AddAppNameHeader(this List<Header> headers, string appName)
	{
		if (headers != null && appName != null)
		{
			string value = new string(appName.Where((char c) => char.IsLetter(c)).ToArray()).ToLower();
			headers.Add(new Header("app-name", value));
			headers.Add(new Header("x-redbox-app-name", value));
		}
		return headers;
	}

	public static HttpRequestMessage AddAuthorizationHeader(this HttpRequestMessage message, string token)
	{
		if (message != null && !string.IsNullOrEmpty(token))
		{
			message.Headers.Add("authorization", token);
		}
		return message;
	}

	public static List<Header> AddAuthorizationHeader(this List<Header> headers, string token)
	{
		if (headers != null && !string.IsNullOrEmpty(token))
		{
			headers.Add(new Header("authorization", token));
		}
		return headers;
	}

	public static HttpRequestMessage AddKioskIdHeader(this HttpRequestMessage message, int kioskId)
	{
		return message.AddKioskIdHeader(kioskId.ToString());
	}

	public static HttpRequestMessage AddKioskIdHeader(this HttpRequestMessage message, int? kioskId)
	{
		if (message != null && kioskId.HasValue)
		{
			message.AddKioskIdHeader(kioskId.Value);
		}
		return message;
	}

	public static HttpRequestMessage AddKioskIdHeader(this HttpRequestMessage message, long kioskId)
	{
		return message.AddKioskIdHeader(kioskId.ToString());
	}

	public static HttpRequestMessage AddKioskIdHeader(this HttpRequestMessage message, long? kioskId)
	{
		if (message != null && kioskId.HasValue)
		{
			message.AddKioskIdHeader(kioskId.Value);
		}
		return message;
	}

	public static HttpRequestMessage AddKioskIdHeader(this HttpRequestMessage message, string kioskId)
	{
		if (message != null && !string.IsNullOrEmpty(kioskId))
		{
			message.Headers.Add("x-redbox-kioskid", kioskId);
		}
		return message;
	}

	public static List<Header> AddKioskIdHeader(this List<Header> headers, int kioskId)
	{
		return headers.AddKioskIdHeader(kioskId.ToString());
	}

	public static List<Header> AddKioskIdHeader(this List<Header> headers, int? kioskId)
	{
		if (headers != null && kioskId.HasValue)
		{
			headers.AddKioskIdHeader(kioskId.Value);
		}
		return headers;
	}

	public static List<Header> AddKioskIdHeader(this List<Header> headers, long kioskId)
	{
		return headers.AddKioskIdHeader(kioskId.ToString());
	}

	public static List<Header> AddKioskIdHeader(this List<Header> headers, long? kioskId)
	{
		if (headers != null && kioskId.HasValue)
		{
			headers.AddKioskIdHeader(kioskId.Value);
		}
		return headers;
	}

	public static List<Header> AddKioskIdHeader(this List<Header> headers, string kioskId)
	{
		if (headers != null && !string.IsNullOrEmpty(kioskId))
		{
			headers.Add(new Header("x-redbox-kioskid", kioskId));
		}
		return headers;
	}

	public static string ToSecureString(this HttpResponseMessage response)
	{
		if (response.RequestMessage.Headers.TryGetValues("x-api-key", out var values))
		{
			response.RequestMessage.Headers.Remove("x-api-key");
			response.RequestMessage.Headers.Add("x-api-key", values.FirstOrDefault()?.MaskLogValue(4));
		}
		values = null;
		if (response.RequestMessage.Headers.TryGetValues("authorization", out values))
		{
			response.RequestMessage.Headers.Remove("authorization");
			response.RequestMessage.Headers.Add("authorization", values.FirstOrDefault()?.MaskLogValue(4));
		}
		return response.ToJson();
	}

	private static async Task<string> ReadStream(Stream stream)
	{
		string result = "";
		try
		{
			if (stream?.CanSeek ?? false)
			{
				stream.Seek(0L, SeekOrigin.Begin);
				result = await new StreamReader(stream).ReadToEndAsync();
				stream.Seek(0L, SeekOrigin.Begin);
			}
		}
		catch (Exception ex) when (ex is BadHttpRequestException || ex is ObjectDisposedException)
		{
			result = "";
		}
		return result;
	}
}
