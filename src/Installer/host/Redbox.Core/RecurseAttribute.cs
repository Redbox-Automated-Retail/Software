using System;

namespace Redbox.Core;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class RecurseAttribute : Attribute
{
}
