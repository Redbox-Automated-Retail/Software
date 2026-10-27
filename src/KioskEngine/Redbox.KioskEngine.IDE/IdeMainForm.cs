using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using Redbox.Lua;
using Redbox.REDS.Framework;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class IdeMainForm : Form
{
	private bool m_isActivated;

	public static IdeMainForm m_instance;

	private readonly OutputToolWindow m_outputToolWindow = new OutputToolWindow();

	private readonly TimersToolWindow m_timersToolWindow = new TimersToolWindow();

	private readonly SymbolsToolWindow m_localsToolWindow = new SymbolsToolWindow();

	private readonly SymbolsToolWindow m_globalsToolWindow = new SymbolsToolWindow();

	private readonly ErrorListToolWindow m_errorListToolWindow = new ErrorListToolWindow();

	private readonly ImmediateToolWindow m_immediateToolWindow = new ImmediateToolWindow();

	private readonly CallStackToolWindow m_callStackToolWindow = new CallStackToolWindow();

	private readonly ViewStackToolWindow m_viewStackToolWindow = new ViewStackToolWindow();

	private readonly PropertiesToolWindow m_propertiesToolWindow = new PropertiesToolWindow();

	private readonly BundleFilterToolWindow m_bundleFilterToolWindow = new BundleFilterToolWindow();

	private readonly ProjectExplorerToolWindow m_projectExplorerToolWindow = new ProjectExplorerToolWindow();

	private IContainer components;

	private MenuStrip m_menuStrip;

	private StatusStrip m_statusStrip;

	private DockPanel m_dockPanel;

	private ToolStrip m_standardToolStrip;

	private ToolStripButton m_openProjectToolStripButton;

	private ToolStripMenuItem m_viewToolStripMenuItem;

	private ToolStripMenuItem m_viewProjectExplorerToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem1;

	private ToolStripMenuItem m_timersToolStripMenuItem;

	private ToolStripMenuItem m_viewStackToolStripMenuItem;

	private ToolStripMenuItem m_outputToolStripMenuItem;

	private ToolStripMenuItem m_propertiesToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem2;

	private ToolStripMenuItem m_legacyDebuggerToolStripMenuItem;

	private ToolStripMenuItem m_fileToolStripMenuItem;

	private ToolStripMenuItem m_editToolStripMenuItem;

	private ToolStripMenuItem m_toolsToolStripMenuItem;

	private ToolStripMenuItem m_helpToolStripMenuItem;

	private ToolStripMenuItem m_globalsToolStripMenuItem;

	private ToolStripMenuItem m_localsToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem3;

	private ToolStripMenuItem m_callstackToolStripMenuItem;

	private ToolStripMenuItem m_immediateWindowToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem4;

	private ToolStripMenuItem m_bundleFiltersToolStripMenuItem;

	private ToolStripMenuItem m_windowToolStripMenuItem;

	private ToolStripMenuItem m_buildToolStripMenuItem;

	private ToolStripStatusLabel m_toolStripStatusLabel;

	private ToolStripProgressBar m_toolStripProgressBar;

	private ToolStripStatusLabel m_editorToolStripStatusLabel;

	private ToolStripMenuItem m_errorListToolStripMenuItem;

	private ToolStripMenuItem m_newToolStripMenuItem;

	private ToolStripMenuItem m_newProjectToolStripMenuItem;

	private ToolStripMenuItem m_newResourceToolStripMenuItem;

	private ToolStripMenuItem m_openToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem5;

	private ToolStripMenuItem m_closeToolStripMenuItem;

	private ToolStripMenuItem m_closeProjectToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem6;

	private ToolStripMenuItem m_saveToolStripMenuItem;

	private ToolStripMenuItem m_saveAsToolStripMenuItem;

	private ToolStripMenuItem m_saveAllToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem7;

	private ToolStripMenuItem m_pageSetupToolStripMenuItem;

	private ToolStripMenuItem m_printToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem8;

	private ToolStripMenuItem m_recentResourcesToolStripMenuItem;

	private ToolStripMenuItem m_recentProjectsToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem9;

	private ToolStripMenuItem m_exitToolStripMenuItem;

	private ToolStripMenuItem m_undoToolStripMenuItem;

	private ToolStripMenuItem m_redoToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem10;

	private ToolStripMenuItem m_cutToolStripMenuItem;

	private ToolStripMenuItem m_copyToolStripMenuItem;

	private ToolStripMenuItem m_pasteToolStripMenuItem;

	private ToolStripMenuItem m_deleteToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem11;

	private ToolStripMenuItem m_selectAllToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem12;

	private ToolStripMenuItem m_findAndReplaceToolStripMenuItem;

	private ToolStripMenuItem m_findToolStripMenuItem;

	private ToolStripMenuItem m_replaceToolStripMenuItem;

	private ToolStripMenuItem goToToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem13;

	private ToolStripMenuItem m_bookmarksToolStripMenuItem;

	private ToolStripMenuItem m_buildBundleToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem14;

	private ToolStripMenuItem m_enableDebuggingToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripMenuItem m_resumeExecutionToolStripMenuItem;

	private ToolStripMenuItem m_breakAllToolStripMenuItem;

	private ToolStripMenuItem m_stopDebugginToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem m_stepIntoToolStripMenuItem;

	private ToolStripMenuItem m_stepOverToolStripMenuItem;

	private ToolStripMenuItem m_stepOutToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripMenuItem m_toggleBreakpointToolStripMenuItem;

	private ToolStripMenuItem m_deleteAllBreakpointToolStripMenuItem;

	private ToolStripMenuItem m_breakpointsToolStripMenuItem;

	private ToolStripMenuItem m_optionsToolStripMenuItem;

	private ToolStrip m_debuggerToolStrip;

	private ToolStripButton m_enableDebuggingToolStripButton;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripButton m_resumeExecutionToolStripButton;

	private ToolStripButton m_breakAllToolStripButton;

	private ToolStripButton m_stopDebuggingToolStripButton;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripButton m_stepIntoToolStripButton;

	private ToolStripButton m_stepOverToolStripButton;

	private ToolStripButton m_stepOutToolStripButton;

	private ToolStripContainer m_toolStripContainer;

	private ToolStripSeparator toolStripMenuItem15;

	private ToolStripMenuItem m_showWhitespaceToolStripMenuItem;

	private ToolStripMenuItem m_showEOLMarkerToolStripMenuItem;

	private ToolStripButton m_saveToolStripButton;

	private ToolStripButton m_saveAllToolStripButton;

	private ToolStripSeparator toolStripSeparator6;

	private ToolStripButton m_cutToolStripButton;

	private ToolStripButton m_copyToolStripButton;

	private ToolStripButton m_pasteToolStripButton;

	private ToolStripSeparator toolStripSeparator7;

	private ToolStripButton m_undoToolStripButton;

	private ToolStripButton m_redoToolStripButton;

	private ToolStripDropDownButton m_newToolStripDropDownButton;

	private ToolStripMenuItem m_newProject2ToolStripMenuItem;

	private ToolStripMenuItem m_newResource2ToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator8;

	private ToolStripSeparator toolStripSeparator9;

	private ToolStripTextBox m_findToolStripTextBox;

	private ToolStripMenuItem m_addToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem16;

	private ToolStripMenuItem m_addNewBundleToolStripMenuItem;

	private ToolStripMenuItem m_addExistingBundleToolStripMenuItem;

	internal PropertiesToolWindow PropertiesWindow => m_propertiesToolWindow;

	public IdeMainForm()
	{
		InitializeComponent();
		m_localsToolWindow.Text = "Locals";
		m_localsToolWindow.RefreshRequested += delegate
		{
			IDebugService service = ServiceLocator.Instance.GetService<IDebugService>();
			if (service.DebuggerInstance != null)
			{
				m_localsToolWindow.ShowSymbols("Locals", ((LuaDebugger)service.DebuggerInstance).GetLocalVars());
			}
		};
		m_globalsToolWindow.Text = "Globals";
		m_globalsToolWindow.RefreshRequested += delegate
		{
			IDebugService service = ServiceLocator.Instance.GetService<IDebugService>();
			if (service.DebuggerInstance != null)
			{
				m_globalsToolWindow.ShowSymbols("_G", ((LuaDebugger)service.DebuggerInstance).GetGlobalVars());
			}
		};
		LogHelper.Instance.Logged += LogToOutputWindow;
		FormAspects.ActsAsPersistent(this, ServiceLocator.Instance.GetService<IUserSettingsStore>());
		base.Load += delegate
		{
			ToolStripManager.LoadSettings(this);
			byte[] value = ServiceLocator.Instance.GetService<IUserSettingsStore>().GetValue<byte[]>("Shell", "DockLayout");
			if (value == null)
			{
				return;
			}
			using MemoryStream stream = new MemoryStream(value);
			m_dockPanel.LoadFromXml(stream, delegate(string s)
			{
				if (s == typeof(TimersToolWindow).ToString())
				{
					return m_timersToolWindow;
				}
				if (s.StartsWith(typeof(SymbolsToolWindow).ToString()))
				{
					string[] array = s.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					if (array.Length == 2)
					{
						SymbolsToolWindow obj = ((array[1] == "Locals") ? m_localsToolWindow : m_globalsToolWindow);
						obj.RefreshSymbols();
						return obj;
					}
				}
				if (s == typeof(ImmediateToolWindow).ToString())
				{
					return m_immediateToolWindow;
				}
				if (s == typeof(CallStackToolWindow).ToString())
				{
					return m_callStackToolWindow;
				}
				if (s == typeof(ViewStackToolWindow).ToString())
				{
					return m_viewStackToolWindow;
				}
				if (s == typeof(PropertiesToolWindow).ToString())
				{
					return m_propertiesToolWindow;
				}
				if (s == typeof(BundleFilterToolWindow).ToString())
				{
					m_bundleFilterToolWindow.RefreshFilters();
					return m_bundleFilterToolWindow;
				}
				if (s == typeof(ProjectExplorerToolWindow).ToString())
				{
					m_projectExplorerToolWindow.RefreshTree();
					return m_projectExplorerToolWindow;
				}
				if (s == typeof(ErrorListToolWindow).ToString())
				{
					return m_errorListToolWindow;
				}
				return (s == typeof(OutputToolWindow).ToString()) ? m_outputToolWindow : null;
			});
		};
		base.FormClosing += delegate
		{
			m_isActivated = false;
			ToolStripManager.SaveSettings(this);
			IUserSettingsStore service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
			string tempFileName = Path.GetTempFileName();
			m_dockPanel.SaveAsXml(tempFileName, Encoding.ASCII);
			service.SetValue("Shell", "DockLayout", File.ReadAllBytes(tempFileName));
			File.Delete(tempFileName);
			LogHelper.Instance.Logged -= LogToOutputWindow;
			m_instance = null;
		};
	}

	public void OpenResourceEditor(IResource resource)
	{
		ResourceEditorWindow resourceEditorWindow = new ResourceEditorWindow();
		resourceEditorWindow.Resource = resource;
		resourceEditorWindow.Show(m_dockPanel);
	}

	public void OpenResourceEditor(IResource resource, int lineNumber)
	{
		ResourceEditorWindow resourceEditorWindow = new ResourceEditorWindow();
		resourceEditorWindow.Resource = resource;
		resourceEditorWindow.SetDebugState(lineNumber, null);
		resourceEditorWindow.Show(m_dockPanel);
	}

	public void OpenResourceEditor(IResource resource, int lineNumber, string error)
	{
		ResourceEditorWindow resourceEditorWindow = new ResourceEditorWindow();
		resourceEditorWindow.Resource = resource;
		resourceEditorWindow.SetDebugState(lineNumber, error);
		resourceEditorWindow.Show(m_dockPanel);
	}

	public static void LogToOutputWindow(string message)
	{
		if (m_instance != null && m_instance.m_outputToolWindow != null && m_instance.m_outputToolWindow.Visible)
		{
			m_instance.m_outputToolWindow.UpdateText(message);
		}
	}

	private void OnActivated(object sender, EventArgs e)
	{
		if (!m_isActivated)
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			if (service != null)
			{
				base.ShowInTaskbar = service.GetValue("Ide", "ShowIDEInTaskBar", defaultValue: true);
			}
			m_isActivated = true;
		}
	}

	private void OnNewProject(object sender, EventArgs e)
	{
		new NewProjectForm().ShowDialog();
		_ = 1;
	}

	private void OnNewResource(object sender, EventArgs e)
	{
		new NewResourceForm().ShowDialog();
		_ = 1;
	}

	private void OnAddNewBundle(object sender, EventArgs e)
	{
	}

	private void OnAddExistingBundle(object sender, EventArgs e)
	{
	}

	private void OnExit(object sender, EventArgs e)
	{
		Application.Exit();
	}

	private void OnShowTimers(object sender, EventArgs e)
	{
		m_timersToolWindow.Show(m_dockPanel);
	}

	private void OnShowLocals(object sender, EventArgs e)
	{
		m_localsToolWindow.RefreshSymbols();
		m_localsToolWindow.Show(m_dockPanel);
	}

	private void OnShowGlobals(object sender, EventArgs e)
	{
		m_globalsToolWindow.RefreshSymbols();
		m_globalsToolWindow.Show(m_dockPanel);
	}

	private void OnShowViewStack(object sender, EventArgs e)
	{
		m_viewStackToolWindow.Show(m_dockPanel);
	}

	private void OnShowCallStack(object sender, EventArgs e)
	{
		m_callStackToolWindow.RefreshCallStack();
		m_callStackToolWindow.Show(m_dockPanel);
	}

	private void OnShowProperties(object sender, EventArgs e)
	{
		m_propertiesToolWindow.Show(m_dockPanel);
	}

	private void OnShowLegacyDebugger(object sender, EventArgs e)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		DebuggerForm.Instance.ActiveBundleSet = service.ActiveBundleSet;
		DebuggerForm.Instance.Filter = service.Filter;
		DebuggerForm.Instance.ActivateDebugger();
	}

	private void OnShowImmediateWindow(object sender, EventArgs e)
	{
		m_immediateToolWindow.Show(m_dockPanel);
	}

	private void OnShowBundleFilters(object sender, EventArgs e)
	{
		m_bundleFilterToolWindow.RefreshFilters();
		m_bundleFilterToolWindow.Show(m_dockPanel);
	}

	private void OnShowBundleExplorer(object sender, EventArgs e)
	{
		m_projectExplorerToolWindow.RefreshTree();
		m_projectExplorerToolWindow.Show(m_dockPanel);
	}

	private void OnShowErrorList(object sender, EventArgs e)
	{
		m_errorListToolWindow.Show(m_dockPanel);
	}

	private void OnShowOutput(object sender, EventArgs e)
	{
		m_outputToolWindow.Show(m_dockPanel);
	}

	private void OnShowOptions(object sender, EventArgs e)
	{
		new PreferencesManager().ShowDialog();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		WeifenLuo.WinFormsUI.Docking.DockPanelSkin dockPanelSkin = new WeifenLuo.WinFormsUI.Docking.DockPanelSkin();
		WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin autoHideStripSkin = new WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin();
		WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
		WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient = new WeifenLuo.WinFormsUI.Docking.TabGradient();
		WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin dockPaneStripSkin = new WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin();
		WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient dockPaneStripGradient = new WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient();
		WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient2 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
		WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient2 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
		WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient3 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
		WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient dockPaneStripToolWindowGradient = new WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient();
		WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient4 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
		WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient5 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
		WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient3 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
		WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient6 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
		WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient7 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.IdeMainForm));
		this.m_menuStrip = new System.Windows.Forms.MenuStrip();
		this.m_fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_newProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_newResourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
		this.m_addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_addNewBundleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_addExistingBundleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem16 = new System.Windows.Forms.ToolStripSeparator();
		this.m_closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_closeProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
		this.m_saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_saveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
		this.m_pageSetupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
		this.m_recentResourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_recentProjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
		this.m_exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
		this.m_cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripSeparator();
		this.m_selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripSeparator();
		this.m_findAndReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.goToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripSeparator();
		this.m_bookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_viewProjectExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
		this.m_timersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_viewStackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_bundleFiltersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
		this.m_globalsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_localsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_callstackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_immediateWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
		this.m_errorListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_outputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
		this.m_legacyDebuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripSeparator();
		this.m_showWhitespaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_showEOLMarkerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_buildBundleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
		this.m_enableDebuggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.m_resumeExecutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_breakAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_stopDebugginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.m_stepIntoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_stepOverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_stepOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.m_toggleBreakpointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_deleteAllBreakpointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_breakpointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_statusStrip = new System.Windows.Forms.StatusStrip();
		this.m_toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
		this.m_toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
		this.m_editorToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
		this.m_dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
		this.m_standardToolStrip = new System.Windows.Forms.ToolStrip();
		this.m_newToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
		this.m_newProject2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_newResource2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
		this.m_openProjectToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_saveToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_saveAllToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
		this.m_cutToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_copyToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_pasteToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
		this.m_undoToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_redoToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
		this.m_findToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
		this.m_debuggerToolStrip = new System.Windows.Forms.ToolStrip();
		this.m_enableDebuggingToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.m_resumeExecutionToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_breakAllToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_stopDebuggingToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.m_stepIntoToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_stepOverToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_stepOutToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_toolStripContainer = new System.Windows.Forms.ToolStripContainer();
		this.m_menuStrip.SuspendLayout();
		this.m_statusStrip.SuspendLayout();
		this.m_standardToolStrip.SuspendLayout();
		this.m_debuggerToolStrip.SuspendLayout();
		this.m_toolStripContainer.TopToolStripPanel.SuspendLayout();
		this.m_toolStripContainer.SuspendLayout();
		base.SuspendLayout();
		this.m_menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.m_fileToolStripMenuItem, this.m_editToolStripMenuItem, this.m_viewToolStripMenuItem, this.m_buildToolStripMenuItem, this.toolStripMenuItem14, this.m_toolsToolStripMenuItem, this.m_windowToolStripMenuItem, this.m_helpToolStripMenuItem });
		this.m_menuStrip.Location = new System.Drawing.Point(0, 0);
		this.m_menuStrip.Name = "m_menuStrip";
		this.m_menuStrip.Size = new System.Drawing.Size(800, 24);
		this.m_menuStrip.TabIndex = 1;
		this.m_menuStrip.Text = "menuStrip1";
		this.m_fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[19]
		{
			this.m_newToolStripMenuItem, this.m_openToolStripMenuItem, this.toolStripMenuItem5, this.m_addToolStripMenuItem, this.toolStripMenuItem16, this.m_closeToolStripMenuItem, this.m_closeProjectToolStripMenuItem, this.toolStripMenuItem6, this.m_saveToolStripMenuItem, this.m_saveAsToolStripMenuItem,
			this.m_saveAllToolStripMenuItem, this.toolStripMenuItem7, this.m_pageSetupToolStripMenuItem, this.m_printToolStripMenuItem, this.toolStripMenuItem8, this.m_recentResourcesToolStripMenuItem, this.m_recentProjectsToolStripMenuItem, this.toolStripMenuItem9, this.m_exitToolStripMenuItem
		});
		this.m_fileToolStripMenuItem.Name = "m_fileToolStripMenuItem";
		this.m_fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
		this.m_fileToolStripMenuItem.Text = "&File";
		this.m_newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.m_newProjectToolStripMenuItem, this.m_newResourceToolStripMenuItem });
		this.m_newToolStripMenuItem.Name = "m_newToolStripMenuItem";
		this.m_newToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_newToolStripMenuItem.Text = "New";
		this.m_newProjectToolStripMenuItem.Name = "m_newProjectToolStripMenuItem";
		this.m_newProjectToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
		this.m_newProjectToolStripMenuItem.Text = "Project...";
		this.m_newProjectToolStripMenuItem.Click += new System.EventHandler(OnNewProject);
		this.m_newResourceToolStripMenuItem.Name = "m_newResourceToolStripMenuItem";
		this.m_newResourceToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
		this.m_newResourceToolStripMenuItem.Text = "Resource...";
		this.m_newResourceToolStripMenuItem.Click += new System.EventHandler(OnNewResource);
		this.m_openToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Open;
		this.m_openToolStripMenuItem.Name = "m_openToolStripMenuItem";
		this.m_openToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_openToolStripMenuItem.Text = "Open...";
		this.toolStripMenuItem5.Name = "toolStripMenuItem5";
		this.toolStripMenuItem5.Size = new System.Drawing.Size(163, 6);
		this.m_addToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.m_addNewBundleToolStripMenuItem, this.m_addExistingBundleToolStripMenuItem });
		this.m_addToolStripMenuItem.Enabled = false;
		this.m_addToolStripMenuItem.Name = "m_addToolStripMenuItem";
		this.m_addToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_addToolStripMenuItem.Text = "Add";
		this.m_addNewBundleToolStripMenuItem.Enabled = false;
		this.m_addNewBundleToolStripMenuItem.Name = "m_addNewBundleToolStripMenuItem";
		this.m_addNewBundleToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
		this.m_addNewBundleToolStripMenuItem.Text = "New Bundle...";
		this.m_addNewBundleToolStripMenuItem.Click += new System.EventHandler(OnAddNewBundle);
		this.m_addExistingBundleToolStripMenuItem.Enabled = false;
		this.m_addExistingBundleToolStripMenuItem.Name = "m_addExistingBundleToolStripMenuItem";
		this.m_addExistingBundleToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
		this.m_addExistingBundleToolStripMenuItem.Text = "Existing Bundle...";
		this.m_addExistingBundleToolStripMenuItem.Click += new System.EventHandler(OnAddExistingBundle);
		this.toolStripMenuItem16.Name = "toolStripMenuItem16";
		this.toolStripMenuItem16.Size = new System.Drawing.Size(163, 6);
		this.m_closeToolStripMenuItem.Enabled = false;
		this.m_closeToolStripMenuItem.Name = "m_closeToolStripMenuItem";
		this.m_closeToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_closeToolStripMenuItem.Text = "Close";
		this.m_closeProjectToolStripMenuItem.Enabled = false;
		this.m_closeProjectToolStripMenuItem.Name = "m_closeProjectToolStripMenuItem";
		this.m_closeProjectToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_closeProjectToolStripMenuItem.Text = "Close Project";
		this.toolStripMenuItem6.Name = "toolStripMenuItem6";
		this.toolStripMenuItem6.Size = new System.Drawing.Size(163, 6);
		this.m_saveToolStripMenuItem.Enabled = false;
		this.m_saveToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Save;
		this.m_saveToolStripMenuItem.Name = "m_saveToolStripMenuItem";
		this.m_saveToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.S | System.Windows.Forms.Keys.Control;
		this.m_saveToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_saveToolStripMenuItem.Text = "Save";
		this.m_saveAsToolStripMenuItem.Enabled = false;
		this.m_saveAsToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.SaveAs;
		this.m_saveAsToolStripMenuItem.Name = "m_saveAsToolStripMenuItem";
		this.m_saveAsToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_saveAsToolStripMenuItem.Text = "Save As...";
		this.m_saveAllToolStripMenuItem.Enabled = false;
		this.m_saveAllToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.SaveAll;
		this.m_saveAllToolStripMenuItem.Name = "m_saveAllToolStripMenuItem";
		this.m_saveAllToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_saveAllToolStripMenuItem.Text = "Save All";
		this.toolStripMenuItem7.Name = "toolStripMenuItem7";
		this.toolStripMenuItem7.Size = new System.Drawing.Size(163, 6);
		this.m_pageSetupToolStripMenuItem.Enabled = false;
		this.m_pageSetupToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.PageSetup;
		this.m_pageSetupToolStripMenuItem.Name = "m_pageSetupToolStripMenuItem";
		this.m_pageSetupToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_pageSetupToolStripMenuItem.Text = "Page Setup...";
		this.m_printToolStripMenuItem.Enabled = false;
		this.m_printToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Print;
		this.m_printToolStripMenuItem.Name = "m_printToolStripMenuItem";
		this.m_printToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.P | System.Windows.Forms.Keys.Control;
		this.m_printToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_printToolStripMenuItem.Text = "Print...";
		this.toolStripMenuItem8.Name = "toolStripMenuItem8";
		this.toolStripMenuItem8.Size = new System.Drawing.Size(163, 6);
		this.m_recentResourcesToolStripMenuItem.Enabled = false;
		this.m_recentResourcesToolStripMenuItem.Name = "m_recentResourcesToolStripMenuItem";
		this.m_recentResourcesToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_recentResourcesToolStripMenuItem.Text = "Recent Resources";
		this.m_recentProjectsToolStripMenuItem.Enabled = false;
		this.m_recentProjectsToolStripMenuItem.Name = "m_recentProjectsToolStripMenuItem";
		this.m_recentProjectsToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_recentProjectsToolStripMenuItem.Text = "Recent Projects";
		this.toolStripMenuItem9.Name = "toolStripMenuItem9";
		this.toolStripMenuItem9.Size = new System.Drawing.Size(163, 6);
		this.m_exitToolStripMenuItem.Name = "m_exitToolStripMenuItem";
		this.m_exitToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4 | System.Windows.Forms.Keys.Alt;
		this.m_exitToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
		this.m_exitToolStripMenuItem.Text = "E&xit";
		this.m_exitToolStripMenuItem.Click += new System.EventHandler(OnExit);
		this.m_editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[14]
		{
			this.m_undoToolStripMenuItem, this.m_redoToolStripMenuItem, this.toolStripMenuItem10, this.m_cutToolStripMenuItem, this.m_copyToolStripMenuItem, this.m_pasteToolStripMenuItem, this.m_deleteToolStripMenuItem, this.toolStripMenuItem11, this.m_selectAllToolStripMenuItem, this.toolStripMenuItem12,
			this.m_findAndReplaceToolStripMenuItem, this.goToToolStripMenuItem, this.toolStripMenuItem13, this.m_bookmarksToolStripMenuItem
		});
		this.m_editToolStripMenuItem.Name = "m_editToolStripMenuItem";
		this.m_editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
		this.m_editToolStripMenuItem.Text = "&Edit";
		this.m_undoToolStripMenuItem.Enabled = false;
		this.m_undoToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Undo;
		this.m_undoToolStripMenuItem.Name = "m_undoToolStripMenuItem";
		this.m_undoToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
		this.m_undoToolStripMenuItem.Text = "Undo";
		this.m_redoToolStripMenuItem.Enabled = false;
		this.m_redoToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Redo;
		this.m_redoToolStripMenuItem.Name = "m_redoToolStripMenuItem";
		this.m_redoToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
		this.m_redoToolStripMenuItem.Text = "Redo";
		this.toolStripMenuItem10.Name = "toolStripMenuItem10";
		this.toolStripMenuItem10.Size = new System.Drawing.Size(161, 6);
		this.m_cutToolStripMenuItem.Enabled = false;
		this.m_cutToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Cut;
		this.m_cutToolStripMenuItem.Name = "m_cutToolStripMenuItem";
		this.m_cutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.X | System.Windows.Forms.Keys.Control;
		this.m_cutToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
		this.m_cutToolStripMenuItem.Text = "Cut";
		this.m_copyToolStripMenuItem.Enabled = false;
		this.m_copyToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Copy;
		this.m_copyToolStripMenuItem.Name = "m_copyToolStripMenuItem";
		this.m_copyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.C | System.Windows.Forms.Keys.Control;
		this.m_copyToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
		this.m_copyToolStripMenuItem.Text = "Copy";
		this.m_pasteToolStripMenuItem.Enabled = false;
		this.m_pasteToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Paste;
		this.m_pasteToolStripMenuItem.Name = "m_pasteToolStripMenuItem";
		this.m_pasteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.V | System.Windows.Forms.Keys.Control;
		this.m_pasteToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
		this.m_pasteToolStripMenuItem.Text = "Paste";
		this.m_deleteToolStripMenuItem.Enabled = false;
		this.m_deleteToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Delete;
		this.m_deleteToolStripMenuItem.Name = "m_deleteToolStripMenuItem";
		this.m_deleteToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
		this.m_deleteToolStripMenuItem.Text = "Delete";
		this.toolStripMenuItem11.Name = "toolStripMenuItem11";
		this.toolStripMenuItem11.Size = new System.Drawing.Size(161, 6);
		this.m_selectAllToolStripMenuItem.Enabled = false;
		this.m_selectAllToolStripMenuItem.Name = "m_selectAllToolStripMenuItem";
		this.m_selectAllToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.A | System.Windows.Forms.Keys.Control;
		this.m_selectAllToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
		this.m_selectAllToolStripMenuItem.Text = "Select All";
		this.toolStripMenuItem12.Name = "toolStripMenuItem12";
		this.toolStripMenuItem12.Size = new System.Drawing.Size(161, 6);
		this.m_findAndReplaceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.m_findToolStripMenuItem, this.m_replaceToolStripMenuItem });
		this.m_findAndReplaceToolStripMenuItem.Name = "m_findAndReplaceToolStripMenuItem";
		this.m_findAndReplaceToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
		this.m_findAndReplaceToolStripMenuItem.Text = "Find and Replace";
		this.m_findToolStripMenuItem.Name = "m_findToolStripMenuItem";
		this.m_findToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
		this.m_findToolStripMenuItem.Text = "Find...";
		this.m_replaceToolStripMenuItem.Name = "m_replaceToolStripMenuItem";
		this.m_replaceToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
		this.m_replaceToolStripMenuItem.Text = "Replace...";
		this.goToToolStripMenuItem.Enabled = false;
		this.goToToolStripMenuItem.Name = "goToToolStripMenuItem";
		this.goToToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
		this.goToToolStripMenuItem.Text = "Go To...";
		this.toolStripMenuItem13.Name = "toolStripMenuItem13";
		this.toolStripMenuItem13.Size = new System.Drawing.Size(161, 6);
		this.m_bookmarksToolStripMenuItem.Name = "m_bookmarksToolStripMenuItem";
		this.m_bookmarksToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
		this.m_bookmarksToolStripMenuItem.Text = "Bookmarks";
		this.m_viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[19]
		{
			this.m_viewProjectExplorerToolStripMenuItem, this.toolStripMenuItem1, this.m_timersToolStripMenuItem, this.m_viewStackToolStripMenuItem, this.m_bundleFiltersToolStripMenuItem, this.toolStripMenuItem3, this.m_globalsToolStripMenuItem, this.m_localsToolStripMenuItem, this.m_callstackToolStripMenuItem, this.m_immediateWindowToolStripMenuItem,
			this.toolStripMenuItem4, this.m_errorListToolStripMenuItem, this.m_outputToolStripMenuItem, this.m_propertiesToolStripMenuItem, this.toolStripMenuItem2, this.m_legacyDebuggerToolStripMenuItem, this.toolStripMenuItem15, this.m_showWhitespaceToolStripMenuItem, this.m_showEOLMarkerToolStripMenuItem
		});
		this.m_viewToolStripMenuItem.Name = "m_viewToolStripMenuItem";
		this.m_viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
		this.m_viewToolStripMenuItem.Text = "&View";
		this.m_viewProjectExplorerToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Bundle;
		this.m_viewProjectExplorerToolStripMenuItem.Name = "m_viewProjectExplorerToolStripMenuItem";
		this.m_viewProjectExplorerToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.L | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt;
		this.m_viewProjectExplorerToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_viewProjectExplorerToolStripMenuItem.Text = "Project Explorer";
		this.m_viewProjectExplorerToolStripMenuItem.Click += new System.EventHandler(OnShowBundleExplorer);
		this.toolStripMenuItem1.Name = "toolStripMenuItem1";
		this.toolStripMenuItem1.Size = new System.Drawing.Size(235, 6);
		this.m_timersToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Timers;
		this.m_timersToolStripMenuItem.Name = "m_timersToolStripMenuItem";
		this.m_timersToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_timersToolStripMenuItem.Text = "Timers";
		this.m_timersToolStripMenuItem.Click += new System.EventHandler(OnShowTimers);
		this.m_viewStackToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Views;
		this.m_viewStackToolStripMenuItem.Name = "m_viewStackToolStripMenuItem";
		this.m_viewStackToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_viewStackToolStripMenuItem.Text = "View Stack";
		this.m_viewStackToolStripMenuItem.Click += new System.EventHandler(OnShowViewStack);
		this.m_bundleFiltersToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Filter;
		this.m_bundleFiltersToolStripMenuItem.Name = "m_bundleFiltersToolStripMenuItem";
		this.m_bundleFiltersToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_bundleFiltersToolStripMenuItem.Text = "Bundle Filters";
		this.m_bundleFiltersToolStripMenuItem.Click += new System.EventHandler(OnShowBundleFilters);
		this.toolStripMenuItem3.Name = "toolStripMenuItem3";
		this.toolStripMenuItem3.Size = new System.Drawing.Size(235, 6);
		this.m_globalsToolStripMenuItem.Name = "m_globalsToolStripMenuItem";
		this.m_globalsToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_globalsToolStripMenuItem.Text = "Globals";
		this.m_globalsToolStripMenuItem.Click += new System.EventHandler(OnShowGlobals);
		this.m_localsToolStripMenuItem.Name = "m_localsToolStripMenuItem";
		this.m_localsToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_localsToolStripMenuItem.Text = "Locals";
		this.m_localsToolStripMenuItem.Click += new System.EventHandler(OnShowLocals);
		this.m_callstackToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.CallStack;
		this.m_callstackToolStripMenuItem.Name = "m_callstackToolStripMenuItem";
		this.m_callstackToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_callstackToolStripMenuItem.Text = "Call Stack";
		this.m_callstackToolStripMenuItem.Click += new System.EventHandler(OnShowCallStack);
		this.m_immediateWindowToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.ImmediateWindow;
		this.m_immediateWindowToolStripMenuItem.Name = "m_immediateWindowToolStripMenuItem";
		this.m_immediateWindowToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.I | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt;
		this.m_immediateWindowToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_immediateWindowToolStripMenuItem.Text = "Immediate Window";
		this.m_immediateWindowToolStripMenuItem.Click += new System.EventHandler(OnShowImmediateWindow);
		this.toolStripMenuItem4.Name = "toolStripMenuItem4";
		this.toolStripMenuItem4.Size = new System.Drawing.Size(235, 6);
		this.m_errorListToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.ErrorList;
		this.m_errorListToolStripMenuItem.Name = "m_errorListToolStripMenuItem";
		this.m_errorListToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_errorListToolStripMenuItem.Text = "Error List";
		this.m_errorListToolStripMenuItem.Click += new System.EventHandler(OnShowErrorList);
		this.m_outputToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Output;
		this.m_outputToolStripMenuItem.Name = "m_outputToolStripMenuItem";
		this.m_outputToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_outputToolStripMenuItem.Text = "Output";
		this.m_outputToolStripMenuItem.Click += new System.EventHandler(OnShowOutput);
		this.m_propertiesToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Properties;
		this.m_propertiesToolStripMenuItem.Name = "m_propertiesToolStripMenuItem";
		this.m_propertiesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
		this.m_propertiesToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_propertiesToolStripMenuItem.Text = "Properties Window";
		this.m_propertiesToolStripMenuItem.Click += new System.EventHandler(OnShowProperties);
		this.toolStripMenuItem2.Name = "toolStripMenuItem2";
		this.toolStripMenuItem2.Size = new System.Drawing.Size(235, 6);
		this.m_legacyDebuggerToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Debugger;
		this.m_legacyDebuggerToolStripMenuItem.Name = "m_legacyDebuggerToolStripMenuItem";
		this.m_legacyDebuggerToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_legacyDebuggerToolStripMenuItem.Text = "Legacy Debugger...";
		this.m_legacyDebuggerToolStripMenuItem.Click += new System.EventHandler(OnShowLegacyDebugger);
		this.toolStripMenuItem15.Name = "toolStripMenuItem15";
		this.toolStripMenuItem15.Size = new System.Drawing.Size(235, 6);
		this.m_showWhitespaceToolStripMenuItem.Enabled = false;
		this.m_showWhitespaceToolStripMenuItem.Name = "m_showWhitespaceToolStripMenuItem";
		this.m_showWhitespaceToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_showWhitespaceToolStripMenuItem.Text = "Show Whitespace";
		this.m_showEOLMarkerToolStripMenuItem.Enabled = false;
		this.m_showEOLMarkerToolStripMenuItem.Name = "m_showEOLMarkerToolStripMenuItem";
		this.m_showEOLMarkerToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
		this.m_showEOLMarkerToolStripMenuItem.Text = "Show EOL Marker";
		this.m_buildToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.m_buildBundleToolStripMenuItem });
		this.m_buildToolStripMenuItem.Name = "m_buildToolStripMenuItem";
		this.m_buildToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
		this.m_buildToolStripMenuItem.Text = "&Build";
		this.m_buildBundleToolStripMenuItem.Name = "m_buildBundleToolStripMenuItem";
		this.m_buildBundleToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
		this.m_buildBundleToolStripMenuItem.Text = "Build Bundle";
		this.toolStripMenuItem14.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[13]
		{
			this.m_enableDebuggingToolStripMenuItem, this.toolStripSeparator2, this.m_resumeExecutionToolStripMenuItem, this.m_breakAllToolStripMenuItem, this.m_stopDebugginToolStripMenuItem, this.toolStripSeparator1, this.m_stepIntoToolStripMenuItem, this.m_stepOverToolStripMenuItem, this.m_stepOutToolStripMenuItem, this.toolStripSeparator3,
			this.m_toggleBreakpointToolStripMenuItem, this.m_deleteAllBreakpointToolStripMenuItem, this.m_breakpointsToolStripMenuItem
		});
		this.toolStripMenuItem14.Name = "toolStripMenuItem14";
		this.toolStripMenuItem14.Size = new System.Drawing.Size(54, 20);
		this.toolStripMenuItem14.Text = "&Debug";
		this.m_enableDebuggingToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Debugger;
		this.m_enableDebuggingToolStripMenuItem.Name = "m_enableDebuggingToolStripMenuItem";
		this.m_enableDebuggingToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
		this.m_enableDebuggingToolStripMenuItem.Text = "Enable Debugging";
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(187, 6);
		this.m_resumeExecutionToolStripMenuItem.Enabled = false;
		this.m_resumeExecutionToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugPlay;
		this.m_resumeExecutionToolStripMenuItem.Name = "m_resumeExecutionToolStripMenuItem";
		this.m_resumeExecutionToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
		this.m_resumeExecutionToolStripMenuItem.Text = "Resume Execution";
		this.m_breakAllToolStripMenuItem.Enabled = false;
		this.m_breakAllToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugPause;
		this.m_breakAllToolStripMenuItem.Name = "m_breakAllToolStripMenuItem";
		this.m_breakAllToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
		this.m_breakAllToolStripMenuItem.Text = "Break All";
		this.m_stopDebugginToolStripMenuItem.Enabled = false;
		this.m_stopDebugginToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStop;
		this.m_stopDebugginToolStripMenuItem.Name = "m_stopDebugginToolStripMenuItem";
		this.m_stopDebugginToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
		this.m_stopDebugginToolStripMenuItem.Text = "Stop Debugging";
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(187, 6);
		this.m_stepIntoToolStripMenuItem.Enabled = false;
		this.m_stepIntoToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepInto;
		this.m_stepIntoToolStripMenuItem.Name = "m_stepIntoToolStripMenuItem";
		this.m_stepIntoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F11;
		this.m_stepIntoToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
		this.m_stepIntoToolStripMenuItem.Text = "Step Into";
		this.m_stepOverToolStripMenuItem.Enabled = false;
		this.m_stepOverToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepOver;
		this.m_stepOverToolStripMenuItem.Name = "m_stepOverToolStripMenuItem";
		this.m_stepOverToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F10;
		this.m_stepOverToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
		this.m_stepOverToolStripMenuItem.Text = "Step Over";
		this.m_stepOutToolStripMenuItem.Enabled = false;
		this.m_stepOutToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepOut;
		this.m_stepOutToolStripMenuItem.Name = "m_stepOutToolStripMenuItem";
		this.m_stepOutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F11 | System.Windows.Forms.Keys.Shift;
		this.m_stepOutToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
		this.m_stepOutToolStripMenuItem.Text = "Step Out";
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(187, 6);
		this.m_toggleBreakpointToolStripMenuItem.Enabled = false;
		this.m_toggleBreakpointToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugBreakpoint;
		this.m_toggleBreakpointToolStripMenuItem.Name = "m_toggleBreakpointToolStripMenuItem";
		this.m_toggleBreakpointToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
		this.m_toggleBreakpointToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
		this.m_toggleBreakpointToolStripMenuItem.Text = "Toggle Breakpoint";
		this.m_deleteAllBreakpointToolStripMenuItem.Enabled = false;
		this.m_deleteAllBreakpointToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugBreakpointClear;
		this.m_deleteAllBreakpointToolStripMenuItem.Name = "m_deleteAllBreakpointToolStripMenuItem";
		this.m_deleteAllBreakpointToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
		this.m_deleteAllBreakpointToolStripMenuItem.Text = "Delete All Breakpoints";
		this.m_breakpointsToolStripMenuItem.Name = "m_breakpointsToolStripMenuItem";
		this.m_breakpointsToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
		this.m_breakpointsToolStripMenuItem.Text = "Breakpoints";
		this.m_breakpointsToolStripMenuItem.Visible = false;
		this.m_toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.m_optionsToolStripMenuItem });
		this.m_toolsToolStripMenuItem.Name = "m_toolsToolStripMenuItem";
		this.m_toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
		this.m_toolsToolStripMenuItem.Text = "&Tools";
		this.m_optionsToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Tools;
		this.m_optionsToolStripMenuItem.Name = "m_optionsToolStripMenuItem";
		this.m_optionsToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
		this.m_optionsToolStripMenuItem.Text = "Options...";
		this.m_optionsToolStripMenuItem.Click += new System.EventHandler(OnShowOptions);
		this.m_windowToolStripMenuItem.Name = "m_windowToolStripMenuItem";
		this.m_windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
		this.m_windowToolStripMenuItem.Text = "&Window";
		this.m_helpToolStripMenuItem.Name = "m_helpToolStripMenuItem";
		this.m_helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
		this.m_helpToolStripMenuItem.Text = "&Help";
		this.m_statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.m_toolStripStatusLabel, this.m_toolStripProgressBar, this.m_editorToolStripStatusLabel });
		this.m_statusStrip.Location = new System.Drawing.Point(0, 551);
		this.m_statusStrip.Name = "m_statusStrip";
		this.m_statusStrip.Size = new System.Drawing.Size(800, 22);
		this.m_statusStrip.TabIndex = 2;
		this.m_statusStrip.Text = "statusStrip1";
		this.m_toolStripStatusLabel.Name = "m_toolStripStatusLabel";
		this.m_toolStripStatusLabel.Size = new System.Drawing.Size(785, 17);
		this.m_toolStripStatusLabel.Spring = true;
		this.m_toolStripStatusLabel.Text = "Ready";
		this.m_toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_toolStripProgressBar.Name = "m_toolStripProgressBar";
		this.m_toolStripProgressBar.Size = new System.Drawing.Size(100, 16);
		this.m_toolStripProgressBar.Visible = false;
		this.m_editorToolStripStatusLabel.AutoSize = false;
		this.m_editorToolStripStatusLabel.Name = "m_editorToolStripStatusLabel";
		this.m_editorToolStripStatusLabel.Size = new System.Drawing.Size(150, 17);
		this.m_editorToolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_editorToolStripStatusLabel.Visible = false;
		this.m_dockPanel.ActiveAutoHideContent = null;
		this.m_dockPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_dockPanel.DockBackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_dockPanel.Location = new System.Drawing.Point(0, 76);
		this.m_dockPanel.Name = "m_dockPanel";
		this.m_dockPanel.Size = new System.Drawing.Size(800, 472);
		dockPanelGradient.EndColor = System.Drawing.SystemColors.ControlLight;
		dockPanelGradient.StartColor = System.Drawing.SystemColors.ControlLight;
		autoHideStripSkin.DockStripGradient = dockPanelGradient;
		tabGradient.EndColor = System.Drawing.SystemColors.Control;
		tabGradient.StartColor = System.Drawing.SystemColors.Control;
		tabGradient.TextColor = System.Drawing.SystemColors.ControlDarkDark;
		autoHideStripSkin.TabGradient = tabGradient;
		dockPanelSkin.AutoHideStripSkin = autoHideStripSkin;
		tabGradient2.EndColor = System.Drawing.SystemColors.ControlLightLight;
		tabGradient2.StartColor = System.Drawing.SystemColors.ControlLightLight;
		tabGradient2.TextColor = System.Drawing.SystemColors.ControlText;
		dockPaneStripGradient.ActiveTabGradient = tabGradient2;
		dockPanelGradient2.EndColor = System.Drawing.SystemColors.Control;
		dockPanelGradient2.StartColor = System.Drawing.SystemColors.Control;
		dockPaneStripGradient.DockStripGradient = dockPanelGradient2;
		tabGradient3.EndColor = System.Drawing.SystemColors.ControlLight;
		tabGradient3.StartColor = System.Drawing.SystemColors.ControlLight;
		tabGradient3.TextColor = System.Drawing.SystemColors.ControlText;
		dockPaneStripGradient.InactiveTabGradient = tabGradient3;
		dockPaneStripSkin.DocumentGradient = dockPaneStripGradient;
		tabGradient4.EndColor = System.Drawing.SystemColors.ActiveCaption;
		tabGradient4.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
		tabGradient4.StartColor = System.Drawing.SystemColors.GradientActiveCaption;
		tabGradient4.TextColor = System.Drawing.SystemColors.ActiveCaptionText;
		dockPaneStripToolWindowGradient.ActiveCaptionGradient = tabGradient4;
		tabGradient5.EndColor = System.Drawing.SystemColors.Control;
		tabGradient5.StartColor = System.Drawing.SystemColors.Control;
		tabGradient5.TextColor = System.Drawing.SystemColors.ControlText;
		dockPaneStripToolWindowGradient.ActiveTabGradient = tabGradient5;
		dockPanelGradient3.EndColor = System.Drawing.SystemColors.ControlLight;
		dockPanelGradient3.StartColor = System.Drawing.SystemColors.ControlLight;
		dockPaneStripToolWindowGradient.DockStripGradient = dockPanelGradient3;
		tabGradient6.EndColor = System.Drawing.SystemColors.GradientInactiveCaption;
		tabGradient6.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
		tabGradient6.StartColor = System.Drawing.SystemColors.GradientInactiveCaption;
		tabGradient6.TextColor = System.Drawing.SystemColors.ControlText;
		dockPaneStripToolWindowGradient.InactiveCaptionGradient = tabGradient6;
		tabGradient7.EndColor = System.Drawing.Color.Transparent;
		tabGradient7.StartColor = System.Drawing.Color.Transparent;
		tabGradient7.TextColor = System.Drawing.SystemColors.ControlDarkDark;
		dockPaneStripToolWindowGradient.InactiveTabGradient = tabGradient7;
		dockPaneStripSkin.ToolWindowGradient = dockPaneStripToolWindowGradient;
		dockPanelSkin.DockPaneStripSkin = dockPaneStripSkin;
		this.m_dockPanel.Skin = dockPanelSkin;
		this.m_dockPanel.TabIndex = 3;
		this.m_standardToolStrip.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_standardToolStrip.Dock = System.Windows.Forms.DockStyle.None;
		this.m_standardToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[14]
		{
			this.m_newToolStripDropDownButton, this.toolStripSeparator8, this.m_openProjectToolStripButton, this.m_saveToolStripButton, this.m_saveAllToolStripButton, this.toolStripSeparator6, this.m_cutToolStripButton, this.m_copyToolStripButton, this.m_pasteToolStripButton, this.toolStripSeparator7,
			this.m_undoToolStripButton, this.m_redoToolStripButton, this.toolStripSeparator9, this.m_findToolStripTextBox
		});
		this.m_standardToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
		this.m_standardToolStrip.Location = new System.Drawing.Point(3, 25);
		this.m_standardToolStrip.Name = "m_standardToolStrip";
		this.m_standardToolStrip.Size = new System.Drawing.Size(401, 25);
		this.m_standardToolStrip.TabIndex = 5;
		this.m_standardToolStrip.Text = "Standard";
		this.m_newToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_newToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.m_newProject2ToolStripMenuItem, this.m_newResource2ToolStripMenuItem });
		this.m_newToolStripDropDownButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.NewDocument;
		this.m_newToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_newToolStripDropDownButton.Name = "m_newToolStripDropDownButton";
		this.m_newToolStripDropDownButton.Size = new System.Drawing.Size(29, 22);
		this.m_newToolStripDropDownButton.Text = "toolStripDropDownButton1";
		this.m_newToolStripDropDownButton.ToolTipText = "New";
		this.m_newProject2ToolStripMenuItem.Name = "m_newProject2ToolStripMenuItem";
		this.m_newProject2ToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
		this.m_newProject2ToolStripMenuItem.Text = "New Project...";
		this.m_newProject2ToolStripMenuItem.Click += new System.EventHandler(OnNewProject);
		this.m_newResource2ToolStripMenuItem.Name = "m_newResource2ToolStripMenuItem";
		this.m_newResource2ToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
		this.m_newResource2ToolStripMenuItem.Text = "New Resource...";
		this.toolStripSeparator8.Name = "toolStripSeparator8";
		this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
		this.m_openProjectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_openProjectToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Open;
		this.m_openProjectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_openProjectToolStripButton.Name = "m_openProjectToolStripButton";
		this.m_openProjectToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_openProjectToolStripButton.Text = "Open Project";
		this.m_openProjectToolStripButton.ToolTipText = "Open Project...";
		this.m_saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_saveToolStripButton.Enabled = false;
		this.m_saveToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Save;
		this.m_saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_saveToolStripButton.Name = "m_saveToolStripButton";
		this.m_saveToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_saveToolStripButton.Text = "toolStripButton1";
		this.m_saveToolStripButton.ToolTipText = "Save (Ctrl+S)";
		this.m_saveAllToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_saveAllToolStripButton.Enabled = false;
		this.m_saveAllToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.SaveAll;
		this.m_saveAllToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_saveAllToolStripButton.Name = "m_saveAllToolStripButton";
		this.m_saveAllToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_saveAllToolStripButton.Text = "toolStripButton2";
		this.m_saveAllToolStripButton.ToolTipText = "Save All (Ctrl+Shift+S)";
		this.toolStripSeparator6.Name = "toolStripSeparator6";
		this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
		this.m_cutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_cutToolStripButton.Enabled = false;
		this.m_cutToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Cut;
		this.m_cutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_cutToolStripButton.Name = "m_cutToolStripButton";
		this.m_cutToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_cutToolStripButton.Text = "toolStripButton3";
		this.m_cutToolStripButton.ToolTipText = "Cut (Ctrl+X)";
		this.m_copyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_copyToolStripButton.Enabled = false;
		this.m_copyToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Copy;
		this.m_copyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_copyToolStripButton.Name = "m_copyToolStripButton";
		this.m_copyToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_copyToolStripButton.Text = "toolStripButton4";
		this.m_copyToolStripButton.ToolTipText = "Copy (Ctrl+V)";
		this.m_pasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_pasteToolStripButton.Enabled = false;
		this.m_pasteToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Paste;
		this.m_pasteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_pasteToolStripButton.Name = "m_pasteToolStripButton";
		this.m_pasteToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_pasteToolStripButton.Text = "toolStripButton5";
		this.m_pasteToolStripButton.ToolTipText = "Paste (Ctrl+V)";
		this.toolStripSeparator7.Name = "toolStripSeparator7";
		this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
		this.m_undoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_undoToolStripButton.Enabled = false;
		this.m_undoToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Undo;
		this.m_undoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_undoToolStripButton.Name = "m_undoToolStripButton";
		this.m_undoToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_undoToolStripButton.Text = "toolStripButton6";
		this.m_undoToolStripButton.ToolTipText = "Undo (Ctrl+Z)";
		this.m_redoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_redoToolStripButton.Enabled = false;
		this.m_redoToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Redo;
		this.m_redoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_redoToolStripButton.Name = "m_redoToolStripButton";
		this.m_redoToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_redoToolStripButton.Text = "toolStripButton7";
		this.m_redoToolStripButton.ToolTipText = "Redo (Ctrl+Y)";
		this.toolStripSeparator9.Name = "toolStripSeparator9";
		this.toolStripSeparator9.Size = new System.Drawing.Size(6, 25);
		this.m_findToolStripTextBox.MaxLength = 255;
		this.m_findToolStripTextBox.Name = "m_findToolStripTextBox";
		this.m_findToolStripTextBox.Size = new System.Drawing.Size(150, 25);
		this.m_debuggerToolStrip.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_debuggerToolStrip.Dock = System.Windows.Forms.DockStyle.None;
		this.m_debuggerToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.m_enableDebuggingToolStripButton, this.toolStripSeparator4, this.m_resumeExecutionToolStripButton, this.m_breakAllToolStripButton, this.m_stopDebuggingToolStripButton, this.toolStripSeparator5, this.m_stepIntoToolStripButton, this.m_stepOverToolStripButton, this.m_stepOutToolStripButton });
		this.m_debuggerToolStrip.Location = new System.Drawing.Point(3, 0);
		this.m_debuggerToolStrip.Name = "m_debuggerToolStrip";
		this.m_debuggerToolStrip.Size = new System.Drawing.Size(185, 25);
		this.m_debuggerToolStrip.TabIndex = 7;
		this.m_enableDebuggingToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_enableDebuggingToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Debugger;
		this.m_enableDebuggingToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_enableDebuggingToolStripButton.Name = "m_enableDebuggingToolStripButton";
		this.m_enableDebuggingToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_enableDebuggingToolStripButton.Text = "Enable Debugging";
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
		this.m_resumeExecutionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_resumeExecutionToolStripButton.Enabled = false;
		this.m_resumeExecutionToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugPlay;
		this.m_resumeExecutionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_resumeExecutionToolStripButton.Name = "m_resumeExecutionToolStripButton";
		this.m_resumeExecutionToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_resumeExecutionToolStripButton.Text = "ResumeExecution";
		this.m_resumeExecutionToolStripButton.ToolTipText = "Resume Execution";
		this.m_breakAllToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_breakAllToolStripButton.Enabled = false;
		this.m_breakAllToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugPause;
		this.m_breakAllToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_breakAllToolStripButton.Name = "m_breakAllToolStripButton";
		this.m_breakAllToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_breakAllToolStripButton.Text = "toolStripButton2";
		this.m_breakAllToolStripButton.ToolTipText = "Break All";
		this.m_stopDebuggingToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_stopDebuggingToolStripButton.Enabled = false;
		this.m_stopDebuggingToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStop;
		this.m_stopDebuggingToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_stopDebuggingToolStripButton.Name = "m_stopDebuggingToolStripButton";
		this.m_stopDebuggingToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_stopDebuggingToolStripButton.Text = "toolStripButton3";
		this.m_stopDebuggingToolStripButton.ToolTipText = "Stop Debugging";
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
		this.m_stepIntoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_stepIntoToolStripButton.Enabled = false;
		this.m_stepIntoToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepInto;
		this.m_stepIntoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_stepIntoToolStripButton.Name = "m_stepIntoToolStripButton";
		this.m_stepIntoToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_stepIntoToolStripButton.Text = "toolStripButton4";
		this.m_stepIntoToolStripButton.ToolTipText = "Step Into";
		this.m_stepOverToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_stepOverToolStripButton.Enabled = false;
		this.m_stepOverToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepOver;
		this.m_stepOverToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_stepOverToolStripButton.Name = "m_stepOverToolStripButton";
		this.m_stepOverToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_stepOverToolStripButton.Text = "toolStripButton5";
		this.m_stepOverToolStripButton.ToolTipText = "Step Over";
		this.m_stepOutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_stepOutToolStripButton.Enabled = false;
		this.m_stepOutToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepOut;
		this.m_stepOutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_stepOutToolStripButton.Name = "m_stepOutToolStripButton";
		this.m_stepOutToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_stepOutToolStripButton.Text = "Step Out";
		this.m_toolStripContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_toolStripContainer.ContentPanel.Size = new System.Drawing.Size(800, 2);
		this.m_toolStripContainer.Location = new System.Drawing.Point(0, 24);
		this.m_toolStripContainer.Name = "m_toolStripContainer";
		this.m_toolStripContainer.Size = new System.Drawing.Size(800, 52);
		this.m_toolStripContainer.TabIndex = 8;
		this.m_toolStripContainer.Text = "toolStripContainer1";
		this.m_toolStripContainer.TopToolStripPanel.Controls.Add(this.m_debuggerToolStrip);
		this.m_toolStripContainer.TopToolStripPanel.Controls.Add(this.m_standardToolStrip);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(800, 573);
		base.Controls.Add(this.m_toolStripContainer);
		base.Controls.Add(this.m_dockPanel);
		base.Controls.Add(this.m_statusStrip);
		base.Controls.Add(this.m_menuStrip);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.IsMdiContainer = true;
		base.MainMenuStrip = this.m_menuStrip;
		this.MinimumSize = new System.Drawing.Size(640, 480);
		base.Name = "IdeMainForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Redbox Engine for Distributed Stores";
		base.Activated += new System.EventHandler(OnActivated);
		this.m_menuStrip.ResumeLayout(false);
		this.m_menuStrip.PerformLayout();
		this.m_statusStrip.ResumeLayout(false);
		this.m_statusStrip.PerformLayout();
		this.m_standardToolStrip.ResumeLayout(false);
		this.m_standardToolStrip.PerformLayout();
		this.m_debuggerToolStrip.ResumeLayout(false);
		this.m_debuggerToolStrip.PerformLayout();
		this.m_toolStripContainer.TopToolStripPanel.ResumeLayout(false);
		this.m_toolStripContainer.TopToolStripPanel.PerformLayout();
		this.m_toolStripContainer.ResumeLayout(false);
		this.m_toolStripContainer.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
