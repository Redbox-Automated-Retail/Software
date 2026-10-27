using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;

namespace Redbox.KioskServices.KernelExtension;

public class CustomerOffersPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private CheckBox m_liveCallCheckBox;

	private CheckBox m_simulateSuccessCheckBox;

	private CheckBox m_includeFreeGameInCartOfferCheckbox;

	private CheckBox m_includeMovieInCart;

	private RadioButton rbInCart;

	private RadioButton rbFracFreeMovieStorable;

	private RadioButton rbFracFreeMovieRedeemable;

	private RadioButton rbFracDiscountStorable;

	private RadioButton rbFracDiscountRedeemable;

	private RadioButton rbNoOffer;

	private CheckBox m_includeMultiNightOfferCheckbox;

	private TextBox txtProductIds;

	private Label label1;

	private Label label2;

	private RadioButton rbPerksFreeDisk;

	private RadioButton rbPerksAmountOff;

	private RadioButton rbPerksMessage;

	private RadioButton rbPerksExtraPoints;

	private CheckBox m_includePurchaseAmtOffInCartOfferCheckbox;

	public CustomerOffersPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		List<long> list = new List<long>();
		string[] array = txtProductIds.Text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (long.TryParse(array[i], out var result))
			{
				list.Add(result);
			}
		}
		CustomerOfferProxy.Instance.MnpProductIds = list;
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_isLoaded = false;
		m_liveCallCheckBox.Checked = CustomerOfferProxy.Instance.LiveCall;
		m_simulateSuccessCheckBox.Checked = CustomerOfferProxy.Instance.SimulateSuccess;
		rbInCart.Checked = CustomerOfferProxy.Instance.IncludeFreeGameInCartOffer || CustomerOfferProxy.Instance.IncludeMovieDiscountInCartOffer || CustomerOfferProxy.Instance.IncludeMNPInCartOffer;
		rbFracDiscountRedeemable.Checked = CustomerOfferProxy.Instance.IncludeFracRedeemableDiscountedDiscOffer;
		rbFracDiscountStorable.Checked = CustomerOfferProxy.Instance.IncludeFracStorableDiscountedDiscOffer;
		rbFracFreeMovieRedeemable.Checked = CustomerOfferProxy.Instance.IncludeFracRedeemableFreeMovieOffer;
		rbFracFreeMovieStorable.Checked = CustomerOfferProxy.Instance.IncludeFracStorableFreeMovieOffer;
		rbPerksFreeDisk.Checked = CustomerOfferProxy.Instance.IncludePerksFreeRentalOffer;
		m_includeFreeGameInCartOfferCheckbox.Checked = CustomerOfferProxy.Instance.IncludeFreeGameInCartOffer;
		m_includeMovieInCart.Checked = CustomerOfferProxy.Instance.IncludeMovieDiscountInCartOffer;
		m_includeMultiNightOfferCheckbox.Checked = CustomerOfferProxy.Instance.IncludeMNPInCartOffer;
		m_includePurchaseAmtOffInCartOfferCheckbox.Checked = CustomerOfferProxy.Instance.IncludePurchaseOffer;
		rbPerksAmountOff.Checked = CustomerOfferProxy.Instance.IncludePerksAmountOffOffer;
		rbPerksFreeDisk.Checked = CustomerOfferProxy.Instance.IncludePerksFreeRentalOffer;
		rbPerksMessage.Checked = CustomerOfferProxy.Instance.IncludePerksReminderMessage;
		rbPerksExtraPoints.Checked = CustomerOfferProxy.Instance.IncludePerksExtraPointsOffer;
		rbNoOffer.Checked = !rbFracDiscountRedeemable.Checked && !rbFracDiscountStorable.Checked && !rbFracFreeMovieRedeemable.Checked && !rbFracFreeMovieStorable.Checked && !rbInCart.Checked && !rbPerksAmountOff.Checked && !rbPerksFreeDisk.Checked && !rbPerksMessage.Checked && !rbPerksExtraPoints.Checked;
		txtProductIds.Text = string.Join(",", CustomerOfferProxy.Instance.MnpProductIds);
		m_isLoaded = true;
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.LiveCall = m_liveCallCheckBox.Checked;
		}
	}

	private void OnSimulateSuccessCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.SimulateSuccess = m_simulateSuccessCheckBox.Checked;
		}
	}

	private void movieInCartOffer_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeMovieDiscountInCartOffer = m_includeMovieInCart.Checked;
		}
	}

	private void m_includeFreeGameInCartOfferCheckbox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeFreeGameInCartOffer = m_includeFreeGameInCartOfferCheckbox.Checked;
		}
	}

	private void rbPresentInCart_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			if (rbInCart.Checked)
			{
				m_includeMovieInCart.Enabled = true;
				m_includeFreeGameInCartOfferCheckbox.Enabled = true;
				m_includeMultiNightOfferCheckbox.Enabled = true;
				m_includePurchaseAmtOffInCartOfferCheckbox.Enabled = true;
				txtProductIds.Enabled = m_includeMultiNightOfferCheckbox.Checked;
				CustomerOfferProxy.Instance.IncludeFracRedeemableDiscountedDiscOffer = false;
				CustomerOfferProxy.Instance.IncludeFracStorableDiscountedDiscOffer = false;
				CustomerOfferProxy.Instance.IncludeFracRedeemableFreeMovieOffer = false;
				CustomerOfferProxy.Instance.IncludeFracStorableFreeMovieOffer = false;
				CustomerOfferProxy.Instance.IncludePerksAmountOffOffer = false;
				CustomerOfferProxy.Instance.IncludePerksExtraPointsOffer = false;
				CustomerOfferProxy.Instance.IncludePerksFreeRentalOffer = false;
				CustomerOfferProxy.Instance.IncludePerksReminderMessage = false;
				CustomerOfferProxy.Instance.IncludePerksExtraPointsOffer = false;
			}
			else
			{
				m_includeMovieInCart.Enabled = false;
				m_includeMovieInCart.Checked = false;
				m_includeFreeGameInCartOfferCheckbox.Enabled = false;
				m_includeFreeGameInCartOfferCheckbox.Checked = false;
				m_includeMultiNightOfferCheckbox.Checked = false;
				m_includeMultiNightOfferCheckbox.Enabled = false;
				m_includePurchaseAmtOffInCartOfferCheckbox.Enabled = false;
				m_includePurchaseAmtOffInCartOfferCheckbox.Checked = false;
				txtProductIds.Enabled = false;
			}
		}
	}

	private void rbFracFreeMovieStorable_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeFracStorableFreeMovieOffer = rbFracFreeMovieStorable.Checked;
		}
	}

	private void rbFracFreeMovieRedeemable_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeFracRedeemableFreeMovieOffer = rbFracFreeMovieRedeemable.Checked;
		}
	}

	private void rbFracDiscountStorable_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeFracStorableDiscountedDiscOffer = rbFracDiscountStorable.Checked;
		}
	}

	private void rbFracDiscountRedeemable_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeFracRedeemableDiscountedDiscOffer = rbFracDiscountRedeemable.Checked;
		}
	}

	private void m_includeMultiNightOfferCheckbox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeMNPInCartOffer = m_includeMultiNightOfferCheckbox.Checked;
			txtProductIds.Enabled = m_includeMultiNightOfferCheckbox.Checked;
		}
	}

	private void RbPerksFreeDisk_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludePerksFreeRentalOffer = rbPerksFreeDisk.Checked;
		}
	}

	private void RbPerksAmountOff_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludePerksAmountOffOffer = rbPerksAmountOff.Checked;
		}
	}

	private void RbPerksMessage_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludePerksReminderMessage = rbPerksMessage.Checked;
		}
	}

	private void RbPerksExtraPoints_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludePerksExtraPointsOffer = rbPerksExtraPoints.Checked;
		}
	}

	private void m_includePurchaseAmtOffInCartOfferCheckbox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludePurchaseOffer = m_includePurchaseAmtOffInCartOfferCheckbox.Checked;
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
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateSuccessCheckBox = new System.Windows.Forms.CheckBox();
		this.m_includeFreeGameInCartOfferCheckbox = new System.Windows.Forms.CheckBox();
		this.m_includeMovieInCart = new System.Windows.Forms.CheckBox();
		this.rbInCart = new System.Windows.Forms.RadioButton();
		this.rbFracFreeMovieStorable = new System.Windows.Forms.RadioButton();
		this.rbFracFreeMovieRedeemable = new System.Windows.Forms.RadioButton();
		this.rbFracDiscountStorable = new System.Windows.Forms.RadioButton();
		this.rbFracDiscountRedeemable = new System.Windows.Forms.RadioButton();
		this.rbNoOffer = new System.Windows.Forms.RadioButton();
		this.m_includeMultiNightOfferCheckbox = new System.Windows.Forms.CheckBox();
		this.txtProductIds = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.rbPerksFreeDisk = new System.Windows.Forms.RadioButton();
		this.rbPerksAmountOff = new System.Windows.Forms.RadioButton();
		this.rbPerksMessage = new System.Windows.Forms.RadioButton();
		this.rbPerksExtraPoints = new System.Windows.Forms.RadioButton();
		this.m_includePurchaseAmtOffInCartOfferCheckbox = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(32, 20);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 0;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.m_simulateSuccessCheckBox.AutoSize = true;
		this.m_simulateSuccessCheckBox.Location = new System.Drawing.Point(32, 53);
		this.m_simulateSuccessCheckBox.Name = "m_simulateSuccessCheckBox";
		this.m_simulateSuccessCheckBox.Size = new System.Drawing.Size(110, 17);
		this.m_simulateSuccessCheckBox.TabIndex = 1;
		this.m_simulateSuccessCheckBox.Text = "Simulate Success";
		this.m_simulateSuccessCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateSuccessCheckBox.CheckedChanged += new System.EventHandler(OnSimulateSuccessCheckChanged);
		this.m_includeFreeGameInCartOfferCheckbox.AutoSize = true;
		this.m_includeFreeGameInCartOfferCheckbox.Location = new System.Drawing.Point(73, 154);
		this.m_includeFreeGameInCartOfferCheckbox.Name = "m_includeFreeGameInCartOfferCheckbox";
		this.m_includeFreeGameInCartOfferCheckbox.Size = new System.Drawing.Size(176, 17);
		this.m_includeFreeGameInCartOfferCheckbox.TabIndex = 2;
		this.m_includeFreeGameInCartOfferCheckbox.Text = "Include Free Game In Cart Offer";
		this.m_includeFreeGameInCartOfferCheckbox.UseVisualStyleBackColor = true;
		this.m_includeFreeGameInCartOfferCheckbox.CheckedChanged += new System.EventHandler(m_includeFreeGameInCartOfferCheckbox_CheckedChanged);
		this.m_includeMovieInCart.AutoSize = true;
		this.m_includeMovieInCart.Location = new System.Drawing.Point(73, 131);
		this.m_includeMovieInCart.Name = "m_includeMovieInCart";
		this.m_includeMovieInCart.Size = new System.Drawing.Size(153, 17);
		this.m_includeMovieInCart.TabIndex = 3;
		this.m_includeMovieInCart.Text = "Include Movie In Cart Offer";
		this.m_includeMovieInCart.UseVisualStyleBackColor = true;
		this.m_includeMovieInCart.CheckedChanged += new System.EventHandler(movieInCartOffer_CheckedChanged);
		this.rbInCart.AutoSize = true;
		this.rbInCart.Location = new System.Drawing.Point(32, 112);
		this.rbInCart.Name = "rbInCart";
		this.rbInCart.Size = new System.Drawing.Size(126, 17);
		this.rbInCart.TabIndex = 4;
		this.rbInCart.TabStop = true;
		this.rbInCart.Text = "Present In Cart Offers";
		this.rbInCart.UseVisualStyleBackColor = true;
		this.rbInCart.CheckedChanged += new System.EventHandler(rbPresentInCart_CheckedChanged);
		this.rbFracFreeMovieStorable.AutoSize = true;
		this.rbFracFreeMovieStorable.Location = new System.Drawing.Point(32, 253);
		this.rbFracFreeMovieStorable.Name = "rbFracFreeMovieStorable";
		this.rbFracFreeMovieStorable.Size = new System.Drawing.Size(187, 17);
		this.rbFracFreeMovieStorable.TabIndex = 5;
		this.rbFracFreeMovieStorable.TabStop = true;
		this.rbFracFreeMovieStorable.Text = "Present FRAC Free Movie 1st Visit";
		this.rbFracFreeMovieStorable.UseVisualStyleBackColor = true;
		this.rbFracFreeMovieStorable.CheckedChanged += new System.EventHandler(rbFracFreeMovieStorable_CheckedChanged);
		this.rbFracFreeMovieRedeemable.AutoSize = true;
		this.rbFracFreeMovieRedeemable.Location = new System.Drawing.Point(32, 276);
		this.rbFracFreeMovieRedeemable.Name = "rbFracFreeMovieRedeemable";
		this.rbFracFreeMovieRedeemable.Size = new System.Drawing.Size(191, 17);
		this.rbFracFreeMovieRedeemable.TabIndex = 6;
		this.rbFracFreeMovieRedeemable.TabStop = true;
		this.rbFracFreeMovieRedeemable.Text = "Present FRAC Free Movie 2nd Visit";
		this.rbFracFreeMovieRedeemable.UseVisualStyleBackColor = true;
		this.rbFracFreeMovieRedeemable.CheckedChanged += new System.EventHandler(rbFracFreeMovieRedeemable_CheckedChanged);
		this.rbFracDiscountStorable.AutoSize = true;
		this.rbFracDiscountStorable.Location = new System.Drawing.Point(32, 299);
		this.rbFracDiscountStorable.Name = "rbFracDiscountStorable";
		this.rbFracDiscountStorable.Size = new System.Drawing.Size(212, 17);
		this.rbFracDiscountStorable.TabIndex = 7;
		this.rbFracDiscountStorable.TabStop = true;
		this.rbFracDiscountStorable.Text = "Present FRAC Discounted Disc 1st Visit";
		this.rbFracDiscountStorable.UseVisualStyleBackColor = true;
		this.rbFracDiscountStorable.CheckedChanged += new System.EventHandler(rbFracDiscountStorable_CheckedChanged);
		this.rbFracDiscountRedeemable.AutoSize = true;
		this.rbFracDiscountRedeemable.Location = new System.Drawing.Point(32, 322);
		this.rbFracDiscountRedeemable.Name = "rbFracDiscountRedeemable";
		this.rbFracDiscountRedeemable.Size = new System.Drawing.Size(216, 17);
		this.rbFracDiscountRedeemable.TabIndex = 8;
		this.rbFracDiscountRedeemable.TabStop = true;
		this.rbFracDiscountRedeemable.Text = "Present FRAC Discounted Disc 2nd Visit";
		this.rbFracDiscountRedeemable.UseVisualStyleBackColor = true;
		this.rbFracDiscountRedeemable.CheckedChanged += new System.EventHandler(rbFracDiscountRedeemable_CheckedChanged);
		this.rbNoOffer.AutoSize = true;
		this.rbNoOffer.Location = new System.Drawing.Point(32, 89);
		this.rbNoOffer.Name = "rbNoOffer";
		this.rbNoOffer.Size = new System.Drawing.Size(65, 17);
		this.rbNoOffer.TabIndex = 9;
		this.rbNoOffer.TabStop = true;
		this.rbNoOffer.Text = "No Offer";
		this.rbNoOffer.UseVisualStyleBackColor = true;
		this.m_includeMultiNightOfferCheckbox.AutoSize = true;
		this.m_includeMultiNightOfferCheckbox.Location = new System.Drawing.Point(73, 200);
		this.m_includeMultiNightOfferCheckbox.Name = "m_includeMultiNightOfferCheckbox";
		this.m_includeMultiNightOfferCheckbox.Size = new System.Drawing.Size(148, 17);
		this.m_includeMultiNightOfferCheckbox.TabIndex = 10;
		this.m_includeMultiNightOfferCheckbox.Text = "Include MNP In Cart Offer";
		this.m_includeMultiNightOfferCheckbox.UseVisualStyleBackColor = true;
		this.m_includeMultiNightOfferCheckbox.CheckedChanged += new System.EventHandler(m_includeMultiNightOfferCheckbox_CheckedChanged);
		this.txtProductIds.Location = new System.Drawing.Point(162, 223);
		this.txtProductIds.Name = "txtProductIds";
		this.txtProductIds.Size = new System.Drawing.Size(273, 20);
		this.txtProductIds.TabIndex = 11;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(90, 226);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(66, 13);
		this.label1.TabIndex = 12;
		this.label1.Text = "Product IDs:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(306, 243);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(107, 13);
		this.label2.TabIndex = 13;
		this.label2.Text = "(Comma delimited list)";
		this.rbPerksFreeDisk.AutoSize = true;
		this.rbPerksFreeDisk.Location = new System.Drawing.Point(32, 345);
		this.rbPerksFreeDisk.Name = "rbPerksFreeDisk";
		this.rbPerksFreeDisk.Size = new System.Drawing.Size(191, 17);
		this.rbPerksFreeDisk.TabIndex = 14;
		this.rbPerksFreeDisk.TabStop = true;
		this.rbPerksFreeDisk.Text = "Present Free Item Perks Equivalent";
		this.rbPerksFreeDisk.UseVisualStyleBackColor = true;
		this.rbPerksFreeDisk.CheckedChanged += new System.EventHandler(RbPerksFreeDisk_CheckedChanged);
		this.rbPerksAmountOff.AutoSize = true;
		this.rbPerksAmountOff.Location = new System.Drawing.Point(32, 368);
		this.rbPerksAmountOff.Name = "rbPerksAmountOff";
		this.rbPerksAmountOff.Size = new System.Drawing.Size(200, 17);
		this.rbPerksAmountOff.TabIndex = 15;
		this.rbPerksAmountOff.TabStop = true;
		this.rbPerksAmountOff.Text = "Present Amount Off Perks Equivalent";
		this.rbPerksAmountOff.UseVisualStyleBackColor = true;
		this.rbPerksAmountOff.CheckedChanged += new System.EventHandler(RbPerksAmountOff_CheckedChanged);
		this.rbPerksMessage.AutoSize = true;
		this.rbPerksMessage.Location = new System.Drawing.Point(32, 412);
		this.rbPerksMessage.Name = "rbPerksMessage";
		this.rbPerksMessage.Size = new System.Drawing.Size(140, 17);
		this.rbPerksMessage.TabIndex = 16;
		this.rbPerksMessage.TabStop = true;
		this.rbPerksMessage.Text = "Present  Perks Message";
		this.rbPerksMessage.UseVisualStyleBackColor = true;
		this.rbPerksMessage.CheckedChanged += new System.EventHandler(RbPerksMessage_CheckedChanged);
		this.rbPerksExtraPoints.AutoSize = true;
		this.rbPerksExtraPoints.Location = new System.Drawing.Point(32, 391);
		this.rbPerksExtraPoints.Name = "rbPerksExtraPoints";
		this.rbPerksExtraPoints.Size = new System.Drawing.Size(203, 17);
		this.rbPerksExtraPoints.TabIndex = 17;
		this.rbPerksExtraPoints.TabStop = true;
		this.rbPerksExtraPoints.Text = "Present Extra Points Perks Equivalent";
		this.rbPerksExtraPoints.UseVisualStyleBackColor = true;
		this.rbPerksExtraPoints.CheckedChanged += new System.EventHandler(RbPerksExtraPoints_CheckedChanged);
		this.m_includePurchaseAmtOffInCartOfferCheckbox.AutoSize = true;
		this.m_includePurchaseAmtOffInCartOfferCheckbox.Location = new System.Drawing.Point(73, 177);
		this.m_includePurchaseAmtOffInCartOfferCheckbox.Name = "m_includePurchaseAmtOffInCartOfferCheckbox";
		this.m_includePurchaseAmtOffInCartOfferCheckbox.Size = new System.Drawing.Size(207, 17);
		this.m_includePurchaseAmtOffInCartOfferCheckbox.TabIndex = 18;
		this.m_includePurchaseAmtOffInCartOfferCheckbox.Text = "Include Amt. off purchase In Cart Offer";
		this.m_includePurchaseAmtOffInCartOfferCheckbox.UseVisualStyleBackColor = true;
		this.m_includePurchaseAmtOffInCartOfferCheckbox.CheckedChanged += new System.EventHandler(m_includePurchaseAmtOffInCartOfferCheckbox_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_includePurchaseAmtOffInCartOfferCheckbox);
		base.Controls.Add(this.rbPerksExtraPoints);
		base.Controls.Add(this.rbPerksMessage);
		base.Controls.Add(this.rbPerksAmountOff);
		base.Controls.Add(this.rbPerksFreeDisk);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.txtProductIds);
		base.Controls.Add(this.m_includeMultiNightOfferCheckbox);
		base.Controls.Add(this.rbNoOffer);
		base.Controls.Add(this.rbFracDiscountRedeemable);
		base.Controls.Add(this.rbFracDiscountStorable);
		base.Controls.Add(this.rbFracFreeMovieRedeemable);
		base.Controls.Add(this.rbFracFreeMovieStorable);
		base.Controls.Add(this.rbInCart);
		base.Controls.Add(this.m_includeMovieInCart);
		base.Controls.Add(this.m_includeFreeGameInCartOfferCheckbox);
		base.Controls.Add(this.m_simulateSuccessCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "CustomerOffersPreferencePage";
		base.Size = new System.Drawing.Size(438, 468);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
