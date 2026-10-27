using System;

namespace KioskClientService.API.Services.Configuration;

public class SettingAttribute : Attribute
{
	public string Name { get; set; }
}
