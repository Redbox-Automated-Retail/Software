using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.IDE;

public class IdePreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Label m_defaultProjectLocationLabel;

	private TextBox m_locationTextBox;

	private Button m_browseButton;

	private ErrorProvider m_errorProvider;

	private CheckBox m_showIdeInTaskBar;

	private CheckBox m_showKioskEngineInTaskBarCheckBox;

	private Label m_headerLabel;

	public IdePreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		m_errorProvider.SetError(m_locationTextBox, string.Empty);
		if (string.IsNullOrEmpty(m_locationTextBox.Text))
		{
			m_errorProvider.SetError(m_locationTextBox, "Provide a valid default project location.");
			return false;
		}
		preferencePage.SetValue("DefaultProjectLocation", m_locationTextBox.Text);
		preferencePage.SetValue("ShowKioskEngineInTaskBar", m_showKioskEngineInTaskBarCheckBox.Checked);
		preferencePage.SetValue("ShowIDEInTaskBar", m_showIdeInTaskBar.Checked);
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_locationTextBox.Text = preferencePage.GetValue("DefaultProjectLocation", "C:\\Projects");
		m_showKioskEngineInTaskBarCheckBox.Checked = preferencePage.GetValue("ShowKioskEngineInTaskBar", defaultValue: false);
		m_showIdeInTaskBar.Checked = preferencePage.GetValue("ShowIDEInTaskBar", defaultValue: true);
	}

	private void OnBrowse(object sender, EventArgs e)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
		{
			SelectedPath = m_locationTextBox.Text
		};
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			m_locationTextBox.Text = folderBrowserDialog.SelectedPath;
		}
	}

	private void OnShowIdeCheckChanged(object sender, EventArgs e)
	{
		Form form = Application.OpenForms["IdeMainForm"];
		if (form != null)
		{
			form.ShowInTaskbar = m_showIdeInTaskBar.Checked;
		}
	}

	private void OnShowKioskEngineCheckChanged(object sender, EventArgs e)
	{
		Form form = Application.OpenForms["HostForm"];
		if (form != null)
		{
			form.ShowInTaskbar = m_showKioskEngineInTaskBarCheckBox.Checked;
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
		this.m_defaultProjectLocationLabel = new System.Windows.Forms.Label();
		this.m_locationTextBox = new System.Windows.Forms.TextBox();
		this.m_browseButton = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_showKioskEngineInTaskBarCheckBox = new System.Windows.Forms.CheckBox();
		this.m_showIdeInTaskBar = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_defaultProjectLocationLabel.AutoSize = true;
		this.m_defaultProjectLocationLabel.Location = new System.Drawing.Point(31, 60);
		this.m_defaultProjectLocationLabel.Name = "m_defaultProjectLocationLabel";
		this.m_defaultProjectLocationLabel.Size = new System.Drawing.Size(124, 13);
		this.m_defaultProjectLocationLabel.TabIndex = 0;
		this.m_defaultProjectLocationLabel.Text = "Default Project Location:";
		this.m_locationTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this.m_locationTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
		this.m_locationTextBox.Location = new System.Drawing.Point(34, 76);
		this.m_locationTextBox.MaxLength = 1024;
		this.m_locationTextBox.Name = "m_locationTextBox";
		this.m_locationTextBox.Size = new System.Drawing.Size(415, 20);
		this.m_locationTextBox.TabIndex = 1;
		this.m_browseButton.Location = new System.Drawing.Point(473, 74);
		this.m_browseButton.Name = "m_browseButton";
		this.m_browseButton.Size = new System.Drawing.Size(24, 23);
		this.m_browseButton.TabIndex = 2;
		this.m_browseButton.Text = "...";
		this.m_browseButton.UseVisualStyleBackColor = true;
		this.m_browseButton.Click += new System.EventHandler(OnBrowse);
		this.m_errorProvider.ContainerControl = this;
		this.m_showKioskEngineInTaskBarCheckBox.AutoSize = true;
		this.m_showKioskEngineInTaskBarCheckBox.Location = new System.Drawing.Point(34, 111);
		this.m_showKioskEngineInTaskBarCheckBox.Name = "m_showKioskEngineInTaskBarCheckBox";
		this.m_showKioskEngineInTaskBarCheckBox.Size = new System.Drawing.Size(175, 17);
		this.m_showKioskEngineInTaskBarCheckBox.TabIndex = 3;
		this.m_showKioskEngineInTaskBarCheckBox.Text = "Show Kiosk Engine in Task Bar";
		this.m_showKioskEngineInTaskBarCheckBox.UseVisualStyleBackColor = true;
		this.m_showKioskEngineInTaskBarCheckBox.CheckedChanged += new System.EventHandler(OnShowKioskEngineCheckChanged);
		this.m_showIdeInTaskBar.AutoSize = true;
		this.m_showIdeInTaskBar.Location = new System.Drawing.Point(34, 134);
		this.m_showIdeInTaskBar.Name = "m_showIdeInTaskBar";
		this.m_showIdeInTaskBar.Size = new System.Drawing.Size(131, 17);
		this.m_showIdeInTaskBar.TabIndex = 4;
		this.m_showIdeInTaskBar.Text = "Show IDE in Task Bar";
		this.m_showIdeInTaskBar.UseVisualStyleBackColor = true;
		this.m_showIdeInTaskBar.CheckedChanged += new System.EventHandler(OnShowIdeCheckChanged);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.MediumBlue;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 15;
		this.m_headerLabel.Text = "  Engine Core: IDE Settings";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_showIdeInTaskBar);
		base.Controls.Add(this.m_showKioskEngineInTaskBarCheckBox);
		base.Controls.Add(this.m_browseButton);
		base.Controls.Add(this.m_locationTextBox);
		base.Controls.Add(this.m_defaultProjectLocationLabel);
		base.Name = "IdePreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
