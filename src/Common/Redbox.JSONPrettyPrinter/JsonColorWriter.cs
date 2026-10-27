using Jayrock.Json;

namespace Redbox.JSONPrettyPrinter;

internal sealed class JsonColorWriter : JsonWriter
{
	private readonly JsonWriter inner;

	private JsonPalette palette;

	public JsonWriter InnerWriter => inner;

	public JsonPalette Palette
	{
		get
		{
			return palette;
		}
		set
		{
			palette = value;
		}
	}

	public override int Index => inner.Index;

	public override JsonWriterBracket Bracket => inner.Bracket;

	public override int Depth => inner.Depth;

	public JsonColorWriter(JsonWriter inner)
		: this(inner, null)
	{
	}

	public JsonColorWriter(JsonWriter inner, JsonPalette palette)
	{
		this.inner = inner;
		this.palette = palette ?? JsonPalette.Auto();
	}

	public override void WriteStartObject()
	{
		Palette.Object.Apply();
		inner.WriteStartObject();
	}

	public override void WriteEndObject()
	{
		Palette.Object.Apply();
		inner.WriteEndObject();
	}

	public override void WriteMember(string name)
	{
		Palette.Member.Apply();
		inner.WriteMember(name);
	}

	public override void WriteStartArray()
	{
		Palette.Array.Apply();
		inner.WriteStartArray();
	}

	public override void WriteEndArray()
	{
		Palette.Array.Apply();
		inner.WriteEndArray();
	}

	public override void WriteString(string value)
	{
		Palette.String.Apply();
		inner.WriteString(value);
	}

	public override void WriteNumber(string value)
	{
		Palette.Number.Apply();
		inner.WriteNumber(value);
	}

	public override void WriteBoolean(bool value)
	{
		Palette.Boolean.Apply();
		inner.WriteBoolean(value);
	}

	public override void WriteNull()
	{
		Palette.Null.Apply();
		inner.WriteNull();
	}

	public override void Flush()
	{
		inner.Flush();
	}

	public override void Close()
	{
		inner.Close();
	}
}
