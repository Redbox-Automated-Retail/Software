using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.HardwareServices.TestProxy;
using Redbox.HardwareServices.TestProxy.Interfaces;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.KernelExtension;

public class ReturnPreferencePage : UserControl, IPreferencePageHost
{
	private readonly TestHardwareService _hardwareService;

	private readonly IInventoryTestService _inventoryService;

	private IContainer components;

	private Label m_headerLabel;

	private Label label1;

	private Button btn_unknownBarcode;

	private TextBox txt_barcodeInput;

	private ComboBox cmb_returnType;

	private Label label2;

	private Button btn_addBarcode;

	private DataGridView dgv_returnQueue;

	private DataGridViewTextBoxColumn Barcodes;

	public ReturnPreferencePage()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		base._002Ector();
		InitializeComponent();
		_hardwareService = (TestHardwareService)ServiceLocator.Instance.GetService<IHardwareService>();
		_inventoryService = ServiceLocator.Instance.GetService<IInventoryTestService>();
		LoadHardwareInfo();
	}

	private void LoadHardwareInfo()
	{
		dgv_returnQueue.DataSource = _inventoryService.VendedInventory;
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		cmb_returnType.DataSource = Enum.GetValues(typeof(ReturnType));
		cmb_returnType.SelectedItem = _hardwareService.ReturnType;
	}

	private void OnAddUnknown(object sender, EventArgs e)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		(dgv_returnQueue.DataSource as BindingList<InventoryData>)?.Add(new InventoryData
		{
			Barcode = "UNKNOWN"
		});
	}

	private void OnAddBarcode(object sender, EventArgs e)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (dgv_returnQueue.DataSource is BindingList<InventoryData> bindingList && !string.IsNullOrWhiteSpace(txt_barcodeInput.Text))
		{
			bindingList.Add(new InventoryData
			{
				Barcode = txt_barcodeInput.Text
			});
			txt_barcodeInput.Text = string.Empty;
		}
	}

	private void OnReturnTypeChanged(object sender, EventArgs e)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_hardwareService.ReturnType = (ReturnType)cmb_returnType.SelectedItem;
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
		this.label1 = new System.Windows.Forms.Label();
		this.btn_unknownBarcode = new System.Windows.Forms.Button();
		this.txt_barcodeInput = new System.Windows.Forms.TextBox();
		this.cmb_returnType = new System.Windows.Forms.ComboBox();
		this.label2 = new System.Windows.Forms.Label();
		this.btn_addBarcode = new System.Windows.Forms.Button();
		this.dgv_returnQueue = new System.Windows.Forms.DataGridView();
		this.Barcodes = new System.Windows.Forms.DataGridViewTextBoxColumn();
		((System.ComponentModel.ISupportInitialize)this.dgv_returnQueue).BeginInit();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.Gold;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.Black;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 7;
		this.m_headerLabel.Text = "  Hardware Service: Return";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.Location = new System.Drawing.Point(340, 42);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(154, 24);
		this.label1.TabIndex = 9;
		this.label1.Text = "Return Queue: ";
		this.btn_unknownBarcode.Location = new System.Drawing.Point(340, 422);
		this.btn_unknownBarcode.Name = "btn_unknownBarcode";
		this.btn_unknownBarcode.Size = new System.Drawing.Size(91, 42);
		this.btn_unknownBarcode.TabIndex = 10;
		this.btn_unknownBarcode.Text = "Add Unknown";
		this.btn_unknownBarcode.UseVisualStyleBackColor = true;
		this.btn_unknownBarcode.Click += new System.EventHandler(OnAddUnknown);
		this.txt_barcodeInput.Location = new System.Drawing.Point(340, 396);
		this.txt_barcodeInput.Name = "txt_barcodeInput";
		this.txt_barcodeInput.Size = new System.Drawing.Size(184, 20);
		this.txt_barcodeInput.TabIndex = 11;
		this.cmb_returnType.FormattingEnabled = true;
		this.cmb_returnType.Location = new System.Drawing.Point(16, 88);
		this.cmb_returnType.Name = "cmb_returnType";
		this.cmb_returnType.Size = new System.Drawing.Size(180, 21);
		this.cmb_returnType.TabIndex = 12;
		this.cmb_returnType.SelectedIndexChanged += new System.EventHandler(OnReturnTypeChanged);
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.Location = new System.Drawing.Point(13, 69);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(97, 16);
		this.label2.TabIndex = 13;
		this.label2.Text = "Return Type:";
		this.btn_addBarcode.Location = new System.Drawing.Point(437, 422);
		this.btn_addBarcode.Name = "btn_addBarcode";
		this.btn_addBarcode.Size = new System.Drawing.Size(88, 42);
		this.btn_addBarcode.TabIndex = 17;
		this.btn_addBarcode.Text = "Add Barcode";
		this.btn_addBarcode.UseVisualStyleBackColor = true;
		this.btn_addBarcode.Click += new System.EventHandler(OnAddBarcode);
		this.dgv_returnQueue.AllowUserToAddRows = false;
		this.dgv_returnQueue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgv_returnQueue.ColumnHeadersVisible = false;
		this.dgv_returnQueue.Columns.AddRange(this.Barcodes);
		this.dgv_returnQueue.Location = new System.Drawing.Point(340, 70);
		this.dgv_returnQueue.Name = "dgv_returnQueue";
		this.dgv_returnQueue.ReadOnly = true;
		this.dgv_returnQueue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.dgv_returnQueue.Size = new System.Drawing.Size(184, 320);
		this.dgv_returnQueue.TabIndex = 18;
		this.Barcodes.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.Barcodes.DataPropertyName = "Barcode";
		this.Barcodes.HeaderText = "";
		this.Barcodes.Name = "Barcodes";
		this.Barcodes.ReadOnly = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.dgv_returnQueue);
		base.Controls.Add(this.btn_addBarcode);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.cmb_returnType);
		base.Controls.Add(this.txt_barcodeInput);
		base.Controls.Add(this.btn_unknownBarcode);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "ReturnPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.dgv_returnQueue).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
