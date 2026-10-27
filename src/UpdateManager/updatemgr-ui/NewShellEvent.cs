using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.UI;

public class NewShellEvent : Form
{
	private IContainer components;

	private ErrorProvider m_errors;

	private Button m_cancel;

	private Button m_submit;

	private TabControl tabControl1;

	private TabPage BasicPage;

	private TextBox m_args_basic;

	private Label label7;

	private TextBox Simple_name;

	private Label label1;

	private TextBox m_recurrance_sec;

	private TextBox m_initial_sec;

	private TextBox m_path;

	private Label m_recurrenceLabel;

	private Label label2;

	private Label scriptlocationlabel;

	private TabPage AdvancedPage;

	private TextBox m_arguments_adv;

	private Label label6;

	private Label label4;

	private TextBox m_adv_name;

	private TextBox Instructions;

	private TextBox m_cron;

	private TextBox m_pathAdvanced;

	private Label label5;

	private Label label3;

	private TextBox m_initial_sec_adv;

	private Label label8;

	public NewShellEvent()
	{
		InitializeComponent();
	}

	private void OnSubmitClick(object sender, EventArgs e)
	{
		m_errors.SetError(m_path, string.Empty);
		m_errors.SetError(m_cron, string.Empty);
		m_errors.SetError(m_pathAdvanced, string.Empty);
		m_errors.SetError(m_initial_sec, string.Empty);
		m_errors.SetError(m_initial_sec_adv, string.Empty);
		bool flag = false;
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		ErrorList errorList = new ErrorList();
		if (tabControl1.SelectedTab == BasicPage)
		{
			if (string.IsNullOrEmpty(m_path.Text))
			{
				m_errors.SetError(m_path, "Path is a required field.");
				flag = true;
			}
			if (string.IsNullOrEmpty(Simple_name.Text))
			{
				m_errors.SetError(Simple_name, "Name is a required field.");
				flag = true;
			}
			TimeSpan? startOffset = null;
			if (!string.IsNullOrEmpty(m_initial_sec.Text))
			{
				if (int.TryParse(m_initial_sec.Text, out var result))
				{
					startOffset = new TimeSpan(0, 0, result);
				}
				else
				{
					m_errors.SetError(m_initial_sec, "Initial Offset is not a valid integer value.");
					flag = true;
				}
			}
			if (string.IsNullOrEmpty(m_recurrance_sec.Text))
			{
				m_errors.SetError(m_recurrance_sec, "Recurrance is a required field.");
				flag = true;
			}
			if (flag)
			{
				return;
			}
			TimeSpan repeatInterval = new TimeSpan(0, 0, int.Parse(m_recurrance_sec.Text));
			string payload = m_path.Text.Escaped() + " " + m_args_basic.Text.Escaped();
			errorList.AddRange(service.ScheduleSimpleJob(Simple_name.Text, payload, PayloadType.Shell, startOffset, null, null, repeatInterval));
		}
		else
		{
			TimeSpan? startOffset2 = null;
			if (!string.IsNullOrEmpty(m_initial_sec_adv.Text))
			{
				if (int.TryParse(m_initial_sec_adv.Text, out var result2))
				{
					startOffset2 = new TimeSpan(0, 0, result2);
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
			if (string.IsNullOrEmpty(m_pathAdvanced.Text))
			{
				m_errors.SetError(m_pathAdvanced, "Path is a required field.");
				flag = true;
			}
			if (string.IsNullOrEmpty(m_adv_name.Text))
			{
				m_errors.SetError(m_adv_name, "Name is a required field.");
				flag = true;
			}
			if (flag)
			{
				return;
			}
			string payload2 = m_pathAdvanced.Text.Escaped() + " " + m_arguments_adv.Text.Escaped();
			errorList.AddRange(service.ScheduleCronJob(m_adv_name.Text, payload2, PayloadType.Shell, startOffset2, null, null, m_cron.Text));
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.UpdateManager.UI.NewShellEvent));
		this.m_errors = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_cancel = new System.Windows.Forms.Button();
		this.m_submit = new System.Windows.Forms.Button();
		this.tabControl1 = new System.Windows.Forms.TabControl();
		this.BasicPage = new System.Windows.Forms.TabPage();
		this.m_args_basic = new System.Windows.Forms.TextBox();
		this.label7 = new System.Windows.Forms.Label();
		this.Simple_name = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_recurrance_sec = new System.Windows.Forms.TextBox();
		this.m_initial_sec = new System.Windows.Forms.TextBox();
		this.m_path = new System.Windows.Forms.TextBox();
		this.m_recurrenceLabel = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.scriptlocationlabel = new System.Windows.Forms.Label();
		this.AdvancedPage = new System.Windows.Forms.TabPage();
		this.m_arguments_adv = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.m_adv_name = new System.Windows.Forms.TextBox();
		this.Instructions = new System.Windows.Forms.TextBox();
		this.m_cron = new System.Windows.Forms.TextBox();
		this.m_pathAdvanced = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.m_initial_sec_adv = new System.Windows.Forms.TextBox();
		this.label8 = new System.Windows.Forms.Label();
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
		this.tabControl1.Size = new System.Drawing.Size(434, 265);
		this.tabControl1.TabIndex = 14;
		this.BasicPage.Controls.Add(this.m_args_basic);
		this.BasicPage.Controls.Add(this.label7);
		this.BasicPage.Controls.Add(this.Simple_name);
		this.BasicPage.Controls.Add(this.label1);
		this.BasicPage.Controls.Add(this.m_recurrance_sec);
		this.BasicPage.Controls.Add(this.m_initial_sec);
		this.BasicPage.Controls.Add(this.m_path);
		this.BasicPage.Controls.Add(this.m_recurrenceLabel);
		this.BasicPage.Controls.Add(this.label2);
		this.BasicPage.Controls.Add(this.scriptlocationlabel);
		this.BasicPage.Location = new System.Drawing.Point(4, 22);
		this.BasicPage.Name = "BasicPage";
		this.BasicPage.Padding = new System.Windows.Forms.Padding(3);
		this.BasicPage.Size = new System.Drawing.Size(426, 239);
		this.BasicPage.TabIndex = 0;
		this.BasicPage.Text = "Basic";
		this.BasicPage.UseVisualStyleBackColor = true;
		this.m_args_basic.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this.m_args_basic.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
		this.m_args_basic.Location = new System.Drawing.Point(119, 61);
		this.m_args_basic.Name = "m_args_basic";
		this.m_args_basic.Size = new System.Drawing.Size(297, 20);
		this.m_args_basic.TabIndex = 3;
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(6, 64);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(60, 13);
		this.label7.TabIndex = 26;
		this.label7.Text = "Arguments:";
		this.Simple_name.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this.Simple_name.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
		this.Simple_name.Location = new System.Drawing.Point(119, 6);
		this.Simple_name.Name = "Simple_name";
		this.Simple_name.Size = new System.Drawing.Size(186, 20);
		this.Simple_name.TabIndex = 1;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(6, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(38, 13);
		this.label1.TabIndex = 24;
		this.label1.Text = "Name:";
		this.m_recurrance_sec.Location = new System.Drawing.Point(119, 113);
		this.m_recurrance_sec.Name = "m_recurrance_sec";
		this.m_recurrance_sec.Size = new System.Drawing.Size(92, 20);
		this.m_recurrance_sec.TabIndex = 5;
		this.m_initial_sec.Location = new System.Drawing.Point(119, 87);
		this.m_initial_sec.Name = "m_initial_sec";
		this.m_initial_sec.Size = new System.Drawing.Size(92, 20);
		this.m_initial_sec.TabIndex = 4;
		this.m_path.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this.m_path.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
		this.m_path.Location = new System.Drawing.Point(119, 35);
		this.m_path.Name = "m_path";
		this.m_path.Size = new System.Drawing.Size(297, 20);
		this.m_path.TabIndex = 2;
		this.m_recurrenceLabel.AutoSize = true;
		this.m_recurrenceLabel.Location = new System.Drawing.Point(6, 116);
		this.m_recurrenceLabel.Name = "m_recurrenceLabel";
		this.m_recurrenceLabel.Size = new System.Drawing.Size(92, 13);
		this.m_recurrenceLabel.TabIndex = 18;
		this.m_recurrenceLabel.Text = "Recurrence: (sec)";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(6, 90);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(91, 13);
		this.label2.TabIndex = 16;
		this.label2.Text = "Initial Offset: (sec)";
		this.scriptlocationlabel.AutoSize = true;
		this.scriptlocationlabel.Location = new System.Drawing.Point(6, 38);
		this.scriptlocationlabel.Name = "scriptlocationlabel";
		this.scriptlocationlabel.Size = new System.Drawing.Size(32, 13);
		this.scriptlocationlabel.TabIndex = 12;
		this.scriptlocationlabel.Text = "Path:";
		this.AdvancedPage.Controls.Add(this.m_initial_sec_adv);
		this.AdvancedPage.Controls.Add(this.label8);
		this.AdvancedPage.Controls.Add(this.m_arguments_adv);
		this.AdvancedPage.Controls.Add(this.label6);
		this.AdvancedPage.Controls.Add(this.label4);
		this.AdvancedPage.Controls.Add(this.m_adv_name);
		this.AdvancedPage.Controls.Add(this.Instructions);
		this.AdvancedPage.Controls.Add(this.m_cron);
		this.AdvancedPage.Controls.Add(this.m_pathAdvanced);
		this.AdvancedPage.Controls.Add(this.label5);
		this.AdvancedPage.Controls.Add(this.label3);
		this.AdvancedPage.Location = new System.Drawing.Point(4, 22);
		this.AdvancedPage.Name = "AdvancedPage";
		this.AdvancedPage.Padding = new System.Windows.Forms.Padding(3);
		this.AdvancedPage.Size = new System.Drawing.Size(426, 239);
		this.AdvancedPage.TabIndex = 1;
		this.AdvancedPage.Text = "Advanced";
		this.AdvancedPage.UseVisualStyleBackColor = true;
		this.m_arguments_adv.Location = new System.Drawing.Point(113, 63);
		this.m_arguments_adv.Name = "m_arguments_adv";
		this.m_arguments_adv.Size = new System.Drawing.Size(268, 20);
		this.m_arguments_adv.TabIndex = 3;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(21, 66);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(60, 13);
		this.label6.TabIndex = 7;
		this.label6.Text = "Arguments:";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(21, 10);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(38, 13);
		this.label4.TabIndex = 6;
		this.label4.Text = "Name:";
		this.m_adv_name.Location = new System.Drawing.Point(113, 7);
		this.m_adv_name.Name = "m_adv_name";
		this.m_adv_name.Size = new System.Drawing.Size(268, 20);
		this.m_adv_name.TabIndex = 1;
		this.Instructions.BackColor = System.Drawing.SystemColors.Control;
		this.Instructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.Instructions.Cursor = System.Windows.Forms.Cursors.Default;
		this.Instructions.Font = new System.Drawing.Font("Lucida Console", 7f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Instructions.Location = new System.Drawing.Point(118, 145);
		this.Instructions.Multiline = true;
		this.Instructions.Name = "Instructions";
		this.Instructions.ReadOnly = true;
		this.Instructions.Size = new System.Drawing.Size(263, 88);
		this.Instructions.TabIndex = 4;
		this.Instructions.Text = resources.GetString("Instructions.Text");
		this.m_cron.Location = new System.Drawing.Point(113, 119);
		this.m_cron.Name = "m_cron";
		this.m_cron.Size = new System.Drawing.Size(268, 20);
		this.m_cron.TabIndex = 5;
		this.m_pathAdvanced.Location = new System.Drawing.Point(113, 35);
		this.m_pathAdvanced.Name = "m_pathAdvanced";
		this.m_pathAdvanced.Size = new System.Drawing.Size(268, 20);
		this.m_pathAdvanced.TabIndex = 2;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(21, 122);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(86, 13);
		this.label5.TabIndex = 1;
		this.label5.Text = "Cron Expression:";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(21, 38);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(32, 13);
		this.label3.TabIndex = 0;
		this.label3.Text = "Path:";
		this.m_initial_sec_adv.Location = new System.Drawing.Point(114, 91);
		this.m_initial_sec_adv.Name = "m_initial_sec_adv";
		this.m_initial_sec_adv.Size = new System.Drawing.Size(92, 20);
		this.m_initial_sec_adv.TabIndex = 4;
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(21, 94);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(91, 13);
		this.label8.TabIndex = 18;
		this.label8.Text = "Initial Offset: (sec)";
		base.AcceptButton = this.m_submit;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancel;
		base.ClientSize = new System.Drawing.Size(537, 286);
		base.Controls.Add(this.tabControl1);
		base.Controls.Add(this.m_cancel);
		base.Controls.Add(this.m_submit);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "NewShellEvent";
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
