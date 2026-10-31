using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Divelements.WizardFramework;
using Redbox.Core;
using Redbox.Installer.Framework;
using Redbox.Macros;

namespace Redbox.Installer.Host;

public class DynamicWizardPage : WizardPage
{
	private readonly PageDefinition m_pageDefinition;

	public DynamicWizardPage(PageDefinition pageDefinition)
	{
		m_pageDefinition = pageDefinition;
	}

	public static ErrorList GetWizardPage(PageDefinition pageDefinition, out WizardPageBase wizardPage)
	{
		ErrorList errorList = new ErrorList();
		wizardPage = null;
		switch (pageDefinition.Type)
		{
		case PageType.Welcome:
		{
			IntroductionPage introductionPage = new IntroductionPage();
			introductionPage.Tag = pageDefinition;
			introductionPage.Text = pageDefinition.Title ?? MacroContainer.Instance.ExpandRuntimeMacros("Welcome to the ${ProductName} Setup");
			introductionPage.ProceedText = pageDefinition.ProceedText ?? "To continue, click Next.";
			IntroductionPage introductionPage2 = introductionPage;
			if (pageDefinition.Text != null)
			{
				introductionPage2.IntroductionText = ((!string.IsNullOrEmpty(pageDefinition.Text)) ? pageDefinition.Text.TrimStart() : MacroContainer.Instance.ExpandRuntimeMacros("This setup package will help you to install the ${ProductName} ${ProductVersion} software package."));
			}
			wizardPage = introductionPage2;
			break;
		}
		case PageType.Finish:
		{
			FinishPage finishPage = new FinishPage();
			finishPage.Tag = pageDefinition;
			finishPage.AllowCancel = false;
			finishPage.AllowMovePrevious = false;
			finishPage.Text = pageDefinition.Title ?? MacroContainer.Instance.ExpandRuntimeMacros("Finishing ${ProductName} Setup");
			finishPage.ProceedText = pageDefinition.ProceedText ?? "To continue, click Finish.";
			FinishPage finishPage2 = finishPage;
			if (pageDefinition.Text != null)
			{
				finishPage2.FinishText = ((!string.IsNullOrEmpty(pageDefinition.Text)) ? pageDefinition.Text.TrimStart() : MacroContainer.Instance.ExpandRuntimeMacros("The installation process is complete; please review the final configuration."));
			}
			if (pageDefinition.Properties.ContainsKey("LaunchOption") && !string.IsNullOrEmpty(pageDefinition.Properties["LaunchOption"]) && InstallerProject.Instance.Mode != Redbox.Installer.Framework.InstallMode.Uninstall)
			{
				pageDefinition.Properties["LaunchCheckState"] = MacroContainer.Instance.ExpandRuntimeMacros(pageDefinition.Properties["LaunchCheckState"]);
				CheckBox launchCheckBox = new CheckBox
				{
					Checked = (string.Compare(pageDefinition.Properties["LaunchCheckState"], bool.TrueString, ignoreCase: true) == 0)
				};
				launchCheckBox.CheckedChanged += delegate
				{
					pageDefinition.Properties["LaunchCheckState"] = launchCheckBox.Checked.ToString();
				};
				launchCheckBox.Text = MacroContainer.Instance.ExpandRuntimeMacros(pageDefinition.Properties["LaunchOption"]);
				launchCheckBox.Dock = DockStyle.Bottom;
				finishPage2.Controls.Add(launchCheckBox);
			}
			wizardPage = finishPage2;
			break;
		}
		case PageType.Progress:
		{
			WizardPage page = new WizardPage
			{
				Text = "Installing",
				AllowCancel = false,
				Tag = pageDefinition,
				AllowMoveNext = false,
				AllowMovePrevious = false,
				Description = "Setup is installing the software",
				ProceedText = "Please wait, setup will continue once install steps are complete."
			};
			InstallProgressView progressView = new InstallProgressView
			{
				Dock = DockStyle.Fill
			};
			progressView.InstallComplete += delegate(ErrorList errorList2)
			{
				if (errorList2 == null || errorList2.Count == 0)
				{
					page.AllowMoveNext = true;
					page.Wizard.GoNext();
				}
				else
				{
					ErrorForm errorForm = new ErrorForm
					{
						Message = "Setup was unable to complete for the following reasons:",
						Errors = errorList2
					};
					errorForm.ShowDialog();
					page.AllowMoveNext = true;
					page.Wizard.GoNext();
				}
			};
			page.Controls.Add(progressView);
			page.AfterDisplay += delegate
			{
				progressView.Execute();
			};
			wizardPage = page;
			break;
		}
		case PageType.Custom:
		{
			DynamicWizardPage dynamicWizardPage = new DynamicWizardPage(pageDefinition);
			errorList.AddRange(dynamicWizardPage.Initialize());
			wizardPage = dynamicWizardPage;
			break;
		}
		}
		return errorList;
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		if (base.Controls.Count > 0 && levent.AffectedControl == base.Controls[0])
		{
			base.Controls[0].Location = new Point(30, 0);
			base.Controls[0].Size = new Size(base.Width - 60, base.Height);
		}
		base.OnLayout(levent);
	}

	internal ErrorList Initialize()
	{
		ErrorList errorList = m_pageDefinition.BuildHost(out var control);
		if (!errorList.ContainsError() && control != null)
		{
			base.Controls.Add(control);
			m_pageDefinition.Configure();
		}
		base.AfterDisplay += delegate
		{
			if (m_pageDefinition.Activate())
			{
				base.AllowMoveNext = true;
				base.Wizard.GoNext();
			}
		};
		base.BeforeMoveNext += delegate(object sender, CancelEventArgs e)
		{
			e.Cancel = !m_pageDefinition.Validate();
			if (!e.Cancel)
			{
				m_pageDefinition.Deactivate();
			}
		};
		base.BeforeMoveBack += delegate
		{
			m_pageDefinition.Deactivate();
		};
		base.Tag = m_pageDefinition;
		Text = m_pageDefinition.Title;
		base.Description = m_pageDefinition.SubTitle;
		ProceedText = m_pageDefinition.ProceedText;
		base.AllowCancel = m_pageDefinition.AllowCancel.Value;
		base.AllowMoveNext = m_pageDefinition.AllowMoveNext.Value;
		base.AllowMovePrevious = m_pageDefinition.AllowMovePrevious.Value;
		return errorList;
	}
}
