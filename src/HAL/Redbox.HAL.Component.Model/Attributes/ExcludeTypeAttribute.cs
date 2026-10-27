using System;

namespace Redbox.HAL.Component.Model.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ExcludeTypeAttribute : Attribute
{
}
