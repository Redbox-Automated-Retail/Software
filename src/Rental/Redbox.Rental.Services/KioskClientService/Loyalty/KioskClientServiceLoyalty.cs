using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Loyalty;
using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Loyalty;

public class KioskClientServiceLoyalty : BaseKioskClientService, IKioskClientServiceLoyalty
{
	private class LoyaltyResponse
	{
		public class ItemEstimation
		{
			public long ProductId { get; set; }

			public int Points { get; set; }
		}

		public bool Success { get; set; }

		public List<Redbox.Rental.Model.Error> Errors { get; set; }

		public Guid MessageId { get; set; }

		public long KioskId { get; set; }

		public List<ItemEstimation> Items { get; set; }

		public void AddSuccessToDictionary(Dictionary<string, object> result)
		{
			result["Success"] = Success;
			if (Items != null && Items.Count > 0)
			{
				ArrayList items = new ArrayList();
				Items.ForEach(delegate(ItemEstimation i)
				{
					items.Add(new Dictionary<string, object>
					{
						{ "ProductId", i.ProductId },
						{ "Points", i.Points }
					});
				});
				result["Items"] = items;
			}
			if (Errors != null && Errors.Count > 0)
			{
				ArrayList errors = new ArrayList();
				Errors.ForEach(delegate(Redbox.Rental.Model.Error i)
				{
					errors.Add(new Dictionary<string, object>
					{
						{ "Code", i.Code },
						{ "Message", i.Message }
					});
				});
				result["Errors"] = errors;
			}
		}

		public void AddErrorToDictionary(Dictionary<string, object> result, string message)
		{
			result["Success"] = false;
			result["Errors"] = new ArrayList
			{
				new Dictionary<string, object>
				{
					{ "Code", "(EA003)" },
					{ "Message", message }
				}
			};
		}
	}

	private class UpdateAccountPayload : IMessageScrub
	{
		public Guid MessageId { get; set; }

		public long KioskId { get; set; }

		public string CustomerProfileNumber { get; set; }

		public string Email { get; set; }

		public string MobilePhoneNumber { get; set; }

		public bool PunchcardOptIn { get; set; }

		public string Pin { get; set; }

		public bool TextClubOptIn { get; set; }

		public AccountSourceType AccountSourceType { get; set; }

		public bool Replay { get; set; }

		public object Scrub()
		{
			ObfuscationHelper obfuscationHelper = new ObfuscationHelper();
			return new
			{
				MessageId = MessageId,
				KioskId = KioskId,
				CustomerProfileNumber = obfuscationHelper.ObfuscateStringData(CustomerProfileNumber, 1, 1),
				Email = obfuscationHelper.ObfuscateStringData(Email, 1, 1),
				MobilePhoneNumber = obfuscationHelper.ObfuscateStringData(MobilePhoneNumber, 1, 1),
				PunchcardOptIn = PunchcardOptIn,
				Pin = "####",
				TextClubOptIn = TextClubOptIn,
				AccountSourceType = AccountSourceType,
				Replay = Replay
			}.ToJson().ToPrettyJson();
		}
	}

	private const string _apiPath = "api/loyalty";

	public async Task<IDictionary<string, object>> EstimateAccrual(CartMessage payload)
	{
		Dictionary<string, object> result = new Dictionary<string, object>();
		string text = "api/loyalty/estimateaccrual";
		LogHelper.Instance.Log("KioskClientServiceLoyalty.EstimateAccrual - POST'ing following estimate accrual payload to '" + text + "': " + payload.ToJson().ToPrettyJson());
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(base.KioskClientServiceBaseUrl, text, payload, HttpMethod.Post);
		APIResponse<LoyaltyResponse> aPIResponse = await service.SendRequestAsync<LoyaltyResponse>(request, ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceEstimateAccrualTimeout, "EstimateAccrual", "c:\\build\\Redbox.Rental.Services\\KioskClientService\\Loyalty\\KioskClientServiceLoyalty.cs");
		if (aPIResponse.IsSuccessStatusCode)
		{
			LogHelper.Instance.Log("KioskClientServiceLoyalty.EstimateAccrual - received successful estimate accrual response: " + aPIResponse.Response?.ToJson());
			if (aPIResponse.Response == null)
			{
				aPIResponse.Response = new LoyaltyResponse();
				aPIResponse.Response.AddErrorToDictionary(result, "Successful result from KCS, but no response object");
			}
			else
			{
				aPIResponse.Response.AddSuccessToDictionary(result);
			}
		}
		else
		{
			LogHelper.Instance.Log($"KioskClientServiceLoyalty.EstimateAccrual - post failed with status code: {aPIResponse.StatusCode}, response: {aPIResponse.Response?.ToJson()}");
			if (aPIResponse.Response == null)
			{
				aPIResponse.Response = new LoyaltyResponse();
				aPIResponse.Response.AddErrorToDictionary(result, $"Status code: {aPIResponse.StatusCode}");
			}
			else
			{
				aPIResponse.Response.AddSuccessToDictionary(result);
			}
		}
		return result;
	}

	public async Task<IDictionary<string, object>> EstimateRedemption(CartMessage payload)
	{
		Dictionary<string, object> result = new Dictionary<string, object>();
		string text = "api/loyalty/estimateredemption";
		LogHelper.Instance.Log("KioskClientServiceLoyalty.EstimateRedemption - POST'ing following estimate redemption payload to '" + text + "': " + payload.ToJson());
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(base.KioskClientServiceBaseUrl, text, payload, HttpMethod.Post);
		APIResponse<LoyaltyResponse> aPIResponse = await service.SendRequestAsync<LoyaltyResponse>(request, ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceEstimateRedemptionTimeout, "EstimateRedemption", "c:\\build\\Redbox.Rental.Services\\KioskClientService\\Loyalty\\KioskClientServiceLoyalty.cs");
		if (aPIResponse.IsSuccessStatusCode)
		{
			LogHelper.Instance.Log("KioskClientServiceLoyalty.EstimateRedemption - received successful estimate redemption response: " + aPIResponse.Response?.ToJson());
			if (aPIResponse.Response == null)
			{
				aPIResponse.Response = new LoyaltyResponse();
				aPIResponse.Response.AddErrorToDictionary(result, "Successful result from KCS, but no response object");
			}
			else
			{
				aPIResponse.Response.AddSuccessToDictionary(result);
			}
		}
		else
		{
			LogHelper.Instance.Log($"KioskClientServiceLoyalty.EstimateAccrual - post failed with status code: {aPIResponse.StatusCode}, response: {aPIResponse.Response?.ToJson()}");
			if (aPIResponse.Response == null)
			{
				aPIResponse.Response = new LoyaltyResponse();
				aPIResponse.Response.AddErrorToDictionary(result, $"Status code: {aPIResponse.StatusCode}");
			}
			else
			{
				aPIResponse.Response.AddSuccessToDictionary(result);
			}
		}
		return result;
	}

	public async Task<IDictionary<string, object>> UpdateAccount(Guid messageId, long kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, AccountSourceType source, bool replay)
	{
		string text = "api/loyalty/updateaccount";
		UpdateAccountPayload updateAccountPayload = new UpdateAccountPayload
		{
			MessageId = messageId,
			KioskId = kioskId,
			CustomerProfileNumber = customerProfileNumber,
			Email = emailAddress,
			MobilePhoneNumber = phoneNumber,
			PunchcardOptIn = punchcardOptIn,
			Pin = EncryptionHelper.Encrypt(pin?.SecureStringToString()),
			TextClubOptIn = textClubOptIn,
			AccountSourceType = source,
			Replay = replay
		};
		LogHelper.Instance.Log($"KioskClientServiceLoyalty.UpdateAccount - POST'ing following update account payload to '{text}': {updateAccountPayload}");
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(base.KioskClientServiceBaseUrl, text, updateAccountPayload, HttpMethod.Post);
		APIResponse<UpdateAccountResponse> aPIResponse = await service.SendRequestAsync<UpdateAccountResponse>(request, ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceUpdateAccountTimeout, "UpdateAccount", "c:\\build\\Redbox.Rental.Services\\KioskClientService\\Loyalty\\KioskClientServiceLoyalty.cs", updateAccountPayload);
		LogHelper.Instance.Log("KioskClientServiceLoyalty.UpdateAccount - response: " + aPIResponse.ResponseContent);
		return aPIResponse.ToUpdateAccountDictionary();
	}
}
