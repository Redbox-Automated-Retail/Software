using System;
using System.Drawing;

namespace Redbox.KioskEngine.ComponentModel;

public interface IFontCacheService : IDisposable
{
	Font GetFont(string name);

	Font RegisterFont(string name, string familyName, float size, FontStyle style);

	Font RegisterFont(string name, string familyName, float size, FontStyle style, byte[] data);

	void DropCache();
}
