using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Management.Console;

internal static class EnvironmentHelper
{
	private static bool m_isDirty;

	private static bool m_isLocked;

	private static bool m_isExecutingImmediate;

	public static bool IsLocked
	{
		get
		{
			return m_isLocked;
		}
		set
		{
			m_isLocked = value;
			if (LockStatusChanged != null)
			{
				LockStatusChanged("", new BoolEventArgs
				{
					State = value
				});
			}
		}
	}

	public static bool IsDirty
	{
		get
		{
			return m_isDirty;
		}
		set
		{
			m_isDirty = value;
			if (SaveableStatusChanged != null)
			{
				SaveableStatusChanged("", new BoolEventArgs
				{
					State = value
				});
			}
		}
	}

	public static bool IsExecutingImmediate
	{
		get
		{
			return m_isExecutingImmediate;
		}
		set
		{
			m_isExecutingImmediate = value;
			if (ExecutingImmediateStausChanged != null)
			{
				ExecutingImmediateStausChanged("", new BoolEventArgs
				{
					State = value
				});
			}
		}
	}

	public static event EventHandler<BoolEventArgs> LockStatusChanged;

	public static event EventHandler<BoolEventArgs> SaveableStatusChanged;

	public static event EventHandler<BoolEventArgs> ExecutingImmediateStausChanged;

	public static event EventHandler Save;

	public static event EventHandler SelectedJobChanged;

	public static void DisplayErrors(ErrorList list, bool clearPrevious, bool keepOpenOnSuccessfulInstruction)
	{
		ErrorListView.Instance.AddTop(list, clearPrevious);
		if (!ErrorListView.Instance.KeepOpenOnSuccessfulInstruction)
		{
			ErrorListView.Instance.KeepOpenOnSuccessfulInstruction = keepOpenOnSuccessfulInstruction;
		}
		if (ListViewTabControl.Instance.Find(ListViewNames.Errors) == null)
		{
			ListViewFactory.Instance.MakeTab(ListViewNames.Errors, ProfileManager.Instance.IsConnected);
		}
		ListViewTabControl.Instance.SetFocus(ListViewNames.Errors);
	}

	public static void LockControl(ToolStripItem ctrl)
	{
		LockStatusChanged += delegate
		{
			ctrl.Enabled = ProfileManager.Instance.IsConnected && !IsLocked;
		};
		ctrl.Enabled = ProfileManager.Instance.IsConnected && !IsLocked;
	}

	public static void ToggleToolStripItems(List<ToolStripItem> items, bool enable)
	{
		foreach (ToolStripItem item in items)
		{
			item.Enabled = enable;
		}
	}

	public static void OnSave(object sender, EventArgs e)
	{
		Save(sender, e);
	}

	public static void OnLock(object sender, EventArgs e)
	{
		IsLocked = true;
		OutputWindow.Instance.Append("Locked Console");
		MessageBox.Show("Settings have been locked.", "Lock Command", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	public static void OnUnlock(object sender, EventArgs e)
	{
		UnlockForm unlockForm = new UnlockForm();
		if (unlockForm.ShowDialog() == DialogResult.OK)
		{
			if (!unlockForm.Password.Equals("m4k31ts0!"))
			{
				MessageBox.Show("The entered password is incorrect; settings will not be unlocked.", "Lock Command", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			IsLocked = false;
			OutputWindow.Instance.Append("Unlocked Console");
			MessageBox.Show("Settings have been unlocked", "Lock Command", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
	}

	public static void OnSelectedJobChanged(object sender, EventArgs e)
	{
		SelectedJobChanged(sender, e);
		OutputWindow.Instance.Append("Selection changed to " + JobComboBox.Instance.GetJobID());
	}
}
