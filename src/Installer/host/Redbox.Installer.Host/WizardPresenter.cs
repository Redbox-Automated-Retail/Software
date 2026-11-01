using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.Installer.Framework;
using Redbox.Macros;

namespace Redbox.Installer.Host;

public class WizardPresenter
{
	private bool m_userFinished;

	private readonly IWizardView m_view;

	public WizardPresenter(IWizardView view)
	{
		m_view = view;
	}

	public void Initialize()
	{
		ErrorList errorList = m_view.ConfigurePages();
		if (errorList.Count != 0)
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.Message = "Setup was unable to compile wizard pages for the following reasons:";
			errorForm.Errors = errorList;
			ErrorForm errorForm2 = errorForm;
			errorForm2.ShowDialog();
		}
	}

	public void Loaded()
	{
		m_view.Text = string.Format("{0} {1} Setup {2}", InstallerProject.Instance.ProductName, InstallerProject.Instance.ProductVersion, (InstallerProject.Instance.Mode == Redbox.Installer.Framework.InstallMode.Uninstall) ? "(Remove)" : string.Empty);
		int width = m_view.Size.Width;
		int height = m_view.Size.Height;
		if (InstallerProject.Instance.Values["width"] != null && int.TryParse(InstallerProject.Instance.Values["width"], out var configuredWidth))
		{
			width = configuredWidth;
		}
		if (InstallerProject.Instance.Values["height"] != null && int.TryParse(InstallerProject.Instance.Values["height"], out var configuredHeight))
		{
			height = configuredHeight;
		}
		if (InstallerProject.Instance.Values["marginimage"] != null)
		{
			FileEntryDefinition fileEntryDefinition = (FileEntryDefinition)InstallerProject.Instance.Definitions.GetByID(InstallerProject.Instance.Values["marginimage"]);
			if (fileEntryDefinition != null && fileEntryDefinition.FileEntry != null)
			{
				using FileStream stream = new FileStream(fileEntryDefinition.FileEntry, FileMode.Open, FileAccess.Read);
				m_view.MarginImage = Image.FromStream(stream);
			}
		}
		if (InstallerProject.Instance.Values["bannerimage"] != null)
		{
			FileEntryDefinition fileEntryDefinition2 = (FileEntryDefinition)InstallerProject.Instance.Definitions.GetByID(InstallerProject.Instance.Values["bannerimage"]);
			if (fileEntryDefinition2 != null && fileEntryDefinition2.FileEntry != null)
			{
				using FileStream stream2 = new FileStream(fileEntryDefinition2.FileEntry, FileMode.Open, FileAccess.Read);
				m_view.BannerImage = Image.FromStream(stream2);
			}
		}
		m_view.Size = new Size(width, height);
	}

	public void Activated()
	{
	}

	public void Closed(CloseReason reason)
	{
		InstallerProject.Instance.Definitions.DisposeAllPages();
		InstallerApplication.Instance.ProcessPendingReboot();
	}

	public bool Closing(CloseReason reason)
	{
		if (m_view.SelectedPageDefinition == null)
		{
			InstallerApplication.Instance.UserCanceled = !m_userFinished;
			return false;
		}
		if (!m_view.SelectedPageDefinition.AllowCancel.Value)
		{
			MessageBox.Show("It is not possible to close the installer from this page.", MacroContainer.Instance.ExpandRuntimeMacros("${ProductName} ${ProductVersion} Setup"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
		InstallerApplication.Instance.UserCanceled = m_view.SelectedPageDefinition.AllowCancel.Value && !m_userFinished;
		return !m_view.SelectedPageDefinition.AllowCancel.Value;
	}

	public void Cancel()
	{
		InstallerApplication.Instance.UserCanceled = true;
		m_view.Close();
	}

	public void Finish()
	{
		m_userFinished = true;
		m_view.Close();
	}
}
