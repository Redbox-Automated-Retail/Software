using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Swashbuckle.Application;

namespace Redbox.KioskEngine.API;

public class APIService : IApiService
{
	public void Initialize()
	{
		string text = ServiceLocator.Instance?.GetService<IConfiguration>()?.KioskEngineApiServiceBaseUrl;
		LogHelper.Instance.Log("Starting APIService: " + text);
		HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(text);
		config.MapHttpAttributeRoutes();
		config.EnableSwagger(delegate(SwaggerDocsConfig c)
		{
			c.SingleApiVersion("v1", "Redbox.KioskEngine.API");
			c.DescribeAllEnumsAsStrings();
			string text2 = Path.ChangeExtension(Assembly.GetExecutingAssembly()?.Location, ".xml");
			if (File.Exists(text2))
			{
				c.IncludeXmlComments(text2);
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
}
