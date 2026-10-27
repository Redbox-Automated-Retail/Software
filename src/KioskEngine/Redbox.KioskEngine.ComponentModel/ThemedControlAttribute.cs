using System;

namespace Redbox.KioskEngine.ComponentModel;

public class ThemedControlAttribute : Attribute
{
	public string ThemeName { get; set; }

	public string ControlName { get; set; }
}
