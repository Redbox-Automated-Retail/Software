using System;

namespace Redbox.KioskEngine.ComponentModel;

[Flags]
public enum RenderOptionFlags
{
	None = 0,
	Background = 1,
	Text = 2,
	Image = 4,
	TextShadow = 8,
	DisableAntiAlias = 0x10,
	Tile = 0x20,
	ElideText = 0x40,
	DrawBorder = 0x80,
	DrawRoundedCorners = 0x100,
	BackgroundGradient = 0x200,
	HtmlToImage = 0x400
}
