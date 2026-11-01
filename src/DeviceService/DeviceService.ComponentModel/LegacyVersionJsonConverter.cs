using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeviceService.ComponentModel;

// Added on the modernize branch (not decompiled).
//
// Newtonsoft.Json wrote System.Version as an object ({"major":1,"minor":2,"build":3,"revision":4,...}) on
// .NET Framework and .NET Core 2.x, where Version had no TypeConverter. On .NET 5+ Version has one, so Newtonsoft
// treats it as a string and rejects the object form. The engine's DeviceService.Client (SignalR 1.1 on .NET
// Framework) still sends and expects the object form, so the Device Service reads both forms and writes the object
// form, as ASP.NET Core 2.2 did.
public sealed class LegacyVersionJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Version);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		switch (reader.TokenType)
		{
		case JsonToken.Null:
			return null;
		case JsonToken.String:
			return Version.Parse((string)reader.Value);
		case JsonToken.StartObject:
		{
			JObject versionObject = JObject.Load(reader);
			int major = Component(versionObject, "major");
			int minor = Component(versionObject, "minor");
			int build = Component(versionObject, "build");
			int revision = Component(versionObject, "revision");
			// The Version constructors reject -1 (an unset component), so pick the one that matches what is set.
			if (build < 0)
			{
				return new Version(major, minor);
			}
			if (revision < 0)
			{
				return new Version(major, minor, build);
			}
			return new Version(major, minor, build, revision);
		}
		default:
			throw new JsonSerializationException($"Unexpected token {reader.TokenType} for System.Version.");
		}
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (!(value is Version version))
		{
			writer.WriteNull();
			return;
		}
		// Property order and names as Newtonsoft's default contract (with camelCase) produced them.
		writer.WriteStartObject();
		writer.WritePropertyName("major");
		writer.WriteValue(version.Major);
		writer.WritePropertyName("minor");
		writer.WriteValue(version.Minor);
		writer.WritePropertyName("build");
		writer.WriteValue(version.Build);
		writer.WritePropertyName("revision");
		writer.WriteValue(version.Revision);
		writer.WritePropertyName("majorRevision");
		writer.WriteValue(version.MajorRevision);
		writer.WritePropertyName("minorRevision");
		writer.WriteValue(version.MinorRevision);
		writer.WriteEndObject();
	}

	private static int Component(JObject obj, string name)
	{
		JToken token = obj.GetValue(name, StringComparison.OrdinalIgnoreCase);
		return token == null || token.Type == JTokenType.Null ? -1 : token.Value<int>();
	}
}
