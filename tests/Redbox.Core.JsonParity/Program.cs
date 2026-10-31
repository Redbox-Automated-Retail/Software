using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using Redbox.Core;

namespace Redbox.Core.JsonParity;

public enum Color
{
	Red,
	Green = 5
}

public class Item
{
	public int Id { get; set; }

	public string Name { get; set; }

	public decimal Price { get; set; }

	public double Ratio { get; set; }

	public float Single { get; set; }

	public long Big { get; set; }

	public bool Flag { get; set; }

	public DateTime When { get; set; }

	public DateTime? Maybe { get; set; }

	public TimeSpan Span { get; set; }

	public Guid Key { get; set; }

	public Color Color { get; set; }

	public char Letter { get; set; }

	public byte[] Bytes { get; set; }

	public List<string> Tags { get; set; }

	public Dictionary<string, object> Extra { get; set; }

	public Item Child { get; set; }

	[ScriptIgnore]
	public string Hidden { get; set; } = "hidden";

	public string Field = "field";

	public Uri Link { get; set; }
}

internal static class Program
{
	private static int Main()
	{
		// Same culture on both runtimes, so differences come from the serializer and not from the machine.
		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		Item item = new Item
		{
			Id = 42,
			Name = "Tab\t \"quote\" \\ </script> & ' é ü 漢字 \u0001 \u2028 \ud83c\udfac",
			Price = 3.14159265358979m,
			Ratio = 0.1 + 0.2,
			Single = 1.1f,
			Big = long.MaxValue,
			Flag = true,
			When = new DateTime(2024, 5, 7, 13, 45, 30, 123, DateTimeKind.Utc),
			Maybe = null,
			Span = TimeSpan.FromMinutes(90.5),
			Key = new Guid("02CAD7C4-E0F5-4b1b-BD04-808B840A61D2"),
			Color = Color.Green,
			Letter = 'x',
			Bytes = new byte[] { 0, 1, 254, 255 },
			Tags = new List<string> { "a", null, "" },
			Extra = new Dictionary<string, object> { ["n"] = 1, ["s"] = "t", ["d"] = 2.5, ["nil"] = null, ["arr"] = new object[] { 1, "two", false } },
			Child = new Item { Id = 1, When = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
			Link = new Uri("https://example.invalid/a?b=c")
		};
		Case("item", () => item.ToJson());
		Case("local date", () => new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Local).ToJson());
		Case("unspecified date", () => new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified).ToJson());
		Case("min/max date", () => new[] { DateTime.MinValue, DateTime.MaxValue }.ToJson());
		Case("numbers", () => new object[] { double.NaN, double.PositiveInfinity, -0.0, 1e300, 1.5e-7, (byte)7, (short)-3, 4294967295u, decimal.MaxValue }.ToJson());
		Case("hashtable", () => new Hashtable { ["k"] = "v" }.ToJson());
		Case("anonymous", () => new { A = 1, B = new[] { "x" } }.ToJson());
		Case("string", () => "plain".ToJson());
		Case("null", () => ((object)null).ToJson());
		string itemJson = item.ToJson();
		Case("round trip", () => itemJson.ToObject<Item>().ToJson());
		Case("to dictionary", () => Dump(itemJson.ToDictionary()));
		Case("parse date forms", () => Dump("{\"a\":\"\\/Date(1715089530123)\\/\",\"b\":\"/Date(0)/\",\"c\":\"\\/Date(-62135596800000)\\/\"}".ToDictionary()));
		Case("parse numbers", () => Dump("{\"i\":1,\"l\":12345678901,\"d\":1.25,\"e\":1e3,\"neg\":-0,\"big\":123456789012345678901234567890}".ToDictionary()));
		Case("parse escapes", () => Dump("{\"s\":\"a\\u0041\\n\\t\\/\\\\\\\"\\ud83c\"}".ToDictionary()));
		Case("parse to type", () => "{\"Id\":\"7\",\"Price\":\"1.50\",\"Color\":\"Green\",\"Tags\":[\"z\"],\"When\":\"\\/Date(86400000)\\/\"}".ToObject<Item>().ToJson());
		Case("bad json", () => "{\"a\":".ToDictionary() == null ? "null" : "parsed");
		Case("trailing comma", () => "[1,2,]".ToObject<List<int>>().Count.ToString());
		Case("deep nesting", () => new string('[', 120).ToObject<object>()?.ToString());
		Case("many members", () => ("{" + string.Join(",", Enumerable.Range(0, 1001).Select(i => "\"k" + i + "\":" + i)) + "}").ToDictionary()?.Count.ToString());
		Case("cycle", () =>
		{
			Item a = new Item();
			a.Child = a;
			return a.ToJson();
		});
		return 0;
	}

	private static void Case(string name, Func<string> run)
	{
		string result;
		try
		{
			result = run();
		}
		catch (Exception ex)
		{
			result = "EXCEPTION " + ex.GetType().FullName + ": " + ex.Message;
		}
		Console.WriteLine("### " + name);
		Console.WriteLine(result ?? "<null>");
	}

	// Type and value of each entry, so number types (int/long/decimal/double) are compared too.
	private static string Dump(object o)
	{
		switch (o)
		{
		case null:
			return "null";
		case IDictionary<string, object> dict:
			return "{" + string.Join(",", dict.Select(p => p.Key + "=" + Dump(p.Value))) + "}";
		case string s:
			return "string:" + s.ToJson();
		case IEnumerable list:
			return "[" + string.Join(",", list.Cast<object>().Select(Dump)) + "]";
		case DateTime d:
			return "DateTime:" + d.Kind + ":" + d.ToString("o", CultureInfo.InvariantCulture);
		default:
			// "R" for floating point: the default ToString changed in .NET Core 3.0.
			return o.GetType().Name + ":" + (o is double dbl ? dbl.ToString("R", CultureInfo.InvariantCulture) : Convert.ToString(o, CultureInfo.InvariantCulture));
		}
	}
}
