using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.UI;

public class MainForm : Form
{
	private IContainer components;

	private GroupBox BITSGroup;

	private GroupBox RepositoryGroup;

	private Button m_resetSelectedRepository;

	private Label m_revisiosForSelectedRepositoryLabel;

	private Label m_repositoryListLabel;

	private Button m_rebuild;

	private Button m_activateTo;

	private Button m_updateTo;

	private Button m_manageBITS;

	private Button m_resetBITS;

	private GroupBox updateServiceGroup;

	private Button m_pollUpdateService;

	private ListView m_changes;

	private ListView m_repositories;

	private ColumnHeader m_nameColumn;

	private ColumnHeader m_labelColumn;

	private ColumnHeader m_activeVersion;

	private ColumnHeader m_updatedVersion;

	private ColumnHeader m_hashColumn;

	private ColumnHeader m_labelColumnRevision;

	private Button m_details;

	private Button m_refresh;

	private Button m_refreshChanges;

	private ColumnHeader m_isActive;

	private Button m_optInOut;

	private GroupBox manageScheduleEvents;

	private Button m_resetScheduleEvent;

	private Button m_manageEventScheduler;

	private Button TrimButton;

	private Button m_serverPollUpdateService;

	public MainForm()
	{
		InitializeComponent();
		if (UpdateManagerApplication.Instance.DeveloperMode)
		{
			m_repositories.Columns[0].Width -= 86;
			ColumnHeader value = new ColumnHeader
			{
				Text = "Subscription",
				Width = 86
			};
			m_repositories.Columns.Add(value);
			m_optInOut.Visible = true;
		}
	}

	private void ManageTransfersOnClick(object sender, EventArgs e)
	{
		try
		{
			new TransferManager().Show(this);
		}
		catch (Exception e2)
		{
			ErrorList errors = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception was thrown in Update Mananger in ManageTransfersOnClick.", e2) };
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errors;
			errorForm.ShowDialog();
		}
	}

	private void OnResetBITS(object sender, EventArgs e)
	{
		if (MessageBox.Show("Are you sure you want to cancel all running transfer jobs?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) != DialogResult.Yes)
		{
			return;
		}
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
			ErrorList errors = service.CancelAll();
			if (errors.ContainsError())
			{
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					ErrorForm errorForm = new ErrorForm();
					errorForm.Errors = errors;
					errorForm.ShowDialog();
				});
			}
		});
	}

	private void OnPollUpdateService(object sender, EventArgs e)
	{
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
			ErrorList errors = service.Poll();
			if (errors.ContainsError())
			{
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					ErrorForm errorForm = new ErrorForm();
					errorForm.Errors = errors;
					errorForm.ShowDialog();
				});
			}
		});
	}

	private void OnLoad(object sender, EventArgs e)
	{
		try
		{
			LoadRepositories();
		}
		catch (Exception e2)
		{
			ErrorList errors = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E998", "An unhandled exception was thrown in Update Mananger.", e2) };
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errors;
			errorForm.ShowDialog();
		}
	}

	private void LoadRepositories(int? selected)
	{
		UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
		{
			m_repositories.BeginUpdate();
			m_repositories.Items.Clear();
		});
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<string> allRepositories = service.GetAllRepositories();
		Stopwatch stopwatch = new Stopwatch();
		foreach (string item in allRepositories)
		{
			stopwatch.Start();
			string repository = item;
			string activeLabel = service.GetActiveLabel(repository);
			string activeRevision = service.GetActiveRevision(repository);
			string stagedRevision = service.GetStagedRevision(repository);
			bool subscribed = service.Subscribed(repository);
			UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
			{
				ListViewItem listViewItem = new ListViewItem
				{
					Text = repository,
					Tag = repository
				};
				listViewItem.SubItems.Add(activeLabel);
				listViewItem.SubItems.Add(activeRevision.Substring(0, 8));
				listViewItem.SubItems.Add(stagedRevision.Substring(0, 8));
				if (UpdateManagerApplication.Instance.DeveloperMode)
				{
					listViewItem.SubItems.Add(subscribed ? "On" : "Off");
				}
				m_repositories.Items.Add(listViewItem);
			});
			stopwatch.Stop();
		}
		m_repositories.SelectedIndices.Clear();
		if (selected.HasValue)
		{
			m_repositories.SelectedIndices.Add(selected.Value);
		}
		m_repositories.EndUpdate();
	}

	private void LoadRepositories()
	{
		UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
		{
			LoadRepositories(null);
		});
	}

	private void LoadChanges()
	{
		LoadChanges(null);
	}

	private void LoadChanges(int? selected)
	{
		UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
		{
			m_changes.BeginUpdate();
		});
		try
		{
			int count = 0;
			string repositoryName = null;
			UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
			{
				m_changes.Items.Clear();
				count = m_repositories.SelectedItems.Count;
				if (count == 1)
				{
					ListViewItem listViewItem = m_repositories.SelectedItems[0];
					repositoryName = (string)listViewItem.Tag;
				}
			});
			if (count != 1)
			{
				return;
			}
			IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
			List<IRevLog> changes = service.GetAllChanges(repositoryName);
			string activeRevision = service.GetActiveRevision(repositoryName);
			string stagedRevision = service.GetStagedRevision(repositoryName);
			changes.Reverse();
			UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
			{
				foreach (IRevLog item in changes)
				{
					ListViewItem listViewItem = new ListViewItem
					{
						Text = item.Hash.Substring(0, 8),
						Tag = item
					};
					listViewItem.SubItems.Add(item.Label);
					if (item.Hash == activeRevision)
					{
						listViewItem.SubItems.Add("Active");
					}
					else
					{
						listViewItem.SubItems.Add("");
					}
					if (item.Hash == stagedRevision)
					{
						listViewItem.SubItems.Add("Staged");
					}
					else
					{
						listViewItem.SubItems.Add("");
					}
					m_changes.Items.Add(listViewItem);
				}
				m_changes.SelectedIndices.Clear();
				if (selected.HasValue)
				{
					m_changes.SelectedIndices.Add(selected.Value);
				}
			});
		}
		catch (Exception exception)
		{
			UpdateManagerApplication.Instance.ThreadSafeExceptionDisplay(exception);
		}
		finally
		{
			UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
			{
				m_changes.EndUpdate();
			});
		}
	}

	private void OnRepositorySelectionChange(object sender, EventArgs e)
	{
		try
		{
			LoadChanges();
		}
		catch (Exception e2)
		{
			ErrorList errors = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E997", "An unhandled exception was thrown in Update Mananger.", e2) };
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errors;
			errorForm.ShowDialog();
		}
	}

	private void UpdateToSelectedVersion(object sender, EventArgs e)
	{
		if (m_repositories.SelectedItems.Count < 1 || m_changes.SelectedItems.Count < 1)
		{
			return;
		}
		string selectedRepository = (string)m_repositories.SelectedItems[0].Tag;
		if (!(m_changes.SelectedItems[0].Tag is IRevLog revLog))
		{
			return;
		}
		string selectedChange = revLog.Hash;
		int selectedRepositoryIndex = m_repositories.SelectedIndices[0];
		int selcetedChangeIndex = m_changes.SelectedIndices[0];
		UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
		{
			m_updateTo.Enabled = false;
		});
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
				ErrorList errors = service.UpdateTo(selectedRepository, selectedChange);
				if (errors.ContainsError())
				{
					UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
					{
						ErrorForm errorForm = new ErrorForm();
						errorForm.Errors = errors;
						errorForm.ShowDialog();
					});
				}
				else
				{
					UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
					{
						LoadRepositories(selectedRepositoryIndex);
						LoadChanges(selcetedChangeIndex);
					});
				}
			}
			catch (Exception e2)
			{
				ErrorList errors2 = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E996", "An unhandled exception occurred while polling update service.", e2) };
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					ErrorForm errorForm = new ErrorForm();
					errorForm.Errors = errors2;
					errorForm.ShowDialog();
				});
			}
			finally
			{
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					m_updateTo.Enabled = true;
				});
			}
		});
	}

	private void ActivateToSelectedVersion(object sender, EventArgs e)
	{
		if (m_repositories.SelectedItems.Count < 1 || m_changes.SelectedItems.Count < 1)
		{
			return;
		}
		string selectedRepository = (string)m_repositories.SelectedItems[0].Tag;
		if (!(m_changes.SelectedItems[0].Tag is IRevLog revLog))
		{
			return;
		}
		string selectedChange = revLog.Hash;
		int selectedRepositoryIndex = m_repositories.SelectedIndices[0];
		int selcetedChangeIndex = m_changes.SelectedIndices[0];
		bool shouldReboot = false;
		UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
		{
			m_activateTo.Enabled = false;
		});
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
				ErrorList errors = service.ActivateTo(selectedRepository, selectedChange, out var deferredMove);
				if (errors.ContainsError())
				{
					UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
					{
						ErrorForm errorForm = new ErrorForm();
						errorForm.Errors = errors;
						errorForm.ShowDialog();
					});
				}
				else
				{
					if (deferredMove)
					{
						UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
						{
							if (MessageBox.Show("To complete this activation this computer must restart. Would you like to reboot now?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
							{
								shouldReboot = true;
							}
						});
					}
					UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
					{
						LoadRepositories(selectedRepositoryIndex);
						LoadChanges(selcetedChangeIndex);
					});
				}
			}
			catch (Exception e2)
			{
				ErrorList errors2 = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E995", "An unhandled exception occurred while polling update service.", e2) };
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					ErrorForm errorForm = new ErrorForm();
					errorForm.Errors = errors2;
					errorForm.ShowDialog();
				});
			}
			finally
			{
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					m_activateTo.Enabled = true;
				});
				if (shouldReboot)
				{
					ShutdownTool.Shutdown(ShutdownFlags.Reboot | ShutdownFlags.ForceIfHung, ShutdownReason.MinorUpgrade);
				}
			}
		});
	}

	private void OnRebuildToSelectedVersion(object sender, EventArgs e)
	{
		if (m_repositories.SelectedItems.Count < 1 || m_changes.SelectedItems.Count < 1)
		{
			return;
		}
		string selectedRepository = (string)m_repositories.SelectedItems[0].Tag;
		if (!(m_changes.SelectedItems[0].Tag is IRevLog revLog))
		{
			return;
		}
		string selectedChange = revLog.Hash;
		int selectedRepositoryIndex = m_repositories.SelectedIndices[0];
		int selcetedChangeIndex = m_changes.SelectedIndices[0];
		UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
		{
			m_rebuild.Enabled = false;
		});
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
				ErrorList errors = service.RebuildTo(selectedRepository, selectedChange);
				if (errors.ContainsError())
				{
					UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
					{
						ErrorForm errorForm = new ErrorForm();
						errorForm.Errors = errors;
						errorForm.ShowDialog();
					});
				}
				else
				{
					UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
					{
						LoadRepositories(selectedRepositoryIndex);
						LoadChanges(selcetedChangeIndex);
					});
				}
			}
			catch (Exception e2)
			{
				ErrorList errors2 = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E994", "An unhandled exception occurred while polling update service.", e2) };
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					ErrorForm errorForm = new ErrorForm();
					errorForm.Errors = errors2;
					errorForm.ShowDialog();
				});
			}
			finally
			{
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					m_rebuild.Enabled = true;
				});
			}
		});
	}

	private void OnResetSelectedRepository(object sender, EventArgs e)
	{
		if (m_repositories.SelectedItems.Count < 1)
		{
			return;
		}
		bool shouldReboot = false;
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			try
			{
				foreach (object selectedItem in m_repositories.SelectedItems)
				{
					if (selectedItem is ListViewItem listViewItem)
					{
						ServiceLocator.Instance.GetService<IRepositoryService>().Reset((string)listViewItem.Tag, out var defferedDelete);
						if (defferedDelete)
						{
							UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
							{
								if (MessageBox.Show("To complete this activation this computer must restart. Would you like to reboot now?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
								{
									shouldReboot = true;
								}
							});
						}
					}
				}
			}
			finally
			{
				LoadRepositories();
				LoadChanges();
				if (shouldReboot)
				{
					ShutdownTool.Shutdown(ShutdownFlags.Reboot | ShutdownFlags.ForceIfHung, ShutdownReason.MinorUpgrade);
				}
			}
		});
	}

	private void OnDetails(object sender, EventArgs e)
	{
		try
		{
			if (m_changes.SelectedItems.Count >= 1 && m_changes.SelectedItems[0].Tag is IRevLog log)
			{
				new ChangeDetails(log).Show(this);
			}
		}
		catch (Exception e2)
		{
			ErrorList errors = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E993", "An unhandled exception was thrown in Update Mananger.", e2) };
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errors;
			errorForm.ShowDialog();
		}
	}

	private void OnRefreshRepositories(object sender, EventArgs e)
	{
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, LoadRepositories);
	}

	private void OnRefreshChanges(object sender, EventArgs e)
	{
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, LoadChanges);
	}

	private void OnToggleSubscription(object sender, EventArgs e)
	{
		if (m_repositories.SelectedItems.Count < 1)
		{
			return;
		}
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			foreach (object selectedItem in m_repositories.SelectedItems)
			{
				if (selectedItem is ListViewItem listViewItem)
				{
					IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
					string name = (string)listViewItem.Tag;
					if (service.Subscribed(name))
					{
						service.DisableSubscription(name);
					}
					else
					{
						service.EndableSubscription(name);
					}
				}
			}
			LoadRepositories();
			LoadChanges();
		});
	}

	private void OnManageEventSchedulerClick(object sender, EventArgs e)
	{
		new ManageScheduleEvents().Show(this);
	}

	private void OnResetScheduleEventClick(object sender, EventArgs e)
	{
		if (MessageBox.Show("Are you sure you want to delete all running schedule events?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
		{
			UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
			{
				ServiceLocator.Instance.GetService<ITaskScheduler>().Clear();
			});
		}
	}

	private void OnSelectedChangeChanged(object sender, EventArgs e)
	{
		if (m_changes.SelectedItems.Count > 0)
		{
			Button activateTo = m_activateTo;
			Button rebuild = m_rebuild;
			Button updateTo = m_updateTo;
			bool flag = (m_details.Enabled = true);
			bool flag3 = (updateTo.Enabled = flag);
			bool enabled = (rebuild.Enabled = flag3);
			activateTo.Enabled = enabled;
		}
		else
		{
			Button activateTo2 = m_activateTo;
			Button rebuild2 = m_rebuild;
			Button updateTo2 = m_updateTo;
			bool flag = (m_details.Enabled = false);
			bool flag3 = (updateTo2.Enabled = flag);
			bool enabled = (rebuild2.Enabled = flag3);
			activateTo2.Enabled = enabled;
		}
	}

	private void TrimButton_Click(object sender, EventArgs e)
	{
		if (m_repositories.SelectedItems.Count < 1)
		{
			return;
		}
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			try
			{
				foreach (object selectedItem in m_repositories.SelectedItems)
				{
					if (selectedItem is ListViewItem listViewItem)
					{
						ServiceLocator.Instance.GetService<IRepositoryService>().TrimRepository((string)listViewItem.Tag);
					}
				}
			}
			finally
			{
				LoadRepositories();
				LoadChanges();
			}
		});
	}

	private void m_serverPollUpdateService_Click(object sender, EventArgs e)
	{
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
			ErrorList errors = service.ServerPoll();
			if (errors.ContainsError())
			{
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					ErrorForm errorForm = new ErrorForm();
					errorForm.Errors = errors;
					errorForm.ShowDialog();
				});
			}
		});
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.UpdateManager.UI.MainForm));
		this.BITSGroup = new System.Windows.Forms.GroupBox();
		this.m_manageBITS = new System.Windows.Forms.Button();
		this.m_resetBITS = new System.Windows.Forms.Button();
		this.RepositoryGroup = new System.Windows.Forms.GroupBox();
		this.TrimButton = new System.Windows.Forms.Button();
		this.m_optInOut = new System.Windows.Forms.Button();
		this.m_refreshChanges = new System.Windows.Forms.Button();
		this.m_refresh = new System.Windows.Forms.Button();
		this.m_details = new System.Windows.Forms.Button();
		this.m_changes = new System.Windows.Forms.ListView();
		this.m_hashColumn = new System.Windows.Forms.ColumnHeader();
		this.m_labelColumnRevision = new System.Windows.Forms.ColumnHeader();
		this.m_isActive = new System.Windows.Forms.ColumnHeader();
		this.m_repositories = new System.Windows.Forms.ListView();
		this.m_nameColumn = new System.Windows.Forms.ColumnHeader();
		this.m_labelColumn = new System.Windows.Forms.ColumnHeader();
		this.m_activeVersion = new System.Windows.Forms.ColumnHeader();
		this.m_updatedVersion = new System.Windows.Forms.ColumnHeader();
		this.m_rebuild = new System.Windows.Forms.Button();
		this.m_activateTo = new System.Windows.Forms.Button();
		this.m_updateTo = new System.Windows.Forms.Button();
		this.m_resetSelectedRepository = new System.Windows.Forms.Button();
		this.m_revisiosForSelectedRepositoryLabel = new System.Windows.Forms.Label();
		this.m_repositoryListLabel = new System.Windows.Forms.Label();
		this.updateServiceGroup = new System.Windows.Forms.GroupBox();
		this.m_serverPollUpdateService = new System.Windows.Forms.Button();
		this.m_pollUpdateService = new System.Windows.Forms.Button();
		this.manageScheduleEvents = new System.Windows.Forms.GroupBox();
		this.m_resetScheduleEvent = new System.Windows.Forms.Button();
		this.m_manageEventScheduler = new System.Windows.Forms.Button();
		this.BITSGroup.SuspendLayout();
		this.RepositoryGroup.SuspendLayout();
		this.updateServiceGroup.SuspendLayout();
		this.manageScheduleEvents.SuspendLayout();
		base.SuspendLayout();
		this.BITSGroup.Controls.Add(this.m_manageBITS);
		this.BITSGroup.Controls.Add(this.m_resetBITS);
		this.BITSGroup.Location = new System.Drawing.Point(12, 12);
		this.BITSGroup.Name = "BITSGroup";
		this.BITSGroup.Size = new System.Drawing.Size(187, 56);
		this.BITSGroup.TabIndex = 1;
		this.BITSGroup.TabStop = false;
		this.BITSGroup.Text = "Manage Transfers";
		this.m_manageBITS.Location = new System.Drawing.Point(13, 22);
		this.m_manageBITS.Name = "m_manageBITS";
		this.m_manageBITS.Size = new System.Drawing.Size(75, 23);
		this.m_manageBITS.TabIndex = 1;
		this.m_manageBITS.Text = "Manage";
		this.m_manageBITS.UseVisualStyleBackColor = true;
		this.m_manageBITS.Click += new System.EventHandler(ManageTransfersOnClick);
		this.m_resetBITS.Location = new System.Drawing.Point(94, 22);
		this.m_resetBITS.Name = "m_resetBITS";
		this.m_resetBITS.Size = new System.Drawing.Size(75, 23);
		this.m_resetBITS.TabIndex = 2;
		this.m_resetBITS.Text = "Reset";
		this.m_resetBITS.UseVisualStyleBackColor = true;
		this.m_resetBITS.Click += new System.EventHandler(OnResetBITS);
		this.RepositoryGroup.Controls.Add(this.TrimButton);
		this.RepositoryGroup.Controls.Add(this.m_optInOut);
		this.RepositoryGroup.Controls.Add(this.m_refreshChanges);
		this.RepositoryGroup.Controls.Add(this.m_refresh);
		this.RepositoryGroup.Controls.Add(this.m_details);
		this.RepositoryGroup.Controls.Add(this.m_changes);
		this.RepositoryGroup.Controls.Add(this.m_repositories);
		this.RepositoryGroup.Controls.Add(this.m_rebuild);
		this.RepositoryGroup.Controls.Add(this.m_activateTo);
		this.RepositoryGroup.Controls.Add(this.m_updateTo);
		this.RepositoryGroup.Controls.Add(this.m_resetSelectedRepository);
		this.RepositoryGroup.Controls.Add(this.m_revisiosForSelectedRepositoryLabel);
		this.RepositoryGroup.Controls.Add(this.m_repositoryListLabel);
		this.RepositoryGroup.Location = new System.Drawing.Point(8, 69);
		this.RepositoryGroup.Name = "RepositoryGroup";
		this.RepositoryGroup.Size = new System.Drawing.Size(843, 417);
		this.RepositoryGroup.TabIndex = 9;
		this.RepositoryGroup.TabStop = false;
		this.RepositoryGroup.Text = "Manage Repositories";
		this.TrimButton.Location = new System.Drawing.Point(288, 351);
		this.TrimButton.Name = "TrimButton";
		this.TrimButton.Size = new System.Drawing.Size(104, 23);
		this.TrimButton.TabIndex = 17;
		this.TrimButton.Text = "Trim Revisions";
		this.TrimButton.UseVisualStyleBackColor = true;
		this.TrimButton.Click += new System.EventHandler(TrimButton_Click);
		this.m_optInOut.Location = new System.Drawing.Point(87, 352);
		this.m_optInOut.Name = "m_optInOut";
		this.m_optInOut.Size = new System.Drawing.Size(113, 23);
		this.m_optInOut.TabIndex = 16;
		this.m_optInOut.Text = "Toggle Subscription";
		this.m_optInOut.UseVisualStyleBackColor = true;
		this.m_optInOut.Visible = false;
		this.m_optInOut.Click += new System.EventHandler(OnToggleSubscription);
		this.m_refreshChanges.Location = new System.Drawing.Point(497, 380);
		this.m_refreshChanges.Name = "m_refreshChanges";
		this.m_refreshChanges.Size = new System.Drawing.Size(75, 23);
		this.m_refreshChanges.TabIndex = 34;
		this.m_refreshChanges.Text = "Refresh";
		this.m_refreshChanges.UseVisualStyleBackColor = true;
		this.m_refreshChanges.Click += new System.EventHandler(OnRefreshChanges);
		this.m_refresh.Location = new System.Drawing.Point(6, 352);
		this.m_refresh.Name = "m_refresh";
		this.m_refresh.Size = new System.Drawing.Size(75, 23);
		this.m_refresh.TabIndex = 14;
		this.m_refresh.Text = "Refresh";
		this.m_refresh.UseVisualStyleBackColor = true;
		this.m_refresh.Click += new System.EventHandler(OnRefreshRepositories);
		this.m_details.Enabled = false;
		this.m_details.Location = new System.Drawing.Point(740, 351);
		this.m_details.Name = "m_details";
		this.m_details.Size = new System.Drawing.Size(75, 23);
		this.m_details.TabIndex = 33;
		this.m_details.Text = "Details";
		this.m_details.UseVisualStyleBackColor = true;
		this.m_details.Click += new System.EventHandler(OnDetails);
		this.m_changes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.m_hashColumn, this.m_labelColumnRevision, this.m_isActive });
		this.m_changes.Font = new System.Drawing.Font("Consolas", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_changes.FullRowSelect = true;
		this.m_changes.HideSelection = false;
		this.m_changes.Location = new System.Drawing.Point(498, 43);
		this.m_changes.Name = "m_changes";
		this.m_changes.Size = new System.Drawing.Size(339, 303);
		this.m_changes.TabIndex = 12;
		this.m_changes.UseCompatibleStateImageBehavior = false;
		this.m_changes.View = System.Windows.Forms.View.Details;
		this.m_changes.SelectedIndexChanged += new System.EventHandler(OnSelectedChangeChanged);
		this.m_hashColumn.Text = "Hash";
		this.m_hashColumn.Width = 96;
		this.m_labelColumnRevision.Text = "Label";
		this.m_labelColumnRevision.Width = 100;
		this.m_isActive.Text = "Status";
		this.m_isActive.Width = 136;
		this.m_repositories.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.m_nameColumn, this.m_labelColumn, this.m_activeVersion, this.m_updatedVersion });
		this.m_repositories.Font = new System.Drawing.Font("Consolas", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_repositories.FullRowSelect = true;
		this.m_repositories.HideSelection = false;
		this.m_repositories.Location = new System.Drawing.Point(6, 43);
		this.m_repositories.Name = "m_repositories";
		this.m_repositories.ShowGroups = false;
		this.m_repositories.Size = new System.Drawing.Size(486, 303);
		this.m_repositories.TabIndex = 10;
		this.m_repositories.UseCompatibleStateImageBehavior = false;
		this.m_repositories.View = System.Windows.Forms.View.Details;
		this.m_repositories.SelectedIndexChanged += new System.EventHandler(OnRepositorySelectionChange);
		this.m_nameColumn.Text = "Name";
		this.m_nameColumn.Width = 204;
		this.m_labelColumn.Text = "Active Label";
		this.m_labelColumn.Width = 85;
		this.m_activeVersion.Text = "Active Version";
		this.m_activeVersion.Width = 96;
		this.m_updatedVersion.Text = "Staged Version";
		this.m_updatedVersion.Width = 96;
		this.m_rebuild.Enabled = false;
		this.m_rebuild.Location = new System.Drawing.Point(659, 351);
		this.m_rebuild.Name = "m_rebuild";
		this.m_rebuild.Size = new System.Drawing.Size(75, 23);
		this.m_rebuild.TabIndex = 32;
		this.m_rebuild.Text = "Rebuild To";
		this.m_rebuild.UseVisualStyleBackColor = true;
		this.m_rebuild.Click += new System.EventHandler(OnRebuildToSelectedVersion);
		this.m_activateTo.Enabled = false;
		this.m_activateTo.Location = new System.Drawing.Point(578, 351);
		this.m_activateTo.Name = "m_activateTo";
		this.m_activateTo.Size = new System.Drawing.Size(75, 23);
		this.m_activateTo.TabIndex = 31;
		this.m_activateTo.Text = "Activate To";
		this.m_activateTo.UseVisualStyleBackColor = true;
		this.m_activateTo.Click += new System.EventHandler(ActivateToSelectedVersion);
		this.m_updateTo.Enabled = false;
		this.m_updateTo.Location = new System.Drawing.Point(498, 351);
		this.m_updateTo.Name = "m_updateTo";
		this.m_updateTo.Size = new System.Drawing.Size(75, 23);
		this.m_updateTo.TabIndex = 30;
		this.m_updateTo.Text = "Update To";
		this.m_updateTo.UseVisualStyleBackColor = true;
		this.m_updateTo.Click += new System.EventHandler(UpdateToSelectedVersion);
		this.m_resetSelectedRepository.Location = new System.Drawing.Point(394, 351);
		this.m_resetSelectedRepository.Name = "m_resetSelectedRepository";
		this.m_resetSelectedRepository.Size = new System.Drawing.Size(75, 23);
		this.m_resetSelectedRepository.TabIndex = 18;
		this.m_resetSelectedRepository.Text = "Reset";
		this.m_resetSelectedRepository.UseVisualStyleBackColor = true;
		this.m_resetSelectedRepository.Click += new System.EventHandler(OnResetSelectedRepository);
		this.m_revisiosForSelectedRepositoryLabel.AutoSize = true;
		this.m_revisiosForSelectedRepositoryLabel.Location = new System.Drawing.Point(314, 26);
		this.m_revisiosForSelectedRepositoryLabel.Name = "m_revisiosForSelectedRepositoryLabel";
		this.m_revisiosForSelectedRepositoryLabel.Size = new System.Drawing.Size(167, 13);
		this.m_revisiosForSelectedRepositoryLabel.TabIndex = 3;
		this.m_revisiosForSelectedRepositoryLabel.Text = "Avaliable Revisions for Repository";
		this.m_repositoryListLabel.AutoSize = true;
		this.m_repositoryListLabel.Location = new System.Drawing.Point(10, 26);
		this.m_repositoryListLabel.Name = "m_repositoryListLabel";
		this.m_repositoryListLabel.Size = new System.Drawing.Size(111, 13);
		this.m_repositoryListLabel.TabIndex = 1;
		this.m_repositoryListLabel.Text = "Avaliable Repositories";
		this.updateServiceGroup.Controls.Add(this.m_serverPollUpdateService);
		this.updateServiceGroup.Controls.Add(this.m_pollUpdateService);
		this.updateServiceGroup.Location = new System.Drawing.Point(205, 12);
		this.updateServiceGroup.Name = "updateServiceGroup";
		this.updateServiceGroup.Size = new System.Drawing.Size(182, 56);
		this.updateServiceGroup.TabIndex = 3;
		this.updateServiceGroup.TabStop = false;
		this.updateServiceGroup.Text = "Manage Update Service";
		this.m_serverPollUpdateService.Location = new System.Drawing.Point(11, 22);
		this.m_serverPollUpdateService.Name = "m_serverPollUpdateService";
		this.m_serverPollUpdateService.Size = new System.Drawing.Size(75, 23);
		this.m_serverPollUpdateService.TabIndex = 3;
		this.m_serverPollUpdateService.Text = "Server Poll";
		this.m_serverPollUpdateService.UseVisualStyleBackColor = true;
		this.m_serverPollUpdateService.Click += new System.EventHandler(m_serverPollUpdateService_Click);
		this.m_pollUpdateService.Location = new System.Drawing.Point(96, 22);
		this.m_pollUpdateService.Name = "m_pollUpdateService";
		this.m_pollUpdateService.Size = new System.Drawing.Size(75, 23);
		this.m_pollUpdateService.TabIndex = 4;
		this.m_pollUpdateService.Text = "Poll";
		this.m_pollUpdateService.UseVisualStyleBackColor = true;
		this.m_pollUpdateService.Click += new System.EventHandler(OnPollUpdateService);
		this.manageScheduleEvents.Controls.Add(this.m_resetScheduleEvent);
		this.manageScheduleEvents.Controls.Add(this.m_manageEventScheduler);
		this.manageScheduleEvents.Location = new System.Drawing.Point(393, 12);
		this.manageScheduleEvents.Name = "manageScheduleEvents";
		this.manageScheduleEvents.Size = new System.Drawing.Size(216, 56);
		this.manageScheduleEvents.TabIndex = 6;
		this.manageScheduleEvents.TabStop = false;
		this.manageScheduleEvents.Text = "Manage Event Scheduler";
		this.m_resetScheduleEvent.Location = new System.Drawing.Point(97, 22);
		this.m_resetScheduleEvent.Name = "m_resetScheduleEvent";
		this.m_resetScheduleEvent.Size = new System.Drawing.Size(106, 23);
		this.m_resetScheduleEvent.TabIndex = 7;
		this.m_resetScheduleEvent.Text = "Delete All Events";
		this.m_resetScheduleEvent.UseVisualStyleBackColor = true;
		this.m_resetScheduleEvent.Click += new System.EventHandler(OnResetScheduleEventClick);
		this.m_manageEventScheduler.Location = new System.Drawing.Point(12, 22);
		this.m_manageEventScheduler.Name = "m_manageEventScheduler";
		this.m_manageEventScheduler.Size = new System.Drawing.Size(75, 23);
		this.m_manageEventScheduler.TabIndex = 6;
		this.m_manageEventScheduler.Text = "Manage";
		this.m_manageEventScheduler.UseVisualStyleBackColor = true;
		this.m_manageEventScheduler.Click += new System.EventHandler(OnManageEventSchedulerClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(858, 489);
		base.Controls.Add(this.manageScheduleEvents);
		base.Controls.Add(this.updateServiceGroup);
		base.Controls.Add(this.RepositoryGroup);
		base.Controls.Add(this.BITSGroup);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		this.MaximumSize = new System.Drawing.Size(864, 517);
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(864, 517);
		base.Name = "MainForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Update Manager";
		base.Load += new System.EventHandler(OnLoad);
		this.BITSGroup.ResumeLayout(false);
		this.RepositoryGroup.ResumeLayout(false);
		this.RepositoryGroup.PerformLayout();
		this.updateServiceGroup.ResumeLayout(false);
		this.manageScheduleEvents.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
