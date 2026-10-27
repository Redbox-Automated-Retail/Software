using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.HardwareServices.TestProxy;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.KernelExtension;

public class StatisticsPreferencePage : UserControl, IPreferencePageHost
{
	private BindingList<IHardwareCorrectionStatistic> _data;

	private IContainer components;

	private Label m_headerLabel;

	private CheckBox m_routerResetEnabled;

	private CheckBox m_routerResetSuccessful;

	private CheckBox m_touchScreenResetSuccessful;

	private Button m_resetRouter;

	private Button m_touchScreenReset;

	private DataGridView m_statsList;

	private BindingSource iHardwareCorrectionStatisticBindingSource;

	private CheckBox m_arcusResetSuccessful;

	private Button m_refresh;

	private CheckBox m_arcusResetOnVend;

	private GroupBox groupBox1;

	private CheckBox m_ArcusResetOnReturn;

	public StatisticsPreferencePage()
	{
		InitializeComponent();
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		m_routerResetEnabled.Checked = ((TestHardwareService)service).IsRouterResetEnabled;
		m_routerResetSuccessful.Checked = ((TestHardwareService)service).RouterResetSuccessful;
		m_touchScreenResetSuccessful.Checked = ((TestHardwareService)service).TouchScreenResetSuccessful;
		_data = ((TestHardwareService)service).HardwareCorrectionStatistics;
		m_statsList.DataSource = _data;
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	private void OnRouterResetChanged(object sender, EventArgs e)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		((TestHardwareService)ServiceLocator.Instance.GetService<IHardwareService>()).IsRouterResetEnabled = m_routerResetEnabled.Checked;
	}

	private void OnRouterResetSuccessfulChanged(object sender, EventArgs e)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		((TestHardwareService)ServiceLocator.Instance.GetService<IHardwareService>()).RouterResetSuccessful = m_routerResetSuccessful.Checked;
	}

	private void OnTouchScreenResetSuccessfulChanged(object sender, EventArgs e)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		((TestHardwareService)ServiceLocator.Instance.GetService<IHardwareService>()).TouchScreenResetSuccessful = m_touchScreenResetSuccessful.Checked;
	}

	private void OnArcusResetSuccessfulChanged(object sender, EventArgs e)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		((TestHardwareService)ServiceLocator.Instance.GetService<IHardwareService>()).ArcusResetSuccessful = m_arcusResetSuccessful.Checked;
	}

	private void OnArcusResetOnVendChanged(object sender, EventArgs e)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		((TestHardwareService)ServiceLocator.Instance.GetService<IHardwareService>()).ArcusResetOnVend = m_arcusResetOnVend.Checked;
	}

	private void OnArcusResetOnReturnChanged(object sender, EventArgs e)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		((TestHardwareService)ServiceLocator.Instance.GetService<IHardwareService>()).ArcusResetOnReturn = m_ArcusResetOnReturn.Checked;
	}

	private void OnResetTouchScreen(object sender, EventArgs e)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		string jobId = null;
		service.ResetTouchScreenController(delegate
		{
		}, delegate(string jobIdentity, string status)
		{
			LogHelper.Instance.Log($"Message: {status}", LogEntryType.Info);
		}, out jobId);
	}

	private void OnResetRouter(object sender, EventArgs e)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		string jobId = null;
		service.PowerCycleRouter(delegate
		{
		}, delegate(string jobIdentity, string status)
		{
			LogHelper.Instance.Log($"Message: {status}", LogEntryType.Info);
		}, out jobId);
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
		this.m_routerResetEnabled = new System.Windows.Forms.CheckBox();
		this.m_routerResetSuccessful = new System.Windows.Forms.CheckBox();
		this.m_touchScreenResetSuccessful = new System.Windows.Forms.CheckBox();
		this.m_resetRouter = new System.Windows.Forms.Button();
		this.m_touchScreenReset = new System.Windows.Forms.Button();
		this.m_statsList = new System.Windows.Forms.DataGridView();
		this.iHardwareCorrectionStatisticBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.m_arcusResetSuccessful = new System.Windows.Forms.CheckBox();
		this.m_refresh = new System.Windows.Forms.Button();
		this.m_arcusResetOnVend = new System.Windows.Forms.CheckBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.m_ArcusResetOnReturn = new System.Windows.Forms.CheckBox();
		((System.ComponentModel.ISupportInitialize)this.m_statsList).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.iHardwareCorrectionStatisticBindingSource).BeginInit();
		this.groupBox1.SuspendLayout();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.Gold;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.Black;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 8;
		this.m_headerLabel.Text = "  Hardware Service: Statistics";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_routerResetEnabled.AutoSize = true;
		this.m_routerResetEnabled.Location = new System.Drawing.Point(23, 57);
		this.m_routerResetEnabled.Name = "m_routerResetEnabled";
		this.m_routerResetEnabled.Size = new System.Drawing.Size(131, 17);
		this.m_routerResetEnabled.TabIndex = 9;
		this.m_routerResetEnabled.Text = "Router Reset Enabled";
		this.m_routerResetEnabled.UseVisualStyleBackColor = true;
		this.m_routerResetEnabled.CheckedChanged += new System.EventHandler(OnRouterResetChanged);
		this.m_routerResetSuccessful.AutoSize = true;
		this.m_routerResetSuccessful.Location = new System.Drawing.Point(23, 80);
		this.m_routerResetSuccessful.Name = "m_routerResetSuccessful";
		this.m_routerResetSuccessful.Size = new System.Drawing.Size(144, 17);
		this.m_routerResetSuccessful.TabIndex = 10;
		this.m_routerResetSuccessful.Text = "Router Reset Successful";
		this.m_routerResetSuccessful.UseVisualStyleBackColor = true;
		this.m_routerResetSuccessful.CheckedChanged += new System.EventHandler(OnRouterResetSuccessfulChanged);
		this.m_touchScreenResetSuccessful.AutoSize = true;
		this.m_touchScreenResetSuccessful.Location = new System.Drawing.Point(23, 103);
		this.m_touchScreenResetSuccessful.Name = "m_touchScreenResetSuccessful";
		this.m_touchScreenResetSuccessful.Size = new System.Drawing.Size(180, 17);
		this.m_touchScreenResetSuccessful.TabIndex = 11;
		this.m_touchScreenResetSuccessful.Text = "Touch Screen Reset Successful";
		this.m_touchScreenResetSuccessful.UseVisualStyleBackColor = true;
		this.m_touchScreenResetSuccessful.CheckedChanged += new System.EventHandler(OnTouchScreenResetSuccessfulChanged);
		this.m_resetRouter.Location = new System.Drawing.Point(401, 53);
		this.m_resetRouter.Name = "m_resetRouter";
		this.m_resetRouter.Size = new System.Drawing.Size(128, 23);
		this.m_resetRouter.TabIndex = 12;
		this.m_resetRouter.Text = "Reset Router";
		this.m_resetRouter.UseVisualStyleBackColor = true;
		this.m_resetRouter.Click += new System.EventHandler(OnResetRouter);
		this.m_touchScreenReset.Location = new System.Drawing.Point(401, 82);
		this.m_touchScreenReset.Name = "m_touchScreenReset";
		this.m_touchScreenReset.Size = new System.Drawing.Size(128, 23);
		this.m_touchScreenReset.TabIndex = 13;
		this.m_touchScreenReset.Text = "Reset Touch Screen";
		this.m_touchScreenReset.UseVisualStyleBackColor = true;
		this.m_touchScreenReset.Click += new System.EventHandler(OnResetTouchScreen);
		this.m_statsList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.m_statsList.Location = new System.Drawing.Point(23, 313);
		this.m_statsList.MultiSelect = false;
		this.m_statsList.Name = "m_statsList";
		this.m_statsList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.m_statsList.Size = new System.Drawing.Size(506, 150);
		this.m_statsList.TabIndex = 14;
		this.iHardwareCorrectionStatisticBindingSource.DataSource = typeof(Redbox.HardwareServices.Proxy.ComponentModel.IHardwareCorrectionStatistic);
		this.m_arcusResetSuccessful.AutoSize = true;
		this.m_arcusResetSuccessful.Location = new System.Drawing.Point(23, 177);
		this.m_arcusResetSuccessful.Name = "m_arcusResetSuccessful";
		this.m_arcusResetSuccessful.Size = new System.Drawing.Size(139, 17);
		this.m_arcusResetSuccessful.TabIndex = 15;
		this.m_arcusResetSuccessful.Text = "Arcus Reset Successful";
		this.m_arcusResetSuccessful.UseVisualStyleBackColor = true;
		this.m_arcusResetSuccessful.CheckedChanged += new System.EventHandler(OnArcusResetSuccessfulChanged);
		this.m_refresh.Location = new System.Drawing.Point(401, 469);
		this.m_refresh.Name = "m_refresh";
		this.m_refresh.Size = new System.Drawing.Size(128, 23);
		this.m_refresh.TabIndex = 16;
		this.m_refresh.Text = "Refresh";
		this.m_refresh.UseVisualStyleBackColor = true;
		this.m_arcusResetOnVend.AutoSize = true;
		this.m_arcusResetOnVend.Location = new System.Drawing.Point(6, 19);
		this.m_arcusResetOnVend.Name = "m_arcusResetOnVend";
		this.m_arcusResetOnVend.Size = new System.Drawing.Size(51, 17);
		this.m_arcusResetOnVend.TabIndex = 17;
		this.m_arcusResetOnVend.Text = "Vend";
		this.m_arcusResetOnVend.UseVisualStyleBackColor = true;
		this.m_arcusResetOnVend.CheckedChanged += new System.EventHandler(OnArcusResetOnVendChanged);
		this.groupBox1.Controls.Add(this.m_ArcusResetOnReturn);
		this.groupBox1.Controls.Add(this.m_arcusResetOnVend);
		this.groupBox1.Location = new System.Drawing.Point(23, 200);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(152, 100);
		this.groupBox1.TabIndex = 18;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Trigger Arcus Reset On";
		this.m_ArcusResetOnReturn.AutoSize = true;
		this.m_ArcusResetOnReturn.Location = new System.Drawing.Point(6, 42);
		this.m_ArcusResetOnReturn.Name = "m_ArcusResetOnReturn";
		this.m_ArcusResetOnReturn.Size = new System.Drawing.Size(58, 17);
		this.m_ArcusResetOnReturn.TabIndex = 18;
		this.m_ArcusResetOnReturn.Text = "Return";
		this.m_ArcusResetOnReturn.UseVisualStyleBackColor = true;
		this.m_ArcusResetOnReturn.CheckedChanged += new System.EventHandler(OnArcusResetOnReturnChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.m_refresh);
		base.Controls.Add(this.m_arcusResetSuccessful);
		base.Controls.Add(this.m_statsList);
		base.Controls.Add(this.m_touchScreenReset);
		base.Controls.Add(this.m_resetRouter);
		base.Controls.Add(this.m_touchScreenResetSuccessful);
		base.Controls.Add(this.m_routerResetSuccessful);
		base.Controls.Add(this.m_routerResetEnabled);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "StatisticsPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_statsList).EndInit();
		((System.ComponentModel.ISupportInitialize)this.iHardwareCorrectionStatisticBindingSource).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
