using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

[AttributeUsage(AttributeTargets.Method)]
public class TweenFunctionAttribute : Attribute
{
	public TweenType Type { get; set; }
}
