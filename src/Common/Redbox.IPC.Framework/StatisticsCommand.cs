using System.Reflection;
using Redbox.Core;

namespace Redbox.IPC.Framework;

[Command("statistics", Filter = "System|Statistics")]
public class StatisticsCommand
{
	[CommandForm(Name = "show")]
	[Usage("STATISTICS show")]
	public void Show(CommandContext context)
	{
		FieldInfo[] fields = typeof(Statistics).GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			string item = $"{fieldInfo.Name} = {fieldInfo.GetValue(Statistics.Instance)}";
			context.Messages.Add(item);
		}
		PropertyInfo[] properties = typeof(Statistics).GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			string item2 = $"{propertyInfo.Name} = {propertyInfo.GetValue(Statistics.Instance, null)}";
			context.Messages.Add(item2);
		}
	}
}
