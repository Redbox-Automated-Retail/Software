using System;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;

namespace Redbox.NetCore.Logging.Serialization;

public class SensitiveDataConverter : JsonConverter
{
	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return false;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue((value is string source) ? source.MaskLogValue(4) : string.Empty);
	}
}
