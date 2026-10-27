using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Management.Console;

public class ImmediateWindow : UserControl
{
	private static ImmediateWindow m_instance;

	private IContainer components;

	private ShellControl m_shellControl;

	public new bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			base.Enabled = value;
			m_shellControl.Enabled = value;
		}
	}

	public static ImmediateWindow Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new ImmediateWindow();
			}
			return m_instance;
		}
	}

	public ImmediateWindow()
	{
		InitializeComponent();
		Dock = DockStyle.Fill;
		ImmediateCommand.Service = ProfileManager.Instance.Service;
		Enabled = true;
		ProfileManager.Instance.Connected += delegate
		{
			ImmediateCommand.Service = ProfileManager.Instance.Service;
		};
		ProfileManager.Instance.Disconnected += delegate
		{
			m_shellControl.Clear();
		};
	}

	private void OnClear(object sender, EventArgs e)
	{
		m_shellControl.Clear();
	}

	private void OnCommandEntered(object sender, CommandEnteredEventArgs e)
	{
		ImmediateCommand command = ImmediateCommand.GetCommand(e.Command);
		if (command == null)
		{
			return;
		}
		if (ImmediateCommand.Service == null)
		{
			OutputWindow.Instance.Append("Immediate window unable to contact service.");
			LogHelper.Instance.Log("Immediate window unable to contact service.", LogEntryType.Error);
			return;
		}
		ImmediateCommandResult immediateCommandResult = command.Execute();
		if (immediateCommandResult.CommandResult != null)
		{
			CommonFunctions.ProcessCommandResult(immediateCommandResult.CommandResult);
		}
	}

	private void OnSelect(object sender, EventArgs e)
	{
		m_shellControl.Select();
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
		this.m_shellControl = new Redbox.HAL.Management.Console.ShellControl();
		base.SuspendLayout();
		this.m_shellControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_shellControl.Enabled = false;
		this.m_shellControl.Font = new System.Drawing.Font("ProFontWindows", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_shellControl.Location = new System.Drawing.Point(0, 0);
		this.m_shellControl.Name = "m_shellControl";
		this.m_shellControl.Prompt = "> ";
		this.m_shellControl.ShellTextBackColor = System.Drawing.SystemColors.Window;
		this.m_shellControl.ShellTextFont = new System.Drawing.Font("ProFontWindows", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_shellControl.ShellTextForeColor = System.Drawing.SystemColors.WindowText;
		this.m_shellControl.Size = new System.Drawing.Size(465, 289);
		this.m_shellControl.TabIndex = 0;
		this.m_shellControl.CommandEntered += new Redbox.HAL.Management.Console.EventCommandEntered(OnCommandEntered);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_shellControl);
		base.Name = "ImmediateWindow";
		base.Size = new System.Drawing.Size(465, 289);
		base.ResumeLayout(false);
	}
}
