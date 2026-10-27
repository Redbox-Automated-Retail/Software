using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.StoreInstallerFrontEnd.Properties;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd;

public class EventPanel : UserControl
{
	private readonly ErrorList m_errors = new ErrorList();

	private IContainer components;

	private Label m_name;

	private TextBox m_description;

	private PictureBox m_status;

	public EventPanel()
	{
		InitializeComponent();
	}

	public EventPanel(string name, string descripion)
	{
		InitializeComponent();
		m_name.Text = name;
		m_description.Text = descripion;
	}

	public void Start()
	{
		m_status.Image = Resources.Spinner32x32;
	}

	public void Error(Redbox.UpdateManager.ComponentModel.Error error)
	{
		m_errors.Add(error);
		m_status.Image = Resources.error2;
	}

	public void Complete()
	{
		m_status.Image = Resources.sucess;
	}

	public void Pending()
	{
		m_status.Image = Resources.pending;
	}

	private void m_status_Click(object sender, EventArgs e)
	{
		if (m_errors.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = m_errors;
			errorForm.ShowDialog();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.UpdateManager.StoreInstallerFrontEnd.EventPanel));
		this.m_name = new System.Windows.Forms.Label();
		this.m_description = new System.Windows.Forms.TextBox();
		this.m_status = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.m_status).BeginInit();
		base.SuspendLayout();
		this.m_name.AutoSize = true;
		this.m_name.Font = new System.Drawing.Font("Consolas", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_name.Location = new System.Drawing.Point(4, 4);
		this.m_name.Name = "m_name";
		this.m_name.Size = new System.Drawing.Size(77, 15);
		this.m_name.TabIndex = 0;
		this.m_name.Text = "Event Name";
		this.m_description.BackColor = System.Drawing.SystemColors.Control;
		this.m_description.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_description.CausesValidation = false;
		this.m_description.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_description.Location = new System.Drawing.Point(7, 22);
		this.m_description.Multiline = true;
		this.m_description.Name = "m_description";
		this.m_description.ReadOnly = true;
		this.m_description.Size = new System.Drawing.Size(439, 67);
		this.m_description.TabIndex = 1;
		this.m_description.TabStop = false;
		this.m_description.Text = resources.GetString("m_description.Text");
		this.m_status.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.m_status.Image = Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.pending;
		this.m_status.Location = new System.Drawing.Point(464, 33);
		this.m_status.Name = "m_status";
		this.m_status.Size = new System.Drawing.Size(32, 32);
		this.m_status.TabIndex = 2;
		this.m_status.TabStop = false;
		this.m_status.Click += new System.EventHandler(m_status_Click);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		base.Controls.Add(this.m_status);
		base.Controls.Add(this.m_description);
		base.Controls.Add(this.m_name);
		this.Font = new System.Drawing.Font("Consolas", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.Name = "EventPanel";
		base.Size = new System.Drawing.Size(515, 92);
		((System.ComponentModel.ISupportInitialize)this.m_status).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
