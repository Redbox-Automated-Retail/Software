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

public class VendPreferencePage : UserControl, IPreferencePageHost
{
	private readonly TestHardwareService _hardwareService;

	private readonly IInventoryTestService _inventoryService;

	private bool _initializing;

	private IContainer components;

	private Label m_headerLabel;

	private Label label2;

	private ComboBox cmb_vendResult;

	public VendPreferencePage()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		base._002Ector();
		InitializeComponent();
		_hardwareService = (TestHardwareService)ServiceLocator.Instance.GetService<IHardwareService>();
		_inventoryService = ServiceLocator.Instance.GetService<IInventoryTestService>();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		_initializing = true;
		cmb_vendResult.DataSource = Enum.GetValues(typeof(PreposResultType));
		cmb_vendResult.SelectedItem = _hardwareService.VendResult;
		_initializing = false;
	}

	private void cmb_vendResult_SelectedValueChanged(object sender, EventArgs e)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!_initializing)
		{
			_hardwareService.VendResult = (PreposResultType)cmb_vendResult.SelectedItem;
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
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.cmb_vendResult = new System.Windows.Forms.ComboBox();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.Gold;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.Black;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 8;
		this.m_headerLabel.Text = "  Hardware Service: Vend";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.Location = new System.Drawing.Point(15, 62);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(96, 16);
		this.label2.TabIndex = 15;
		this.label2.Text = "Vend Result:";
		this.cmb_vendResult.FormattingEnabled = true;
		this.cmb_vendResult.Location = new System.Drawing.Point(18, 81);
		this.cmb_vendResult.Name = "cmb_vendResult";
		this.cmb_vendResult.Size = new System.Drawing.Size(180, 21);
		this.cmb_vendResult.TabIndex = 14;
		this.cmb_vendResult.SelectedValueChanged += new System.EventHandler(cmb_vendResult_SelectedValueChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.label2);
		base.Controls.Add(this.cmb_vendResult);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "VendPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
