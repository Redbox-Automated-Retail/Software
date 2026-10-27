using System;
using Newtonsoft.Json;

namespace Redbox.NetCore.Middleware.Attributes;

public class TrimmingConverter : JsonConverter
{
	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(string);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return ((string)reader.Value)?.Trim();
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
