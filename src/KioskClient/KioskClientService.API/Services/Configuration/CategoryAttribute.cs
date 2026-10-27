using System;

namespace KioskClientService.API.Services.Configuration;

public class CategoryAttribute : Attribute
{
	public string Name { get; set; }
}
