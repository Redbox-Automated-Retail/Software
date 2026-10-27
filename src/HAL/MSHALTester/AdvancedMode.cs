using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.MSHALTester;

public class AdvancedMode : Form
{
	private readonly object ListLock = new object();

	private readonly ConfigPropertyList PropertyList;

	private readonly HardwareService Service;

	private readonly List<HardwareJobAdapter> JobList = new List<HardwareJobAdapter>();

	private readonly BindingList<HardwareJobAdapter> BindingList;

	private readonly BindingSource BindingSource;

	private readonly ButtonAspectsManager Manager;

	private IContainer components;

	private Panel panel1;

	private Button m_exitButton;

	private Button button2;

	private Button m_saveConfigButton;

	private DataGridView m_jobListDataView;

	private Button button1;

	private Button button3;

	private Button button4;

	private Timer m_jobTimer;

	private TextBox m_vendMoveOutput;

	private GroupBox groupBox1;

	public AdvancedMode(HardwareService service)
	{
		InitializeComponent();
		Manager = new ButtonAspectsManager();
		Service = service;
		PropertyList = new ConfigPropertyList(service);
		panel1.Controls.Add(PropertyList);
		PropertyList.OnConfigItemChange += PropertyList_OnConfigItemChange;
		PropertyList.OnSave += PropertyList_OnSave;
		BindingList = new BindingList<HardwareJobAdapter>(JobList);
		BindingSource = new BindingSource(BindingList, null);
		m_jobListDataView.DataSource = BindingSource;
		RefreshJobs();
		m_jobListDataView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		m_jobListDataView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		m_jobTimer.Enabled = true;
	}

	private void PropertyList_OnSave()
	{
		m_saveConfigButton.Enabled = false;
	}

	private void PropertyList_OnConfigItemChange()
	{
		m_saveConfigButton.Enabled = true;
	}

	private void m_exitButton_Click(object sender, EventArgs e)
	{
		m_jobTimer.Enabled = false;
		PropertyList.OnSave -= PropertyList_OnSave;
		PropertyList.Dispose();
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void button2_Click(object sender, EventArgs e)
	{
		m_jobTimer.Enabled = false;
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		try
		{
			new DeckConfigurationForm(Service).ShowDialog();
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("Unable to run Decks form.", e2);
		}
	}

	private void RefreshJobs()
	{
		JobList.ForEach(delegate(HardwareJobAdapter s)
		{
			s.Removable = true;
		});
		if (Service.GetJobs(out var jobs).Success)
		{
			HardwareJob[] array = jobs;
			foreach (HardwareJob nj in array)
			{
				HardwareJobAdapter hardwareJobAdapter = JobList.Find((HardwareJobAdapter each) => each.ID == nj.ID);
				if (hardwareJobAdapter == null)
				{
					BindingSource.Add(new HardwareJobAdapter(nj));
					continue;
				}
				hardwareJobAdapter.Merge(nj);
				hardwareJobAdapter.Removable = false;
			}
		}
		foreach (HardwareJobAdapter item in JobList.FindAll((HardwareJobAdapter each) => each.Removable))
		{
			BindingSource.Remove(item);
		}
	}

	private void m_saveConfigButton_Click(object sender, EventArgs e)
	{
		PropertyList.Save();
	}

	private void button1_Click_1(object sender, EventArgs e)
	{
		OnListAction(delegate(HardwareJobAdapter j)
		{
			if (j.Status != HardwareJobStatus.Suspended)
			{
				j.Job.Suspend();
			}
		});
	}

	private void OnListAction(Action<HardwareJobAdapter> action)
	{
		foreach (DataGridViewRow selectedRow in m_jobListDataView.SelectedRows)
		{
			HardwareJobAdapter obj = selectedRow.DataBoundItem as HardwareJobAdapter;
			action(obj);
		}
	}

	private void button3_Click(object sender, EventArgs e)
	{
		OnListAction(delegate(HardwareJobAdapter j)
		{
			if (j.Status == HardwareJobStatus.Suspended)
			{
				j.Job.Pend();
			}
		});
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		RefreshJobs();
	}

	private void button4_Click(object sender, EventArgs e)
	{
		using ButtonAspects buttonAspects = Manager.MakeAspect(sender);
		string tagInstruction = buttonAspects.GetTagInstruction();
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		m_vendMoveOutput.Text = instructionHelper.ExecuteErrorCode(tagInstruction).ToString().ToUpper();
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
		this.panel1 = new System.Windows.Forms.Panel();
		this.m_exitButton = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.m_saveConfigButton = new System.Windows.Forms.Button();
		this.m_jobListDataView = new System.Windows.Forms.DataGridView();
		this.button1 = new System.Windows.Forms.Button();
		this.button3 = new System.Windows.Forms.Button();
		this.button4 = new System.Windows.Forms.Button();
		this.m_jobTimer = new System.Windows.Forms.Timer(this.components);
		this.m_vendMoveOutput = new System.Windows.Forms.TextBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		((System.ComponentModel.ISupportInitialize)this.m_jobListDataView).BeginInit();
		this.groupBox1.SuspendLayout();
		base.SuspendLayout();
		this.panel1.Location = new System.Drawing.Point(12, 12);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(371, 492);
		this.panel1.TabIndex = 0;
		this.m_exitButton.BackColor = System.Drawing.Color.GreenYellow;
		this.m_exitButton.Location = new System.Drawing.Point(583, 461);
		this.m_exitButton.Name = "m_exitButton";
		this.m_exitButton.Size = new System.Drawing.Size(112, 42);
		this.m_exitButton.TabIndex = 1;
		this.m_exitButton.Text = "Exit";
		this.m_exitButton.UseVisualStyleBackColor = false;
		this.m_exitButton.Click += new System.EventHandler(m_exitButton_Click);
		this.button2.Location = new System.Drawing.Point(437, 461);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(113, 43);
		this.button2.TabIndex = 2;
		this.button2.Text = "Cancel";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(button2_Click);
		this.m_saveConfigButton.Enabled = false;
		this.m_saveConfigButton.Location = new System.Drawing.Point(400, 364);
		this.m_saveConfigButton.Name = "m_saveConfigButton";
		this.m_saveConfigButton.Size = new System.Drawing.Size(150, 65);
		this.m_saveConfigButton.TabIndex = 4;
		this.m_saveConfigButton.Text = "Save Configuration Change";
		this.m_saveConfigButton.UseVisualStyleBackColor = true;
		this.m_saveConfigButton.Click += new System.EventHandler(m_saveConfigButton_Click);
		this.m_jobListDataView.AllowUserToAddRows = false;
		this.m_jobListDataView.AllowUserToDeleteRows = false;
		this.m_jobListDataView.AllowUserToOrderColumns = true;
		this.m_jobListDataView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.m_jobListDataView.Location = new System.Drawing.Point(18, 19);
		this.m_jobListDataView.Name = "m_jobListDataView";
		this.m_jobListDataView.ReadOnly = true;
		this.m_jobListDataView.Size = new System.Drawing.Size(536, 150);
		this.m_jobListDataView.TabIndex = 5;
		this.button1.Location = new System.Drawing.Point(18, 175);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(145, 50);
		this.button1.TabIndex = 6;
		this.button1.Text = "Suspend Selected";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click_1);
		this.button3.Location = new System.Drawing.Point(409, 175);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(145, 50);
		this.button3.TabIndex = 7;
		this.button3.Text = "Resume Selected";
		this.button3.UseVisualStyleBackColor = true;
		this.button3.Click += new System.EventHandler(button3_Click);
		this.button4.Location = new System.Drawing.Point(400, 284);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(150, 54);
		this.button4.TabIndex = 8;
		this.button4.Tag = "MOVEVEND";
		this.button4.Text = "Move To Vend";
		this.button4.UseVisualStyleBackColor = true;
		this.button4.Click += new System.EventHandler(button4_Click);
		this.m_jobTimer.Interval = 1000;
		this.m_jobTimer.Tick += new System.EventHandler(timer1_Tick);
		this.m_vendMoveOutput.Location = new System.Drawing.Point(556, 318);
		this.m_vendMoveOutput.Name = "m_vendMoveOutput";
		this.m_vendMoveOutput.ReadOnly = true;
		this.m_vendMoveOutput.Size = new System.Drawing.Size(139, 20);
		this.m_vendMoveOutput.TabIndex = 9;
		this.groupBox1.Controls.Add(this.m_jobListDataView);
		this.groupBox1.Controls.Add(this.button1);
		this.groupBox1.Controls.Add(this.button3);
		this.groupBox1.Location = new System.Drawing.Point(400, 12);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(569, 240);
		this.groupBox1.TabIndex = 10;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Job Control";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(990, 520);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.m_vendMoveOutput);
		base.Controls.Add(this.button4);
		base.Controls.Add(this.m_saveConfigButton);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.m_exitButton);
		base.Controls.Add(this.panel1);
		base.Name = "AdvancedMode";
		this.Text = "AdvancedMode";
		((System.ComponentModel.ISupportInitialize)this.m_jobListDataView).EndInit();
		this.groupBox1.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
