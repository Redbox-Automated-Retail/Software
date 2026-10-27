using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

public class JobToolStrip : ToolStrip, IJobStrip
{
	private Func<List<HardwareJobWrapper>> m_updateJobSource;

	private IContainer components;

	public JobToolStrip(Func<List<HardwareJobWrapper>> UpdateJobSource, UpdateSource source, bool trashLocked)
	{
		m_updateJobSource = UpdateJobSource;
		InitializeComponent();
		new ToolStripLabel();
		base.Tag = source;
		Items.AddRange(new ToolStripItem[12]
		{
			JobItem.Instance.MakeJobToolStripButton(JobItems.Garbage, locked: false),
			JobItem.Instance.MakeJobToolStripButton(JobItems.ViewErrors, locked: false),
			new ToolStripSeparator(),
			JobItem.Instance.MakeJobToolStripButton(JobItems.Connect, locked: false),
			JobItem.Instance.MakeJobToolStripButton(JobItems.Disconnect, locked: false),
			new ToolStripSeparator(),
			JobItem.Instance.MakeJobToolStripButton(JobItems.SetLabel, locked: false),
			JobItem.Instance.MakeJobToolStripButton(JobItems.SetStartTime, locked: false),
			new ToolStripSeparator(),
			JobItem.Instance.MakeJobToolStripButton(JobItems.Resume, locked: false),
			JobItem.Instance.MakeJobToolStripButton(JobItems.Suspend, locked: false),
			JobItem.Instance.MakeJobToolStripButton(JobItems.Stop, locked: false)
		});
		if (trashLocked)
		{
			Items.Add(JobItem.Instance.MakeJobToolStripButton(JobItems.TrashLocked, trashLocked));
			ProfileManager.Instance.Connected += delegate
			{
				JobHelper.UpdateButtons(Items, UpdateSource());
			};
			ProfileManager.Instance.Disconnected += delegate
			{
				JobHelper.UpdateButtons(Items, UpdateSource());
			};
			EnvironmentHelper.LockStatusChanged += delegate
			{
				JobHelper.UpdateButtons(Items, UpdateSource());
			};
		}
		else
		{
			Items.Add(JobItem.Instance.MakeJobToolStripButton(JobItems.Trash, trashLocked));
		}
	}

	public List<HardwareJobWrapper> UpdateSource()
	{
		return m_updateJobSource();
	}

	public void UpdateButtons(object sender, EventArgs e)
	{
		JobHelper.UpdateButtons(Items, m_updateJobSource());
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
	}
}
