using Redbox.Core;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Ads;
using Redbox.Rental.Model.KioskClientService.Application;
using Redbox.Rental.Model.KioskClientService.Authentication;
using Redbox.Rental.Model.KioskClientService.Campaign;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.KioskClientService.CustomerProfile;
using Redbox.Rental.Model.KioskClientService.Data;
using Redbox.Rental.Model.KioskClientService.Dual;
using Redbox.Rental.Model.KioskClientService.Installer;
using Redbox.Rental.Model.KioskClientService.Inventory;
using Redbox.Rental.Model.KioskClientService.Kiosk;
using Redbox.Rental.Model.KioskClientService.Loyalty;
using Redbox.Rental.Model.KioskClientService.Marketing;
using Redbox.Rental.Model.KioskClientService.Personalization;
using Redbox.Rental.Model.KioskClientService.Planogram;
using Redbox.Rental.Model.KioskClientService.Product;
using Redbox.Rental.Model.KioskClientService.Products;
using Redbox.Rental.Model.KioskClientService.Promotion;
using Redbox.Rental.Model.KioskClientService.Session;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskClientService.TestData;
using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Model.KioskClientService.Utilities;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.Services.KioskClientService.Application;
using Redbox.Rental.Services.KioskClientService.Authentication;
using Redbox.Rental.Services.KioskClientService.Campaign;
using Redbox.Rental.Services.KioskClientService.Configuration;
using Redbox.Rental.Services.KioskClientService.CustomerProfile;
using Redbox.Rental.Services.KioskClientService.Data;
using Redbox.Rental.Services.KioskClientService.Dual;
using Redbox.Rental.Services.KioskClientService.Installer;
using Redbox.Rental.Services.KioskClientService.Inventory;
using Redbox.Rental.Services.KioskClientService.Kiosk;
using Redbox.Rental.Services.KioskClientService.Loyalty;
using Redbox.Rental.Services.KioskClientService.Marketing;
using Redbox.Rental.Services.KioskClientService.Personalization;
using Redbox.Rental.Services.KioskClientService.Planogram;
using Redbox.Rental.Services.KioskClientService.Product;
using Redbox.Rental.Services.KioskClientService.Products;
using Redbox.Rental.Services.KioskClientService.Promotion;
using Redbox.Rental.Services.KioskClientService.Session;
using Redbox.Rental.Services.KioskClientService.Subscriptions;
using Redbox.Rental.Services.KioskClientService.TestData;
using Redbox.Rental.Services.KioskClientService.Transactions;
using Redbox.Rental.Services.KioskClientService.Utilities;

namespace Redbox.Rental.Services.KioskClientService;

public class KioskClientService : IKioskClientService
{
	public static KioskClientService Instance => Singleton<KioskClientService>.Instance;

	public IKioskClientServiceAds Ads { get; private set; }

	public IKioskClientServiceApplication Application { get; private set; }

	public IKioskClientServiceAuthentication Authentication { get; private set; }

	public IKioskClientServiceCampaign Campaign { get; private set; }

	public IKioskClientServiceCustomerProfile CustomerProfile { get; private set; }

	public IKioskClientServiceData Data { get; private set; }

	public IKioskClientServiceDual Dual { get; private set; }

	public IKioskClientServiceInstaller Installer { get; private set; }

	public IKioskClientServiceInventory Inventory { get; private set; }

	public IKioskClientServiceKiosk Kiosk { get; private set; }

	public IKioskClientServiceLoyalty Loyalty { get; private set; }

	public IKioskClientServiceMarketing Marketing { get; private set; }

	public IKioskClientServicePersonalization Personalization { get; private set; }

	public IKioskClientServicePlanogram Planogram { get; private set; }

	public IKioskClientServiceProduct Product { get; private set; }

	public IKioskClientServiceProductGroups ProductGroups { get; private set; }

	public IKioskClientServicePromotion Promotion { get; private set; }

	public IKioskClientServiceQRCodeUtilities QRCodeUtilities { get; private set; }

	public IKioskClientServiceSession Session { get; private set; }

	public IKioskClientServiceSubscriptions Subscriptions { get; private set; }

	public IKioskClientServiceTestData TestData { get; private set; }

	public IKioskClientServiceTransaction Transaction { get; private set; }

	public IKioskClientServiceConfiguration Configuration { get; private set; }

	private KioskClientService()
	{
		Ads = new KioskClientServiceAds();
		Application = new KioskClientServiceApplication();
		Authentication = new KioskClientServicesAuthentication();
		Campaign = new KioskClientServiceCampaign();
		CustomerProfile = new KioskClientServiceCustomerProfile();
		Data = new KioskClientServiceData();
		Dual = new KioskClientServiceDual();
		Installer = new KioskClientServiceInstaller();
		Inventory = new KioskClientServiceInventory();
		Kiosk = new KioskClientServiceKiosk();
		Loyalty = new KioskClientServiceLoyalty();
		Marketing = new KioskClientServiceMarketing();
		Personalization = new KioskClientServicePersonalization();
		Product = new KioskClientServiceProduct();
		ProductGroups = new KioskClientServiceProductGroups();
		Promotion = new KioskClientServicePromotion();
		QRCodeUtilities = new KioskClientServiceQRCodeUtilities();
		Session = new KioskClientServiceSession();
		Subscriptions = new KioskClientServiceSubscriptions();
		TestData = new KioskClientServiceTestData();
		Transaction = new KioskClientServiceTransaction();
		Planogram = new KioskClientServicePlanogram();
		Configuration = new KioskClientServiceConfiguration();
	}
}
