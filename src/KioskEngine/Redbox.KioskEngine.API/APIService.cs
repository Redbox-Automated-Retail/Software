#if NETFRAMEWORK
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Swashbuckle.Application;
#else
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
#endif

namespace Redbox.KioskEngine.API;

public class APIService : IApiService
{
#if NETFRAMEWORK
	public void Initialize()
	{
		string? text = ServiceLocator.Instance?.GetService<IConfiguration>()?.KioskEngineApiServiceBaseUrl;
		LogHelper.Instance.Log("Starting APIService: " + text);
		HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(text);
		config.MapHttpAttributeRoutes();
		config.EnableSwagger(delegate(SwaggerDocsConfig c)
		{
			c.SingleApiVersion("v1", "Redbox.KioskEngine.API");
			c.DescribeAllEnumsAsStrings();
			string xmlDocPath = Path.ChangeExtension(Assembly.GetExecutingAssembly()?.Location, ".xml");
			if (File.Exists(xmlDocPath))
			{
				c.IncludeXmlComments(xmlDocPath);
			}
		}).EnableSwaggerUi(delegate(SwaggerUiConfig x)
		{
			x.DisableValidator();
			x.DocExpansion(DocExpansion.List);
		});
		Task.Run(async delegate
		{
			await new HttpSelfHostServer(config).OpenAsync();
			LogHelper.Instance.Log("API Service started successfully");
		});
	}
#else
	// .NET 10: ASP.NET Core (Kestrel) in the engine process instead of Web API's WCF self-host. Same URL, routes and
	// JSON (Compat/WebApiCompat.cs); Swagger at /swagger instead of /swagger/ui/index.
	private WebApplication m_app = null!; // set by Initialize

	public void Initialize()
	{
		string? text = ServiceLocator.Instance?.GetService<IConfiguration>()?.KioskEngineApiServiceBaseUrl;
		LogHelper.Instance.Log("Starting APIService: " + text);
		WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = AppContext.BaseDirectory });
		builder.Logging.ClearProviders();
		builder.WebHost.UseUrls(text!);
		builder.Services.AddControllers(delegate(Microsoft.AspNetCore.Mvc.MvcOptions options)
		{
			options.OutputFormatters.Insert(0, new HttpResponseMessageOutputFormatter());
			options.Conventions.Add(new WebApiParameterBindingConvention());
			// Nullable reference types must not make non-nullable model properties [Required]: binding stays as before nullable was turned on.
			options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
		}).AddApplicationPart(typeof(APIService).Assembly).AddNewtonsoftJson(delegate(Microsoft.AspNetCore.Mvc.MvcNewtonsoftJsonOptions options)
		{
			// Web API's JsonMediaTypeFormatter used member names as they are (no camelCase).
			options.SerializerSettings.ContractResolver = new DefaultContractResolver();
		});
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();
		m_app = builder.Build();
		m_app.UseSwagger();
		m_app.UseSwaggerUI();
		m_app.MapControllers();
		Task.Run(async delegate
		{
			try
			{
				await m_app.StartAsync();
				LogHelper.Instance.Log("API Service started successfully");
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("API Service failed to start.", e);
			}
		});
	}
#endif
}