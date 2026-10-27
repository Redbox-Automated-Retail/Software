using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using Redbox.Lua;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class ImmediateToolWindow : ToolWindow
{
	private IContainer components;

	private ToolStrip m_toolStrip;

	private ToolStripButton m_executeScriptToolStripButton;

	private ShellControl m_shellControl;

	public ImmediateToolWindow()
	{
		InitializeComponent();
	}

	private void OnCommandEntered(object sender, CommandEnteredEventArgs e)
	{
		try
		{
			ReadOnlyCollection<object> readOnlyCollection = ((LuaDebugger)ServiceLocator.Instance.GetService<IDebugService>().DebuggerInstance).Lua.DoString(e.Command);
			if (readOnlyCollection == null || readOnlyCollection.Count == 0)
			{
				m_shellControl.WriteText("nil");
				return;
			}
			foreach (object item in readOnlyCollection)
			{
				m_shellControl.WriteText(LuaHelper.FormatLuaValue(item));
			}
		}
		catch (Exception ex)
		{
			m_shellControl.WriteText(ex.Message);
		}
	}

	private void OnExecuteScript(object sender, EventArgs e)
	{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.ImmediateToolWindow));
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_executeScriptToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_shellControl = new Redbox.KioskEngine.IDE.ShellControl();
		this.m_toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.m_executeScriptToolStripButton });
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(567, 23);
		this.m_toolStrip.TabIndex = 0;
		this.m_executeScriptToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_executeScriptToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Execute;
		this.m_executeScriptToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_executeScriptToolStripButton.Name = "m_executeScriptToolStripButton";
		this.m_executeScriptToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_executeScriptToolStripButton.Click += new System.EventHandler(OnExecuteScript);
		this.m_shellControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_shellControl.Location = new System.Drawing.Point(0, 23);
		this.m_shellControl.Name = "m_shellControl";
		this.m_shellControl.Prompt = "> ";
		this.m_shellControl.ShellTextBackColor = System.Drawing.SystemColors.Window;
		this.m_shellControl.ShellTextFont = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_shellControl.ShellTextForeColor = System.Drawing.SystemColors.WindowText;
		this.m_shellControl.Size = new System.Drawing.Size(567, 235);
		this.m_shellControl.TabIndex = 1;
		this.m_shellControl.CommandEntered += new Redbox.KioskEngine.IDE.EventCommandEntered(OnCommandEntered);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(567, 258);
		base.Controls.Add(this.m_shellControl);
		base.Controls.Add(this.m_toolStrip);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.HideOnClose = true;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "ImmediateToolWindow";
		base.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
		this.Text = "Immediate";
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
