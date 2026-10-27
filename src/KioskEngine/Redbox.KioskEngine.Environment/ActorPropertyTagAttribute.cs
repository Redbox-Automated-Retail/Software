using System;

namespace Redbox.KioskEngine.Environment;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal class ActorPropertyTagAttribute : Attribute
{
	public string Name { get; set; }
}
