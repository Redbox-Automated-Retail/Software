using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console;

public class Form1 : Form
{
	private IContainer components;

	private MenuStrip m_menuStrip;

	private ToolStripMenuItem toolStripMenuItem1;

	private ToolStripMenuItem connectToolStripMenuItem;

	private ToolStripMenuItem disconnectToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripMenuItem saveToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripMenuItem lockSettingsToolStripMenuItem;

	private ToolStripMenuItem unlockSettingsToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripMenuItem exitToolStripMenuItem;

	private ToolStripMenuItem viewToolStripMenuItem;

	private ToolStripMenuItem programsToolStripMenuItem;

	private ToolStripMenuItem toolsToolStripMenuItem;

	private ToolStripMenuItem helpToolStripMenuItem;

	private ToolStripMenuItem aboutHalManagementConsoleToolStripMenuItem;

	private ToolStripMenuItem autoResumeJobOnScheduleToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator11;

	private ToolStripMenuItem executeScriptToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator12;

	private ToolStripMenuItem initializeToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator13;

	private ToolStripMenuItem vendToolStripMenuItem;

	private ToolStripMenuItem returnToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator14;

	private ToolStripMenuItem syncToolStripMenuItem;

	private ToolStripMenuItem softSyncToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator15;

	private ToolStripMenuItem thinToolStripMenuItem;

	private ToolStripMenuItem qlmUnloadToolStripMenuItem;

	private ToolStripMenuItem unloadThinToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator16;

	private ToolStripMenuItem getMachineListToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator17;

	private ToolStripMenuItem newInstallSyncToolStripMenuItem;

	private ToolStripMenuItem getInventoryStateToolStripMenuItem;

	private ToolStripMenuItem resetInventoryStateToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator26;

	private ToolStripMenuItem reloadHalConfigurationToolStripMenuItem;

	private ToolStripMenuItem backupHalConfigurationToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator27;

	private ToolStrip m_standardToolStrip;

	private ToolStripButton m_lockButton;

	private ToolStripButton m_unlockButton;

	private ToolStripSeparator toolStripSeparator35;

	private ToolStripLabel m_profileLabel;

	private ToolStripButton m_connectButton;

	private ToolStripButton m_disconnectButton;

	private ToolStripSeparator toolStripSeparator36;

	private ToolStripButton m_saveButton;

	private ToolStripSeparator toolStripSeparator40;

	private ToolStripButton m_executeScriptButton;

	private ToolStripSeparator toolStripSeparator41;

	private ToolStripButton m_initButton;

	private ToolStripButton m_vendButton;

	private ToolStripSeparator toolStripSeparator42;

	private ToolStripButton m_returnButton;

	private ToolStripButton m_syncButton;

	private ToolStripSeparator toolStripSeparator43;

	private ToolStripButton m_softSyncButton;

	private ToolStripButton m_thinButton;

	private ToolStripButton m_unloadButton;

	private ToolStripSeparator toolStripSeparator44;

	private ToolStripButton m_unloadThinButton;

	private ToolStripSeparator toolStripSeparator45;

	private ToolStripButton m_getMachineInfoButton;

	private ToolStripMenuItem immediateWindowToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator46;

	private ToolStripMenuItem errorListToolStripMenuItem;

	private ToolStripMenuItem outputToolStripMenuItem;

	private ToolStripMenuItem propertiesWindowToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator47;

	private ToolStripMenuItem jobListToolStripMenuItem;

	private ToolStripMenuItem stackToolStripMenuItem;

	private ToolStripMenuItem symbolsToolStripMenuItem;

	private ToolStripMenuItem programEventsToolStripMenuItem;

	private ToolStripMenuItem programResultsToolStripMenuItem;

	private ToolStripMenuItem pickerSensorsToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator49;

	private ToolStripMenuItem toolbarsToolStripMenuItem;

	private ToolStripMenuItem standardToolStripMenuItem;

	private ToolStripMenuItem jobControlToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator50;

	private ToolStripButton m_initialNightSyncButton;

	private Timer m_timer;

	internal SplitContainer m_eastWestSplitContainer;

	internal SplitContainer m_northSouthSplitContainer;

	internal TabControl m_listViewTabControl;

	private ToolStripMenuItem dataInfoAndConfigurationToolStripMenuItem;

	private ToolStripMenuItem emptySearchPatternToolToolStripMenuItem;

	private ToolStripMenuItem getMaintenanceListToolStripMenuItem;

	private ToolStripButton m_getMaintListButton;

	private bool FormDisposed { get; set; }

	public Form1()
	{
		EnvironmentHelper.IsLocked = !Settings.Default.DeveloperMode;
		InitializeToolBars();
		LoadConfiguration();
		InitializeComponent();
		InitializeListViews();
		base.FormClosing += OnFormClose;
		JobDropDownMenu jobDropDownMenu = new JobDropDownMenu(JobComboBox.Instance.GetSelectedJobList, UpdateSource.JobComboBox, trashLocked: true);
		ToolStripMenuItem value = new ToolStripMenuItem
		{
			DropDown = jobDropDownMenu,
			Name = "JOB",
			Text = "&Job"
		};
		m_menuStrip.Items.Insert(3, value);
		JobComboBox.Instance.SelectedIndexChanged += jobDropDownMenu.UpdateButtons;
		JobComboBox.Instance.EnabledChanged += jobDropDownMenu.UpdateButtons;
		JobComboBox.Instance.TextChanged += jobDropDownMenu.UpdateButtons;
		JobHelper.JobList.ListChanged += jobDropDownMenu.UpdateButtons;
		m_eastWestSplitContainer.Panel1.Controls.Add(LeftPanel.Instance);
		m_northSouthSplitContainer.Panel1.Controls.Add(TouchScreenAccess.Instance);
		m_northSouthSplitContainer.Panel2.Controls.Add(ListViewTabControl.Instance);
		ProfileManager.Instance.Connected += delegate
		{
			m_timer.Enabled = true;
			TouchScreenAccess.Instance.Enabled = true;
			ImmediateWindow.Instance.Enabled = true;
			LeftPanel.Instance.Enabled = true;
			m_disconnectButton.Enabled = true;
			disconnectToolStripMenuItem.Enabled = true;
			connectToolStripMenuItem.Enabled = false;
			m_connectButton.Enabled = false;
			ToggleLocks(EnvironmentHelper.IsLocked);
			UpdateProgramButtons();
			Refresh();
		};
		ProfileManager.Instance.Disconnected += delegate
		{
			m_timer.Enabled = false;
			TouchScreenAccess.Instance.Enabled = false;
			ImmediateWindow.Instance.Enabled = false;
			LeftPanel.Instance.Enabled = false;
			UpdateProgramButtons();
			m_connectButton.Enabled = true;
			connectToolStripMenuItem.Enabled = true;
			disconnectToolStripMenuItem.Enabled = false;
			m_disconnectButton.Enabled = false;
			m_lockButton.Enabled = false;
			lockSettingsToolStripMenuItem.Enabled = false;
			m_unlockButton.Enabled = false;
			unlockSettingsToolStripMenuItem.Enabled = false;
			thinToolStripMenuItem.Enabled = false;
			m_thinButton.Enabled = false;
			qlmUnloadToolStripMenuItem.Enabled = false;
			m_unloadButton.Enabled = false;
			unloadThinToolStripMenuItem.Enabled = false;
			m_unloadThinButton.Enabled = false;
			m_initialNightSyncButton.Enabled = false;
			newInstallSyncToolStripMenuItem.Enabled = false;
			immediateWindowToolStripMenuItem.Enabled = false;
			propertiesWindowToolStripMenuItem.Enabled = false;
			outputToolStripMenuItem.Enabled = false;
			getInventoryStateToolStripMenuItem.Enabled = false;
			resetInventoryStateToolStripMenuItem.Enabled = false;
			EnvironmentHelper.IsDirty = false;
			Refresh();
		};
		EnvironmentHelper.SaveableStatusChanged += OnSaveableStateChanged;
		EnvironmentHelper.LockStatusChanged += OnLockStateChanged;
		LockControls();
		InitializeExteriorHandlers();
		if (!EnvironmentHelper.IsLocked)
		{
			DevMode();
		}
	}

	public void CloseForm()
	{
		if (!FormDisposed)
		{
			FormDisposed = true;
			HardwareService service = ProfileManager.Instance.Service;
			if (service != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(" VENDDOOR CLOSE");
				stringBuilder.AppendLine(" GRIPPER RENT");
				stringBuilder.AppendLine(" GRIPPER RETRACT");
				stringBuilder.AppendLine(" SENSOR PICKER-OFF");
				stringBuilder.AppendLine(" ROLLER STOP");
				stringBuilder.AppendLine(" RINGLIGHT OFF");
				stringBuilder.AppendLine(" CLEAR");
				service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
			}
			Application.Exit();
		}
	}

	private void OnLockStateChanged(object sender, BoolEventArgs e)
	{
		ToggleLocks(e.State);
	}

	private void OnSaveableStateChanged(object sender, BoolEventArgs e)
	{
		m_saveButton.Enabled = e.State;
		saveToolStripMenuItem.Enabled = e.State;
	}

	private void InitializeExteriorHandlers()
	{
		connectToolStripMenuItem.Click += ProfileManager.Instance.OnConnect;
		m_connectButton.Click += ProfileManager.Instance.OnConnect;
		disconnectToolStripMenuItem.Click += ProfileManager.Instance.OnDisconnect;
		m_disconnectButton.Click += ProfileManager.Instance.OnDisconnect;
		lockSettingsToolStripMenuItem.Click += EnvironmentHelper.OnLock;
		m_lockButton.Click += EnvironmentHelper.OnLock;
		unlockSettingsToolStripMenuItem.Click += EnvironmentHelper.OnUnlock;
		m_unlockButton.Click += EnvironmentHelper.OnUnlock;
		saveToolStripMenuItem.Click += EnvironmentHelper.OnSave;
		m_saveButton.Click += EnvironmentHelper.OnSave;
	}

	private void LoadConfiguration()
	{
	}

	private void InitializeListViews()
	{
		ListViewFactory.Instance.MakeTab(ListViewNames.Job, enable: false);
		ListViewFactory.Instance.MakeTab(ListViewNames.Stack, enable: false);
		ListViewFactory.Instance.MakeTab(ListViewNames.Symbols, enable: false);
		ListViewFactory.Instance.MakeTab(ListViewNames.ProgramEvents, enable: false);
		ListViewTabControl.Instance.SetFocus(ListViewNames.Job);
	}

	private void InitializeToolBars()
	{
		ToolStrip toolStrip = new JobToolStrip(JobComboBox.Instance.GetSelectedJobList, UpdateSource.JobComboBox, trashLocked: true);
		toolStrip.SuspendLayout();
		toolStrip.Name = "JobToolStrip";
		toolStrip.RenderMode = ToolStripRenderMode.Professional;
		toolStrip.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		toolStrip.Dock = DockStyle.Top;
		toolStrip.Location = new Point(0, 48);
		toolStrip.Size = new Size(1024, 25);
		toolStrip.TabIndex = 2;
		toolStrip.Text = "Job Tool Strip";
		toolStrip.ResumeLayout(performLayout: false);
		base.Controls.Add(toolStrip);
		new JobToolStrip(JobComboBox.Instance.GetSelectedJobList, UpdateSource.JobComboBox, trashLocked: true);
		JobComboBox.Instance.SelectedIndexChanged += (toolStrip as JobToolStrip).UpdateButtons;
		JobComboBox.Instance.EnabledChanged += (toolStrip as JobToolStrip).UpdateButtons;
		JobComboBox.Instance.TextChanged += (toolStrip as JobToolStrip).UpdateButtons;
		JobHelper.JobList.ListChanged += (toolStrip as JobToolStrip).UpdateButtons;
		toolStrip.Items.Insert(0, JobComboBox.Instance);
		toolStrip.Items.Insert(0, new ToolStripLabel
		{
			Text = "Job: "
		});
		toolStrip.ResumeLayout();
	}

	private void UpdateProgramButtons()
	{
		m_executeScriptButton.Enabled = ProfileManager.Instance.IsConnected;
		m_syncButton.Enabled = ProfileManager.Instance.IsConnected;
		m_returnButton.Enabled = ProfileManager.Instance.IsConnected;
		m_vendButton.Enabled = ProfileManager.Instance.IsConnected;
		m_softSyncButton.Enabled = ProfileManager.Instance.IsConnected;
		m_initButton.Enabled = ProfileManager.Instance.IsConnected;
		m_getMachineInfoButton.Enabled = ProfileManager.Instance.IsConnected;
		m_getMaintListButton.Enabled = ProfileManager.Instance.IsConnected;
		m_thinButton.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
		m_unloadButton.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
		m_unloadThinButton.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
		m_initialNightSyncButton.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
		returnToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
		vendToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
		initializeToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
		softSyncToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
		syncToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
		executeScriptToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
		getMachineListToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
		getMaintenanceListToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
		thinToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
		qlmUnloadToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
		unloadThinToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
		newInstallSyncToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
		dataInfoAndConfigurationToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
	}

	private void ToggleLocks(bool locked)
	{
		m_unlockButton.Enabled = locked;
		unlockSettingsToolStripMenuItem.Enabled = locked;
		m_lockButton.Enabled = !locked;
		lockSettingsToolStripMenuItem.Enabled = !locked;
	}

	private void OnGlobalTimer(object sender, EventArgs e)
	{
		ListViewTabControl.Instance.RefreshTab();
	}

	private void OnScheduleScriptByTag(object sender, EventArgs e)
	{
		if (sender is ToolStripItem toolStripItem)
		{
			CommonFunctions.ScheduleJob(toolStripItem.Tag.ToString().ToLower(), toolStripItem.Tag.ToString(), new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Normal
			}, autoResumeJobOnScheduleToolStripMenuItem.Checked);
		}
	}

	private void OnMenuListViewToggle(object sender, EventArgs e)
	{
		if (!(sender is ToolStripMenuItem { Tag: not null } toolStripMenuItem))
		{
			return;
		}
		ListViewNames listViewNames = (ListViewNames)toolStripMenuItem.Tag;
		TabPageExtension tabPageExtension = ListViewTabControl.Instance.Find(listViewNames);
		if (toolStripMenuItem.Checked)
		{
			if (tabPageExtension == null)
			{
				ListViewFactory.Instance.MakeTab(listViewNames, ProfileManager.Instance.IsConnected);
			}
			if (listViewNames == ListViewNames.Errors)
			{
				ErrorListView.Instance.KeepOpenOnSuccessfulInstruction = true;
			}
		}
		else
		{
			ListViewTabControl.Instance.Remove(tabPageExtension);
		}
	}

	private void OnMenuLeftSideToggle(object sender, EventArgs e)
	{
		if (sender is ToolStripMenuItem { Tag: not null } toolStripMenuItem)
		{
			LeftPanelTab tab = (LeftPanelTab)toolStripMenuItem.Tag;
			if (toolStripMenuItem.Checked)
			{
				LeftPanel.Instance.AddTab(tab);
			}
			else
			{
				LeftPanel.Instance.RemoveTab(tab);
			}
		}
	}

	private void OnMenuHelpAbout(object sender, EventArgs e)
	{
		Label label = new Label();
		label.Dock = DockStyle.Fill;
		label.Image = Resources.hal_splash;
		Label label2 = new Label();
		label2.Text = $"Version: {Program.Version}";
		label2.BackColor = Color.Maroon;
		label2.ForeColor = Color.White;
		label2.AutoSize = true;
		label2.Font = new Font("Arial", 13f, FontStyle.Bold, GraphicsUnit.Point, 0);
		label2.Location = new Point(40, label.Image.Size.Height / 2 - 20);
		Form f = new Form();
		f.AutoSize = false;
		f.Size = label.Image.Size;
		f.FormBorderStyle = FormBorderStyle.None;
		label.Click += delegate
		{
			f.Close();
		};
		f.StartPosition = FormStartPosition.CenterScreen;
		f.Controls.Add(label2);
		f.Controls.Add(label);
		f.Show();
	}

	private void OnMenuToolsDeckConfiguration(object sender, EventArgs e)
	{
		try
		{
			new DeckConfigurationForm(ProfileManager.Instance.Service, allowInventoryUpdate: true).ShowDialog();
		}
		catch (Exception ex)
		{
			OutputWindow.Instance.Append("Unable to run Decks form.");
			OutputWindow.Instance.Append(ex.Message);
		}
		LeftPanel.Instance.ReloadPropertiesConfiguration();
	}

	private void OnMenuToolsExportInventory(object sender, EventArgs e)
	{
		HardwareCommandResult inventoryState = ProfileManager.Instance.Service.GetInventoryState();
		if (!inventoryState.Success)
		{
			CommonFunctions.ProcessCommandResult(inventoryState);
			return;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "XML|*.xml";
		saveFileDialog.Title = "Export Inventory";
		if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
		{
			return;
		}
		using StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, append: false);
		streamWriter.Write(inventoryState.CommandMessages[0]);
	}

	private void OnMenuToolsImportInventory(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "XML|*.xml";
		openFileDialog.Title = "Import Inventory";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			string inventoryState;
			using (StreamReader streamReader = new StreamReader(openFileDialog.FileName))
			{
				inventoryState = streamReader.ReadToEnd();
			}
			HardwareCommandResult hardwareCommandResult = ProfileManager.Instance.Service.SetInventoryState(inventoryState);
			CommonFunctions.ProcessCommandResult(hardwareCommandResult);
			if (!hardwareCommandResult.Success)
			{
				MessageBox.Show("Import Inventory Failed.", "Failed to import inventory.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
	}

	private void OnMenuToolsExportXml(object sender, EventArgs e)
	{
		HardwareCommandResult inventoryState = ProfileManager.Instance.Service.GetInventoryState();
		if (!inventoryState.Success)
		{
			CommonFunctions.ProcessCommandResult(inventoryState);
			return;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "XML|*.xml";
		saveFileDialog.Title = "Export Inventory xml";
		if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName) || !File.Exists(saveFileDialog.FileName))
		{
			return;
		}
		using StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, append: false);
		streamWriter.Write(inventoryState.CommandMessages[0]);
	}

	private void OnMenuToolsImportXml(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "XML|*.xml";
		openFileDialog.Title = "Import Inventory xml";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			string inventoryState;
			using (StreamReader streamReader = new StreamReader(openFileDialog.FileName))
			{
				inventoryState = streamReader.ReadToEnd();
			}
			CommonFunctions.ProcessCommandResult(ProfileManager.Instance.Service.SetInventoryState(inventoryState));
		}
	}

	private void OnMenuToolsESPTool(object sender, EventArgs e)
	{
	}

	private void OnSync(object sender, EventArgs e)
	{
		SlotRangeForm slotRangeForm = new SlotRangeForm(skipSlotCheck: true);
		if (slotRangeForm.ShowDialog() == DialogResult.OK)
		{
			HardwareCommandResult hardwareCommandResult = ProfileManager.Instance.Service.HardSync(slotRangeForm.Range, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Normal
			}, out var job);
			if (hardwareCommandResult.Success && autoResumeJobOnScheduleToolStripMenuItem.Checked)
			{
				job.Resume();
			}
			CommonFunctions.ProcessCommandResult(hardwareCommandResult);
		}
	}

	private void OnVend(object sender, EventArgs e)
	{
		BarcodeInputForm barcodeInputForm = new BarcodeInputForm
		{
			Text = "Vend Items"
		};
		if (barcodeInputForm.ShowDialog() == DialogResult.OK && barcodeInputForm.Barcodes.Length != 0)
		{
			HardwareCommandResult hardwareCommandResult = ProfileManager.Instance.Service.Vend(barcodeInputForm.Barcodes, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Normal
			}, out var job);
			if (hardwareCommandResult.Success && autoResumeJobOnScheduleToolStripMenuItem.Checked)
			{
				job.Resume();
			}
			CommonFunctions.ProcessCommandResult(hardwareCommandResult);
		}
	}

	private void OnNewInstallSync(object sender, EventArgs e)
	{
		SlotRangeForm slotRangeForm = new SlotRangeForm(skipSlotCheck: true);
		if (slotRangeForm.ShowDialog() == DialogResult.OK)
		{
			HardwareCommandResult hardwareCommandResult = ProfileManager.Instance.Service.HardSync(slotRangeForm.Range, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Normal
			}, out var job);
			if (hardwareCommandResult.Success && autoResumeJobOnScheduleToolStripMenuItem.Checked)
			{
				job.Resume();
			}
			CommonFunctions.ProcessCommandResult(hardwareCommandResult);
		}
	}

	private void m_getMaintListButton_Click(object sender, EventArgs e)
	{
	}

	private void m_getMachineInfoButton_Click(object sender, EventArgs e)
	{
	}

	private void OnExecuteScript(object sender, EventArgs e)
	{
		ExecuteScriptForm executeScriptForm = new ExecuteScriptForm();
		if (executeScriptForm.ShowDialog() != DialogResult.Cancel)
		{
			CommonFunctions.ProcessCommandResult(ProfileManager.Instance.Service.ScheduleJob(executeScriptForm.SelectedScript, null, enableDebugging: false, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Normal
			}, out var _));
		}
	}

	private void LockControls()
	{
		EnvironmentHelper.LockControl(thinToolStripMenuItem);
		EnvironmentHelper.LockControl(m_thinButton);
		EnvironmentHelper.LockControl(qlmUnloadToolStripMenuItem);
		EnvironmentHelper.LockControl(m_unloadButton);
		EnvironmentHelper.LockControl(unloadThinToolStripMenuItem);
		EnvironmentHelper.LockControl(m_unloadThinButton);
		EnvironmentHelper.LockControl(m_initialNightSyncButton);
		EnvironmentHelper.LockControl(newInstallSyncToolStripMenuItem);
		EnvironmentHelper.LockControl(immediateWindowToolStripMenuItem);
		EnvironmentHelper.LockControl(propertiesWindowToolStripMenuItem);
		EnvironmentHelper.LockControl(outputToolStripMenuItem);
		EnvironmentHelper.LockControl(getInventoryStateToolStripMenuItem);
		EnvironmentHelper.LockControl(resetInventoryStateToolStripMenuItem);
	}

	private void OnFormClose(object sender, FormClosingEventArgs e)
	{
		CloseForm();
	}

	private void OnClose(object sender, EventArgs e)
	{
		CloseForm();
	}

	private void DevMode()
	{
		LeftPanel.Instance.AddTab(LeftPanelTab.Immediate);
		immediateWindowToolStripMenuItem.Checked = true;
		ListViewFactory.Instance.MakeTab(ListViewNames.OutputWindow, enable: true);
		outputToolStripMenuItem.Checked = true;
		autoResumeJobOnScheduleToolStripMenuItem.Checked = true;
		ProfileManager.Instance.Connected += delegate
		{
			CommonFunctions.ProcessCommandResult(ProfileManager.Instance.Service.ExecuteImmediate("NOP", out var _));
			EnvironmentHelper.IsLocked = EnvironmentHelper.IsLocked;
		};
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.HAL.Management.Console.Form1));
		this.m_menuStrip = new System.Windows.Forms.MenuStrip();
		this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.lockSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.unlockSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolbarsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.standardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.jobControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator49 = new System.Windows.Forms.ToolStripSeparator();
		this.pickerSensorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator46 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripSeparator47 = new System.Windows.Forms.ToolStripSeparator();
		this.programsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.autoResumeJobOnScheduleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
		this.executeScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
		this.initializeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
		this.vendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.returnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
		this.syncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.softSyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
		this.thinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.qlmUnloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.unloadThinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
		this.getMaintenanceListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.getMachineListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
		this.newInstallSyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.getInventoryStateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.resetInventoryStateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator26 = new System.Windows.Forms.ToolStripSeparator();
		this.reloadHalConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.backupHalConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator27 = new System.Windows.Forms.ToolStripSeparator();
		this.dataInfoAndConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.emptySearchPatternToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.aboutHalManagementConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_standardToolStrip = new System.Windows.Forms.ToolStrip();
		this.m_lockButton = new System.Windows.Forms.ToolStripButton();
		this.m_unlockButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator35 = new System.Windows.Forms.ToolStripSeparator();
		this.m_profileLabel = new System.Windows.Forms.ToolStripLabel();
		this.m_connectButton = new System.Windows.Forms.ToolStripButton();
		this.m_disconnectButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator36 = new System.Windows.Forms.ToolStripSeparator();
		this.m_saveButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator40 = new System.Windows.Forms.ToolStripSeparator();
		this.m_executeScriptButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator41 = new System.Windows.Forms.ToolStripSeparator();
		this.m_initButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator42 = new System.Windows.Forms.ToolStripSeparator();
		this.m_vendButton = new System.Windows.Forms.ToolStripButton();
		this.m_returnButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator43 = new System.Windows.Forms.ToolStripSeparator();
		this.m_syncButton = new System.Windows.Forms.ToolStripButton();
		this.m_softSyncButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator44 = new System.Windows.Forms.ToolStripSeparator();
		this.m_thinButton = new System.Windows.Forms.ToolStripButton();
		this.m_unloadButton = new System.Windows.Forms.ToolStripButton();
		this.m_unloadThinButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator45 = new System.Windows.Forms.ToolStripSeparator();
		this.m_getMaintListButton = new System.Windows.Forms.ToolStripButton();
		this.m_getMachineInfoButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator50 = new System.Windows.Forms.ToolStripSeparator();
		this.m_initialNightSyncButton = new System.Windows.Forms.ToolStripButton();
		this.m_timer = new System.Windows.Forms.Timer(this.components);
		this.m_eastWestSplitContainer = new System.Windows.Forms.SplitContainer();
		this.m_northSouthSplitContainer = new System.Windows.Forms.SplitContainer();
		this.m_listViewTabControl = new System.Windows.Forms.TabControl();
		this.immediateWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.propertiesWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.errorListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.outputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.jobListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.stackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.symbolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.programEventsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.programResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_menuStrip.SuspendLayout();
		this.m_standardToolStrip.SuspendLayout();
		this.m_eastWestSplitContainer.Panel2.SuspendLayout();
		this.m_eastWestSplitContainer.SuspendLayout();
		this.m_northSouthSplitContainer.SuspendLayout();
		base.SuspendLayout();
		this.m_menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.toolStripMenuItem1, this.viewToolStripMenuItem, this.programsToolStripMenuItem, this.toolsToolStripMenuItem, this.helpToolStripMenuItem });
		this.m_menuStrip.Location = new System.Drawing.Point(0, 0);
		this.m_menuStrip.Name = "m_menuStrip";
		this.m_menuStrip.Size = new System.Drawing.Size(1024, 24);
		this.m_menuStrip.TabIndex = 0;
		this.m_menuStrip.Text = "Menu ";
		this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.connectToolStripMenuItem, this.disconnectToolStripMenuItem, this.toolStripSeparator3, this.saveToolStripMenuItem, this.toolStripSeparator4, this.lockSettingsToolStripMenuItem, this.unlockSettingsToolStripMenuItem, this.toolStripSeparator5, this.exitToolStripMenuItem });
		this.toolStripMenuItem1.Name = "toolStripMenuItem1";
		this.toolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
		this.toolStripMenuItem1.Text = "&File";
		this.connectToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.connect;
		this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
		this.connectToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
		this.connectToolStripMenuItem.Text = "&Connect";
		this.disconnectToolStripMenuItem.Enabled = false;
		this.disconnectToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.disconnect;
		this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
		this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
		this.disconnectToolStripMenuItem.Text = "&Disconnect";
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(153, 6);
		this.saveToolStripMenuItem.Enabled = false;
		this.saveToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.save;
		this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
		this.saveToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
		this.saveToolStripMenuItem.Text = "&Save";
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(153, 6);
		this.lockSettingsToolStripMenuItem.Enabled = false;
		this.lockSettingsToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.locked;
		this.lockSettingsToolStripMenuItem.Name = "lockSettingsToolStripMenuItem";
		this.lockSettingsToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
		this.lockSettingsToolStripMenuItem.Text = "&Lock Settings";
		this.unlockSettingsToolStripMenuItem.Enabled = false;
		this.unlockSettingsToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.unlock;
		this.unlockSettingsToolStripMenuItem.Name = "unlockSettingsToolStripMenuItem";
		this.unlockSettingsToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
		this.unlockSettingsToolStripMenuItem.Text = "&Unlock Settings";
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(153, 6);
		this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
		this.exitToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
		this.exitToolStripMenuItem.Text = "&Exit";
		this.exitToolStripMenuItem.Click += new System.EventHandler(OnClose);
		this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[14]
		{
			this.toolbarsToolStripMenuItem, this.toolStripSeparator49, this.immediateWindowToolStripMenuItem, this.propertiesWindowToolStripMenuItem, this.pickerSensorsToolStripMenuItem, this.toolStripSeparator46, this.errorListToolStripMenuItem, this.outputToolStripMenuItem, this.toolStripSeparator47, this.jobListToolStripMenuItem,
			this.stackToolStripMenuItem, this.symbolsToolStripMenuItem, this.programEventsToolStripMenuItem, this.programResultsToolStripMenuItem
		});
		this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
		this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
		this.viewToolStripMenuItem.Text = "&View";
		this.toolbarsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.standardToolStripMenuItem, this.jobControlToolStripMenuItem });
		this.toolbarsToolStripMenuItem.Enabled = false;
		this.toolbarsToolStripMenuItem.Name = "toolbarsToolStripMenuItem";
		this.toolbarsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.toolbarsToolStripMenuItem.Text = "Toolbars";
		this.standardToolStripMenuItem.Checked = true;
		this.standardToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
		this.standardToolStripMenuItem.Name = "standardToolStripMenuItem";
		this.standardToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
		this.standardToolStripMenuItem.Text = "Standard";
		this.jobControlToolStripMenuItem.Checked = true;
		this.jobControlToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
		this.jobControlToolStripMenuItem.Name = "jobControlToolStripMenuItem";
		this.jobControlToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
		this.jobControlToolStripMenuItem.Text = "Job Control";
		this.toolStripSeparator49.Name = "toolStripSeparator49";
		this.toolStripSeparator49.Size = new System.Drawing.Size(190, 6);
		this.pickerSensorsToolStripMenuItem.Checked = true;
		this.pickerSensorsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
		this.pickerSensorsToolStripMenuItem.Enabled = false;
		this.pickerSensorsToolStripMenuItem.Name = "pickerSensorsToolStripMenuItem";
		this.pickerSensorsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.pickerSensorsToolStripMenuItem.Text = "Picker Sensors";
		this.toolStripSeparator46.Name = "toolStripSeparator46";
		this.toolStripSeparator46.Size = new System.Drawing.Size(190, 6);
		this.toolStripSeparator47.Name = "toolStripSeparator47";
		this.toolStripSeparator47.Size = new System.Drawing.Size(190, 6);
		this.programsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[20]
		{
			this.autoResumeJobOnScheduleToolStripMenuItem, this.toolStripSeparator11, this.executeScriptToolStripMenuItem, this.toolStripSeparator12, this.initializeToolStripMenuItem, this.toolStripSeparator13, this.vendToolStripMenuItem, this.returnToolStripMenuItem, this.toolStripSeparator14, this.syncToolStripMenuItem,
			this.softSyncToolStripMenuItem, this.toolStripSeparator15, this.thinToolStripMenuItem, this.qlmUnloadToolStripMenuItem, this.unloadThinToolStripMenuItem, this.toolStripSeparator16, this.getMaintenanceListToolStripMenuItem, this.getMachineListToolStripMenuItem, this.toolStripSeparator17, this.newInstallSyncToolStripMenuItem
		});
		this.programsToolStripMenuItem.Name = "programsToolStripMenuItem";
		this.programsToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
		this.programsToolStripMenuItem.Text = "&Programs";
		this.autoResumeJobOnScheduleToolStripMenuItem.CheckOnClick = true;
		this.autoResumeJobOnScheduleToolStripMenuItem.Name = "autoResumeJobOnScheduleToolStripMenuItem";
		this.autoResumeJobOnScheduleToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.autoResumeJobOnScheduleToolStripMenuItem.Text = "&Auto Resume Job on Submit";
		this.toolStripSeparator11.Name = "toolStripSeparator11";
		this.toolStripSeparator11.Size = new System.Drawing.Size(221, 6);
		this.executeScriptToolStripMenuItem.Enabled = false;
		this.executeScriptToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.execute;
		this.executeScriptToolStripMenuItem.Name = "executeScriptToolStripMenuItem";
		this.executeScriptToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.executeScriptToolStripMenuItem.Text = "&Execute Script";
		this.executeScriptToolStripMenuItem.Click += new System.EventHandler(OnExecuteScript);
		this.toolStripSeparator12.Name = "toolStripSeparator12";
		this.toolStripSeparator12.Size = new System.Drawing.Size(221, 6);
		this.initializeToolStripMenuItem.Enabled = false;
		this.initializeToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.init;
		this.initializeToolStripMenuItem.Name = "initializeToolStripMenuItem";
		this.initializeToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.initializeToolStripMenuItem.Tag = "init";
		this.initializeToolStripMenuItem.Text = "&Initialize";
		this.initializeToolStripMenuItem.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.toolStripSeparator13.Name = "toolStripSeparator13";
		this.toolStripSeparator13.Size = new System.Drawing.Size(221, 6);
		this.vendToolStripMenuItem.Enabled = false;
		this.vendToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.cd_in_case;
		this.vendToolStripMenuItem.Name = "vendToolStripMenuItem";
		this.vendToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.vendToolStripMenuItem.Tag = "vend";
		this.vendToolStripMenuItem.Text = "&Vend";
		this.vendToolStripMenuItem.Click += new System.EventHandler(OnVend);
		this.returnToolStripMenuItem.Enabled = false;
		this.returnToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.cd_in_case_return;
		this.returnToolStripMenuItem.Name = "returnToolStripMenuItem";
		this.returnToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.returnToolStripMenuItem.Tag = "return";
		this.returnToolStripMenuItem.Text = "&Return";
		this.returnToolStripMenuItem.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.toolStripSeparator14.Name = "toolStripSeparator14";
		this.toolStripSeparator14.Size = new System.Drawing.Size(221, 6);
		this.syncToolStripMenuItem.Enabled = false;
		this.syncToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.sync;
		this.syncToolStripMenuItem.Name = "syncToolStripMenuItem";
		this.syncToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.syncToolStripMenuItem.Text = "&Sync";
		this.syncToolStripMenuItem.Click += new System.EventHandler(OnSync);
		this.softSyncToolStripMenuItem.Enabled = false;
		this.softSyncToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.soft_sync;
		this.softSyncToolStripMenuItem.Name = "softSyncToolStripMenuItem";
		this.softSyncToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.softSyncToolStripMenuItem.Tag = "soft-sync";
		this.softSyncToolStripMenuItem.Text = "S&oft Sync";
		this.softSyncToolStripMenuItem.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.toolStripSeparator15.Name = "toolStripSeparator15";
		this.toolStripSeparator15.Size = new System.Drawing.Size(221, 6);
		this.thinToolStripMenuItem.Enabled = false;
		this.thinToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.thin;
		this.thinToolStripMenuItem.Name = "thinToolStripMenuItem";
		this.thinToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.thinToolStripMenuItem.Tag = "thin";
		this.thinToolStripMenuItem.Text = "&Thin";
		this.thinToolStripMenuItem.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.qlmUnloadToolStripMenuItem.Enabled = false;
		this.qlmUnloadToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.qlm_unload;
		this.qlmUnloadToolStripMenuItem.Name = "qlmUnloadToolStripMenuItem";
		this.qlmUnloadToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.qlmUnloadToolStripMenuItem.Tag = "qlm-unload";
		this.qlmUnloadToolStripMenuItem.Text = "Qlm &Unload";
		this.qlmUnloadToolStripMenuItem.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.unloadThinToolStripMenuItem.Enabled = false;
		this.unloadThinToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.UnloadThin;
		this.unloadThinToolStripMenuItem.Name = "unloadThinToolStripMenuItem";
		this.unloadThinToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.unloadThinToolStripMenuItem.Tag = "unload-thin";
		this.unloadThinToolStripMenuItem.Text = "Un&load Thin";
		this.unloadThinToolStripMenuItem.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.toolStripSeparator16.Name = "toolStripSeparator16";
		this.toolStripSeparator16.Size = new System.Drawing.Size(221, 6);
		this.getMaintenanceListToolStripMenuItem.Enabled = false;
		this.getMaintenanceListToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.todo_list;
		this.getMaintenanceListToolStripMenuItem.Name = "getMaintenanceListToolStripMenuItem";
		this.getMaintenanceListToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.getMaintenanceListToolStripMenuItem.Tag = "get-maintenance-list";
		this.getMaintenanceListToolStripMenuItem.Text = "Get Maintenance List";
		this.getMaintenanceListToolStripMenuItem.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.getMachineListToolStripMenuItem.Enabled = false;
		this.getMachineListToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.computer;
		this.getMachineListToolStripMenuItem.Name = "getMachineListToolStripMenuItem";
		this.getMachineListToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.getMachineListToolStripMenuItem.Tag = "get-machine-info";
		this.getMachineListToolStripMenuItem.Text = "Get Machine List";
		this.getMachineListToolStripMenuItem.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.toolStripSeparator17.Name = "toolStripSeparator17";
		this.toolStripSeparator17.Size = new System.Drawing.Size(221, 6);
		this.newInstallSyncToolStripMenuItem.Enabled = false;
		this.newInstallSyncToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.newinstallsync;
		this.newInstallSyncToolStripMenuItem.Name = "newInstallSyncToolStripMenuItem";
		this.newInstallSyncToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.newInstallSyncToolStripMenuItem.Text = "&New Install Sync";
		this.newInstallSyncToolStripMenuItem.Click += new System.EventHandler(OnNewInstallSync);
		this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.getInventoryStateToolStripMenuItem, this.resetInventoryStateToolStripMenuItem, this.toolStripSeparator26, this.reloadHalConfigurationToolStripMenuItem, this.backupHalConfigurationToolStripMenuItem, this.toolStripSeparator27, this.dataInfoAndConfigurationToolStripMenuItem, this.emptySearchPatternToolToolStripMenuItem });
		this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
		this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
		this.toolsToolStripMenuItem.Text = "&Tools";
		this.getInventoryStateToolStripMenuItem.Enabled = false;
		this.getInventoryStateToolStripMenuItem.Name = "getInventoryStateToolStripMenuItem";
		this.getInventoryStateToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.getInventoryStateToolStripMenuItem.Text = "&Get Inventory State";
		this.getInventoryStateToolStripMenuItem.Click += new System.EventHandler(OnMenuToolsExportInventory);
		this.resetInventoryStateToolStripMenuItem.Enabled = false;
		this.resetInventoryStateToolStripMenuItem.Name = "resetInventoryStateToolStripMenuItem";
		this.resetInventoryStateToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.resetInventoryStateToolStripMenuItem.Text = "&Reset Inventory State";
		this.resetInventoryStateToolStripMenuItem.Click += new System.EventHandler(OnMenuToolsImportInventory);
		this.toolStripSeparator26.Name = "toolStripSeparator26";
		this.toolStripSeparator26.Size = new System.Drawing.Size(221, 6);
		this.reloadHalConfigurationToolStripMenuItem.Enabled = false;
		this.reloadHalConfigurationToolStripMenuItem.Name = "reloadHalConfigurationToolStripMenuItem";
		this.reloadHalConfigurationToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.reloadHalConfigurationToolStripMenuItem.Text = "Reload Hal Configuration";
		this.reloadHalConfigurationToolStripMenuItem.Click += new System.EventHandler(OnMenuToolsImportXml);
		this.backupHalConfigurationToolStripMenuItem.Enabled = false;
		this.backupHalConfigurationToolStripMenuItem.Name = "backupHalConfigurationToolStripMenuItem";
		this.backupHalConfigurationToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.backupHalConfigurationToolStripMenuItem.Text = "Backup Hal Configuration";
		this.backupHalConfigurationToolStripMenuItem.Click += new System.EventHandler(OnMenuToolsExportInventory);
		this.toolStripSeparator27.Name = "toolStripSeparator27";
		this.toolStripSeparator27.Size = new System.Drawing.Size(221, 6);
		this.dataInfoAndConfigurationToolStripMenuItem.Enabled = false;
		this.dataInfoAndConfigurationToolStripMenuItem.Name = "dataInfoAndConfigurationToolStripMenuItem";
		this.dataInfoAndConfigurationToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.dataInfoAndConfigurationToolStripMenuItem.Text = "&Deck Info and Configuration";
		this.dataInfoAndConfigurationToolStripMenuItem.Click += new System.EventHandler(OnMenuToolsDeckConfiguration);
		this.emptySearchPatternToolToolStripMenuItem.Enabled = false;
		this.emptySearchPatternToolToolStripMenuItem.Name = "emptySearchPatternToolToolStripMenuItem";
		this.emptySearchPatternToolToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
		this.emptySearchPatternToolToolStripMenuItem.Text = "&Empty Search Pattern Tool";
		this.emptySearchPatternToolToolStripMenuItem.Click += new System.EventHandler(OnMenuToolsESPTool);
		this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.aboutHalManagementConsoleToolStripMenuItem });
		this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
		this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
		this.helpToolStripMenuItem.Text = "&Help";
		this.aboutHalManagementConsoleToolStripMenuItem.Image = Redbox.HAL.Management.Console.Properties.Resources.hal;
		this.aboutHalManagementConsoleToolStripMenuItem.Name = "aboutHalManagementConsoleToolStripMenuItem";
		this.aboutHalManagementConsoleToolStripMenuItem.Size = new System.Drawing.Size(248, 22);
		this.aboutHalManagementConsoleToolStripMenuItem.Text = "&About Hal Management Console";
		this.aboutHalManagementConsoleToolStripMenuItem.Click += new System.EventHandler(OnMenuHelpAbout);
		this.m_standardToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[27]
		{
			this.m_lockButton, this.m_unlockButton, this.toolStripSeparator35, this.m_profileLabel, this.m_connectButton, this.m_disconnectButton, this.toolStripSeparator36, this.m_saveButton, this.toolStripSeparator40, this.m_executeScriptButton,
			this.toolStripSeparator41, this.m_initButton, this.toolStripSeparator42, this.m_vendButton, this.m_returnButton, this.toolStripSeparator43, this.m_syncButton, this.m_softSyncButton, this.toolStripSeparator44, this.m_thinButton,
			this.m_unloadButton, this.m_unloadThinButton, this.toolStripSeparator45, this.m_getMaintListButton, this.m_getMachineInfoButton, this.toolStripSeparator50, this.m_initialNightSyncButton
		});
		this.m_standardToolStrip.Location = new System.Drawing.Point(0, 24);
		this.m_standardToolStrip.Name = "m_standardToolStrip";
		this.m_standardToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
		this.m_standardToolStrip.Size = new System.Drawing.Size(1024, 25);
		this.m_standardToolStrip.TabIndex = 1;
		this.m_standardToolStrip.Text = "Standard ToolStrip";
		this.m_lockButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_lockButton.Enabled = false;
		this.m_lockButton.Image = (System.Drawing.Image)resources.GetObject("m_lockButton.Image");
		this.m_lockButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_lockButton.Name = "m_lockButton";
		this.m_lockButton.Size = new System.Drawing.Size(23, 22);
		this.m_lockButton.Text = "Lock";
		this.m_unlockButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_unlockButton.Enabled = false;
		this.m_unlockButton.Image = (System.Drawing.Image)resources.GetObject("m_unlockButton.Image");
		this.m_unlockButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_unlockButton.Name = "m_unlockButton";
		this.m_unlockButton.Size = new System.Drawing.Size(23, 22);
		this.m_unlockButton.Text = "Unlock";
		this.toolStripSeparator35.Name = "toolStripSeparator35";
		this.toolStripSeparator35.Size = new System.Drawing.Size(6, 25);
		this.m_profileLabel.Name = "m_profileLabel";
		this.m_profileLabel.Size = new System.Drawing.Size(44, 22);
		this.m_profileLabel.Text = "Profile:";
		this.m_connectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_connectButton.Image = (System.Drawing.Image)resources.GetObject("m_connectButton.Image");
		this.m_connectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_connectButton.Name = "m_connectButton";
		this.m_connectButton.Size = new System.Drawing.Size(23, 22);
		this.m_connectButton.Text = "Connect";
		this.m_disconnectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_disconnectButton.Enabled = false;
		this.m_disconnectButton.Image = (System.Drawing.Image)resources.GetObject("m_disconnectButton.Image");
		this.m_disconnectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_disconnectButton.Name = "m_disconnectButton";
		this.m_disconnectButton.Size = new System.Drawing.Size(23, 22);
		this.m_disconnectButton.Text = "Disconnect";
		this.toolStripSeparator36.Name = "toolStripSeparator36";
		this.toolStripSeparator36.Size = new System.Drawing.Size(6, 25);
		this.m_saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_saveButton.Enabled = false;
		this.m_saveButton.Image = (System.Drawing.Image)resources.GetObject("m_saveButton.Image");
		this.m_saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_saveButton.Name = "m_saveButton";
		this.m_saveButton.Size = new System.Drawing.Size(23, 22);
		this.m_saveButton.Text = "Save";
		this.toolStripSeparator40.Name = "toolStripSeparator40";
		this.toolStripSeparator40.Size = new System.Drawing.Size(6, 25);
		this.m_executeScriptButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_executeScriptButton.Enabled = false;
		this.m_executeScriptButton.Image = (System.Drawing.Image)resources.GetObject("m_executeScriptButton.Image");
		this.m_executeScriptButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_executeScriptButton.Name = "m_executeScriptButton";
		this.m_executeScriptButton.Size = new System.Drawing.Size(23, 22);
		this.m_executeScriptButton.Text = "Execute Script";
		this.m_executeScriptButton.Click += new System.EventHandler(OnExecuteScript);
		this.toolStripSeparator41.Name = "toolStripSeparator41";
		this.toolStripSeparator41.Size = new System.Drawing.Size(6, 25);
		this.m_initButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_initButton.Enabled = false;
		this.m_initButton.Image = (System.Drawing.Image)resources.GetObject("m_initButton.Image");
		this.m_initButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_initButton.Name = "m_initButton";
		this.m_initButton.Size = new System.Drawing.Size(23, 22);
		this.m_initButton.Tag = "init";
		this.m_initButton.Text = "Init";
		this.m_initButton.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.toolStripSeparator42.Name = "toolStripSeparator42";
		this.toolStripSeparator42.Size = new System.Drawing.Size(6, 25);
		this.m_vendButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_vendButton.Enabled = false;
		this.m_vendButton.Image = (System.Drawing.Image)resources.GetObject("m_vendButton.Image");
		this.m_vendButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_vendButton.Name = "m_vendButton";
		this.m_vendButton.Size = new System.Drawing.Size(23, 22);
		this.m_vendButton.Tag = "vend";
		this.m_vendButton.Text = "Vend";
		this.m_vendButton.Click += new System.EventHandler(OnVend);
		this.m_returnButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_returnButton.Enabled = false;
		this.m_returnButton.Image = (System.Drawing.Image)resources.GetObject("m_returnButton.Image");
		this.m_returnButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_returnButton.Name = "m_returnButton";
		this.m_returnButton.Size = new System.Drawing.Size(23, 22);
		this.m_returnButton.Tag = "return";
		this.m_returnButton.Text = "Return";
		this.m_returnButton.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.toolStripSeparator43.Name = "toolStripSeparator43";
		this.toolStripSeparator43.Size = new System.Drawing.Size(6, 25);
		this.m_syncButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_syncButton.Enabled = false;
		this.m_syncButton.Image = (System.Drawing.Image)resources.GetObject("m_syncButton.Image");
		this.m_syncButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_syncButton.Name = "m_syncButton";
		this.m_syncButton.Size = new System.Drawing.Size(23, 22);
		this.m_syncButton.Tag = "sync";
		this.m_syncButton.Text = "Sync";
		this.m_syncButton.Click += new System.EventHandler(OnSync);
		this.m_softSyncButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_softSyncButton.Enabled = false;
		this.m_softSyncButton.Image = (System.Drawing.Image)resources.GetObject("m_softSyncButton.Image");
		this.m_softSyncButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_softSyncButton.Name = "m_softSyncButton";
		this.m_softSyncButton.Size = new System.Drawing.Size(23, 22);
		this.m_softSyncButton.Tag = "soft-sync";
		this.m_softSyncButton.Text = "Soft Sync";
		this.m_softSyncButton.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.toolStripSeparator44.Name = "toolStripSeparator44";
		this.toolStripSeparator44.Size = new System.Drawing.Size(6, 25);
		this.m_thinButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_thinButton.Enabled = false;
		this.m_thinButton.Image = (System.Drawing.Image)resources.GetObject("m_thinButton.Image");
		this.m_thinButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_thinButton.Name = "m_thinButton";
		this.m_thinButton.Size = new System.Drawing.Size(23, 22);
		this.m_thinButton.Tag = "thin";
		this.m_thinButton.Text = "Thin";
		this.m_thinButton.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.m_unloadButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_unloadButton.Enabled = false;
		this.m_unloadButton.Image = (System.Drawing.Image)resources.GetObject("m_unloadButton.Image");
		this.m_unloadButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_unloadButton.Name = "m_unloadButton";
		this.m_unloadButton.Size = new System.Drawing.Size(23, 22);
		this.m_unloadButton.Tag = "qlm-unload";
		this.m_unloadButton.Text = "QLM Unload";
		this.m_unloadButton.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.m_unloadThinButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_unloadThinButton.Enabled = false;
		this.m_unloadThinButton.Image = (System.Drawing.Image)resources.GetObject("m_unloadThinButton.Image");
		this.m_unloadThinButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_unloadThinButton.Name = "m_unloadThinButton";
		this.m_unloadThinButton.Size = new System.Drawing.Size(23, 22);
		this.m_unloadThinButton.Tag = "unload-thin";
		this.m_unloadThinButton.Text = "QLM Unload Thin Super Job";
		this.m_unloadThinButton.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.toolStripSeparator45.Name = "toolStripSeparator45";
		this.toolStripSeparator45.Size = new System.Drawing.Size(6, 25);
		this.m_getMaintListButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_getMaintListButton.Enabled = false;
		this.m_getMaintListButton.Image = (System.Drawing.Image)resources.GetObject("m_getMaintListButton.Image");
		this.m_getMaintListButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_getMaintListButton.Name = "m_getMaintListButton";
		this.m_getMaintListButton.Size = new System.Drawing.Size(23, 22);
		this.m_getMaintListButton.Tag = "get-maintenance-list";
		this.m_getMaintListButton.Text = "Get Maintenance List";
		this.m_getMaintListButton.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.m_getMachineInfoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_getMachineInfoButton.Enabled = false;
		this.m_getMachineInfoButton.Image = (System.Drawing.Image)resources.GetObject("m_getMachineInfoButton.Image");
		this.m_getMachineInfoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_getMachineInfoButton.Name = "m_getMachineInfoButton";
		this.m_getMachineInfoButton.Size = new System.Drawing.Size(23, 22);
		this.m_getMachineInfoButton.Tag = "get-machine-info";
		this.m_getMachineInfoButton.Text = "Get Machine Info";
		this.m_getMachineInfoButton.Click += new System.EventHandler(OnScheduleScriptByTag);
		this.toolStripSeparator50.Name = "toolStripSeparator50";
		this.toolStripSeparator50.Size = new System.Drawing.Size(6, 25);
		this.m_initialNightSyncButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_initialNightSyncButton.Enabled = false;
		this.m_initialNightSyncButton.Image = (System.Drawing.Image)resources.GetObject("m_initialNightSyncButton.Image");
		this.m_initialNightSyncButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_initialNightSyncButton.Name = "m_initialNightSyncButton";
		this.m_initialNightSyncButton.Size = new System.Drawing.Size(23, 22);
		this.m_initialNightSyncButton.Text = "Initial Night Sync";
		this.m_initialNightSyncButton.Click += new System.EventHandler(OnNewInstallSync);
		this.m_timer.Interval = 1500;
		this.m_timer.Tick += new System.EventHandler(OnGlobalTimer);
		this.m_eastWestSplitContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_eastWestSplitContainer.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_eastWestSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.m_eastWestSplitContainer.IsSplitterFixed = true;
		this.m_eastWestSplitContainer.Location = new System.Drawing.Point(0, 74);
		this.m_eastWestSplitContainer.Margin = new System.Windows.Forms.Padding(0);
		this.m_eastWestSplitContainer.Name = "m_eastWestSplitContainer";
		this.m_eastWestSplitContainer.Panel1.BackColor = System.Drawing.Color.White;
		this.m_eastWestSplitContainer.Panel2.Controls.Add(this.m_northSouthSplitContainer);
		this.m_eastWestSplitContainer.Panel2MinSize = 20;
		this.m_eastWestSplitContainer.Size = new System.Drawing.Size(1024, 669);
		this.m_eastWestSplitContainer.SplitterDistance = 252;
		this.m_eastWestSplitContainer.SplitterWidth = 6;
		this.m_eastWestSplitContainer.TabIndex = 100;
		this.m_eastWestSplitContainer.TabStop = false;
		this.m_northSouthSplitContainer.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_northSouthSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.m_northSouthSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_northSouthSplitContainer.IsSplitterFixed = true;
		this.m_northSouthSplitContainer.Location = new System.Drawing.Point(0, 0);
		this.m_northSouthSplitContainer.Name = "m_northSouthSplitContainer";
		this.m_northSouthSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.m_northSouthSplitContainer.Panel1.BackColor = System.Drawing.Color.White;
		this.m_northSouthSplitContainer.Panel2.BackColor = System.Drawing.Color.White;
		this.m_northSouthSplitContainer.Size = new System.Drawing.Size(766, 669);
		this.m_northSouthSplitContainer.SplitterDistance = 343;
		this.m_northSouthSplitContainer.SplitterWidth = 6;
		this.m_northSouthSplitContainer.TabIndex = 100;
		this.m_northSouthSplitContainer.TabStop = false;
		this.m_listViewTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_listViewTabControl.Location = new System.Drawing.Point(0, 0);
		this.m_listViewTabControl.Name = "m_listViewTabControl";
		this.m_listViewTabControl.SelectedIndex = 0;
		this.m_listViewTabControl.Size = new System.Drawing.Size(762, 215);
		this.m_listViewTabControl.TabIndex = 0;
		this.immediateWindowToolStripMenuItem.CheckOnClick = true;
		this.immediateWindowToolStripMenuItem.Enabled = false;
		this.immediateWindowToolStripMenuItem.Name = "immediateWindowToolStripMenuItem";
		this.immediateWindowToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.immediateWindowToolStripMenuItem.Tag = Redbox.HAL.Management.Console.LeftPanelTab.Immediate;
		this.immediateWindowToolStripMenuItem.Text = "&Immediate Window";
		this.immediateWindowToolStripMenuItem.Click += new System.EventHandler(OnMenuLeftSideToggle);
		this.propertiesWindowToolStripMenuItem.Checked = true;
		this.propertiesWindowToolStripMenuItem.CheckOnClick = true;
		this.propertiesWindowToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
		this.propertiesWindowToolStripMenuItem.Enabled = false;
		this.propertiesWindowToolStripMenuItem.Name = "propertiesWindowToolStripMenuItem";
		this.propertiesWindowToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
		this.propertiesWindowToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.propertiesWindowToolStripMenuItem.Tag = Redbox.HAL.Management.Console.LeftPanelTab.Properties;
		this.propertiesWindowToolStripMenuItem.Text = "&Properties Window";
		this.propertiesWindowToolStripMenuItem.Click += new System.EventHandler(OnMenuLeftSideToggle);
		this.errorListToolStripMenuItem.CheckOnClick = true;
		this.errorListToolStripMenuItem.Name = "errorListToolStripMenuItem";
		this.errorListToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.E | System.Windows.Forms.Keys.Control;
		this.errorListToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.errorListToolStripMenuItem.Tag = Redbox.HAL.Management.Console.ListViewNames.Errors;
		this.errorListToolStripMenuItem.Text = "Error &List";
		this.errorListToolStripMenuItem.Click += new System.EventHandler(OnMenuListViewToggle);
		this.outputToolStripMenuItem.CheckOnClick = true;
		this.outputToolStripMenuItem.Enabled = false;
		this.outputToolStripMenuItem.Name = "outputToolStripMenuItem";
		this.outputToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.outputToolStripMenuItem.Tag = Redbox.HAL.Management.Console.ListViewNames.OutputWindow;
		this.outputToolStripMenuItem.Text = "&Output";
		this.outputToolStripMenuItem.Click += new System.EventHandler(OnMenuListViewToggle);
		this.jobListToolStripMenuItem.Checked = true;
		this.jobListToolStripMenuItem.CheckOnClick = true;
		this.jobListToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
		this.jobListToolStripMenuItem.Enabled = false;
		this.jobListToolStripMenuItem.Name = "jobListToolStripMenuItem";
		this.jobListToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.jobListToolStripMenuItem.Tag = Redbox.HAL.Management.Console.ListViewNames.Job;
		this.jobListToolStripMenuItem.Text = "&Jobs";
		this.jobListToolStripMenuItem.Click += new System.EventHandler(OnMenuListViewToggle);
		this.stackToolStripMenuItem.Checked = true;
		this.stackToolStripMenuItem.CheckOnClick = true;
		this.stackToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
		this.stackToolStripMenuItem.Name = "stackToolStripMenuItem";
		this.stackToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.stackToolStripMenuItem.Tag = Redbox.HAL.Management.Console.ListViewNames.Stack;
		this.stackToolStripMenuItem.Text = "&Stack";
		this.stackToolStripMenuItem.Click += new System.EventHandler(OnMenuListViewToggle);
		this.symbolsToolStripMenuItem.Checked = true;
		this.symbolsToolStripMenuItem.CheckOnClick = true;
		this.symbolsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
		this.symbolsToolStripMenuItem.Name = "symbolsToolStripMenuItem";
		this.symbolsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.symbolsToolStripMenuItem.Tag = Redbox.HAL.Management.Console.ListViewNames.Symbols;
		this.symbolsToolStripMenuItem.Text = "S&ymbols";
		this.symbolsToolStripMenuItem.Click += new System.EventHandler(OnMenuListViewToggle);
		this.programEventsToolStripMenuItem.Checked = true;
		this.programEventsToolStripMenuItem.CheckOnClick = true;
		this.programEventsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
		this.programEventsToolStripMenuItem.Name = "programEventsToolStripMenuItem";
		this.programEventsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.programEventsToolStripMenuItem.Tag = Redbox.HAL.Management.Console.ListViewNames.ProgramEvents;
		this.programEventsToolStripMenuItem.Text = "Program Events";
		this.programEventsToolStripMenuItem.Click += new System.EventHandler(OnMenuListViewToggle);
		this.programResultsToolStripMenuItem.CheckOnClick = true;
		this.programResultsToolStripMenuItem.Name = "programResultsToolStripMenuItem";
		this.programResultsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
		this.programResultsToolStripMenuItem.Tag = Redbox.HAL.Management.Console.ListViewNames.Results;
		this.programResultsToolStripMenuItem.Text = "Program Results";
		this.programResultsToolStripMenuItem.Click += new System.EventHandler(OnMenuListViewToggle);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1024, 742);
		base.Controls.Add(this.m_standardToolStrip);
		base.Controls.Add(this.m_menuStrip);
		base.Controls.Add(this.m_eastWestSplitContainer);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MainMenuStrip = this.m_menuStrip;
		base.Name = "Form1";
		this.Text = "HAL Management Console";
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		base.Click += new System.EventHandler(OnClose);
		this.m_menuStrip.ResumeLayout(false);
		this.m_menuStrip.PerformLayout();
		this.m_standardToolStrip.ResumeLayout(false);
		this.m_standardToolStrip.PerformLayout();
		this.m_eastWestSplitContainer.Panel2.ResumeLayout(false);
		this.m_eastWestSplitContainer.ResumeLayout(false);
		this.m_northSouthSplitContainer.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
