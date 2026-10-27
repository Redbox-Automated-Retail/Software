namespace KioskClientService.API.Services.Configuration;

[Category(Name = "ABTesting")]
public class ABTestingConfiguration : BaseCategorySetting
{
	public string OutOfStockTitleDetails { get; set; } = "A";

	public int ScreenSaverTimeout { get; set; } = 25000;
}
