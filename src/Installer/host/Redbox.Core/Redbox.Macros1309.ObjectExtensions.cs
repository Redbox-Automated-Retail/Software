namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EObjectExtensions
{
	public static string ToJson(this object instance)
	{
		return Redbox_002EMacros1309_002EJavaScriptConverterRegistry.Instance.GetSerializer().Serialize(instance);
	}
}
