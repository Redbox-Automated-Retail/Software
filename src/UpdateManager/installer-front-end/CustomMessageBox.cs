using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd;

public sealed class CustomMessageBox : Form
{
	private const int Timeout = 30;

	private int _count = 30;

	private readonly Timer _timer = new Timer();

	private readonly Timer _timer2 = new Timer();

	private IContainer? components;

	private Label MessageLabel = null!;

	private Button OKButton = null!;

	public static DialogResult Show(string title, string message, Font font)
	{
		using CustomMessageBox customMessageBox = new CustomMessageBox(title, message, font);
		return customMessageBox.ShowDialog();
	}

	// The controls above are created by InitializeComponent.
	private CustomMessageBox(string title, string message, Font font)
	{
		Font = font;
		ForeColor = SystemColors.WindowText;
		InitializeComponent();
		Text = title;
		MessageLabel.Text = message;
		OKButton.Font = new Font(FontFamily.GenericSansSerif, 8.25f, FontStyle.Regular);
		OKButton.Text = $"OK ({30})";
	}

	private void TimedClose(object? sender, EventArgs e)
	{
		_timer.Interval = 30000;
		_timer2.Interval = 1000;
		_timer.Tick += OkButtonClick;
		_timer2.Tick += UpdateOkButton;
		_timer.Start();
		_timer2.Start();
	}

	private void UpdateOkButton(object? sender, EventArgs e)
	{
		OKButton.Text = $"OK ({--_count})";
	}

	private void OkButtonClick(object? sender, EventArgs e)
	{
		_timer.Stop();
		_timer2.Stop();
		Close();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.UpdateManager.StoreInstallerFrontEnd.CustomMessageBox));
		this.MessageLabel = new System.Windows.Forms.Label();
		this.OKButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.MessageLabel.AutoSize = true;
		this.MessageLabel.Location = new System.Drawing.Point(12, 9);
		this.MessageLabel.Name = "MessageLabel";
		this.MessageLabel.Size = new System.Drawing.Size(35, 13);
		this.MessageLabel.TabIndex = 0;
		this.MessageLabel.Text = "label1";
		this.OKButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.OKButton.Location = new System.Drawing.Point(29, 61);
		this.OKButton.Name = "OKButton";
		this.OKButton.Size = new System.Drawing.Size(75, 23);
		this.OKButton.TabIndex = 1;
		this.OKButton.Text = "OK";
		this.OKButton.UseVisualStyleBackColor = true;
		this.OKButton.Click += new System.EventHandler(OkButtonClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoSize = true;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.ClientSize = new System.Drawing.Size(116, 96);
		base.ControlBox = false;
		base.Controls.Add(this.OKButton);
		base.Controls.Add(this.MessageLabel);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon")!;
		base.Name = "CustomMessageBox";
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "CustomMessageBox";
		base.TopMost = true;
		base.Activated += new System.EventHandler(TimedClose);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
