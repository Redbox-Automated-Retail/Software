using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Session;

namespace Redbox.KioskServices.Proxy;

internal class MarketingServices : IMarketingServices
{
	private static class RegistryKeys
	{
		public const string ConfirmationStatusCode = "ConfirmationStatusCode";

		public const string CustomerProfileNumber = "CustomerProfileNumber";

		public const string EmailVerified = "EmailVerified";

		public const string LiveCall = "LiveCall";

		public const string MobilePhoneNumber = "MobilePhoneNumber";

		public const string PromptForEarlyId = "PromptForEarlyId";

		public const string PromptForPerks = "PromptForPerks";

		public const string SimulateCommunicationsFailure = "SimulateCommunicationsFailure";

		public const string TextClubEnabled = "TextClubEnabled";
	}

	private class OptInItem : IOptInItem
	{
		public long ItemId { get; set; }

		public KioskOptIntItemType Item_Type { get; set; }

		public OptInMethodType OptInMethodType { get; set; }

		public string OptInMethodData { get; set; }

		public string TemplateName { get; set; }

		public string CampaignName { get; set; }
	}

	private const string RegistryPath = "Remote Services\\Marketing Optin Services";

	private int m_working;

	public ConfirmationStatus ConfirmationStatusCode
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "ConfirmationStatusCode", "DoNotShow");
			return (ConfirmationStatus)Enum.Parse(typeof(ConfirmationStatus), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "ConfirmationStatusCode", value);
		}
	}

	public string CustomerProfileNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "CustomerProfileNumber", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "CustomerProfileNumber", value);
		}
	}

	public bool? EmailVerified
	{
		get
		{
			bool? result = null;
			if (bool.TryParse(ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<string>("Remote Services\\Marketing Optin Services", "EmailVerified", null), out var result2))
			{
				result = result2;
			}
			return result;
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "EmailVerified", value.ToString());
		}
	}

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "LiveCall", value);
		}
	}

	public string MobilePhoneNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "MobilePhoneNumber", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "MobilePhoneNumber", value);
		}
	}

	public bool PromptForEarlyId
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "PromptForEarlyId", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "PromptForEarlyId", value);
		}
	}

	public bool PromptForPerks
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "PromptForPerks", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "PromptForPerks", value);
		}
	}

	public bool TextClubEnabled
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "TextClubEnabled", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "TextClubEnabled", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "SimulateCommunicationsFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "SimulateCommunicationsFailure", value);
		}
	}

	public void SendEmailConfirm(long kioskId, string emailAddress, string accountNumber, SendEmailConfirmCallback completeCallback)
	{
		SendEmailConfirm(kioskId, emailAddress, accountNumber, processConfirmEmailResult);
		void processConfirmEmailResult(IRemoteServiceResult remoteServiceResult)
		{
			completeCallback(GetEmailConfirmResult(remoteServiceResult));
		}
	}

	public void KioskOptIn(long kioskId, List<IOptInItem> optInItems, RemoteServiceCallback callback)
	{
		ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		ThreadPool.QueueUserWorkItem(delegate
		{
			Interlocked.Increment(ref m_working);
			ErrorList errorList = null;
			try
			{
				IEnumerable<IOptInItem> enumerable = (optInItems ?? new List<IOptInItem>()).Where((IOptInItem x) => !string.IsNullOrEmpty(x.OptInMethodData));
				errorList = ((!enumerable.Any()) ? new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("U001", "No Valid Optins To Send", (optInItems?.Count() ?? 0) + " items were missing OptInMethodData") } : PerformKioskOptInApiCall(kioskId, enumerable));
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PromotionServices.KioskOptIn.", e);
			}
			finally
			{
				try
				{
					if (callback != null)
					{
						ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
						if (service != null)
						{
							RemoteServiceResult remoteServiceResult = new RemoteServiceResult
							{
								Success = true
							};
							if (errorList != null)
							{
								remoteServiceResult.Errors.AddRange(errorList);
								remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
							}
							service.EnqueueCallback(new RemoteServiceCallbackEntry
							{
								Callback = callback,
								Result = remoteServiceResult,
								Message = "Invoking callback from PromotionServices.KioskOptIn."
							});
						}
					}
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public IOptInItem NewOptInItem()
	{
		return new OptInItem();
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reset()
	{
	}

	internal MarketingServices()
	{
	}

	protected IRemoteServiceResult PerformEmailConfirmationApiCall(long kioskId, string emailAddress, string accountNumber)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendMarketingApisToKioskClientService)
		{
			return KCSPerformEmailConfirmationApiCall(kioskId, emailAddress, accountNumber);
		}
		return KSPerformEmailConfirmationApiCall(kioskId, emailAddress, accountNumber);
	}

	protected ErrorList PerformKioskOptInApiCall(long kioskId, IEnumerable<IOptInItem> optInItems)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendMarketingApisToKioskClientService)
		{
			return KCSPerformKioskOptInApiCall(kioskId, optInItems);
		}
		return KSPerformKioskOptInApiCall(kioskId, optInItems);
	}

	private IEmailConfirmResult GetEmailConfirmResult(IRemoteServiceResult result)
	{
		try
		{
			EmailConfirmResult obj = new EmailConfirmResult
			{
				Success = result.Success,
				Errors = result.Errors,
				CustomerProfileNumber = result.GetProperty<string>("customer_profile_number"),
				MobilePhoneNumber = result.GetProperty<string>("mobile_phone_number"),
				PromptForPerks = result.GetProperty<bool>("prompt_for_perks"),
				PromptForEarlyId = result.GetProperty<bool>("prompt_for_early_id"),
				TextClubEnabled = result.GetProperty<bool>("text_club_enabled"),
				EmailVerified = result.GetProperty<bool?>("email_verified")
			};
			ConfirmationStatus confirmationStatus = ConfirmationStatus.DontShow;
			if (Enum.TryParse<ConfirmationStatus>(result.GetProperty<string>("confirmation_status"), out var result2))
			{
				confirmationStatus = result2;
			}
			if (confirmationStatus == ConfirmationStatus.DontShow)
			{
				confirmationStatus = ConfirmationStatus.DontShow;
			}
			((IEmailConfirmResult)obj).ConfirmationStatus = confirmationStatus;
			return obj;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("Unhandled Exception processing Email result", e);
			return null;
		}
	}

	private void SendEmailConfirm(long kioskId, string emailAddress, string accountNumber, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			SendLiveCall(kioskId, emailAddress, accountNumber, completeCallback);
		}
		else
		{
			SendSimulatedCall(completeCallback);
		}
	}

	private void SendLiveCall(long kioskId, string emailAddress, string accountNumber, RemoteServiceCallback completeCallback)
	{
		ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				IRemoteServiceResult remoteServiceResult2 = PerformEmailConfirmationApiCall(kioskId, emailAddress, accountNumber);
				remoteServiceResult.Success = remoteServiceResult2.Success;
				foreach (KeyValuePair<string, object> property in remoteServiceResult2.Properties)
				{
					remoteServiceResult.Properties.Add(property);
				}
				if (remoteServiceResult2.Errors != null)
				{
					remoteServiceResult.Errors.AddRange(remoteServiceResult2.Errors);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in BaseMarketingServices.SendLiveCall.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from BaseMarketingServices.SendLiveCall."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void SendSimulatedCall(RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			LogHelper.Instance.Log("Simulated call to KioskServices.MarketingOptinServices.");
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (SimulateCommunicationsFailure)
				{
					remoteServiceResult.Properties["has_errors"] = false;
					remoteServiceResult.Success = false;
					RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
					{
						Type = InstructionType.WaitForSuccessfulPing
					};
					remoteServiceResult.InnerInstructions.Add(item);
					remoteServiceResult.Properties["server_name"] = "Simulated";
					remoteServiceResult.Properties["server_busy"] = true;
					remoteServiceResult.Properties["is_online"] = false;
					remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DontShow.ToString();
					remoteServiceResult.Properties["email_verified"] = null;
				}
				else
				{
					remoteServiceResult.Success = true;
					remoteServiceResult.Properties["has_errors"] = false;
					remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatusCode.ToString();
					remoteServiceResult.Properties["customer_profile_number"] = CustomerProfileNumber;
					remoteServiceResult.Properties["mobile_phone_number"] = MobilePhoneNumber;
					remoteServiceResult.Properties["prompt_for_perks"] = PromptForPerks;
					remoteServiceResult.Properties["prompt_for_early_id"] = PromptForEarlyId;
					remoteServiceResult.Properties["text_club_enabled"] = TextClubEnabled;
					remoteServiceResult.Properties["email_verified"] = EmailVerified;
					remoteServiceResult.Properties["server_name"] = "Simulated";
					remoteServiceResult.Properties["server_busy"] = false;
					remoteServiceResult.Properties["is_online"] = true;
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					DateTime now = DateTime.Now;
					dictionary.Add("theDate", now.Ticks);
					dictionary.Add("dateTimeKind", "Utc");
					dictionary.Add("dateString", now.ToString("yyyy-mm-ddThh:mm:ss"));
					remoteServiceResult.Properties["server_time"] = dictionary;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MarketingOptinServicesProxy.SimulatedCall.", e);
				remoteServiceResult.Success = true;
				remoteServiceResult.Properties["server_name"] = "Catch";
				remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DontShow.ToString();
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult?.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from MarketingOptinServicesProxy.SimulatedCall."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private IRemoteServiceResult KCSPerformEmailConfirmationApiCall(long kioskId, string emailAddress, string accountNumber)
	{
		LogHelper.Instance.Log("Live call to KioskClientService EmailConfirmation, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
		return ServiceLocator.Instance.GetService<IKioskClientService>().Marketing.EmailConfirmation(kioskId, emailAddress, accountNumber);
	}

	private ErrorList KCSPerformKioskOptInApiCall(long kioskId, IEnumerable<IOptInItem> optInItems)
	{
		LogHelper.Instance.Log("Live call to KioskClientService KioskOptIn, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
		return ServiceLocator.Instance.GetService<IKioskClientService>().Marketing.KioskOptIn(kioskId, optInItems);
	}

	private IRemoteServiceResult KSPerformEmailConfirmationApiCall(long kioskId, string emailAddress, string accountNumber)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		DateTime now = DateTime.Now;
		dictionary.Add("theDate", now.Ticks);
		dictionary.Add("dateTimeKind", "Unspecified");
		dictionary.Add("dateString", now.ToString("yyyy-mm-ddThh:mm:ss"));
		LogHelper.Instance.Log("Live call to KioskServices EmailConfirmation, URL: {0}", KioskServicesProxy.Instance.Url);
		IDictionary<string, object> dictionary2 = KioskServicesProxy.Instance.ExecuteMessage(new
		{
			MessageType = "EmailConfirm",
			TransactionDate = dictionary,
			Email = emailAddress,
			AccountNumber = accountNumber,
			MessageId = Guid.NewGuid(),
			KioskId = kioskId,
			EngineVersion = service["EngineVersion"],
			BundleVersion = service["ProductVersion"],
			UseMessageControl = false
		});
		RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
		remoteServiceResult.Success = dictionary2.ContainsKey("Success") && (bool)dictionary2["Success"];
		remoteServiceResult.Properties["confirmation_status"] = (dictionary2.ContainsKey("ConfirmationStatus") ? ((ConfirmationStatus)dictionary2["ConfirmationStatus"]/*cast due to constrained. prefix*/).ToString() : string.Empty);
		remoteServiceResult.Properties["customer_profile_number"] = (dictionary2.ContainsKey("CustomerProfileNumber") ? ((string)dictionary2["CustomerProfileNumber"]) : string.Empty);
		remoteServiceResult.Properties["mobile_phone_number"] = (dictionary2.ContainsKey("MobilePhoneNumber") ? ((string)dictionary2["MobilePhoneNumber"]) : string.Empty);
		remoteServiceResult.Properties["prompt_for_perks"] = dictionary2.ContainsKey("PromptForPerks") && (bool)dictionary2["PromptForPerks"];
		remoteServiceResult.Properties["prompt_for_early_id"] = dictionary2.ContainsKey("PromptForEarlyId") && (bool)dictionary2["PromptForEarlyId"];
		remoteServiceResult.Properties["text_club_enabled"] = dictionary2.ContainsKey("TextClubEnabled") && (bool)dictionary2["TextClubEnabled"];
		remoteServiceResult.Properties["email_verified"] = (dictionary2.ContainsKey("EmailVerified") ? ((bool?)dictionary2["EmailVerified"]) : ((bool?)null));
		if (dictionary2.ContainsKey("Errors"))
		{
			if (dictionary2["Errors"] is ArrayList)
			{
				foreach (object item in dictionary2["Errors"] as ArrayList)
				{
					Redbox.KioskEngine.ComponentModel.Error error = item as Redbox.KioskEngine.ComponentModel.Error;
					remoteServiceResult.Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError(error.Code, error.Description, error.Details));
				}
			}
			else if (dictionary2["Errors"] is IEnumerable<Redbox.KioskEngine.ComponentModel.Error>)
			{
				foreach (Redbox.KioskEngine.ComponentModel.Error item2 in dictionary2["Errors"] as IEnumerable<Redbox.KioskEngine.ComponentModel.Error>)
				{
					remoteServiceResult.Errors.Add(item2);
				}
			}
		}
		return remoteServiceResult;
	}

	private ErrorList KSPerformKioskOptInApiCall(long kioskId, IEnumerable<IOptInItem> optInItems)
	{
		LogHelper.Instance.Log("Live call to KioskServices KioskOptIn, URL: {0}", KioskServicesProxy.Instance.Url);
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		return ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "KioskOptIn", new
		{
			MessageType = "KioskOptIn",
			MessageId = Guid.NewGuid(),
			KioskId = kioskId,
			EngineVersion = service["EngineVersion"],
			BundleVersion = service["ProductVersion"],
			RequestDate = DateTime.Now,
			OptInItems = optInItems
		});
	}
}
