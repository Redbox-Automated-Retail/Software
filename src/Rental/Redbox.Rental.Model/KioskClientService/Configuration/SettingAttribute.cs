using System;

namespace Redbox.Rental.Model.KioskClientService.Configuration;

public class SettingAttribute : Attribute
{
	public string Name { get; set; }
}
