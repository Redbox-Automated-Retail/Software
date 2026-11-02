using System;

namespace Redbox.KioskEngine.ComponentModel;

public class ThemedControlAttribute : Attribute
{
	// Both are set where the attribute is applied ([ThemedControl(ThemeName = ..., ControlName = ...)]).
	public string ThemeName { get; set; } = null!;

	public string ControlName { get; set; } = null!;
}
