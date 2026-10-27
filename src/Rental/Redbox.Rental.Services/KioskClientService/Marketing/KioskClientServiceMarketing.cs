using System;
using System.Collections.Generic;
using System.Net;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model.KioskClientService.Marketing;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.KioskClientService.Marketing;

public class KioskClientServiceMarketing : BaseKioskClientService, IKioskClientServiceMarketing
{
	private interface IObfuscate
	{
		string Obfuscate();
	}

	private class EmailConfirmationPayload : IObfuscate
	{
		public Guid MessageId { get; set; }

		public long KioskId { get; set; }

		public string Email { get; set; }

		public string AccountNumber { get; set; }

		public DateTime TransactionDate { get; set; }

		public string Obfuscate()
		{
			ObfuscationHelper obfuscationHelper = new ObfuscationHelper();
			return new EmailConfirmationPayload
			{
				MessageId = MessageId,
				KioskId = KioskId,
				Email = obfuscationHelper.ObfuscateStringData(Email, 1, 1),
				AccountNumber = obfuscationHelper.ObfuscateStringData(AccountNumber, 3, 0),
				TransactionDate = TransactionDate
			}.ToJson().ToPrettyJson();
		}
	}

	private class EmailConfirmationResponse : IObfuscate
	{
		public Guid MessageId { get; set; }

		public bool Success { get; set; }

		public List<Redbox.KioskEngine.ComponentModel.Error> Errors { get; set; } = new List<Redbox.KioskEngine.ComponentModel.Error>();

		public ConfirmationStatus ConfirmationStatus { get; set; }

		public string CustomerProfileNumber { get; set; }

		public string MobilePhoneNumber { get; set; }

		public bool PromptForPerks { get; set; }

		public bool PromptForEarlyId { get; set; }

		public bool TextClubEnabled { get; set; }

		public bool? EmailVerified { get; set; }

		public string Obfuscate()
		{
			ObfuscationHelper obfuscationHelper = new ObfuscationHelper();
			return new EmailConfirmationResponse
			{
				MessageId = MessageId,
				Success = Success,
				Errors = Errors,
				ConfirmationStatus = ConfirmationStatus,
				CustomerProfileNumber = obfuscationHelper.ObfuscateStringData(CustomerProfileNumber, 3, 0),
				MobilePhoneNumber = obfuscationHelper.ObfuscateStringData(MobilePhoneNumber, 1, 0),
				PromptForPerks = PromptForPerks,
				PromptForEarlyId = PromptForEarlyId,
				TextClubEnabled = TextClubEnabled,
				EmailVerified = EmailVerified
			}.ToJson().ToPrettyJson();
		}
	}

	private class KioskOptInPayload : IObfuscate
	{
		public Guid MessageId { get; set; }

		public long KioskId { get; set; }

		public DateTime RequestDate { get; set; }

		public List<IOptInItem> OptInItems { get; set; } = new List<IOptInItem>();

		public string Obfuscate()
		{
			ObfuscationHelper obfuscationHelper = new ObfuscationHelper();
			IMarketingServices service = ServiceLocator.Instance.GetService<IMarketingServices>();
			KioskOptInPayload kioskOptInPayload = new KioskOptInPayload
			{
				MessageId = MessageId,
				KioskId = KioskId,
				RequestDate = RequestDate
			};
			foreach (IOptInItem optInItem2 in OptInItems)
			{
				IOptInItem optInItem = service.NewOptInItem();
				optInItem.CampaignName = optInItem2.CampaignName;
				optInItem.ItemId = optInItem2.ItemId;
				optInItem.Item_Type = optInItem2.Item_Type;
				optInItem.OptInMethodData = obfuscationHelper.ObfuscateStringData(optInItem2.OptInMethodData, 1, 1);
				optInItem.OptInMethodType = optInItem2.OptInMethodType;
				optInItem.TemplateName = optInItem2.TemplateName;
				kioskOptInPayload.OptInItems.Add(optInItem);
			}
			return kioskOptInPayload.ToJson().ToPrettyJson();
		}
	}

	private const string _apiPath = "api/marketing";

	public IRemoteServiceResult EmailConfirmation(long kioskId, string emailAddress, string accountNumber)
	{
		RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
		Uri uri = new Uri(base.KioskClientServiceBaseUrl + "/api/marketing/emailconfirmation");
		EmailConfirmationPayload emailConfirmationPayload = new EmailConfirmationPayload
		{
			MessageId = Guid.NewGuid(),
			KioskId = kioskId,
			Email = emailAddress,
			AccountNumber = accountNumber,
			TransactionDate = DateTime.Now
		};
		LogHelper.Instance.Log("KioskClientServiceMarketing.EmailConfirmation - POST'ing following email confirmation payload to '" + uri.AbsoluteUri + "': " + emailConfirmationPayload.Obfuscate());
		WebResult<EmailConfirmationResponse> webResult = ExecuteWebRequest<EmailConfirmationResponse>(uri, emailConfirmationPayload, "POST", 20000);
		if (webResult.ExceptionStatus == WebExceptionStatus.Success)
		{
			LogHelper.Instance.Log("KioskClientServiceMarketing.EmailConfirmation - received successful email confimration response: " + webResult.Response.Obfuscate());
			remoteServiceResult.Success = webResult.Response.Success;
			remoteServiceResult.Properties["confirmation_status"] = webResult.Response.ConfirmationStatus.ToString();
			remoteServiceResult.Properties["customer_profile_number"] = webResult.Response.CustomerProfileNumber;
			remoteServiceResult.Properties["mobile_phone_number"] = webResult.Response.MobilePhoneNumber;
			remoteServiceResult.Properties["prompt_for_perks"] = webResult.Response.PromptForPerks;
			remoteServiceResult.Properties["prompt_for_early_id"] = webResult.Response.PromptForEarlyId;
			remoteServiceResult.Properties["text_club_enabled"] = webResult.Response.TextClubEnabled;
			remoteServiceResult.Properties["email_verified"] = webResult.Response.EmailVerified;
			if (webResult.Response.Errors != null)
			{
				remoteServiceResult.Errors.AddRange(webResult.Response.Errors);
			}
		}
		else
		{
			LogHelper.Instance.Log($"KioskClientServiceMarketing.EmailConfirmation - post failed with status code: {webResult.ExceptionStatus}");
			remoteServiceResult.Success = false;
			remoteServiceResult.Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("(EC000)", webResult.ExceptionStatus.ToString(), string.Empty));
		}
		return remoteServiceResult;
	}

	public ErrorList KioskOptIn(long kioskId, IEnumerable<IOptInItem> optInItems)
	{
		ErrorList errorList = new ErrorList();
		Uri uri = new Uri(base.KioskClientServiceBaseUrl + "/api/marketing/kioskoptin");
		KioskOptInPayload kioskOptInPayload = new KioskOptInPayload
		{
			MessageId = Guid.NewGuid(),
			KioskId = kioskId,
			RequestDate = DateTime.Now
		};
		kioskOptInPayload.OptInItems.AddRange(optInItems);
		LogHelper.Instance.Log("KioskClientServiceMarketing.KioskOptIn - POST'ing following kiosk opt in payload to '" + uri.AbsoluteUri + "': " + kioskOptInPayload.Obfuscate());
		WebResult<Dictionary<string, object>> webResult = ExecuteWebRequest<Dictionary<string, object>>(uri, kioskOptInPayload, "POST", 20000);
		if (webResult.ExceptionStatus != WebExceptionStatus.Success)
		{
			if (webResult.Response.ContainsKey("Errors"))
			{
				foreach (Dictionary<string, object> item in (List<Dictionary<string, object>>)webResult.Response["Errors"])
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError(item["Code"].ToString(), item["Message"].ToString(), string.Empty));
				}
			}
			else
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("(KOI000)", "KioskClientServiceMarketing.KioskOptIn - api call failed with no specific errors", string.Empty));
			}
		}
		return errorList;
	}
}
