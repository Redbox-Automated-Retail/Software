using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.IDE;

public class SecureBrowser : Form
{
	private readonly NavigateUrlCallback m_navigateUrlCallback;

	private readonly IDictionary<string, string> m_approvedUrls;

	private IContainer components;

	private Label label1;

	private ComboBox m_urlComboBox;

	private Panel panel1;

	private WebBrowser m_browser;

	private Button btn_dropdown;

	private Button btn_close;

	private Panel panel2;

	private Label label2;

	private System.Windows.Forms.PictureBox pictureBox1;

	public SecureBrowser(IDictionary<string, string> approvedUrls, NavigateUrlCallback navigateUrlCallback)
	{
		InitializeComponent();
		m_navigateUrlCallback = navigateUrlCallback;
		m_approvedUrls = approvedUrls;
		foreach (KeyValuePair<string, string> approvedUrl in approvedUrls)
		{
			m_urlComboBox.Items.Add(approvedUrl.Key);
		}
	}

	private void OnLoad(object sender, EventArgs e)
	{
		Cursor.Show();
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		Cursor.Hide();
	}

	private void SecureBrowserComboBox_SelectedValueChanged(object sender, EventArgs e)
	{
		NavigateToUrl(m_urlComboBox.Text);
	}

	private void SecureBrowserComboBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			NavigateToUrl(m_urlComboBox.Text);
		}
	}

	private bool IsWebsiteApproved(string currentUrl)
	{
		foreach (KeyValuePair<string, string> approvedUrl in m_approvedUrls)
		{
			if (approvedUrl.Value.StartsWith(currentUrl))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsUrlAddressAllowed(string url)
	{
		return Regex.IsMatch(url, "((192.168.)|(172.16.))\\d{1,3}\\.\\d{1,3}$");
	}

	private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
	{
		if (m_navigateUrlCallback != null)
		{
			label2.Text = "Secure Browser - Loading...";
			m_navigateUrlCallback("Valid URL", e.Url.ToString());
		}
	}

	private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
	{
		Cursor.Current = Cursors.Default;
		label2.Text = "Secure Browser";
		m_browser.Focus();
	}

	private void NavigateToUrl(string urlName)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string text = (m_approvedUrls.ContainsKey(urlName) ? m_approvedUrls[urlName] : string.Empty);
		if (!string.IsNullOrEmpty(text))
		{
			m_browser.Navigate(service.ExpandProperties(text));
			return;
		}
		if (IsUrlAddressAllowed(urlName) || IsWebsiteApproved(urlName))
		{
			m_browser.Navigate(service.ExpandProperties(urlName));
			return;
		}
		m_browser.Navigate(string.Empty);
		m_navigateUrlCallback("Invalid URL", urlName);
	}

	private void btn_dropdown_Click(object sender, EventArgs e)
	{
		m_urlComboBox.DroppedDown = true;
	}

	private void btn_close_Click(object sender, EventArgs e)
	{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.SecureBrowser));
		this.m_urlComboBox = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.panel1 = new System.Windows.Forms.Panel();
		this.btn_dropdown = new System.Windows.Forms.Button();
		this.btn_close = new System.Windows.Forms.Button();
		this.m_browser = new System.Windows.Forms.WebBrowser();
		this.panel2 = new System.Windows.Forms.Panel();
		this.label2 = new System.Windows.Forms.Label();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.panel1.SuspendLayout();
		this.panel2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.m_urlComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_urlComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.m_urlComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 16f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_urlComboBox.Location = new System.Drawing.Point(175, 8);
		this.m_urlComboBox.Name = "m_urlComboBox";
		this.m_urlComboBox.Size = new System.Drawing.Size(823, 33);
		this.m_urlComboBox.TabIndex = 1;
		this.m_urlComboBox.SelectedIndexChanged += new System.EventHandler(SecureBrowserComboBox_SelectedValueChanged);
		this.m_urlComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(SecureBrowserComboBox_KeyPress);
		this.label1.Font = new System.Drawing.Font("Tahoma", 16f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.Black;
		this.label1.Location = new System.Drawing.Point(5, 12);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(163, 24);
		this.label1.TabIndex = 2;
		this.label1.Text = "URL Address:";
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.BackColor = System.Drawing.Color.DimGray;
		this.panel1.Controls.Add(this.btn_dropdown);
		this.panel1.Controls.Add(this.label1);
		this.panel1.Controls.Add(this.m_urlComboBox);
		this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.panel1.ForeColor = System.Drawing.Color.White;
		this.panel1.Location = new System.Drawing.Point(0, 50);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(1024, 50);
		this.panel1.TabIndex = 3;
		this.btn_dropdown.BackColor = System.Drawing.Color.White;
		this.btn_dropdown.BackgroundImage = (System.Drawing.Image)resources.GetObject("btn_dropdown.BackgroundImage");
		this.btn_dropdown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.btn_dropdown.FlatAppearance.BorderColor = System.Drawing.Color.Maroon;
		this.btn_dropdown.FlatAppearance.BorderSize = 0;
		this.btn_dropdown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btn_dropdown.Font = new System.Drawing.Font("Microsoft Sans Serif", 13f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.btn_dropdown.ForeColor = System.Drawing.Color.Black;
		this.btn_dropdown.Location = new System.Drawing.Point(940, 8);
		this.btn_dropdown.Name = "btn_dropdown";
		this.btn_dropdown.Size = new System.Drawing.Size(84, 33);
		this.btn_dropdown.TabIndex = 3;
		this.btn_dropdown.UseVisualStyleBackColor = false;
		this.btn_dropdown.Click += new System.EventHandler(btn_dropdown_Click);
		this.btn_close.BackColor = System.Drawing.Color.LightGray;
		this.btn_close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btn_close.FlatAppearance.BorderSize = 0;
		this.btn_close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btn_close.Font = new System.Drawing.Font("Tahoma", 24f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.btn_close.ForeColor = System.Drawing.Color.White;
		this.btn_close.Location = new System.Drawing.Point(940, 0);
		this.btn_close.Name = "btn_close";
		this.btn_close.Size = new System.Drawing.Size(84, 50);
		this.btn_close.TabIndex = 4;
		this.btn_close.Text = "X";
		this.btn_close.UseVisualStyleBackColor = false;
		this.btn_close.Click += new System.EventHandler(btn_close_Click);
		this.m_browser.Location = new System.Drawing.Point(26, 125);
		this.m_browser.MinimumSize = new System.Drawing.Size(20, 20);
		this.m_browser.Name = "m_browser";
		this.m_browser.Size = new System.Drawing.Size(972, 617);
		this.m_browser.TabIndex = 4;
		this.m_browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(Browser_DocumentCompleted);
		this.m_browser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(Browser_Navigating);
		this.panel2.BackColor = System.Drawing.Color.Maroon;
		this.panel2.Controls.Add(this.label2);
		this.panel2.Controls.Add(this.pictureBox1);
		this.panel2.Controls.Add(this.btn_close);
		this.panel2.Location = new System.Drawing.Point(0, 0);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(1024, 50);
		this.panel2.TabIndex = 5;
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("Tahoma", 24f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.ForeColor = System.Drawing.Color.White;
		this.label2.Location = new System.Drawing.Point(55, 5);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(268, 39);
		this.label2.TabIndex = 1;
		this.label2.Text = "Secure Browser";
		this.pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
		this.pictureBox1.Location = new System.Drawing.Point(0, 0);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(45, 50);
		this.pictureBox1.TabIndex = 0;
		this.pictureBox1.TabStop = false;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		base.CancelButton = this.btn_close;
		base.ClientSize = new System.Drawing.Size(1024, 768);
		base.Controls.Add(this.panel2);
		base.Controls.Add(this.m_browser);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		this.MaximumSize = new System.Drawing.Size(1024, 768);
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(1024, 768);
		base.Name = "SecureBrowser";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Secure Browser";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(OnFormClosing);
		base.Load += new System.EventHandler(OnLoad);
		this.panel1.ResumeLayout(false);
		this.panel2.ResumeLayout(false);
		this.panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
	}
}
