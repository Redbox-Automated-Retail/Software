using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;

namespace Redbox.KioskServices.Proxy;

public class PersonalizationServicesProxy : IPersonalizationServices
{
	private int m_working;

	public static PersonalizationServicesProxy Instance => Singleton<PersonalizationServicesProxy>.Instance;

	public string CustomerProfileNumber { get; set; }

	private PersonalizationServicesProxy()
	{
	}

	public void UpdateAccount(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback completeCallback, bool queued = false, AccountSourceType source = AccountSourceType.General)
	{
		if (queued)
		{
			QueueAccountUpdate(kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, completeCallback, source);
		}
		else
		{
			PerformLiveAccountUpdate(kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, completeCallback, source);
		}
	}

	private void PerformLiveAccountUpdate(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback completeCallback, AccountSourceType source)
	{
		ThreadPool.QueueUserWorkItem(async delegate
		{
			RemoteServiceResult result = new RemoteServiceResult();
			result.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (!ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone)
				{
					Guid messageId = Guid.NewGuid();
					IConfiguration configuration = ServiceLocator.Instance.GetService<IConfiguration>();
					bool sendUpdateAccountToKioskClientService = configuration.SendUpdateAccountToKioskClientService;
					string arg = (sendUpdateAccountToKioskClientService ? configuration.KioskClientServiceBaseUrl : KioskServicesProxy.Instance.Url);
					LogHelper.Instance.Log($"Live call to remote IPersonalization.UpdateAccount, URL: {arg}");
					IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
					IDictionary<string, object> dictionary;
					if (sendUpdateAccountToKioskClientService)
					{
						dictionary = await ServiceLocator.Instance.GetService<IKioskClientService>().Loyalty.UpdateAccount(messageId, (long)kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, source, false);
					}
					else
					{
						dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							KioskId = kioskId,
							MessageId = messageId,
							MessageType = "UpdateAccount",
							EngineVersion = macroService["EngineVersion"],
							BundleVersion = macroService["ProductVersion"],
							CustomerProfileNumber = customerProfileNumber,
							Email = emailAddress,
							MobilePhoneNumber = phoneNumber,
							Pin = EncryptionHelper.Encrypt(pin?.SecureStringToString()),
							PunchCardOptin = punchcardOptIn,
							TextClubOptin = textClubOptIn,
							AccountSourceType = source
						});
						ServiceLocator.Instance.GetService<IHealthServices>().UpdateOnlineStatus(dictionary);
					}
					result.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					result.Properties["MobilePhoneNumber"] = GetResponseValue(dictionary, "MobilePhoneNumber", null)?.ToString();
					result.Properties["CustomerProfileNumber"] = GetResponseValue(dictionary, "CustomerProfileNumber", null)?.ToString();
					if (dictionary.ContainsKey("TempPassword"))
					{
						result.Properties["TempPassword"] = GetResponseValue(dictionary, "TempPassword", null)?.ToString();
					}
					if (dictionary.ContainsKey("ServerName"))
					{
						result.Properties["server_name"] = dictionary["ServerName"];
					}
					if (!result.Success)
					{
						ArrayList arrayList = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
						if (arrayList != null && arrayList.Count > 0)
						{
							if (arrayList[0] is Dictionary<string, object> dictionary2)
							{
								object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(UpdateAccountResponseErrors?), (string)dictionary2["Code"]);
								result.Properties["response"] = fieldsForResponseCode ?? ((object)UpdateAccountResponseErrors.Error);
							}
						}
						else
						{
							result.Properties["response"] = UpdateAccountResponseErrors.Error;
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to UpdateAccount.", LogEntryType.Info);
					result.Properties["response"] = UpdateAccountResponseErrors.CommunicationError;
					result.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.UpdateAccount.", e);
				result.Properties["response"] = UpdateAccountResponseErrors.Error;
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
						Message = "Invoking completeCallback from PersonalizationServicesProxy.PerformLiveUpdateAccount."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void QueueAccountUpdate(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback callback, AccountSourceType source)
	{
		ThreadPool.QueueUserWorkItem(async delegate
		{
			ErrorList errors = null;
			try
			{
				Interlocked.Increment(ref m_working);
				Guid messageId = Guid.NewGuid();
				IConfiguration configuration = ServiceLocator.Instance.GetService<IConfiguration>();
				bool sendUpdateAccountToKioskClientService = configuration.SendUpdateAccountToKioskClientService;
				string arg = (sendUpdateAccountToKioskClientService ? configuration.KioskClientServiceBaseUrl : KioskServicesProxy.Instance.Url);
				LogHelper.Instance.Log($"Enqueue call to remote IPersonalizationServices.QueueAccountUpdate, URL: {arg}");
				if (sendUpdateAccountToKioskClientService)
				{
					await ServiceLocator.Instance.GetService<IKioskClientService>().Loyalty.UpdateAccount(messageId, (long)kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, source, true);
				}
				else
				{
					IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
					IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
					errors = queueService.Enqueue(254, "UpdateAccount", new
					{
						KioskId = kioskId,
						MessageId = messageId,
						MessageType = "UpdateAccount",
						EngineVersion = macroService["EngineVersion"],
						BundleVersion = macroService["ProductVersion"],
						CustomerProfileNumber = customerProfileNumber,
						Email = emailAddress,
						MobilePhoneNumber = phoneNumber,
						Pin = EncryptionHelper.Encrypt(pin?.SecureStringToString()),
						PunchCardOptin = punchcardOptIn,
						TextClubOptin = textClubOptIn,
						AccountSourceType = source
					});
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.QueueAccountUpdate.", e);
			}
			finally
			{
				try
				{
					ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
					if (callback != null && callbackService != null)
					{
						RemoteServiceResult remoteServiceResult = new RemoteServiceResult
						{
							Success = true
						};
						if (errors != null)
						{
							remoteServiceResult.Errors.AddRange(errors);
							remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
						}
						callbackService.EnqueueCallback(new RemoteServiceCallbackEntry
						{
							Callback = callback,
							Result = remoteServiceResult,
							Message = "Invoking callback from PersonalizationServicesProxy.QueueAccountUpdate."
						});
					}
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private static object GetResponseValue(IDictionary<string, object> response, string key, object DefaultValue)
	{
		if (!response.ContainsKey(key) || response[key] == null)
		{
			return DefaultValue;
		}
		return response[key];
	}
}
