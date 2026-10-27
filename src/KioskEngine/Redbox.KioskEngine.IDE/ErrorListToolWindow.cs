using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.IDE.Properties;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class ErrorListToolWindow : ToolWindow
{
	private IContainer components;

	private ToolStrip m_toolStrip;

	private ToolStripButton m_errorsToolStripButton;

	private ToolStripButton m_warningsToolStripButton;

	private ToolStripButton m_messagesToolStripButton;

	private ListView m_listView;

	private ColumnHeader m_codeColumnHeader;

	private ColumnHeader m_descriptionColumnHeader;

	public ErrorListToolWindow()
	{
		InitializeComponent();
	}

	private void OnClickErrors(object sender, EventArgs e)
	{
		m_errorsToolStripButton.Checked = !m_errorsToolStripButton.Checked;
	}

	private void OnClickWarnings(object sender, EventArgs e)
	{
		m_warningsToolStripButton.Checked = !m_warningsToolStripButton.Checked;
	}

	private void OnClickMessages(object sender, EventArgs e)
	{
		m_messagesToolStripButton.Checked = !m_messagesToolStripButton.Checked;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.ErrorListToolWindow));
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_errorsToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_warningsToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_messagesToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_listView = new System.Windows.Forms.ListView();
		this.m_codeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_descriptionColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.m_errorsToolStripButton, this.m_warningsToolStripButton, this.m_messagesToolStripButton });
		this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(589, 25);
		this.m_toolStrip.TabIndex = 0;
		this.m_toolStrip.Text = "toolStrip1";
		this.m_errorsToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Critical;
		this.m_errorsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_errorsToolStripButton.Name = "m_errorsToolStripButton";
		this.m_errorsToolStripButton.Size = new System.Drawing.Size(56, 22);
		this.m_errorsToolStripButton.Text = "Errors";
		this.m_errorsToolStripButton.Click += new System.EventHandler(OnClickErrors);
		this.m_warningsToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Warning;
		this.m_warningsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_warningsToolStripButton.Name = "m_warningsToolStripButton";
		this.m_warningsToolStripButton.Size = new System.Drawing.Size(72, 22);
		this.m_warningsToolStripButton.Text = "Warnings";
		this.m_warningsToolStripButton.Click += new System.EventHandler(OnClickWarnings);
		this.m_messagesToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Information;
		this.m_messagesToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_messagesToolStripButton.Name = "m_messagesToolStripButton";
		this.m_messagesToolStripButton.Size = new System.Drawing.Size(74, 22);
		this.m_messagesToolStripButton.Text = "Messages";
		this.m_messagesToolStripButton.Click += new System.EventHandler(OnClickMessages);
		this.m_listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.m_codeColumnHeader, this.m_descriptionColumnHeader });
		this.m_listView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_listView.FullRowSelect = true;
		this.m_listView.GridLines = true;
		this.m_listView.Location = new System.Drawing.Point(0, 25);
		this.m_listView.Name = "m_listView";
		this.m_listView.Size = new System.Drawing.Size(589, 259);
		this.m_listView.TabIndex = 1;
		this.m_listView.UseCompatibleStateImageBehavior = false;
		this.m_listView.View = System.Windows.Forms.View.Details;
		this.m_codeColumnHeader.Text = "Code";
		this.m_codeColumnHeader.Width = 85;
		this.m_descriptionColumnHeader.Text = "Description";
		this.m_descriptionColumnHeader.Width = 600;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(589, 284);
		base.Controls.Add(this.m_listView);
		base.Controls.Add(this.m_toolStrip);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.HideOnClose = true;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "ErrorListToolWindow";
		base.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
		this.Text = "Error List";
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
