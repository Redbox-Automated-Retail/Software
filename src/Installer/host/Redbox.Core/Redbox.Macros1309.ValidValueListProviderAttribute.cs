using System;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002EValidValueListProviderAttribute : Attribute
{
	public string MethodName { get; private set; }

	public Redbox_002EMacros1309_002EValidValueListProviderAttribute(string methodName)
	{
		MethodName = methodName;
	}
}
