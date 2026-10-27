namespace KioskClientService.API.Services.Configuration;

[Category(Name = "ProfileData")]
public class ProfileDataConfiguration : BaseCategorySetting
{
	public bool ProfileDataUpdateEnabled { get; set; }
}
