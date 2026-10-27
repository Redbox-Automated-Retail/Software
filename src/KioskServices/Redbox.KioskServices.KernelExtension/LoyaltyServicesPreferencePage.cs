using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.KioskServices.KernelExtension;

public class LoyaltyServicesPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Label m_headerLabel;

	private CheckBox cb_LiveCall;

	private DataGridView dgv_accrualsRedemptions;

	private Label label1;

	private TextBox tb_accrualDefault;

	private TextBox tb_redemptionDefault;

	private Label label2;

	private CheckBox cb_simulateAccrualSuccess;

	private ComboBox cbb_accrualErrors;

	private ComboBox cbb_redemptionErrors;

	private Label label3;

	private Label label4;

	private CheckBox cb_simulateRedemptionSuccess;

	public LoyaltyServicesPreferencePage()
	{
		InitializeComponent();
		dgv_accrualsRedemptions.CurrentCell = null;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		SuspendLayout();
		dgv_accrualsRedemptions.ClearSelection();
		ILoyaltyServices service = ServiceLocator.Instance.GetService<ILoyaltyServices>();
		if (service != null && ServiceLocator.Instance.GetService<IRentalShoppingCartService>() != null)
		{
			cbb_accrualErrors.DataSource = Enum.GetValues(typeof(EstimateAccrualErrors));
			cbb_accrualErrors.SelectedItem = service.AccrualError;
			cbb_redemptionErrors.DataSource = Enum.GetValues(typeof(EstimateRedemptionErrors));
			cbb_redemptionErrors.SelectedItem = service.RedemptionError;
			tb_accrualDefault.Text = service.DefaultAccrualPoints.ToString();
			tb_redemptionDefault.Text = service.DefaultRedemptionPoints.ToString();
			cb_LiveCall.Checked = service.LiveCall;
			cb_simulateAccrualSuccess.Checked = service.SimulateAccrualSuccess;
			cb_simulateRedemptionSuccess.Checked = service.SimulateRedemptionSuccess;
			dgv_accrualsRedemptions.DataSource = service.Items;
			dgv_accrualsRedemptions.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			ResumeLayout();
		}
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		if (ServiceLocator.Instance.GetService<ILoyaltyServices>() == null)
		{
			return false;
		}
		return true;
	}

	private void OnLiveCallChanged(object sender, EventArgs e)
	{
		ILoyaltyServices service = ServiceLocator.Instance.GetService<ILoyaltyServices>();
		if (service != null)
		{
			service.LiveCall = cb_LiveCall.Checked;
		}
	}

	private void OnAccrualErrorChanged(object sender, EventArgs e)
	{
		ILoyaltyServices service = ServiceLocator.Instance.GetService<ILoyaltyServices>();
		if (service != null)
		{
			service.AccrualError = (EstimateAccrualErrors)cbb_accrualErrors.SelectedItem;
		}
	}

	private void OnRedemptionErrorChanged(object sender, EventArgs e)
	{
		ILoyaltyServices service = ServiceLocator.Instance.GetService<ILoyaltyServices>();
		if (service != null)
		{
			service.RedemptionError = (EstimateRedemptionErrors)cbb_redemptionErrors.SelectedItem;
		}
	}

	private void OnTextChanged(object sender, EventArgs e)
	{
		int result = 0;
		if (!(sender is TextBox textBox))
		{
			return;
		}
		if (!int.TryParse(textBox.Text, out result))
		{
			string text = textBox.Text;
			textBox.Text = text.Substring(text.Length - 2, 1);
			return;
		}
		ILoyaltyServices service = ServiceLocator.Instance.GetService<ILoyaltyServices>();
		if (service != null)
		{
			if (textBox.Name.Contains("accrual"))
			{
				service.DefaultAccrualPoints = result;
			}
			else
			{
				service.DefaultRedemptionPoints = result;
			}
		}
	}

	private void OnSimulateAccrualSuccessChanged(object sender, EventArgs e)
	{
		ILoyaltyServices service = ServiceLocator.Instance.GetService<ILoyaltyServices>();
		if (service != null)
		{
			service.SimulateAccrualSuccess = cb_simulateAccrualSuccess.Checked;
		}
	}

	private void OnSimulateRedemptionSuccessChanged(object sender, EventArgs e)
	{
		ILoyaltyServices service = ServiceLocator.Instance.GetService<ILoyaltyServices>();
		if (service != null)
		{
			service.SimulateRedemptionSuccess = cb_simulateRedemptionSuccess.Checked;
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
		this.cb_LiveCall = new System.Windows.Forms.CheckBox();
		this.dgv_accrualsRedemptions = new System.Windows.Forms.DataGridView();
		this.label1 = new System.Windows.Forms.Label();
		this.tb_accrualDefault = new System.Windows.Forms.TextBox();
		this.tb_redemptionDefault = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.cb_simulateAccrualSuccess = new System.Windows.Forms.CheckBox();
		this.cbb_accrualErrors = new System.Windows.Forms.ComboBox();
		this.cbb_redemptionErrors = new System.Windows.Forms.ComboBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.cb_simulateRedemptionSuccess = new System.Windows.Forms.CheckBox();
		((System.ComponentModel.ISupportInitialize)this.dgv_accrualsRedemptions).BeginInit();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 43;
		this.m_headerLabel.Text = "Loyalty Services: Accruals and Redemptions";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cb_LiveCall.AutoSize = true;
		this.cb_LiveCall.Location = new System.Drawing.Point(17, 54);
		this.cb_LiveCall.Name = "cb_LiveCall";
		this.cb_LiveCall.Size = new System.Drawing.Size(66, 17);
		this.cb_LiveCall.TabIndex = 66;
		this.cb_LiveCall.Text = "Live Call";
		this.cb_LiveCall.UseVisualStyleBackColor = true;
		this.cb_LiveCall.CheckedChanged += new System.EventHandler(OnLiveCallChanged);
		this.dgv_accrualsRedemptions.AllowUserToAddRows = false;
		this.dgv_accrualsRedemptions.AllowUserToDeleteRows = false;
		this.dgv_accrualsRedemptions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgv_accrualsRedemptions.Location = new System.Drawing.Point(17, 217);
		this.dgv_accrualsRedemptions.Name = "dgv_accrualsRedemptions";
		this.dgv_accrualsRedemptions.RowHeadersVisible = false;
		this.dgv_accrualsRedemptions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
		this.dgv_accrualsRedemptions.Size = new System.Drawing.Size(506, 313);
		this.dgv_accrualsRedemptions.TabIndex = 71;
		this.dgv_accrualsRedemptions.TabStop = false;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(14, 194);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(113, 13);
		this.label1.TabIndex = 73;
		this.label1.Text = "Default Accrual Value:";
		this.tb_accrualDefault.Location = new System.Drawing.Point(133, 191);
		this.tb_accrualDefault.Name = "tb_accrualDefault";
		this.tb_accrualDefault.Size = new System.Drawing.Size(108, 20);
		this.tb_accrualDefault.TabIndex = 74;
		this.tb_accrualDefault.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.tb_accrualDefault.TextChanged += new System.EventHandler(OnTextChanged);
		this.tb_redemptionDefault.Location = new System.Drawing.Point(416, 191);
		this.tb_redemptionDefault.Name = "tb_redemptionDefault";
		this.tb_redemptionDefault.Size = new System.Drawing.Size(107, 20);
		this.tb_redemptionDefault.TabIndex = 76;
		this.tb_redemptionDefault.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.tb_redemptionDefault.TextChanged += new System.EventHandler(OnTextChanged);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(276, 194);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(134, 13);
		this.label2.TabIndex = 75;
		this.label2.Text = "Default Redemption Value:";
		this.cb_simulateAccrualSuccess.AutoSize = true;
		this.cb_simulateAccrualSuccess.Location = new System.Drawing.Point(251, 54);
		this.cb_simulateAccrualSuccess.Name = "cb_simulateAccrualSuccess";
		this.cb_simulateAccrualSuccess.Size = new System.Drawing.Size(149, 17);
		this.cb_simulateAccrualSuccess.TabIndex = 77;
		this.cb_simulateAccrualSuccess.Text = "Simulate Accrual Success";
		this.cb_simulateAccrualSuccess.UseVisualStyleBackColor = true;
		this.cb_simulateAccrualSuccess.CheckedChanged += new System.EventHandler(OnSimulateAccrualSuccessChanged);
		this.cbb_accrualErrors.FormattingEnabled = true;
		this.cbb_accrualErrors.Location = new System.Drawing.Point(377, 76);
		this.cbb_accrualErrors.Name = "cbb_accrualErrors";
		this.cbb_accrualErrors.Size = new System.Drawing.Size(142, 21);
		this.cbb_accrualErrors.TabIndex = 78;
		this.cbb_accrualErrors.SelectionChangeCommitted += new System.EventHandler(OnAccrualErrorChanged);
		this.cbb_redemptionErrors.FormattingEnabled = true;
		this.cbb_redemptionErrors.Location = new System.Drawing.Point(377, 127);
		this.cbb_redemptionErrors.Name = "cbb_redemptionErrors";
		this.cbb_redemptionErrors.Size = new System.Drawing.Size(142, 21);
		this.cbb_redemptionErrors.TabIndex = 79;
		this.cbb_redemptionErrors.SelectionChangeCommitted += new System.EventHandler(OnRedemptionErrorChanged);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(294, 79);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(76, 13);
		this.label3.TabIndex = 80;
		this.label3.Text = "Accrual Errors:";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(273, 130);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(97, 13);
		this.label4.TabIndex = 81;
		this.label4.Text = "Redemption Errors:";
		this.cb_simulateRedemptionSuccess.AutoSize = true;
		this.cb_simulateRedemptionSuccess.Location = new System.Drawing.Point(251, 103);
		this.cb_simulateRedemptionSuccess.Name = "cb_simulateRedemptionSuccess";
		this.cb_simulateRedemptionSuccess.Size = new System.Drawing.Size(170, 17);
		this.cb_simulateRedemptionSuccess.TabIndex = 82;
		this.cb_simulateRedemptionSuccess.Text = "Simulate Redemption Success";
		this.cb_simulateRedemptionSuccess.UseVisualStyleBackColor = true;
		this.cb_simulateRedemptionSuccess.CheckedChanged += new System.EventHandler(OnSimulateRedemptionSuccessChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.cb_simulateRedemptionSuccess);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.cbb_redemptionErrors);
		base.Controls.Add(this.cbb_accrualErrors);
		base.Controls.Add(this.cb_simulateAccrualSuccess);
		base.Controls.Add(this.tb_redemptionDefault);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.tb_accrualDefault);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.dgv_accrualsRedemptions);
		base.Controls.Add(this.cb_LiveCall);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "LoyaltyServicesPreferencePage";
		base.Size = new System.Drawing.Size(542, 533);
		((System.ComponentModel.ISupportInitialize)this.dgv_accrualsRedemptions).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
