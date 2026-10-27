using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

public class JobDropDownMenu : ToolStripDropDownMenu, IJobStrip
{
	private Func<List<HardwareJobWrapper>> UpdateJobSource;

	private IContainer components;

	public JobDropDownMenu(Func<List<HardwareJobWrapper>> UpdateJobSource, UpdateSource source, bool trashLocked)
	{
		this.UpdateJobSource = UpdateJobSource;
		base.Name = "m_jobMenuItem";
		Text = "&Job";
		base.Tag = source;
		InitializeComponent();
		Items.AddRange(new ToolStripItem[12]
		{
			JobItem.Instance.MakeJobMenuItem(JobItems.Garbage, locked: false),
			JobItem.Instance.MakeJobMenuItem(JobItems.ViewErrors, locked: false),
			new ToolStripSeparator(),
			JobItem.Instance.MakeJobMenuItem(JobItems.Connect, locked: false),
			JobItem.Instance.MakeJobMenuItem(JobItems.Disconnect, locked: false),
			new ToolStripSeparator(),
			JobItem.Instance.MakeJobMenuItem(JobItems.SetLabel, locked: false),
			JobItem.Instance.MakeJobMenuItem(JobItems.SetStartTime, locked: false),
			new ToolStripSeparator(),
			JobItem.Instance.MakeJobMenuItem(JobItems.Resume, locked: false),
			JobItem.Instance.MakeJobMenuItem(JobItems.Suspend, locked: false),
			JobItem.Instance.MakeJobMenuItem(JobItems.Stop, locked: false)
		});
		if (trashLocked)
		{
			Items.Add(JobItem.Instance.MakeJobMenuItem(JobItems.TrashLocked, locked: true));
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
			Items.Add(JobItem.Instance.MakeJobMenuItem(JobItems.Trash, locked: false));
		}
	}

	public List<HardwareJobWrapper> UpdateSource()
	{
		return UpdateJobSource();
	}

	public void UpdateButtons(object sender, EventArgs e)
	{
		JobHelper.UpdateButtons(Items, UpdateJobSource());
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
