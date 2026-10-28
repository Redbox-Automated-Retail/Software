using System;

namespace Redbox.HAL.Core;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ExcludeTypeAttribute : Attribute
{
}
