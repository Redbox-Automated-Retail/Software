using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.HardwareServices.TestProxy;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.FraudServices;

namespace Redbox.HardwareServices.KernelExtension;

public class HardwareStatePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Label m_headerLabel;

	private ComboBox cmb_cameraGeneration;

	private Label label1;

	private Label label2;

	private ComboBox cmb_barcodeDecoder;

	private Label label3;

	private Label label4;

	private TextBox textBoxReadAttempts;

	private ContextMenuStrip contextMenuStrip1;

	private TextBox textBoxBarcodesRead;

	private Label label5;

	private TextBox textBoxSecureBarcodesRead;

	private Label label6;

	public HardwareStatePage()
	{
		InitializeComponent();
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		IHardwareState service = ServiceLocator.Instance.GetService<IHardwareState>();
		cmb_cameraGeneration.DataSource = Enum.GetValues(typeof(CameraGeneration));
		cmb_cameraGeneration.SelectedItem = (CameraGeneration)service.CurrentCameraGeneration;
		cmb_barcodeDecoder.DataSource = Enum.GetValues(typeof(BarcodeServices));
		cmb_barcodeDecoder.SelectedItem = (BarcodeServices)service.BarcodeDecoder;
		textBoxBarcodesRead.Text = TestHardwareService.Instance.CameraReadStatsBarcodesRead.ToString();
		textBoxReadAttempts.Text = TestHardwareService.Instance.CameraReadStatsReadAttemps.ToString();
		textBoxSecureBarcodesRead.Text = TestHardwareService.Instance.CameraReadStatsSecureBarcodesRead.ToString();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		if (cmb_cameraGeneration.SelectedItem != null)
		{
			TestHardwareService.Instance.CurrentCameraGeneration = (int)cmb_cameraGeneration.SelectedItem;
		}
		if (cmb_barcodeDecoder.SelectedItem != null)
		{
			TestHardwareService.Instance.BarcodeDecoder = (int)cmb_barcodeDecoder.SelectedItem;
		}
		if (int.TryParse(textBoxSecureBarcodesRead.Text, out var result))
		{
			TestHardwareService.Instance.CameraReadStatsSecureBarcodesRead = result;
		}
		if (int.TryParse(textBoxBarcodesRead.Text, out var result2))
		{
			TestHardwareService.Instance.CameraReadStatsBarcodesRead = result2;
		}
		if (int.TryParse(textBoxReadAttempts.Text, out var result3))
		{
			TestHardwareService.Instance.CameraReadStatsReadAttemps = result3;
		}
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
		this.components = new System.ComponentModel.Container();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.cmb_cameraGeneration = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.cmb_barcodeDecoder = new System.Windows.Forms.ComboBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.textBoxReadAttempts = new System.Windows.Forms.TextBox();
		this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.textBoxBarcodesRead = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.textBoxSecureBarcodesRead = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.Gold;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.Black;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 7;
		this.m_headerLabel.Text = "  Hardware Service: Hardware State";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cmb_cameraGeneration.FormattingEnabled = true;
		this.cmb_cameraGeneration.Location = new System.Drawing.Point(125, 55);
		this.cmb_cameraGeneration.Name = "cmb_cameraGeneration";
		this.cmb_cameraGeneration.Size = new System.Drawing.Size(140, 21);
		this.cmb_cameraGeneration.TabIndex = 8;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(18, 55);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(101, 13);
		this.label1.TabIndex = 9;
		this.label1.Text = "Camera Generation:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(18, 82);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(94, 13);
		this.label2.TabIndex = 10;
		this.label2.Text = "Barcode Decoder:";
		this.cmb_barcodeDecoder.FormattingEnabled = true;
		this.cmb_barcodeDecoder.Location = new System.Drawing.Point(125, 82);
		this.cmb_barcodeDecoder.Name = "cmb_barcodeDecoder";
		this.cmb_barcodeDecoder.Size = new System.Drawing.Size(140, 21);
		this.cmb_barcodeDecoder.TabIndex = 11;
		this.label3.AutoSize = true;
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label3.Location = new System.Drawing.Point(18, 138);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(269, 13);
		this.label3.TabIndex = 12;
		this.label3.Text = "Camera Read Stats (used in barcode tracking)";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(18, 162);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(80, 13);
		this.label4.TabIndex = 13;
		this.label4.Text = "Read Attempts:";
		this.textBoxReadAttempts.Location = new System.Drawing.Point(151, 159);
		this.textBoxReadAttempts.Name = "textBoxReadAttempts";
		this.textBoxReadAttempts.Size = new System.Drawing.Size(46, 20);
		this.textBoxReadAttempts.TabIndex = 14;
		this.contextMenuStrip1.Name = "contextMenuStrip1";
		this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
		this.textBoxBarcodesRead.Location = new System.Drawing.Point(151, 185);
		this.textBoxBarcodesRead.Name = "textBoxBarcodesRead";
		this.textBoxBarcodesRead.Size = new System.Drawing.Size(46, 20);
		this.textBoxBarcodesRead.TabIndex = 17;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(18, 188);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(84, 13);
		this.label5.TabIndex = 16;
		this.label5.Text = "Barcodes Read:";
		this.textBoxSecureBarcodesRead.Location = new System.Drawing.Point(151, 211);
		this.textBoxSecureBarcodesRead.Name = "textBoxSecureBarcodesRead";
		this.textBoxSecureBarcodesRead.Size = new System.Drawing.Size(46, 20);
		this.textBoxSecureBarcodesRead.TabIndex = 19;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(18, 214);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(121, 13);
		this.label6.TabIndex = 18;
		this.label6.Text = "Secure Barcodes Read:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.textBoxSecureBarcodesRead);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.textBoxBarcodesRead);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.textBoxReadAttempts);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.cmb_barcodeDecoder);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.cmb_cameraGeneration);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "HardwareStatePage";
		base.Size = new System.Drawing.Size(542, 506);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
