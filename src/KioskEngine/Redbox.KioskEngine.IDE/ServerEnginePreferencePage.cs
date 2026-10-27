using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.IDE;

public class ServerEnginePreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Label label1;

	private TextBox m_serverEngineUrlTextBox;

	private Label label2;

	private TextBox m_serverEngineTmeoutTextBox;

	private ErrorProvider m_errorProvider;

	public ServerEnginePreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		m_errorProvider.SetError(m_serverEngineUrlTextBox, string.Empty);
		m_errorProvider.SetError(m_serverEngineTmeoutTextBox, string.Empty);
		IServerEngineService service = ServiceLocator.Instance.GetService<IServerEngineService>();
		if (service == null)
		{
			return true;
		}
		if (string.IsNullOrEmpty(m_serverEngineUrlTextBox.Text))
		{
			m_errorProvider.SetError(m_serverEngineTmeoutTextBox, "A valid URL is required to the Server Engine service, e.g. rcp://localhost:7008.");
			return false;
		}
		service.Url = m_serverEngineUrlTextBox.Text;
		if (!int.TryParse(m_serverEngineTmeoutTextBox.Text, out var result))
		{
			m_errorProvider.SetError(m_serverEngineTmeoutTextBox, "A valid integer value is required for the timeout.");
			return false;
		}
		service.Timeout = result;
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		IServerEngineService service = ServiceLocator.Instance.GetService<IServerEngineService>();
		if (service != null)
		{
			m_serverEngineUrlTextBox.Text = service.Url;
			m_serverEngineTmeoutTextBox.Text = service.Timeout.ToString();
		}
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
		this.label1 = new System.Windows.Forms.Label();
		this.m_serverEngineUrlTextBox = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_serverEngineTmeoutTextBox = new System.Windows.Forms.TextBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(30, 54);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(71, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Service URL:";
		this.m_serverEngineUrlTextBox.Location = new System.Drawing.Point(107, 51);
		this.m_serverEngineUrlTextBox.Name = "m_serverEngineUrlTextBox";
		this.m_serverEngineUrlTextBox.Size = new System.Drawing.Size(399, 20);
		this.m_serverEngineUrlTextBox.TabIndex = 1;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(15, 81);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(87, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Service Timeout:";
		this.m_serverEngineTmeoutTextBox.Location = new System.Drawing.Point(107, 78);
		this.m_serverEngineTmeoutTextBox.Name = "m_serverEngineTmeoutTextBox";
		this.m_serverEngineTmeoutTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_serverEngineTmeoutTextBox.TabIndex = 3;
		this.m_errorProvider.ContainerControl = this;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_serverEngineTmeoutTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_serverEngineUrlTextBox);
		base.Controls.Add(this.label1);
		base.Name = "ServerEnginePreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
