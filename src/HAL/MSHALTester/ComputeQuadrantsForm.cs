using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.MSHALTester;

public class ComputeQuadrantsForm : Form
{
	private IContainer components;

	private Label label1;

	private TextBox m_startOffsetTextBox;

	private Label label2;

	private TextBox m_numberOfQuadrantsTextBox;

	private ErrorProvider m_errorProvider;

	private Button m_okButton;

	private Button m_cancelButton;

	private TextBox m_slotsPerQuadrantTextBox;

	private Label label3;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public decimal? StartOffset
	{
		get
		{
			if (decimal.TryParse(m_startOffsetTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_startOffsetTextBox.Clear();
			if (value.HasValue)
			{
				m_startOffsetTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? NumberOfQuadrants
	{
		get
		{
			if (int.TryParse(m_numberOfQuadrantsTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_numberOfQuadrantsTextBox.Clear();
			if (value.HasValue)
			{
				m_numberOfQuadrantsTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? SlotsPerQuadrant
	{
		get
		{
			if (int.TryParse(m_slotsPerQuadrantTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_slotsPerQuadrantTextBox.Clear();
			if (value.HasValue)
			{
				m_slotsPerQuadrantTextBox.Text = value.ToString();
			}
		}
	}

	public ComputeQuadrantsForm()
	{
		InitializeComponent();
	}

	private void OnOK(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_startOffsetTextBox, string.Empty);
		m_errorProvider.SetError(m_numberOfQuadrantsTextBox, string.Empty);
		if (!StartOffset.HasValue)
		{
			m_errorProvider.SetError(m_startOffsetTextBox, "Start Offset is a required value.");
			return;
		}
		if (!NumberOfQuadrants.HasValue)
		{
			m_errorProvider.SetError(m_numberOfQuadrantsTextBox, "Number of Quadrants is a required value.");
			return;
		}
		if (!SlotsPerQuadrant.HasValue)
		{
			m_errorProvider.SetError(m_slotsPerQuadrantTextBox, "Slots per Quadrant is a required value.");
			return;
		}
		decimal? startOffset = StartOffset;
		if ((startOffset.GetValueOrDefault() < 0m) & startOffset.HasValue)
		{
			m_errorProvider.SetError(m_startOffsetTextBox, "Start Offset must be a positive value.");
			return;
		}
		if (NumberOfQuadrants < 0)
		{
			m_errorProvider.SetError(m_numberOfQuadrantsTextBox, "Number of Quadrants must be a positive value.");
			return;
		}
		if (SlotsPerQuadrant < 0)
		{
			m_errorProvider.SetError(m_slotsPerQuadrantTextBox, "Slots per Quadrant must be a positive value.");
			return;
		}
		base.DialogResult = DialogResult.OK;
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
		this.label1 = new System.Windows.Forms.Label();
		this.m_startOffsetTextBox = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_numberOfQuadrantsTextBox = new System.Windows.Forms.TextBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.label3 = new System.Windows.Forms.Label();
		this.m_slotsPerQuadrantTextBox = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(63, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Start Offset:";
		this.m_startOffsetTextBox.Location = new System.Drawing.Point(129, 9);
		this.m_startOffsetTextBox.Name = "m_startOffsetTextBox";
		this.m_startOffsetTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_startOffsetTextBox.TabIndex = 1;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 37);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(111, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Number of Quadrants:";
		this.m_numberOfQuadrantsTextBox.Enabled = false;
		this.m_numberOfQuadrantsTextBox.Location = new System.Drawing.Point(129, 34);
		this.m_numberOfQuadrantsTextBox.Name = "m_numberOfQuadrantsTextBox";
		this.m_numberOfQuadrantsTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_numberOfQuadrantsTextBox.TabIndex = 3;
		this.m_errorProvider.ContainerControl = this;
		this.m_okButton.Location = new System.Drawing.Point(85, 100);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 6;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(166, 100);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 7;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(12, 63);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(98, 13);
		this.label3.TabIndex = 4;
		this.label3.Text = "Slots per Quadrant:";
		this.m_slotsPerQuadrantTextBox.Enabled = false;
		this.m_slotsPerQuadrantTextBox.Location = new System.Drawing.Point(129, 60);
		this.m_slotsPerQuadrantTextBox.Name = "m_slotsPerQuadrantTextBox";
		this.m_slotsPerQuadrantTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_slotsPerQuadrantTextBox.TabIndex = 5;
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(253, 135);
		base.ControlBox = false;
		base.Controls.Add(this.m_slotsPerQuadrantTextBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_numberOfQuadrantsTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_startOffsetTextBox);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "ComputeQuadrantsForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Compute Quadrants";
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
