using System.Reflection;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

[Command("statistics")]
public class StatisticsCommand
{
	[CommandForm(Name = "show")]
	[Usage("STATISTICS show")]
	public void Show(CommandContext context)
	{
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public;
		FieldInfo[] fields = typeof(Statistics).GetFields(bindingAttr);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			string item = $"{fieldInfo.Name} = {fieldInfo.GetValue(Statistics.Instance)}";
			context.Messages.Add(item);
		}
		PropertyInfo[] properties = typeof(Statistics).GetProperties(bindingAttr);
		PropertyInfo[] array2 = properties;
		foreach (PropertyInfo propertyInfo in array2)
		{
			string item2 = $"{propertyInfo.Name} = {propertyInfo.GetValue(Statistics.Instance, null)}";
			context.Messages.Add(item2);
		}
	}
}
