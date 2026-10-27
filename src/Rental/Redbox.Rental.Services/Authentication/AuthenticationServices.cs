using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Authentication;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Authentication;

namespace Redbox.Rental.Services.Authentication;

public class AuthenticationServices : IAuthenticationServices
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

	private int m_working;

	private TimeSpan LockoutTime => TimeSpan.FromMinutes(ServiceLocator.Instance.GetService<IConfiguration>()?.LoginLockoutMinutes ?? 5);

	public void Authenticate(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback)
	{
		if (IsUserAccountLocked(userName))
		{
			SendLockoutResponse(userName, completeCallback);
		}
		else
		{
			CallKCSAuthenticate(storeNumber, userName, password, useLdapAuthentication, authCallback);
		}
		void authCallback(IRemoteServiceResult r)
		{
			RecordLoginAttempts(userName, r, completeCallback);
		}
	}

	private void CallKCSAuthenticate(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback)
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
				if (!ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
				{
					IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
					AuthenticateRequest request = new AuthenticateRequest
					{
						KioskId = int.Parse(storeNumber),
						UseNTAuthentication = useLdapAuthentication,
						Password = EncryptionHelper.Encrypt(password),
						Username = userName
					};
					AuthenticateResponse authenticateResponse = service.Authentication.Authenticate(request);
					if (authenticateResponse == null)
					{
						remoteServiceResult.Properties["response"] = AuthenticationResponse.Error.ToString();
						remoteServiceResult.Success = false;
						remoteServiceResult.Properties["is_online"] = false;
					}
					else if (authenticateResponse.Success)
					{
						remoteServiceResult.Properties["response"] = AuthenticationResponse.Valid.ToString();
						remoteServiceResult.Success = true;
					}
					else
					{
						remoteServiceResult.Success = false;
						Redbox.Rental.Model.Error error = authenticateResponse.Errors.FirstOrDefault();
						remoteServiceResult.Properties["response"] = ((error == null) ? AuthenticationResponse.Error.ToString() : error.Code);
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to authenticate.", LogEntryType.Info);
					remoteServiceResult.Properties["response"] = ((!useLdapAuthentication) ? AuthenticationResponse.AuthenticationFailed.ToString() : AuthenticationResponse.Valid.ToString());
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in AuthenticationServicesProxy.Authenticate.", e);
				remoteServiceResult.Properties["response"] = AuthenticationResponse.Error.ToString();
				remoteServiceResult.Properties["is_online"] = false;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
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
		LogHelper.Instance.Log("AuthenticationServicesProxy ignoring request from locked accout: UserName " + userName);
		RemoteServiceResult remoteServiceResult = new RemoteServiceResult
		{
			Success = false
		};
		remoteServiceResult.Properties["response"] = AuthenticationResponse.AccountLocked.ToString();
		remoteServiceResult.Properties["is_online"] = true;
		completeCallback(remoteServiceResult);
	}
}
