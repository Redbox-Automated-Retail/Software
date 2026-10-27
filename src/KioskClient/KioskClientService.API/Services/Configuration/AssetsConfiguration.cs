namespace KioskClientService.API.Services.Configuration;

[Category(Name = "Assets")]
public class AssetsConfiguration : BaseCategorySetting, IAssetsConfiguration
{
	public int LifespanDays { get; set; } = 60;
}
