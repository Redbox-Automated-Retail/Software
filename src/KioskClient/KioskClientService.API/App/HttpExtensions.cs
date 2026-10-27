using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace KioskClientService.API.App;

public static class HttpExtensions
{
	public static HttpContent ToStandardPayloadContent(this object source)
	{
		return new StringContent(JsonConvert.SerializeObject(source, Formatting.None, new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		}), Encoding.UTF8, "application/json");
	}

	public static HttpContent ToGZipPayloadContent(this object source)
	{
		return new CompressedContent(source.ToStandardPayloadContent(), "gzip");
	}

	public static HttpContent ToDeflatePayloadContent(this object source)
	{
		return new CompressedContent(source.ToStandardPayloadContent(), "deflate");
	}

	public static IServiceCollection AddHttpService(this IServiceCollection services)
	{
		services.AddHttpClient<IHttpService, HttpService>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
		{
			AllowAutoRedirect = true,
			UseCookies = true,
			CookieContainer = new CookieContainer(),
			AutomaticDecompression = DecompressionMethods.GZip
		});
		return services;
	}

	public static void AddCompressedRequestPayloadMiddleware(this IApplicationBuilder app)
	{
	}

	public static void AddUpdateMerchandizingMiddleware(this IApplicationBuilder app)
	{
		app.UseMiddleware<UpdateMerchandizingStatusMiddleware>(Array.Empty<object>());
	}

	public static string ToQueryString(this Dictionary<string, string> parameters)
	{
		string text = string.Empty;
		foreach (KeyValuePair<string, string> parameter in parameters)
		{
			text += ((parameter.Value != null) ? (((text.Length == 0) ? "?" : "&") + parameter.Key + "=" + parameter.Value) : string.Empty);
		}
		return text;
	}

	public static string ToQueryString<T>(this List<T> parameters, string parameterName, string existingQueryString = null)
	{
		string text = existingQueryString ?? string.Empty;
		foreach (T parameter in parameters)
		{
			text += ((parameter != null) ? string.Format("{0}{1}={2}", (text.Length == 0) ? "?" : "&", parameterName, parameter) : string.Empty);
		}
		return text;
	}
}
