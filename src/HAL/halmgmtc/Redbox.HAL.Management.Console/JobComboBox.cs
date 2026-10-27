using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

internal class JobComboBox : ToolStripComboBox
{
	private string m_previousSelectedID;

	private static JobComboBox m_instance;

	public static JobComboBox Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new JobComboBox();
			}
			return m_instance;
		}
	}

	public List<HardwareJobWrapper> GetSelectedJobList()
	{
		return new List<HardwareJobWrapper> { (HardwareJobWrapper)base.SelectedItem };
	}

	public string GetJobID()
	{
		if (base.SelectedItem == null || base.SelectedItem.ToString() == Constants.None)
		{
			return null;
		}
		return base.SelectedItem.ToString();
	}

	private JobComboBox()
	{
		base.ComboBox.DataSource = JobHelper.JobComboList;
		base.Name = "m_jobToolStripComboBox";
		Size = new Size(121, 25);
		base.DropDownStyle = ComboBoxStyle.DropDownList;
		Enabled = false;
		base.SelectedIndexChanged += OnSelectedJobChanged;
		ProfileManager.Instance.Connected += delegate
		{
			Enabled = true;
		};
		ProfileManager.Instance.Disconnected += delegate
		{
			Enabled = false;
			JobHelper.ClearJobComboList();
		};
	}

	private void OnSelectedJobChanged(object sender, EventArgs e)
	{
		if (ProfileManager.Instance.IsConnected && (m_previousSelectedID == null || !(m_previousSelectedID == base.SelectedItem.ToString())))
		{
			m_previousSelectedID = base.SelectedItem.ToString();
			EnvironmentHelper.OnSelectedJobChanged(sender, e);
		}
	}
}
