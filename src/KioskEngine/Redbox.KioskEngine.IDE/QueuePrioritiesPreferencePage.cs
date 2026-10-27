using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.IDE;

public class QueuePrioritiesPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Label m_headerLabel;

	private ListView listViewQueuePriorities;

	private ColumnHeader columnHeaderPriority;

	private ColumnHeader columnHeaderValueRange;

	private ColumnHeader columnHeaderIncludeTimeRange;

	private ColumnHeader columnHeaderExcludeTimeRange;

	private ColumnHeader columnHeaderDescription;

	public QueuePrioritiesPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		listViewQueuePriorities.Items.Clear();
		service?.ForEachPriority(delegate(IQueueServicePriority eachQueueServicePriority)
		{
			ListViewItem listViewItem = new ListViewItem
			{
				Text = eachQueueServicePriority.PriorityType.ToString()
			};
			string text = $"{eachQueueServicePriority.MinimumPriorityValue} - {eachQueueServicePriority.MaximumPriorityValue}";
			listViewItem.SubItems.Add(text);
			string text2 = $"{eachQueueServicePriority.StartTime} - {eachQueueServicePriority.EndTime}";
			listViewItem.SubItems.Add(text2);
			string text3 = eachQueueServicePriority.ExcludeTimeRanges.Select((IQueueServicePriority y) => y.PriorityType.ToString()).Join(", ");
			listViewItem.SubItems.Add(text3);
			listViewItem.SubItems.Add(eachQueueServicePriority.Description);
			listViewQueuePriorities.Items.Add(listViewItem);
		});
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
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.listViewQueuePriorities = new System.Windows.Forms.ListView();
		this.columnHeaderPriority = new System.Windows.Forms.ColumnHeader();
		this.columnHeaderValueRange = new System.Windows.Forms.ColumnHeader();
		this.columnHeaderIncludeTimeRange = new System.Windows.Forms.ColumnHeader();
		this.columnHeaderExcludeTimeRange = new System.Windows.Forms.ColumnHeader();
		this.columnHeaderDescription = new System.Windows.Forms.ColumnHeader();
		base.SuspendLayout();
		this.m_headerLabel.BackColor = System.Drawing.Color.MediumBlue;
		this.m_headerLabel.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(524, 38);
		this.m_headerLabel.TabIndex = 24;
		this.m_headerLabel.Text = "Message Queue Priorities";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.listViewQueuePriorities.Columns.AddRange(new System.Windows.Forms.ColumnHeader[5] { this.columnHeaderPriority, this.columnHeaderValueRange, this.columnHeaderIncludeTimeRange, this.columnHeaderExcludeTimeRange, this.columnHeaderDescription });
		this.listViewQueuePriorities.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listViewQueuePriorities.Location = new System.Drawing.Point(0, 38);
		this.listViewQueuePriorities.Name = "listViewQueuePriorities";
		this.listViewQueuePriorities.Size = new System.Drawing.Size(524, 468);
		this.listViewQueuePriorities.TabIndex = 25;
		this.listViewQueuePriorities.UseCompatibleStateImageBehavior = false;
		this.listViewQueuePriorities.View = System.Windows.Forms.View.Details;
		this.columnHeaderPriority.Text = "Priority";
		this.columnHeaderPriority.Width = 70;
		this.columnHeaderValueRange.Text = "Value Range";
		this.columnHeaderValueRange.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.columnHeaderValueRange.Width = 100;
		this.columnHeaderIncludeTimeRange.Text = "Time Range";
		this.columnHeaderIncludeTimeRange.Width = 140;
		this.columnHeaderExcludeTimeRange.Text = "Exclude Time Ranges";
		this.columnHeaderExcludeTimeRange.Width = 120;
		this.columnHeaderDescription.Text = "Description";
		this.columnHeaderDescription.Width = 700;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.listViewQueuePriorities);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "QueuePrioritiesPreferencePage";
		base.Size = new System.Drawing.Size(524, 506);
		base.ResumeLayout(false);
	}
}
