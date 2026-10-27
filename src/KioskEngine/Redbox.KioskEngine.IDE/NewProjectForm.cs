using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.IDE;

public class NewProjectForm : Form
{
	private IContainer components;

	private Label label1;

	private Label label2;

	private TextBox m_projectNameTextBox;

	private TextBox m_locationTextBox;

	private ErrorProvider m_errorProvider;

	private Button m_browseButton;

	private Button m_okButton;

	private Button m_cancelButton;

	public string ProjectName
	{
		get
		{
			return m_projectNameTextBox.Text;
		}
		set
		{
			m_projectNameTextBox.Text = value;
		}
	}

	public string ProjectLocation
	{
		get
		{
			return m_locationTextBox.Text;
		}
		set
		{
			m_locationTextBox.Text = value;
		}
	}

	public NewProjectForm()
	{
		InitializeComponent();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		if (service != null)
		{
			m_locationTextBox.Text = service.GetValue("Ide", "DefaultProjectLocation", "C:\\Projects");
		}
	}

	private void OnOK(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_projectNameTextBox, string.Empty);
		m_errorProvider.SetError(m_locationTextBox, string.Empty);
		if (string.IsNullOrEmpty(m_projectNameTextBox.Text))
		{
			m_errorProvider.SetError(m_projectNameTextBox, "Provide a valid project name.");
			return;
		}
		if (string.IsNullOrEmpty(m_locationTextBox.Text))
		{
			m_errorProvider.SetError(m_locationTextBox, "Specify a valid location for the project.");
			return;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCancel(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
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
		this.label2 = new System.Windows.Forms.Label();
		this.m_projectNameTextBox = new System.Windows.Forms.TextBox();
		this.m_locationTextBox = new System.Windows.Forms.TextBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_browseButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 16);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(74, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Project Name:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(35, 44);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(51, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Location:";
		this.m_projectNameTextBox.Location = new System.Drawing.Point(92, 13);
		this.m_projectNameTextBox.MaxLength = 255;
		this.m_projectNameTextBox.Name = "m_projectNameTextBox";
		this.m_projectNameTextBox.Size = new System.Drawing.Size(239, 20);
		this.m_projectNameTextBox.TabIndex = 1;
		this.m_locationTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this.m_locationTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
		this.m_locationTextBox.Location = new System.Drawing.Point(92, 41);
		this.m_locationTextBox.MaxLength = 1024;
		this.m_locationTextBox.Name = "m_locationTextBox";
		this.m_locationTextBox.Size = new System.Drawing.Size(343, 20);
		this.m_locationTextBox.TabIndex = 3;
		this.m_errorProvider.ContainerControl = this;
		this.m_browseButton.Location = new System.Drawing.Point(457, 39);
		this.m_browseButton.Name = "m_browseButton";
		this.m_browseButton.Size = new System.Drawing.Size(27, 23);
		this.m_browseButton.TabIndex = 4;
		this.m_browseButton.Text = "...";
		this.m_browseButton.UseVisualStyleBackColor = true;
		this.m_browseButton.Click += new System.EventHandler(OnBrowse);
		this.m_okButton.Location = new System.Drawing.Point(513, 11);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 5;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(513, 40);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 6;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(600, 86);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_browseButton);
		base.Controls.Add(this.m_locationTextBox);
		base.Controls.Add(this.m_projectNameTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "NewProjectForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Create New Project";
		base.Load += new System.EventHandler(OnLoad);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
