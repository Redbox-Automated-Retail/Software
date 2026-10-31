using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Redbox.Core;

namespace Redbox.GetOpts;

public class OptionValue
{
	private readonly object m_container;

	private readonly MemberAccessor m_member;

	private readonly OptionAttribute m_definition;

	private readonly DescriptionAttribute m_description;

	private readonly DefaultValueAttribute m_defaultValue;

	public bool Required => m_definition.Required;

	public static OptionValueList GetOptionValues(object container)
	{
		OptionValueList optionValueList = new OptionValueList();
		MemberInfo[] members = container.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty);
		MemberInfo[] array = members;
		foreach (MemberInfo memberInfo in array)
		{
			OptionAttribute optionAttribute = (OptionAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(OptionAttribute));
			if (optionAttribute != null)
			{
				DescriptionAttribute description = (DescriptionAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(DescriptionAttribute));
				DefaultValueAttribute defaultValue = (DefaultValueAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(DefaultValueAttribute));
				optionValueList.AddLast(new OptionValue(container, new MemberAccessor(memberInfo), optionAttribute, description, defaultValue));
			}
		}
		return optionValueList;
	}

	public void SetValue(object key, object value)
	{
		if (IsDictionary())
		{
			PropertyInfo property = m_member.Type.GetProperty("Item");
			Type convertToType = m_member.Type.GetGenericArguments()[0];
			Type convertToType2 = m_member.Type.GetGenericArguments()[1];
			property.SetValue(m_member.GetValue(m_container), ConversionHelper.ChangeType(value, convertToType2), new object[1] { ConversionHelper.ChangeType(key, convertToType) });
		}
		else if (IsList())
		{
			Type convertToType2 = m_member.Type.GetGenericArguments()[0];
			MethodInfo method = m_member.Type.GetMethod("Insert");
			method.Invoke(m_member.GetValue(m_container), new object[2]
			{
				0,
				ConversionHelper.ChangeType(value, convertToType2)
			});
		}
		else
		{
			object value2 = ConversionHelper.ChangeType(value, m_member.Type);
			EnsureConstraints(value2);
			m_member.SetValue(m_container, value2);
		}
	}

	public object GetValue()
	{
		return m_member.GetValue(m_container);
	}

	public override string ToString()
	{
		int num = 80;
		try
		{
			num = Console.WindowWidth;
		}
		catch
		{
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = string.Empty;
		if (m_definition.LongName != null)
		{
			text = $"--{m_definition.LongName}";
		}
		if (m_definition.ShortName != null)
		{
			if (m_definition.LongName != null)
			{
				text += ", ";
			}
			text += $"-{m_definition.ShortName}";
		}
		if (m_definition.LongName == null && m_definition.ShortName == null)
		{
			text += $"--{m_member.Name.ToLowerInvariant()}";
		}
		if (IsFlag())
		{
			text += "[+|-]";
		}
		else if (IsDictionary())
		{
			text += ":key=value";
		}
		stringBuilder.AppendFormat("  {0,-15}", text);
		int num2 = 15;
		if (m_definition.Required)
		{
			stringBuilder.Append("Required. ");
			num2 += 10;
		}
		if (m_defaultValue != null)
		{
			string text2 = $"Default value is '{m_defaultValue.Value}'. ";
			num2 += text2.Length;
			stringBuilder.Append(text2);
		}
		if (m_description != null)
		{
			string[] array = m_description.Description.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				stringBuilder.AppendFormat("{0} ", text3);
				num2 += text3.Length + 1;
				if (num2 > num - 15)
				{
					stringBuilder.Append("\n                 ");
					num2 = 15;
				}
			}
		}
		if (m_definition.Min != null && m_definition.Max != null)
		{
			stringBuilder.AppendFormat("\n                  Valid values range from {0} to {1} inclusive.", m_definition.Min, m_definition.Max);
		}
		return stringBuilder.ToString();
	}

	public string GetName()
	{
		return m_definition.LongName ?? m_definition.ShortName ?? m_member.Name;
	}

	public bool IsValidName(string name)
	{
		return string.Compare(name, m_definition.LongName, ignoreCase: true) == 0 || string.Compare(name, m_definition.ShortName, ignoreCase: true) == 0 || string.Compare(name, m_member.Name, ignoreCase: true) == 0;
	}

	internal OptionValue(object container, MemberAccessor member, OptionAttribute definition, DescriptionAttribute description, DefaultValueAttribute defaultValue)
	{
		m_member = member;
		m_container = container;
		m_definition = definition;
		m_description = description;
		m_defaultValue = defaultValue;
		if (m_defaultValue != null)
		{
			SetValue(null, m_defaultValue.Value);
		}
	}

	internal void EnsureConstraints(object value)
	{
		if (m_definition.Min == null || m_definition.Max == null || !(value is IComparable comparable) || (comparable.CompareTo(m_definition.Min) >= 0 && comparable.CompareTo(m_definition.Max) <= 0))
		{
			return;
		}
		throw new ArgumentOutOfRangeException($"{GetName()} must be a value from {m_definition.Min} to {m_definition.Max}.");
	}

	internal bool IsFlag()
	{
		return m_member.Type == typeof(bool);
	}

	internal bool IsList()
	{
		return m_member.Type.IsGenericType && m_member.Type.GetGenericTypeDefinition() == typeof(IList<>);
	}

	internal bool IsDictionary()
	{
		return m_member.Type.IsGenericType && m_member.Type.GetGenericTypeDefinition() == typeof(IDictionary<, >);
	}
}
