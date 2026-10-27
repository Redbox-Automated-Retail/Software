using System;
using System.Text;

namespace Redbox.HAL.Core;

public class BaseConverter
{
	private readonly string _toNumberingScheme;

	private readonly string _fromNumberingScheme;

	private readonly int _maxFromSchemeCharacter;

	private static readonly BaseConverter _binToOct = new BaseConverter(2, NumberingSchemes.ZeroToZ, 8, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _binToDec = new BaseConverter(2, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _binToHex = new BaseConverter(2, NumberingSchemes.ZeroToZ, 16, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _octToBin = new BaseConverter(8, NumberingSchemes.ZeroToZ, 2, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _octToDec = new BaseConverter(8, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _octToHex = new BaseConverter(8, NumberingSchemes.ZeroToZ, 16, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _decToBin = new BaseConverter(10, NumberingSchemes.ZeroToZ, 2, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _decToOct = new BaseConverter(10, NumberingSchemes.ZeroToZ, 8, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _decToHex = new BaseConverter(10, NumberingSchemes.ZeroToZ, 16, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _hexToBin = new BaseConverter(16, NumberingSchemes.ZeroToZ, 2, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _hexToOct = new BaseConverter(16, NumberingSchemes.ZeroToZ, 8, NumberingSchemes.ZeroToZ);

	private static readonly BaseConverter _hexToDec = new BaseConverter(16, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);

	public static BaseConverter BinToOct => _binToOct;

	public static BaseConverter BinToDec => _binToDec;

	public static BaseConverter BinToHex => _binToHex;

	public static BaseConverter OctToBin => _octToBin;

	public static BaseConverter OctToDec => _octToDec;

	public static BaseConverter OctToHex => _octToHex;

	public static BaseConverter DecToBin => _decToBin;

	public static BaseConverter DecToOct => _decToOct;

	public static BaseConverter DecToHex => _decToHex;

	public static BaseConverter HexToBin => _hexToBin;

	public static BaseConverter HexToOct => _hexToOct;

	public static BaseConverter HexToDec => _hexToDec;

	public int From { get; private set; }

	public int To { get; private set; }

	public static BaseConverter Create(int fromRadix)
	{
		return new BaseConverter(fromRadix, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);
	}

	public static BaseConverter Create(NumberBases fromRadix)
	{
		return new BaseConverter((int)fromRadix, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);
	}

	public static BaseConverter Create(NumberBases fromRadix, NumberBases toRadix)
	{
		return new BaseConverter((int)fromRadix, NumberingSchemes.ZeroToZ, (int)toRadix, NumberingSchemes.ZeroToZ);
	}

	public static BaseConverter Create(int fromRadix, int toRadix)
	{
		return new BaseConverter(fromRadix, NumberingSchemes.ZeroToZ, toRadix, NumberingSchemes.ZeroToZ);
	}

	public static BaseConverter Create(NumberBases fromRadix, NumberingSchemes fromScheme, NumberBases toRadix, NumberingSchemes toScheme)
	{
		return new BaseConverter((int)fromRadix, fromScheme, (int)toRadix, toScheme);
	}

	public static BaseConverter Create(int fromRadix, NumberingSchemes fromScheme, int toRadix, NumberingSchemes toScheme)
	{
		return new BaseConverter(fromRadix, fromScheme, toRadix, toScheme);
	}

	public static BaseConverter Create(int fromRadix, NumberingSchemes fromScheme, NumberBases toRadix, NumberingSchemes toScheme)
	{
		return new BaseConverter(fromRadix, fromScheme, (int)toRadix, toScheme);
	}

	public static BaseConverter Create(NumberBases fromRadix, NumberingSchemes fromScheme, int toRadix, NumberingSchemes toScheme)
	{
		return new BaseConverter((int)fromRadix, fromScheme, toRadix, toScheme);
	}

	public static string Convert(NumberBases fromRadix, string value)
	{
		return Convert((int)fromRadix, 10, value);
	}

	public static string Convert(int fromRadix, string value)
	{
		return Convert(fromRadix, 10, value);
	}

	public static string Convert(NumberBases fromRadix, NumberBases toRadix, string value)
	{
		return Convert((int)fromRadix, NumberingSchemes.ZeroToZ, (int)toRadix, NumberingSchemes.ZeroToZ, value);
	}

	public static string Convert(NumberBases fromRadix, int toRadix, string value)
	{
		return Convert((int)fromRadix, NumberingSchemes.ZeroToZ, toRadix, NumberingSchemes.ZeroToZ, value);
	}

	public static string Convert(int fromRadix, int toRadix, string value)
	{
		return Convert(fromRadix, NumberingSchemes.ZeroToZ, toRadix, NumberingSchemes.ZeroToZ, value);
	}

	public static string Convert(NumberBases fromRadix, NumberingSchemes fromScheme, NumberBases toRadix, NumberingSchemes toScheme, string value)
	{
		return Convert((int)fromRadix, fromScheme, (int)toRadix, toScheme, value);
	}

	public static string Convert(NumberBases fromRadix, NumberingSchemes fromScheme, int toRadix, NumberingSchemes toScheme, string value)
	{
		return Convert((int)fromRadix, fromScheme, toRadix, toScheme, value);
	}

	public static string Convert(int fromRadix, NumberingSchemes fromScheme, NumberBases toRadix, NumberingSchemes toScheme, string value)
	{
		return Convert(fromRadix, fromScheme, (int)toRadix, toScheme, value);
	}

	public static string Convert(int fromRadix, NumberingSchemes fromScheme, int toRadix, NumberingSchemes toScheme, string value)
	{
		return new BaseConverter(fromRadix, fromScheme, toRadix, toScheme).Convert(value);
	}

	public string Convert(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new ArgumentNullException("value");
		}
		long value2 = ConvertToBase10(value, From, _fromNumberingScheme, _maxFromSchemeCharacter);
		if (To != 10)
		{
			return ConvertFromBase10(value2, To, _toNumberingScheme);
		}
		return value2.ToString();
	}

	private BaseConverter(int fromRadix, NumberingSchemes fromScheme, int toRadix, NumberingSchemes toScheme)
	{
		if (fromRadix < 2 || fromRadix > 36)
		{
			throw new ArgumentOutOfRangeException("fromRadix", "Radix can be from 2 to 36 inclusive");
		}
		if (toRadix < 2 || toRadix > 36)
		{
			throw new ArgumentOutOfRangeException("toRadix", "Radix can be from 2 to 36 inclusive");
		}
		if (fromRadix > 26 && fromScheme == NumberingSchemes.AToZ)
		{
			throw new ArgumentOutOfRangeException("fromRadix", "Invalid numbering scheme for specified number base");
		}
		if (toRadix > 26 && fromScheme == NumberingSchemes.AToZ)
		{
			throw new ArgumentOutOfRangeException("toRadix", "Invalid numbering scheme for specified number base");
		}
		From = fromRadix;
		_fromNumberingScheme = GetCharactersForNumberingScheme(fromScheme);
		To = toRadix;
		_toNumberingScheme = GetCharactersForNumberingScheme(toScheme);
		_maxFromSchemeCharacter = ((fromScheme == NumberingSchemes.ZeroToZ) ? fromRadix : (fromRadix + 1));
	}

	private static string GetCharactersForNumberingScheme(NumberingSchemes scheme)
	{
		return scheme switch
		{
			NumberingSchemes.AToZ => "_ABCDEFGHIJKLMNOPQRSTUVWXYZ", 
			NumberingSchemes.ZeroToZ => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", 
			_ => throw new ArgumentOutOfRangeException("scheme"), 
		};
	}

	private static long ConvertToBase10(string value, int fromBase, string characters, int maxFromSchemeCharacter)
	{
		StringBuilder stringBuilder = new StringBuilder(value);
		int num = 0;
		long num2 = 0L;
		while (stringBuilder.Length > 0)
		{
			int num3 = Array.IndexOf(characters.ToCharArray(), stringBuilder[stringBuilder.Length - 1]);
			if (num3 < 0)
			{
				throw new FormatException("Unsupported character in value string");
			}
			if (num3 >= maxFromSchemeCharacter)
			{
				throw new FormatException("Value contains character not valid for number base");
			}
			num2 += num3 * (long)Math.Pow(fromBase, num);
			if (num2 < 0)
			{
				throw new OverflowException();
			}
			stringBuilder.Length--;
			num++;
		}
		return num2;
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
