using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class FontCacheService : IFontCacheService, IDisposable
{
	private IDictionary<string, Font> m_fonts;

	private PrivateFontCollection m_privateFontCollection;

	public static FontCacheService Instance => Singleton<FontCacheService>.Instance;

	internal IDictionary<string, Font> Fonts
	{
		get
		{
			if (m_fonts == null)
			{
				m_fonts = new Dictionary<string, Font>();
			}
			return m_fonts;
		}
	}

	internal PrivateFontCollection PrivateFonts
	{
		get
		{
			if (m_privateFontCollection == null)
			{
				m_privateFontCollection = new PrivateFontCollection();
			}
			return m_privateFontCollection;
		}
	}

	public Font GetFont(string name)
	{
		if (!Fonts.ContainsKey(name))
		{
			return null;
		}
		return Fonts[name];
	}

	public Font RegisterFont(string name, string familyName, float size, FontStyle style)
	{
		Font font = GetFont(name) ?? new Font(familyName, size, style);
		Fonts[name] = font;
		return font;
	}

	public Font RegisterFont(string name, string familyName, float size, FontStyle style, byte[] data)
	{
		Font font = GetFont(name);
		if (font != null)
		{
			return font;
		}
		FontFamily fontFamily = null;
		StringComparison ccIgnoreCase = StringComparison.CurrentCultureIgnoreCase;
		if (PrivateFonts.Families.Length != 0)
		{
			fontFamily = PrivateFonts.Families.ToList().Find((FontFamily f) => f.Name.Equals(familyName, ccIgnoreCase));
		}
		if (fontFamily == null)
		{
			IntPtr intPtr = Marshal.AllocCoTaskMem(data.Length);
			AddFontMemResourceEx(data, data.Length, IntPtr.Zero, out var _);
			Marshal.Copy(data, 0, intPtr, data.Length);
			PrivateFonts.AddMemoryFont(intPtr, data.Length);
			Marshal.FreeCoTaskMem(intPtr);
			fontFamily = PrivateFonts.Families.ToList().Find((FontFamily f) => f.Name.Equals(familyName, ccIgnoreCase));
			if (fontFamily == null && familyName == "Target Alt Regular")
			{
				fontFamily = PrivateFonts.Families.ToList().Find((FontFamily f) => f.Name.Equals("Target Alt", ccIgnoreCase));
			}
			if (fontFamily == null && familyName == "VAG Rounded Std Light")
			{
				fontFamily = PrivateFonts.Families.ToList().Find((FontFamily f) => f.Name.Equals("VAG Rounded Std", ccIgnoreCase));
			}
		}
		if (fontFamily == null)
		{
			fontFamily = PrivateFonts.Families.FirstOrDefault();
		}
		font = new Font(fontFamily, size, style);
		Fonts[name] = font;
		return font;
	}

	public ErrorList Initialize()
	{
		LogHelper.Instance.Log("Initialize font cache service.");
		ServiceLocator.Instance.AddService(typeof(IFontCacheService), Instance);
		return new ErrorList();
	}

	public void DropCache()
	{
		LogHelper.Instance.Log("Drop font cache.");
		foreach (Font value in Fonts.Values)
		{
			try
			{
				LogHelper.Instance.Log("...Dipose font '{0}'.", value.Name);
				value.Dispose();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in FontCacheService.DropCache.", e);
			}
		}
		Fonts.Clear();
		if (m_privateFontCollection != null)
		{
			m_privateFontCollection.Dispose();
		}
		m_privateFontCollection = null;
	}

	public void Dispose()
	{
		DropCache();
	}

	private FontCacheService()
	{
	}

	[DllImport("gdi32.dll", ExactSpelling = true)]
	private static extern IntPtr AddFontMemResourceEx(byte[] pbFont, int cbFont, IntPtr pdv, out uint pcFonts);

	[DllImport("gdi32.dll")]
	private static extern bool RemoveFontMemResourceEx(IntPtr fh);
}
