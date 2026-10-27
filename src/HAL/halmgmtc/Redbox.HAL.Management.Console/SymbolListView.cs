using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console;

public class SymbolListView<T> : UserControl
{
	private IContainer components;

	private ToolStrip toolStrip1;

	private DataGridView dataGridView1;

	private Panel panel1;

	private ToolStripButton toolStripButton1;

	private ToolStripButton toolStripButton2;

	public SymbolListView(string[,] headers, BindingList<T> data)
	{
		InitializeComponent();
		base.Enabled = false;
		SuspendLayout();
		dataGridView1 = new AutomatedDataGridView<T>(headers, data, emptyEndColumn: false);
		dataGridView1.AutoSize = true;
		panel1.Controls.Add(dataGridView1);
		ResumeLayout();
		EnvironmentHelper.SelectedJobChanged += delegate
		{
			JobHelper.SymbolList.Clear();
		};
		ProfileManager.Instance.Connected += delegate
		{
			base.Enabled = true;
		};
		ProfileManager.Instance.Disconnected += delegate
		{
			base.Enabled = false;
			JobHelper.SymbolList.Clear();
		};
	}

	internal void RefreshData()
	{
		JobHelper.RefreshSymbolList();
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
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
		this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
		this.panel1 = new System.Windows.Forms.Panel();
		this.toolStrip1.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripButton1, this.toolStripButton2 });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new System.Drawing.Size(410, 25);
		this.toolStrip1.TabIndex = 0;
		this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.toolStripButton1.Image = Redbox.HAL.Management.Console.Properties.Resources.refresh;
		this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolStripButton1.Name = "toolStripButton1";
		this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
		this.toolStripButton1.Text = "Refresh";
		this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.toolStripButton2.Image = Redbox.HAL.Management.Console.Properties.Resources.clear;
		this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolStripButton2.Name = "toolStripButton2";
		this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
		this.toolStripButton2.Text = "Clear";
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
		this.panel1.Location = new System.Drawing.Point(0, 25);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(409, 382);
		this.panel1.TabIndex = 1;
		base.Controls.Add(this.toolStrip1);
		base.Controls.Add(this.panel1);
		base.Name = "SymbolListView";
		base.Size = new System.Drawing.Size(410, 411);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
