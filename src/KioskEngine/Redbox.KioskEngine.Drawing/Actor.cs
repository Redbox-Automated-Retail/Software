using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Drawing;

public class Actor : IActor, IDisposable
{
	private Form _mainForm;

	private bool m_clickStarted;

	private double WPFSizeConvertion = 1.3333333730697632;

	private int m_x;

	private int m_y;

	private Font m_font;

	private int m_width;

	private int m_height;

	private string m_text;

	private string m_html;

	private System.Drawing.Image m_image;

	private bool m_enabled;

	private bool m_visible;

	private bool? m_locked;

	private float m_opacity;

	private int m_frameNumber;

	private string m_errorText;

	private System.Drawing.Size? m_cornerSize;

	private System.Drawing.Color? m_borderColor;

	private float m_strokeWeight;

	private float? m_gradientAngle;

	private System.Drawing.Color m_foregroundColor;

	private System.Drawing.Color m_backgroundColor;

	private IStyleSheetStyle m_style;

	private float? m_cornerSweepAngle;

	private static bool m_drawErrorTop;

	private float? m_textRotationAngle;

	private System.Windows.Forms.WebBrowser m_browserControl;

	private System.Drawing.Color? m_gradientTargetColor;

	private System.Drawing.Point? m_textTranslationPoint;

	private RenderOptionFlags m_optionFlags;

	private FrameDimension m_frameDimension;

	private StringAlignment m_verticalAlignment;

	private StringAlignment m_horizontalAlignment;

	private FrameworkElement m_wpfFrameworkElement;

	private TextBlock m_wpfTextBlock;

	private System.Windows.Controls.Image m_wpfImage;

	public bool HitExists => Hit != null;

	public IntPtr MainHandle
	{
		get
		{
			if (_mainForm == null)
			{
				IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
				_mainForm = (service as ApplicationContext).MainForm;
			}
			return _mainForm.Handle;
		}
	}

	public int X
	{
		get
		{
			return m_x;
		}
		set
		{
			if (m_x != value)
			{
				Rectangle rectangle = new Rectangle(m_x, Y, Width, Height);
				m_x = value;
				NotifyScene(new Rectangle[2]
				{
					rectangle,
					new Rectangle(m_x, Y, Width, Height)
				});
				if (m_wpfFrameworkElement != null)
				{
					m_wpfFrameworkElement.Margin = Margin;
				}
			}
		}
	}

	public int Y
	{
		get
		{
			return m_y;
		}
		set
		{
			if (m_y != value)
			{
				Rectangle rectangle = new Rectangle(X, m_y, Width, Height);
				m_y = value;
				NotifyScene(new Rectangle[2]
				{
					rectangle,
					new Rectangle(X, m_y, Width, Height)
				});
				if (m_wpfFrameworkElement != null)
				{
					m_wpfFrameworkElement.Margin = Margin;
				}
			}
		}
	}

	public Font Font
	{
		get
		{
			if (m_font == null)
			{
				IFontCacheService service = ServiceLocator.Instance.GetService<IFontCacheService>();
				if (service != null)
				{
					m_font = service.RegisterFont("DefaultActorFont", "Arial", 16f, System.Drawing.FontStyle.Regular);
				}
			}
			return m_font;
		}
		set
		{
			SetFieldValue(ref m_font, value);
		}
	}

	public int Width
	{
		get
		{
			return m_width;
		}
		set
		{
			SetFieldValue(ref m_width, value);
			if (m_wpfFrameworkElement != null)
			{
				m_wpfFrameworkElement.Margin = Margin;
			}
		}
	}

	public int Height
	{
		get
		{
			return m_height;
		}
		set
		{
			SetFieldValue(ref m_height, value);
			if (m_wpfFrameworkElement != null)
			{
				m_wpfFrameworkElement.Margin = Margin;
			}
		}
	}

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			SetFieldValue(ref m_text, value);
			UpdateWPFText();
		}
	}

	public string Html
	{
		get
		{
			return m_html;
		}
		set
		{
			SetFieldValue(ref m_html, value);
			if (m_html != null && m_html != string.Empty)
			{
				m_browserControl = new System.Windows.Forms.WebBrowser();
				m_browserControl.ScrollBarsEnabled = false;
				m_browserControl.DocumentCompleted += BrowserDocumentCompleted;
				m_browserControl.Height = Height;
				m_browserControl.Width = Width;
				m_browserControl.DocumentText = m_html;
				MarkDirty();
			}
		}
	}

	public string Name { get; set; }

	public IScene Scene { get; set; }

	public bool IsDirty { get; internal set; }

	public float Opacity
	{
		get
		{
			return m_opacity;
		}
		set
		{
			if (SetFieldValue(ref m_opacity, value, notifyScene: false, processStyle: false))
			{
				if (m_opacity < 0f)
				{
					m_opacity = 0f;
				}
				if (m_opacity >= 1f)
				{
					m_opacity = 1f;
				}
				NotifyScene(new Rectangle[1]
				{
					new Rectangle(X, Y, Width, Height)
				});
			}
		}
	}

	public int FrameCount { get; internal set; }

	public int? TabOrder { get; set; }

	public int FrameNumber
	{
		get
		{
			return m_frameNumber;
		}
		set
		{
			if (SetFieldValue(ref m_frameNumber, value, notifyScene: false, processStyle: false))
			{
				if (m_frameNumber >= FrameCount)
				{
					m_frameNumber = 0;
				}
				if (m_frameNumber < 0)
				{
					m_frameNumber = 0;
				}
				NotifyScene(new Rectangle[1]
				{
					new Rectangle(X, Y, Width, Height)
				});
			}
		}
	}

	public System.Drawing.Image Image
	{
		get
		{
			return m_image;
		}
		set
		{
			if (!SetFieldValue(ref m_image, value, notifyScene: false, processStyle: false))
			{
				return;
			}
			if (m_image != null)
			{
				if (Width == 0)
				{
					Width = m_image.Width;
				}
				if (Height == 0)
				{
					Height = m_image.Height;
				}
				m_frameDimension = new FrameDimension(m_image.FrameDimensionsList[0]);
				FrameCount = m_image.GetFrameCount(m_frameDimension);
			}
			NotifyScene(new Rectangle[1]
			{
				new Rectangle(X, Y, Width, Height)
			});
			AssignWPFImage(m_image);
		}
	}

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			SetFieldValue(ref m_visible, value);
			if (m_wpfFrameworkElement != null)
			{
				m_wpfFrameworkElement.Visibility = ((!value) ? Visibility.Hidden : Visibility.Visible);
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			SetFieldValue(ref m_enabled, value, notifyScene: true, processStyle: true);
		}
	}

	public bool? Locked
	{
		get
		{
			return m_locked;
		}
		set
		{
			if (!m_locked.HasValue)
			{
				m_locked = value;
				return;
			}
			throw new ArgumentException("Locked can only be set once.");
		}
	}

	public object Tag { get; set; }

	public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

	public string ErrorText
	{
		get
		{
			return m_errorText;
		}
		set
		{
			SetFieldValue(ref m_errorText, value);
		}
	}

	public float[] TabStops { get; set; }

	public float StrokeWeight
	{
		get
		{
			return m_strokeWeight;
		}
		set
		{
			SetFieldValue(ref m_strokeWeight, value);
		}
	}

	public string StyleName { get; set; }

	public Rectangle? HotSpot { get; set; }

	public Rectangle? TextRegion { get; set; }

	public HitTestFlags HitFlags { get; set; }

	public System.Drawing.Size? CornerSize
	{
		get
		{
			return m_cornerSize;
		}
		set
		{
			SetFieldValue(ref m_cornerSize, value);
		}
	}

	public System.Drawing.Color? BorderColor
	{
		get
		{
			return m_borderColor;
		}
		set
		{
			SetFieldValue(ref m_borderColor, value);
		}
	}

	public System.Drawing.Color ForegroundColor
	{
		get
		{
			return m_foregroundColor;
		}
		set
		{
			SetFieldValue(ref m_foregroundColor, value);
		}
	}

	public System.Drawing.Color BackgroundColor
	{
		get
		{
			return m_backgroundColor;
		}
		set
		{
			SetFieldValue(ref m_backgroundColor, value);
		}
	}

	public float? GradientAngle
	{
		get
		{
			return m_gradientAngle;
		}
		set
		{
			SetFieldValue(ref m_gradientAngle, value);
		}
	}

	public float? CornerSweepAngle
	{
		get
		{
			return m_cornerSweepAngle;
		}
		set
		{
			SetFieldValue(ref m_cornerSweepAngle, value);
		}
	}

	public float? TextRotationAngle
	{
		get
		{
			return m_textRotationAngle;
		}
		set
		{
			SetFieldValue(ref m_textRotationAngle, value);
		}
	}

	public System.Drawing.Color? GradientTargetColor
	{
		get
		{
			return m_gradientTargetColor;
		}
		set
		{
			SetFieldValue(ref m_gradientTargetColor, value);
		}
	}

	public string RelativeToActorName { get; set; }

	public System.Drawing.Point? TextTranslationPoint
	{
		get
		{
			return m_textTranslationPoint;
		}
		set
		{
			SetFieldValue(ref m_textTranslationPoint, value);
		}
	}

	public RenderStateFlags StateFlags { get; set; }

	public RenderOptionFlags OptionFlags
	{
		get
		{
			return m_optionFlags;
		}
		set
		{
			SetFieldValue(ref m_optionFlags, value);
		}
	}

	public StringAlignment VerticalAlignment
	{
		get
		{
			return m_verticalAlignment;
		}
		set
		{
			SetFieldValue(ref m_verticalAlignment, value);
		}
	}

	public StringAlignment HorizontalAlignment
	{
		get
		{
			return m_horizontalAlignment;
		}
		set
		{
			SetFieldValue(ref m_horizontalAlignment, value);
		}
	}

	public IStyleSheetStyle Style
	{
		get
		{
			return m_style;
		}
		set
		{
			SetFieldValue(ref m_style, value);
		}
	}

	public FrameworkElement WPFFrameworkElement
	{
		get
		{
			if (m_wpfFrameworkElement == null)
			{
				m_wpfFrameworkElement = CreateUIElement();
			}
			return m_wpfFrameworkElement;
		}
		set
		{
			m_wpfFrameworkElement = value;
		}
	}

	public string WPFControlName { get; set; }

	public string WPFControlAssemblyName { get; set; }

	private System.Windows.HorizontalAlignment WPFHorizontalAlignmnet => HorizontalAlignment switch
	{
		StringAlignment.Near => System.Windows.HorizontalAlignment.Left, 
		StringAlignment.Center => System.Windows.HorizontalAlignment.Center, 
		StringAlignment.Far => System.Windows.HorizontalAlignment.Right, 
		_ => System.Windows.HorizontalAlignment.Left, 
	};

	private VerticalAlignment WPFVerticalAlignmnet => VerticalAlignment switch
	{
		StringAlignment.Near => System.Windows.VerticalAlignment.Top, 
		StringAlignment.Center => System.Windows.VerticalAlignment.Center, 
		StringAlignment.Far => System.Windows.VerticalAlignment.Bottom, 
		_ => System.Windows.VerticalAlignment.Top, 
	};

	private Thickness Margin
	{
		get
		{
			int num = ((Width != 0) ? (Scene.Width - Width - X) : 0);
			int num2 = ((Height != 0) ? (Scene.Height - Height - Y) : 0);
			return new Thickness(X, Y, num, num2);
		}
	}

	public event WPFHitHandler OnWPFHit;

	public event EventHandler Hit;

	public Actor()
	{
		Opacity = 1f;
		StrokeWeight = 1f;
		Enabled = true;
		Visible = true;
		IsDirty = true;
		BackgroundColor = System.Drawing.Color.Black;
		ForegroundColor = System.Drawing.Color.White;
		OptionFlags = RenderOptionFlags.Text | RenderOptionFlags.Image;
	}

	public void RaiseHit()
	{
		if (Hit != null)
		{
			Hit(this, EventArgs.Empty);
		}
	}

	public void ClearHit()
	{
		Hit = null;
	}

	public void MarkDirty()
	{
		NotifyScene(new Rectangle[1]
		{
			new Rectangle(X, Y, Width, Height)
		});
	}

	public virtual void Dispose()
	{
		ClearHit();
	}

	public MeasureTextResult MeasureText()
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		using Graphics graphics = Graphics.FromHwnd(MainHandle);
		graphics.PixelOffsetMode = PixelOffsetMode.Half;
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		Rectangle textRegion = GetTextRegion(forTranslation: false);
		StringFormat stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
		int charactersFitted;
		int linesFilled;
		return new MeasureTextResult(graphics.MeasureString(service.ExpandProperties(Text), Font, new SizeF(textRegion.Width, textRegion.Height), stringFormat, out charactersFitted, out linesFilled), charactersFitted, linesFilled);
	}

	public void CenterHorizontally()
	{
		if (Scene != null)
		{
			X = (Scene.Width - Width) / 2;
		}
	}

	public void CenterVertically()
	{
		if (Scene != null)
		{
			Y = (Scene.Height - Height) / 2;
		}
	}

	public void Render(Graphics context)
	{
		OnRender(context);
	}

	protected FrameworkElement CreateUIElement()
	{
		FrameworkElement frameworkElement = null;
		frameworkElement = (string.IsNullOrEmpty(WPFControlName) ? CreateWPFActor() : CreateWPFControl());
		frameworkElement.Tag = Name;
		frameworkElement.Margin = Margin;
		frameworkElement.MouseDown += FrameworkElement_MouseDown;
		frameworkElement.MouseLeave += FrameworkElement_MouseLeave;
		frameworkElement.MouseUp += FrameworkElement_MouseUp;
		frameworkElement.IsHitTestVisible = WPFControlName != null || (HitFlags & (HitTestFlags.Enabled | HitTestFlags.DrawHotSpot)) != 0;
		return frameworkElement;
	}

	private FrameworkElement CreateWPFActor()
	{
		Grid grid = new Grid();
		if ((OptionFlags & RenderOptionFlags.Background) != RenderOptionFlags.None || (OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
		{
			Border border = new Border
			{
				Opacity = Opacity
			};
			System.Windows.Media.Color mediaColor = GetMediaColor(BackgroundColor, Opacity);
			if ((OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
			{
				System.Drawing.Color color = (GradientTargetColor.HasValue ? GradientTargetColor.Value : BackgroundColor);
				System.Windows.Media.Color mediaColor2 = GetMediaColor(color, Opacity);
				border.Background = new System.Windows.Media.LinearGradientBrush(mediaColor, mediaColor2, GradientAngle ?? 180f);
			}
			else
			{
				border.Background = new SolidColorBrush(mediaColor);
			}
			if (CornerSize.HasValue)
			{
				border.CornerRadius = new CornerRadius(CornerSize.Value.Width);
			}
			grid.Children.Add(border);
		}
		if ((OptionFlags & RenderOptionFlags.Image) != RenderOptionFlags.None)
		{
			m_wpfImage = new System.Windows.Controls.Image
			{
				Opacity = Opacity
			};
			AssignWPFImage(Image);
			m_wpfImage.Stretch = Stretch.Fill;
			grid.Children.Add(m_wpfImage);
		}
		if ((OptionFlags & RenderOptionFlags.DrawBorder) != RenderOptionFlags.None)
		{
			Border border2 = new Border();
			border2.BorderThickness = new Thickness(StrokeWeight);
			border2.BorderBrush = new SolidColorBrush(GetMediaColor(BorderColor ?? System.Drawing.Color.Red));
			if (CornerSize.HasValue)
			{
				border2.CornerRadius = new CornerRadius(CornerSize.Value.Width);
			}
			grid.Children.Add(border2);
		}
		if ((OptionFlags & RenderOptionFlags.Text) != RenderOptionFlags.None)
		{
			m_wpfTextBlock = new TextBlock
			{
				Opacity = Opacity
			};
			System.Windows.Media.FontFamily fontFamily = null;
			if (fontFamily == null)
			{
				fontFamily = new System.Windows.Media.FontFamily(Font.FontFamily.Name);
				LogHelper.Instance.Log("Unable to find Font Family {0}.  Using default font instead.", Font.FontFamily.Name);
			}
			m_wpfTextBlock.FontFamily = fontFamily;
			m_wpfTextBlock.FontSize = (double)Font.Size * WPFSizeConvertion;
			if (Font.Style == System.Drawing.FontStyle.Bold)
			{
				m_wpfTextBlock.FontWeight = FontWeights.Bold;
			}
			else if (Font.Style == System.Drawing.FontStyle.Regular)
			{
				m_wpfTextBlock.FontWeight = FontWeights.Regular;
			}
			else
			{
				m_wpfTextBlock.FontWeight = FontWeights.Normal;
			}
			if (Font.Style == System.Drawing.FontStyle.Italic)
			{
				m_wpfTextBlock.FontStyle = FontStyles.Italic;
			}
			else
			{
				m_wpfTextBlock.FontStyle = FontStyles.Normal;
			}
			m_wpfTextBlock.HorizontalAlignment = WPFHorizontalAlignmnet;
			m_wpfTextBlock.VerticalAlignment = WPFVerticalAlignmnet;
			System.Windows.Media.Color mediaColor3 = GetMediaColor(ForegroundColor);
			if (mediaColor3.A == 0 && mediaColor3.R == 0 && mediaColor3.B == 0 && mediaColor3.G == 0)
			{
				mediaColor3.A = byte.MaxValue;
			}
			m_wpfTextBlock.Foreground = new SolidColorBrush(mediaColor3);
			UpdateWPFText();
			string text = fontFamily.FamilyNames[XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name)];
			LogHelper.Instance.Log("Font for Actor {0}: Family: {1}, Size: {2}, Text: {3}", Name, text, m_wpfTextBlock.FontSize, m_wpfTextBlock.Text);
			grid.Children.Add(m_wpfTextBlock);
		}
		return grid;
	}

	private FrameworkElement CreateWPFControl()
	{
		FrameworkElement frameworkElement = ServiceLocator.Instance.GetService<IThemeService>()?.GetNewThemedControlInstance(WPFControlName);
		if (frameworkElement == null)
		{
			if (!string.IsNullOrEmpty(WPFControlName) && !string.IsNullOrEmpty(WPFControlAssemblyName))
			{
				string text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), WPFControlAssemblyName);
				if (File.Exists(text))
				{
					Assembly assembly = Assembly.LoadFile(text);
					if (assembly != null)
					{
						Type type = assembly.GetType(WPFControlName);
						if (type != null)
						{
							object obj = Activator.CreateInstance(type);
							if (obj != null)
							{
								if (obj is System.Windows.Controls.UserControl userControl)
								{
									frameworkElement = userControl;
								}
								else
								{
									LogHelper.Instance.Log("Type {0} is not a UserControl", WPFControlName);
								}
							}
							else
							{
								LogHelper.Instance.Log("Unable to create instance of type {0}", WPFControlName);
							}
						}
						else
						{
							LogHelper.Instance.Log("Unable to get type {1} in assembly {0}", text, WPFControlName);
						}
					}
					else
					{
						LogHelper.Instance.Log("Unable to load assembly {0}", text);
					}
				}
				else
				{
					LogHelper.Instance.Log("Unable to find assembly {0}", text);
				}
			}
			else
			{
				LogHelper.Instance.Log("WPFControlName and WPFControlAssemblyName must have values");
			}
		}
		if (frameworkElement is IWPFActor iWPFActor)
		{
			iWPFActor.Actor = this;
			iWPFActor.OnWPFHit += delegate
			{
				OnWPFHit?.Invoke(this);
			};
		}
		return frameworkElement;
	}

	private void AssignWPFImage(System.Drawing.Image image)
	{
		if (m_wpfImage != null && image != null)
		{
			m_wpfImage.Source = ToBitmapImage(new Bitmap(Image));
		}
	}

	private System.Windows.Media.Color GetMediaColor(System.Drawing.Color color)
	{
		return new System.Windows.Media.Color
		{
			R = color.R,
			G = color.G,
			B = color.B,
			A = color.A
		};
	}

	private System.Windows.Media.Color GetMediaColor(System.Drawing.Color color, double opacity)
	{
		int num = (int)Math.Ceiling(255.0 * opacity);
		return new System.Windows.Media.Color
		{
			R = color.R,
			G = color.G,
			B = color.B,
			A = (byte)num
		};
	}

	private void UpdateWPFText()
	{
		if (m_wpfTextBlock != null)
		{
			string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(Text);
			m_wpfTextBlock.Text = text;
		}
	}

	private void FrameworkElement_MouseUp(object sender, MouseButtonEventArgs e)
	{
		if (!m_clickStarted)
		{
			return;
		}
		if (Enabled)
		{
			RaiseHit();
			if (OnWPFHit != null)
			{
				OnWPFHit(this);
			}
		}
		m_clickStarted = false;
	}

	private void FrameworkElement_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		m_clickStarted = false;
	}

	private void FrameworkElement_MouseDown(object sender, MouseButtonEventArgs e)
	{
		m_clickStarted = true;
	}

	private double ToWPFScale(double value)
	{
		return value * WPFSizeConvertion;
	}

	private BitmapImage ToBitmapImage(Bitmap bitmap)
	{
		using MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Png);
		memoryStream.Position = 0L;
		BitmapImage bitmapImage = new BitmapImage();
		bitmapImage.BeginInit();
		bitmapImage.StreamSource = memoryStream;
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		bitmapImage.EndInit();
		return bitmapImage;
	}

	protected void NotifyScene(Rectangle[] rectangles)
	{
		if (Scene != null)
		{
			IsDirty = Visible;
			Scene.MakeDirty(rectangles);
		}
	}

	protected virtual void OnRender(Graphics context)
	{
		if (!Visible)
		{
			IsDirty = false;
			return;
		}
		context.ResetTransform();
		if ((OptionFlags & RenderOptionFlags.DisableAntiAlias) == 0)
		{
			context.SmoothingMode = SmoothingMode.AntiAlias;
		}
		bool hasValue = TextRotationAngle.HasValue;
		bool hasValue2 = TextTranslationPoint.HasValue;
		if ((OptionFlags & RenderOptionFlags.Background) != RenderOptionFlags.None || (OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
		{
			System.Drawing.Color alphaRampedColor = GetAlphaRampedColor(BackgroundColor, Opacity);
			if (CornerSize.HasValue)
			{
				using System.Drawing.Pen p = new System.Drawing.Pen(alphaRampedColor, StrokeWeight);
				DrawRoundedRect(context, p, X, Y, Width, Height, CornerSize.Value.Width, fill: true);
			}
			else
			{
				System.Drawing.Brush brush = null;
				try
				{
					if ((OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
					{
						System.Drawing.Color alphaRampedColor2 = GetAlphaRampedColor(GradientTargetColor ?? BackgroundColor, Opacity);
						brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(X, Y, Width, Height), alphaRampedColor, alphaRampedColor2, GradientAngle ?? 180f);
					}
					else
					{
						brush = new SolidBrush(alphaRampedColor);
					}
					context.FillRectangle(brush, X, Y, Width, Height);
				}
				finally
				{
					brush?.Dispose();
				}
			}
		}
		if ((OptionFlags & RenderOptionFlags.Image) != RenderOptionFlags.None && Image != null)
		{
			ImageAttributes imageAttributes = new ImageAttributes();
			if (FrameCount > 1)
			{
				Image.SelectActiveFrame(m_frameDimension, FrameNumber);
			}
			else
			{
				imageAttributes.SetColorMatrix(new ColorMatrix
				{
					Matrix33 = Opacity
				});
			}
			if ((OptionFlags & RenderOptionFlags.Tile) != RenderOptionFlags.None)
			{
				imageAttributes.SetWrapMode(WrapMode.Tile);
			}
			context.DrawImage(Image, new Rectangle(X, Y, Width, Height), 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, imageAttributes);
		}
		if ((OptionFlags & RenderOptionFlags.DrawBorder) != RenderOptionFlags.None)
		{
			using System.Drawing.Pen pen = new System.Drawing.Pen(BorderColor ?? System.Drawing.Color.Red, StrokeWeight);
			if (CornerSize.HasValue)
			{
				DrawRoundedRect(context, pen, X, Y, Width, Height, CornerSize.Value.Width, fill: false);
			}
			else
			{
				context.DrawRectangle(pen, new Rectangle(X, Y, Width, Height));
			}
		}
		if ((OptionFlags & RenderOptionFlags.Text) != RenderOptionFlags.None && !string.IsNullOrEmpty(Text))
		{
			using SolidBrush brush2 = new SolidBrush(GetAlphaRampedColor(ForegroundColor, Opacity));
			StringFormat stringFormat;
			if ((OptionFlags & RenderOptionFlags.ElideText) != RenderOptionFlags.None)
			{
				stringFormat = (StringFormat)StringFormat.GenericDefault.Clone();
				stringFormat.Trimming |= StringTrimming.EllipsisCharacter;
			}
			else
			{
				stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
			}
			stringFormat.Alignment = HorizontalAlignment;
			stringFormat.LineAlignment = VerticalAlignment;
			if ((OptionFlags & RenderOptionFlags.DisableAntiAlias) == 0)
			{
				context.PixelOffsetMode = PixelOffsetMode.Half;
				context.TextRenderingHint = TextRenderingHint.AntiAlias;
			}
			if (TabStops != null && TabStops.Length != 0)
			{
				stringFormat.SetTabStops(0f, TabStops);
			}
			Rectangle textRegion = GetTextRegion(hasValue2);
			string s = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(Text);
			if (hasValue2)
			{
				context.TranslateTransform(TextTranslationPoint.Value.X, TextTranslationPoint.Value.Y);
			}
			if (hasValue)
			{
				context.RotateTransform(TextRotationAngle.Value);
			}
			if ((OptionFlags & RenderOptionFlags.TextShadow) != RenderOptionFlags.None)
			{
				using SolidBrush brush3 = new SolidBrush(GetAlphaRampedColor(System.Drawing.Color.Black, Opacity));
				context.DrawString(s, Font, brush3, new Rectangle(textRegion.Left + 1, textRegion.Top + 1, textRegion.Width, textRegion.Height), stringFormat);
			}
			context.DrawString(s, Font, brush2, textRegion, stringFormat);
			if (hasValue2 | hasValue)
			{
				context.ResetTransform();
			}
		}
		if ((OptionFlags & RenderOptionFlags.HtmlToImage) != RenderOptionFlags.None && Image != null)
		{
			ImageAttributes imageAttributes2 = new ImageAttributes();
			imageAttributes2.SetColorMatrix(new ColorMatrix
			{
				Matrix33 = Opacity
			});
			context.DrawImage(Image, new Rectangle(X, Y, Width, Height), 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, imageAttributes2);
		}
		if ((HitFlags & HitTestFlags.DrawHotSpot) != HitTestFlags.None)
		{
			context.DrawRectangle(Pens.HotPink, GetHitTestRectangle());
		}
		if (!string.IsNullOrEmpty(ErrorText))
		{
			using (Font font = new Font("Tahoma", 7f, System.Drawing.FontStyle.Bold))
			{
				int num = Y + Height + 2;
				if (m_drawErrorTop)
				{
					num = Y - font.Height - 2;
				}
				context.DrawString(ErrorText, font, System.Drawing.Brushes.Red, X, num);
			}
			m_drawErrorTop = !m_drawErrorTop;
		}
		StateFlags |= RenderStateFlags.Drawn;
		IsDirty = false;
	}

	internal Rectangle GetTextRegion(bool forTranslation)
	{
		int num = ((!forTranslation) ? X : 0);
		int num2 = ((!forTranslation) ? Y : 0);
		int width = Width;
		int height = Height;
		if (TextRegion.HasValue)
		{
			if (!forTranslation)
			{
				num += TextRegion.Value.Left;
				num2 += TextRegion.Value.Top;
			}
			width = TextRegion.Value.Width;
			height = TextRegion.Value.Height;
		}
		return new Rectangle(num, num2, width, height);
	}

	internal Rectangle GetHitTestRectangle()
	{
		int num = X;
		int num2 = Y;
		int width = Width;
		int height = Height;
		if (HotSpot.HasValue)
		{
			num += HotSpot.Value.Left;
			num2 += HotSpot.Value.Top;
			width = HotSpot.Value.Width;
			height = HotSpot.Value.Height;
		}
		return new Rectangle(num, num2, width, height);
	}

	private void BrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
	{
		Bitmap bitmap = new Bitmap(Width, Height);
		m_browserControl.DrawToBitmap(bitmap, new Rectangle(0, 0, Width, Height));
		Image = bitmap;
	}

	private bool SetFieldValue<T>(ref T field, T value)
	{
		return SetFieldValue(ref field, value, notifyScene: true, processStyle: false);
	}

	private bool SetFieldValue<T>(ref T field, T value, bool notifyScene, bool processStyle)
	{
		if ((field == null && value != null) || (field != null && !field.Equals(value)))
		{
			field = value;
			if (processStyle && Style != null)
			{
				IStyleSheetState state = Style.GetState(Enabled ? "enabled" : "disabled");
				if (state != null)
				{
					PropertyInfo[] properties = typeof(Actor).GetProperties();
					foreach (PropertyInfo propertyInfo in properties)
					{
						string name = propertyInfo.Name;
						name = name.Substring(0, 1).ToLower() + name.Substring(1);
						object obj = state[name];
						if (obj == null)
						{
							continue;
						}
						try
						{
							if (name != "image")
							{
								object value2 = ConversionHelper.ChangeType(obj, propertyInfo.PropertyType);
								propertyInfo.SetValue(this, value2, null);
								continue;
							}
							string text = obj as string;
							if (text != null && text.StartsWith("cache:"))
							{
								IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
								Image = service.GetImage(text.Substring(6));
							}
							else if (text != null)
							{
								IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
								Image = service2.GetBitmap(text);
							}
						}
						catch (Exception e)
						{
							LogHelper.Instance.Log($"Unable to set actor property '{propertyInfo.Name}' with value '{obj}'.", e);
						}
					}
				}
			}
			if (notifyScene && (Width > 0 || Height > 0))
			{
				NotifyScene(new Rectangle[1]
				{
					new Rectangle(X, Y, Width, Height)
				});
			}
			return true;
		}
		return false;
	}

	private static System.Drawing.Color GetAlphaRampedColor(System.Drawing.Color color, float opacity)
	{
		return System.Drawing.Color.FromArgb((int)Math.Ceiling(255f * opacity), color);
	}

	private void DrawRoundedRect(Graphics g, System.Drawing.Pen p, float x, float y, float width, float height, float radius, bool fill)
	{
		using GraphicsPath graphicsPath = new GraphicsPath();
		if (radius * 2f > height || radius * 2f > width)
		{
			radius = ((height > width) ? ((int)(width / 2f)) : ((int)(height / 2f)));
		}
		float num = radius * 2f;
		float sweepAngle = CornerSweepAngle ?? 90f;
		graphicsPath.AddArc(x + width - num, y, num, num, 270f, sweepAngle);
		if (y + height - radius < y + radius)
		{
			graphicsPath.AddLine(x + width, y + radius, x + width, y + height - radius);
		}
		graphicsPath.AddArc(x + width - num, y + height - num, num, num, 0f, sweepAngle);
		if (x + width - radius < x + radius)
		{
			graphicsPath.AddLine(x + width - radius, y + height, x + radius, y + height);
		}
		graphicsPath.AddArc(x, y + height - num, num, num, 90f, sweepAngle);
		if (y + height - radius < y + radius)
		{
			graphicsPath.AddLine(x, y + height - radius, x, y + radius);
		}
		graphicsPath.AddArc(x, y, num, num, 180f, sweepAngle);
		if (x + width - radius > x + radius)
		{
			graphicsPath.AddLine(x + radius, y, x + width - radius, y);
		}
		graphicsPath.CloseFigure();
		if (fill)
		{
			System.Drawing.Brush brush = null;
			try
			{
				if ((OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
				{
					System.Drawing.Color alphaRampedColor = GetAlphaRampedColor(GradientTargetColor ?? BackgroundColor, Opacity);
					brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle((int)x, (int)y, (int)width, (int)height), p.Color, alphaRampedColor, GradientAngle ?? 180f);
				}
				else
				{
					brush = new SolidBrush(p.Color);
				}
				g.FillPath(brush, graphicsPath);
				return;
			}
			finally
			{
				brush?.Dispose();
			}
		}
		g.DrawPath(p, graphicsPath);
	}
}
