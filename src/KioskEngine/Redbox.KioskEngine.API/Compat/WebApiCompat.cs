// Written for the modernize branch (not decompiled). Compiled for .NET 10 only.
//
// The engine's API controllers were written for ASP.NET Web API 5 (self-hosted on WCF), which does not exist on
// .NET 10. Instead of rewriting them, this file gives them the few Web API types they use, on top of ASP.NET Core:
// ApiController with Request.CreateResponse(...) and Configuration.Formatters.JsonFormatter, [HttpGet]/[HttpPost]/
// [Route], and Swashbuckle 5's [SwaggerResponse]. Responses are built with the same System.Net.Http.Formatting
// JsonMediaTypeFormatter that Web API used (package Microsoft.AspNet.WebApi.Client), so the JSON is the same.
#if !NETFRAMEWORK
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace System.Web.Http
{
	public abstract class ApiController : Microsoft.AspNetCore.Mvc.ControllerBase
	{
		private static readonly HttpConfiguration SharedConfiguration = new HttpConfiguration();

		// Web API's ApiController.Request is the HttpRequestMessage; the controllers only use it to create responses.
		public new HttpRequestMessage Request => new HttpRequestMessage();

		public HttpConfiguration Configuration => SharedConfiguration;
	}

	public sealed class HttpConfiguration
	{
		public MediaTypeFormatterCollection Formatters { get; } = new MediaTypeFormatterCollection();
	}

	public sealed class HttpGetAttribute : Microsoft.AspNetCore.Mvc.HttpGetAttribute
	{
	}

	public sealed class HttpPostAttribute : Microsoft.AspNetCore.Mvc.HttpPostAttribute
	{
	}

	public sealed class RouteAttribute : Microsoft.AspNetCore.Mvc.RouteAttribute
	{
		public RouteAttribute(string template)
			: base(template)
		{
		}
	}
}

namespace System.Net.Http
{
	public static class HttpRequestMessageCompatExtensions
	{
		// Web API: Request.CreateResponse(statusCode, value, formatter) = an ObjectContent with that formatter.
		public static HttpResponseMessage CreateResponse<T>(this HttpRequestMessage request, HttpStatusCode statusCode, T value, MediaTypeFormatter formatter)
		{
			return new HttpResponseMessage(statusCode)
			{
				Content = new ObjectContent<T>(value, formatter),
				RequestMessage = request
			};
		}
	}
}

namespace Swashbuckle.Swagger.Annotations
{
	// Swashbuckle 5 documentation attribute; documentation only, so a no-op here.
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public sealed class SwaggerResponseAttribute : Attribute
	{
		public SwaggerResponseAttribute(HttpStatusCode statusCode)
		{
		}

		public SwaggerResponseAttribute(HttpStatusCode statusCode, string? description = null, Type? type = null)
		{
		}

		public string? Description { get; set; }

		public Type? Type { get; set; }
	}
}

namespace Redbox.KioskEngine.API
{
	// Writes the HttpResponseMessage an action returned (status, headers, content) to the ASP.NET Core response.
	internal sealed class HttpResponseMessageOutputFormatter : IOutputFormatter
	{
		public bool CanWriteResult(OutputFormatterCanWriteContext context)
		{
			return context.Object is HttpResponseMessage;
		}

		public async Task WriteAsync(OutputFormatterWriteContext context)
		{
			HttpResponseMessage message = (HttpResponseMessage)context.Object!; // CanWriteResult accepted it
			Microsoft.AspNetCore.Http.HttpResponse response = context.HttpContext.Response;
			response.StatusCode = (int)message.StatusCode;
			foreach (var header in message.Headers)
			{
				response.Headers[header.Key] = header.Value.ToArray();
			}
			if (message.Content != null)
			{
				byte[] body = await message.Content.ReadAsByteArrayAsync();
				foreach (var header in message.Content.Headers)
				{
					response.Headers[header.Key] = header.Value.ToArray();
				}
				response.ContentLength = body.Length;
				await response.Body.WriteAsync(body, 0, body.Length);
			}
		}
	}

	// Web API bound complex parameters from the body and simple ones from the URI; ASP.NET Core without
	// [ApiController] would bind complex ones from the query/form instead.
	internal sealed class WebApiParameterBindingConvention : IActionModelConvention
	{
		public void Apply(ActionModel action)
		{
			foreach (ParameterModel parameter in action.Parameters)
			{
				if (parameter.BindingInfo?.BindingSource != null)
				{
					continue;
				}
				Type type = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;
				bool simple = type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan);
				if (!simple)
				{
					// Simple types keep ASP.NET Core's default (route values, then the query string), as in Web API.
					parameter.BindingInfo ??= new BindingInfo();
					parameter.BindingInfo.BindingSource = BindingSource.Body;
				}
			}
		}
	}
}
#endif
