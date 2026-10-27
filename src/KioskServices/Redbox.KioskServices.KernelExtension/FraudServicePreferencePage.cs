using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.KioskServices.KernelExtension;

public class FraudServicePreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Label m_headerLabel;

	private CheckBox cb_irEnabled;

	private CheckBox cb_paperEnabled;

	private CheckBox cb_peeledEnabled;

	private ListBox m_paperExclusionList;

	private ListBox m_peeledInclusionList;

	private Label label1;

	private Label label2;

	private Label lbl_error;

	public FraudServicePreferencePage()
	{
		InitializeComponent();
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		IFraudService service = ServiceLocator.Instance.GetService<IFraudService>();
		lbl_error.Text = ((service == null) ? "FAILED TO LOAD FRAUD SERVICE" : string.Empty);
		m_paperExclusionList.DataSource = service.FraudPaperExcludedFormats;
		m_peeledInclusionList.DataSource = service.FraudPeelIncludeFormats.ToList();
		cb_irEnabled.Checked = service.HardwareSupportsIRScan;
		cb_paperEnabled.Checked = service.IsIRSecurityEnabled;
		cb_peeledEnabled.Checked = service.IsFraudImageEnabled;
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
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
		this.cb_irEnabled = new System.Windows.Forms.CheckBox();
		this.cb_paperEnabled = new System.Windows.Forms.CheckBox();
		this.cb_peeledEnabled = new System.Windows.Forms.CheckBox();
		this.m_paperExclusionList = new System.Windows.Forms.ListBox();
		this.m_peeledInclusionList = new System.Windows.Forms.ListBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.lbl_error = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 39;
		this.m_headerLabel.Text = "FraudService";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cb_irEnabled.AutoSize = true;
		this.cb_irEnabled.Enabled = false;
		this.cb_irEnabled.Location = new System.Drawing.Point(47, 109);
		this.cb_irEnabled.Name = "cb_irEnabled";
		this.cb_irEnabled.Size = new System.Drawing.Size(128, 17);
		this.cb_irEnabled.TabIndex = 40;
		this.cb_irEnabled.Text = "Hardware IR Enabled";
		this.cb_irEnabled.UseVisualStyleBackColor = true;
		this.cb_paperEnabled.AutoSize = true;
		this.cb_paperEnabled.Enabled = false;
		this.cb_paperEnabled.Location = new System.Drawing.Point(47, 132);
		this.cb_paperEnabled.Name = "cb_paperEnabled";
		this.cb_paperEnabled.Size = new System.Drawing.Size(96, 17);
		this.cb_paperEnabled.TabIndex = 41;
		this.cb_paperEnabled.Text = "Paper Enabled";
		this.cb_paperEnabled.UseVisualStyleBackColor = true;
		this.cb_peeledEnabled.AutoSize = true;
		this.cb_peeledEnabled.Enabled = false;
		this.cb_peeledEnabled.Location = new System.Drawing.Point(47, 155);
		this.cb_peeledEnabled.Name = "cb_peeledEnabled";
		this.cb_peeledEnabled.Size = new System.Drawing.Size(101, 17);
		this.cb_peeledEnabled.TabIndex = 42;
		this.cb_peeledEnabled.Text = "Peeled Enabled";
		this.cb_peeledEnabled.UseVisualStyleBackColor = true;
		this.m_paperExclusionList.FormattingEnabled = true;
		this.m_paperExclusionList.Location = new System.Drawing.Point(47, 216);
		this.m_paperExclusionList.Name = "m_paperExclusionList";
		this.m_paperExclusionList.Size = new System.Drawing.Size(203, 134);
		this.m_paperExclusionList.TabIndex = 43;
		this.m_peeledInclusionList.FormattingEnabled = true;
		this.m_peeledInclusionList.Location = new System.Drawing.Point(308, 216);
		this.m_peeledInclusionList.Name = "m_peeledInclusionList";
		this.m_peeledInclusionList.Size = new System.Drawing.Size(222, 134);
		this.m_peeledInclusionList.TabIndex = 44;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(44, 200);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(91, 13);
		this.label1.TabIndex = 45;
		this.label1.Text = "Paper Exclusions:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(305, 200);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(93, 13);
		this.label2.TabIndex = 46;
		this.label2.Text = "Peeled Inclusions:";
		this.lbl_error.AutoSize = true;
		this.lbl_error.ForeColor = System.Drawing.Color.Red;
		this.lbl_error.Location = new System.Drawing.Point(157, 57);
		this.lbl_error.Name = "lbl_error";
		this.lbl_error.Size = new System.Drawing.Size(0, 13);
		this.lbl_error.TabIndex = 47;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.lbl_error);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_peeledInclusionList);
		base.Controls.Add(this.m_paperExclusionList);
		base.Controls.Add(this.cb_peeledEnabled);
		base.Controls.Add(this.cb_paperEnabled);
		base.Controls.Add(this.cb_irEnabled);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "FraudServicePreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
