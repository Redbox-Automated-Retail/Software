using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class EditSearchPathForm : Form
{
	private IContainer components;

	private ListBox m_listBox;

	private Label label1;

	private TextBox m_pathTextBox;

	private Button m_browseButton;

	private Button m_addButton;

	private Button m_removeButton;

	private Button m_clearButton;

	private ErrorProvider m_errorProvider;

	private Button m_moveDownButton;

	private Button m_moveUpButton;

	private Button m_okButton;

	private Button m_cancelButton;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public List<string> SearchPath { get; set; }

	public EditSearchPathForm()
	{
		InitializeComponent();
	}

	private void OnSelectedPathChanged(object sender, EventArgs e)
	{
		m_removeButton.Enabled = m_listBox.SelectedItems.Count > 0;
		m_moveUpButton.Enabled = m_listBox.SelectedItems.Count > 0;
		m_moveDownButton.Enabled = m_listBox.SelectedItems.Count > 0;
	}

	private void OnRemove(object sender, EventArgs e)
	{
		foreach (int selectedIndex in m_listBox.SelectedIndices)
		{
			m_listBox.Items.RemoveAt(selectedIndex);
		}
	}

	private void OnClear(object sender, EventArgs e)
	{
		m_listBox.Items.Clear();
	}

	private void OnBrowse(object sender, EventArgs e)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
		{
			SelectedPath = m_pathTextBox.Text
		};
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			m_pathTextBox.Text = folderBrowserDialog.SelectedPath;
		}
	}

	private void OnAdd(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_browseButton, string.Empty);
		if (m_pathTextBox.Text.Length == 0)
		{
			m_errorProvider.SetError(m_browseButton, "Specify a valid path.");
			return;
		}
		m_listBox.Items.Add(m_pathTextBox.Text);
		m_pathTextBox.Clear();
		m_pathTextBox.Focus();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		RefreshList();
	}

	private void OnMoveUp(object sender, EventArgs e)
	{
		object selectedItem = m_listBox.SelectedItem;
		if (selectedItem != null)
		{
			int num = m_listBox.Items.IndexOf(selectedItem);
			if (num != 0)
			{
				m_listBox.Items.RemoveAt(num);
				m_listBox.Items.Insert(num - 1, selectedItem);
				m_listBox.SelectedItem = selectedItem;
			}
		}
	}

	private void OnMoveDown(object sender, EventArgs e)
	{
		object selectedItem = m_listBox.SelectedItem;
		if (selectedItem != null)
		{
			int num = m_listBox.Items.IndexOf(selectedItem);
			if (num != -1 && num < m_listBox.Items.Count - 1)
			{
				m_listBox.Items.RemoveAt(num);
				m_listBox.Items.Insert(num + 1, selectedItem);
				m_listBox.SelectedItem = selectedItem;
			}
		}
	}

	private void RefreshList()
	{
		m_listBox.SuspendLayout();
		m_listBox.BeginUpdate();
		m_listBox.Items.Clear();
		foreach (string item in SearchPath)
		{
			m_listBox.Items.Add(item);
		}
		m_listBox.EndUpdate();
		m_listBox.ResumeLayout();
		m_clearButton.Enabled = m_listBox.Items.Count > 0;
	}

	private void OnOK(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		SearchPath.Clear();
		foreach (string item in m_listBox.Items)
		{
			SearchPath.Add(item);
		}
		Close();
	}

	private void OnCancel(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
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
		this.m_listBox = new System.Windows.Forms.ListBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_pathTextBox = new System.Windows.Forms.TextBox();
		this.m_browseButton = new System.Windows.Forms.Button();
		this.m_addButton = new System.Windows.Forms.Button();
		this.m_removeButton = new System.Windows.Forms.Button();
		this.m_clearButton = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_moveUpButton = new System.Windows.Forms.Button();
		this.m_moveDownButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_listBox.FormattingEnabled = true;
		this.m_listBox.Location = new System.Drawing.Point(93, 41);
		this.m_listBox.Name = "m_listBox";
		this.m_listBox.Size = new System.Drawing.Size(438, 290);
		this.m_listBox.TabIndex = 6;
		this.m_listBox.SelectedIndexChanged += new System.EventHandler(OnSelectedPathChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(55, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(32, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Path:";
		this.m_pathTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this.m_pathTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
		this.m_pathTextBox.Location = new System.Drawing.Point(93, 6);
		this.m_pathTextBox.Name = "m_pathTextBox";
		this.m_pathTextBox.Size = new System.Drawing.Size(308, 20);
		this.m_pathTextBox.TabIndex = 1;
		this.m_browseButton.Location = new System.Drawing.Point(407, 4);
		this.m_browseButton.Name = "m_browseButton";
		this.m_browseButton.Size = new System.Drawing.Size(24, 23);
		this.m_browseButton.TabIndex = 2;
		this.m_browseButton.Text = "...";
		this.m_browseButton.UseVisualStyleBackColor = true;
		this.m_browseButton.Click += new System.EventHandler(OnBrowse);
		this.m_addButton.Location = new System.Drawing.Point(465, 4);
		this.m_addButton.Name = "m_addButton";
		this.m_addButton.Size = new System.Drawing.Size(66, 23);
		this.m_addButton.TabIndex = 3;
		this.m_addButton.Text = "Add";
		this.m_addButton.UseVisualStyleBackColor = true;
		this.m_addButton.Click += new System.EventHandler(OnAdd);
		this.m_removeButton.Enabled = false;
		this.m_removeButton.Location = new System.Drawing.Point(93, 337);
		this.m_removeButton.Name = "m_removeButton";
		this.m_removeButton.Size = new System.Drawing.Size(75, 23);
		this.m_removeButton.TabIndex = 7;
		this.m_removeButton.Text = "Remove";
		this.m_removeButton.UseVisualStyleBackColor = true;
		this.m_removeButton.Click += new System.EventHandler(OnRemove);
		this.m_clearButton.Enabled = false;
		this.m_clearButton.Location = new System.Drawing.Point(174, 337);
		this.m_clearButton.Name = "m_clearButton";
		this.m_clearButton.Size = new System.Drawing.Size(75, 23);
		this.m_clearButton.TabIndex = 8;
		this.m_clearButton.Text = "Clear";
		this.m_clearButton.UseVisualStyleBackColor = true;
		this.m_clearButton.Click += new System.EventHandler(OnClear);
		this.m_errorProvider.ContainerControl = this;
		this.m_moveUpButton.Enabled = false;
		this.m_moveUpButton.Location = new System.Drawing.Point(12, 41);
		this.m_moveUpButton.Name = "m_moveUpButton";
		this.m_moveUpButton.Size = new System.Drawing.Size(75, 23);
		this.m_moveUpButton.TabIndex = 4;
		this.m_moveUpButton.Text = "Move Up";
		this.m_moveUpButton.UseVisualStyleBackColor = true;
		this.m_moveUpButton.Click += new System.EventHandler(OnMoveUp);
		this.m_moveDownButton.Enabled = false;
		this.m_moveDownButton.Location = new System.Drawing.Point(12, 70);
		this.m_moveDownButton.Name = "m_moveDownButton";
		this.m_moveDownButton.Size = new System.Drawing.Size(75, 23);
		this.m_moveDownButton.TabIndex = 5;
		this.m_moveDownButton.Text = "Move Down";
		this.m_moveDownButton.UseVisualStyleBackColor = true;
		this.m_moveDownButton.Click += new System.EventHandler(OnMoveDown);
		this.m_okButton.Location = new System.Drawing.Point(375, 337);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 9;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(456, 337);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 10;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(543, 368);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_moveDownButton);
		base.Controls.Add(this.m_moveUpButton);
		base.Controls.Add(this.m_clearButton);
		base.Controls.Add(this.m_removeButton);
		base.Controls.Add(this.m_addButton);
		base.Controls.Add(this.m_browseButton);
		base.Controls.Add(this.m_pathTextBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_listBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "EditSearchPathForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Edit Search Path";
		base.Load += new System.EventHandler(OnLoad);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
