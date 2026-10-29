using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.IconLib;
using System.Reflection;
using System.Windows.Forms;
using Divelements.WizardFramework;
using Redbox.Core;
using Redbox.Extensibility.ComponentModel;
using Redbox.Extensibility.Shell.PropertyGrid;
using Redbox.Installer.Framework;

namespace Redbox.Installer.Host;

public class MainForm : Form, IWizardView, IShellConfiguration
{
	private readonly Wizard m_wizard;

	private readonly WizardPresenter m_presenter;

	private IContainer components;

	public Form HostForm { get; set; }

	public string Title { get; set; }

	public new string ProductName { get; set; }

	public new string ProductVersion { get; set; }

	public string BugReportingUrl { get; set; }

	public bool ShowResizeGripper { get; set; }

	public bool PersistSettings { get; set; }

	public string RootRegistryPath { get; set; }

	public string BugReportingArea { get; set; }

	public string BugReportingProject { get; set; }

	public string BugReportingUserName { get; set; }

	public Image BannerImage
	{
		get
		{
			return m_wizard.BannerImage;
		}
		set
		{
			m_wizard.BannerImage = value;
		}
	}

	public Image MarginImage
	{
		get
		{
			return m_wizard.MarginImage;
		}
		set
		{
			m_wizard.MarginImage = value;
		}
	}

	public PageDefinition SelectedPageDefinition
	{
		get
		{
			if (m_wizard.SelectedPage == null)
			{
				return null;
			}
			return m_wizard.SelectedPage.Tag as PageDefinition;
		}
	}

	public static MainForm Instance => Singleton<MainForm>.Instance;

	private MainForm()
	{
		InitializeComponent();
		m_wizard = new Wizard
		{
			AutoCloseModalForm = false,
			Location = new Point(0, 0),
			Name = "m_wizard",
			OwnerForm = this,
			Size = new Size(634, 448),
			TabIndex = 0
		};
		m_wizard.Finish += OnFinish;
		m_wizard.Cancel += OnCancel;
		base.Controls.Add(m_wizard);
		ServiceLocator.Instance.AddService(typeof(IShellConfiguration), this);
		ServiceLocator.Instance.AddService(typeof(IPropertyGridService), PropertyGridService.Instance);
		m_presenter = new WizardPresenter(this);
		m_presenter.Initialize();
	}

	public Redbox.Installer.Framework.ErrorList ConfigurePages()
	{
		Redbox.Installer.Framework.ErrorList errorList = new Redbox.Installer.Framework.ErrorList();
		List<WizardPageBase> list = new List<WizardPageBase>();
		foreach (PageDefinition page in InstallerProject.Instance.Pages)
		{
			errorList.AddRange(DynamicWizardPage.GetWizardPage(page, out var wizardPage));
			if (!errorList.ContainsError() && wizardPage != null)
			{
				list.Add(wizardPage);
			}
		}
		WizardPageBase[] array = list.ToArray();
		m_wizard.Controls.AddRange(array);
		Wizard.SetPageOrder(array);
		return errorList;
	}

	public Redbox.Extensibility.ComponentModel.ErrorList Shutdown()
	{
		return new Redbox.Extensibility.ComponentModel.ErrorList();
	}

	public Redbox.Extensibility.ComponentModel.ErrorList Initialize()
	{
		return new Redbox.Extensibility.ComponentModel.ErrorList();
	}

	public void LoadLayouts()
	{
	}

	public void SaveLayouts()
	{
	}

	public void ThreadSafeHostUpdate(MethodInvoker method)
	{
		if (base.InvokeRequired)
		{
			Invoke(method);
		}
		else
		{
			method();
		}
	}

	private void OnLoad(object sender, EventArgs e)
	{
		try
		{
			MultiIcon multiIcon = new MultiIcon();
			multiIcon.Load(Assembly.GetExecutingAssembly().Location);
			base.Icon = multiIcon[0].Icon;
		}
		catch
		{
		}
		m_presenter.Loaded();
	}

	private void OnActivated(object sender, EventArgs e)
	{
		m_presenter.Activated();
	}

	private void OnClosing(object sender, FormClosingEventArgs e)
	{
		e.Cancel = m_presenter.Closing(e.CloseReason);
	}

	private void OnClosed(object sender, FormClosedEventArgs e)
	{
		m_presenter.Closed(e.CloseReason);
	}

	private void OnCancel(object sender, EventArgs e)
	{
		m_presenter.Cancel();
	}

	private void OnFinish(object sender, EventArgs e)
	{
		m_presenter.Finish();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.Installer.Host.MainForm));
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(634, 448);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "MainForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Redbox Installer from XML";
		base.Load += new System.EventHandler(OnLoad);
		base.Activated += new System.EventHandler(OnActivated);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(OnClosed);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(OnClosing);
		base.ResumeLayout(false);
	}

	virtual void IWizardView.Close()
	{
		Close();
	}
}
