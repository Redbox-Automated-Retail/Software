using System;

namespace Redbox.KioskEngine.Bootstrap;

[AttributeUsage(AttributeTargets.Method)]
public class EngineCommandAttribute : Attribute
{
	// Set where the attribute is applied ([EngineCommand(Name = ...)]).
	public string Name { get; set; } = null!;
}
