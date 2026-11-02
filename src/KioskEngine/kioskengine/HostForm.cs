using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.IDE;

namespace Redbox.KioskEngine.Bootstrap;

public class HostForm : Form
{
	private struct COPYDATASTRUCT
	{
		public IntPtr dwData;

		public int cbData;

		[MarshalAs(UnmanagedType.LPStr)]
		public string lpData;
	}

	private const int WM_USER = 1024;

	private const int WM_COPYDATA = 74;

	private bool m_isCursorHidden;

	private DateTime _keyPressStartAfter = DateTime.MinValue;

	private IClassicTrackDataService? _classicTrackDataService;

	private bool windowSizeInitialized;

	private IContainer? components;

	private IClassicTrackDataService ClassicTrackDataService
	{
		get
		{
			if (_classicTrackDataService == null)
			{
				_classicTrackDataService = ServiceLocator.Instance.GetService<IClassicTrackDataService>();
			}
			return _classicTrackDataService;
		}
	}

	public HostForm()
	{
		InitializeComponent();
		ServiceLocator.Instance.GetService<IViewService>().UITaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
		IMachineSettingsStore machineSettingsStore = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		if (!Debugger.IsAttached && !machineSettingsStore.GetValue("Core", "EnableMouseCursor", defaultValue: true))
		{
			Cursor.Hide();
			m_isCursorHidden = true;
		}
		base.ShowInTaskbar = machineSettingsStore.GetValue("Ide", "ShowKioskEngineInTaskBar", defaultValue: false);
		machineSettingsStore.GetValue<string?>("Store", "Environment", null);
		machineSettingsStore.GetValue("Ide", "ShowChrome", defaultValue: false);
		if (Debugger.IsAttached)
		{
			base.FormBorderStyle = FormBorderStyle.Sizable;
			base.ControlBox = true;
			MaximumSize = System.Drawing.Size.Empty;
		}
		EngineApplication.DisableProcessWindowsGhosting();
	}

	protected override void WndProc(ref Message m)
	{
		EnvironmentNotificationType environmentNotificationType = EnvironmentNotificationType.None;
		IEnvironmentNotificationService environmentNotificationService = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		if (environmentNotificationService != null)
		{
			environmentNotificationType = environmentNotificationService.ProcessClassicCardReaderNotification(ref m);
		}
		if (environmentNotificationType == EnvironmentNotificationType.CreditCardReaderInvalid && environmentNotificationService != null)
		{
			environmentNotificationService.RaiseInvalidClassicCardReader();
		}
		else if (environmentNotificationType == EnvironmentNotificationType.CreditCardReaderValid && environmentNotificationService != null)
		{
			environmentNotificationService.RaiseValidClassicCardReader();
		}
		else
		{
			switch (m.Msg)
			{
			case 74:
				CommandHelper.ProcessCommandMessage(((COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT))!).lpData);
				break;
			}
		}
		base.WndProc(ref m);
	}

	private void OnPaint(object? sender, PaintEventArgs e)
	{
		IRenderingService renderingService = ServiceLocator.Instance.GetService<IRenderingService>();
		if (renderingService.ActiveScene != null)
		{
			renderingService.ActiveScene.MakeDirty(null);
		}
	}

	private string EncodePassword(string originalPassword)
	{
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
#if NETFRAMEWORK
		byte[] bytes = Encoding.Default.GetBytes(originalPassword);
#else
		// Encoding.Default is UTF-8 on .NET 10; the stored hash was made with the ANSI code page.
		byte[] bytes = NetFrameworkCompatibility.AnsiCodePage.GetBytes(originalPassword);
#endif
		return BitConverter.ToString(mD5CryptoServiceProvider.ComputeHash(bytes));
	}

	public void OnKeyDown(object? sender, KeyEventArgs e)
	{
		IResourceBundleService bundleService = ServiceLocator.Instance.GetService<IResourceBundleService>();
		switch (e.KeyCode)
		{
		case Keys.Home:
		{
			ITextToSpeechService textToSpeechService = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (textToSpeechService != null)
			{
				textToSpeechService.AudioDeviceConnected = true;
				textToSpeechService.RunSpeechWorkflow($"tts_{ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame()?.ViewFrame?.ViewName}");
			}
			break;
		}
		case Keys.End:
		{
			ITextToSpeechService textToSpeechService = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (textToSpeechService != null)
			{
				textToSpeechService.AudioDeviceConnected = false;
			}
			break;
		}
		case Keys.F1:
			ScreenshotHelper.Capture(this);
			break;
		case Keys.F2:
			if (bundleService != null && bundleService.Filter["environment"] != "Production")
			{
				new ManageViewsForm().ShowDialog();
			}
			break;
		case Keys.F4:
		{
			PreferencesManager preferencesManager = new PreferencesManager();
			if (bundleService == null)
			{
				break;
			}
			XmlNode xml = bundleService.GetXml("config");
			if (xml == null)
			{
				break;
			}
			XmlNodeList? xmlNodeList = xml.SelectNodes("row");
			if (xmlNodeList == null)
			{
				break;
			}
			string? passwordHash = null;
			foreach (XmlNode row in xmlNodeList)
			{
				XmlNodeList? columns = row.SelectNodes("column");
				if (columns == null)
				{
					continue;
				}
				foreach (XmlNode column in columns)
				{
					if (column.GetAttributeValue<string>("name") == "F4PasswordHash")
					{
						passwordHash = column.GetNodeValue(string.Empty);
						break;
					}
				}
			}
			if (passwordHash == null)
			{
				break;
			}
			UnlockForm unlockForm = new UnlockForm();
			if (unlockForm.ShowDialog() == DialogResult.OK)
			{
				SHA256Managed sHA256Managed = new SHA256Managed();
				string enteredHash = Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(unlockForm.Password))));
				sHA256Managed.Clear();
				if (enteredHash == passwordHash)
				{
					preferencesManager.ShowDialog();
				}
				else
				{
					System.Windows.Forms.MessageBox.Show("The password provided is invalid.", "Redbox Engine for Distributed Stores", MessageBoxButtons.OK);
				}
			}
			break;
		}
		case Keys.F5:
			if (bundleService != null && bundleService.Filter["environment"] != "Production")
			{
				ServiceLocator.Instance.GetService<IIdeService>()?.Show();
			}
			break;
		case Keys.F6:
			if (bundleService != null && bundleService.Filter["environment"] != "Production")
			{
				new ManageTimersForm().ShowDialog();
			}
			break;
		case Keys.F8:
			ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.ToggleTouchScreen();
			break;
		case Keys.F10:
		{
			IRenderingService renderingService = ServiceLocator.Instance.GetService<IRenderingService>();
			string name = "ADALine";
			if (renderingService.ActiveScene!.SceneRenderType == RenderType.WPF)
			{
				System.Windows.Controls.UserControl? userControl = (renderingService.ActiveScene.Actors.First?.Value)?.WPFFrameworkElement as System.Windows.Controls.UserControl;
				Grid? grid = userControl?.Content as Grid;
				IEnumerator enumerator = grid!.Children.GetEnumerator();
				bool lineFound = false;
				while (enumerator.MoveNext())
				{
					if (enumerator.Current is Line line)
					{
						line.Visibility = ((line.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible);
						userControl!.InvalidateVisual();
						lineFound = true;
						break;
					}
				}
				if (!lineFound)
				{
					Line element = new Line
					{
						X1 = 0.0,
						Y1 = 286.0,
						X2 = 1024.0,
						Y2 = 286.0,
						Stroke = new SolidColorBrush
						{
							Color = Colors.GreenYellow
						},
						StrokeThickness = 3.0,
						Stretch = Stretch.None,
						Name = name,
						Visibility = Visibility.Visible
					};
					grid.Children.Add(element);
				}
			}
			else
			{
				renderingService.ActiveScene.SuspendDrawing();
				IActor? actor = renderingService.ActiveScene.GetActor(name);
				if (actor != null)
				{
					renderingService.ActiveScene.Remove(actor);
				}
				else
				{
					IActor lineActor = renderingService.ActiveScene.CreateActor();
					lineActor.Name = name;
					lineActor.Y = 286;
					lineActor.X = 0;
					lineActor.Width = 1024;
					lineActor.Height = 3;
					lineActor.BackgroundColor = System.Drawing.Color.GreenYellow;
					lineActor.Visible = true;
					lineActor.HitFlags = HitTestFlags.None;
					lineActor.ForegroundColor = System.Drawing.Color.GreenYellow;
					lineActor.OptionFlags = RenderOptionFlags.Background;
					renderingService.ActiveScene.AddLast(lineActor);
				}
				renderingService.ActiveScene.ResumeDrawing();
			}
			break;
		}
		case Keys.F11:
			if (m_isCursorHidden)
			{
				Cursor.Show();
			}
			else
			{
				Cursor.Hide();
			}
			m_isCursorHidden = !m_isCursorHidden;
			break;
		case Keys.F12:
		{
			IViewService viewService = ServiceLocator.Instance.GetService<IViewService>();
			if (viewService.Peek() == "product_detail_view")
			{
				viewService.Push("title_details_dev_view", false);
				viewService.Show();
			}
			break;
		}
		}
	}

	public void OnKeyPress(object? sender, KeyPressEventArgs e)
	{
		IResourceBundleService resourceBundleService = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (e.KeyChar == '\u001b')
		{
			Close();
			e.Handled = true;
			return;
		}
		IClassicTrackDataService classicTrackDataService = ClassicTrackDataService;
		if (classicTrackDataService != null && classicTrackDataService.AllowTrackDataParsing)
		{
			e.Handled = ClassicTrackDataService.ProcessKey(e.KeyChar, delegate
			{
				_keyPressStartAfter = DateTime.Now.AddSeconds(3.0);
			});
		}
		if (e.Handled)
		{
			return;
		}
		if (e.KeyChar == '`')
		{
			ResourceBundleService.Instance.GetManifest(out var manifestInfo);
			string? text = ResourceBundleService.Instance.Filter["view_name"];
			if (!(manifestInfo.ProductName != "Rental Application"))
			{
				if (!(manifestInfo.ProductName == "Rental Application"))
				{
					goto IL_00ea;
				}
				switch (text)
				{
				case "start_view":
				case "welcome_view":
				case "maintenance_view":
					break;
				default:
					goto IL_00ea;
				}
			}
			EngineApplication.Instance.SelectBundle();
			goto IL_00ea;
		}
		IClassicTrackDataService classicTrackDataService2 = ClassicTrackDataService;
		if (classicTrackDataService2 != null && classicTrackDataService2.AllowTrackDataParsing && resourceBundleService != null && resourceBundleService.Filter["environment"] != "Production")
		{
			switch (e.KeyChar)
			{
			case '~':
				SendKeys.Send("{%}B4111111111111111{^}JOE/COOL                {^}1608041103263?;4111111111111111=1608041103263?");
				break;
			case '!':
				SendKeys.Send("{%}B24931070601{^}JOE/COOL                {^}1608041103263?;249931070601=1608041103263?");
				break;
			case '@':
				SendKeys.Send("{%}B6010562000017685{^}REDBOX                {^}00010004000060013779?;6010562000017685=00010004000060013779?");
				break;
			case '#':
				SendKeys.Send("{%}B4111111111111111{^}JOE/COOL                {^}1608041103263?;4111111111=1608041103263?");
				break;
			}
		}
		else if (DeviceServiceClientHelperSimulator.Instance.UseSimulatedCardReader)
		{
			e.Handled = DeviceServiceClientHelperSimulator.Instance.ProcessKey(e.KeyChar);
		}
		goto IL_019d;
		IL_00ea:
		e.Handled = true;
		goto IL_019d;
		IL_019d:
		if (DateTime.Now > _keyPressStartAfter)
		{
			EngineApplication.Instance.ActivateKeyPressHandlers(new KeyPressEventArgsEx(e, System.Windows.Forms.Control.ModifierKeys));
		}
	}

	private void OnActivated(object? sender, EventArgs e)
	{
		IRenderingService renderingService = ServiceLocator.Instance.GetService<IRenderingService>();
		if (renderingService.ActiveScene != null)
		{
			renderingService.ActiveScene.MakeDirty(null);
			EngineApplication.Instance.ActivateActivatedHandlers();
			Focus();
			BringToFront();
		}
	}

	private void OnMouseClick(object? sender, MouseEventArgs e)
	{
		IRenderingService renderingService = ServiceLocator.Instance.GetService<IRenderingService>();
		if (renderingService.ActiveScene != null)
		{
			renderingService.ActiveScene.HitTest(e.X, e.Y);
			EngineApplication.Instance.ActivateMouseClickHandlers(e);
		}
	}

	private void OnMouseDoubleClick(object? sender, MouseEventArgs e)
	{
		EngineApplication.Instance.ActivateMouseDoubleClickHandlers(e);
	}

	private void HostForm_Shown(object? sender, EventArgs e)
	{
		if (!Debugger.IsAttached || windowSizeInitialized)
		{
			return;
		}
		windowSizeInitialized = true;
		string? value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<string>("Core", "DebugWindowSize");
		if (value != null)
		{
			string[] size = value.Split(',');
			if (size.Length == 2 && int.TryParse(size[0], out var width) && int.TryParse(size[1], out var height) && width != 0 && height != 0)
			{
				base.Width = width;
				base.Height = height;
			}
		}
	}

	private void HostForm_FormClosing(object? sender, FormClosingEventArgs e)
	{
		if (Debugger.IsAttached)
		{
			IMachineSettingsStore machineSettingsStore = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			string value = $"{base.Width},{base.Height}";
			machineSettingsStore.SetValue("Core", "DebugWindowSize", value);
		}
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.Bootstrap.HostForm));
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(9f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(153, 153, 152);
		base.ClientSize = new System.Drawing.Size(1024, 768);
		base.ControlBox = false;
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon")!;
		base.MaximizeBox = false;
		this.MaximumSize = new System.Drawing.Size(1024, 768);
		base.MinimizeBox = false;
		base.Name = "HostForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Redbox Kiosk Engine";
		base.Activated += new System.EventHandler(OnActivated);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(HostForm_FormClosing);
		base.Shown += new System.EventHandler(HostForm_Shown);
		base.Paint += new System.Windows.Forms.PaintEventHandler(OnPaint);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(OnKeyDown);
		base.KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPress);
		base.MouseClick += new System.Windows.Forms.MouseEventHandler(OnMouseClick);
		base.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(OnMouseDoubleClick);
		base.ResumeLayout(false);
	}
}
