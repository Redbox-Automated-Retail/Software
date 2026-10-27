using System.Net;

namespace Redbox.Macros.Functions;

[FunctionSet("dns", "DNS")]
public class DnsFunctions : FunctionSetBase
{
	public DnsFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("get-host-name")]
	public static string GetHostName()
	{
		return Dns.GetHostName();
	}
}
