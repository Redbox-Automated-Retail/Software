using System.Windows.Forms;
using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console;

internal class JobItemFactory
{
	private JobItems[] m_orderedJobItems = new JobItems[13]
	{
		JobItems.Garbage,
		JobItems.ViewErrors,
		JobItems.Seperator,
		JobItems.Connect,
		JobItems.Disconnect,
		JobItems.Seperator,
		JobItems.SetLabel,
		JobItems.SetStartTime,
		JobItems.Seperator,
		JobItems.Resume,
		JobItems.Suspend,
		JobItems.Stop,
		JobItems.Trash
	};

	internal static JobItemFactory Instance => Singleton<JobItemFactory>.Instance;

	internal ToolStrip MakeToolStrip(UpdateSource source)
	{
		ToolStrip toolStrip = new ToolStrip();
		toolStrip.Name = "Jobz";
		toolStrip.Text = "Jobz";
		toolStrip.Tag = source;
		JobItems[] orderedJobItems = m_orderedJobItems;
		foreach (JobItems jobItems in orderedJobItems)
		{
			if (jobItems == JobItems.Seperator)
			{
				toolStrip.Items.Add(new ToolStripSeparator());
			}
			else
			{
				toolStrip.Items.Add(JobItem.Instance.MakeJobToolStripButton(jobItems, locked: false));
			}
		}
		return toolStrip;
	}

	internal ToolStripMenuItem MakeMenuItem(UpdateSource source)
	{
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Name = "JOBz";
		toolStripMenuItem.Text = "JOBz";
		toolStripMenuItem.Tag = source;
		JobItems[] orderedJobItems = m_orderedJobItems;
		foreach (JobItems jobItems in orderedJobItems)
		{
			if (jobItems == JobItems.Seperator)
			{
				toolStripMenuItem.DropDown.Items.Add(new ToolStripSeparator());
			}
			else
			{
				toolStripMenuItem.DropDown.Items.Add(JobItem.Instance.MakeJobMenuItem(jobItems, locked: false));
			}
		}
		return toolStripMenuItem;
	}

	internal object MakeMyControl(ControlType ctrlType, UpdateSource source)
	{
		return ctrlType switch
		{
			ControlType.ToolStrip => MakeToolStrip(source), 
			ControlType.MenuItem => MakeMenuItem(source), 
			_ => null, 
		};
	}

	private JobItemFactory()
	{
	}
}
