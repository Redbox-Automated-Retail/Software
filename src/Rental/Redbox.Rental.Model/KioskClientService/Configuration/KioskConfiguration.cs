namespace Redbox.Rental.Model.KioskClientService.Configuration;

public class KioskConfiguration : BaseConfiguration
{
	public DeviceServiceConfiguration DeviceService { get; set; } = new DeviceServiceConfiguration();

	public BrowseConfiguration Browse { get; set; } = new BrowseConfiguration();

	public CMSConfiguration CMS { get; set; } = new CMSConfiguration();

	public _KioskConfiguration Kiosk { get; set; } = new _KioskConfiguration();

	public DualConfiguration Dual { get; set; } = new DualConfiguration();

	public InventoryConfiguration Inventory { get; set; } = new InventoryConfiguration();

	public MarketingGeneralConfiguration MarketingGeneral { get; set; } = new MarketingGeneralConfiguration();

	public MarketingAdConfiguration MarketingAd { get; set; } = new MarketingAdConfiguration();

	public MarketingReturnConfiguration MarketingReturn { get; set; } = new MarketingReturnConfiguration();

	public ABTestingConfiguration ABTesting { get; set; } = new ABTestingConfiguration();

	public KioskHealthConfiguration KioskHealth { get; set; } = new KioskHealthConfiguration();
}
