using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Alsing.SourceCode;
using Alsing.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using Redbox.Lua;
using Redbox.REDS.Framework;
using Skybound.VisualTips;
using Skybound.VisualTips.Rendering;
using VisualHint.SmartPropertyGrid;

namespace Redbox.KioskEngine.IDE;

public class DebuggerForm : Form
{
	private bool m_isActivated;

	private bool m_isApplicationRunning;

	private bool m_refreshingBreakpoints;

	private static readonly DebuggerForm m_instance = new DebuggerForm();

	private IContainer components;

	private StatusStrip m_statusStrip;

	private MenuStrip m_menuStrip;

	private ToolStripMenuItem m_debugToolStripMenuItem;

	private ToolStripMenuItem m_fileToolStripMenuItem;

	private Splitter m_mainSplitter;

	private SyntaxBoxControl m_syntaxBoxControl;

	private SyntaxDocument m_syntaxDocument;

	private Splitter m_sourceSplitter;

	private ToolStrip m_toolStrip;

	private TabControl m_tabControl;

	private TabPage m_localsTabPage;

	private TabPage m_callStackTabPage;

	private ListView m_callStackListView;

	private ColumnHeader m_funcNameColumnHeader;

	private ColumnHeader m_scriptNameColumnHeader;

	private ColumnHeader m_scriptLineNoColumnHeader;

	private TabPage m_globalsTabPage;

	private TabPage m_immediateTabPage;

	private ShellControl m_shellControl;

	private TreeView m_treeView;

	private ImageList m_imageList;

	private ToolStripButton m_resumeExecutionToolStripButton;

	private ToolStripButton m_breakAllToolStripButton;

	private ToolStripButton m_stopDebuggingToolStripButton;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton m_stepIntoToolStripButton;

	private ToolStripButton m_stepOverToolStripButton;

	private ToolStripMenuItem m_resumeExecutionToolStripMenuItem;

	private ToolStripMenuItem m_breakAllToolStripMenuItem;

	private ToolStripMenuItem m_stopDebugginToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem1;

	private ToolStripMenuItem m_stepIntoToolStripMenuItem;

	private ToolStripMenuItem m_stepOverToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem2;

	private ToolStripMenuItem m_toggleBreakpointToolStripMenuItem;

	private ToolStripMenuItem m_deleteAllBreakpointToolStripMenuItem;

	private ToolStripStatusLabel m_statusLabel;

	private ToolStripMenuItem m_enableDebuggingToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton m_enableDebuggingToolStripButton;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripButton m_stepOutToolStripButton;

	private ToolStripMenuItem m_stepOutToolStripMenuItem;

	private ToolStripMenuItem m_editToolStripMenuItem;

	private DynamicPropertyGrid m_globalsPropertyGrid;

	private DynamicPropertyGrid m_localsPropertyGrid;

	private ToolStripMenuItem m_showWhitespaceToolStripMenuItem;

	private ToolStripMenuItem m_showEOLMarkerToolStripMenuItem;

	private ToolStripMenuItem m_switchBundleToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripMenuItem m_closeToolStripMenuItem;

	private ToolStripMenuItem m_goToToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripMenuItem m_findAndReplaceToolStripMenuItem;

	private ToolStripMenuItem m_quickFindToolStripMenuItem;

	private ToolStripMenuItem m_quickReplaceToolStripMenuItem;

	private VisualTipProvider m_visualTipProvider;

	private ToolStripMenuItem m_breakpointsToolStripMenuItem;

	private ImageList m_gutterImageList;

	public static DebuggerForm Instance => m_instance;

	public IResourceBundleFilter Filter { get; set; }

	public string CurrentResourceName { get; private set; }

	public bool IsApplicationRunning
	{
		get
		{
			return m_isApplicationRunning;
		}
		set
		{
			m_isApplicationRunning = value;
			m_resumeExecutionToolStripButton.Enabled = !m_isApplicationRunning;
			m_resumeExecutionToolStripMenuItem.Enabled = !m_isApplicationRunning;
		}
	}

	public IResourceBundle ActiveBundle { get; set; }

	public IResourceBundleSet ActiveBundleSet { get; set; }

	public LuaDebugger Debugger { get; set; }

	public event EventHandler StartApplication;

	public DebuggerForm()
	{
		InitializeComponent();
		m_syntaxDocument.SetSyntaxFromEmbeddedResource(typeof(DebuggerForm).Assembly, "Redbox.KioskEngine.IDE.Lua.syn");
		m_syntaxBoxControl.GutterIcons.Images.Add(m_gutterImageList.Images[0]);
	}

	public void ActivateDebugger()
	{
		ActivateDebugger(null);
	}

	public void ActivateDebugger(string resourceName)
	{
		ActivateDebugger(resourceName, 1, null);
	}

	public void ActivateDebugger(string resourceName, int lineNumber, string error)
	{
		BringDebuggerToFront();
		if (resourceName != CurrentResourceName && m_treeView.Nodes.Count > 0)
		{
			SelectResourceNode(resourceName, m_treeView.Nodes[0]);
			SetDocumentText(resourceName);
		}
		string text = $"Runtime Error: {error}";
		SetCurrentDebugLine(lineNumber, (!string.IsNullOrEmpty(error)) ? text : null);
		if (!string.IsNullOrEmpty(error) && string.Compare(error, "breakpoint hit", ignoreCase: true) != 0)
		{
			VisualTip tip = new VisualTip(error, "REDS Engine Runtime Error")
			{
				FooterText = $"The error occurred on or near line: {lineNumber}."
			};
			Point point = m_syntaxBoxControl.PointToScreen(m_syntaxBoxControl.Cursor.HotSpot);
			m_visualTipProvider.ShowTip(tip, new Rectangle(point.X + 250, point.Y, 100, 100));
		}
		RefreshTabs();
	}

	public void Resume()
	{
		OnResume(this, EventArgs.Empty);
	}

	public void StepInto()
	{
		OnStepInto(this, EventArgs.Empty);
	}

	public void StepOut()
	{
		OnStepOut(this, EventArgs.Empty);
	}

	public void StepOver()
	{
		OnStepOver(this, EventArgs.Empty);
	}

	public static void BringDebuggerToFront()
	{
		m_instance.Show();
		m_instance.BringToFront();
	}

	public void SetDocumentText(string resourceName)
	{
		if (string.IsNullOrEmpty(resourceName))
		{
			m_syntaxDocument.Text = null;
			Text = "REDS Engine Debugger";
			return;
		}
		string script = GetScript(resourceName);
		if (!string.IsNullOrEmpty(script))
		{
			m_syntaxDocument.Text = script;
			m_syntaxBoxControl.GotoLine(1);
			Text = $"{resourceName} - REDS Engine Debugger";
			CurrentResourceName = resourceName;
			RefreshBreakpoints();
		}
	}

	public void SelectResourceNode(string resourceName)
	{
		SelectResourceNode(resourceName, m_treeView.Nodes[0]);
	}

	public void SetCurrentDebugLine(int lineNumber, string message)
	{
		if (lineNumber < m_syntaxDocument.Lines.Length)
		{
			ClearCurrentLineSymbol();
			int num = lineNumber - 1;
			if (num < 0)
			{
				num = 0;
			}
			m_syntaxBoxControl.GotoLine(num);
			int num2 = num - 5;
			if (num2 < 0)
			{
				num2 = 0;
			}
			m_syntaxBoxControl.ScrollIntoView(num2);
			ShowCurrentLineSymbol();
		}
		if (!string.IsNullOrEmpty(message))
		{
			m_statusLabel.Text = message;
		}
		else
		{
			SetReady();
		}
	}

	public void SetActionButtonsState(bool enabled)
	{
		m_resumeExecutionToolStripButton.Enabled = enabled;
		m_resumeExecutionToolStripMenuItem.Enabled = enabled;
		m_stepIntoToolStripButton.Enabled = enabled;
		m_stepIntoToolStripMenuItem.Enabled = enabled;
		m_stepOverToolStripButton.Enabled = enabled;
		m_stepOverToolStripMenuItem.Enabled = enabled;
		m_stepOutToolStripButton.Enabled = enabled;
		m_stepOutToolStripMenuItem.Enabled = enabled;
	}

	public void RefreshTabs()
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			string text = m_tabControl.SelectedTab.Text;
			if (!(text == "Globals"))
			{
				if (text == "Call Stack")
				{
					RefreshCallStack();
				}
				else
				{
					RefreshLocalList();
				}
			}
			else
			{
				RefreshGlobalList();
			}
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	public void SetDebuggerInstance(object instance)
	{
		Debugger = instance as LuaDebugger;
	}

	private void OnActivated(object sender, EventArgs e)
	{
		Cursor.Show();
		if (!m_isActivated)
		{
			RefreshResourceTree();
			RefreshTabs();
			string value = ServiceLocator.Instance.GetService<IUserSettingsStore>().GetValue<string>("Debugger", "SelectedNodePath");
			SelectNodePath(value);
			SetDebuggerState();
			SetBreakpointState();
			m_isActivated = true;
		}
	}

	private void OnAfterSelect(object sender, TreeViewEventArgs e)
	{
		ServiceLocator.Instance.GetService<IUserSettingsStore>().SetValue("Debugger", "SelectedNodePath", e.Node.FullPath);
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		m_visualTipProvider.HideTip();
		if (Debugger != null)
		{
			Debugger.Run();
		}
		Cursor.Hide();
		Hide();
		m_isActivated = false;
		e.Cancel = true;
	}

	private void OnCommandEntered(object sender, CommandEnteredEventArgs e)
	{
		try
		{
			ReadOnlyCollection<object> readOnlyCollection = Debugger.Lua.DoString(e.Command);
			if (readOnlyCollection == null || readOnlyCollection.Count == 0)
			{
				m_shellControl.WriteText("nil");
				return;
			}
			foreach (object item in readOnlyCollection)
			{
				m_shellControl.WriteText(LuaHelper.FormatLuaValue(item));
			}
		}
		catch (Exception ex)
		{
			m_shellControl.WriteText(ex.Message);
		}
	}

	private void OnToggleDebugging(object sender, EventArgs e)
	{
		IUserSettingsStore service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
		bool value = service.GetValue<bool>("Debugger", "Enabled");
		if (value)
		{
			OnDeleteAllBreakpoints(sender, e);
		}
		service.SetValue("Debugger", "Enabled", !value);
		SetDebuggerState();
		SetBreakpointState();
	}

	private void OnTreeDoubleClick(object sender, EventArgs e)
	{
		if (m_treeView.SelectedNode != null && m_treeView.SelectedNode.Tag is IResource resource && string.Compare(resource.Type.Name, "script", ignoreCase: true) == 0)
		{
			SetDocumentText(resource.Name);
		}
	}

	private void OnToggleBreakpoint(object sender, EventArgs e)
	{
		ToggleBreakpoint();
	}

	private void OnDeleteAllBreakpoints(object sender, EventArgs e)
	{
		m_syntaxDocument.ClearBreakpoints();
		if (Debugger != null)
		{
			Debugger.ClearAllBreakpoints();
			SetBreakpointState();
		}
	}

	private void OnResume(object sender, EventArgs e)
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			ClearCurrentLineSymbol();
			SetActionButtonsState(enabled: false);
			if (!m_isApplicationRunning && StartApplication != null)
			{
				StartApplication(this, EventArgs.Empty);
			}
			else
			{
				Debugger.Run();
			}
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnStepOut(object sender, EventArgs e)
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			ClearCurrentLineSymbol();
			SetActionButtonsState(enabled: false);
			Debugger.StepOut();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnStepInto(object sender, EventArgs e)
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			ClearCurrentLineSymbol();
			SetActionButtonsState(enabled: false);
			Debugger.StepInto();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnStepOver(object sender, EventArgs e)
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			ClearCurrentLineSymbol();
			SetActionButtonsState(enabled: false);
			Debugger.StepOver();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnBreakpointAdded(object sender, RowEventArgs e)
	{
		if (!m_refreshingBreakpoints)
		{
			Debugger.AddBreakpoint(CurrentResourceName, e.Row.Index + 1);
			SetBreakpointState();
		}
	}

	private void OnBreakpointRemoved(object sender, RowEventArgs e)
	{
		if (!m_refreshingBreakpoints)
		{
			Debugger.RemoveBreakpoint(CurrentResourceName, e.Row.Index + 1);
			SetBreakpointState();
		}
	}

	private void SetReady()
	{
		m_statusLabel.Text = "Ready";
	}

	private void OnCaretChange(object sender, EventArgs e)
	{
		SetBreakpointState();
	}

	private void ToggleBreakpoint()
	{
		int index = m_syntaxBoxControl.Caret.Position.Y;
		m_syntaxDocument[index].Breakpoint = !m_syntaxDocument[index].Breakpoint;
	}

	private void RefreshBreakpoints()
	{
		if (m_refreshingBreakpoints)
		{
			return;
		}
		try
		{
			m_refreshingBreakpoints = true;
			m_syntaxDocument.ClearBreakpoints();
			if (Debugger == null)
			{
				return;
			}
			LuaDebugBreakpointContainer breakpoints = Debugger.GetBreakpoints(CurrentResourceName);
			m_syntaxBoxControl.AllowBreakPoints = breakpoints.Count > 0;
			foreach (LuaDebugBreakpoint item in breakpoints)
			{
				m_syntaxDocument[item.Line - 1].Breakpoint = item.Enabled;
				if (!item.Enabled)
				{
					m_syntaxDocument[item.Line - 1].Images.Add(13);
				}
				else
				{
					m_syntaxDocument[item.Line - 1].Images.Remove(13);
				}
			}
		}
		finally
		{
			m_refreshingBreakpoints = false;
			SetBreakpointState();
		}
	}

	private void ShowCurrentLineSymbol()
	{
		m_syntaxBoxControl.Document[m_syntaxBoxControl.Caret.Position.Y].Images.Add(2);
	}

	private void ClearCurrentLineSymbol()
	{
		m_syntaxBoxControl.Document[m_syntaxBoxControl.Caret.Position.Y].Images.Remove(2);
	}

	private void SelectNodePath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		string[] array = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		int num = 0;
		TreeNode[] array2 = m_treeView.Nodes.Find(array[num++], searchAllChildren: false);
		if (array2.Length == 0)
		{
			return;
		}
		TreeNode treeNode;
		for (treeNode = array2[0]; treeNode != null; treeNode = array2[0])
		{
			treeNode.Expand();
			if (num >= array.Length)
			{
				break;
			}
			array2 = treeNode.Nodes.Find(array[num++], searchAllChildren: false);
			if (array2.Length == 0)
			{
				break;
			}
		}
		m_treeView.SelectedNode = treeNode;
	}

	private void SetDebuggerState()
	{
		IUserSettingsStore service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
		bool flag = Debugger != null && service.GetValue<bool>("Debugger", "Enabled");
		m_enableDebuggingToolStripMenuItem.Checked = flag;
		m_enableDebuggingToolStripMenuItem.Text = (flag ? "Disable Debugging" : "Enable Debugging");
		m_enableDebuggingToolStripButton.Checked = flag;
		m_enableDebuggingToolStripButton.Text = (flag ? "Disable Debugging" : "Enable Debugging");
		if (Debugger != null)
		{
			if (!flag)
			{
				Debugger.State = LuaDebuggerState.Disabled;
			}
			else if (Debugger.State == LuaDebuggerState.Disabled)
			{
				Debugger.State = LuaDebuggerState.Running;
			}
		}
	}

	private void SetBreakpointState()
	{
		IUserSettingsStore service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
		m_syntaxBoxControl.AllowBreakPoints = false;
		m_breakpointsToolStripMenuItem.Visible = false;
		m_toggleBreakpointToolStripMenuItem.Enabled = false;
		m_deleteAllBreakpointToolStripMenuItem.Enabled = false;
		if (Debugger == null || !service.GetValue<bool>("Debugger", "Enabled"))
		{
			return;
		}
		m_syntaxBoxControl.AllowBreakPoints = true;
		m_toggleBreakpointToolStripMenuItem.Enabled = !string.IsNullOrEmpty(CurrentResourceName);
		LuaDebugFileContainer files = Debugger.GetFiles();
		if (files.Count == 0)
		{
			return;
		}
		bool flag = false;
		m_breakpointsToolStripMenuItem.DropDownItems.Clear();
		foreach (LuaDebugFile item in files)
		{
			LuaDebugFile workingFile = item;
			foreach (LuaDebugBreakpoint breakpoint in item.Breakpoints)
			{
				LuaDebugBreakpoint workingBreakpoint = breakpoint;
				ToolStripMenuItem obj = (ToolStripMenuItem)m_breakpointsToolStripMenuItem.DropDownItems.Add($"Script: {item.FileName}, Line: {breakpoint.Line}");
				obj.Click += delegate(object o, EventArgs e)
				{
					try
					{
						m_refreshingBreakpoints = true;
						workingBreakpoint.Enabled = !workingBreakpoint.Enabled;
						((ToolStripMenuItem)o).Checked = workingBreakpoint.Enabled;
						if (CurrentResourceName == workingFile.FileName)
						{
							m_syntaxDocument[workingBreakpoint.Line - 1].Breakpoint = workingBreakpoint.Enabled;
							if (!workingBreakpoint.Enabled)
							{
								m_syntaxDocument[workingBreakpoint.Line - 1].Images.Add(13);
							}
							else
							{
								m_syntaxDocument[workingBreakpoint.Line - 1].Images.Remove(13);
							}
						}
					}
					finally
					{
						m_refreshingBreakpoints = false;
					}
				};
				obj.Checked = breakpoint.Enabled;
				flag = true;
			}
		}
		m_breakpointsToolStripMenuItem.Visible = flag;
		m_deleteAllBreakpointToolStripMenuItem.Enabled = flag;
	}

	private void RefreshGlobalList()
	{
		if (Debugger != null)
		{
			RefreshVariableTable(m_globalsPropertyGrid, "_G", Debugger.GetGlobalVars());
		}
	}

	private void RefreshLocalList()
	{
		if (Debugger != null)
		{
			RefreshVariableTable(m_localsPropertyGrid, "Locals", Debugger.GetLocalVars());
		}
	}

	private void OnTabSelected(object sender, TabControlEventArgs e)
	{
		RefreshTabs();
	}

	private void OnFind(object sender, EventArgs e)
	{
		m_syntaxBoxControl.ShowFind();
	}

	private void OnReplace(object sender, EventArgs e)
	{
		m_syntaxBoxControl.ShowReplace();
	}

	private void OnGoTo(object sender, EventArgs e)
	{
		m_syntaxBoxControl.ShowGotoLine();
	}

	private void OnShowWhitespace(object sender, EventArgs e)
	{
		m_syntaxBoxControl.ShowWhitespace = !m_syntaxBoxControl.ShowWhitespace;
		m_showWhitespaceToolStripMenuItem.Checked = !m_showWhitespaceToolStripMenuItem.Checked;
	}

	private void OnShowEOL(object sender, EventArgs e)
	{
		m_syntaxBoxControl.ShowEOLMarker = !m_syntaxBoxControl.ShowEOLMarker;
		m_showEOLMarkerToolStripMenuItem.Checked = !m_showEOLMarkerToolStripMenuItem.Checked;
	}

	private void SelectResourceNode(string resourceName, TreeNode parentNode)
	{
		foreach (TreeNode node in parentNode.Nodes)
		{
			SelectResourceNode(resourceName, node);
			if (node.Tag is IResource resource && string.Compare(resource.Name, resourceName, ignoreCase: true) == 0)
			{
				m_treeView.SelectedNode = node;
				break;
			}
		}
	}

	private static object GetAdjustedValue(object var, out Type type, out bool isReadOnly)
	{
		isReadOnly = false;
		object obj = "nil";
		if (var != null)
		{
			obj = var;
		}
		type = obj.GetType();
		if (type == typeof(LuaTable) || type == typeof(LuaFunction) || type.Name == "LuaCSFunction")
		{
			obj = LuaHelper.FormatLuaValue(obj);
			type = obj.GetType();
			isReadOnly = true;
		}
		return obj;
	}

	private static void RefreshVariableTable(VisualHint.SmartPropertyGrid.PropertyGrid grid, string category, IEnumerable<LuaVar> vars)
	{
		grid.SuspendLayout();
		grid.BeginUpdate();
		grid.Clear();
		grid.DisplayMode = VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized;
		PropertyEnumerator underCategory = grid.AppendRootCategory(1, category);
		int id = 2;
		foreach (LuaVar var in vars)
		{
			List<LuaTable> visited = new List<LuaTable>();
			object adjustedValue = GetAdjustedValue(var.Value, out var type, out var _);
			PropertyEnumerator propertyEnumerator = grid.AppendManagedProperty(underCategory, id++, var.Name, type, adjustedValue, string.Empty, new DefaultValueAttribute(adjustedValue));
			if (var.Value is LuaTable table)
			{
				propertyEnumerator.Property.Tag = adjustedValue;
				propertyEnumerator.Property.Feel = grid.GetRegisteredFeel("button");
				BuildNestedTableProperties(grid, propertyEnumerator, id, table, visited);
			}
		}
		grid.EndUpdate();
		grid.ResumeLayout();
	}

	private static void BuildNestedTableProperties(VisualHint.SmartPropertyGrid.PropertyGrid grid, PropertyEnumerator parentEnumerator, int id, LuaTable table, ICollection<LuaTable> visited)
	{
		if (visited.Contains(table))
		{
			return;
		}
		visited.Add(table);
		foreach (object key in table.Keys)
		{
			object adjustedValue = GetAdjustedValue(table[key], out var type, out var _);
			PropertyEnumerator propertyEnumerator = grid.AppendManagedProperty(parentEnumerator, id++, key.ToString(), type, adjustedValue, key.ToString(), new DefaultValueAttribute(adjustedValue));
			if (table[key] is LuaTable table2)
			{
				propertyEnumerator.Property.Feel = grid.GetRegisteredFeel("button");
				BuildNestedTableProperties(grid, propertyEnumerator, id, table2, visited);
			}
		}
	}

	private void RefreshCallStack()
	{
		CallStackEntry[] callStack = Debugger.GetCallStack();
		m_callStackListView.BeginUpdate();
		m_callStackListView.Items.Clear();
		CallStackEntry[] array = callStack;
		foreach (CallStackEntry callStackEntry in array)
		{
			ListViewItem listViewItem = new ListViewItem(callStackEntry.FunctionName);
			listViewItem.SubItems.Add(callStackEntry.FileName);
			listViewItem.SubItems.Add(callStackEntry.Line.ToString());
			m_callStackListView.Items.Add(listViewItem);
		}
		m_callStackListView.EndUpdate();
	}

	private void RefreshResourceTree()
	{
		m_treeView.BeginUpdate();
		m_treeView.Nodes.Clear();
		if (ActiveBundleSet == null)
		{
			return;
		}
		TreeNode treeNode = new TreeNode("Loaded Resources", 5, 5);
		treeNode.Name = treeNode.Text;
		TreeNode treeNode2 = new TreeNode("Views", 6, 6);
		treeNode2.Name = treeNode2.Text;
		TreeNode treeNode3 = new TreeNode("Scripts", 6, 6);
		treeNode3.Name = treeNode3.Text;
		treeNode.Nodes.Add(treeNode2);
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		foreach (IResource resource in ActiveBundleSet.GetResources("view", Filter))
		{
			TreeNode treeNode4 = new TreeNode(resource.Name, 4, 4)
			{
				Name = resource.Name,
				Tag = resource
			};
			TreeNode treeNode5 = new TreeNode("Actors", 6, 6);
			treeNode5.Name = treeNode5.Text;
			XmlNode xmlNode = ((XmlNode)resource.GetAspect("content").GetContent()).ChildNodes[0];
			string attributeValue = xmlNode.GetAttributeValue<string>("onEnter");
			if (!string.IsNullOrEmpty(attributeValue))
			{
				dictionary[attributeValue] = true;
				TreeNode treeNode6 = new TreeNode($"onEnter: {attributeValue}", 7, 7)
				{
					Tag = ActiveBundleSet.GetResource(attributeValue, Filter)
				};
				treeNode6.Name = treeNode6.Text;
				treeNode4.Nodes.Add(treeNode6);
			}
			string attributeValue2 = xmlNode.GetAttributeValue<string>("onLeave");
			if (!string.IsNullOrEmpty(attributeValue2))
			{
				dictionary[attributeValue2] = true;
				TreeNode treeNode7 = new TreeNode($"onLeave: {attributeValue2}", 7, 7)
				{
					Tag = ActiveBundleSet.GetResource(attributeValue2, Filter)
				};
				treeNode7.Name = treeNode7.Text;
				treeNode4.Nodes.Add(treeNode7);
			}
			XmlNodeList xmlNodeList = xmlNode.SelectNodes("actor");
			if (xmlNodeList != null)
			{
				foreach (XmlNode item in xmlNodeList)
				{
					string attributeValue3 = item.GetAttributeValue<string>("onHit");
					if (!string.IsNullOrEmpty(attributeValue3))
					{
						dictionary[attributeValue3] = true;
						string attributeValue4 = item.GetAttributeValue<string>("name");
						TreeNode treeNode8 = new TreeNode($"{attributeValue4}: {attributeValue3}", 7, 7)
						{
							Tag = ActiveBundleSet.GetResource(attributeValue3, Filter)
						};
						treeNode8.Name = treeNode8.Text;
						treeNode5.Nodes.Add(treeNode8);
					}
				}
			}
			if (treeNode5.Nodes.Count > 0)
			{
				treeNode4.Nodes.Add(treeNode5);
			}
			treeNode2.Nodes.Add(treeNode4);
		}
		treeNode.Nodes.Add(treeNode3);
		foreach (IResource resource2 in ActiveBundleSet.GetResources("script", Filter))
		{
			if (!dictionary.ContainsKey(resource2.Name))
			{
				TreeNode node2 = new TreeNode(resource2.Name, 7, 7)
				{
					Name = resource2.Name,
					Tag = resource2
				};
				treeNode3.Nodes.Add(node2);
			}
		}
		m_treeView.Nodes.Add(treeNode);
		m_treeView.EndUpdate();
	}

	private string GetScript(string resourceName)
	{
		if (ActiveBundleSet == null)
		{
			throw new ArgumentException("No active resource bundle set available.");
		}
		IResource resource = ActiveBundleSet.GetResource(resourceName, Filter);
		if (resource == null)
		{
			return null;
		}
		return resource.GetAspect("content").GetContent() as string;
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.DebuggerForm));
		Skybound.VisualTips.Rendering.VisualTipOfficeRenderer renderer = new Skybound.VisualTips.Rendering.VisualTipOfficeRenderer();
		this.m_statusStrip = new System.Windows.Forms.StatusStrip();
		this.m_statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
		this.m_menuStrip = new System.Windows.Forms.MenuStrip();
		this.m_fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_switchBundleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.m_closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_findAndReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_quickFindToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_quickReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_goToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.m_showWhitespaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_showEOLMarkerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_enableDebuggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.m_resumeExecutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_breakAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_stopDebugginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
		this.m_stepIntoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_stepOverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_stepOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
		this.m_toggleBreakpointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_deleteAllBreakpointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_breakpointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_mainSplitter = new System.Windows.Forms.Splitter();
		this.m_syntaxBoxControl = new Alsing.Windows.Forms.SyntaxBoxControl();
		this.m_syntaxDocument = new Alsing.SourceCode.SyntaxDocument(this.components);
		this.m_sourceSplitter = new System.Windows.Forms.Splitter();
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_enableDebuggingToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.m_resumeExecutionToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_breakAllToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_stopDebuggingToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.m_stepIntoToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_stepOverToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_stepOutToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_tabControl = new System.Windows.Forms.TabControl();
		this.m_localsTabPage = new System.Windows.Forms.TabPage();
		this.m_localsPropertyGrid = new Redbox.KioskEngine.IDE.DynamicPropertyGrid();
		this.m_globalsTabPage = new System.Windows.Forms.TabPage();
		this.m_globalsPropertyGrid = new Redbox.KioskEngine.IDE.DynamicPropertyGrid();
		this.m_callStackTabPage = new System.Windows.Forms.TabPage();
		this.m_callStackListView = new System.Windows.Forms.ListView();
		this.m_funcNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_scriptNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_scriptLineNoColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_immediateTabPage = new System.Windows.Forms.TabPage();
		this.m_shellControl = new Redbox.KioskEngine.IDE.ShellControl();
		this.m_imageList = new System.Windows.Forms.ImageList(this.components);
		this.m_treeView = new System.Windows.Forms.TreeView();
		this.m_visualTipProvider = new Skybound.VisualTips.VisualTipProvider(this.components);
		this.m_gutterImageList = new System.Windows.Forms.ImageList(this.components);
		this.m_statusStrip.SuspendLayout();
		this.m_menuStrip.SuspendLayout();
		this.m_toolStrip.SuspendLayout();
		this.m_tabControl.SuspendLayout();
		this.m_localsTabPage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_localsPropertyGrid).BeginInit();
		this.m_globalsTabPage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_globalsPropertyGrid).BeginInit();
		this.m_callStackTabPage.SuspendLayout();
		this.m_immediateTabPage.SuspendLayout();
		base.SuspendLayout();
		this.m_statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.m_statusLabel });
		this.m_statusStrip.Location = new System.Drawing.Point(0, 551);
		this.m_statusStrip.Name = "m_statusStrip";
		this.m_statusStrip.Size = new System.Drawing.Size(792, 22);
		this.m_statusStrip.TabIndex = 0;
		this.m_statusLabel.Name = "m_statusLabel";
		this.m_statusLabel.Size = new System.Drawing.Size(38, 17);
		this.m_statusLabel.Text = "Ready";
		this.m_menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.m_fileToolStripMenuItem, this.m_editToolStripMenuItem, this.m_debugToolStripMenuItem });
		this.m_menuStrip.Location = new System.Drawing.Point(0, 0);
		this.m_menuStrip.Name = "m_menuStrip";
		this.m_menuStrip.Size = new System.Drawing.Size(792, 24);
		this.m_menuStrip.TabIndex = 1;
		this.m_fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.m_switchBundleToolStripMenuItem, this.toolStripSeparator4, this.m_closeToolStripMenuItem });
		this.m_fileToolStripMenuItem.Name = "m_fileToolStripMenuItem";
		this.m_fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
		this.m_fileToolStripMenuItem.Text = "&File";
		this.m_switchBundleToolStripMenuItem.Name = "m_switchBundleToolStripMenuItem";
		this.m_switchBundleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.m_switchBundleToolStripMenuItem.Text = "Switch Bundle...";
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(149, 6);
		this.m_closeToolStripMenuItem.Name = "m_closeToolStripMenuItem";
		this.m_closeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.m_closeToolStripMenuItem.Text = "Close";
		this.m_editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.m_findAndReplaceToolStripMenuItem, this.m_goToToolStripMenuItem, this.toolStripSeparator5, this.m_showWhitespaceToolStripMenuItem, this.m_showEOLMarkerToolStripMenuItem });
		this.m_editToolStripMenuItem.Name = "m_editToolStripMenuItem";
		this.m_editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
		this.m_editToolStripMenuItem.Text = "&Edit";
		this.m_findAndReplaceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.m_quickFindToolStripMenuItem, this.m_quickReplaceToolStripMenuItem });
		this.m_findAndReplaceToolStripMenuItem.Name = "m_findAndReplaceToolStripMenuItem";
		this.m_findAndReplaceToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
		this.m_findAndReplaceToolStripMenuItem.Text = "&Find and Replace";
		this.m_quickFindToolStripMenuItem.Name = "m_quickFindToolStripMenuItem";
		this.m_quickFindToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F | System.Windows.Forms.Keys.Control;
		this.m_quickFindToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
		this.m_quickFindToolStripMenuItem.Text = "Quick Find...";
		this.m_quickFindToolStripMenuItem.Click += new System.EventHandler(OnFind);
		this.m_quickReplaceToolStripMenuItem.Name = "m_quickReplaceToolStripMenuItem";
		this.m_quickReplaceToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.H | System.Windows.Forms.Keys.Control;
		this.m_quickReplaceToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
		this.m_quickReplaceToolStripMenuItem.Text = "Quick Replace...";
		this.m_quickReplaceToolStripMenuItem.Click += new System.EventHandler(OnReplace);
		this.m_goToToolStripMenuItem.Name = "m_goToToolStripMenuItem";
		this.m_goToToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.G | System.Windows.Forms.Keys.Control;
		this.m_goToToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
		this.m_goToToolStripMenuItem.Text = "Go To...";
		this.m_goToToolStripMenuItem.Click += new System.EventHandler(OnGoTo);
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(156, 6);
		this.m_showWhitespaceToolStripMenuItem.Name = "m_showWhitespaceToolStripMenuItem";
		this.m_showWhitespaceToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
		this.m_showWhitespaceToolStripMenuItem.Text = "Show Whitespace";
		this.m_showWhitespaceToolStripMenuItem.Click += new System.EventHandler(OnShowWhitespace);
		this.m_showEOLMarkerToolStripMenuItem.Name = "m_showEOLMarkerToolStripMenuItem";
		this.m_showEOLMarkerToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
		this.m_showEOLMarkerToolStripMenuItem.Text = "Show EOL Marker";
		this.m_showEOLMarkerToolStripMenuItem.Click += new System.EventHandler(OnShowEOL);
		this.m_debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[13]
		{
			this.m_enableDebuggingToolStripMenuItem, this.toolStripSeparator2, this.m_resumeExecutionToolStripMenuItem, this.m_breakAllToolStripMenuItem, this.m_stopDebugginToolStripMenuItem, this.toolStripMenuItem1, this.m_stepIntoToolStripMenuItem, this.m_stepOverToolStripMenuItem, this.m_stepOutToolStripMenuItem, this.toolStripMenuItem2,
			this.m_toggleBreakpointToolStripMenuItem, this.m_deleteAllBreakpointToolStripMenuItem, this.m_breakpointsToolStripMenuItem
		});
		this.m_debugToolStripMenuItem.Name = "m_debugToolStripMenuItem";
		this.m_debugToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
		this.m_debugToolStripMenuItem.Text = "&Debug";
		this.m_enableDebuggingToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.Debugger;
		this.m_enableDebuggingToolStripMenuItem.Name = "m_enableDebuggingToolStripMenuItem";
		this.m_enableDebuggingToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
		this.m_enableDebuggingToolStripMenuItem.Text = "Enable Debugging";
		this.m_enableDebuggingToolStripMenuItem.Click += new System.EventHandler(OnToggleDebugging);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(176, 6);
		this.m_resumeExecutionToolStripMenuItem.Enabled = false;
		this.m_resumeExecutionToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugPlay;
		this.m_resumeExecutionToolStripMenuItem.Name = "m_resumeExecutionToolStripMenuItem";
		this.m_resumeExecutionToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
		this.m_resumeExecutionToolStripMenuItem.Text = "Resume Execution";
		this.m_resumeExecutionToolStripMenuItem.Click += new System.EventHandler(OnResume);
		this.m_breakAllToolStripMenuItem.Enabled = false;
		this.m_breakAllToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugPause;
		this.m_breakAllToolStripMenuItem.Name = "m_breakAllToolStripMenuItem";
		this.m_breakAllToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
		this.m_breakAllToolStripMenuItem.Text = "Break All";
		this.m_stopDebugginToolStripMenuItem.Enabled = false;
		this.m_stopDebugginToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStop;
		this.m_stopDebugginToolStripMenuItem.Name = "m_stopDebugginToolStripMenuItem";
		this.m_stopDebugginToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
		this.m_stopDebugginToolStripMenuItem.Text = "Stop Debugging";
		this.toolStripMenuItem1.Name = "toolStripMenuItem1";
		this.toolStripMenuItem1.Size = new System.Drawing.Size(176, 6);
		this.m_stepIntoToolStripMenuItem.Enabled = false;
		this.m_stepIntoToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepInto;
		this.m_stepIntoToolStripMenuItem.Name = "m_stepIntoToolStripMenuItem";
		this.m_stepIntoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F11;
		this.m_stepIntoToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
		this.m_stepIntoToolStripMenuItem.Text = "Step Into";
		this.m_stepIntoToolStripMenuItem.Click += new System.EventHandler(OnStepInto);
		this.m_stepOverToolStripMenuItem.Enabled = false;
		this.m_stepOverToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepOver;
		this.m_stepOverToolStripMenuItem.Name = "m_stepOverToolStripMenuItem";
		this.m_stepOverToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F10;
		this.m_stepOverToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
		this.m_stepOverToolStripMenuItem.Text = "Step Over";
		this.m_stepOverToolStripMenuItem.Click += new System.EventHandler(OnStepOver);
		this.m_stepOutToolStripMenuItem.Enabled = false;
		this.m_stepOutToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepOut;
		this.m_stepOutToolStripMenuItem.Name = "m_stepOutToolStripMenuItem";
		this.m_stepOutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F11 | System.Windows.Forms.Keys.Shift;
		this.m_stepOutToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
		this.m_stepOutToolStripMenuItem.Text = "Step Out";
		this.m_stepOutToolStripMenuItem.Click += new System.EventHandler(OnStepOut);
		this.toolStripMenuItem2.Name = "toolStripMenuItem2";
		this.toolStripMenuItem2.Size = new System.Drawing.Size(176, 6);
		this.m_toggleBreakpointToolStripMenuItem.Enabled = false;
		this.m_toggleBreakpointToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugBreakpoint;
		this.m_toggleBreakpointToolStripMenuItem.Name = "m_toggleBreakpointToolStripMenuItem";
		this.m_toggleBreakpointToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
		this.m_toggleBreakpointToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
		this.m_toggleBreakpointToolStripMenuItem.Text = "Toggle Breakpoint";
		this.m_toggleBreakpointToolStripMenuItem.Click += new System.EventHandler(OnToggleBreakpoint);
		this.m_deleteAllBreakpointToolStripMenuItem.Enabled = false;
		this.m_deleteAllBreakpointToolStripMenuItem.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugBreakpointClear;
		this.m_deleteAllBreakpointToolStripMenuItem.Name = "m_deleteAllBreakpointToolStripMenuItem";
		this.m_deleteAllBreakpointToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
		this.m_deleteAllBreakpointToolStripMenuItem.Text = "Delete All Breakpoints";
		this.m_deleteAllBreakpointToolStripMenuItem.Click += new System.EventHandler(OnDeleteAllBreakpoints);
		this.m_breakpointsToolStripMenuItem.Name = "m_breakpointsToolStripMenuItem";
		this.m_breakpointsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
		this.m_breakpointsToolStripMenuItem.Text = "Breakpoints";
		this.m_breakpointsToolStripMenuItem.Visible = false;
		this.m_mainSplitter.Location = new System.Drawing.Point(259, 49);
		this.m_mainSplitter.Name = "m_mainSplitter";
		this.m_mainSplitter.Size = new System.Drawing.Size(3, 502);
		this.m_mainSplitter.TabIndex = 3;
		this.m_mainSplitter.TabStop = false;
		this.m_syntaxBoxControl.ActiveView = Alsing.Windows.Forms.ActiveView.BottomRight;
		this.m_syntaxBoxControl.AutoListPosition = null;
		this.m_syntaxBoxControl.AutoListSelectedText = "a123";
		this.m_syntaxBoxControl.AutoListVisible = false;
		this.m_syntaxBoxControl.BackColor = System.Drawing.Color.White;
		this.m_syntaxBoxControl.BorderStyle = Alsing.Windows.Forms.BorderStyle.None;
		this.m_syntaxBoxControl.CopyAsRTF = false;
		this.m_syntaxBoxControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_syntaxBoxControl.Document = this.m_syntaxDocument;
		this.m_syntaxBoxControl.FontName = "Courier new";
		this.m_syntaxBoxControl.HighLightActiveLine = true;
		this.m_syntaxBoxControl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.m_syntaxBoxControl.InfoTipCount = 1;
		this.m_syntaxBoxControl.InfoTipPosition = null;
		this.m_syntaxBoxControl.InfoTipSelectedIndex = 1;
		this.m_syntaxBoxControl.InfoTipVisible = false;
		this.m_syntaxBoxControl.Location = new System.Drawing.Point(262, 49);
		this.m_syntaxBoxControl.LockCursorUpdate = false;
		this.m_syntaxBoxControl.Name = "m_syntaxBoxControl";
		this.m_syntaxBoxControl.ReadOnly = true;
		this.m_syntaxBoxControl.ShowScopeIndicator = false;
		this.m_syntaxBoxControl.Size = new System.Drawing.Size(530, 328);
		this.m_syntaxBoxControl.SmoothScroll = false;
		this.m_syntaxBoxControl.SplitviewH = -4;
		this.m_syntaxBoxControl.SplitviewV = -4;
		this.m_syntaxBoxControl.TabGuideColor = System.Drawing.Color.FromArgb(222, 219, 214);
		this.m_syntaxBoxControl.TabIndex = 4;
		this.m_syntaxBoxControl.WhitespaceColor = System.Drawing.SystemColors.ControlDark;
		this.m_syntaxBoxControl.CaretChange += new System.EventHandler(OnCaretChange);
		this.m_syntaxDocument.Lines = new string[1] { "" };
		this.m_syntaxDocument.MaxUndoBufferSize = 1000;
		this.m_syntaxDocument.Modified = false;
		this.m_syntaxDocument.UndoStep = 0;
		this.m_syntaxDocument.BreakPointAdded += new Alsing.SourceCode.RowEventHandler(OnBreakpointAdded);
		this.m_syntaxDocument.BreakPointRemoved += new Alsing.SourceCode.RowEventHandler(OnBreakpointRemoved);
		this.m_sourceSplitter.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_sourceSplitter.Location = new System.Drawing.Point(262, 377);
		this.m_sourceSplitter.Name = "m_sourceSplitter";
		this.m_sourceSplitter.Size = new System.Drawing.Size(530, 3);
		this.m_sourceSplitter.TabIndex = 5;
		this.m_sourceSplitter.TabStop = false;
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.m_enableDebuggingToolStripButton, this.toolStripSeparator3, this.m_resumeExecutionToolStripButton, this.m_breakAllToolStripButton, this.m_stopDebuggingToolStripButton, this.toolStripSeparator1, this.m_stepIntoToolStripButton, this.m_stepOverToolStripButton, this.m_stepOutToolStripButton });
		this.m_toolStrip.Location = new System.Drawing.Point(0, 24);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(792, 25);
		this.m_toolStrip.TabIndex = 6;
		this.m_enableDebuggingToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_enableDebuggingToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Debugger;
		this.m_enableDebuggingToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_enableDebuggingToolStripButton.Name = "m_enableDebuggingToolStripButton";
		this.m_enableDebuggingToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_enableDebuggingToolStripButton.Text = "Enable Debugging";
		this.m_enableDebuggingToolStripButton.Click += new System.EventHandler(OnToggleDebugging);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
		this.m_resumeExecutionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_resumeExecutionToolStripButton.Enabled = false;
		this.m_resumeExecutionToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugPlay;
		this.m_resumeExecutionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_resumeExecutionToolStripButton.Name = "m_resumeExecutionToolStripButton";
		this.m_resumeExecutionToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_resumeExecutionToolStripButton.Text = "ResumeExecution";
		this.m_resumeExecutionToolStripButton.ToolTipText = "Resume Execution";
		this.m_resumeExecutionToolStripButton.Click += new System.EventHandler(OnResume);
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
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.m_stepIntoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_stepIntoToolStripButton.Enabled = false;
		this.m_stepIntoToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepInto;
		this.m_stepIntoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_stepIntoToolStripButton.Name = "m_stepIntoToolStripButton";
		this.m_stepIntoToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_stepIntoToolStripButton.Text = "toolStripButton4";
		this.m_stepIntoToolStripButton.ToolTipText = "Step Into";
		this.m_stepIntoToolStripButton.Click += new System.EventHandler(OnStepInto);
		this.m_stepOverToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_stepOverToolStripButton.Enabled = false;
		this.m_stepOverToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepOver;
		this.m_stepOverToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_stepOverToolStripButton.Name = "m_stepOverToolStripButton";
		this.m_stepOverToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_stepOverToolStripButton.Text = "toolStripButton5";
		this.m_stepOverToolStripButton.ToolTipText = "Step Over";
		this.m_stepOverToolStripButton.Click += new System.EventHandler(OnStepOver);
		this.m_stepOutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_stepOutToolStripButton.Enabled = false;
		this.m_stepOutToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStepOut;
		this.m_stepOutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_stepOutToolStripButton.Name = "m_stepOutToolStripButton";
		this.m_stepOutToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_stepOutToolStripButton.Text = "Step Out";
		this.m_stepOutToolStripButton.Click += new System.EventHandler(OnStepOut);
		this.m_tabControl.Alignment = System.Windows.Forms.TabAlignment.Bottom;
		this.m_tabControl.Controls.Add(this.m_localsTabPage);
		this.m_tabControl.Controls.Add(this.m_globalsTabPage);
		this.m_tabControl.Controls.Add(this.m_callStackTabPage);
		this.m_tabControl.Controls.Add(this.m_immediateTabPage);
		this.m_tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_tabControl.ImageList = this.m_imageList;
		this.m_tabControl.Location = new System.Drawing.Point(262, 380);
		this.m_tabControl.Name = "m_tabControl";
		this.m_tabControl.SelectedIndex = 0;
		this.m_tabControl.Size = new System.Drawing.Size(530, 171);
		this.m_tabControl.TabIndex = 7;
		this.m_tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(OnTabSelected);
		this.m_localsTabPage.Controls.Add(this.m_localsPropertyGrid);
		this.m_localsTabPage.ImageIndex = 1;
		this.m_localsTabPage.Location = new System.Drawing.Point(4, 4);
		this.m_localsTabPage.Name = "m_localsTabPage";
		this.m_localsTabPage.Padding = new System.Windows.Forms.Padding(3);
		this.m_localsTabPage.Size = new System.Drawing.Size(522, 144);
		this.m_localsTabPage.TabIndex = 0;
		this.m_localsTabPage.Text = "Locals";
		this.m_localsTabPage.UseVisualStyleBackColor = true;
		this.m_localsPropertyGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_localsPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_localsPropertyGrid.Location = new System.Drawing.Point(3, 3);
		this.m_localsPropertyGrid.Name = "m_localsPropertyGrid";
		this.m_localsPropertyGrid.PropertyLabelBackColor = System.Drawing.SystemColors.Window;
		this.m_localsPropertyGrid.PropertyValueBackColor = System.Drawing.SystemColors.Window;
		this.m_localsPropertyGrid.ShowAdditionalIndentation = true;
		this.m_localsPropertyGrid.ShowDefaultValues = true;
		this.m_localsPropertyGrid.Size = new System.Drawing.Size(516, 138);
		this.m_localsPropertyGrid.TabIndex = 0;
		this.m_localsPropertyGrid.ToolbarVisibility = true;
		this.m_localsPropertyGrid.ToolTipMode = VisualHint.SmartPropertyGrid.PropertyGrid.ToolTipModes.ToolTipsOnValuesAndLabels;
		this.m_globalsTabPage.Controls.Add(this.m_globalsPropertyGrid);
		this.m_globalsTabPage.ImageIndex = 2;
		this.m_globalsTabPage.Location = new System.Drawing.Point(4, 4);
		this.m_globalsTabPage.Name = "m_globalsTabPage";
		this.m_globalsTabPage.Padding = new System.Windows.Forms.Padding(3);
		this.m_globalsTabPage.Size = new System.Drawing.Size(522, 144);
		this.m_globalsTabPage.TabIndex = 2;
		this.m_globalsTabPage.Text = "Globals";
		this.m_globalsTabPage.UseVisualStyleBackColor = true;
		this.m_globalsPropertyGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_globalsPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_globalsPropertyGrid.Location = new System.Drawing.Point(3, 3);
		this.m_globalsPropertyGrid.Name = "m_globalsPropertyGrid";
		this.m_globalsPropertyGrid.PropertyLabelBackColor = System.Drawing.SystemColors.Window;
		this.m_globalsPropertyGrid.PropertyValueBackColor = System.Drawing.SystemColors.Window;
		this.m_globalsPropertyGrid.ShowAdditionalIndentation = true;
		this.m_globalsPropertyGrid.ShowDefaultValues = true;
		this.m_globalsPropertyGrid.Size = new System.Drawing.Size(516, 138);
		this.m_globalsPropertyGrid.TabIndex = 1;
		this.m_globalsPropertyGrid.ToolbarVisibility = true;
		this.m_globalsPropertyGrid.ToolTipMode = VisualHint.SmartPropertyGrid.PropertyGrid.ToolTipModes.ToolTipsOnValuesAndLabels;
		this.m_callStackTabPage.Controls.Add(this.m_callStackListView);
		this.m_callStackTabPage.ImageIndex = 3;
		this.m_callStackTabPage.Location = new System.Drawing.Point(4, 4);
		this.m_callStackTabPage.Name = "m_callStackTabPage";
		this.m_callStackTabPage.Padding = new System.Windows.Forms.Padding(3);
		this.m_callStackTabPage.Size = new System.Drawing.Size(522, 144);
		this.m_callStackTabPage.TabIndex = 1;
		this.m_callStackTabPage.Text = "Call Stack";
		this.m_callStackTabPage.UseVisualStyleBackColor = true;
		this.m_callStackListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.m_funcNameColumnHeader, this.m_scriptNameColumnHeader, this.m_scriptLineNoColumnHeader });
		this.m_callStackListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_callStackListView.FullRowSelect = true;
		this.m_callStackListView.GridLines = true;
		this.m_callStackListView.Location = new System.Drawing.Point(3, 3);
		this.m_callStackListView.Name = "m_callStackListView";
		this.m_callStackListView.Size = new System.Drawing.Size(516, 138);
		this.m_callStackListView.TabIndex = 0;
		this.m_callStackListView.UseCompatibleStateImageBehavior = false;
		this.m_callStackListView.View = System.Windows.Forms.View.Details;
		this.m_funcNameColumnHeader.Text = "Function Name";
		this.m_funcNameColumnHeader.Width = 150;
		this.m_scriptNameColumnHeader.Text = "Script";
		this.m_scriptNameColumnHeader.Width = 150;
		this.m_scriptLineNoColumnHeader.Text = "Line #";
		this.m_scriptLineNoColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_immediateTabPage.Controls.Add(this.m_shellControl);
		this.m_immediateTabPage.ImageIndex = 0;
		this.m_immediateTabPage.Location = new System.Drawing.Point(4, 4);
		this.m_immediateTabPage.Name = "m_immediateTabPage";
		this.m_immediateTabPage.Padding = new System.Windows.Forms.Padding(3);
		this.m_immediateTabPage.Size = new System.Drawing.Size(522, 144);
		this.m_immediateTabPage.TabIndex = 3;
		this.m_immediateTabPage.Text = "Immediate";
		this.m_immediateTabPage.UseVisualStyleBackColor = true;
		this.m_shellControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_shellControl.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_shellControl.Location = new System.Drawing.Point(3, 3);
		this.m_shellControl.Name = "m_shellControl";
		this.m_shellControl.Prompt = "> ";
		this.m_shellControl.ShellTextBackColor = System.Drawing.SystemColors.Window;
		this.m_shellControl.ShellTextFont = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_shellControl.ShellTextForeColor = System.Drawing.SystemColors.WindowText;
		this.m_shellControl.Size = new System.Drawing.Size(516, 138);
		this.m_shellControl.TabIndex = 0;
		this.m_shellControl.CommandEntered += new Redbox.KioskEngine.IDE.EventCommandEntered(OnCommandEntered);
		this.m_imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("m_imageList.ImageStream");
		this.m_imageList.TransparentColor = System.Drawing.Color.Transparent;
		this.m_imageList.Images.SetKeyName(0, "debug-immediate");
		this.m_imageList.Images.SetKeyName(1, "debug-watch");
		this.m_imageList.Images.SetKeyName(2, "symbols");
		this.m_imageList.Images.SetKeyName(3, "debug-callstack");
		this.m_imageList.Images.SetKeyName(4, "view-resource");
		this.m_imageList.Images.SetKeyName(5, "bundle");
		this.m_imageList.Images.SetKeyName(6, "folder");
		this.m_imageList.Images.SetKeyName(7, "script");
		this.m_treeView.Dock = System.Windows.Forms.DockStyle.Left;
		this.m_treeView.FullRowSelect = true;
		this.m_treeView.HideSelection = false;
		this.m_treeView.ImageIndex = 0;
		this.m_treeView.ImageList = this.m_imageList;
		this.m_treeView.ItemHeight = 19;
		this.m_treeView.Location = new System.Drawing.Point(0, 49);
		this.m_treeView.Name = "m_treeView";
		this.m_treeView.PathSeparator = "/";
		this.m_treeView.SelectedImageIndex = 0;
		this.m_treeView.Size = new System.Drawing.Size(259, 502);
		this.m_treeView.TabIndex = 8;
		this.m_treeView.DoubleClick += new System.EventHandler(OnTreeDoubleClick);
		this.m_treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(OnAfterSelect);
		this.m_visualTipProvider.Animation = Skybound.VisualTips.VisualTipAnimation.Enabled;
		this.m_visualTipProvider.DisplayAtMousePosition = false;
		this.m_visualTipProvider.DisplayMode = Skybound.VisualTips.VisualTipDisplayMode.Manual;
		this.m_visualTipProvider.Renderer = renderer;
		this.m_gutterImageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("m_gutterImageList.ImageStream");
		this.m_gutterImageList.TransparentColor = System.Drawing.Color.Transparent;
		this.m_gutterImageList.Images.SetKeyName(0, "breakpoint-disabled.png");
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(792, 573);
		base.Controls.Add(this.m_tabControl);
		base.Controls.Add(this.m_sourceSplitter);
		base.Controls.Add(this.m_syntaxBoxControl);
		base.Controls.Add(this.m_mainSplitter);
		base.Controls.Add(this.m_treeView);
		base.Controls.Add(this.m_statusStrip);
		base.Controls.Add(this.m_toolStrip);
		base.Controls.Add(this.m_menuStrip);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MainMenuStrip = this.m_menuStrip;
		this.MinimumSize = new System.Drawing.Size(640, 480);
		base.Name = "DebuggerForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Kiosk Engine Debugger";
		base.Activated += new System.EventHandler(OnActivated);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(OnFormClosing);
		this.m_statusStrip.ResumeLayout(false);
		this.m_statusStrip.PerformLayout();
		this.m_menuStrip.ResumeLayout(false);
		this.m_menuStrip.PerformLayout();
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		this.m_tabControl.ResumeLayout(false);
		this.m_localsTabPage.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.m_localsPropertyGrid).EndInit();
		this.m_globalsTabPage.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.m_globalsPropertyGrid).EndInit();
		this.m_callStackTabPage.ResumeLayout(false);
		this.m_immediateTabPage.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
