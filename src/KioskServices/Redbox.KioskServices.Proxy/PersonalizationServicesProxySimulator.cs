using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.Proxy;

public class PersonalizationServicesProxySimulator : IPersonalizationServices
{
	private int m_working;

	public static PersonalizationServicesProxySimulator Instance => Singleton<PersonalizationServicesProxySimulator>.Instance;

	public Redbox.KioskServices.Proxy.ComponentModel.UpdateAccountResponseErrors UpdateAccountResponseCode
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "UpdateAccountResponseError", "Error");
			return (Redbox.KioskServices.Proxy.ComponentModel.UpdateAccountResponseErrors)Enum.Parse(typeof(Redbox.KioskServices.Proxy.ComponentModel.UpdateAccountResponseErrors), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "UpdateAccountResponseError", value);
		}
	}

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LiveCall", value);
		}
	}

	public bool SimulateSuccess
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "SimulateSuccess", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "SimulateSuccess", value);
		}
	}

	public string CustomerProfileNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "CustomerProfileNumber", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "CustomerProfileNumber", value);
		}
	}

	public string LoginEmail
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LoginEmail", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LoginEmail", value);
		}
	}

	public string MobilePhoneNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "MobilePhoneNumber", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "MobilePhoneNumber", value);
		}
	}

	public List<IStoredPromoCode> StoredPromoCodes => new List<IStoredPromoCode>
	{
		new StoredPromoCode
		{
			PromoCode = "TESTCODE1",
			UsageDescription = "Onceonly",
			ExpirationText = "EXPIRES TODAY",
			CampaignName = "Rent 1 disc, get a Free 1-night DVD rental",
			ExpirationDate = DateTime.Now,
			RedemptionTypeDescription = "FixedValue",
			DaysUntilExpiration = null
		},
		new StoredPromoCode
		{
			PromoCode = "TESTBDAY",
			UsageDescription = "birthday rental",
			ExpirationText = "TWO DAYS LEFT",
			CampaignName = "1 free birthday rental",
			ExpirationDate = DateTime.Now.AddDays(2.0),
			RedemptionTypeDescription = "FixedValue",
			DaysUntilExpiration = 1
		}
	};

	public List<IAcceptedOffer> AcceptedOffers => new List<IAcceptedOffer>
	{
		new AcceptedOffer
		{
			Status = "Active",
			EndValueText = "2x pts",
			Name = "Multi-night Rentals",
			TimeToComplete = "7 D",
			Description = "Earn 2X the points for every multi-night rental.",
			CompleteDate = DateTime.Now.AddHours(1.0),
			MaxValue = 0,
			RemainderValue = 2,
			CurrentValue = 0
		},
		new AcceptedOffer
		{
			Status = "Active",
			EndValueText = "5000 points",
			Name = "Game Dash - Genre",
			TimeToComplete = null,
			Description = "Rent 6 Xbox or Playstation games in 14 days. ",
			CompleteDate = DateTime.Now.AddHours(26.0),
			CurrentValue = 2,
			MaxValue = 6,
			RemainderValue = 4
		},
		new AcceptedOffer
		{
			Status = "Completed",
			EndValueText = "250 pts",
			Name = "Maximize Your Cart",
			TimeToComplete = "junk",
			Description = "Rent 6 Xbox or Playstation games in 14 days. ",
			CompleteDate = DateTime.Now.AddDays(15.0)
		}
	};

	private PersonalizationServicesProxySimulator()
	{
	}

	public void UpdateAccount(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback completeCallback, bool queued = false, Redbox.KioskServices.Proxy.ComponentModel.AccountSourceType accountSourceType = Redbox.KioskServices.Proxy.ComponentModel.AccountSourceType.General)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Simulate call to remote IPersonalization.UpdateAccount");
				remoteServiceResult.Success = SimulateSuccess;
				remoteServiceResult.Properties["CustomerProfileNumber"] = CustomerProfileNumber;
				remoteServiceResult.Properties["MobilePhoneNumber"] = MobilePhoneNumber;
				remoteServiceResult.Properties["server_name"] = "SimulatedCall";
				if (!SimulateSuccess)
				{
					remoteServiceResult.Properties["response"] = UpdateAccountResponseCode;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.PerformSimulatedUpdateAccount.", e);
				remoteServiceResult.Properties["response"] = Redbox.KioskServices.Proxy.ComponentModel.UpdateAccountResponseErrors.Error;
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
						Message = "Invoking completeCallback from PersonalizationServicesProxy.PerformSimulatedUpdateAccount."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}
}
