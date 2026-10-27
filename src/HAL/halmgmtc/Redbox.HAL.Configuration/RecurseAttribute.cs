using System;

namespace Redbox.HAL.Configuration;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class RecurseAttribute : Attribute
{
}
