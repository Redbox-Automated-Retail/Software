using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd;

public class Host : Form
{
	private readonly Dictionary<string, EventPanel> m_eventDict = new Dictionary<string, EventPanel>();

	private IContainer components;

	private FlowLayoutPanel m_events;

	private OpenFileDialog m_fileDialog;

	public Host()
	{
		InitializeComponent();
	}

	public FileInfo ShowFileDialog()
	{
		if (m_fileDialog.ShowDialog() != DialogResult.OK)
		{
			return null;
		}
		return new FileInfo(m_fileDialog.FileName);
	}

	public void AddEvent(string name, string description)
	{
		EventPanel value = new EventPanel(name, description);
		m_events.Controls.Add(value);
		m_eventDict.Add(name, value);
	}

	public void EventStart(string name)
	{
		m_eventDict[name].Start();
		m_eventDict[name].Focus();
	}

	public void EventComplete(string name)
	{
		m_eventDict[name].Complete();
		m_eventDict[name].Focus();
	}

	public void EventError(string name, Error error)
	{
		m_eventDict[name].Error(error);
		m_eventDict[name].Focus();
	}

	public void Pending(string name)
	{
		m_eventDict[name].Pending();
		m_eventDict[name].Focus();
	}

	public void ErrorAll()
	{
		foreach (EventPanel value in m_eventDict.Values)
		{
			value.Error(Error.NewError("E999", "A unrecoverable error occurred.", "Please contact support staff."));
		}
	}

	public void ShowFinish()
	{
		Application.Exit();
	}

	private void m_exit_Click(object sender, EventArgs e)
	{
		Application.Exit();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.UpdateManager.StoreInstallerFrontEnd.Host));
		this.m_events = new System.Windows.Forms.FlowLayoutPanel();
		this.m_fileDialog = new System.Windows.Forms.OpenFileDialog();
		base.SuspendLayout();
		this.m_events.AutoScroll = true;
		this.m_events.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_events.Location = new System.Drawing.Point(0, 0);
		this.m_events.Name = "m_events";
		this.m_events.Size = new System.Drawing.Size(538, 577);
		this.m_events.TabIndex = 0;
		this.m_fileDialog.AddExtension = false;
		this.m_fileDialog.FileName = "HAL.xml";
		this.m_fileDialog.InitialDirectory = "C:\\";
		this.m_fileDialog.SupportMultiDottedExtensions = true;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		base.ClientSize = new System.Drawing.Size(538, 577);
		base.Controls.Add(this.m_events);
		this.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.Name = "Host";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Store Installer";
		base.ResumeLayout(false);
	}
}
