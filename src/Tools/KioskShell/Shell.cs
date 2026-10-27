using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Outerwall.Shell;
using Outerwall.Shell.HotKeys;
using Redbox.Core;
using Redbox.Shell.HotKeys;
using Redbox.Shell.Properties;

namespace Redbox.Shell;

public class Shell : Form
{
	private readonly LowLevelMouseHook _mouseHook;

	private readonly HotKeyManager _hotKeyManager;

	private readonly IHotKeyCommand _attentionSequenceCommand;

	private const int WS_EX_TRANSPARENT = 32;

	private const int WS_EX_TOOLWINDOW = 128;

	private IContainer components;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams obj = base.CreateParams;
			obj.ExStyle |= 32;
			obj.ExStyle |= 128;
			return obj;
		}
	}

	public Shell()
	{
		InitializeComponent();
		_mouseHook = new LowLevelMouseHook();
		_hotKeyManager = new HotKeyManager();
		try
		{
			_attentionSequenceCommand = new AttentionSequenceCommand();
			HotKey hotKey = new HotKey("Redbox Desktop", Settings.Default.AttentionSequene, _attentionSequenceCommand.Execute);
			_hotKeyManager.AddHotKey(hotKey);
			_hotKeyManager.Start();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"The redbox desktop hot key \"{Settings.Default.AttentionSequene}\" could not be registered.", e);
			_hotKeyManager.Dispose();
		}
	}

	protected override void WndProc(ref Message m)
	{
		if ((long)m.Msg == 32770)
		{
			LogHelper.Instance.Log("Installing mouse hook");
			_mouseHook.SetHook();
		}
		else if ((long)m.Msg == 32771)
		{
			LogHelper.Instance.Log("Removing mouse hook");
			_mouseHook.UnHook();
		}
		else if ((long)m.Msg == 32772)
		{
			IntPtr intPtr = (_mouseHook.IsHooked ? new IntPtr(1) : IntPtr.Zero);
			LogHelper.Instance.Log("Mouse hook status = {0}", intPtr);
			m.Result = intPtr;
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		if (File.Exists(Settings.Default.Wallpaper))
		{
			BackgroundImage = Image.FromFile(Settings.Default.Wallpaper);
			BackgroundImageLayout = ImageLayout.Center;
		}
	}

	private void Shell_Load(object sender, EventArgs e)
	{
		try
		{
			SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("Error trimming memory usauge", e2);
		}
	}

	[DllImport("kernel32.dll")]
	private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

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
		base.SuspendLayout();
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.BackColor = System.Drawing.Color.FromArgb(0, 0, 0);
		base.TransparencyKey = System.Drawing.Color.FromArgb(0, 0, 0);
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		base.ControlBox = false;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Location = new System.Drawing.Point(0, 0);
		base.Name = "Shell";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		base.Load += new System.EventHandler(Shell_Load);
		base.ResumeLayout(false);
	}
}
