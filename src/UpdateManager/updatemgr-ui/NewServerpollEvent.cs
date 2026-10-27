using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.UI;

public class NewServerpollEvent : Form
{
	private const string JobName = "Serverpoll";

	private IContainer components;

	private ErrorProvider m_errors;

	private Button m_cancel;

	private Button m_submit;

	private TabControl tabControl1;

	private TabPage BasicPage;

	private TextBox m_recurrance_sec;

	private TextBox m_initial_sec;

	private Label m_recurrenceLabel;

	private Label label2;

	private TabPage AdvancedPage;

	private TextBox Instructions;

	private TextBox m_cron;

	private Label label5;

	private TextBox m_initial_sec_adv;

	private Label label1;

	public NewServerpollEvent()
	{
		InitializeComponent();
	}

	private void OnSubmitClick(object sender, EventArgs e)
	{
		m_errors.SetError(m_cron, string.Empty);
		m_errors.SetError(m_initial_sec, string.Empty);
		m_errors.SetError(m_initial_sec_adv, string.Empty);
		bool flag = false;
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		ErrorList errorList = new ErrorList();
		if (tabControl1.SelectedTab == BasicPage)
		{
			TimeSpan? startOffset = null;
			TimeSpan? timeSpan = null;
			if (!string.IsNullOrEmpty(m_initial_sec.Text))
			{
				if (int.TryParse(m_initial_sec.Text, out var result))
				{
					startOffset = TimeSpan.FromSeconds(result);
				}
				else
				{
					m_errors.SetError(m_initial_sec, "Initial Offset is not a valid integer value.");
					flag = true;
				}
			}
			int result2;
			if (string.IsNullOrEmpty(m_recurrance_sec.Text))
			{
				m_errors.SetError(m_recurrance_sec, "Recurrence is a required field.");
				flag = true;
			}
			else if (int.TryParse(m_recurrance_sec.Text, out result2))
			{
				timeSpan = TimeSpan.FromSeconds(result2);
			}
			else
			{
				m_errors.SetError(m_initial_sec, "Initial Offset is not a valid integer value.");
				flag = true;
			}
			if (flag)
			{
				return;
			}
			errorList.AddRange(service.ScheduleSimpleJob("Serverpoll", "Serverpoll", PayloadType.ServerPoll, startOffset, null, null, timeSpan.GetValueOrDefault()));
		}
		else
		{
			TimeSpan? startOffset2 = null;
			if (!string.IsNullOrEmpty(m_initial_sec_adv.Text))
			{
				if (int.TryParse(m_initial_sec_adv.Text, out var result3))
				{
					startOffset2 = TimeSpan.FromSeconds(result3);
				}
				else
				{
					m_errors.SetError(m_initial_sec_adv, "Initial Offset is not a valid integer value.");
					flag = true;
				}
			}
			if (string.IsNullOrEmpty(m_cron.Text))
			{
				m_errors.SetError(m_cron, "Expression is a required field.");
				flag = true;
			}
			if (flag)
			{
				return;
			}
			errorList.AddRange(service.ScheduleCronJob("Serverpoll", "Serverpoll", PayloadType.ServerPoll, startOffset2, null, null, m_cron.Text));
		}
		if (errorList.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errorList;
			errorForm.ShowDialog();
		}
		Close();
	}

	private void OnCancelClick(object sender, EventArgs e)
	{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.UpdateManager.UI.NewServerpollEvent));
		this.m_errors = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_cancel = new System.Windows.Forms.Button();
		this.m_submit = new System.Windows.Forms.Button();
		this.tabControl1 = new System.Windows.Forms.TabControl();
		this.BasicPage = new System.Windows.Forms.TabPage();
		this.m_recurrance_sec = new System.Windows.Forms.TextBox();
		this.m_initial_sec = new System.Windows.Forms.TextBox();
		this.m_recurrenceLabel = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.AdvancedPage = new System.Windows.Forms.TabPage();
		this.Instructions = new System.Windows.Forms.TextBox();
		this.m_cron = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_initial_sec_adv = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errors).BeginInit();
		this.tabControl1.SuspendLayout();
		this.BasicPage.SuspendLayout();
		this.AdvancedPage.SuspendLayout();
		base.SuspendLayout();
		this.m_errors.ContainerControl = this;
		this.m_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancel.Location = new System.Drawing.Point(452, 64);
		this.m_cancel.Name = "m_cancel";
		this.m_cancel.Size = new System.Drawing.Size(75, 23);
		this.m_cancel.TabIndex = 7;
		this.m_cancel.Text = "Cancel";
		this.m_cancel.UseVisualStyleBackColor = true;
		this.m_cancel.Click += new System.EventHandler(OnCancelClick);
		this.m_submit.Location = new System.Drawing.Point(452, 30);
		this.m_submit.Name = "m_submit";
		this.m_submit.Size = new System.Drawing.Size(75, 23);
		this.m_submit.TabIndex = 6;
		this.m_submit.Text = "OK";
		this.m_submit.UseVisualStyleBackColor = true;
		this.m_submit.Click += new System.EventHandler(OnSubmitClick);
		this.tabControl1.Controls.Add(this.BasicPage);
		this.tabControl1.Controls.Add(this.AdvancedPage);
		this.tabControl1.Location = new System.Drawing.Point(12, 9);
		this.tabControl1.Name = "tabControl1";
		this.tabControl1.SelectedIndex = 0;
		this.tabControl1.Size = new System.Drawing.Size(434, 195);
		this.tabControl1.TabIndex = 14;
		this.BasicPage.Controls.Add(this.m_recurrance_sec);
		this.BasicPage.Controls.Add(this.m_initial_sec);
		this.BasicPage.Controls.Add(this.m_recurrenceLabel);
		this.BasicPage.Controls.Add(this.label2);
		this.BasicPage.Location = new System.Drawing.Point(4, 22);
		this.BasicPage.Name = "BasicPage";
		this.BasicPage.Padding = new System.Windows.Forms.Padding(3);
		this.BasicPage.Size = new System.Drawing.Size(426, 169);
		this.BasicPage.TabIndex = 0;
		this.BasicPage.Text = "Basic";
		this.BasicPage.UseVisualStyleBackColor = true;
		this.m_recurrance_sec.Location = new System.Drawing.Point(119, 32);
		this.m_recurrance_sec.Name = "m_recurrance_sec";
		this.m_recurrance_sec.Size = new System.Drawing.Size(92, 20);
		this.m_recurrance_sec.TabIndex = 5;
		this.m_initial_sec.Location = new System.Drawing.Point(119, 6);
		this.m_initial_sec.Name = "m_initial_sec";
		this.m_initial_sec.Size = new System.Drawing.Size(92, 20);
		this.m_initial_sec.TabIndex = 4;
		this.m_recurrenceLabel.AutoSize = true;
		this.m_recurrenceLabel.Location = new System.Drawing.Point(6, 35);
		this.m_recurrenceLabel.Name = "m_recurrenceLabel";
		this.m_recurrenceLabel.Size = new System.Drawing.Size(92, 13);
		this.m_recurrenceLabel.TabIndex = 18;
		this.m_recurrenceLabel.Text = "Recurrence: (sec)";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(6, 9);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(91, 13);
		this.label2.TabIndex = 16;
		this.label2.Text = "Initial Offset: (sec)";
		this.AdvancedPage.Controls.Add(this.m_initial_sec_adv);
		this.AdvancedPage.Controls.Add(this.label1);
		this.AdvancedPage.Controls.Add(this.Instructions);
		this.AdvancedPage.Controls.Add(this.m_cron);
		this.AdvancedPage.Controls.Add(this.label5);
		this.AdvancedPage.Location = new System.Drawing.Point(4, 22);
		this.AdvancedPage.Name = "AdvancedPage";
		this.AdvancedPage.Padding = new System.Windows.Forms.Padding(3);
		this.AdvancedPage.Size = new System.Drawing.Size(426, 169);
		this.AdvancedPage.TabIndex = 1;
		this.AdvancedPage.Text = "Advanced";
		this.AdvancedPage.UseVisualStyleBackColor = true;
		this.Instructions.BackColor = System.Drawing.SystemColors.Control;
		this.Instructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.Instructions.Cursor = System.Windows.Forms.Cursors.Default;
		this.Instructions.Font = new System.Drawing.Font("Lucida Console", 7f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Instructions.Location = new System.Drawing.Point(108, 63);
		this.Instructions.Multiline = true;
		this.Instructions.Name = "Instructions";
		this.Instructions.ReadOnly = true;
		this.Instructions.Size = new System.Drawing.Size(263, 88);
		this.Instructions.TabIndex = 4;
		this.Instructions.Text = resources.GetString("Instructions.Text");
		this.m_cron.Location = new System.Drawing.Point(103, 37);
		this.m_cron.Name = "m_cron";
		this.m_cron.Size = new System.Drawing.Size(268, 20);
		this.m_cron.TabIndex = 5;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(11, 40);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(86, 13);
		this.label5.TabIndex = 1;
		this.label5.Text = "Cron Expression:";
		this.m_initial_sec_adv.Location = new System.Drawing.Point(106, 6);
		this.m_initial_sec_adv.Name = "m_initial_sec_adv";
		this.m_initial_sec_adv.Size = new System.Drawing.Size(92, 20);
		this.m_initial_sec_adv.TabIndex = 17;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(11, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(91, 13);
		this.label1.TabIndex = 18;
		this.label1.Text = "Initial Offset: (sec)";
		base.AcceptButton = this.m_submit;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancel;
		base.ClientSize = new System.Drawing.Size(537, 228);
		base.Controls.Add(this.tabControl1);
		base.Controls.Add(this.m_cancel);
		base.Controls.Add(this.m_submit);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "NewServerpollEvent";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Create New Shell Event";
		((System.ComponentModel.ISupportInitialize)this.m_errors).EndInit();
		this.tabControl1.ResumeLayout(false);
		this.BasicPage.ResumeLayout(false);
		this.BasicPage.PerformLayout();
		this.AdvancedPage.ResumeLayout(false);
		this.AdvancedPage.PerformLayout();
		base.ResumeLayout(false);
	}
}
