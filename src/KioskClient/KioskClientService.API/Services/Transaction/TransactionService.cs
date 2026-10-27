using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.TestData;
using KioskClientService.API.Services.Transaction.API;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Transaction;

public class TransactionService : ITransactionService
{
	private class Constants
	{
		public class AuthorizeErrorCodes
		{
			public const string CheckoutApiError = "(A030)";

			public const string MissingOrExtraSubscriptionLineItem = "(A031)";

			public const string CardCPDifferentFromSubCP = "(A032)";

			public const string SubscriptionNotProcessedDueToAuthFailure = "(A033)";

			public const string InvalidCPServiceResponse = "(A035)";

			public const string CPServiceApiError = "(A036)";

			public const string InventoryServiceApiError = "(A037)";

			public const string CustomerBlockListed = "(A038)";

			public const string AuthorizeProcessingError = "(A100)";

			public const string MerchantServiceConnectionError = "(A101)";

			public const string KioskDatConnectionError = "(A102)";

			public const string CPServiceConnectionError = "(A203)";

			public const string PromoServiceConnectionError = "(A104)";

			public const string CheckoutApiConnectionError = "(A105)";

			public const string InventoryServiceConnectionError = "(A106)";

			public const string InvalidCheckoutApiResponse = "(A034)";
		}

		public class TestData
		{
			public class Authorize
			{
				public const string LiveCall = "Authorize.LiveCall";

				public const string SimulateCommunicationFailure = "Authorize.SimulateCommunicationFailure";

				public const string SimulateServerBusy = "Authorize.SimulateServerBusy";

				public const string ErrorCode = "Authorize.ErrorCode";

				public const string AccountNumber = "Authorize.AccountNumber";

				public const string HasCustomerProfile = "Authorize.HasCustomerProfile";

				public const string CustomerProfileNumber = "Authorize.CustomerProfileNumber";

				public const string CustomerProfileFirstName = "Authorize.CustomerProfileFirstName";

				public const string CustomerProfileLastName = "Authorize.CustomerProfileLastName";

				public const string Email = "Authorize.Email";

				public const string ReferenceNumber = "Authorize.ReferenceNumber";

				public const string ConfirmationStatus = "Authorize.ConfirmationStatus";
			}

			public class GCAuth
			{
				public const string LiveCall = "GCAuth.LiveCall";

				public const string SimulateCommunicationFailure = "GCAuth.SimulateCommunicationFailure";

				public const string SimulateServerBusy = "GCAuth.SimulateServerBusy";

				public const string ErrorCode = "GCAuth.ErrorCode";

				public const string GCResponseType = "GCAuth.GCResponseType";

				public const string Exists = "GCAuth.Exists";

				public const string GCStatusType = "GCAuth.GCStatusType";

				public const string Balance = "GCAuth.Balance";

				public const string MaxDiscs = "GCAuth.MaxDiscs";
			}

			public class AfterSwipeCustomerOffers
			{
				public enum Offers
				{
					NoOffer,
					InCartOffers,
					FracFreeMovieFirstVisit,
					FracFreeMovieSecondVisit,
					FracDiscountedDiscFirstVisit,
					FracDiscountedDiscSecondVisit,
					FreeItemPerksEquivalent,
					AmountOffPerksEquivalent,
					ExtraPointsPerksEquivalent,
					PerksMessage
				}

				public const string LiveCall = "AfterSwipeCustomerOffers.LiveCall";

				public const string Success = "AfterSwipeCustomerOffers.Success";

				public const string SelectedOffer = "AfterSwipeCustomerOffers.SelectedOffer";

				public const string IncludeMovieInCartOffer = "AfterSwipeCustomerOffers.IncludeMovieInCartOffer";

				public const string IncludeFreeGameInCartOffer = "AfterSwipeCustomerOffers.IncludeFreeGameInCartOffer";

				public const string IncludeAmountOffPurchaseInCartOffer = "AfterSwipeCustomerOffers.IncludeAmountOffPurchaseInCartOffer";

				public const string IncludeMultiNightPriceInCartOffer = "AfterSwipeCustomerOffers.IncludeMultiNightPriceInCartOffer";

				public const string MultiNightPriceProductIds = "AfterSwipeCustomerOffers.MultiNightPriceProductIds";
			}

			public const string CategoryName = "TransactionService";
		}
	}

	private const int _cacheExpiry = 30;

	private readonly ITransactionApiClient _transactionClient;

	private readonly IMemoryCache _cache;

	private readonly ILogger<ITransactionService> _logger;

	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly ITestDataService _testDataService;

	private readonly string _promoCheckCachePrefix = "[RPC]";

	private readonly TimeSpan _promoCheckCacheMinutes = TimeSpan.FromMinutes(30.0);

	private static readonly List<string> _subscriptionErrorCodes = new List<string> { "(A030)", "(A031)", "(A033)", "(A034)" };

	public TransactionService(IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, ITestDataService testDataService, IMemoryCache cache, ITransactionApiClient client, ILogger<ITransactionService> logger)
	{
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_testDataService = testDataService;
		_transactionClient = client;
		_cache = cache;
		_logger = logger;
	}

	public async Task<AuthorizeResponse> Authorize(AuthorizeRequest request)
	{
		AuthorizeResponse authorizeResponse;
		if (await GetUseTestData("Authorize.LiveCall"))
		{
			authorizeResponse = await GetTestDataAuthorizeResponse(request);
		}
		else
		{
			APIResponse<AuthorizeResponse> aPIResponse = await _transactionClient.Authorize(request);
			CheckForAuthOfflineError(aPIResponse);
			authorizeResponse = aPIResponse.Response;
			if (authorizeResponse == null || !authorizeResponse.Success)
			{
				_logger.LogErrorWithSource(aPIResponse.Exception, "Authorize was unsuccessful.", "Authorize");
				LogResponseErrors(aPIResponse);
			}
		}
		return authorizeResponse;
	}

	private void CheckForAuthOfflineError<T>(APIResponse<T> response) where T : AuthorizeResponse
	{
		if (response.StatusCode == HttpStatusCode.InternalServerError && response.Response != null)
		{
			response.Response.Errors.Add(new Error
			{
				Code = "(A100)",
				Message = "Server Error"
			});
			response.Response.Success = false;
		}
	}

	public async Task<GCAuthResponse> GiftCardAuthorize(GCAuthRequest request)
	{
		GCAuthResponse gCAuthResponse;
		if (await GetUseTestData("GCAuth.LiveCall"))
		{
			gCAuthResponse = await GetTestDataGCAuthorizeResponse(request);
		}
		else
		{
			APIResponse<GCAuthResponse> aPIResponse = await _transactionClient.GiftCardAuthorize(request);
			CheckForAuthOfflineError(aPIResponse);
			gCAuthResponse = aPIResponse.Response;
			if (gCAuthResponse == null || !gCAuthResponse.Success)
			{
				_logger.LogErrorWithSource(aPIResponse.Exception, "GCAuthorize was unsuccessful.", "GiftCardAuthorize");
				LogResponseErrors(aPIResponse);
			}
		}
		return gCAuthResponse;
	}

	public async Task<AuthorizeResponse> PickupAuthorize(PickupAuthorizeRequest request)
	{
		AuthorizeResponse authorizeResponse;
		if (await GetUseTestData("Authorize.LiveCall"))
		{
			authorizeResponse = await GetTestDataAuthorizeResponse(request);
		}
		else
		{
			APIResponse<AuthorizeResponse> aPIResponse = await _transactionClient.PickupAuthorize(request);
			CheckForAuthOfflineError(aPIResponse);
			authorizeResponse = aPIResponse.Response;
			if (authorizeResponse == null || !authorizeResponse.Success)
			{
				_logger.LogErrorWithSource(aPIResponse.Exception, "PickupAuthorize was unsuccessful.", "PickupAuthorize");
				LogResponseErrors(aPIResponse);
			}
		}
		return authorizeResponse;
	}

	public async Task<AuthorizeResponse> ReservationAuthorize(ResvAuthRequest request)
	{
		AuthorizeResponse authorizeResponse;
		if (await GetUseTestData("Authorize.LiveCall"))
		{
			authorizeResponse = await GetTestDataAuthorizeResponse(request);
		}
		else
		{
			APIResponse<AuthorizeResponse> aPIResponse = await _transactionClient.ReservationAuthorize(request);
			CheckForAuthOfflineError(aPIResponse);
			authorizeResponse = aPIResponse.Response;
			if (authorizeResponse == null || !authorizeResponse.Success)
			{
				_logger.LogErrorWithSource(aPIResponse.Exception, "ReservationAuthorize was unsuccessful.", "ReservationAuthorize");
				LogResponseErrors(aPIResponse);
			}
		}
		return authorizeResponse;
	}

	public async Task<APIResponse<CancelReservationResponse>> CancelReservation(CancelReservationRequest request)
	{
		APIResponse<CancelReservationResponse> aPIResponse = await _transactionClient.CancelReservation(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "CancelReservation was unsuccessful.", "CancelReservation");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<APIResponse<ExpireReservationResponse>> ExpireReservation(ExpireReservationRequest request)
	{
		APIResponse<ExpireReservationResponse> aPIResponse = await _transactionClient.ExpireReservation(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "ExpireReservation was unsuccessful.", "ExpireReservation");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<APIResponse<ReturnResponse>> Return(ReturnRequest request)
	{
		APIResponse<ReturnResponse> aPIResponse = await _transactionClient.Return(request);
		if (aPIResponse?.Response?.Success != true)
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "Return was unsuccessful.", "Return");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<APIResponse<ReconcileResponse>> Reconcile(ReconcileRequest request)
	{
		APIResponse<ReconcileResponse> aPIResponse = await _transactionClient.Reconcile(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "Reconcile was unsuccessful.", "Reconcile");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<APIResponse<ReconcileResponse>> ReservationPickup(ReservationPickupRequest request)
	{
		APIResponse<ReconcileResponse> aPIResponse = await _transactionClient.ReservationPickup(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "ReservationPickup was unsuccessful.", "ReservationPickup");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<APIResponse<PickupValidationResponse>> PickupValidation(PickupValidationRequest request)
	{
		APIResponse<PickupValidationResponse> aPIResponse = await _transactionClient.PickupValidation(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "PickupValidation was unsuccessful.", "PickupValidation");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	private void LogResponseErrors<T>(APIResponse<T> response)
	{
		if (response?.Errors == null || response.Errors.Count <= 0)
		{
			return;
		}
		foreach (Error error in response.Errors)
		{
			_logger.LogErrorWithSource("Error code " + error.Code + " message " + error.Message, "LogResponseErrors");
		}
	}

	public async Task<APIResponse<GetCustomerOffersResponse>> GetCustomerOffers(GetCustomerOffersRequest request)
	{
		APIResponse<GetCustomerOffersResponse> aPIResponse;
		if (await GetUseTestData("AfterSwipeCustomerOffers.LiveCall"))
		{
			aPIResponse = await GetTestDataGetCustomerOfferResponse(request);
		}
		else
		{
			aPIResponse = await _transactionClient.GetCustomerOffers(request);
			if (!(aPIResponse.Response?.Success ?? false))
			{
				_logger.LogErrorWithSource(aPIResponse.Exception, "GetCustomerOffers was unsuccessful.", "GetCustomerOffers");
				LogResponseErrors(aPIResponse);
			}
		}
		return aPIResponse;
	}

	private async Task<APIResponse<GetCustomerOffersResponse>> GetTestDataGetCustomerOfferResponse(GetCustomerOffersRequest request)
	{
		_logger?.LogInformation("TransactionService.GetCustomerOffers returning test data result");
		GetCustomerOffersResponse getCustomerOffersResponse = new GetCustomerOffersResponse
		{
			NewOffers = new List<CustomerOffer>(),
			StoredOffers = new List<CustomerOffer>(),
			Success = true
		};
		if (bool.TryParse(await GetTestDataValue("AfterSwipeCustomerOffers.Success"), out var result))
		{
			getCustomerOffersResponse.Success = result;
		}
		GetCustomerOffersResponse getCustomerOffersResponse2 = getCustomerOffersResponse;
		getCustomerOffersResponse2.EmailAddress = await GetTestDataValue("Login.LoginEmail");
		switch (ToOffer(await GetTestDataValue("AfterSwipeCustomerOffers.SelectedOffer")))
		{
		case Constants.TestData.AfterSwipeCustomerOffers.Offers.InCartOffers:
			await AddMultiNightPriceInCartOffer(getCustomerOffersResponse);
			await AddMovieInCartOffer(getCustomerOffersResponse);
			await AddPurchaseInCartOffer(getCustomerOffersResponse);
			await AddFreeGameInCartOffer(getCustomerOffersResponse);
			break;
		case Constants.TestData.AfterSwipeCustomerOffers.Offers.FracFreeMovieFirstVisit:
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				OfferId = 1L,
				StoredOfferId = 3L,
				DiscountType = "Free",
				ProductType = "Movies",
				OfferScopeType = "StoredRedeemable",
				IncludeFormatIds = new List<int> { 1, 2, 19 },
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				ExpirationDate = new DateTime(2019, 7, 25),
				OfferDiscountCode = "FracRedeemFree",
				CustomerSegmentType = "Frac",
				CampaignId = 123L
			});
			break;
		case Constants.TestData.AfterSwipeCustomerOffers.Offers.FracFreeMovieSecondVisit:
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				OfferId = 5L,
				DiscountType = "Free",
				ProductType = "Movies",
				OfferScopeType = "Storeable",
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				ExpirationDate = new DateTime(2019, 7, 25),
				OfferDiscountCode = "FracStorableFree",
				CustomerSegmentType = "Frac",
				CampaignId = 123L
			});
			break;
		case Constants.TestData.AfterSwipeCustomerOffers.Offers.FracDiscountedDiscFirstVisit:
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				OfferId = 2L,
				StoredOfferId = 4L,
				DiscountType = "AmountOff",
				DiscountAmount = 1.25m,
				ProductType = "All",
				OfferScopeType = "StoredRedeemable",
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				ExpirationDate = new DateTime(2019, 7, 5),
				CustomerSegmentType = "Frac",
				OfferDiscountCode = "FracRedeemDisc",
				CampaignId = 123L
			});
			break;
		case Constants.TestData.AfterSwipeCustomerOffers.Offers.FracDiscountedDiscSecondVisit:
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				OfferId = 6L,
				DiscountType = "AmountOff",
				DiscountAmount = 1.25m,
				ProductType = "All",
				OfferScopeType = "Storeable",
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				ExpirationDate = new DateTime(2019, 7, 5),
				PromotionCode = null,
				OfferDiscountCode = "FracStorableDicount",
				CustomerSegmentType = "Frac",
				CampaignId = 123L
			});
			break;
		case Constants.TestData.AfterSwipeCustomerOffers.Offers.FreeItemPerksEquivalent:
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				OfferId = 7L,
				DiscountType = "Free",
				ProductType = "All",
				OfferScopeType = "Session",
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				ExpirationDate = new DateTime(2019, 7, 5),
				CustomerSegmentType = "NewMemberPerksSignup",
				OfferDiscountCode = "PerksFreeRental",
				CampaignId = 123L
			});
			break;
		case Constants.TestData.AfterSwipeCustomerOffers.Offers.AmountOffPerksEquivalent:
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				OfferId = 7L,
				DiscountType = "AmountOff",
				DiscountAmount = 1.00m,
				ProductType = "All",
				OfferScopeType = "Session",
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				ExpirationDate = new DateTime(2019, 11, 5),
				CustomerSegmentType = "NewMemberPerksSignup",
				OfferDiscountCode = "PerksAmountOff",
				CampaignId = 123L
			});
			break;
		case Constants.TestData.AfterSwipeCustomerOffers.Offers.ExtraPointsPerksEquivalent:
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				OfferId = 7L,
				DiscountType = "Loyalty",
				DiscountAmount = 1500m,
				ProductType = "All",
				OfferScopeType = "Session",
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				ExpirationDate = new DateTime(2019, 11, 5),
				CustomerSegmentType = "NewMemberPerksSignup",
				OfferDiscountCode = "PerksExtraDVDPoints",
				CampaignId = 123L
			});
			break;
		case Constants.TestData.AfterSwipeCustomerOffers.Offers.PerksMessage:
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				OfferId = 0L,
				DiscountType = "Treatment",
				ProductType = "All",
				OfferScopeType = "Session",
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				ExpirationDate = new DateTime(2019, 11, 5),
				CustomerSegmentType = "NewMemberPerksSignup",
				OfferDiscountCode = "NoOffer",
				CampaignId = 123L
			});
			break;
		}
		APIResponse<GetCustomerOffersResponse> aPIResponse = new APIResponse<GetCustomerOffersResponse>(null, _logger);
		aPIResponse.Response = getCustomerOffersResponse;
		aPIResponse.StatusCode = HttpStatusCode.OK;
		return aPIResponse;
	}

	private async Task AddMultiNightPriceInCartOffer(GetCustomerOffersResponse getCustomerOffersResponse)
	{
		if (!bool.TryParse(await GetTestDataValue("AfterSwipeCustomerOffers.IncludeMultiNightPriceInCartOffer"), out var result) || !result)
		{
			return;
		}
		CustomerOffer offer = new CustomerOffer
		{
			OfferId = 1234L,
			DiscountType = "MultiNightPrice",
			DiscountAmount = 0.5m,
			ProductType = "Movies",
			OfferScopeType = "Session",
			ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
			ExpirationDate = new DateTime(2019, 7, 25),
			CustomerSegmentType = "PersonalizedMultiNightPrice",
			OfferProducts = new List<OfferProduct>(),
			OfferDiscountCode = "ExtraNight50c",
			CampaignId = 123L
		};
		string[] array = (await GetTestDataValue("AfterSwipeCustomerOffers.MultiNightPriceProductIds"))?.Replace(" ", null)?.Split(',');
		string[] array2 = array;
		foreach (string s in array2)
		{
			if (long.TryParse(s, out var result2))
			{
				offer.OfferProducts.Add(new OfferProduct
				{
					ProductId = result2,
					DiscountedPrice = 0.5m,
					NumberOfNights = (byte)2
				});
			}
		}
		getCustomerOffersResponse.NewOffers.Add(offer);
	}

	private async Task AddMovieInCartOffer(GetCustomerOffersResponse getCustomerOffersResponse)
	{
		if (bool.TryParse(await GetTestDataValue("AfterSwipeCustomerOffers.IncludeMovieInCartOffer"), out var result) && result)
		{
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				OfferId = 2L,
				DiscountType = "AmountOff",
				DiscountAmount = 1m,
				ProductType = "Movies",
				OfferScopeType = "Session",
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				CustomerSegmentType = "BasketStretch",
				OfferDiscountCode = "MovieDollarOff",
				CampaignId = 123L
			});
		}
	}

	private async Task AddPurchaseInCartOffer(GetCustomerOffersResponse getCustomerOffersResponse)
	{
		if (bool.TryParse(await GetTestDataValue("AfterSwipeCustomerOffers.IncludeAmountOffPurchaseInCartOffer"), out var result) && result)
		{
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				OfferId = 9L,
				DiscountType = "AmountOff",
				DiscountAmount = 1m,
				ProductType = "Movies",
				OfferScopeType = "Session",
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				CustomerSegmentType = "BasketStretch",
				OfferDiscountCode = "PrmDollarOff",
				CampaignId = 1234L,
				ActionCode = "Purchase"
			});
		}
	}

	private async Task AddFreeGameInCartOffer(GetCustomerOffersResponse getCustomerOffersResponse)
	{
		if (bool.TryParse(await GetTestDataValue("AfterSwipeCustomerOffers.IncludeFreeGameInCartOffer"), out var result) && result)
		{
			getCustomerOffersResponse.NewOffers.Add(new CustomerOffer
			{
				DiscountType = "Free",
				ProductType = "Games",
				OfferScopeType = "Session",
				ExcludeIfWithinRedboxReleaseDate = TimeSpan.FromDays(35.0),
				ExpirationDate = new DateTime(2019, 7, 25),
				PromotionCode = "DVDONME",
				CustomerSegmentType = "BasketStretch",
				CampaignId = 123L
			});
		}
	}

	private static Constants.TestData.AfterSwipeCustomerOffers.Offers ToOffer(string offer)
	{
		Constants.TestData.AfterSwipeCustomerOffers.Offers result = Constants.TestData.AfterSwipeCustomerOffers.Offers.NoOffer;
		if (Enum.TryParse<Constants.TestData.AfterSwipeCustomerOffers.Offers>(offer, out var result2))
		{
			result = result2;
		}
		return result;
	}

	public async Task<ReturnPromoCheckResponse> ReturnPromoCheck(string barcode)
	{
		ReturnPromoCheckResponse result = new ReturnPromoCheckResponse
		{
			StatusCode = HttpStatusCode.OK,
			IsLastDisc = false
		};
		if (!_cache.TryGetValue<ReturnPromoCheckApiResponse>(ToRpcKey(barcode), out var value) || value == null)
		{
			value = await CallReturnPromoCheck(barcode);
			if (value == null || value.StatusCode != HttpStatusCode.OK)
			{
				result.StatusCode = value.StatusCode;
				result.Promotion = null;
				return result;
			}
		}
		else
		{
			_logger.LogDebugWithSource("Promo check response for barcode " + barcode + " retrieved from cache", "ReturnPromoCheck");
		}
		if (value.Discs.Any((DiscReturnStatus d) => !d.IsReturned && d.Barcode != barcode))
		{
			_logger.LogDebugWithSource("Not all discs are returned, returning null promotion.", "ReturnPromoCheck");
			foreach (DiscReturnStatus disc in value.Discs)
			{
				if (disc.Barcode == barcode)
				{
					disc.IsReturned = true;
				}
			}
			foreach (DiscReturnStatus disc2 in value.Discs)
			{
				_cache.Set(ToRpcKey(disc2.Barcode), value, _promoCheckCacheMinutes);
			}
			result.Promotion = null;
		}
		else
		{
			result.Promotion = value.Promotion;
			result.IsLastDisc = true;
		}
		return result;
	}

	private string ToRpcKey(string key)
	{
		return _promoCheckCachePrefix + " " + key;
	}

	private async Task<ReturnPromoCheckApiResponse> CallReturnPromoCheck(string barcode)
	{
		APIResponse<ReturnPromoCheckApiResponse> aPIResponse = await _transactionClient.ReturnPromoCheck(barcode);
		ReturnPromoCheckApiResponse returnPromoCheckApiResponse = aPIResponse.Response;
		if (!aPIResponse.IsSuccessStatusCode)
		{
			string text = $"ReturnPromoCheck was unsuccessful. Status Code: {aPIResponse.StatusCode}";
			if (returnPromoCheckApiResponse != null)
			{
				returnPromoCheckApiResponse.StatusCode = aPIResponse.StatusCode;
				text = text + " Message: " + returnPromoCheckApiResponse.Error;
			}
			else
			{
				returnPromoCheckApiResponse = new ReturnPromoCheckApiResponse
				{
					StatusCode = aPIResponse.StatusCode
				};
				if ((aPIResponse.Errors?.Count ?? 0) > 0)
				{
					returnPromoCheckApiResponse.Error = aPIResponse.Errors.First().Message;
				}
			}
			_logger.LogErrorWithSource(text, "CallReturnPromoCheck");
			LogResponseErrors(aPIResponse);
		}
		return returnPromoCheckApiResponse;
	}

	private async Task<AuthorizeResponse> GetTestDataAuthorizeResponse(AuthorizeRequest authorizeRequest)
	{
		_logger?.LogInformation("TransactionService.Authorize returning test data result");
		if (bool.TryParse(await GetTestDataValue("Authorize.SimulateCommunicationFailure"), out var result) && result)
		{
			return null;
		}
		AuthorizeResponse response = new AuthorizeResponse
		{
			MessageId = authorizeRequest.MessageId
		};
		response.Errors = new List<Error>();
		string text = await GetTestDataValue("Authorize.ErrorCode");
		if (!string.IsNullOrEmpty(text))
		{
			response.Errors.Add(new Error
			{
				Code = text,
				Message = "error code " + text
			});
		}
		await FillTestDataAuthorizeResponse(authorizeRequest, response);
		return response;
	}

	private async Task FillTestDataAuthorizeResponse(AuthorizeRequest authorizeRequest, AuthorizeResponse response)
	{
		bool flag = (authorizeRequest.ShoppingCart?.Groups?.FirstOrDefault((LineItemGroup x) => x.GroupType == LineItemGroupType.Subscription)?.Items?.Count).GetValueOrDefault() > 0;
		bool flag2 = (authorizeRequest.ShoppingCart?.Groups?.FirstOrDefault((LineItemGroup x) => x.GroupType == LineItemGroupType.Rental || x.GroupType == LineItemGroupType.Purchase)?.Items?.Count).GetValueOrDefault() > 0;
		bool flag3 = response.Errors.Any((Error x) => _subscriptionErrorCodes.Contains(x.Code));
		response.KioskTransactionSuccess = flag2 && (response.Errors.Count() == 0 || response.Errors.All((Error x) => _subscriptionErrorCodes.Contains(x.Code)));
		response.SubscriptionTransactionSuccess = flag && !flag3 && (!flag2 || response.KioskTransactionSuccess);
		response.Success = response.KioskTransactionSuccess || response.SubscriptionTransactionSuccess;
		AuthorizeResponse authorizeResponse = response;
		authorizeResponse.AccountNumber = await GetTestDataValue("Authorize.AccountNumber");
		if (bool.TryParse(await GetTestDataValue("Authorize.HasCustomerProfile"), out var result))
		{
			response.HasProfile = result;
		}
		authorizeResponse = response;
		authorizeResponse.CustomerProfileNumber = await GetTestDataValue("Authorize.CustomerProfileNumber");
		authorizeResponse = response;
		authorizeResponse.CustomerProfileFirstName = await GetTestDataValue("Authorize.CustomerProfileFirstName");
		authorizeResponse = response;
		authorizeResponse.CustomerProfileLastName = await GetTestDataValue("Authorize.CustomerProfileLastName");
		authorizeResponse = response;
		authorizeResponse.Email = await GetTestDataValue("Authorize.Email");
		if (long.TryParse(await GetTestDataValue("Authorize.ReferenceNumber"), out var result2))
		{
			response.ReferenceNumber = result2;
		}
		if (Enum.TryParse<ConfirmationStatus>(await GetTestDataValue("Authorize.ConfirmationStatus"), out var result3))
		{
			response.ConfirmationStatus = result3;
		}
	}

	private async Task<GCAuthResponse> GetTestDataGCAuthorizeResponse(GCAuthRequest request)
	{
		_logger?.LogInformation("TransactionService.GiftCardAuthorize returning test data result");
		if (bool.TryParse(await GetTestDataValue("GCAuth.SimulateCommunicationFailure"), out var result) && result)
		{
			return null;
		}
		GCAuthResponse response = new GCAuthResponse
		{
			MessageId = request.MessageId
		};
		response.Errors = new List<Error>();
		string text = await GetTestDataValue("GCAuth.ErrorCode");
		if (!string.IsNullOrEmpty(text))
		{
			response.Errors.Add(new Error
			{
				Code = text,
				Message = "error code " + text
			});
		}
		await FillTestDataAuthorizeResponse(request, response);
		GiftCardSummary gcSummary = new GiftCardSummary();
		if (decimal.TryParse(await GetTestDataValue("GCAuth.Balance"), out var result2))
		{
			gcSummary.Balance = result2;
		}
		if (bool.TryParse(await GetTestDataValue("GCAuth.Exists"), out var result3))
		{
			gcSummary.Exists = result3;
		}
		if (int.TryParse(await GetTestDataValue("GCAuth.MaxDiscs"), out var result4))
		{
			gcSummary.MaxDiscs = result4;
		}
		if (Enum.TryParse<GiftCardStatus>(await GetTestDataValue("GCAuth.GCStatusType"), out var result5))
		{
			gcSummary.Status = result5;
		}
		return response;
	}

	private async Task<string> GetTestDataValue(string propertyName)
	{
		return (await (_testDataService?.ReadTestDataValueAsync("TransactionService", propertyName)))?.PropertyValue;
	}

	private async Task<bool> GetUseTestData(string liveCallPropertyName)
	{
		bool result = false;
		if (_kioskConfiguration?.TestData?.TestDataEnabled == true)
		{
			ReadTestDataResponse readTestDataResponse = await (_testDataService?.ReadTestDataValueAsync("TransactionService", liveCallPropertyName));
			if (readTestDataResponse != null && readTestDataResponse.Success && readTestDataResponse.GetPropertyValue<bool?>() == false)
			{
				result = true;
			}
		}
		return result;
	}
}
