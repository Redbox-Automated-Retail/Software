using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Alsing.SourceCode;
using Alsing.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;

namespace Redbox.KioskEngine.IDE;

public class QueuePreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private ListView m_queueListView;

	private ColumnHeader m_idColumnHeader;

	private ColumnHeader m_typeColumnHeader;

	private ColumnHeader m_priorityColumnHeader;

	private ColumnHeader m_createdOnColumnHeader;

	private ErrorProvider m_errorProvider;

	private ToolStrip m_toolStrip;

	private ToolStripButton toolStripButton1;

	private ToolStripLabel toolStripLabel1;

	private ToolStripTextBox m_refreshRateToolStripTextBox;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton m_startQueueWorkerToolStripButton;

	private ToolStripButton m_stopQueueWorkerToolStripButton;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripButton m_removeToolStripButton;

	private ToolStripButton m_exportToolStripButton;

	private ToolStripButton m_cleanToolStripButton;

	private SyntaxBoxControl m_syntaxBoxControl;

	private SyntaxDocument m_syntaxDocument;

	private ToolStripButton m_saveToolStripButton;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripLabel toolStripLabel3;

	private ToolStripLabel toolStripLabel4;

	private Label m_headerLabel;

	public QueuePreferencePage()
	{
		InitializeComponent();
		m_syntaxDocument.SetSyntaxFromEmbeddedResource(typeof(QueuePreferencePage).Assembly, "Redbox.KioskEngine.IDE.JavaScript.syn");
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		m_refreshRateToolStripTextBox.Text = service.QueueReadPeriod.ToString();
		SetButtonState();
		RefreshList();
	}

	private void OnSelectedMessageChanged(object sender, EventArgs e)
	{
		m_removeToolStripButton.Enabled = m_queueListView.SelectedItems.Count > 0;
		if (m_queueListView.SelectedItems.Count == 1)
		{
			string text = m_queueListView.SelectedItems[0].Tag as string;
			if (text.Length > 10000)
			{
				text = string.Empty;
			}
			m_syntaxDocument.Text = text.ToPrettyJson();
		}
		else
		{
			m_syntaxDocument.Clear();
		}
	}

	private void OnClear(object sender, EventArgs e)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		try
		{
			Cursor = Cursors.WaitCursor;
			service.Clear();
			RefreshList();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnExport(object sender, EventArgs e)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		bool flag = service.IsWorkerStarted();
		if (flag)
		{
			service.StopQueueWorker();
		}
		try
		{
			Cursor = Cursors.WaitCursor;
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Title = "Export Message File",
				DefaultExt = ".xml",
				Filter = "XML Files (*.xml)|*.xml"
			};
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				service.ExportToXml(saveFileDialog.FileName);
			}
		}
		finally
		{
			if (flag)
			{
				service.StartQueueWorker();
			}
			Cursor = Cursors.Default;
			SetButtonState();
		}
	}

	private void OnRemove(object sender, EventArgs e)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (m_queueListView.SelectedItems.Count == 0)
		{
			return;
		}
		try
		{
			Cursor = Cursors.WaitCursor;
			foreach (ListViewItem selectedItem in m_queueListView.SelectedItems)
			{
				service.DeleteMessage(int.Parse(selectedItem.Text));
			}
			RefreshList();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnStopQueueWorker(object sender, EventArgs e)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		try
		{
			Cursor = Cursors.WaitCursor;
			m_stopQueueWorkerToolStripButton.Enabled = false;
			service.StopQueueWorker();
			m_startQueueWorkerToolStripButton.Enabled = true;
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnStartQueueWorker(object sender, EventArgs e)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		try
		{
			Cursor = Cursors.WaitCursor;
			m_startQueueWorkerToolStripButton.Enabled = false;
			service.StartQueueWorker();
			m_stopQueueWorkerToolStripButton.Enabled = true;
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		RefreshList();
	}

	private void RefreshList()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		m_queueListView.SuspendLayout();
		m_queueListView.BeginUpdate();
		m_queueListView.Items.Clear();
		m_syntaxDocument.Clear();
		service.ForEach(delegate(IMessage m)
		{
			ListViewItem listViewItem = new ListViewItem(m.ID.ToString());
			if (m.Type != "Reconcile")
			{
				listViewItem.Tag = Encoding.ASCII.GetString(m.Data);
			}
			else
			{
				listViewItem.Tag = "Reconcile";
			}
			listViewItem.SubItems.Add(m.Type);
			listViewItem.SubItems.Add(m.Priority.ToString());
			listViewItem.SubItems.Add(m.CreatedOn.ToString());
			m_queueListView.Items.Add(listViewItem);
		});
		m_queueListView.EndUpdate();
		m_queueListView.ResumeLayout();
		m_removeToolStripButton.Enabled = m_queueListView.SelectedItems.Count > 0;
		m_cleanToolStripButton.Enabled = m_queueListView.Items.Count > 0;
		m_exportToolStripButton.Enabled = m_queueListView.Items.Count > 0;
	}

	private void SetButtonState()
	{
		bool flag = ServiceLocator.Instance.GetService<IQueueService>().IsWorkerStarted();
		m_startQueueWorkerToolStripButton.Enabled = !flag;
		m_stopQueueWorkerToolStripButton.Enabled = flag;
	}

	private void OnDocumentChanged(object sender, EventArgs e)
	{
		m_saveToolStripButton.Enabled = m_syntaxDocument.UndoBuffer.Count > 0 && m_queueListView.SelectedItems.Count == 1;
	}

	private void OnSave(object sender, EventArgs e)
	{
		if (m_queueListView.SelectedItems.Count != 0)
		{
			IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
			string s = m_queueListView.SelectedItems[0].SubItems[0].Text;
			service.Update(int.Parse(s), m_syntaxDocument.Text);
			RefreshList();
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
		this.components = new System.ComponentModel.Container();
		this.m_queueListView = new System.Windows.Forms.ListView();
		this.m_idColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_typeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_priorityColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_createdOnColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
		this.m_refreshRateToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
		this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
		this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.m_startQueueWorkerToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_stopQueueWorkerToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.m_saveToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_removeToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.m_exportToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_cleanToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_syntaxBoxControl = new Alsing.Windows.Forms.SyntaxBoxControl();
		this.m_syntaxDocument = new Alsing.SourceCode.SyntaxDocument(this.components);
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		this.m_toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.m_queueListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.m_idColumnHeader, this.m_typeColumnHeader, this.m_priorityColumnHeader, this.m_createdOnColumnHeader });
		this.m_queueListView.FullRowSelect = true;
		this.m_queueListView.HideSelection = false;
		this.m_queueListView.Location = new System.Drawing.Point(0, 85);
		this.m_queueListView.Margin = new System.Windows.Forms.Padding(4);
		this.m_queueListView.Name = "m_queueListView";
		this.m_queueListView.Size = new System.Drawing.Size(721, 377);
		this.m_queueListView.TabIndex = 12;
		this.m_queueListView.UseCompatibleStateImageBehavior = false;
		this.m_queueListView.View = System.Windows.Forms.View.Details;
		this.m_queueListView.SelectedIndexChanged += new System.EventHandler(OnSelectedMessageChanged);
		this.m_idColumnHeader.Text = "ID";
		this.m_idColumnHeader.Width = 100;
		this.m_typeColumnHeader.Text = "Type";
		this.m_typeColumnHeader.Width = 110;
		this.m_priorityColumnHeader.Text = "Priority";
		this.m_priorityColumnHeader.Width = 65;
		this.m_createdOnColumnHeader.Text = "Created On";
		this.m_createdOnColumnHeader.Width = 150;
		this.m_errorProvider.ContainerControl = this;
		this.m_toolStrip.CanOverflow = false;
		this.m_toolStrip.Dock = System.Windows.Forms.DockStyle.None;
		this.m_toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[15]
		{
			this.toolStripLabel1, this.m_refreshRateToolStripTextBox, this.toolStripLabel3, this.toolStripLabel4, this.toolStripSeparator1, this.toolStripButton1, this.toolStripSeparator2, this.m_startQueueWorkerToolStripButton, this.m_stopQueueWorkerToolStripButton, this.toolStripSeparator3,
			this.m_saveToolStripButton, this.m_removeToolStripButton, this.toolStripSeparator5, this.m_exportToolStripButton, this.m_cleanToolStripButton
		});
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.m_toolStrip.Location = new System.Drawing.Point(5, 53);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(524, 27);
		this.m_toolStrip.TabIndex = 20;
		this.m_toolStrip.Text = "toolStrip1";
		this.toolStripLabel1.AutoSize = false;
		this.toolStripLabel1.Name = "toolStripLabel1";
		this.toolStripLabel1.Size = new System.Drawing.Size(120, 20);
		this.toolStripLabel1.Text = "Poll Interval (ms):";
		this.m_refreshRateToolStripTextBox.MaxLength = 6;
		this.m_refreshRateToolStripTextBox.Name = "m_refreshRateToolStripTextBox";
		this.m_refreshRateToolStripTextBox.Size = new System.Drawing.Size(85, 27);
		this.toolStripLabel3.Name = "toolStripLabel3";
		this.toolStripLabel3.Size = new System.Drawing.Size(25, 20);
		this.toolStripLabel3.Text = "    ";
		this.toolStripLabel4.Name = "toolStripLabel4";
		this.toolStripLabel4.Size = new System.Drawing.Size(25, 20);
		this.toolStripLabel4.Text = "    ";
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
		this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.toolStripButton1.Image = Redbox.KioskEngine.IDE.Properties.Resources.Refresh;
		this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolStripButton1.Name = "toolStripButton1";
		this.toolStripButton1.Size = new System.Drawing.Size(29, 24);
		this.toolStripButton1.Text = "Refresh";
		this.toolStripButton1.Click += new System.EventHandler(OnRefresh);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 23);
		this.m_startQueueWorkerToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_startQueueWorkerToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugPlay;
		this.m_startQueueWorkerToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_startQueueWorkerToolStripButton.Name = "m_startQueueWorkerToolStripButton";
		this.m_startQueueWorkerToolStripButton.Size = new System.Drawing.Size(29, 24);
		this.m_startQueueWorkerToolStripButton.Text = "Start Queue Worker";
		this.m_startQueueWorkerToolStripButton.Click += new System.EventHandler(OnStartQueueWorker);
		this.m_stopQueueWorkerToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_stopQueueWorkerToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStop;
		this.m_stopQueueWorkerToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_stopQueueWorkerToolStripButton.Name = "m_stopQueueWorkerToolStripButton";
		this.m_stopQueueWorkerToolStripButton.Size = new System.Drawing.Size(29, 24);
		this.m_stopQueueWorkerToolStripButton.Text = "Stop Queue Worker";
		this.m_stopQueueWorkerToolStripButton.Click += new System.EventHandler(OnStopQueueWorker);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 23);
		this.m_saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_saveToolStripButton.Enabled = false;
		this.m_saveToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Save;
		this.m_saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_saveToolStripButton.Name = "m_saveToolStripButton";
		this.m_saveToolStripButton.Size = new System.Drawing.Size(29, 24);
		this.m_saveToolStripButton.Text = "Save";
		this.m_saveToolStripButton.Click += new System.EventHandler(OnSave);
		this.m_removeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_removeToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Delete;
		this.m_removeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_removeToolStripButton.Name = "m_removeToolStripButton";
		this.m_removeToolStripButton.Size = new System.Drawing.Size(29, 24);
		this.m_removeToolStripButton.Text = "Remove";
		this.m_removeToolStripButton.ToolTipText = "Remove";
		this.m_removeToolStripButton.Click += new System.EventHandler(OnRemove);
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(6, 23);
		this.m_exportToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_exportToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.SaveAs;
		this.m_exportToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_exportToolStripButton.Name = "m_exportToolStripButton";
		this.m_exportToolStripButton.Size = new System.Drawing.Size(29, 24);
		this.m_exportToolStripButton.Text = "Export";
		this.m_exportToolStripButton.Click += new System.EventHandler(OnExport);
		this.m_cleanToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_cleanToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Clean;
		this.m_cleanToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_cleanToolStripButton.Name = "m_cleanToolStripButton";
		this.m_cleanToolStripButton.Size = new System.Drawing.Size(29, 24);
		this.m_cleanToolStripButton.Text = "Clear";
		this.m_cleanToolStripButton.Visible = false;
		this.m_cleanToolStripButton.Click += new System.EventHandler(OnClear);
		this.m_syntaxBoxControl.ActiveView = Alsing.Windows.Forms.ActiveView.BottomRight;
		this.m_syntaxBoxControl.AllowBreakPoints = false;
		this.m_syntaxBoxControl.AutoListPosition = null;
		this.m_syntaxBoxControl.AutoListSelectedText = "a123";
		this.m_syntaxBoxControl.AutoListVisible = false;
		this.m_syntaxBoxControl.BackColor = System.Drawing.Color.White;
		this.m_syntaxBoxControl.BorderStyle = Alsing.Windows.Forms.BorderStyle.None;
		this.m_syntaxBoxControl.CopyAsRTF = false;
		this.m_syntaxBoxControl.Document = this.m_syntaxDocument;
		this.m_syntaxBoxControl.FontName = "Courier new";
		this.m_syntaxBoxControl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.m_syntaxBoxControl.InfoTipCount = 1;
		this.m_syntaxBoxControl.InfoTipPosition = null;
		this.m_syntaxBoxControl.InfoTipSelectedIndex = 1;
		this.m_syntaxBoxControl.InfoTipVisible = false;
		this.m_syntaxBoxControl.Location = new System.Drawing.Point(-1, 470);
		this.m_syntaxBoxControl.LockCursorUpdate = false;
		this.m_syntaxBoxControl.Margin = new System.Windows.Forms.Padding(4);
		this.m_syntaxBoxControl.Name = "m_syntaxBoxControl";
		this.m_syntaxBoxControl.ShowEOLMarker = true;
		this.m_syntaxBoxControl.ShowGutterMargin = false;
		this.m_syntaxBoxControl.ShowScopeIndicator = false;
		this.m_syntaxBoxControl.ShowTabGuides = true;
		this.m_syntaxBoxControl.ShowWhitespace = true;
		this.m_syntaxBoxControl.Size = new System.Drawing.Size(723, 153);
		this.m_syntaxBoxControl.SmoothScroll = false;
		this.m_syntaxBoxControl.SplitView = false;
		this.m_syntaxBoxControl.SplitviewH = -4;
		this.m_syntaxBoxControl.SplitviewV = -4;
		this.m_syntaxBoxControl.TabGuideColor = System.Drawing.Color.FromArgb(222, 219, 214);
		this.m_syntaxBoxControl.TabIndex = 22;
		this.m_syntaxBoxControl.TabsToSpaces = true;
		this.m_syntaxBoxControl.WhitespaceColor = System.Drawing.SystemColors.ControlDark;
		this.m_syntaxDocument.Lines = new string[1] { "" };
		this.m_syntaxDocument.MaxUndoBufferSize = 1000;
		this.m_syntaxDocument.Modified = false;
		this.m_syntaxDocument.UndoStep = 0;
		this.m_syntaxDocument.Change += new System.EventHandler(OnDocumentChanged);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.MediumBlue;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(723, 47);
		this.m_headerLabel.TabIndex = 23;
		this.m_headerLabel.Text = "  Engine Core: Message Queue";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_syntaxBoxControl);
		base.Controls.Add(this.m_queueListView);
		base.Controls.Add(this.m_toolStrip);
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "QueuePreferencePage";
		base.Size = new System.Drawing.Size(723, 623);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
