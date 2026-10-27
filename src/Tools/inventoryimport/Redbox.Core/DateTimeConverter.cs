using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Redbox.Core;

internal class DateTimeConverter : JavaScriptConverter
{
	private const string DateKey = "theDate";

	private const string KindKey = "dateTimeKind";

	public override IEnumerable<Type> SupportedTypes
	{
		get
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(DateTime));
			list.Add(typeof(DateTime?));
			return list;
		}
	}

	public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (obj.GetType() == typeof(DateTime))
		{
			dictionary["theDate"] = ((DateTime)obj).Ticks;
			dictionary["dateTimeKind"] = ((DateTime)obj).Kind.ToString("G");
			return dictionary;
		}
		if (obj.GetType() == typeof(DateTime?))
		{
			DateTime? dateTime = (DateTime?)obj;
			if (dateTime.HasValue)
			{
				dictionary["theDate"] = dateTime.Value.Ticks;
				dictionary["dateTimeKind"] = dateTime.Value.Kind.ToString("G");
			}
		}
		return dictionary;
	}

	public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
	{
		if (dictionary.ContainsKey("theDate") && dictionary.ContainsKey("dateTimeKind"))
		{
			return new DateTime(long.Parse(dictionary["theDate"].ToString()), (DateTimeKind)Enum.Parse(typeof(DateTimeKind), (string)dictionary["dateTimeKind"]));
		}
		if (dictionary.ContainsKey("theDate") && !dictionary.ContainsKey("dateTimeKind"))
		{
			return new DateTime(long.Parse(dictionary["theDate"].ToString()), DateTimeKind.Unspecified);
		}
		return null;
	}
}
