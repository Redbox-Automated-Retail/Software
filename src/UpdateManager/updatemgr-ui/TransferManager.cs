using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.UI;

public class TransferManager : Form
{
	private IContainer components;

	private Button m_cancel;

	private Button m_suspend;

	private Button m_details;

	private Button m_resume;

	private ListView m_jobs;

	private ColumnHeader JobName;

	private ColumnHeader ID;

	private ColumnHeader Status;

	private ColumnHeader JobType;

	private ColumnHeader StartTime;

	private ColumnHeader TotalBytes;

	private Button m_refresh;

	private ColumnHeader m_bytesTransfered;

	public TransferManager()
	{
		InitializeComponent();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		UpdateManagerApplication.Instance.AsyncButtonAction(null, LoadJobsIntoList);
	}

	private void OnSuspend(object sender, EventArgs e)
	{
		List<ITransferJob> jobs = new List<ITransferJob>();
		ErrorList errors = new ErrorList();
		foreach (object selectedItem in m_jobs.SelectedItems)
		{
			if (selectedItem is ListViewItem { Tag: ITransferJob tag })
			{
				jobs.Add(tag);
			}
		}
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			foreach (ITransferJob item in jobs)
			{
				errors.AddRange(item.Suspend());
				if (errors.ContainsError())
				{
					UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errors);
				}
			}
			LoadJobsIntoList();
		});
	}

	private void OnCancel(object sender, EventArgs e)
	{
		List<ITransferJob> jobs = new List<ITransferJob>();
		ErrorList errors = new ErrorList();
		foreach (object selectedItem in m_jobs.SelectedItems)
		{
			if (selectedItem is ListViewItem { Tag: ITransferJob tag })
			{
				jobs.Add(tag);
			}
		}
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			foreach (ITransferJob item in jobs)
			{
				errors.AddRange(item.Cancel());
				if (errors.ContainsError())
				{
					UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errors);
				}
			}
			LoadJobsIntoList();
		});
	}

	private void OnResume(object sender, EventArgs e)
	{
		List<ITransferJob> jobs = new List<ITransferJob>();
		ErrorList errors = new ErrorList();
		foreach (object selectedItem in m_jobs.SelectedItems)
		{
			if (selectedItem is ListViewItem { Tag: ITransferJob tag })
			{
				jobs.Add(tag);
			}
		}
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			foreach (ITransferJob item in jobs)
			{
				errors.AddRange(item.Resume());
				if (errors.ContainsError())
				{
					UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errors);
				}
			}
			LoadJobsIntoList();
		});
	}

	private void OnDetails(object sender, EventArgs e)
	{
		if (m_jobs.SelectedItems.Count >= 1)
		{
			new TransferDetails(m_jobs.SelectedItems[0].Tag as ITransferJob).ShowDialog();
		}
	}

	private void LoadJobsIntoList()
	{
		ErrorList jobs = ServiceLocator.Instance.GetService<ITransferService>().GetJobs(out var jobs2, allUsers: true);
		if (jobs.ContainsError())
		{
			UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(jobs);
			return;
		}
		UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
		{
			m_jobs.SuspendLayout();
			m_jobs.BeginUpdate();
			m_jobs.Items.Clear();
			foreach (ITransferJob item in jobs2)
			{
				ListViewItem listViewItem = new ListViewItem
				{
					Text = item.Name,
					Tag = item
				};
				listViewItem.SubItems.Add(item.ID.ToString());
				listViewItem.SubItems.Add(item.Status.ToString("G"));
				listViewItem.SubItems.Add(item.JobType.ToString("G"));
				listViewItem.SubItems.Add(item.StartTime.ToLongTimeString());
				listViewItem.SubItems.Add((item.TotalBytes == ulong.MaxValue) ? "N/A" : item.TotalBytes.FormatAsByteSize());
				listViewItem.SubItems.Add((item.TotalBytesTransfered == ulong.MaxValue) ? "N/A" : item.TotalBytesTransfered.FormatAsByteSize());
				m_jobs.Items.Add(listViewItem);
			}
			m_jobs.EndUpdate();
			m_jobs.ResumeLayout();
		});
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, LoadJobsIntoList);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.UpdateManager.UI.TransferManager));
		this.m_cancel = new System.Windows.Forms.Button();
		this.m_suspend = new System.Windows.Forms.Button();
		this.m_details = new System.Windows.Forms.Button();
		this.m_resume = new System.Windows.Forms.Button();
		this.m_jobs = new System.Windows.Forms.ListView();
		this.JobName = new System.Windows.Forms.ColumnHeader();
		this.ID = new System.Windows.Forms.ColumnHeader();
		this.Status = new System.Windows.Forms.ColumnHeader();
		this.JobType = new System.Windows.Forms.ColumnHeader();
		this.StartTime = new System.Windows.Forms.ColumnHeader();
		this.TotalBytes = new System.Windows.Forms.ColumnHeader();
		this.m_refresh = new System.Windows.Forms.Button();
		this.m_bytesTransfered = new System.Windows.Forms.ColumnHeader();
		base.SuspendLayout();
		this.m_cancel.Location = new System.Drawing.Point(174, 248);
		this.m_cancel.Name = "m_cancel";
		this.m_cancel.Size = new System.Drawing.Size(75, 23);
		this.m_cancel.TabIndex = 3;
		this.m_cancel.Text = "Cancel";
		this.m_cancel.UseVisualStyleBackColor = true;
		this.m_cancel.Click += new System.EventHandler(OnCancel);
		this.m_suspend.Location = new System.Drawing.Point(12, 248);
		this.m_suspend.Name = "m_suspend";
		this.m_suspend.Size = new System.Drawing.Size(75, 23);
		this.m_suspend.TabIndex = 1;
		this.m_suspend.Text = "Suspend";
		this.m_suspend.UseVisualStyleBackColor = true;
		this.m_suspend.Click += new System.EventHandler(OnSuspend);
		this.m_details.Location = new System.Drawing.Point(255, 248);
		this.m_details.Name = "m_details";
		this.m_details.Size = new System.Drawing.Size(75, 23);
		this.m_details.TabIndex = 4;
		this.m_details.Text = "Details";
		this.m_details.UseVisualStyleBackColor = true;
		this.m_details.Click += new System.EventHandler(OnDetails);
		this.m_resume.Location = new System.Drawing.Point(93, 248);
		this.m_resume.Name = "m_resume";
		this.m_resume.Size = new System.Drawing.Size(75, 23);
		this.m_resume.TabIndex = 2;
		this.m_resume.Text = "Resume";
		this.m_resume.UseVisualStyleBackColor = true;
		this.m_resume.Click += new System.EventHandler(OnResume);
		this.m_jobs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[7] { this.JobName, this.ID, this.Status, this.JobType, this.StartTime, this.TotalBytes, this.m_bytesTransfered });
		this.m_jobs.Font = new System.Drawing.Font("Consolas", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_jobs.FullRowSelect = true;
		this.m_jobs.Location = new System.Drawing.Point(7, 8);
		this.m_jobs.Name = "m_jobs";
		this.m_jobs.Size = new System.Drawing.Size(831, 230);
		this.m_jobs.TabIndex = 0;
		this.m_jobs.UseCompatibleStateImageBehavior = false;
		this.m_jobs.View = System.Windows.Forms.View.Details;
		this.JobName.Text = "Job Name";
		this.JobName.Width = 180;
		this.ID.Text = "ID";
		this.ID.Width = 220;
		this.Status.Text = "Status";
		this.Status.Width = 80;
		this.JobType.Text = "Job Type";
		this.JobType.Width = 70;
		this.StartTime.Text = "Start Time";
		this.StartTime.Width = 80;
		this.TotalBytes.Text = "Total Bytes";
		this.TotalBytes.Width = 80;
		this.m_refresh.Location = new System.Drawing.Point(758, 248);
		this.m_refresh.Name = "m_refresh";
		this.m_refresh.Size = new System.Drawing.Size(75, 23);
		this.m_refresh.TabIndex = 5;
		this.m_refresh.Text = "Refresh";
		this.m_refresh.UseVisualStyleBackColor = true;
		this.m_refresh.Click += new System.EventHandler(OnRefresh);
		this.m_bytesTransfered.Text = "Bytes Transfered";
		this.m_bytesTransfered.Width = 110;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(845, 281);
		base.Controls.Add(this.m_refresh);
		base.Controls.Add(this.m_jobs);
		base.Controls.Add(this.m_resume);
		base.Controls.Add(this.m_details);
		base.Controls.Add(this.m_suspend);
		base.Controls.Add(this.m_cancel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "TransferManager";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Manage Transfers";
		base.Load += new System.EventHandler(OnLoad);
		base.ResumeLayout(false);
	}
}
