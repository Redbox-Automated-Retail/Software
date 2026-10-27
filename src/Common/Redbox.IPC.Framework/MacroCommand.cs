using Redbox.Core;

namespace Redbox.IPC.Framework;

[Command("macro", Filter = "System|Macro")]
public class MacroCommand
{
	[Usage("MACRO list")]
	[CommandForm(Name = "list")]
	public void List(CommandContext context)
	{
		foreach (string key in CommandService.Instance.Properties.Keys)
		{
			string item = $"{key} expands to '{CommandService.Instance.Properties[key]}'.";
			context.Messages.Add(item);
		}
	}

	[CommandForm(Name = "create")]
	[Usage("MACRO create symbol value: 'expansion'")]
	public void Create(CommandContext context, [CommandKeyValue(IsRequired = true)] string value)
	{
		context.ForEachSymbolDo(delegate(string each)
		{
			CommandService.Instance.Properties[each] = value;
		}, new string[3] { "list", "create", "delete" });
	}

	[CommandForm(Name = "delete")]
	[Usage("MACRO delete symbol ...")]
	public void Delete(CommandContext context)
	{
		context.ForEachSymbolDo(delegate(string each)
		{
			CommandService.Instance.Properties.Remove(each);
		}, new string[3] { "list", "create", "delete" });
	}
}
