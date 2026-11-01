using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Authentication;

namespace Redbox.KioskServices.Proxy;

internal class AuthenticationServices : IAuthenticationServices
{
	private class UserLoginInfo
	{
		public string UserId { get; set; }

		public int FailedAttempts { get; set; }

		public DateTime LastAttemptTime { get; set; }

		public DateTime FirstAttemptTime { get; set; }
	}

	private const int _maxRetries = 5;

	private readonly TimeSpan _retryTime = TimeSpan.FromMinutes(1.0);

	private readonly Dictionary<string, UserLoginInfo> _loginFailures = new Dictionary<string, UserLoginInfo>();

	protected int m_working;

	private TimeSpan LockoutTime => TimeSpan.FromMinutes(ServiceLocator.Instance.GetService<IConfiguration>()?.LoginLockoutMinutes ?? 5);

	public bool EnableLocalAccountCaching
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Authentication Services", "EnableLocalAccountCaching", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Authentication Services", "EnableLocalAccountCaching", value);
		}
	}

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Authentication Services", "LiveCall", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Authentication Services", "LiveCall", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Authentication Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Authentication Services", "SimulateCommunicationFailure", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Authentication Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Authentication Services", "SimulateServerBusyFailure", value);
		}
	}

	public string[] Users
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Authentication Services", "Users", Array.Empty<string>());
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Authentication Services", "Users", value);
		}
	}

	private void GetUserResponse(string userName, out Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse response)
	{
		response = Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.AuthenticationFailed;
		if (Users == null)
		{
			return;
		}
		string[] users = Users;
		for (int i = 0; i < users.Length; i++)
		{
			string[] array = users[i].Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 2 && string.Compare(array[0], userName, ignoreCase: true) == 0)
			{
				response = Enum<Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse>.ParseIgnoringCase(array[1], Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Valid);
				break;
			}
		}
	}

	private bool IsUserAccountLocked(string userName)
	{
		if (_loginFailures.ContainsKey(userName))
		{
			UserLoginInfo userLoginInfo = _loginFailures[userName];
			if (userLoginInfo.FailedAttempts >= 5)
			{
				return DateTime.Now - userLoginInfo.LastAttemptTime < LockoutTime;
			}
			return false;
		}
		return false;
	}

	private void RecordLoginAttempts(string userName, IRemoteServiceResult result, RemoteServiceCallback completeCallback)
	{
		try
		{
			if (result.Success)
			{
				if (_loginFailures.ContainsKey(userName))
				{
					_loginFailures.Remove(userName);
				}
			}
			else
			{
				if (!(bool)result.Properties["is_online"])
				{
					return;
				}
				if (!_loginFailures.ContainsKey(userName))
				{
					_loginFailures.Add(userName, new UserLoginInfo
					{
						UserId = userName,
						FailedAttempts = 1,
						FirstAttemptTime = DateTime.Now,
						LastAttemptTime = DateTime.Now
					});
					return;
				}
				UserLoginInfo userLoginInfo = _loginFailures[userName];
				if (DateTime.Now - userLoginInfo.FirstAttemptTime > _retryTime)
				{
					userLoginInfo.FailedAttempts = 1;
					userLoginInfo.FirstAttemptTime = DateTime.Now;
				}
				else
				{
					userLoginInfo.FailedAttempts++;
				}
				userLoginInfo.LastAttemptTime = DateTime.Now;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("Exception encountered while recording failed login", e);
		}
		finally
		{
			completeCallback(result);
		}
	}

	private void SendLockoutResponse(string userName, RemoteServiceCallback completeCallback)
	{
		LogHelper.Instance.Log($"AuthenticationServicesProxy ignoring request from locked accout: UserName {userName}");
		RemoteServiceResult remoteServiceResult = new RemoteServiceResult
		{
			Success = false
		};
		remoteServiceResult.Properties["response"] = Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.AccountLocked.ToString();
		remoteServiceResult.Properties["is_online"] = true;
		completeCallback(remoteServiceResult);
	}

	private void KCSPerformLiveCall(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback)
	{
		Task.Run(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Live call to KioskClientService Authenticate, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
				if (!ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone)
				{
					IKioskClientService kioskClientService = ServiceLocator.Instance.GetService<IKioskClientService>();
					AuthenticateRequest request = new AuthenticateRequest
					{
						KioskId = int.Parse(storeNumber),
						UseNTAuthentication = useLdapAuthentication,
						Password = EncryptionHelper.Encrypt(password),
						Username = userName
					};
					AuthenticateResponse authenticateResponse = kioskClientService.Authentication.Authenticate(request);
					if (authenticateResponse == null)
					{
						remoteServiceResult.Properties["response"] = Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Error.ToString();
						remoteServiceResult.Success = false;
						remoteServiceResult.Properties["is_online"] = false;
					}
					else if (authenticateResponse.Success)
					{
						remoteServiceResult.Properties["response"] = Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Valid.ToString();
						remoteServiceResult.Success = true;
					}
					else
					{
						remoteServiceResult.Success = false;
						Redbox.Rental.Model.Error error = authenticateResponse.Errors.FirstOrDefault();
						remoteServiceResult.Properties["response"] = ((error == null) ? Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Error.ToString() : error.Code);
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to authenticate.", LogEntryType.Info);
					remoteServiceResult.Properties["response"] = ((!useLdapAuthentication) ? Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.AuthenticationFailed.ToString() : Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Valid.ToString());
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in AuthenticationServicesProxy.Authenticate.", e);
				remoteServiceResult.Properties["response"] = Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Error.ToString();
				remoteServiceResult.Properties["is_online"] = false;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult?.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from AuthenticationServicesProxy.Authenticate."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void KSPerformLiveCall(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Live call to KioskServices Authenticate, URL: {0}", KioskServicesProxy.Instance.Url);
				if (!ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone)
				{
					IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
					IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
					{
						MessageType = "Authenticate",
						MessageId = Guid.NewGuid(),
						KioskId = int.Parse(storeNumber),
						EngineVersion = macroService["EngineVersion"],
						BundleVersion = macroService["ProductVersion"],
						UseNTAuthentication = useLdapAuthentication,
						Password = EncryptionHelper.Encrypt(password),
						Username = userName
					});
					HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
					remoteServiceResult.Properties["response"] = Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Valid.ToString();
					remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
					remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					if (dictionary.ContainsKey("ServerName"))
					{
						remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
					}
					if (!remoteServiceResult.Success && !dictionary.ContainsKey("ExceptionRaised"))
					{
						ArrayList arrayList = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
						if (arrayList != null && arrayList.Count > 0)
						{
							if (arrayList[0] is Dictionary<string, object> dictionary2)
							{
								object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse?), (string)dictionary2["Code"]);
								remoteServiceResult.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Error.ToString());
							}
						}
						else
						{
							remoteServiceResult.Properties["response"] = Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Error.ToString();
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to authenticate.", LogEntryType.Info);
					remoteServiceResult.Properties["response"] = ((!useLdapAuthentication) ? Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.AuthenticationFailed.ToString() : Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Valid.ToString());
					remoteServiceResult.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in AuthenticationServicesProxy.Authenticate.", e);
				remoteServiceResult.Properties["response"] = Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Error.ToString();
				remoteServiceResult.Properties["is_online"] = false;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult?.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from AuthenticationServicesProxy.Authenticate."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	protected void PerformLiveCall(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendAuthenticationApisToKioskClientService)
		{
			KCSPerformLiveCall(storeNumber, userName, password, useLdapAuthentication, completeCallback);
		}
		else
		{
			KSPerformLiveCall(storeNumber, userName, password, useLdapAuthentication, completeCallback);
		}
	}

	protected void SimulateCall(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			LogHelper.Instance.Log("Simulating call to IAuthenticationServices.Authenticate.");
			LogHelper.Instance.LogTo("UserLoginLog", "Authentication Simulated (Live Call is disabled)");
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				int num = new Random().Next(1, 5);
				new AutoResetEvent(initialState: false).WaitOne(num * 1000);
				remoteServiceResult.Properties["store_number"] = storeNumber;
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
					remoteServiceResult.Properties["is_online"] = false;
				}
				else if (SimulateCommunicationsFailure)
				{
					remoteServiceResult.Success = false;
					RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
					{
						Type = InstructionType.WaitForSuccessfulPing
					};
					remoteServiceResult.InnerInstructions.Add(item);
					remoteServiceResult.Properties["response"] = "Communications Failure";
					remoteServiceResult.Properties["is_online"] = false;
				}
				else
				{
					GetUserResponse(userName, out var response);
					remoteServiceResult.Success = response == Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Valid;
					remoteServiceResult.Properties["response"] = response.ToString();
					remoteServiceResult.Properties["user_name"] = userName;
					remoteServiceResult.Properties["password"] = password;
					remoteServiceResult.Properties["use_ldap"] = useLdapAuthentication;
					remoteServiceResult.Properties["is_online"] = true;
				}
				remoteServiceResult.Properties["stand_alone"] = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in AuthenticationServicesProxy.Authenticate.", e);
				remoteServiceResult.Properties["response"] = Redbox.KioskServices.Proxy.ComponentModel.AuthenticationResponse.Error.ToString();
				remoteServiceResult.Properties["is_online"] = false;
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
						Message = "Invoking completeCallback from AuthenticationServicesProxy.Authenticate."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	internal AuthenticationServices()
	{
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Authenticate(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback)
	{
		if (IsUserAccountLocked(userName))
		{
			SendLockoutResponse(userName, completeCallback);
		}
		else if (LiveCall)
		{
			PerformLiveCall(storeNumber, userName, password, useLdapAuthentication, authCallback);
		}
		else
		{
			SimulateCall(storeNumber, userName, password, useLdapAuthentication, authCallback);
		}
		void authCallback(IRemoteServiceResult r)
		{
			RecordLoginAttempts(userName, r, completeCallback);
		}
	}

	public void Reset()
	{
	}
}
