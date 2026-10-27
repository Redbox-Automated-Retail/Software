using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console;

public class LeftPanel : UserControl
{
	private TabPage m_propertyTabPage;

	private TabPage m_immediateTabPage;

	private IContainer components;

	private TabControl m_leftTabControl;

	private Panel m_sensorPanel;

	public static LeftPanel Instance => Singleton<LeftPanel>.Instance;

	public void AddTab(LeftPanelTab tab)
	{
		switch (tab)
		{
		case LeftPanelTab.Properties:
			if (m_propertyTabPage == null)
			{
				m_propertyTabPage = new TabPage();
				m_propertyTabPage.BackColor = Color.White;
				m_propertyTabPage.Text = "Properties";
				m_propertyTabPage.Controls.Add(new ConfigPropertyList());
			}
			m_leftTabControl.TabPages.Add(m_propertyTabPage);
			break;
		case LeftPanelTab.Immediate:
			if (m_immediateTabPage == null)
			{
				m_immediateTabPage = new TabPage();
				m_immediateTabPage.Text = "Immediate";
				m_immediateTabPage.Controls.Add(ImmediateWindow.Instance);
				m_immediateTabPage.Enter += delegate
				{
					m_immediateTabPage.Controls[0].Select();
				};
			}
			m_leftTabControl.TabPages.Add(m_immediateTabPage);
			break;
		}
	}

	public void RemoveTab(LeftPanelTab tab)
	{
		switch (tab)
		{
		case LeftPanelTab.Properties:
			if (m_propertyTabPage != null)
			{
				m_leftTabControl.TabPages.Remove(m_propertyTabPage);
			}
			break;
		case LeftPanelTab.Immediate:
			if (m_immediateTabPage != null)
			{
				m_leftTabControl.TabPages.Remove(m_immediateTabPage);
			}
			break;
		}
	}

	public void ReloadPropertiesConfiguration()
	{
		if (m_propertyTabPage != null && m_propertyTabPage.Controls.Count > 0 && m_propertyTabPage.Controls["ConfigPropertyList"] is ConfigPropertyList configPropertyList)
		{
			configPropertyList.LoadConfiguration();
		}
	}

	private LeftPanel()
	{
		InitializeComponent();
		Dock = DockStyle.Fill;
		base.Visible = true;
		base.Enabled = false;
		AddTab(LeftPanelTab.Properties);
		m_sensorPanel.Controls.Add(SensorView.Instance);
		m_sensorPanel.Visible = true;
		ProfileManager.Instance.Connected += delegate
		{
			base.Enabled = true;
			Refresh();
		};
		ProfileManager.Instance.Disconnected += delegate
		{
			base.Enabled = false;
		};
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
		this.m_leftTabControl = new System.Windows.Forms.TabControl();
		this.m_sensorPanel = new System.Windows.Forms.Panel();
		base.SuspendLayout();
		this.m_leftTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_leftTabControl.ItemSize = new System.Drawing.Size(80, 30);
		this.m_leftTabControl.Location = new System.Drawing.Point(5, 2);
		this.m_leftTabControl.Name = "m_leftTabControl";
		this.m_leftTabControl.SelectedIndex = 0;
		this.m_leftTabControl.Size = new System.Drawing.Size(262, 425);
		this.m_leftTabControl.TabIndex = 2;
		this.m_sensorPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_sensorPanel.Location = new System.Drawing.Point(5, 433);
		this.m_sensorPanel.Name = "m_sensorPanel";
		this.m_sensorPanel.Size = new System.Drawing.Size(262, 150);
		this.m_sensorPanel.TabIndex = 3;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.Controls.Add(this.m_sensorPanel);
		base.Controls.Add(this.m_leftTabControl);
		base.Margin = new System.Windows.Forms.Padding(5);
		base.Name = "LeftPanel";
		base.Padding = new System.Windows.Forms.Padding(5);
		base.Size = new System.Drawing.Size(275, 600);
		base.ResumeLayout(false);
	}
}
