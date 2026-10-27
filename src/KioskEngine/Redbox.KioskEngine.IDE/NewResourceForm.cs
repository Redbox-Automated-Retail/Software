using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class NewResourceForm : Form
{
	private IContainer components;

	private Label label1;

	private TextBox m_nameTextBox;

	private ErrorProvider m_errorProvider;

	private Button m_okButton;

	private Button m_cancelButton;

	private Label label2;

	private ResourceTypeTreeView m_resourceTypeTreeView;

	public NewResourceForm()
	{
		InitializeComponent();
	}

	private void OnOK(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCancel(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
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
		this.m_nameTextBox = new System.Windows.Forms.TextBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		this.m_resourceTypeTreeView = new Redbox.KioskEngine.IDE.ResourceTypeTreeView();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 17);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(38, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Name:";
		this.m_nameTextBox.Location = new System.Drawing.Point(56, 14);
		this.m_nameTextBox.Name = "m_nameTextBox";
		this.m_nameTextBox.Size = new System.Drawing.Size(241, 20);
		this.m_nameTextBox.TabIndex = 1;
		this.m_errorProvider.ContainerControl = this;
		this.m_okButton.Location = new System.Drawing.Point(335, 12);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 4;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(335, 41);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 5;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 41);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(34, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Type:";
		this.m_resourceTypeTreeView.Bundle = null;
		this.m_resourceTypeTreeView.Location = new System.Drawing.Point(56, 41);
		this.m_resourceTypeTreeView.Name = "m_resourceTypeTreeView";
		this.m_resourceTypeTreeView.SelectedType = null;
		this.m_resourceTypeTreeView.Size = new System.Drawing.Size(241, 220);
		this.m_resourceTypeTreeView.TabIndex = 6;
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(422, 273);
		base.ControlBox = false;
		base.Controls.Add(this.m_resourceTypeTreeView);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_nameTextBox);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "NewResourceForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "New Resource";
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
