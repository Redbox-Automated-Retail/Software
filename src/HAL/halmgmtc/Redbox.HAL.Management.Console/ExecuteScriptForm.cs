using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;

namespace Redbox.HAL.Management.Console;

public class ExecuteScriptForm : Form
{
	private bool m_isInitialized;

	private IContainer components;

	private Button m_okButton;

	private Button m_cancelButton;

	private Label label1;

	private ComboBox m_scriptDropDownList;

	private Label label2;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string SelectedScript
	{
		get
		{
			if (m_scriptDropDownList.SelectedIndex != -1)
			{
				return m_scriptDropDownList.SelectedItem.ToString();
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_scriptDropDownList.SelectedIndex = -1;
			}
			else
			{
				m_scriptDropDownList.SelectedIndex = m_scriptDropDownList.FindStringExact(value);
			}
		}
	}

	public ExecuteScriptForm()
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

	private void OnActivated(object sender, EventArgs e)
	{
		if (!m_isInitialized)
		{
			HardwareCommandResult programs = ProfileManager.Instance.Service.GetPrograms();
			if (!programs.Success)
			{
				Close();
			}
			m_scriptDropDownList.BeginUpdate();
			for (int i = 0; i < programs.CommandMessages.Count; i++)
			{
				string[] array = programs.CommandMessages[i].Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				m_scriptDropDownList.Items.Add(array[0]);
			}
			m_scriptDropDownList.EndUpdate();
			if (m_scriptDropDownList.Items.Count > 0)
			{
				m_scriptDropDownList.SelectedIndex = 0;
			}
			m_isInitialized = true;
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
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.m_scriptDropDownList = new System.Windows.Forms.ComboBox();
		this.label2 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_okButton.Location = new System.Drawing.Point(279, 12);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 0;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(279, 41);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 1;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 12);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(121, 13);
		this.label1.TabIndex = 2;
		this.label1.Text = "Select script to execute:";
		this.m_scriptDropDownList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_scriptDropDownList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_scriptDropDownList.FormattingEnabled = true;
		this.m_scriptDropDownList.Location = new System.Drawing.Point(12, 28);
		this.m_scriptDropDownList.Name = "m_scriptDropDownList";
		this.m_scriptDropDownList.Size = new System.Drawing.Size(239, 21);
		this.m_scriptDropDownList.TabIndex = 3;
		this.label2.Location = new System.Drawing.Point(12, 52);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(261, 58);
		this.label2.TabIndex = 4;
		this.label2.Text = "NOTE:  If the script requires input parameters on the stack, select the scheduled job from the Job Control toolbar and then use the Stack View to populate the necessary values.";
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(366, 114);
		base.ControlBox = false;
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_scriptDropDownList);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ExecuteScriptForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Execute Script";
		base.Activated += new System.EventHandler(OnActivated);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
