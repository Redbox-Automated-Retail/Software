using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.ProxyApi;
using KioskClientService.API.Services.TestData;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
	private class Constants
	{
		public enum AuthenticationResponse
		{
			Valid,
			MissingKiosk,
			MissingUser,
			MarketAccessDenied,
			AuthenticationFailed,
			Error,
			AccountLocked
		}

		public class TestData
		{
			public class Authentication
			{
				public const string LiveCall = "Authentication.LiveCall";

				public const string SimulateCommunicationsFailure = "Authentication.SimulateCommunicationsFailure";

				public const string UserAuthenticationResponses = "Authentication.UserAuthenticationResponses";
			}

			public const string CategoryName = "ProxyServiceAuthentication";
		}
	}

	private class UserAuthenticationResponse
	{
		public string UserName { get; set; }

		public string AuthenticationResponse { get; set; }
	}

	private readonly ILogger<IAuthenticationService> _logger;

	private readonly IProxyApi _proxyApi;

	private readonly ITestDataService _testDataService;

	private readonly IKioskConfiguration _kioskConfiguration;

	public AuthenticationService(ILogger<IAuthenticationService> logger, IProxyApi proxyApi, ITestDataService testDataService, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration)
	{
		_logger = logger;
		_proxyApi = proxyApi;
		_testDataService = testDataService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
	}

	public async Task<KioskModels.KioskAuthenticateResponse> Authenticate(KioskModels.KioskAuthenticateRequest request)
	{
		return (!(await GetUseTestData("Authentication.LiveCall"))) ? (await _proxyApi.Authenticate(request))?.Response : (await GetTestDataAuthenticationResponse(request));
	}

	private async Task<KioskModels.KioskAuthenticateResponse> GetTestDataAuthenticationResponse(KioskModels.KioskAuthenticateRequest request)
	{
		_logger?.LogInformation("returning test data result");
		KioskModels.KioskAuthenticateResponse response = new KioskModels.KioskAuthenticateResponse();
		if (bool.TryParse(await GetTestDataValue("Authentication.SimulateCommunicationsFailure"), out var result) && result)
		{
			return null;
		}
		List<UserAuthenticationResponse> userAuthenticationReponses = null;
		string value = await GetTestDataValue("Authentication.UserAuthenticationResponses");
		if (!string.IsNullOrEmpty(value))
		{
			try
			{
				userAuthenticationReponses = JsonConvert.DeserializeObject<List<UserAuthenticationResponse>>(value);
			}
			catch
			{
			}
		}
		UserAuthenticationResponse userAuthenticationResponse = null;
		if ((userAuthenticationReponses?.Any() ?? false) && !string.IsNullOrEmpty(request.Username))
		{
			userAuthenticationResponse = userAuthenticationReponses.FirstOrDefault((UserAuthenticationResponse x) => x.UserName == request.Username);
		}
		if (userAuthenticationResponse != null)
		{
			if (userAuthenticationResponse.AuthenticationResponse != Constants.AuthenticationResponse.Valid.ToString())
			{
				response.Errors.Add(new Error
				{
					Code = userAuthenticationResponse.AuthenticationResponse
				});
			}
		}
		else
		{
			response.Errors.Add(new Error
			{
				Code = Constants.AuthenticationResponse.MissingUser.ToString()
			});
		}
		List<Error> errors = response.Errors;
		response.Success = errors == null || !errors.Any();
		return response;
	}

	private async Task<string> GetTestDataValue(string propertyName)
	{
		return (await (_testDataService?.ReadTestDataValueAsync("ProxyServiceAuthentication", propertyName)))?.PropertyValue;
	}

	private async Task<bool> GetUseTestData(string liveCallPropertyName)
	{
		bool result = false;
		if (_kioskConfiguration?.TestData?.TestDataEnabled == true)
		{
			ReadTestDataResponse readTestDataResponse = await (_testDataService?.ReadTestDataValueAsync("ProxyServiceAuthentication", liveCallPropertyName));
			if (readTestDataResponse != null && readTestDataResponse.Success && readTestDataResponse.GetPropertyValue<bool?>() == false)
			{
				result = true;
			}
		}
		return result;
	}
}
