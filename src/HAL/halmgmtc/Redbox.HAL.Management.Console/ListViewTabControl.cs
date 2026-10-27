using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console;

public class ListViewTabControl : UserControl
{
	private readonly AtomicFlag Flag = new AtomicFlag();

	private TabControl m_listViews;

	private static ListViewTabControl m_instance;

	private IContainer components;

	public static ListViewTabControl Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new ListViewTabControl();
			}
			return m_instance;
		}
	}

	public bool Add(ListViewNames name, UserControl control, TabPageExtension.RefreshData data, bool enabled)
	{
		if (Find(name) != null)
		{
			return false;
		}
		TabPageExtension tabPageExtension = new TabPageExtension();
		tabPageExtension.ListView = control;
		control.Dock = DockStyle.Fill;
		control.Enabled = enabled;
		tabPageExtension.Name = name.ToString();
		tabPageExtension.Text = name.ToString();
		tabPageExtension.Tag = name;
		tabPageExtension.refreshData = data;
		control.Parent = tabPageExtension;
		tabPageExtension.Controls.Add(control);
		m_listViews.TabPages.Add(tabPageExtension);
		control.Select();
		return true;
	}

	public TabPageExtension Find(ListViewNames name)
	{
		foreach (TabPageExtension tabPage in m_listViews.TabPages)
		{
			if ((ListViewNames)tabPage.Tag == name)
			{
				return tabPage;
			}
		}
		return null;
	}

	public bool Remove(ListViewNames name)
	{
		if (name == ListViewNames.Errors)
		{
			ErrorListView.Instance.KeepOpenOnSuccessfulInstruction = false;
		}
		TabPageExtension tabPageExtension = Find(name);
		if (tabPageExtension != null)
		{
			m_listViews.TabPages.Remove(tabPageExtension);
			return true;
		}
		return false;
	}

	public bool Remove(TabPageExtension tab)
	{
		if (tab == null)
		{
			return false;
		}
		if (tab.Name == ListViewNames.Errors.ToString())
		{
			ErrorListView.Instance.KeepOpenOnSuccessfulInstruction = false;
		}
		m_listViews.TabPages.Remove(tab);
		return true;
	}

	public void RefreshTab()
	{
		if (!ProfileManager.Instance.IsConnected)
		{
			return;
		}
		TabPageExtension tabPageExtension = (TabPageExtension)m_listViews.SelectedTab;
		if (tabPageExtension == null || tabPageExtension.refreshData == null)
		{
			return;
		}
		if (!Flag.Set())
		{
			LogHelper.Instance.Log("Attempting to Refresh Listview but it was already locked.  Dropping Request.", LogEntryType.Error);
			return;
		}
		try
		{
			tabPageExtension.SuspendLayout();
			tabPageExtension.refreshData();
			tabPageExtension.ResumeLayout();
		}
		finally
		{
			Flag.Clear();
		}
	}

	public TabPageExtension SelectedTab()
	{
		return (TabPageExtension)m_listViews.SelectedTab;
	}

	public bool SetFocus(ListViewNames name)
	{
		TabPageExtension tabPageExtension = Find(name);
		if (tabPageExtension == null)
		{
			return false;
		}
		if (tabPageExtension == SelectedTab())
		{
			return true;
		}
		m_listViews.SelectedTab = tabPageExtension;
		return true;
	}

	private ListViewTabControl()
	{
		InitializeComponent();
		Dock = DockStyle.Fill;
		m_listViews = new TabControl();
		m_listViews.SizeMode = TabSizeMode.Fixed;
		m_listViews.ItemSize = new Size
		{
			Height = 30,
			Width = 80
		};
		m_listViews.Dock = DockStyle.Fill;
		Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
		base.Controls.Add(m_listViews);
		base.Enabled = true;
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
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	}
}
