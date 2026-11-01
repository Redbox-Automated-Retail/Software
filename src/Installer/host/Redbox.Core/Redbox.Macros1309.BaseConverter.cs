using System;
using System.Text;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002EBaseConverter
{
	private readonly string _toNumberingScheme;

	private readonly string _fromNumberingScheme;

	private readonly int _maxFromSchemeCharacter;

	private static readonly Redbox_002EMacros1309_002EBaseConverter _binToOct = new Redbox_002EMacros1309_002EBaseConverter(2, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 8, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _binToDec = new Redbox_002EMacros1309_002EBaseConverter(2, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 10, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _binToHex = new Redbox_002EMacros1309_002EBaseConverter(2, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 16, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _octToBin = new Redbox_002EMacros1309_002EBaseConverter(8, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 2, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _octToDec = new Redbox_002EMacros1309_002EBaseConverter(8, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 10, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _octToHex = new Redbox_002EMacros1309_002EBaseConverter(8, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 16, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _decToBin = new Redbox_002EMacros1309_002EBaseConverter(10, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 2, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _decToOct = new Redbox_002EMacros1309_002EBaseConverter(10, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 8, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _decToHex = new Redbox_002EMacros1309_002EBaseConverter(10, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 16, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _hexToBin = new Redbox_002EMacros1309_002EBaseConverter(16, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 2, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _hexToOct = new Redbox_002EMacros1309_002EBaseConverter(16, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 8, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	private static readonly Redbox_002EMacros1309_002EBaseConverter _hexToDec = new Redbox_002EMacros1309_002EBaseConverter(16, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 10, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);

	public static Redbox_002EMacros1309_002EBaseConverter BinToOct => _binToOct;

	public static Redbox_002EMacros1309_002EBaseConverter BinToDec => _binToDec;

	public static Redbox_002EMacros1309_002EBaseConverter BinToHex => _binToHex;

	public static Redbox_002EMacros1309_002EBaseConverter OctToBin => _octToBin;

	public static Redbox_002EMacros1309_002EBaseConverter OctToDec => _octToDec;

	public static Redbox_002EMacros1309_002EBaseConverter OctToHex => _octToHex;

	public static Redbox_002EMacros1309_002EBaseConverter DecToBin => _decToBin;

	public static Redbox_002EMacros1309_002EBaseConverter DecToOct => _decToOct;

	public static Redbox_002EMacros1309_002EBaseConverter DecToHex => _decToHex;

	public static Redbox_002EMacros1309_002EBaseConverter HexToBin => _hexToBin;

	public static Redbox_002EMacros1309_002EBaseConverter HexToOct => _hexToOct;

	public static Redbox_002EMacros1309_002EBaseConverter HexToDec => _hexToDec;

	public int From { get; private set; }

	public int To { get; private set; }

	public static Redbox_002EMacros1309_002EBaseConverter Create(int fromRadix)
	{
		return new Redbox_002EMacros1309_002EBaseConverter(fromRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 10, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);
	}

	public static Redbox_002EMacros1309_002EBaseConverter Create(Redbox_002EMacros1309_002ENumberBases fromRadix)
	{
		return new Redbox_002EMacros1309_002EBaseConverter((int)fromRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, 10, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);
	}

	public static Redbox_002EMacros1309_002EBaseConverter Create(Redbox_002EMacros1309_002ENumberBases fromRadix, Redbox_002EMacros1309_002ENumberBases toRadix)
	{
		return new Redbox_002EMacros1309_002EBaseConverter((int)fromRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, (int)toRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);
	}

	public static Redbox_002EMacros1309_002EBaseConverter Create(int fromRadix, int toRadix)
	{
		return new Redbox_002EMacros1309_002EBaseConverter(fromRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, toRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ);
	}

	public static Redbox_002EMacros1309_002EBaseConverter Create(Redbox_002EMacros1309_002ENumberBases fromRadix, Redbox_002EMacros1309_002ENumberingSchemes fromScheme, Redbox_002EMacros1309_002ENumberBases toRadix, Redbox_002EMacros1309_002ENumberingSchemes toScheme)
	{
		return new Redbox_002EMacros1309_002EBaseConverter((int)fromRadix, fromScheme, (int)toRadix, toScheme);
	}

	public static Redbox_002EMacros1309_002EBaseConverter Create(int fromRadix, Redbox_002EMacros1309_002ENumberingSchemes fromScheme, int toRadix, Redbox_002EMacros1309_002ENumberingSchemes toScheme)
	{
		return new Redbox_002EMacros1309_002EBaseConverter(fromRadix, fromScheme, toRadix, toScheme);
	}

	public static Redbox_002EMacros1309_002EBaseConverter Create(int fromRadix, Redbox_002EMacros1309_002ENumberingSchemes fromScheme, Redbox_002EMacros1309_002ENumberBases toRadix, Redbox_002EMacros1309_002ENumberingSchemes toScheme)
	{
		return new Redbox_002EMacros1309_002EBaseConverter(fromRadix, fromScheme, (int)toRadix, toScheme);
	}

	public static Redbox_002EMacros1309_002EBaseConverter Create(Redbox_002EMacros1309_002ENumberBases fromRadix, Redbox_002EMacros1309_002ENumberingSchemes fromScheme, int toRadix, Redbox_002EMacros1309_002ENumberingSchemes toScheme)
	{
		return new Redbox_002EMacros1309_002EBaseConverter((int)fromRadix, fromScheme, toRadix, toScheme);
	}

	public static string Convert(Redbox_002EMacros1309_002ENumberBases fromRadix, string value)
	{
		return Convert((int)fromRadix, 10, value);
	}

	public static string Convert(int fromRadix, string value)
	{
		return Convert(fromRadix, 10, value);
	}

	public static string Convert(Redbox_002EMacros1309_002ENumberBases fromRadix, Redbox_002EMacros1309_002ENumberBases toRadix, string value)
	{
		return Convert((int)fromRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, (int)toRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, value);
	}

	public static string Convert(Redbox_002EMacros1309_002ENumberBases fromRadix, int toRadix, string value)
	{
		return Convert((int)fromRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, toRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, value);
	}

	public static string Convert(int fromRadix, int toRadix, string value)
	{
		return Convert(fromRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, toRadix, Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ, value);
	}

	public static string Convert(Redbox_002EMacros1309_002ENumberBases fromRadix, Redbox_002EMacros1309_002ENumberingSchemes fromScheme, Redbox_002EMacros1309_002ENumberBases toRadix, Redbox_002EMacros1309_002ENumberingSchemes toScheme, string value)
	{
		return Convert((int)fromRadix, fromScheme, (int)toRadix, toScheme, value);
	}

	public static string Convert(Redbox_002EMacros1309_002ENumberBases fromRadix, Redbox_002EMacros1309_002ENumberingSchemes fromScheme, int toRadix, Redbox_002EMacros1309_002ENumberingSchemes toScheme, string value)
	{
		return Convert((int)fromRadix, fromScheme, toRadix, toScheme, value);
	}

	public static string Convert(int fromRadix, Redbox_002EMacros1309_002ENumberingSchemes fromScheme, Redbox_002EMacros1309_002ENumberBases toRadix, Redbox_002EMacros1309_002ENumberingSchemes toScheme, string value)
	{
		return Convert(fromRadix, fromScheme, (int)toRadix, toScheme, value);
	}

	public static string Convert(int fromRadix, Redbox_002EMacros1309_002ENumberingSchemes fromScheme, int toRadix, Redbox_002EMacros1309_002ENumberingSchemes toScheme, string value)
	{
		Redbox_002EMacros1309_002EBaseConverter converter = new Redbox_002EMacros1309_002EBaseConverter(fromRadix, fromScheme, toRadix, toScheme);
		return converter.Convert(value);
	}

	public string Convert(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new ArgumentNullException("value");
		}
		long base10Value = ConvertToBase10(value, From, _fromNumberingScheme, _maxFromSchemeCharacter);
		if (To != 10)
		{
			return ConvertFromBase10(base10Value, To, _toNumberingScheme);
		}
		return base10Value.ToString();
	}

	private Redbox_002EMacros1309_002EBaseConverter(int fromRadix, Redbox_002EMacros1309_002ENumberingSchemes fromScheme, int toRadix, Redbox_002EMacros1309_002ENumberingSchemes toScheme)
	{
		if (fromRadix < 2 || fromRadix > 36)
		{
			throw new ArgumentOutOfRangeException("fromRadix", "Radix can be from 2 to 36 inclusive");
		}
		if (toRadix < 2 || toRadix > 36)
		{
			throw new ArgumentOutOfRangeException("toRadix", "Radix can be from 2 to 36 inclusive");
		}
		if (fromRadix > 26 && fromScheme == Redbox_002EMacros1309_002ENumberingSchemes.AToZ)
		{
			throw new ArgumentOutOfRangeException("fromRadix", "Invalid numbering scheme for specified number base");
		}
		if (toRadix > 26 && fromScheme == Redbox_002EMacros1309_002ENumberingSchemes.AToZ)
		{
			throw new ArgumentOutOfRangeException("toRadix", "Invalid numbering scheme for specified number base");
		}
		From = fromRadix;
		_fromNumberingScheme = GetCharactersForNumberingScheme(fromScheme);
		To = toRadix;
		_toNumberingScheme = GetCharactersForNumberingScheme(toScheme);
		_maxFromSchemeCharacter = ((fromScheme == Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ) ? fromRadix : (fromRadix + 1));
	}

	private static string GetCharactersForNumberingScheme(Redbox_002EMacros1309_002ENumberingSchemes scheme)
	{
		return scheme switch
		{
			Redbox_002EMacros1309_002ENumberingSchemes.AToZ => "_ABCDEFGHIJKLMNOPQRSTUVWXYZ", 
			Redbox_002EMacros1309_002ENumberingSchemes.ZeroToZ => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", 
			_ => throw new ArgumentOutOfRangeException("scheme"), 
		};
	}

	private static long ConvertToBase10(string value, int fromBase, string characters, int maxFromSchemeCharacter)
	{
		StringBuilder stringBuilder = new StringBuilder(value);
		int position = 0;
		long result = 0L;
		while (stringBuilder.Length > 0)
		{
			int index = Array.IndexOf(characters.ToCharArray(), stringBuilder[stringBuilder.Length - 1]);
			if (index < 0)
			{
				throw new FormatException("Unsupported character in value string");
			}
			if (index >= maxFromSchemeCharacter)
			{
				throw new FormatException("Value contains character not valid for number base");
			}
			result += index * (long)Math.Pow(fromBase, position);
			if (result < 0)
			{
				throw new OverflowException();
			}
			stringBuilder.Length--;
			position++;
		}
		return result;
	}

	private static string ConvertFromBase10(long value, int toBase, string characters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (value > 0)
		{
			int index = (int)(value % toBase);
			stringBuilder.Insert(0, characters[index]);
			value /= toBase;
		}
		return stringBuilder.ToString();
	}
}
