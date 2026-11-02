#nullable disable // reference source kept as it is; stays oblivious where it is linked into nullable-enabled projects
// Written for the modernize branch (not decompiled, not from referencesource): the System.Web internals that the
// referencesource copy of System.Web.Script.Serialization uses, for the .NET 10 build of Redbox.Core. See README.md.
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System.Web.Resources
{
	// Message texts as in .NET Framework 4.8's System.Web.Extensions resources (System.Web.Resources.AtlasWeb).
	internal static class AtlasWeb
	{
		internal const string JSON_ArrayTypeNotSupported = "Type '{0}' is not supported for deserialization of an array.";
		internal const string JSON_BadEscape = "Unrecognized escape sequence.";
		internal const string JSON_CannotConvertObjectToType = "Cannot convert object of type '{0}' to type '{1}'";
		internal const string JSON_CannotCreateListType = "Cannot create instance of {0}.";
		internal const string JSON_CannotSerializeMemberGeneric = "Cannot serialize member '{0}' on type '{1}'.";
		internal const string JSON_CircularReference = "A circular reference was detected while serializing an object of type '{0}'.";
		internal const string JSON_DepthLimitExceeded = "RecursionLimit exceeded.";
		internal const string JSON_DeserializerTypeMismatch = "Cannot deserialize object graph into type of '{0}'.";
		internal const string JSON_DictionaryTypeNotSupported = "Type '{0}' is not supported for serialization/deserialization of a dictionary, keys must be strings or objects.";
		internal const string JSON_ExpectedOpenBrace = "Invalid object passed in, '{' expected.";
		internal const string JSON_IllegalPrimitive = "Invalid JSON primitive: {0}.";
		internal const string JSON_InvalidArrayEnd = "Invalid array passed in, ']' expected.";
		internal const string JSON_InvalidArrayExpectComma = "Invalid array passed in, ',' expected.";
		internal const string JSON_InvalidArrayExtraComma = "Invalid array passed in, extra trailing ','.";
		internal const string JSON_InvalidArrayStart = "Invalid array passed in, '[' expected.";
		internal const string JSON_InvalidEnumType = "Enums based on System.Int64 or System.UInt64 are not JSON-serializable because JavaScript does not support the necessary precision.";
		internal const string JSON_InvalidMaxJsonLength = "Value must be a positive integer.";
		internal const string JSON_InvalidMemberName = "Invalid object passed in, member name expected.";
		internal const string JSON_InvalidObject = "Invalid object passed in, ':' or '}' expected.";
		internal const string JSON_InvalidRecursionLimit = "RecursionLimit must be a positive integer.";
		internal const string JSON_MaxJsonLengthExceeded = "Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property.";
		internal const string JSON_NoConstructor = "No parameterless constructor defined for type of '{0}'.";
		internal const string JSON_StringNotQuoted = "Invalid string passed in, '\\\"' expected.";
		internal const string JSON_UnterminatedString = "Unterminated string passed in.";
		internal const string JSON_ValueTypeCannotBeNull = "Cannot convert null to a value type.";
	}
}

namespace System.Web
{
	// In System.Web these asserted reflection permission for partial trust; without code access security they are
	// plain reflection calls.
	internal static class SecurityUtils
	{
		internal static object FieldInfoGetValue(System.Reflection.FieldInfo field, object target)
		{
			return field.GetValue(target);
		}

		internal static object MethodInfoInvoke(System.Reflection.MethodInfo method, object target, object[] args)
		{
			return method.Invoke(target, args);
		}
	}

	internal static class SR
	{
		internal const string CollectionCountExceeded_JavaScriptObjectDeserializer = "The maximum number of items has already been deserialized into a single dictionary by the JavaScriptSerializer. To change the maximum allowed JSON dictionary entry count from its current value of {0}, change the \"aspnet:MaxJsonDeserializerMembers\" setting. See http://go.microsoft.com/fwlink/?LinkId=238386 for more information.";

		internal static string GetString(string format, params object[] args)
		{
			return string.Format(CultureInfo.CurrentCulture, format, args);
		}
	}
}

namespace System.Web.Util
{
	internal static class AppSettings
	{
		private static int? _maxJsonDeserializerMembers;

		// System.Web reads <appSettings><add key="aspnet:MaxJsonDeserializerMembers" .../> once; outside ASP.NET the
		// default is Int32.MaxValue (System.Web/Util/AppSettings.cs).
		internal static int MaxJsonDeserializerMembers
		{
			get
			{
				if (_maxJsonDeserializerMembers == null)
				{
					int value = int.MaxValue;
					try
					{
						if (int.TryParse(ConfigurationManager.AppSettings["aspnet:MaxJsonDeserializerMembers"], NumberStyles.Integer, CultureInfo.InvariantCulture, out int configured) && configured >= 0)
						{
							value = configured;
						}
					}
					catch (ConfigurationErrorsException)
					{
					}
					_maxJsonDeserializerMembers = value;
				}
				return _maxJsonDeserializerMembers.Value;
			}
		}
	}

	// .NET Framework's "R" format: the shortest of G15/G17 (G7/G9 for float) that round-trips, upper-case exponent,
	// and negative zero as "0". .NET Core 3.0+ changed "r" (lower-case exponent, "-0"), which would change the JSON.
	internal static class NetFrameworkRoundTrip
	{
		internal static string Format(double value)
		{
			if (value == 0.0)
			{
				return "0";
			}
			if (double.IsNaN(value) || double.IsInfinity(value))
			{
				return value.ToString(CultureInfo.InvariantCulture);
			}
			string s = value.ToString("G15", CultureInfo.InvariantCulture);
			return double.Parse(s, CultureInfo.InvariantCulture) == value ? s : value.ToString("G17", CultureInfo.InvariantCulture);
		}

		internal static string Format(float value)
		{
			if (value == 0f)
			{
				return "0";
			}
			if (float.IsNaN(value) || float.IsInfinity(value))
			{
				return value.ToString(CultureInfo.InvariantCulture);
			}
			string s = value.ToString("G7", CultureInfo.InvariantCulture);
			return float.Parse(s, CultureInfo.InvariantCulture) == value ? s : value.ToString("G9", CultureInfo.InvariantCulture);
		}
	}

	internal static class Debug
	{
		[Conditional("DEBUG")]
		internal static void Assert(bool condition)
		{
			System.Diagnostics.Debug.Assert(condition);
		}
	}

	// .NET 4.5+ behaviour (aspnet:AllowRelaxedUnicodeDecoding not set): unpaired surrogates become U+FFFD.
	internal static class Utf16StringValidator
	{
		internal static string ValidateString(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}
			StringBuilder builder = null;
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];
				bool valid;
				if (char.IsHighSurrogate(c))
				{
					valid = i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]);
					if (valid)
					{
						builder?.Append(c).Append(input[i + 1]);
						i++;
						continue;
					}
				}
				else
				{
					valid = !char.IsLowSurrogate(c);
				}
				if (!valid && builder == null)
				{
					builder = new StringBuilder(input.Length);
					builder.Append(input, 0, i);
				}
				builder?.Append(valid ? c : '�');
			}
			return builder?.ToString() ?? input;
		}
	}
}
