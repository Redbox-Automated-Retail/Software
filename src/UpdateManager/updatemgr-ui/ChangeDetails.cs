using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.UI;

public class ChangeDetails : Form
{
	private readonly IRevLog m_log;

	private IContainer components;

	private ListView m_changes;

	private ColumnHeader m_name;

	private ColumnHeader m_path;

	private ColumnHeader m_targetHash;

	private ColumnHeader m_patchSignature;

	private ColumnHeader m_isComposite;

	private ColumnHeader m_isSeed;

	private Label hashLabel;

	private Label m_hash;

	private Label m_label;

	private Label labelLabel;

	public ChangeDetails(IRevLog log)
	{
		InitializeComponent();
		m_log = log;
	}

	private void LoadChageDetails(IRevLog log)
	{
		m_changes.BeginUpdate();
		m_hash.Text = log.Hash;
		m_label.Text = log.Label;
		foreach (ChangeItem change in log.Changes)
		{
			ListViewItem listViewItem = new ListViewItem
			{
				Text = change.TargetName,
				Tag = change
			};
			listViewItem.SubItems.Add(change.TargetPath);
			listViewItem.SubItems.Add(change.VersionHash);
			listViewItem.SubItems.Add(change.ContentHash);
			listViewItem.SubItems.Add(change.IsSeed.ToString());
			listViewItem.SubItems.Add(change.Composite.ToString());
			m_changes.Items.Add(listViewItem);
		}
		m_changes.EndUpdate();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		LoadChageDetails(m_log);
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
		this.m_changes = new System.Windows.Forms.ListView();
		this.m_name = new System.Windows.Forms.ColumnHeader();
		this.m_path = new System.Windows.Forms.ColumnHeader();
		this.m_targetHash = new System.Windows.Forms.ColumnHeader();
		this.m_patchSignature = new System.Windows.Forms.ColumnHeader();
		this.m_isComposite = new System.Windows.Forms.ColumnHeader();
		this.m_isSeed = new System.Windows.Forms.ColumnHeader();
		this.hashLabel = new System.Windows.Forms.Label();
		this.m_hash = new System.Windows.Forms.Label();
		this.m_label = new System.Windows.Forms.Label();
		this.labelLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_changes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[6] { this.m_name, this.m_path, this.m_targetHash, this.m_patchSignature, this.m_isComposite, this.m_isSeed });
		this.m_changes.Font = new System.Drawing.Font("Consolas", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_changes.FullRowSelect = true;
		this.m_changes.Location = new System.Drawing.Point(12, 85);
		this.m_changes.Name = "m_changes";
		this.m_changes.Size = new System.Drawing.Size(642, 329);
		this.m_changes.TabIndex = 0;
		this.m_changes.UseCompatibleStateImageBehavior = false;
		this.m_changes.View = System.Windows.Forms.View.Details;
		this.m_name.Text = "File Name";
		this.m_name.Width = 78;
		this.m_path.Text = "Path";
		this.m_path.Width = 206;
		this.m_targetHash.Text = "Version Hash";
		this.m_targetHash.Width = 89;
		this.m_patchSignature.Text = "Patch Signature";
		this.m_patchSignature.Width = 112;
		this.m_isComposite.Text = "Is Composite?";
		this.m_isComposite.Width = 93;
		this.m_isSeed.Text = "Is Seed?";
		this.hashLabel.AutoSize = true;
		this.hashLabel.Location = new System.Drawing.Point(12, 9);
		this.hashLabel.Name = "hashLabel";
		this.hashLabel.Size = new System.Drawing.Size(35, 13);
		this.hashLabel.TabIndex = 1;
		this.hashLabel.Text = "Hash:";
		this.m_hash.AutoSize = true;
		this.m_hash.Location = new System.Drawing.Point(78, 9);
		this.m_hash.Name = "m_hash";
		this.m_hash.Size = new System.Drawing.Size(42, 13);
		this.m_hash.TabIndex = 2;
		this.m_hash.Text = "<hash>";
		this.m_label.AutoSize = true;
		this.m_label.Location = new System.Drawing.Point(78, 43);
		this.m_label.Name = "m_label";
		this.m_label.Size = new System.Drawing.Size(41, 13);
		this.m_label.TabIndex = 4;
		this.m_label.Text = "<label>";
		this.labelLabel.AutoSize = true;
		this.labelLabel.Location = new System.Drawing.Point(12, 43);
		this.labelLabel.Name = "labelLabel";
		this.labelLabel.Size = new System.Drawing.Size(36, 13);
		this.labelLabel.TabIndex = 3;
		this.labelLabel.Text = "Label:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(666, 426);
		base.Controls.Add(this.m_label);
		base.Controls.Add(this.labelLabel);
		base.Controls.Add(this.m_hash);
		base.Controls.Add(this.hashLabel);
		base.Controls.Add(this.m_changes);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "ChangeDetails";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Change Details";
		base.Load += new System.EventHandler(OnLoad);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
