using System;
using System.Collections.Generic;

namespace Redbox.HAL.IPC.Framework;

internal class CommandInstance
{
	public readonly IDictionary<string, FormMethod> FormMethodCache = new Dictionary<string, FormMethod>();

	public Type CommandType { get; set; }

	public string CommandDescription { get; set; }

	public object GetInstance()
	{
		return Activator.CreateInstance(CommandType);
	}

	public bool HasDefault()
	{
		return FormMethodCache.ContainsKey("Default");
	}

	public bool HasOnlyDefault()
	{
		if (FormMethodCache.Count == 1)
		{
			return HasDefault();
		}
		return false;
	}

	public void InvokeDefault(CommandResult result, CommandContext context, CommandTokenizer tokenizer)
	{
		if (HasDefault())
		{
			FormMethodCache["Default"].Invoke(result, context, tokenizer, GetInstance());
		}
	}

	public FormMethod GetMethod(string formName)
	{
		if (string.IsNullOrEmpty(formName))
		{
			return null;
		}
		string key = formName.ToUpper();
		if (FormMethodCache.ContainsKey(key))
		{
			return FormMethodCache[key];
		}
		return null;
	}
}
