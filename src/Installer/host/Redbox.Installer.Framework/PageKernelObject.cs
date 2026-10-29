using System;
using System.Windows.Forms;
using Redbox.ScriptingHost.Framework;

namespace Redbox.Installer.Framework;

public class PageKernelObject : KernelObject
{
	public string PageID { get; set; }

	public event ValidateHandler Validate;

	public event ActivateHandler Activate;

	public event ConfigureHandler Configure;

	public event DeactivateHandler Deactivate;

	public string getInstanceName(string macroName)
	{
		return $"{PageID}:{macroName}";
	}

	public bool RaiseActivate()
	{
		try
		{
			return Activate != null && Activate();
		}
		catch (Exception ex)
		{
			MessageBox.Show("An unhandled exception was rasied in the page kernel object:" + Environment.NewLine + ex, "Redbox Installer from XML Page Kernel Object", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return false;
		}
	}

	public bool RaiseValidate()
	{
		try
		{
			return Validate == null || Validate();
		}
		catch (Exception ex)
		{
			MessageBox.Show("An unhandled exception was rasied in the page kernel object:" + Environment.NewLine + ex, "Redbox Installer from XML Page Kernel Object", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return false;
		}
	}

	public void RaiseConfigure()
	{
		if (Configure == null)
		{
			return;
		}
		try
		{
			Configure();
		}
		catch (Exception ex)
		{
			MessageBox.Show("An unhandled exception was rasied in the page kernel object:" + Environment.NewLine + ex, "Redbox Installer from XML Page Kernel Object", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void RaiseDeactivate()
	{
		if (Deactivate == null)
		{
			return;
		}
		try
		{
			Deactivate();
		}
		catch (Exception ex)
		{
			MessageBox.Show("An unhandled exception was rasied in the page kernel object:" + Environment.NewLine + ex, "Redbox Installer from XML Page Kernel Object", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}
}
