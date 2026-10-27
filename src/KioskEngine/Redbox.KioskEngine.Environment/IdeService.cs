using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.Lua;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.Environment;

public class IdeService : IIdeService
{
	private IdeMainForm m_ideMainForm;

	public static IdeService Instance => Singleton<IdeService>.Instance;

	public Form MainForm => m_ideMainForm;

	public IProject CurrentProject { get; private set; }

	public ReadOnlyCollection<string> RecentProjects
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			if (service == null)
			{
				return new List<string>().AsReadOnly();
			}
			return new List<string>(service.GetValue("Ide", "RecentProjects", new string[0])).AsReadOnly();
		}
	}

	public LuaDebugger Debugger { get; set; }

	public Redbox.KioskEngine.ComponentModel.ErrorList OpenProject(string path)
	{
		return new Redbox.KioskEngine.ComponentModel.ErrorList();
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList CloseProject()
	{
		return new Redbox.KioskEngine.ComponentModel.ErrorList();
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Build()
	{
		return Build(null);
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Build(string path)
	{
		return new Redbox.KioskEngine.ComponentModel.ErrorList();
	}

	public void Show()
	{
		if (m_ideMainForm != null && m_ideMainForm.IsDisposed)
		{
			m_ideMainForm = null;
		}
		if (m_ideMainForm == null)
		{
			m_ideMainForm = new IdeMainForm();
			IdeMainForm.m_instance = m_ideMainForm;
			m_ideMainForm.Show();
		}
		else
		{
			m_ideMainForm.BringToFront();
		}
	}

	public void Close()
	{
		if (m_ideMainForm != null)
		{
			if (m_ideMainForm.Visible)
			{
				m_ideMainForm.Close();
			}
			m_ideMainForm = null;
		}
	}

	public void ActivateDebugger(string resourceName, int lineNumber, string error)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		IResource resource = service.ActiveBundleSet.GetResource(resourceName, service.Filter);
		Show();
		((IdeMainForm)MainForm).OpenResourceEditor(resource, lineNumber, error);
	}

	public void SetDebuggerInstance(object instance)
	{
		Debugger = instance as LuaDebugger;
	}

	private IdeService()
	{
	}
}
