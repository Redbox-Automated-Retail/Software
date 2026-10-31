using System;
using System.Collections.Generic;

namespace BrokerGtcp.Check;

/// <summary>Prints one PASS/FAIL line per check and counts the failures.</summary>
internal static class Check
{
	public static int Passed { get; private set; }

	public static int Failed { get; private set; }

	public static bool That(bool condition, string name, string detail = null)
	{
		if (condition)
		{
			Passed++;
		}
		else
		{
			Failed++;
		}
		Console.WriteLine((condition ? "PASS " : "FAIL ") + name + (detail == null ? "" : " (" + detail + ")"));
		return condition;
	}

	public static bool Equal<T>(T expected, T actual, string name)
	{
		bool equal = EqualityComparer<T>.Default.Equals(expected, actual);
		return That(equal, name, equal ? null : $"expected {Show(expected)}, got {Show(actual)}");
	}

	public static bool Bytes(byte[] expected, byte[] actual, string name)
	{
		if (expected.AsSpan().SequenceEqual(actual))
		{
			return That(true, name, $"{actual.Length} bytes identical");
		}
		int index = 0;
		while (index < expected.Length && index < actual.Length && expected[index] == actual[index])
		{
			index++;
		}
		return That(false, name, $"first difference at offset 0x{index:x} (expected {expected.Length} bytes, got {actual.Length})");
	}

	/// <summary>Runs a check body; an exception counts as a failure of that check.</summary>
	public static void Run(string name, Action body)
	{
		try
		{
			body();
		}
		catch (Exception e)
		{
			That(false, name, e.GetType().Name + ": " + e.Message);
		}
	}

	private static string Show<T>(T value) => value switch
	{
		null => "null",
		DateTime d => d.ToString("o") + " " + d.Kind,
		string s => "\"" + s + "\"",
		_ => value.ToString()
	};
}
