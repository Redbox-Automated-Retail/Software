using System;
using System.Reflection;
using System.Web.Script.Serialization;

namespace Redbox.Core;

public static class JavaScriptSerializerExtensions
{
	public static object Deserialize(this JavaScriptSerializer serializer, Type type, string json)
	{
		MethodInfo methodInfo = serializer.GetType().GetMethod("Deserialize", new Type[1] { typeof(string) }).MakeGenericMethod(type);
		object[] parameters = new string[1] { json };
		return methodInfo.Invoke(serializer, parameters);
	}
}
