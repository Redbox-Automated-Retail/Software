using System;
using System.Configuration;
using System.IO;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace Redbox.JSONPrettyPrinter;

[Serializable]
internal sealed class JsonPalette : IJsonImportable
{
	private ConsoleBrush defaultBrush;

	private ConsoleBrush nullBrush;

	private ConsoleBrush stringBrush;

	private ConsoleBrush numberBrush;

	private ConsoleBrush booleanBrush;

	private ConsoleBrush objectBrush;

	private ConsoleBrush memberBrush;

	private ConsoleBrush arrayBrush;

	public ConsoleBrush DefaultBrush
	{
		get
		{
			return defaultBrush;
		}
		set
		{
			defaultBrush = value;
		}
	}

	public ConsoleBrush Null
	{
		get
		{
			return nullBrush;
		}
		set
		{
			nullBrush = value;
		}
	}

	public ConsoleBrush String
	{
		get
		{
			return stringBrush;
		}
		set
		{
			stringBrush = value;
		}
	}

	public ConsoleBrush Number
	{
		get
		{
			return numberBrush;
		}
		set
		{
			numberBrush = value;
		}
	}

	public ConsoleBrush Boolean
	{
		get
		{
			return booleanBrush;
		}
		set
		{
			booleanBrush = value;
		}
	}

	public ConsoleBrush Object
	{
		get
		{
			return objectBrush;
		}
		set
		{
			objectBrush = value;
		}
	}

	public ConsoleBrush Member
	{
		get
		{
			return memberBrush;
		}
		set
		{
			memberBrush = value;
		}
	}

	public ConsoleBrush Array
	{
		get
		{
			return arrayBrush;
		}
		set
		{
			arrayBrush = value;
		}
	}

	public JsonPalette()
		: this(ConsoleBrush.Current)
	{
	}

	public JsonPalette(ConsoleBrush defaultBrush)
	{
		this.defaultBrush = defaultBrush;
		nullBrush = defaultBrush;
		stringBrush = defaultBrush;
		numberBrush = defaultBrush;
		booleanBrush = defaultBrush;
		objectBrush = defaultBrush;
		memberBrush = defaultBrush;
		arrayBrush = defaultBrush;
	}

	public static JsonPalette Auto()
	{
		return Auto(ConsoleBrush.Current);
	}

	public static JsonPalette Auto(ConsoleBrush defaultBrush)
	{
		string text;
		try
		{
			text = "{nil=Gray,str=Yellow,num=Green,bit=Magenta,obj=Red,arr=Red,mem=Cyan}";
		}
		catch (SettingsPropertyNotFoundException)
		{
			text = null;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "{nil=Gray,str=Yellow,num=Green,bit=Magenta,obj=Red,arr=Red,mem=Cyan}";
			defaultBrush = new ConsoleBrush(ConsoleColor.White, ConsoleColor.Black);
		}
		JsonPalette jsonPalette = new JsonPalette(defaultBrush);
		jsonPalette.ImportJson(text);
		return jsonPalette;
	}

	public void ImportJson(string text)
	{
		ImportJson(new JsonTextReader(new StringReader(text)));
	}

	public void ImportJson(JsonReader reader)
	{
		((IJsonImportable)this).Import(new ImportContext(), reader);
	}

	void IJsonImportable.Import(ImportContext context, JsonReader reader)
	{
		reader.MoveToContent();
		if (reader.TokenClass != JsonTokenClass.Object)
		{
			reader.Skip();
			return;
		}
		reader.Read();
		do
		{
			string text = reader.ReadMember().ToLowerInvariant();
			ConsoleColor value = EnumHelper.TryParse<ConsoleColor>(reader.ReadString(), ignoreCase: true) ?? DefaultBrush.Foreground;
			switch (text)
			{
			case "arr":
			case "array":
				Array = Array.ResetForeground(value);
				break;
			case "obj":
			case "object":
				Object = Object.ResetForeground(value);
				break;
			case "mem":
			case "member":
				Member = Member.ResetForeground(value);
				break;
			case "str":
			case "string":
				String = String.ResetForeground(value);
				break;
			case "num":
			case "number":
				Number = Number.ResetForeground(value);
				break;
			case "bit":
			case "boolean":
				Boolean = Boolean.ResetForeground(value);
				break;
			case "nil":
			case "null":
				Null = Null.ResetForeground(value);
				break;
			}
		}
		while (reader.TokenClass != JsonTokenClass.EndObject);
		reader.Read();
	}
}
