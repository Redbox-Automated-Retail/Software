using System.Reflection;
using Redbox.HAL.Component.Model.Attributes;
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
		foreach (FieldInfo fieldInfo in fields)
		{
			string fieldLine = $"{fieldInfo.Name} = {fieldInfo.GetValue(Statistics.Instance)}";
			context.Messages.Add(fieldLine);
		}
		PropertyInfo[] properties = typeof(Statistics).GetProperties(bindingAttr);
		foreach (PropertyInfo propertyInfo in properties)
		{
			string propertyLine = $"{propertyInfo.Name} = {propertyInfo.GetValue(Statistics.Instance, null)}";
			context.Messages.Add(propertyLine);
		}
	}
}
