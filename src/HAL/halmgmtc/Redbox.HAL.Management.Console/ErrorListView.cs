using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console;

public class ErrorListView : UserControl
{
	private static ErrorList m_list;

	private static ErrorListView m_instance;

	private IContainer components;

	private ToolStrip toolStrip1;

	private ToolStripButton m_warningButton;

	private ToolStripButton m_errorButton;

	private Panel panel1;

	private DataGridView dataGridView1;

	private DataGridViewTextBoxColumn Type;

	private DataGridViewTextBoxColumn Code;

	private DataGridViewTextBoxColumn Description;

	public static ErrorListView Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new ErrorListView();
			}
			return m_instance;
		}
	}

	public bool KeepOpenOnSuccessfulInstruction { get; set; }

	internal void Add(ErrorList list, bool clearPrevious)
	{
		if (clearPrevious)
		{
			Clear();
		}
		m_list.AddRange(list);
		RefreshData();
	}

	internal void AddTop(ErrorList list, bool clearPrevious)
	{
		if (clearPrevious)
		{
			Clear();
		}
		foreach (Error item in list)
		{
			m_list.Insert(0, item);
		}
		RefreshData();
	}

	internal void RefreshData()
	{
		dataGridView1.SuspendLayout();
		dataGridView1.Rows.Clear();
		foreach (Error item in m_list)
		{
			if ((m_warningButton.Checked || !item.IsWarning) && (m_errorButton.Checked || item.IsWarning))
			{
				DataGridViewRow dataGridViewRow = new DataGridViewRow();
				dataGridViewRow.Cells.AddRange(new DataGridViewImageCell(), new DataGridViewTextBoxCell(), new DataGridViewTextBoxCell());
				if (item.IsWarning)
				{
					dataGridViewRow.Cells[0].Value = Resources.warning;
				}
				else
				{
					dataGridViewRow.Cells[0].Value = Resources.cross;
				}
				dataGridViewRow.Cells[1].Value = item.Code;
				dataGridViewRow.Cells[2].Value = item.Description + Environment.NewLine + item.Details;
				dataGridView1.Rows.Add(dataGridViewRow);
			}
		}
		dataGridView1.ResumeLayout();
	}

	internal void Clear()
	{
		if (m_list.Count > 0)
		{
			m_list.Clear();
			RefreshData();
		}
	}

	private ErrorListView()
	{
		InitializeComponent();
		base.Enabled = true;
		m_list = new ErrorList();
	}

	private void OnWarning_Click(object sender, EventArgs e)
	{
		RefreshData();
	}

	private void OnError_Click(object sender, EventArgs e)
	{
		RefreshData();
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
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.m_warningButton = new System.Windows.Forms.ToolStripButton();
		this.m_errorButton = new System.Windows.Forms.ToolStripButton();
		this.panel1 = new System.Windows.Forms.Panel();
		this.dataGridView1 = new System.Windows.Forms.DataGridView();
		this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Code = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.toolStrip1.SuspendLayout();
		this.panel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGridView1).BeginInit();
		base.SuspendLayout();
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.m_warningButton, this.m_errorButton });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new System.Drawing.Size(368, 25);
		this.toolStrip1.TabIndex = 0;
		this.toolStrip1.Text = "toolStrip1";
		this.m_warningButton.Checked = true;
		this.m_warningButton.CheckOnClick = true;
		this.m_warningButton.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_warningButton.Image = Redbox.HAL.Management.Console.Properties.Resources.warning;
		this.m_warningButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_warningButton.Name = "m_warningButton";
		this.m_warningButton.Size = new System.Drawing.Size(72, 22);
		this.m_warningButton.Text = "Warning";
		this.m_warningButton.Click += new System.EventHandler(OnWarning_Click);
		this.m_errorButton.Checked = true;
		this.m_errorButton.CheckOnClick = true;
		this.m_errorButton.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_errorButton.Image = Redbox.HAL.Management.Console.Properties.Resources.cross;
		this.m_errorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_errorButton.Name = "m_errorButton";
		this.m_errorButton.Size = new System.Drawing.Size(52, 22);
		this.m_errorButton.Text = "Error";
		this.m_errorButton.Click += new System.EventHandler(OnError_Click);
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.Controls.Add(this.dataGridView1);
		this.panel1.Location = new System.Drawing.Point(3, 30);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(365, 117);
		this.panel1.TabIndex = 1;
		this.dataGridView1.AllowUserToAddRows = false;
		this.dataGridView1.AllowUserToDeleteRows = false;
		this.dataGridView1.AllowUserToResizeColumns = false;
		this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
		this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
		this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
		this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.dataGridView1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
		this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridView1.Columns.AddRange(this.Type, this.Code, this.Description);
		dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		dataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle;
		this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGridView1.Location = new System.Drawing.Point(0, 0);
		this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 1, 3, 4);
		this.dataGridView1.Name = "dataGridView1";
		this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
		this.dataGridView1.Size = new System.Drawing.Size(365, 117);
		this.dataGridView1.TabIndex = 0;
		this.Type.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
		this.Type.HeaderText = "";
		this.Type.MinimumWidth = 50;
		this.Type.Name = "Type";
		this.Type.ReadOnly = true;
		this.Type.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.Type.Width = 50;
		this.Code.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.Code.HeaderText = "Code";
		this.Code.Name = "Code";
		this.Code.ReadOnly = true;
		this.Code.Width = 57;
		this.Description.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.Description.HeaderText = "Description";
		this.Description.Name = "Description";
		this.Description.ReadOnly = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.toolStrip1);
		base.Name = "ErrorListView";
		base.Size = new System.Drawing.Size(368, 150);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		this.panel1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dataGridView1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
