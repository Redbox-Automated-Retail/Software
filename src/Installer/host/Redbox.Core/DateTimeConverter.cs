using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Redbox.Core;

internal class DateTimeConverter : JavaScriptConverter
{
	private const string DateKey = "theDate";

	private const string KindKey = "dateTimeKind";

	private const string DateString = "dateString";

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
		DateTime? dateTime = ((obj is DateTime) ? new DateTime?((DateTime)obj) : ((DateTime?)obj));
		if (dateTime.HasValue)
		{
			dictionary["theDate"] = dateTime.Value.Ticks;
			dictionary["dateTimeKind"] = dateTime.Value.Kind.ToString("G");
			if (dateTime.Value.Kind == DateTimeKind.Local)
			{
				dateTime = new DateTime(dateTime.Value.Ticks, DateTimeKind.Unspecified);
			}
			dictionary["dateString"] = dateTime.Value.ToString("o");
		}
		return dictionary;
	}

	public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
	{
		if (dictionary.ContainsKey("dateString"))
		{
			return DateTime.Parse(dictionary["dateString"].ToString(), null, DateTimeStyles.RoundtripKind);
		}
		DateTimeKind kind = DateTimeKind.Unspecified;
		if (dictionary.ContainsKey("dateTimeKind"))
		{
			kind = (DateTimeKind)Enum.Parse(typeof(DateTimeKind), dictionary["dateTimeKind"].ToString());
		}
		if (dictionary.ContainsKey("theDate"))
		{
			return new DateTime(long.Parse(dictionary["theDate"].ToString()), kind);
		}
		return null;
	}
}
