using System;

namespace Redbox.KioskEngine.ComponentModel;

[AttributeUsage(AttributeTargets.Class)]
public class ViewDefinitionAttribute : Attribute
{
	// Set where the attribute is applied ([ViewDefinition(ViewName = ...)]).
	public string ViewName { get; set; } = null!;
}
