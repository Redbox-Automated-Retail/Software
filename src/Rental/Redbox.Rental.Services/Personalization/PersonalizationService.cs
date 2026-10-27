using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Personalization;

public class PersonalizationService : IPersonalizationService
{
	private Action<IUpdateAccountResult> _updateAccountResultCallback;

	private int m_working;

	public bool IsEarlyIdEnabled => Redbox.Rental.Services.Configuration.Configuration.Instance?.EarlyIdEnabled ?? false;

	public bool IsLoyaltyEnabled => Redbox.Rental.Services.Configuration.Configuration.Instance?.LoyaltyEnabled ?? false;

	public void SetSelectedPromo(string promoCode)
	{
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (personalizationSession != null)
		{
			personalizationSession.SelectedPromo = promoCode;
		}
	}

	public void ClearSelectedPromo()
	{
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (personalizationSession != null)
		{
			personalizationSession.SelectedPromo = null;
		}
	}

	public void UpdateAccount(string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, Action<IUpdateAccountResult> resultCallback, bool queued = false, AccountSourceType source = AccountSourceType.General)
	{
		_updateAccountResultCallback = resultCallback;
		int kioskId = (int)(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0);
		RemoteServiceCallback remoteServiceCallback = ProcessUpdateAccountResult;
		if (queued)
		{
			QueueAccountUpdate(kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, remoteServiceCallback, source);
		}
		else
		{
			PerformLiveAccountUpdate(kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, remoteServiceCallback, source);
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
				if (!ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
				{
					Guid messageId = Guid.NewGuid();
					string kioskClientServiceBaseUrl = ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl;
					LogHelper.Instance.Log("Live call to remote IPersonalization.UpdateAccount, URL: " + kioskClientServiceBaseUrl);
					ServiceLocator.Instance.GetService<IMacroService>();
					IDictionary<string, object> dictionary = await ServiceLocator.Instance.GetService<IKioskClientService>().Loyalty.UpdateAccount(messageId, kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, source, replay: false);
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
					result.Properties["stand_alone"] = ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
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
				string kioskClientServiceBaseUrl = ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl;
				LogHelper.Instance.Log("Enqueue call to remote IPersonalizationServices.QueueAccountUpdate, URL: " + kioskClientServiceBaseUrl);
				await ServiceLocator.Instance.GetService<IKioskClientService>().Loyalty.UpdateAccount(messageId, kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, source, replay: true);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.QueueAccountUpdate.", e);
			}
			finally
			{
				try
				{
					ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
					if (callback != null && service != null)
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
						service.EnqueueCallback(new RemoteServiceCallbackEntry
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

	private void ProcessUpdateAccountResult(IRemoteServiceResult result)
	{
		UpdateAccountResult updateAccountResult = new UpdateAccountResult
		{
			Success = result.Success,
			UpdateAccountResponseError = UpdateAccountResponseErrors.None,
			RemoteServiceErrors = result.Errors
		};
		if (!result.Success && result.Properties.ContainsKey("response") && result.Properties["response"] is UpdateAccountResponseErrors)
		{
			switch (updateAccountResult.UpdateAccountResponseError = (UpdateAccountResponseErrors)result.Properties["response"])
			{
			case UpdateAccountResponseErrors.PhoneNumberConflict:
				updateAccountResult.ErrorMessage = Resources.personalization_service_error_UPA0001;
				break;
			case UpdateAccountResponseErrors.CommunicationError:
				updateAccountResult.ErrorMessage = Resources.personalization_service_error_UPA0002;
				break;
			default:
				updateAccountResult.ErrorMessage = Resources.personalization_service_error_UPA0002;
				break;
			}
		}
		else
		{
			updateAccountResult.CustomerProfileNumber = result.GetProperty<string>("CustomerProfileNumber");
			updateAccountResult.TempPassword = result.GetProperty<string>("TempPassword");
		}
		_updateAccountResultCallback?.Invoke(updateAccountResult);
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
