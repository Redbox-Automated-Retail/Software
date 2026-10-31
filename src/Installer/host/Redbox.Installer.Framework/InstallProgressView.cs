using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Redbox.Installer.Framework;

public class InstallProgressView : UserControl
{
	private IContainer components;

	private TableLayoutPanel m_tableLayoutPanel;

	private Label m_progressLabel;

	private ProgressBar m_progressBar;

	public event InstallCompleteHandler InstallComplete;

	public InstallProgressView()
	{
		InitializeComponent();
	}

	public void Execute()
	{
		m_progressBar.Value = 0;
		m_progressBar.Maximum = InstallerProject.Instance.TotalNumberOfTasks;
		Thread thread = new Thread(ExecuteInstallAction);
		thread.IsBackground = true;
		Thread thread2 = thread;
		thread2.Start();
	}

	private void ExecuteInstallAction(object state)
	{
		ErrorList errors = InstallerProject.Instance.Execute(delegate(Definition definition)
		{
			if (definition != null)
			{
				m_progressLabel.Text = "Status: " + definition.ToExecutionMessage();
			}
			m_progressBar.Increment(1);
		});
		if (InstallComplete != null)
		{
			InstallComplete(errors);
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
		this.m_tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.m_progressLabel = new System.Windows.Forms.Label();
		this.m_progressBar = new System.Windows.Forms.ProgressBar();
		this.m_tableLayoutPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_tableLayoutPanel.ColumnCount = 1;
		this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.m_tableLayoutPanel.Controls.Add(this.m_progressLabel, 0, 0);
		this.m_tableLayoutPanel.Controls.Add(this.m_progressBar, 0, 1);
		this.m_tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_tableLayoutPanel.Name = "m_tableLayoutPanel";
		this.m_tableLayoutPanel.RowCount = 3;
		this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24f));
		this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130f));
		this.m_tableLayoutPanel.Size = new System.Drawing.Size(349, 270);
		this.m_tableLayoutPanel.TabIndex = 0;
		this.m_progressLabel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_progressLabel.Location = new System.Drawing.Point(3, 0);
		this.m_progressLabel.Name = "m_progressLabel";
		this.m_progressLabel.Size = new System.Drawing.Size(343, 116);
		this.m_progressLabel.TabIndex = 0;
		this.m_progressLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		this.m_progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_progressBar.Location = new System.Drawing.Point(3, 119);
		this.m_progressBar.Name = "m_progressBar";
		this.m_progressBar.Size = new System.Drawing.Size(343, 18);
		this.m_progressBar.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Transparent;
		base.Controls.Add(this.m_tableLayoutPanel);
		base.Name = "InstallProgressPage";
		base.Size = new System.Drawing.Size(349, 270);
		this.m_tableLayoutPanel.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
