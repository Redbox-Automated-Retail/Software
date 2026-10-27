using System.ComponentModel;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class ToolWindow : DockContent
{
	private IContainer components;

	internal IUserSettingsStore UserSettings => ServiceLocator.Instance.GetService<IUserSettingsStore>();

	internal IResourceBundleService BundleService => ServiceLocator.Instance.GetService<IResourceBundleService>();

	internal IMachineSettingsStore MachineSettings => ServiceLocator.Instance.GetService<IMachineSettingsStore>();

	public ToolWindow()
	{
		InitializeComponent();
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
		this.Text = "ToolWindow";
	}
}
