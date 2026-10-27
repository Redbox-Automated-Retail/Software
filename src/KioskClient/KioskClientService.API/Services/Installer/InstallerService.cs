using System.Threading.Tasks;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.ProxyApi;
using KioskClientService.API.Services.TestData;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Installer;

public class InstallerService : IInstallerService
{
	private static class Constants
	{
		public class TestData
		{
			public class StoreServices
			{
				public const string LiveCall = "StoreServices.LiveCall";
			}

			public const string CategoryName = "ProxyServiceStoreServices";
		}

		public static class TestValues
		{
			public const string OpsMarket = "UnNamed";

			public const string KaseyaServer = "Kaseya6";

			public static InstallerModels.State TestState1 => new InstallerModels.State
			{
				Id = 1,
				Description = "State1"
			};

			public static InstallerModels.State TestState2 => new InstallerModels.State
			{
				Id = 2,
				Description = "State2"
			};

			public static InstallerModels.Banner TestBanner1 => new InstallerModels.Banner
			{
				Id = 1,
				Name = "Banner1"
			};

			public static InstallerModels.Banner TestBanner2 => new InstallerModels.Banner
			{
				Id = 2,
				Name = "Banner2"
			};

			public static InstallerModels.PendingKiosk TestKiosk1 => new InstallerModels.PendingKiosk
			{
				Id = 1,
				Address = "1 Main street",
				City = "SomePlace",
				State = "SomeState",
				MarketName = "Market1",
				ZipCode = "99999",
				DueTime = "09:00PM"
			};

			public static InstallerModels.PendingKiosk TestKiosk2 => new InstallerModels.PendingKiosk
			{
				Id = 2,
				Address = "2 Main street",
				City = "SomePlace",
				State = "SomeState",
				MarketName = "Market1",
				ZipCode = "99999",
				DueTime = "09:00PM"
			};
		}
	}

	private readonly ILogger<InstallerService> _logger;

	private readonly IProxyApi _proxyApi;

	private readonly ITestDataService _testDataService;

	private readonly IKioskConfiguration _kioskConfiguration;

	public InstallerService(ILogger<InstallerService> logger, IProxyApi proxyApi, ITestDataService testDataService, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration)
	{
		_logger = logger;
		_proxyApi = proxyApi;
		_testDataService = testDataService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
	}

	public async Task<InstallerModels.OpsMarketResponse> GetOpsMarket(int kioskid)
	{
		return (!(await GetUseTestData("StoreServices.LiveCall"))) ? (await _proxyApi.GetOpsMarket(kioskid))?.Response : GetTestDataOpsMarketResponse();
	}

	private InstallerModels.OpsMarketResponse GetTestDataOpsMarketResponse()
	{
		_logger?.LogInformation("returning test data result");
		InstallerModels.OpsMarketResponse opsMarketResponse = new InstallerModels.OpsMarketResponse();
		opsMarketResponse.OpsMarket = "UnNamed";
		opsMarketResponse.KaseyaServer = "Kaseya6";
		opsMarketResponse.Success = true;
		return opsMarketResponse;
	}

	public async Task<InstallerModels.PendingKiosksResponse> GetPendingKiosks(short stateid, int bannerId)
	{
		return (!(await GetUseTestData("StoreServices.LiveCall"))) ? (await _proxyApi.GetPendingKiosks(stateid, bannerId))?.Response : GetTestDataPendingKiosksResponse();
	}

	private InstallerModels.PendingKiosksResponse GetTestDataPendingKiosksResponse()
	{
		_logger?.LogInformation("returning test data result");
		InstallerModels.PendingKiosksResponse pendingKiosksResponse = new InstallerModels.PendingKiosksResponse();
		pendingKiosksResponse.PendingKiosks.Add(Constants.TestValues.TestKiosk1);
		pendingKiosksResponse.PendingKiosks.Add(Constants.TestValues.TestKiosk2);
		pendingKiosksResponse.Success = true;
		return pendingKiosksResponse;
	}

	public async Task<InstallerModels.PendingBannersResponse> GetPendingBanners(int stateid)
	{
		return (!(await GetUseTestData("StoreServices.LiveCall"))) ? (await _proxyApi.GetPendingBanners(stateid))?.Response : GetTestDataPendingBannersResponse();
	}

	private InstallerModels.PendingBannersResponse GetTestDataPendingBannersResponse()
	{
		_logger?.LogInformation("returning test data result");
		InstallerModels.PendingBannersResponse pendingBannersResponse = new InstallerModels.PendingBannersResponse();
		pendingBannersResponse.Banners.Add(Constants.TestValues.TestBanner1);
		pendingBannersResponse.Banners.Add(Constants.TestValues.TestBanner2);
		pendingBannersResponse.Success = true;
		return pendingBannersResponse;
	}

	public async Task<InstallerModels.PendingStatesResponse> GetPendingStates()
	{
		return (!(await GetUseTestData("StoreServices.LiveCall"))) ? (await _proxyApi.GetPendingStates())?.Response : GetTestDataPendingStateResponse();
	}

	private InstallerModels.PendingStatesResponse GetTestDataPendingStateResponse()
	{
		_logger?.LogInformation("returning test data result");
		InstallerModels.PendingStatesResponse pendingStatesResponse = new InstallerModels.PendingStatesResponse();
		pendingStatesResponse.States.Add(Constants.TestValues.TestState1);
		pendingStatesResponse.States.Add(Constants.TestValues.TestState2);
		pendingStatesResponse.Success = true;
		return pendingStatesResponse;
	}

	private async Task<bool> GetUseTestData(string liveCallPropertyName)
	{
		bool result = false;
		if (_kioskConfiguration?.TestData?.TestDataEnabled == true)
		{
			ReadTestDataResponse readTestDataResponse = await (_testDataService?.ReadTestDataValueAsync("ProxyServiceStoreServices", liveCallPropertyName));
			if (readTestDataResponse != null && readTestDataResponse.Success && readTestDataResponse.GetPropertyValue<bool?>() == false)
			{
				result = true;
			}
		}
		return result;
	}
}
