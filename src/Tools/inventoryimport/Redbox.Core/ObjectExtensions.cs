using System.Web.Script.Serialization;

namespace Redbox.Core;

public static class ObjectExtensions
{
	public static string ToJson(this object instance)
	{
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = int.MaxValue;
		JavaScriptSerializer javaScriptSerializer2 = javaScriptSerializer;
		javaScriptSerializer2.RegisterConverters(new JavaScriptConverter[2]
		{
			new DateTimeConverter(),
			new TimeSpanConverter()
		});
		return javaScriptSerializer2.Serialize(instance);
	}
}
