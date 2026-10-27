using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class TableEditor : UserControl
{
	private IContainer components;

	private DataGridView m_dataGridView;

	public TableEditor()
	{
		InitializeComponent();
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
		this.m_dataGridView = new System.Windows.Forms.DataGridView();
		((System.ComponentModel.ISupportInitialize)this.m_dataGridView).BeginInit();
		base.SuspendLayout();
		this.m_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.m_dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_dataGridView.Location = new System.Drawing.Point(0, 0);
		this.m_dataGridView.Name = "m_dataGridView";
		this.m_dataGridView.Size = new System.Drawing.Size(725, 453);
		this.m_dataGridView.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_dataGridView);
		base.Name = "TableEditor";
		base.Size = new System.Drawing.Size(725, 453);
		((System.ComponentModel.ISupportInitialize)this.m_dataGridView).EndInit();
		base.ResumeLayout(false);
	}
}
