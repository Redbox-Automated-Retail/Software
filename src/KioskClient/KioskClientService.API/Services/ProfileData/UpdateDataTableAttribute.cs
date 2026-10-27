using System;

namespace KioskClientService.API.Services.ProfileData;

[AttributeUsage(AttributeTargets.Class)]
public class UpdateDataTableAttribute : Attribute
{
	public UpdateDataTable UpdateDataTable { get; set; }
}
