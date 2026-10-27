using System;
using System.Drawing;
using System.Net.Mail;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Kernel.Views.Common;
using Redbox.Rental.Model.Analytics;

namespace Redbox.Rental.Kernel.Views.KeyBoardView;

internal class KeyboardViewUI
{
	private IBitmapCacheService _bitmapCacheService;

	private IRenderingService _rendService;

	private IFontCacheService _fontCacheService;

	private IResourceBundleService _resourceBundleService;

	private IStyleSheetService _styleSheetService;

	private IMacroService _macroService;

	private IViewService _viewService;

	private IActor _okButton;

	private IActor _okArrowButton;

	private Action _next;

	private Action _cancel;

	private Func<string, string> _validate;

	private static string[] line1 = new string[10] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

	private static string[] line2 = new string[10] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" };

	private static string[] line3 = new string[9] { "A", "S", "D", "F", "G", "H", "J", "K", "L" };

	private static string[] line4 = new string[8] { "Z", "X", "C", "V", "B", "N", "M", "-" };

	private static string[] line5 = new string[3] { "_", "@", "." };

	internal KeyboardViewType KeyboardType { get; set; }

	internal bool IsAda { get; set; }

	internal bool IsSpanish { get; set; }

	internal string ErrorMessage { get; set; }

	internal string CancelButtonText { get; set; }

	internal string OkButtonText { get; set; }

	internal KeyboardViewUI(string pageName, bool isAda, bool isSpanish)
	{
		Enum.TryParse<KeyboardViewType>(pageName, ignoreCase: true, out var result);
		KeyboardType = result;
		IsAda = isAda;
		IsSpanish = isSpanish;
		_bitmapCacheService = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		_fontCacheService = ServiceLocator.Instance.GetService<IFontCacheService>();
		_macroService = ServiceLocator.Instance.GetService<IMacroService>();
		_rendService = ServiceLocator.Instance.GetService<IRenderingService>();
		_resourceBundleService = ServiceLocator.Instance.GetService<IResourceBundleService>();
		_styleSheetService = ServiceLocator.Instance.GetService<IStyleSheetService>();
		_viewService = ServiceLocator.Instance.GetService<IViewService>();
	}

	internal void RegisterValidate(Func<string, string> onValidate)
	{
		_validate = onValidate;
	}

	internal void RegisterNext(Action action)
	{
		_next = action;
	}

	internal void RegisterCancel(Action action)
	{
		_cancel = action;
	}

	private void DisplayBackground()
	{
		IActor actor = _rendService.ActiveScene.CreateActor();
		actor.Name = "back_ground_image";
		actor.X = 0;
		actor.Y = 0;
		actor.Width = 1024;
		actor.Height = 768;
		actor.BackgroundColor = Color.White;
		actor.OptionFlags = RenderOptionFlags.Background;
		_rendService.ActiveScene.AddFirst(actor);
		IActor actor2 = _rendService.ActiveScene.CreateActor();
		actor2.X = 0;
		actor2.Y = 654;
		actor2.Width = 1024;
		actor2.Height = 768 - actor2.Y;
		actor2.BackgroundColor = Color.FromArgb(56, 11, 69);
		actor2.OptionFlags = RenderOptionFlags.Background;
		_rendService.ActiveScene.AddLast(actor2);
		IActor actor3 = _rendService.ActiveScene.CreateActor();
		actor3.X = 0;
		actor3.Y = actor2.Y + 3;
		actor3.Width = 1024;
		actor3.Height = 768 - actor3.Y;
		actor3.BackgroundColor = Color.FromArgb(56, 11, 69);
		actor3.OptionFlags = RenderOptionFlags.Background;
		_rendService.ActiveScene.AddLast(actor3);
		if (KeyboardType != KeyboardViewType.Promo)
		{
			IActor actor4 = _rendService.ActiveScene.CreateActor();
			actor4.Name = "marketing_label";
			actor4.X = 30;
			actor4.Y = actor3.Y + (actor3.Height - 100) / 2;
			actor4.Width = 460;
			actor4.Height = 100;
			actor4.ForegroundColor = Color.FromArgb(128, 128, 128);
			actor4.OptionFlags = RenderOptionFlags.Text;
			actor4.Font = _fontCacheService.GetFont("font_akzidenz_regular_small");
			actor4.VerticalAlignment = StringAlignment.Center;
			actor4.Text = "${KeyBoard:MarketingLabel}";
			_rendService.ActiveScene.AddLast(actor4);
		}
		Font font = _fontCacheService.GetFont("font_target_small");
		_okButton = _rendService.ActiveScene.CreateActor().AddRubineButtonStyle();
		_okButton.Name = "ok_button";
		_okButton.X = 803;
		_okButton.Y = 674;
		_okButton.Width = 196;
		_okButton.Height = 66;
		_okButton.CornerSize = new Size
		{
			Width = 33,
			Height = 25
		};
		_okButton.Text = OkButtonText ?? (IsSpanish ? "   CONTINUAR" : "NEXT");
		_okButton.Font = font;
		_okButton.Hit += OnOkButtonPress;
		_okButton.HorizontalAlignment = StringAlignment.Near;
		_rendService.ActiveScene.AddLast(_okButton);
		_okArrowButton = _rendService.ActiveScene.CreateActor();
		_okArrowButton.Name = "image_next_right_arrow";
		_okArrowButton.X = _okButton.X + 132 + (IsSpanish ? 15 : 0);
		_okArrowButton.Y = _okButton.Y + 21;
		_okArrowButton.OptionFlags = RenderOptionFlags.Image;
		_okArrowButton.HitFlags = HitTestFlags.Enabled;
		_okArrowButton.Image = GetImage("image_next_white_arrow_big");
		_okArrowButton.Hit += OnOkButtonPress;
		_rendService.ActiveScene.AddLast(_okArrowButton);
		IActor actor5 = _rendService.ActiveScene.CreateActor().AddDarkGrayButtonStyle();
		actor5.Name = "cancel_button";
		actor5.X = 583;
		actor5.Y = _okButton.Y;
		actor5.Width = _okButton.Width;
		actor5.Height = _okButton.Height;
		actor5.CornerSize = _okButton.CornerSize;
		actor5.Text = CancelButtonText ?? (IsSpanish ? "NO GRACIAS" : "NO THANKS");
		actor5.Font = font;
		actor5.Hit += OnCancelButtonPress;
		_rendService.ActiveScene.AddLast(actor5);
	}

	private void DisplayHeader()
	{
		IActor actor = _rendService.ActiveScene.CreateActor();
		actor.Name = "message_label";
		actor.X = 28;
		actor.Y = 88;
		actor.Width = 808;
		actor.Height = 48;
		actor.OptionFlags = RenderOptionFlags.Text;
		actor.Font = _fontCacheService.GetFont("font_akzidenz_regular_small");
		actor.ForegroundColor = Color.FromArgb(44, 46, 50);
		actor.Text = "${KeyBoard:MessageLabel}";
		_rendService.ActiveScene.AddLast(actor);
		IActor actor2 = _rendService.ActiveScene.CreateActor();
		actor2.Name = "error_label";
		actor2.X = 80;
		actor2.Y = 120;
		actor2.Width = 808;
		actor2.Height = 24;
		actor2.OptionFlags = RenderOptionFlags.Text;
		actor2.Font = _fontCacheService.GetFont("font_akzidenz_bold_small");
		actor2.ForegroundColor = Color.Red;
		actor2.VerticalAlignment = StringAlignment.Center;
		actor2.HorizontalAlignment = StringAlignment.Center;
		actor2.Text = "${KeyBoard:ErrorLabel}";
		_rendService.ActiveScene.AddLast(actor2);
		IActor actor3 = _rendService.ActiveScene.CreateActor();
		actor3.Name = "separator_1_top";
		actor3.X = 10;
		actor3.Y = 75;
		actor3.Width = 1004;
		actor3.Height = 4;
		actor3.OptionFlags = RenderOptionFlags.Background;
		actor3.BackgroundColor = Color.FromArgb(44, 46, 50);
		_rendService.ActiveScene.AddLast(actor3);
		IActor actor4 = _rendService.ActiveScene.CreateActor();
		actor4.Name = "panel_tag";
		actor4.X = 30;
		actor4.Y = 17;
		actor4.OptionFlags = RenderOptionFlags.Text;
		actor4.Font = _fontCacheService.GetFont("font_target_medium");
		actor4.ForegroundColor = Color.FromArgb(44, 46, 50);
		actor4.Text = "${KeyBoard:PanelTag}";
		_rendService.ActiveScene.AddLast(actor4);
	}

	public void ValidateInput()
	{
		string text = _macroService["KeyBoard:Text"];
		string message = _validate?.Invoke(text) ?? string.Empty;
		if (new MailAddress(text).Address != text)
		{
			DisplayErrorMessage(message);
		}
	}

	private void DisplayErrorMessage(string message)
	{
		_macroService["KeyBoard:ErrorLabel"] = message;
		_rendService.ActiveScene.MakeDirty(new Rectangle[1]
		{
			new Rectangle
			{
				X = 0,
				Y = 0,
				Height = 768,
				Width = 1024
			}
		});
	}

	private void DisplayBackClearButtons(IStyleSheetStyle style, int x, int y)
	{
		Font font = _fontCacheService.GetFont("font_akzidenz_bold_smaller");
		int value = (IsAda ? 100 : 136);
		string text = (IsSpanish ? ((IsAda ? " " : string.Empty) + "       ATRÁS") : "         BACK");
		IActor actor = CreateCharacter(font, style, text, x, y, value);
		actor.HorizontalAlignment = StringAlignment.Near;
		actor.Hit -= OnCharacterHit;
		actor.Hit += OnBackSpaceHit;
		actor.Name = "back_button";
		if (!IsAda)
		{
			actor.CornerSize = new Size
			{
				Height = 31,
				Width = 31
			};
			actor.Height = 66;
			actor.HorizontalAlignment = StringAlignment.Center;
			actor.Text = actor.Text.TrimStart();
		}
		IActor actor2 = _rendService.ActiveScene.CreateActor();
		actor2.X = actor.X + ((IsAda && IsSpanish) ? 2 : 10);
		actor2.Y = actor.Y + (IsAda ? 20 : 24);
		actor2.Image = GetImage("image_back_icon");
		actor2.OptionFlags = RenderOptionFlags.Image;
		actor2.HitFlags = HitTestFlags.Enabled;
		actor2.Hit += OnBackSpaceHit;
		_rendService.ActiveScene.AddLast(actor2);
		if (KeyboardType != KeyboardViewType.Promo)
		{
			string text2 = (IsSpanish ? ((IsAda ? " " : string.Empty) + "     BORRAR  ") : "        CLEAR");
			IActor actor3 = CreateCharacter(font, style, text2, x + actor.Width + 14, y, actor.Width);
			actor3.HorizontalAlignment = StringAlignment.Near;
			actor3.Hit -= OnCharacterHit;
			actor3.Hit += OnClearHit;
			actor3.Name = "clear_button";
			if (!IsAda)
			{
				actor3.CornerSize = new Size
				{
					Height = 31,
					Width = 31
				};
				actor3.Height = 66;
				actor3.HorizontalAlignment = StringAlignment.Center;
				actor3.Text = actor3.Text.TrimStart();
			}
			IActor actor4 = _rendService.ActiveScene.CreateActor();
			actor4.X = actor3.X + ((IsAda && IsSpanish) ? 2 : 10);
			actor4.Y = actor3.Y + (IsAda ? 17 : 22);
			actor4.Image = GetImage("image_clear_icon");
			actor4.OptionFlags = RenderOptionFlags.Image;
			actor3.HitFlags = HitTestFlags.Enabled;
			actor4.Hit += OnClearHit;
			_rendService.ActiveScene.AddLast(actor4);
		}
	}

	internal void DisplayEmailLayout()
	{
		Font font = _fontCacheService.GetFont("font_target_small_bold");
		Font font2 = _fontCacheService.GetFont("font_akzidenz_bold_tiny");
		IStyleSheet styleSheet = ServiceLocator.Instance.GetService<IStyleSheetService>().GetStyleSheet("style_sheet_common");
		IStyleSheetStyle style = styleSheet.GetStyle("rubine_button_style");
		_rendService.ActiveScene.SuspendDrawing();
		DisplayBackground();
		DisplayHeader();
		IActor actor = _rendService.ActiveScene.CreateActor();
		actor.Name = "text_label";
		actor.X = 67;
		actor.Y = 147;
		actor.Width = ((KeyboardType == KeyboardViewType.Promo) ? 706 : 615);
		actor.Height = 67;
		actor.Text = "${KeyBoard:Text}";
		actor.ForegroundColor = Color.FromArgb(0, 0, 0);
		actor.VerticalAlignment = StringAlignment.Center;
		actor.HorizontalAlignment = StringAlignment.Center;
		actor.Font = font;
		actor.StrokeWeight = 3f;
		actor.Image = GetImage("image_email_entry_box");
		actor.OptionFlags = RenderOptionFlags.Text | RenderOptionFlags.Image;
		_rendService.ActiveScene.AddLast(actor);
		int startXPos = -3;
		int startYPos = (IsAda ? 290 : 227);
		int offsetX = 70;
		int offsetCounter = 1;
		int offsetXAdditional = ((!IsAda && KeyboardType == KeyboardViewType.Promo) ? 6 : 0);
		line1.ForEach(delegate(string character)
		{
			CreateCharacter(font, style, character, startXPos + (offsetX + offsetXAdditional) * offsetCounter++, startYPos);
		});
		int x = (IsAda ? (startXPos + offsetX * offsetCounter) : (actor.X + actor.Width + 14));
		int y = (IsAda ? startYPos : actor.Y);
		DisplayBackClearButtons(style, x, y);
		if (!IsAda && KeyboardType != KeyboardViewType.Promo)
		{
			CreateCharacter(font2, style, "@YAHOO.COM", startXPos + offsetX * offsetCounter++, startYPos, 150);
		}
		offsetCounter = 1;
		startXPos += 30;
		startYPos += 70;
		line2.ForEach(delegate(string character)
		{
			CreateCharacter(font, style, character, startXPos + offsetX * offsetCounter++, startYPos);
		});
		if (KeyboardType != KeyboardViewType.Promo)
		{
			CreateCharacter(font2, style, "@GMAIL.COM", startXPos + offsetX * offsetCounter++, startYPos, 150);
		}
		offsetCounter = 1;
		startXPos += 30;
		startYPos += 70;
		line3.ForEach(delegate(string character)
		{
			CreateCharacter(font, style, character, startXPos + offsetX * offsetCounter++, startYPos);
		});
		if (KeyboardType != KeyboardViewType.Promo)
		{
			CreateCharacter(font2, style, "@HOTMAIL.COM", startXPos + offsetX * offsetCounter++, startYPos, 160);
		}
		offsetCounter = 1;
		startXPos += 30;
		startYPos += 70;
		line4.ForEach(delegate(string character)
		{
			CreateCharacter(font, style, character, startXPos + offsetX * offsetCounter++, startYPos);
		});
		if (KeyboardType != KeyboardViewType.Promo)
		{
			CreateCharacter(font2, style, IsAda ? "@YAHOO.COM" : "@AOL.COM", startXPos + offsetX * offsetCounter++, startYPos, 170);
		}
		offsetCounter = 1;
		startXPos += 30;
		startYPos += 70;
		int value = ((KeyboardType == KeyboardViewType.Promo) ? 446 : 315);
		IActor actor2 = CreateCharacter(font, style, "SPACE", startXPos + offsetX, startYPos, value);
		actor2.Tag = " ";
		if (KeyboardType != KeyboardViewType.Promo)
		{
			startXPos += actor2.Width + 14;
			line5.ForEach(delegate(string character)
			{
				CreateCharacter(font, style, character, startXPos + offsetX * offsetCounter++, startYPos);
			});
			CreateCharacter(font2, style, ".COM", startXPos + offsetX * offsetCounter, startYPos, 126).Name = "dotcom_button";
		}
		string text = _macroService["KeyBoard:Text"];
		DisableOkButton();
		if (text.Length >= 4)
		{
			EnableOkButton();
		}
		_rendService.ActiveScene.ResumeDrawing();
	}

	private IActor CreateCharacter(Font font, IStyleSheetStyle style, string text, int x, int y, int? width = null)
	{
		IActor actor = _rendService.ActiveScene.CreateActor().AddRubineButtonStyle();
		actor.Name = text.Replace(" ", "_") + "_button";
		actor.X = x;
		actor.Y = y;
		actor.Width = width ?? 56;
		actor.Height = 56;
		actor.Tag = text;
		actor.Text = text;
		actor.Font = font;
		actor.Style = style;
		actor.StyleName = style.Name;
		actor.CornerSize = new Size
		{
			Height = 28,
			Width = 28
		};
		actor.Hit += OnCharacterHit;
		actor.MarkDirty();
		_rendService.ActiveScene.AddLast(actor);
		return actor;
	}

	private void OnCharacterHit(object sender, EventArgs e)
	{
		string text = _macroService["KeyBoard:Text"];
		_rendService.ActiveScene.SuspendDrawing();
		if (text.Length < 50)
		{
			_macroService["KeyBoard:Text"] = text + ((IActor)sender).Tag;
			DisplayErrorMessage(string.Empty);
			if (text.Length + ((IActor)sender).Tag.ToString().Length >= 4)
			{
				EnableOkButton();
			}
		}
		_rendService.ActiveScene.ResumeDrawing();
	}

	private void OnClearHit(object sender, EventArgs e)
	{
		_ = _macroService["KeyBoard:Text"];
		_rendService.ActiveScene.SuspendDrawing();
		_macroService["KeyBoard:Text"] = string.Empty;
		DisplayErrorMessage(string.Empty);
		_rendService.ActiveScene.ResumeDrawing();
	}

	private void OnBackSpaceHit(object sender, EventArgs e)
	{
		string text = _macroService["KeyBoard:Text"];
		_rendService.ActiveScene.SuspendDrawing();
		DisplayErrorMessage(string.Empty);
		if (text != null && text.Length > 0)
		{
			_macroService["KeyBoard:Text"] = text.Substring(0, text.Length - 1);
		}
		if (text.Length - 1 < 4)
		{
			DisableOkButton();
		}
		_rendService.ActiveScene.ResumeDrawing();
	}

	private void OnOkButtonPress(object sender, EventArgs e)
	{
		string arg = _macroService["KeyBoard:Text"];
		string text = _validate(arg);
		if (!string.IsNullOrEmpty(text))
		{
			DisplayErrorMessage(text);
			LogHelper.Instance.Log("Keyboard - Failed Validation.");
			return;
		}
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (service != null && service.GetCurrentSession() != null)
		{
			ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("KeyboardOk");
		}
		_next?.Invoke();
	}

	private void OnCancelButtonPress(object sender, EventArgs e)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (service != null && service.GetCurrentSession() != null)
		{
			ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("KeyboardCancel");
		}
		if (_cancel != null)
		{
			_cancel();
			return;
		}
		IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
		if (service2.Stack.Count > 1)
		{
			service2.Pop();
			service2.Show();
		}
	}

	private void EnableOkButton()
	{
		_okButton.Enabled = true;
		if (IsSpanish)
		{
			_okButton.HorizontalAlignment = StringAlignment.Near;
		}
		_okArrowButton.Enabled = true;
		_okArrowButton.Image = GetImage("image_next_white_arrow_big");
	}

	private void DisableOkButton()
	{
		_okButton.Enabled = false;
		if (IsSpanish)
		{
			_okButton.HorizontalAlignment = StringAlignment.Near;
		}
		_okArrowButton.Enabled = false;
		_okArrowButton.Image = GetImage("image_next_white_arrow_big");
	}

	private Image GetImage(string name)
	{
		Image image = _bitmapCacheService.GetImage(name);
		if (image == null)
		{
			image = _resourceBundleService.GetBitmap(name);
		}
		return image;
	}
}
