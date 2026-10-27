using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.UI;

public class ManageScheduleEvents : Form
{
	private IContainer components;

	private ListView m_events;

	private Button m_deleteEvent;

	private Button m_stopScheduler;

	private Button m_resetScheduler;

	private Button m_forceEvent;

	private Button m_startScheduler;

	private Button m_newScriptEvent;

	private Button m_newShellEvent;

	private Button m_refreshButton;

	private ColumnHeader payload;

	private ColumnHeader type;

	private ColumnHeader cronexpression;

	private ColumnHeader TaskName;

	private Button m_newServerpollEvent;

	public ManageScheduleEvents()
	{
		InitializeComponent();
	}

	private void OnRestartSchdeuler(object sender, EventArgs e)
	{
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			ErrorList errorList = ServiceLocator.Instance.GetService<ITaskScheduler>().Restart();
			if (errorList.ContainsError())
			{
				UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errorList);
			}
			UpdateManagerApplication.Instance.ThreadSafeHostUpdate(SetButtonState);
		});
	}

	private void OnStopScheduler(object sender, EventArgs e)
	{
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			ErrorList errorList = ServiceLocator.Instance.GetService<ITaskScheduler>().Stop();
			if (errorList.ContainsError())
			{
				UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errorList);
			}
			UpdateManagerApplication.Instance.ThreadSafeHostUpdate(SetButtonState);
		});
	}

	private void OnStartScheduler(object sender, EventArgs e)
	{
		UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
		{
			ErrorList errorList = ServiceLocator.Instance.GetService<ITaskScheduler>().Start();
			if (errorList.ContainsError())
			{
				UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errorList);
			}
			UpdateManagerApplication.Instance.ThreadSafeHostUpdate(SetButtonState);
		});
	}

	private void OnLoad(object sender, EventArgs e)
	{
		LoadScheduleEvents();
		SetButtonState();
	}

	private void LoadScheduleEvents()
	{
		UpdateManagerApplication.Instance.AsyncButtonAction(null, delegate
		{
			ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
			List<ITask> tasks;
			try
			{
				service.List(out tasks);
			}
			catch (Exception exception)
			{
				LogHelper.Instance.Log("Could not load list", LogEntryType.Fatal);
				UpdateManagerApplication.Instance.ThreadSafeExceptionDisplay(exception);
				return;
			}
			UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
			{
				try
				{
					m_events.BeginUpdate();
					m_events.Items.Clear();
					foreach (ITask item in tasks)
					{
						ListViewItem listViewItem = new ListViewItem
						{
							Text = item.Name,
							Tag = item
						};
						listViewItem.SubItems.Add(item.Payload);
						listViewItem.SubItems.Add(Enum.GetName(typeof(PayloadType), item.PayloadType));
						string text = ((item.ScheduleType == ScheduleType.Cron) ? item.CronExpression : item.RepeatInterval.ToString());
						if (item.StartOffset.HasValue)
						{
							text = text + " Offset:" + item.StartOffset.ToString();
						}
						listViewItem.SubItems.Add(text);
						m_events.Items.Add(listViewItem);
					}
					m_events.SelectedIndices.Clear();
					m_events.EndUpdate();
				}
				catch (Exception exception2)
				{
					UpdateManagerApplication.Instance.ThreadSafeExceptionDisplay(exception2);
				}
			});
		});
	}

	private void OnNewScriptEvent(object sender, EventArgs e)
	{
		new NewScriptEvent().ShowDialog();
		LoadScheduleEvents();
	}

	private void OnNewShellEvent(object sender, EventArgs e)
	{
		new NewShellEvent().ShowDialog();
		LoadScheduleEvents();
	}

	private void OnNewServerpollEvent(object sender, EventArgs e)
	{
		new NewServerpollEvent().ShowDialog();
		LoadScheduleEvents();
	}

	private void OnDelete(object sender, EventArgs e)
	{
		if (m_events.SelectedItems.Count < 1)
		{
			return;
		}
		try
		{
			ITaskScheduler scheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
			UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
			{
				m_deleteEvent.Enabled = false;
			});
			try
			{
				IEnumerable<ListViewItem> source = from ListViewItem item in m_events.SelectedItems
					where item != null
					select item;
				Func<ListViewItem, ErrorList> func2 = default(Func<ListViewItem, ErrorList>);
				Func<ListViewItem, ErrorList> func = func2;
				if (func == null)
				{
					func = (func2 = (ListViewItem item) => scheduler.Delete(item.SubItems[0].Text));
				}
				using IEnumerator<ErrorForm> enumerator = (from errors2 in source.Select(func)
					where errors2.ContainsError()
					select new ErrorForm
					{
						Errors = errors2
					}).GetEnumerator();
				if (enumerator.MoveNext())
				{
					enumerator.Current.ShowDialog();
					return;
				}
			}
			catch (Exception e2)
			{
				ErrorList foo = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E992", "An unhandled exception occurred while polling update service.", e2) };
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					ErrorForm errorForm3 = new ErrorForm();
					errorForm3.Errors = foo;
					errorForm3.ShowDialog();
				});
			}
			finally
			{
				UpdateManagerApplication.Instance.ThreadSafeHostUpdate(delegate
				{
					m_deleteEvent.Enabled = true;
				});
			}
			ErrorList errorList = scheduler.Restart();
			if (errorList.ContainsError())
			{
				ErrorForm errorForm = new ErrorForm();
				errorForm.Errors = errorList;
				errorForm.ShowDialog();
			}
			LoadScheduleEvents();
		}
		catch (Exception e3)
		{
			ErrorList errors = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E991", "An unhandled exception was thrown in Update Mananger.", e3) };
			ErrorForm errorForm2 = new ErrorForm();
			errorForm2.Errors = errors;
			errorForm2.ShowDialog();
		}
	}

	private void OnForce(object sender, EventArgs e)
	{
		if (m_events.SelectedItems.Count < 1)
		{
			return;
		}
		try
		{
			List<ITask> list = (from item in m_events.SelectedItems.OfType<ListViewItem>()
				select (ITask)item.Tag into scheduleEvent
				where scheduleEvent != null
				select scheduleEvent).ToList();
			if (list.Count > 0)
			{
				ITaskScheduler scheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
				UpdateManagerApplication.Instance.AsyncButtonAction((Control)sender, delegate
				{
					foreach (ITask item in list)
					{
						ErrorList errorList = scheduler.ForceTask(item.Name, out var _);
						if (errorList.ContainsError())
						{
							UpdateManagerApplication.Instance.ThreadSafeErrorDisplay(errorList);
							break;
						}
					}
				});
			}
			LoadScheduleEvents();
		}
		catch (Exception e2)
		{
			ErrorList errors = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E990", "An unhandled exception was thrown in Update Mananger.", e2) };
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errors;
			errorForm.ShowDialog();
		}
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		LoadScheduleEvents();
	}

	private void SetButtonState()
	{
		bool isRunning = ServiceLocator.Instance.GetService<ITaskScheduler>().IsRunning;
		m_startScheduler.Enabled = !isRunning;
		m_stopScheduler.Enabled = isRunning;
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
		this.m_events = new System.Windows.Forms.ListView();
		this.TaskName = new System.Windows.Forms.ColumnHeader();
		this.payload = new System.Windows.Forms.ColumnHeader();
		this.type = new System.Windows.Forms.ColumnHeader();
		this.cronexpression = new System.Windows.Forms.ColumnHeader();
		this.m_deleteEvent = new System.Windows.Forms.Button();
		this.m_stopScheduler = new System.Windows.Forms.Button();
		this.m_resetScheduler = new System.Windows.Forms.Button();
		this.m_forceEvent = new System.Windows.Forms.Button();
		this.m_startScheduler = new System.Windows.Forms.Button();
		this.m_newScriptEvent = new System.Windows.Forms.Button();
		this.m_newShellEvent = new System.Windows.Forms.Button();
		this.m_refreshButton = new System.Windows.Forms.Button();
		this.m_newServerpollEvent = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_events.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.TaskName, this.payload, this.type, this.cronexpression });
		this.m_events.FullRowSelect = true;
		this.m_events.Location = new System.Drawing.Point(12, 83);
		this.m_events.Name = "m_events";
		this.m_events.ShowGroups = false;
		this.m_events.Size = new System.Drawing.Size(608, 208);
		this.m_events.TabIndex = 6;
		this.m_events.UseCompatibleStateImageBehavior = false;
		this.m_events.View = System.Windows.Forms.View.Details;
		this.TaskName.Text = "Name";
		this.payload.DisplayIndex = 3;
		this.payload.Text = "Payload";
		this.payload.Width = 169;
		this.type.Text = "Type";
		this.type.Width = 78;
		this.cronexpression.DisplayIndex = 1;
		this.cronexpression.Text = "Schedule";
		this.cronexpression.Width = 312;
		this.m_deleteEvent.Location = new System.Drawing.Point(546, 298);
		this.m_deleteEvent.Name = "m_deleteEvent";
		this.m_deleteEvent.Size = new System.Drawing.Size(75, 23);
		this.m_deleteEvent.TabIndex = 9;
		this.m_deleteEvent.Text = "Delete";
		this.m_deleteEvent.UseVisualStyleBackColor = true;
		this.m_deleteEvent.Click += new System.EventHandler(OnDelete);
		this.m_stopScheduler.Enabled = false;
		this.m_stopScheduler.Location = new System.Drawing.Point(94, 12);
		this.m_stopScheduler.Name = "m_stopScheduler";
		this.m_stopScheduler.Size = new System.Drawing.Size(75, 23);
		this.m_stopScheduler.TabIndex = 1;
		this.m_stopScheduler.Text = "Stop";
		this.m_stopScheduler.UseVisualStyleBackColor = true;
		this.m_stopScheduler.Click += new System.EventHandler(OnStopScheduler);
		this.m_resetScheduler.Location = new System.Drawing.Point(13, 12);
		this.m_resetScheduler.Name = "m_resetScheduler";
		this.m_resetScheduler.Size = new System.Drawing.Size(75, 23);
		this.m_resetScheduler.TabIndex = 0;
		this.m_resetScheduler.Text = "Restart";
		this.m_resetScheduler.UseVisualStyleBackColor = true;
		this.m_resetScheduler.Click += new System.EventHandler(OnRestartSchdeuler);
		this.m_forceEvent.Location = new System.Drawing.Point(94, 298);
		this.m_forceEvent.Name = "m_forceEvent";
		this.m_forceEvent.Size = new System.Drawing.Size(75, 23);
		this.m_forceEvent.TabIndex = 8;
		this.m_forceEvent.Text = "Force";
		this.m_forceEvent.UseVisualStyleBackColor = true;
		this.m_forceEvent.Click += new System.EventHandler(OnForce);
		this.m_startScheduler.Enabled = false;
		this.m_startScheduler.Location = new System.Drawing.Point(175, 12);
		this.m_startScheduler.Name = "m_startScheduler";
		this.m_startScheduler.Size = new System.Drawing.Size(75, 23);
		this.m_startScheduler.TabIndex = 2;
		this.m_startScheduler.Text = "Start";
		this.m_startScheduler.UseVisualStyleBackColor = true;
		this.m_startScheduler.Click += new System.EventHandler(OnStartScheduler);
		this.m_newScriptEvent.Location = new System.Drawing.Point(12, 55);
		this.m_newScriptEvent.Name = "m_newScriptEvent";
		this.m_newScriptEvent.Size = new System.Drawing.Size(101, 23);
		this.m_newScriptEvent.TabIndex = 3;
		this.m_newScriptEvent.Text = "New Script Event";
		this.m_newScriptEvent.UseVisualStyleBackColor = true;
		this.m_newScriptEvent.Click += new System.EventHandler(OnNewScriptEvent);
		this.m_newShellEvent.Location = new System.Drawing.Point(119, 55);
		this.m_newShellEvent.Name = "m_newShellEvent";
		this.m_newShellEvent.Size = new System.Drawing.Size(101, 23);
		this.m_newShellEvent.TabIndex = 4;
		this.m_newShellEvent.Text = " New Shell Event";
		this.m_newShellEvent.UseVisualStyleBackColor = true;
		this.m_newShellEvent.Click += new System.EventHandler(OnNewShellEvent);
		this.m_refreshButton.Location = new System.Drawing.Point(12, 298);
		this.m_refreshButton.Name = "m_refreshButton";
		this.m_refreshButton.Size = new System.Drawing.Size(75, 23);
		this.m_refreshButton.TabIndex = 7;
		this.m_refreshButton.Text = "Refresh";
		this.m_refreshButton.UseVisualStyleBackColor = true;
		this.m_refreshButton.Click += new System.EventHandler(OnRefresh);
		this.m_newServerpollEvent.Location = new System.Drawing.Point(226, 55);
		this.m_newServerpollEvent.Name = "m_newServerpollEvent";
		this.m_newServerpollEvent.Size = new System.Drawing.Size(123, 23);
		this.m_newServerpollEvent.TabIndex = 10;
		this.m_newServerpollEvent.Text = " New Serverpoll Event";
		this.m_newServerpollEvent.UseVisualStyleBackColor = true;
		this.m_newServerpollEvent.Click += new System.EventHandler(OnNewServerpollEvent);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(633, 333);
		base.Controls.Add(this.m_newServerpollEvent);
		base.Controls.Add(this.m_refreshButton);
		base.Controls.Add(this.m_newShellEvent);
		base.Controls.Add(this.m_newScriptEvent);
		base.Controls.Add(this.m_startScheduler);
		base.Controls.Add(this.m_forceEvent);
		base.Controls.Add(this.m_resetScheduler);
		base.Controls.Add(this.m_stopScheduler);
		base.Controls.Add(this.m_deleteEvent);
		base.Controls.Add(this.m_events);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "ManageScheduleEvents";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Manage Schedule Events";
		base.Load += new System.EventHandler(OnLoad);
		base.ResumeLayout(false);
	}
}
