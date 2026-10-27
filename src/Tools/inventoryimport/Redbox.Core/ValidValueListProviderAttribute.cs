using System;

namespace Redbox.Core;

public class ValidValueListProviderAttribute : Attribute
{
	public string MethodName { get; private set; }

	public ValidValueListProviderAttribute(string methodName)
	{
		MethodName = methodName;
	}
}
