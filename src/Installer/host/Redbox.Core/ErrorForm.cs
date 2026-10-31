using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Redbox.Core;

public class ErrorForm : Form
{
	private IEnumerable m_errors;

	private IContainer components;

	private Panel m_messagePanel;

	private Label m_messageLabel;

	private ListView m_errorListView;

	private TextBox m_detailTextBox;

	private ColumnHeader m_codeColumnHeader;

	private ColumnHeader m_descriptionColumnHeader;

	private ImageList m_imageList;

	private Panel m_topPanel;

	private Splitter m_splitter;

	private Panel m_bottomPanel;

	private Button m_closeButton;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string Message
	{
		get
		{
			return m_messageLabel.Text;
		}
		set
		{
			m_messageLabel.Text = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IEnumerable Errors
	{
		get
		{
			return m_errors;
		}
		set
		{
			m_errors = value;
			RefreshList();
		}
	}

	public ErrorForm()
	{
		InitializeComponent();
	}

	private void OnClose(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void OnSelectedErrorChanged(object sender, EventArgs e)
	{
		m_detailTextBox.Clear();
		if (m_errorListView.SelectedItems.Count <= 0)
		{
			return;
		}
		object tag = m_errorListView.SelectedItems[0].Tag;
		if (tag != null)
		{
			PropertyInfo property = tag.GetType().GetProperty("Details");
			object obj = string.Empty;
			if (property != null)
			{
				obj = property.GetValue(tag, null);
			}
			m_detailTextBox.Text = ((obj != null) ? obj.ToString() : string.Empty);
		}
	}

	private void RefreshList()
	{
		m_errorListView.Items.Clear();
		if (Errors == null)
		{
			return;
		}
		foreach (object error in Errors)
		{
			PropertyInfo property = error.GetType().GetProperty("Code");
			PropertyInfo property2 = error.GetType().GetProperty("IsWarning");
			PropertyInfo property3 = error.GetType().GetProperty("Description");
			string text = string.Empty;
			if (property != null)
			{
				object value = property.GetValue(error, null);
				if (value != null)
				{
					text = value.ToString();
				}
			}
			bool flag = false;
			if (property2 != null)
			{
				flag = (bool)property2.GetValue(error, null);
			}
			string text2 = string.Empty;
			if (property3 != null)
			{
				object value2 = property3.GetValue(error, null);
				if (value2 != null)
				{
					text2 = value2.ToString();
				}
			}
			ListViewItem listViewItem = new ListViewItem(text, flag ? 1 : 0);
			listViewItem.SubItems.Add(text2);
			listViewItem.Tag = error;
			m_errorListView.Items.Add(listViewItem);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.Core.ErrorForm));
		this.m_messagePanel = new System.Windows.Forms.Panel();
		this.m_messageLabel = new System.Windows.Forms.Label();
		this.m_errorListView = new System.Windows.Forms.ListView();
		this.m_codeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_descriptionColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_imageList = new System.Windows.Forms.ImageList(this.components);
		this.m_detailTextBox = new System.Windows.Forms.TextBox();
		this.m_topPanel = new System.Windows.Forms.Panel();
		this.m_splitter = new System.Windows.Forms.Splitter();
		this.m_bottomPanel = new System.Windows.Forms.Panel();
		this.m_closeButton = new System.Windows.Forms.Button();
		this.m_messagePanel.SuspendLayout();
		this.m_topPanel.SuspendLayout();
		this.m_bottomPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_messagePanel.AutoSize = true;
		this.m_messagePanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.m_messagePanel.Controls.Add(this.m_messageLabel);
		this.m_messagePanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_messagePanel.Location = new System.Drawing.Point(0, 0);
		this.m_messagePanel.Name = "m_messagePanel";
		this.m_messagePanel.Padding = new System.Windows.Forms.Padding(3);
		this.m_messagePanel.Size = new System.Drawing.Size(542, 19);
		this.m_messagePanel.TabIndex = 1;
		this.m_messageLabel.AutoSize = true;
		this.m_messageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_messageLabel.Location = new System.Drawing.Point(3, 3);
		this.m_messageLabel.Name = "m_messageLabel";
		this.m_messageLabel.Size = new System.Drawing.Size(102, 13);
		this.m_messageLabel.TabIndex = 0;
		this.m_messageLabel.Text = "The following errors:";
		this.m_errorListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.m_codeColumnHeader, this.m_descriptionColumnHeader });
		this.m_errorListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_errorListView.FullRowSelect = true;
		this.m_errorListView.Location = new System.Drawing.Point(3, 3);
		this.m_errorListView.Name = "m_errorListView";
		this.m_errorListView.Size = new System.Drawing.Size(536, 160);
		this.m_errorListView.SmallImageList = this.m_imageList;
		this.m_errorListView.TabIndex = 2;
		this.m_errorListView.UseCompatibleStateImageBehavior = false;
		this.m_errorListView.View = System.Windows.Forms.View.Details;
		this.m_errorListView.SelectedIndexChanged += new System.EventHandler(OnSelectedErrorChanged);
		this.m_codeColumnHeader.Text = "Code";
		this.m_codeColumnHeader.Width = 75;
		this.m_descriptionColumnHeader.Text = "Description";
		this.m_descriptionColumnHeader.Width = 455;
		this.m_imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("m_imageList.ImageStream");
		this.m_imageList.TransparentColor = System.Drawing.Color.Transparent;
		this.m_imageList.Images.SetKeyName(0, "cross.png");
		this.m_imageList.Images.SetKeyName(1, "warning.png");
		this.m_detailTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_detailTextBox.Location = new System.Drawing.Point(3, 1);
		this.m_detailTextBox.Multiline = true;
		this.m_detailTextBox.Name = "m_detailTextBox";
		this.m_detailTextBox.ReadOnly = true;
		this.m_detailTextBox.Size = new System.Drawing.Size(536, 132);
		this.m_detailTextBox.TabIndex = 4;
		this.m_topPanel.Controls.Add(this.m_errorListView);
		this.m_topPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_topPanel.Location = new System.Drawing.Point(0, 19);
		this.m_topPanel.Name = "m_topPanel";
		this.m_topPanel.Padding = new System.Windows.Forms.Padding(3, 3, 3, 1);
		this.m_topPanel.Size = new System.Drawing.Size(542, 164);
		this.m_topPanel.TabIndex = 5;
		this.m_splitter.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_splitter.Location = new System.Drawing.Point(0, 183);
		this.m_splitter.MinSize = 100;
		this.m_splitter.Name = "m_splitter";
		this.m_splitter.Size = new System.Drawing.Size(542, 3);
		this.m_splitter.TabIndex = 6;
		this.m_splitter.TabStop = false;
		this.m_bottomPanel.Controls.Add(this.m_detailTextBox);
		this.m_bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_bottomPanel.Location = new System.Drawing.Point(0, 186);
		this.m_bottomPanel.Name = "m_bottomPanel";
		this.m_bottomPanel.Padding = new System.Windows.Forms.Padding(3, 1, 3, 3);
		this.m_bottomPanel.Size = new System.Drawing.Size(542, 136);
		this.m_bottomPanel.TabIndex = 7;
		this.m_closeButton.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.m_closeButton.Location = new System.Drawing.Point(0, 322);
		this.m_closeButton.Name = "m_closeButton";
		this.m_closeButton.Size = new System.Drawing.Size(542, 51);
		this.m_closeButton.TabIndex = 8;
		this.m_closeButton.Text = "Close";
		this.m_closeButton.UseVisualStyleBackColor = true;
		this.m_closeButton.Click += new System.EventHandler(OnClose);
		base.AcceptButton = this.m_closeButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(542, 373);
		base.Controls.Add(this.m_bottomPanel);
		base.Controls.Add(this.m_closeButton);
		base.Controls.Add(this.m_splitter);
		base.Controls.Add(this.m_topPanel);
		base.Controls.Add(this.m_messagePanel);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(550, 400);
		base.Name = "ErrorForm";
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Errors";
		this.m_messagePanel.ResumeLayout(false);
		this.m_messagePanel.PerformLayout();
		this.m_topPanel.ResumeLayout(false);
		this.m_bottomPanel.ResumeLayout(false);
		this.m_bottomPanel.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
