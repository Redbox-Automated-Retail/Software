using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;

namespace Redbox.KioskServices.Proxy;

public class PromotionServices : IPromotionServices
{
	private int m_working;

	public static IPromotionServices Instance => (IPromotionServices)(object)Singleton<PromotionServices>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Promotion Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Promotion Services", "LiveCall", value);
		}
	}

	public string[] PromoCodes
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Promotion Services", "PromoCodes", new string[0]);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Promotion Services", "PromoCodes", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Promotion Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Promotion Services", "SimulateServerBusyFailure", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Promotion Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Promotion Services", "SimulateCommunicationFailure", value);
		}
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reset()
	{
	}

	public void ValidatePromotionCode(string storeNumber, string promotionCode, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			PerformLiveCall(storeNumber, promotionCode, completeCallback);
		}
		else
		{
			SimulatedCall(storeNumber, promotionCode, completeCallback);
		}
	}

	private PromotionServices()
	{
	}

	private void PerformLiveCall(string storeNumber, string promotionCode, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(async delegate
		{
			RemoteServiceResult result = new RemoteServiceResult();
			result.Properties["is_online"] = true;
			result.Properties["store"] = storeNumber;
			result.Properties["code"] = promotionCode;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				IConfiguration configuration = ServiceLocator.Instance.GetService<IConfiguration>();
				bool sendPromoCodeValidationToKioskClientService = configuration.SendPromoCodeValidationToKioskClientService;
				string serviceUrl = (sendPromoCodeValidationToKioskClientService ? configuration.KioskClientServiceBaseUrl : KioskServicesProxy.Instance.Url);
				if (!ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone)
				{
					LogHelper.Instance.Log($"Live call to remote IPromotionServices.ValidatePromotionCode, URL: {serviceUrl}");
					if (!int.TryParse(storeNumber, out var kioskId))
					{
						LogHelper.Instance.Log($"The passed storeNumber '{storeNumber}' cannot be converted into an Int32.", LogEntryType.Error);
						result.Success = false;
						result.Properties["response"] = PromotionValidationResponse.Error.ToString();
					}
					else
					{
						IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
						Guid messageId = Guid.NewGuid();
						IDictionary<string, object> responseData = ((!sendPromoCodeValidationToKioskClientService) ? KioskServicesProxy.Instance.ExecuteMessage(new
						{
							KioskId = kioskId,
							Code = promotionCode,
							MessageId = messageId,
							MessageType = "DiscountCodeValidation",
							EngineVersion = macroService["EngineVersion"],
							BundleVersion = macroService["ProductVersion"]
						}) : (await ServiceLocator.Instance.GetService<IKioskClientService>().Promotion.ValidatePromotionCode(messageId, promotionCode, kioskId)));
						HealthServicesProxy.Instance.UpdateOnlineStatus(responseData);
						result.Success = responseData.ContainsKey("Success") && (bool)responseData["Success"];
						result.Properties["response"] = PromotionValidationResponse.Valid.ToString();
						result.Properties["amount"] = (responseData.ContainsKey("Amount") ? responseData["Amount"] : ((object)1));
						result.Properties["promotion_intent"] = (responseData.ContainsKey("PromotionIntentCode") ? responseData["PromotionIntentCode"] : PromotionIntentCode.FixedValue.ToString());
						result.Properties["rent_qty"] = (responseData.ContainsKey("RentQty") ? responseData["RentQty"] : ((object)0));
						result.Properties["rent_format"] = (responseData.ContainsKey("RentFormat") ? responseData["RentFormat"] : null);
						result.Properties["get_qty"] = (responseData.ContainsKey("GetQty") ? responseData["GetQty"] : ((object)0));
						result.Properties["get_format"] = (responseData.ContainsKey("GetFormat") ? responseData["GetFormat"] : null);
						Dictionary<string, object> campaignTitles = (responseData.ContainsKey("CampaignTitles") ? (responseData["CampaignTitles"] as Dictionary<string, object>) : null);
						if (campaignTitles != null)
						{
							ArrayList campaignTitleList = campaignTitles["Titles"] as ArrayList;
							Dictionary<string, object> campaignTitlesData = new Dictionary<string, object>
							{
								["include"] = (bool)campaignTitles["Include"],
								["titles"] = campaignTitleList
							};
							result.Properties["campaign_titles"] = campaignTitlesData;
						}
						if (responseData.ContainsKey("ServerName"))
						{
							result.Properties["server_name"] = responseData["ServerName"];
						}
						result.Properties["product_type_id"] = (responseData.ContainsKey("ProductTypeId") ? responseData["ProductTypeId"] : null);
						result.Properties["format_ids"] = (responseData.ContainsKey("FormatIds") ? (responseData["FormatIds"] as ArrayList) : null);
						result.Properties["action_code"] = (responseData.ContainsKey("ActionType") ? responseData["ActionType"] : PromotionActionCode.Rental.ToString());
						result.Properties["allow_full_discount"] = (responseData.ContainsKey("AllowFullDiscount") ? responseData["AllowFullDiscount"] : ((object)true));
						result.Properties["default_promo"] = (responseData.ContainsKey("DefaultPromo") ? responseData["DefaultPromo"] : null);
						if (!result.Success)
						{
							ArrayList arrayList = (responseData.ContainsKey("Errors") ? (responseData["Errors"] as ArrayList) : null);
							if (arrayList != null && arrayList.Count > 0)
							{
								if (arrayList[0] is Dictionary<string, object> firstError)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(PromotionValidationResponse?), (string)firstError["Code"]);
									result.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : PromotionValidationResponse.Error.ToString());
								}
							}
							else
							{
								result.Properties["response"] = PromotionValidationResponse.Error.ToString();
							}
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, not issuing realtime call to ValidatePromotionCode.", LogEntryType.Info);
					result.Properties["response"] = PromotionValidationResponse.Error.ToString();
					result.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PromotionServicesProxy.ValidatePromotionCode.", e);
				result.Properties["response"] = PromotionValidationResponse.Error.ToString();
			}
			finally
			{
				try
				{
					result.ExecutionTime = executionTimer.Elapsed;
					result.Properties["stand_alone"] = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", result?.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = result,
						Message = "Invoking completeCallback from PromotionServicesProxy.ValidatePromotionCode."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void SimulatedCall(string storeNumber, string promotionCode, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			LogHelper.Instance.Log("Simulating call to IPromotionServices.ValidatePromotionCode.");
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				remoteServiceResult.Properties["store"] = storeNumber;
				remoteServiceResult.Properties["code"] = promotionCode;
				int delaySeconds = new Random().Next(1, 5);
				new AutoResetEvent(initialState: false).WaitOne(delaySeconds * 1000);
				PromotionValidationResponse response;
				PromotionIntentCode intent;
				decimal amount;
				if (SimulateServerBusyError)
				{
					remoteServiceResult.Success = false;
					RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
					{
						Type = InstructionType.WaitAbsolute
					};
					remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
					remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
					remoteServiceResult.Properties["response"] = "Server Busy";
				}
				else if (SimulateCommunicationsFailure)
				{
					remoteServiceResult.Success = false;
					RemoteServiceProviderInstruction instruction = new RemoteServiceProviderInstruction
					{
						Type = InstructionType.WaitForSuccessfulPing
					};
					remoteServiceResult.InnerInstructions.Add(instruction);
					remoteServiceResult.Properties["response"] = "Communications Failure";
				}
				else if (DoesCodeMatch(promotionCode, out response, out intent, out amount))
				{
					remoteServiceResult.Success = response == PromotionValidationResponse.Valid;
					remoteServiceResult.Properties["amount"] = amount;
					remoteServiceResult.Properties["promotion_intent"] = intent.ToString();
					remoteServiceResult.Properties["response"] = response.ToString();
					remoteServiceResult.Properties["rent_qty"] = 0;
					remoteServiceResult.Properties["rent_format"] = null;
					remoteServiceResult.Properties["get_qty"] = 0;
					remoteServiceResult.Properties["get_format"] = null;
					remoteServiceResult.Properties["campaign_titles"] = null;
					remoteServiceResult.Properties["product_type_id"] = null;
					remoteServiceResult.Properties["format_ids"] = null;
					remoteServiceResult.Properties["action_code"] = PromotionActionCode.Rental.ToString();
					remoteServiceResult.Properties["allow_full_discount"] = true;
					remoteServiceResult.Properties["default_promo"] = null;
				}
				else
				{
					remoteServiceResult.Success = false;
					remoteServiceResult.Properties["amount"] = 0;
					remoteServiceResult.Properties["promotion_intent"] = PromotionIntentCode.FixedValue.ToString();
					remoteServiceResult.Properties["response"] = PromotionValidationResponse.CodeNotValid.ToString();
					remoteServiceResult.Properties["rent_qty"] = 0;
					remoteServiceResult.Properties["rent_format"] = null;
					remoteServiceResult.Properties["get_qty"] = 0;
					remoteServiceResult.Properties["get_format"] = null;
					remoteServiceResult.Properties["campaign_titles"] = null;
					remoteServiceResult.Properties["product_type_id"] = null;
					remoteServiceResult.Properties["format_ids"] = null;
					remoteServiceResult.Properties["action_code"] = PromotionActionCode.Rental.ToString();
					remoteServiceResult.Properties["allow_full_discount"] = true;
					remoteServiceResult.Properties["default_promo"] = null;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PromotionServicesProxy.ValidatePromotionCode.", e);
				remoteServiceResult.Properties["response"] = PromotionValidationResponse.Error.ToString();
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult?.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from PromotionServicesProxy.ValidatePromotionCode."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private bool DoesCodeMatch(string promotionCode, out PromotionValidationResponse response, out PromotionIntentCode intent, out decimal amount)
	{
		amount = 0m;
		response = PromotionValidationResponse.CodeNotValid;
		intent = PromotionIntentCode.FixedValue;
		if (PromoCodes == null)
		{
			return false;
		}
		string[] promoCodes = PromoCodes;
		for (int i = 0; i < promoCodes.Length; i++)
		{
			string[] fields = promoCodes[i].Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			if (fields.Length >= 4 && string.Compare(fields[0], promotionCode, ignoreCase: true) == 0)
			{
				decimal.TryParse(fields[1], out amount);
				intent = Enum<PromotionIntentCode>.ParseIgnoringCase(fields[2], PromotionIntentCode.FixedValue);
				response = Enum<PromotionValidationResponse>.ParseIgnoringCase(fields[3], PromotionValidationResponse.Valid);
				return true;
			}
		}
		return false;
	}
}
