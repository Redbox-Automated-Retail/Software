using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Profile;
using Swashbuckle.Swagger.Annotations;

namespace Redbox.KioskEngine.API.Controllers.PreferencesManager;

public class EngineCoreController : ApiController
{
	[HttpGet]
	[Route("api/enginecore/getkioskengineversions")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(EngineCoreKioskEngineVersionResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetVersions()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		EngineCoreKioskEngineVersionResponse response = new EngineCoreKioskEngineVersionResponse();
		await Task.Run(delegate
		{
			KioskEngineVersions kioskEngineVersions = response.Content;
			AppDomain.CurrentDomain.GetAssemblies().ForEach(delegate(Assembly eachAssembly)
			{
				AssemblyName assemblyName = eachAssembly?.GetName();
				if (assemblyName?.Name?.ToUpper()?.StartsWith("REDBOX") == true || assemblyName?.Name?.ToUpper() == "KIOSKENGINE")
				{
					kioskEngineVersions.Assemblies.Add(new AssemblyInfo
					{
						Name = assemblyName.Name,
						Version = assemblyName.Version?.ToString()
					});
				}
			});
			kioskEngineVersions.Assemblies.Sort((AssemblyInfo a, AssemblyInfo b) => a.Name.CompareTo(b.Name));
			ServiceLocator.Instance.GetService<IResourceBundleService>()?.Bundles?.ForEach(delegate(IBundleSpecifier eachBundle)
			{
				kioskEngineVersions.Bundles.Add(new BundleInfo
				{
					Name = eachBundle?.Name,
					Version = eachBundle?.Version?.ToString()
				});
			});
			kioskEngineVersions.Bundles.Sort((BundleInfo a, BundleInfo b) => a.Name.CompareTo(b.Name));
			LogHelper.Instance.Log("EngineCoreController.GetVersions result=" + JsonConvert.SerializeObject(response.Content));
		});
		response.SetResponseTime((int)stopwatch.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpGet]
	[Route("api/enginecore/getstoreinfo")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(GetStoreInfoResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetStoreInfo()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GetStoreInfoResponse response = new GetStoreInfoResponse();
		await Task.Run(delegate
		{
			Store store = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
			if (store != null)
			{
				response.Content = new StoreInfo
				{
					Address = store.Address,
					Address1 = store.Address1,
					Address2 = store.Address2,
					BannerId = store.BannerId,
					City = store.City,
					CollectionMethod = store.CollectionMethod,
					County = store.County,
					DigitalPurchaseTaxRate = store.DigitalPurchaseTaxRate,
					InvSync = store.InvSync,
					Market = new MarketInfo
					{
						MarketId = (store.Market?.MarketId ?? 0),
						MarketName = store.Market?.MarketName
					},
					MarketId = store.MarketId,
					OpenDate = store.OpenDate,
					PurchasePrice = store.PurchasePrice,
					PurchaseTaxRate = store.PurchaseTaxRate,
					RentalTaxRate = store.RentalTaxRate,
					ReturnOnlyModeDate = store.ReturnOnlyModeDate,
					ReturnTime = store.ReturnTime,
					Sellthru = store.Sellthru,
					ServiceFee = store.ServiceFee,
					ServiceFeeTaxRate = store.ServiceFeeTaxRate,
					State = store.State,
					StoreId = store.StoreId,
					VendorId = store.VendorId,
					Zip = store.Zip
				};
				LogHelper.Instance.Log("EngineCoreController.GetStoreInfo result=" + JsonConvert.SerializeObject(response.Content));
			}
		});
		response.SetResponseTime((int)stopwatch.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpGet]
	[Route("api/enginecore/getpreferencevalue")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(GetPreferenceValueResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetPreferenceValue(string pageName, string settingName)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GetPreferenceValueResponse response = new GetPreferenceValueResponse();
		await Task.Run(delegate
		{
			string text = ServiceLocator.Instance.GetService<IPreferenceService>()?.GetValue<string>(pageName, settingName);
			response.Content = text;
			LogHelper.Instance.Log("EngineCoreController.GetPreferenceValue pageName=" + pageName + ", settingName=" + settingName + ", result=" + text);
		});
		response.SetResponseTime((int)stopwatch.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpGet]
	[Route("api/enginecore/machinesettingvalue")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(MachineSettingValueResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetMachineSettingValue(string settingPath, string settingName)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		MachineSettingValueResponse response = new MachineSettingValueResponse
		{
			SettingPath = settingPath,
			SettingName = settingName
		};
		await Task.Run(delegate
		{
			string text = ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue<string>(settingPath, settingName);
			response.Content = text;
			LogHelper.Instance.Log("EngineCoreController.GetMachineSettingValue settingPath=" + settingPath + ", settingName=" + settingName + ", result=" + text);
		});
		response.SetResponseTime((int)stopwatch.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpPost]
	[Route("api/enginecore/machinesettingvalue")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(MachineSettingValueResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> SetMachineSettingValue(string settingPath, string settingName, string value)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		MachineSettingValueResponse response = new MachineSettingValueResponse
		{
			SettingPath = settingPath,
			SettingName = settingName
		};
		await Task.Run(delegate
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue(settingPath, settingName, value);
			response.Content = value;
			LogHelper.Instance.Log("EngineCoreController.SetMachineSettingValue settingPath=" + settingPath + ", settingName=" + settingName + ", value=" + value);
		});
		response.SetResponseTime((int)stopwatch.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpGet]
	[Route("api/enginecore/applicationstatevalue")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApplicationStateValueResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetApplicationStateValue(string settingName)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		ApplicationStateValueResponse response = new ApplicationStateValueResponse
		{
			SettingName = settingName
		};
		await Task.Run(delegate
		{
			IApplicationState service = ServiceLocator.Instance.GetService<IApplicationState>();
			if (settingName == "IsStandAlone")
			{
				response.Content = service.IsStandAlone.ToString();
			}
			LogHelper.Instance.Log("EngineCoreController.GetApplicationStateValue settingName=" + settingName + ", result=" + response.Content);
		});
		response.SetResponseTime((int)stopwatch.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpPost]
	[Route("api/enginecore/applicationstatevalue")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApplicationStateValueResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> SetApplicationStateValue(string settingName, string value)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		ApplicationStateValueResponse response = new ApplicationStateValueResponse
		{
			SettingName = settingName
		};
		await Task.Run(delegate
		{
			IApplicationState service = ServiceLocator.Instance.GetService<IApplicationState>();
			if (settingName == "IsStandAlone" && bool.TryParse(value, out var result))
			{
				service.IsStandAlone = result;
				response.Content = service.IsStandAlone.ToString();
			}
			LogHelper.Instance.Log("EngineCoreController.SetApplicationStateValue settingName=" + settingName + ", value=" + response.Content);
		});
		response.SetResponseTime((int)stopwatch.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}
}
