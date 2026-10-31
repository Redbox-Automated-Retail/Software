using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.Core;

namespace Redbox.Installer.Framework;

public class DefinitionList : List<Definition>
{
	public DefinitionList()
	{
	}

	public DefinitionList(IEnumerable<Definition> collection)
		: base(collection)
	{
	}

	public void Install(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Definition current = enumerator.Current;
			if (current.GetTaskCount() != 0)
			{
				progress?.Invoke(current);
				errors.AddRange(current.Install(module, progress));
			}
		}
	}

	public void Uninstall(Action<Definition> progress, ErrorList errors)
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Definition current = enumerator.Current;
			if (current.GetTaskCount() != 0)
			{
				progress?.Invoke(current);
				errors.AddRange(current.Uninstall(progress));
			}
		}
	}

	public int GetTaskCount()
	{
		int num = 0;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Definition current = enumerator.Current;
			num += current.GetTaskCount();
		}
		return num;
	}

	public Definition GetByID(string id)
	{
		return Find((Definition each) => string.Compare(each.ID, id, ignoreCase: true) == 0);
	}

	public int GetNumberOfFiles()
	{
		int num = 0;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Definition current = enumerator.Current;
			num += current.GetCountValue();
		}
		return num;
	}

	public void DisposeAllPages()
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Definition current = enumerator.Current;
			if (current.GetType() == typeof(PageDefinition))
			{
				((PageDefinition)current).Dispose();
			}
		}
	}

	public int GetSizeOfAllFiles()
	{
		int num = 0;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Definition current = enumerator.Current;
			num += current.Size;
		}
		return num;
	}

	public void LoadFromResources(string path)
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Definition current = enumerator.Current;
			using LibraryModule module = new LibraryModule(path);
			current.LoadFromResource(module);
		}
	}

	public DefinitionList GetFiles()
	{
		return new DefinitionList(FindAll((Definition each) => each.GetType() == typeof(FileEntryDefinition)));
	}

	public DefinitionList GetPages()
	{
		return new DefinitionList(FindAll((Definition each) => each.GetType() == typeof(PageDefinition) && each.EvaluateIfExpression(null)));
	}

	public ErrorList SaveToResources(ResourceUpdateSession session, XmlDocument xmlDocument, string basePath)
	{
		ErrorList errorList = new ErrorList();
		int num = 0;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Definition current = enumerator.Current;
			current.SaveToResource(session, num++, xmlDocument, basePath, errorList);
		}
		return errorList;
	}

	public Definition GetInstallProgressPage()
	{
		return Find((Definition each) => each.GetType() == typeof(PageDefinition) && ((PageDefinition)each).Type == PageType.Progress);
	}

	public DefinitionList GetLoadableModules()
	{
		return new DefinitionList(FindAll((Definition each) => each.GetType() == typeof(FileEntryDefinition) && ((FileEntryDefinition)each).IsLoadableModule()));
	}

	public void SortByInstallExecutionOrder()
	{
		Sort((Definition x, Definition y) => x.InstallSortOrder.CompareTo(y.InstallSortOrder));
	}

	public void SortByUninstallExecutionOrder()
	{
		Sort((Definition x, Definition y) => x.UninstallSortOrder.CompareTo(y.UninstallSortOrder));
	}

	public DefinitionList GetLaunchOnlyProcesses()
	{
		return new DefinitionList(FindAll((Definition each) => each.GetType() == typeof(ExecuteProcessDefinition) && ((ExecuteProcessDefinition)each).LaunchOnly));
	}

	public DefinitionList GetProhibitedProcessesForNotify()
	{
		return new DefinitionList(FindAll((Definition each) => each.GetType() == typeof(ProhibitedProcessDefinition) && !((ProhibitedProcessDefinition)each).Kill));
	}
}
