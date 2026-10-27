using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

public class AutomatedDataGridView<T> : DataGridView
{
	public AutomatedDataGridView(string[,] headers, BindingList<T> data, bool emptyEndColumn)
	{
		InitializeComponent();
		BuildGrid(headers, emptyEndColumn);
		base.DataSource = data;
	}

	public AutomatedDataGridView(string[,] headers, BindingSource data, bool emptyEndColumn)
	{
		InitializeComponent();
		BuildGrid(headers, emptyEndColumn);
		base.DataSource = data;
	}

	private void BuildGrid(string[,] headers, bool emptyEndColumn)
	{
		for (int i = 0; i < headers.Length / 2; i++)
		{
			DataGridViewColumn dataGridViewColumn = new DataGridViewTextBoxColumn();
			dataGridViewColumn.HeaderText = headers[i, 0];
			dataGridViewColumn.DataPropertyName = headers[i, 1];
			dataGridViewColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewColumn.ReadOnly = true;
			dataGridViewColumn.SortMode = DataGridViewColumnSortMode.Automatic;
			base.Columns.Add(dataGridViewColumn);
		}
		if (emptyEndColumn)
		{
			DataGridViewColumn dataGridViewColumn2 = new DataGridViewTextBoxColumn();
			dataGridViewColumn2.HeaderText = string.Empty;
			dataGridViewColumn2.ReadOnly = true;
			base.Columns.Add(dataGridViewColumn2);
		}
		base.Columns[base.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
	}

	private void InitializeComponent()
	{
		((System.ComponentModel.ISupportInitialize)this).BeginInit();
		base.SuspendLayout();
		base.AutoGenerateColumns = false;
		base.AllowUserToAddRows = false;
		base.AllowUserToDeleteRows = false;
		base.AllowUserToResizeColumns = true;
		base.AllowUserToResizeRows = false;
		base.BackgroundColor = System.Drawing.Color.White;
		base.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		base.ColumnHeadersHeight = 25;
		base.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		base.DefaultCellStyle.BackColor = System.Drawing.Color.White;
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		base.RowHeadersVisible = true;
		base.RowHeadersWidth = 25;
		base.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		base.TabIndex = 1;
		base.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
		((System.ComponentModel.ISupportInitialize)this).EndInit();
		base.ResumeLayout(false);
	}
}
