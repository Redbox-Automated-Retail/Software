using System;

namespace Redbox.Rental.Model.KioskClientService.Configuration;

public class MaskLogValue : Attribute
{
	public int VisibleChars { get; set; }
}
