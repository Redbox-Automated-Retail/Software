using System;

namespace Redbox.HAL.Core;

public class ValidValueListProviderAttribute : Attribute
{
	public string MethodName { get; private set; }

	public ValidValueListProviderAttribute(string methodName)
	{
		MethodName = methodName;
	}
}
