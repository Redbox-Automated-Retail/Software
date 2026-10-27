using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.UI;

public class TransferDetails : Form
{
	private readonly ErrorList m_errors;

	private IContainer components;

	private Label label1;

	private Label label2;

	private Label m_labelTotalBytes;

	private Label label4;

	private ProgressBar m_downloadProgress;

	private Label m_totalBytes;

	private Label m_statusDisplay;

	private Label label5;

	private Label label3;

	private Label m_startDate;

	private Label label9;

	private Label m_finishDate;

	private Label label10;

	private Button m_showErrors;

	private Label m_localFile;

	private Label label100;

	private Label label11;

	private ToolTip m_longUrl;

	private TextBox m_urlDisplay;

	private TextBox m_id;

	private TextBox m_name;

	public TransferDetails(ITransferJob job)
	{
		InitializeComponent();
		job.GetErrors(out var errors);
		m_showErrors.Visible = errors.ContainsError();
		m_errors = errors;
		m_name.Text = job.Name;
		m_id.Text = job.ID.ToString();
		m_statusDisplay.Text = job.Status.ToString("G");
		if (job.Status != TransferStatus.Error)
		{
			m_downloadProgress.Maximum = (int)(job.TotalBytes / 1048576);
			m_downloadProgress.Value = (int)(job.TotalBytesTransfered / 1048576);
		}
		m_totalBytes.Text = ((job.TotalBytes == ulong.MaxValue) ? "N/A" : job.TotalBytes.FormatAsByteSize());
		m_startDate.Text = job.StartTime.ToLongTimeString();
		m_finishDate.Text = (job.FinishTime.HasValue ? job.FinishTime.Value.ToLongTimeString() : "Job is still running.");
		m_errors.AddRange(job.GetItems(out var items));
		ITransferItem transferItem = ((items.Count > 0) ? items[0] : null);
		if (transferItem != null)
		{
			if (transferItem.RemoteURL.Length > 80)
			{
				m_longUrl.SetToolTip(m_urlDisplay, transferItem.RemoteURL);
				m_urlDisplay.Text = transferItem.RemoteURL.Substring(0, 80) + ".....";
			}
			else
			{
				m_urlDisplay.Text = transferItem.RemoteURL;
			}
			m_localFile.Text = Path.Combine(transferItem.Path, transferItem.Name);
		}
	}

	private void OnShowErrors(object sender, EventArgs e)
	{
		ErrorForm errorForm = new ErrorForm();
		errorForm.Errors = m_errors;
		errorForm.ShowDialog();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.UpdateManager.UI.TransferDetails));
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.m_labelTotalBytes = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.m_downloadProgress = new System.Windows.Forms.ProgressBar();
		this.m_totalBytes = new System.Windows.Forms.Label();
		this.m_statusDisplay = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.m_startDate = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.m_finishDate = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.m_showErrors = new System.Windows.Forms.Button();
		this.m_localFile = new System.Windows.Forms.Label();
		this.label100 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.m_longUrl = new System.Windows.Forms.ToolTip(this.components);
		this.m_urlDisplay = new System.Windows.Forms.TextBox();
		this.m_id = new System.Windows.Forms.TextBox();
		this.m_name = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.Location = new System.Drawing.Point(49, 13);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(42, 15);
		this.label1.TabIndex = 0;
		this.label1.Text = "Name:";
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.Location = new System.Drawing.Point(63, 41);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(28, 15);
		this.label2.TabIndex = 1;
		this.label2.Text = "ID:";
		this.m_labelTotalBytes.AutoSize = true;
		this.m_labelTotalBytes.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_labelTotalBytes.Location = new System.Drawing.Point(564, 13);
		this.m_labelTotalBytes.Name = "m_labelTotalBytes";
		this.m_labelTotalBytes.Size = new System.Drawing.Size(119, 15);
		this.m_labelTotalBytes.TabIndex = 2;
		this.m_labelTotalBytes.Text = "Total Megabytes:";
		this.label4.AutoSize = true;
		this.label4.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label4.Location = new System.Drawing.Point(262, 13);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(14, 15);
		this.label4.TabIndex = 3;
		this.label4.Text = " ";
		this.m_downloadProgress.Location = new System.Drawing.Point(649, 131);
		this.m_downloadProgress.Name = "m_downloadProgress";
		this.m_downloadProgress.Size = new System.Drawing.Size(162, 23);
		this.m_downloadProgress.TabIndex = 4;
		this.m_totalBytes.AutoSize = true;
		this.m_totalBytes.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_totalBytes.Location = new System.Drawing.Point(696, 13);
		this.m_totalBytes.Name = "m_totalBytes";
		this.m_totalBytes.Size = new System.Drawing.Size(98, 15);
		this.m_totalBytes.TabIndex = 5;
		this.m_totalBytes.Text = "<total bytes>";
		this.m_statusDisplay.AutoSize = true;
		this.m_statusDisplay.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_statusDisplay.Location = new System.Drawing.Point(296, 125);
		this.m_statusDisplay.Name = "m_statusDisplay";
		this.m_statusDisplay.Size = new System.Drawing.Size(63, 15);
		this.m_statusDisplay.TabIndex = 9;
		this.m_statusDisplay.Text = "<status>";
		this.m_statusDisplay.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label5.AutoSize = true;
		this.label5.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label5.Location = new System.Drawing.Point(232, 125);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(56, 15);
		this.label5.TabIndex = 8;
		this.label5.Text = "Status:";
		this.label3.AutoSize = true;
		this.label3.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label3.Location = new System.Drawing.Point(510, 134);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(133, 15);
		this.label3.TabIndex = 10;
		this.label3.Text = "Transfer Progress:";
		this.m_startDate.AutoSize = true;
		this.m_startDate.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_startDate.Location = new System.Drawing.Point(99, 97);
		this.m_startDate.Name = "m_startDate";
		this.m_startDate.Size = new System.Drawing.Size(84, 15);
		this.m_startDate.TabIndex = 13;
		this.m_startDate.Text = "<startdate>";
		this.label9.AutoSize = true;
		this.label9.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label9.Location = new System.Drawing.Point(7, 97);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(84, 15);
		this.label9.TabIndex = 11;
		this.label9.Text = "Start Date:";
		this.m_finishDate.AutoSize = true;
		this.m_finishDate.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_finishDate.Location = new System.Drawing.Point(99, 125);
		this.m_finishDate.Name = "m_finishDate";
		this.m_finishDate.Size = new System.Drawing.Size(91, 15);
		this.m_finishDate.TabIndex = 16;
		this.m_finishDate.Text = "<finishdate>";
		this.label10.AutoSize = true;
		this.label10.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label10.Location = new System.Drawing.Point(0, 125);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(91, 15);
		this.label10.TabIndex = 15;
		this.label10.Text = "Finish Date:";
		this.m_showErrors.Location = new System.Drawing.Point(407, 131);
		this.m_showErrors.Name = "m_showErrors";
		this.m_showErrors.Size = new System.Drawing.Size(97, 23);
		this.m_showErrors.TabIndex = 1;
		this.m_showErrors.Text = "Show Errors";
		this.m_showErrors.UseVisualStyleBackColor = true;
		this.m_showErrors.Click += new System.EventHandler(OnShowErrors);
		this.m_localFile.AutoSize = true;
		this.m_localFile.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_localFile.Location = new System.Drawing.Point(303, 97);
		this.m_localFile.Name = "m_localFile";
		this.m_localFile.Size = new System.Drawing.Size(35, 15);
		this.m_localFile.TabIndex = 21;
		this.m_localFile.Text = "<id>";
		this.label100.AutoSize = true;
		this.label100.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label100.Location = new System.Drawing.Point(204, 97);
		this.label100.Name = "label100";
		this.label100.Size = new System.Drawing.Size(84, 15);
		this.label100.TabIndex = 19;
		this.label100.Text = "Local File:";
		this.label11.AutoSize = true;
		this.label11.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label11.Location = new System.Drawing.Point(49, 69);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(35, 15);
		this.label11.TabIndex = 18;
		this.label11.Text = "URL:";
		this.m_longUrl.Tag = "m_urlDisplay";
		this.m_urlDisplay.BackColor = System.Drawing.SystemColors.Control;
		this.m_urlDisplay.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_urlDisplay.Location = new System.Drawing.Point(102, 69);
		this.m_urlDisplay.Name = "m_urlDisplay";
		this.m_urlDisplay.Size = new System.Drawing.Size(692, 16);
		this.m_urlDisplay.TabIndex = 22;
		this.m_id.BackColor = System.Drawing.SystemColors.Control;
		this.m_id.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_id.Location = new System.Drawing.Point(97, 40);
		this.m_id.Name = "m_id";
		this.m_id.Size = new System.Drawing.Size(692, 16);
		this.m_id.TabIndex = 23;
		this.m_name.BackColor = System.Drawing.SystemColors.Control;
		this.m_name.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_name.Location = new System.Drawing.Point(97, 12);
		this.m_name.Name = "m_name";
		this.m_name.Size = new System.Drawing.Size(461, 16);
		this.m_name.TabIndex = 24;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(814, 166);
		base.Controls.Add(this.m_name);
		base.Controls.Add(this.m_id);
		base.Controls.Add(this.m_urlDisplay);
		base.Controls.Add(this.m_localFile);
		base.Controls.Add(this.label100);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.m_showErrors);
		base.Controls.Add(this.m_finishDate);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.m_startDate);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_statusDisplay);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.m_totalBytes);
		base.Controls.Add(this.m_downloadProgress);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_labelTotalBytes);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		this.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "TransferDetails";
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Details";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
