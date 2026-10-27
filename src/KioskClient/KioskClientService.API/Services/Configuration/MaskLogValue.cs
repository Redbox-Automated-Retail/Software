using System;

namespace KioskClientService.API.Services.Configuration;

public class MaskLogValue : Attribute
{
	public int VisibleChars { get; set; }
}
