using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.UI;

public class NewScriptEvent : Form
{
	private IContainer components;

	private Button m_submit;

	private Button m_cancel;

	private ErrorProvider m_errors;

	private TabControl m_newScriptTabControl;

	private TabPage BasicPage;

	private TextBox m_basic_path;

	private Label m_recurrenceLabel;

	private Label label2;

	private Label scriptlocationlabel;

	private TabPage AdvancedPage;

	private Label label5;

	private Label label3;

	private TextBox Instructions;

	private TextBox m_advanced_cronExpression;

	private TextBox m_advanced_path;

	private TextBox m_basic_recurrence;

	private TextBox m_basic_initial;

	private TextBox m_basic_name;

	private Label label1;

	private Label label4;

	private TextBox m_advanced_name;

	private TextBox m_advanced_initial;

	private Label label6;

	public NewScriptEvent()
	{
		InitializeComponent();
	}

	private void OnSubmitClick(object sender, EventArgs e)
	{
		m_errors.SetError(m_basic_path, string.Empty);
		m_errors.SetError(m_basic_name, string.Empty);
		m_errors.SetError(m_basic_initial, string.Empty);
		m_errors.SetError(m_basic_recurrence, string.Empty);
		m_errors.SetError(m_advanced_initial, string.Empty);
		m_errors.SetError(m_advanced_cronExpression, string.Empty);
		m_errors.SetError(m_advanced_path, string.Empty);
		m_errors.SetError(m_advanced_name, string.Empty);
		bool flag = false;
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		ErrorList errorList = new ErrorList();
		if (m_newScriptTabControl.SelectedTab == BasicPage)
		{
			if (string.IsNullOrEmpty(m_basic_path.Text))
			{
				m_errors.SetError(m_basic_path, "Path is a required field.");
				flag = true;
			}
			if (string.IsNullOrEmpty(m_basic_name.Text))
			{
				m_errors.SetError(m_basic_name, "Name is a required field.");
				flag = true;
			}
			TimeSpan? startOffset = null;
			if (!string.IsNullOrEmpty(m_basic_initial.Text))
			{
				if (int.TryParse(m_basic_initial.Text, out var result))
				{
					startOffset = new TimeSpan(0, 0, result);
				}
				else
				{
					m_errors.SetError(m_basic_initial, "Initial Offset is not a valid integer value.");
					flag = true;
				}
			}
			if (string.IsNullOrEmpty(m_basic_recurrence.Text))
			{
				m_errors.SetError(m_basic_recurrence, "Recurrance is a required field.");
				flag = true;
			}
			if (flag)
			{
				return;
			}
			errorList.AddRange(service.ScheduleSimpleJob(repeatInterval: new TimeSpan(0, 0, int.Parse(m_basic_recurrence.Text)), name: m_basic_name.Text, payload: m_basic_path.Text.Escaped(), payloadType: PayloadType.Script, startOffset: startOffset, startTime: null, endTime: null));
		}
		else
		{
			if (string.IsNullOrEmpty(m_advanced_cronExpression.Text))
			{
				m_errors.SetError(m_advanced_cronExpression, "Expression is a required field.");
				flag = true;
			}
			TimeSpan? startOffset2 = null;
			if (!string.IsNullOrEmpty(m_advanced_initial.Text))
			{
				if (int.TryParse(m_advanced_initial.Text, out var result2))
				{
					startOffset2 = new TimeSpan(0, 0, result2);
				}
				else
				{
					m_errors.SetError(m_advanced_initial, "Initial Offset is not a valid integer value.");
					flag = true;
				}
			}
			if (string.IsNullOrEmpty(m_advanced_path.Text))
			{
				m_errors.SetError(m_advanced_path, "Path is a required field.");
				flag = true;
			}
			if (string.IsNullOrEmpty(m_advanced_name.Text))
			{
				m_errors.SetError(m_advanced_name, "Name is a required field.");
				flag = true;
			}
			if (flag)
			{
				return;
			}
			errorList.AddRange(service.ScheduleCronJob(m_advanced_name.Text, m_advanced_path.Text.Escaped(), PayloadType.Script, startOffset2, null, null, m_advanced_cronExpression.Text));
		}
		if (errorList.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errorList;
			errorForm.ShowDialog();
		}
		Close();
	}

	private void OnCancel(object sender, EventArgs e)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.UpdateManager.UI.NewScriptEvent));
		this.m_submit = new System.Windows.Forms.Button();
		this.m_cancel = new System.Windows.Forms.Button();
		this.m_errors = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_newScriptTabControl = new System.Windows.Forms.TabControl();
		this.BasicPage = new System.Windows.Forms.TabPage();
		this.m_basic_name = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_basic_recurrence = new System.Windows.Forms.TextBox();
		this.m_basic_initial = new System.Windows.Forms.TextBox();
		this.m_basic_path = new System.Windows.Forms.TextBox();
		this.m_recurrenceLabel = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.scriptlocationlabel = new System.Windows.Forms.Label();
		this.AdvancedPage = new System.Windows.Forms.TabPage();
		this.label4 = new System.Windows.Forms.Label();
		this.m_advanced_name = new System.Windows.Forms.TextBox();
		this.Instructions = new System.Windows.Forms.TextBox();
		this.m_advanced_cronExpression = new System.Windows.Forms.TextBox();
		this.m_advanced_path = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.m_advanced_initial = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errors).BeginInit();
		this.m_newScriptTabControl.SuspendLayout();
		this.BasicPage.SuspendLayout();
		this.AdvancedPage.SuspendLayout();
		base.SuspendLayout();
		this.m_submit.Location = new System.Drawing.Point(452, 12);
		this.m_submit.Name = "m_submit";
		this.m_submit.Size = new System.Drawing.Size(75, 23);
		this.m_submit.TabIndex = 5;
		this.m_submit.Text = "OK";
		this.m_submit.UseVisualStyleBackColor = true;
		this.m_submit.Click += new System.EventHandler(OnSubmitClick);
		this.m_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancel.Location = new System.Drawing.Point(452, 41);
		this.m_cancel.Name = "m_cancel";
		this.m_cancel.Size = new System.Drawing.Size(75, 23);
		this.m_cancel.TabIndex = 6;
		this.m_cancel.Text = "Cancel";
		this.m_cancel.UseVisualStyleBackColor = true;
		this.m_cancel.Click += new System.EventHandler(OnCancel);
		this.m_errors.ContainerControl = this;
		this.m_newScriptTabControl.Controls.Add(this.BasicPage);
		this.m_newScriptTabControl.Controls.Add(this.AdvancedPage);
		this.m_newScriptTabControl.Location = new System.Drawing.Point(12, 12);
		this.m_newScriptTabControl.Name = "m_newScriptTabControl";
		this.m_newScriptTabControl.SelectedIndex = 0;
		this.m_newScriptTabControl.Size = new System.Drawing.Size(434, 234);
		this.m_newScriptTabControl.TabIndex = 12;
		this.BasicPage.Controls.Add(this.m_basic_name);
		this.BasicPage.Controls.Add(this.label1);
		this.BasicPage.Controls.Add(this.m_basic_recurrence);
		this.BasicPage.Controls.Add(this.m_basic_initial);
		this.BasicPage.Controls.Add(this.m_basic_path);
		this.BasicPage.Controls.Add(this.m_recurrenceLabel);
		this.BasicPage.Controls.Add(this.label2);
		this.BasicPage.Controls.Add(this.scriptlocationlabel);
		this.BasicPage.Location = new System.Drawing.Point(4, 22);
		this.BasicPage.Name = "BasicPage";
		this.BasicPage.Padding = new System.Windows.Forms.Padding(3);
		this.BasicPage.Size = new System.Drawing.Size(426, 208);
		this.BasicPage.TabIndex = 0;
		this.BasicPage.Text = "Basic";
		this.BasicPage.UseVisualStyleBackColor = true;
		this.m_basic_name.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this.m_basic_name.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
		this.m_basic_name.Location = new System.Drawing.Point(119, 36);
		this.m_basic_name.Name = "m_basic_name";
		this.m_basic_name.Size = new System.Drawing.Size(186, 20);
		this.m_basic_name.TabIndex = 2;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(6, 39);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(38, 13);
		this.label1.TabIndex = 24;
		this.label1.Text = "Name:";
		this.m_basic_recurrence.Location = new System.Drawing.Point(119, 88);
		this.m_basic_recurrence.Name = "m_basic_recurrence";
		this.m_basic_recurrence.Size = new System.Drawing.Size(92, 20);
		this.m_basic_recurrence.TabIndex = 4;
		this.m_basic_initial.Location = new System.Drawing.Point(119, 62);
		this.m_basic_initial.Name = "m_basic_initial";
		this.m_basic_initial.Size = new System.Drawing.Size(92, 20);
		this.m_basic_initial.TabIndex = 3;
		this.m_basic_path.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this.m_basic_path.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
		this.m_basic_path.Location = new System.Drawing.Point(119, 10);
		this.m_basic_path.Name = "m_basic_path";
		this.m_basic_path.Size = new System.Drawing.Size(297, 20);
		this.m_basic_path.TabIndex = 1;
		this.m_recurrenceLabel.AutoSize = true;
		this.m_recurrenceLabel.Location = new System.Drawing.Point(6, 91);
		this.m_recurrenceLabel.Name = "m_recurrenceLabel";
		this.m_recurrenceLabel.Size = new System.Drawing.Size(92, 13);
		this.m_recurrenceLabel.TabIndex = 18;
		this.m_recurrenceLabel.Text = "Recurrence: (sec)";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(6, 65);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(91, 13);
		this.label2.TabIndex = 16;
		this.label2.Text = "Initial Offset: (sec)";
		this.scriptlocationlabel.AutoSize = true;
		this.scriptlocationlabel.Location = new System.Drawing.Point(6, 13);
		this.scriptlocationlabel.Name = "scriptlocationlabel";
		this.scriptlocationlabel.Size = new System.Drawing.Size(32, 13);
		this.scriptlocationlabel.TabIndex = 12;
		this.scriptlocationlabel.Text = "Path:";
		this.AdvancedPage.Controls.Add(this.m_advanced_initial);
		this.AdvancedPage.Controls.Add(this.label6);
		this.AdvancedPage.Controls.Add(this.label4);
		this.AdvancedPage.Controls.Add(this.m_advanced_name);
		this.AdvancedPage.Controls.Add(this.Instructions);
		this.AdvancedPage.Controls.Add(this.m_advanced_cronExpression);
		this.AdvancedPage.Controls.Add(this.m_advanced_path);
		this.AdvancedPage.Controls.Add(this.label5);
		this.AdvancedPage.Controls.Add(this.label3);
		this.AdvancedPage.Location = new System.Drawing.Point(4, 22);
		this.AdvancedPage.Name = "AdvancedPage";
		this.AdvancedPage.Padding = new System.Windows.Forms.Padding(3);
		this.AdvancedPage.Size = new System.Drawing.Size(426, 208);
		this.AdvancedPage.TabIndex = 1;
		this.AdvancedPage.Text = "Advanced";
		this.AdvancedPage.UseVisualStyleBackColor = true;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(14, 10);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(38, 13);
		this.label4.TabIndex = 6;
		this.label4.Text = "Name:";
		this.m_advanced_name.Location = new System.Drawing.Point(106, 7);
		this.m_advanced_name.Name = "m_advanced_name";
		this.m_advanced_name.Size = new System.Drawing.Size(268, 20);
		this.m_advanced_name.TabIndex = 1;
		this.Instructions.BackColor = System.Drawing.SystemColors.Control;
		this.Instructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.Instructions.Cursor = System.Windows.Forms.Cursors.Default;
		this.Instructions.Font = new System.Drawing.Font("Lucida Console", 7f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Instructions.Location = new System.Drawing.Point(111, 114);
		this.Instructions.Multiline = true;
		this.Instructions.Name = "Instructions";
		this.Instructions.ReadOnly = true;
		this.Instructions.Size = new System.Drawing.Size(312, 90);
		this.Instructions.TabIndex = 4;
		this.Instructions.Text = resources.GetString("Instructions.Text");
		this.m_advanced_cronExpression.Location = new System.Drawing.Point(106, 88);
		this.m_advanced_cronExpression.Name = "m_advanced_cronExpression";
		this.m_advanced_cronExpression.Size = new System.Drawing.Size(268, 20);
		this.m_advanced_cronExpression.TabIndex = 4;
		this.m_advanced_path.Location = new System.Drawing.Point(106, 34);
		this.m_advanced_path.Name = "m_advanced_path";
		this.m_advanced_path.Size = new System.Drawing.Size(268, 20);
		this.m_advanced_path.TabIndex = 2;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(14, 91);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(86, 13);
		this.label5.TabIndex = 1;
		this.label5.Text = "Cron Expression:";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(14, 37);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(32, 13);
		this.label3.TabIndex = 0;
		this.label3.Text = "Path:";
		this.m_advanced_initial.Location = new System.Drawing.Point(106, 61);
		this.m_advanced_initial.Name = "m_advanced_initial";
		this.m_advanced_initial.Size = new System.Drawing.Size(92, 20);
		this.m_advanced_initial.TabIndex = 3;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(14, 64);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(91, 13);
		this.label6.TabIndex = 18;
		this.label6.Text = "Initial Offset: (sec)";
		base.AcceptButton = this.m_submit;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancel;
		base.ClientSize = new System.Drawing.Size(540, 258);
		base.Controls.Add(this.m_newScriptTabControl);
		base.Controls.Add(this.m_cancel);
		base.Controls.Add(this.m_submit);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "NewScriptEvent";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Create New Script Event";
		((System.ComponentModel.ISupportInitialize)this.m_errors).EndInit();
		this.m_newScriptTabControl.ResumeLayout(false);
		this.BasicPage.ResumeLayout(false);
		this.BasicPage.PerformLayout();
		this.AdvancedPage.ResumeLayout(false);
		this.AdvancedPage.PerformLayout();
		base.ResumeLayout(false);
	}
}
