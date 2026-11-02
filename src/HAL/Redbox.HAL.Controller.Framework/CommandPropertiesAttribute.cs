using System;
using System.Reflection;

namespace Redbox.HAL.Controller.Framework;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class CommandPropertiesAttribute : Attribute
{
	public int StatusBit { get; set; }

	public string? Command { get; set; }

	public int CommandWait { get; set; }

	public string? ResetCommand { get; set; }

	public AddressSelector Address { get; set; }

	public int WaitPauseTime { get; set; }

	public CommandPropertiesAttribute()
	{
		StatusBit = (WaitPauseTime = (CommandWait = -1));
		Address = AddressSelector.None;
	}

	public static CommandPropertiesAttribute? GetProperties(FieldInfo fieldInfo)
	{
		CommandPropertiesAttribute[] attributes = (CommandPropertiesAttribute[])Attribute.GetCustomAttributes(fieldInfo, typeof(CommandPropertiesAttribute));
		if (attributes != null && attributes.Length != 0)
		{
			return attributes[0];
		}
		return null;
	}
}
