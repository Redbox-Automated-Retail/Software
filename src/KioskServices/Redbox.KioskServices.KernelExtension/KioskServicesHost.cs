using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Kernel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.KernelExtension;

public class KioskServicesHost : IKernelExtensionHost, IDisposable
{
	private IQueueMessageProcessorService _defaultMessageProcessorService;

	private IQueueMessageProcessorService _softSyncMessageProcessorService;

	public IKernelExtension Extension { get; set; }

	public void Dispose()
	{
	}

	public bool CanSwitch(out ErrorList errors)
	{
		errors = new ErrorList();
		bool flag = ServiceLocator.Instance.GetService<IMarketingServices>().CanSwitch();
		LogHelper.Instance.Log("KioskService -> MarketingService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH669", "Bundle Switch Error", "Can not switch while marketing service is performing work."));
			return false;
		}
		flag = ServiceLocator.Instance.GetService<IStoreServices>().CanSwitch();
		LogHelper.Instance.Log("KioskService -> StoreService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH666", "Bundle Switch Error", "Can not switch while store service is performing work."));
			return false;
		}
		flag = ServiceLocator.Instance.GetService<IPromotionServices>().CanSwitch();
		LogHelper.Instance.Log("KioskService -> PromotionService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH665", "Bundle Switch Error", "Can not switch while promotion service is performing work."));
			return false;
		}
		flag = ServiceLocator.Instance.GetService<IHealthServices>().CanSwitch();
		LogHelper.Instance.Log("KioskService -> HealthService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH664", "Bundle Switch Error", "Can not switch while health service is performing work."));
			return false;
		}
		flag = ServiceLocator.Instance.GetService<IAuthenticationServices>().CanSwitch();
		LogHelper.Instance.Log("KioskService -> AuthenticationService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH663", "Bundle Switch Error", "Can not switch while authentication service is performing work."));
			return false;
		}
		LogHelper.Instance.Log("KioskService CanSwitch: True");
		return true;
	}

	public ErrorList PreLoad()
	{
		return new ErrorList();
	}

	public ErrorList Load()
	{
		ErrorList result = new ErrorList();
		LogHelper.Instance.Log("Kiosk Services Kernel Extension Load.");
		ServiceLocator.Instance.GetService<IObjectCacheService>()?.SetObject("Kiosk Services Proxy", new ServiceProxy());
		ServiceLocator.Instance.AddStoreServices();
		ServiceLocator.Instance.AddService(typeof(IHealthServices), HealthServicesProxy.Instance);
		ServiceLocator.Instance.AddService(typeof(IPromotionServices), PromotionServices.Instance);
		ServiceLocator.Instance.AddAuthenticationServices();
		ServiceLocator.Instance.AddMarketingServices();
		if (GetPzSLiveCall())
		{
			ServiceLocator.Instance.AddService(typeof(IPersonalizationServices), PersonalizationServicesProxy.Instance);
		}
		else
		{
			ServiceLocator.Instance.AddService(typeof(IPersonalizationServices), PersonalizationServicesProxySimulator.Instance);
		}
		ServiceLocator.Instance.AddService(typeof(ICustomerOfferService), CustomerOfferProxy.Instance);
		ServiceLocator.Instance.AddService(typeof(ILoyaltyServices), LoyaltyServicesProxy.Instance);
		IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
		if (service != null)
		{
			service.AddPreferencePage("KioskServices_General", "Store", "Kiosk Services/General", PreferencePageTarget.LocalSystem, new GeneralPreferencePage());
			service.AddPreferencePage("KioskServices_AuthenticationServices", "Remote Services\\Authentication Services", "Kiosk Services/Authentication Services", PreferencePageTarget.LocalSystem, new AuthSvcsPreferencePage());
			service.AddPreferencePage("KioskServices_HealthServices", "Remote Services\\Health Services", "Kiosk Services/Health Services", PreferencePageTarget.LocalSystem, new HealthServicesPreferencePage());
			service.AddPreferencePage("KioskServices_PromotionServices", "Remote Services\\Promotion Services", "Kiosk Services/Promotion Services", PreferencePageTarget.LocalSystem, new PromotionServicesPreferencePage());
			service.AddPreferencePage("KoiskServices_MarketingOptinServices", "Remote Services\\Marketing Optin Services", "Kiosk Services/Marketing Optin Services", PreferencePageTarget.LocalSystem, new MarketingOptinPreferencesPage());
			service.AddPreferencePage("KioskServices_LoyaltyServices", "Remote Services\\Loyalty Services", "Kiosk Services/Loyalty Services", PreferencePageTarget.LocalSystem, new LoyaltyServicesPreferencePage());
			service.AddPreferencePage("KioskServices_PersonalizationGeneral", "Remote Services\\Personalization Services", "Kiosk Services/Personalization Services/General", PreferencePageTarget.LocalSystem, new PersonalizationGeneralPreferencePage());
			service.AddPreferencePage("KioskServices_PersonalizationUpdateAccount", "Remote Services\\Personalization Services", "Kiosk Services/Personalization Services/Update Account", PreferencePageTarget.LocalSystem, new PersonalizationUpdateAccountPreferencePage());
			service.AddPreferencePage("FraudService", "Local Services\\Fraud Services", "Kiosk Services/Fraud Services", PreferencePageTarget.LocalSystem, new FraudServicePreferencePage());
			service.AddPreferencePage("CustomerOffers", "Local Services\\Customer Offers", "Kiosk Services/Customer Offers", PreferencePageTarget.LocalSystem, new CustomerOffersPreferencePage());
		}
		return result;
	}

	public ErrorList Reset()
	{
		LogHelper.Instance.Log("Kiosk Services Kernel Extension Reset:");
		ErrorList result = new ErrorList();
		IKernelFunctionRegistryService service = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
		if (service != null)
		{
			LogHelper.Instance.Log("...Register Kiosk Services kernel functions.");
			service.RegisterKernelFunctions(typeof(KernelRegistrationObject).Assembly, "Kiosk Services");
		}
		LogHelper.Instance.Log("...Reset Kiosk Services Proxy.");
		ServiceLocator.Instance.GetService<IAuthenticationServices>().Reset();
		ServiceLocator.Instance.GetService<IPromotionServices>().Reset();
		ServiceLocator.Instance.GetService<IMarketingServices>().Reset();
		return result;
	}

	public ErrorList Activate()
	{
		LogHelper.Instance.Log("Kiosk Services Kernel Extension Activate:");
		ErrorList result = new ErrorList();
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			_defaultMessageProcessorService = new DefaultMessageProcessorService();
			service.RegisterMessageProcessorService(_defaultMessageProcessorService);
			_softSyncMessageProcessorService = new SoftSyncMessageProcessorService();
			service.RegisterMessageProcessorService(_softSyncMessageProcessorService);
		}
		ServiceLocator.Instance.GetService<IHealthServices>().StartPingTimer();
		return result;
	}

	public ErrorList Deactivate()
	{
		LogHelper.Instance.Log("Kiosk Services Kernel Extension Deactivate:");
		ErrorList result = new ErrorList();
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null && _defaultMessageProcessorService != null)
		{
			if (_defaultMessageProcessorService != null)
			{
				service.UnRegisterMessageProcessorService(_softSyncMessageProcessorService);
				_softSyncMessageProcessorService = null;
			}
			if (_softSyncMessageProcessorService != null)
			{
				service.UnRegisterMessageProcessorService(_defaultMessageProcessorService);
				_defaultMessageProcessorService = null;
			}
		}
		ServiceLocator.Instance.GetService<IHealthServices>().StopPingTimer();
		return result;
	}

	public void HandleHostCrash(Exception e)
	{
		LogHelper.Instance.Log("Kiosk Services Kernel Extension Host Crash:", e);
		ServiceLocator.Instance.GetService<IHealthServices>().SendAlert(ServiceLocator.Instance.GetService<IStoreServices>().StoreNumberString, "ApplicationCrash", null, e.ToString(), DateTime.Now, null);
	}

	public void RegisterCommunicationActivity(Action<object, EventArgs> action)
	{
		KioskServicesProxy.Instance.RegisterCommunicationActivity(action);
	}

	private static bool GetPzSLiveCall()
	{
		return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LiveCall", defaultValue: true);
	}
}
