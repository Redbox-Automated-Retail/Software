using System;
using System.Reflection;

namespace Redbox.Core;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class CardCodeAttribute : Attribute
{
	public string Code { get; set; }

	public static CardCodeAttribute GetCode(CardType cardType)
	{
		FieldInfo field = typeof(CardType).GetField(cardType.ToString());
		if (field == null)
		{
			return null;
		}
		return Attribute.GetCustomAttribute(field, typeof(CardCodeAttribute)) as CardCodeAttribute;
	}
}
