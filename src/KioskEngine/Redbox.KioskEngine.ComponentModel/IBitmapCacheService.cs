using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Redbox.KioskEngine.ComponentModel;

public interface IBitmapCacheService : IDisposable
{
	Image GetImage(string name);

	Image RegisterImage(string name, byte[] data);

	BitmapImage RegisterBitmapImage(string name, byte[] data);

	Image RegisterImage(string name, string path);

	Image RegisterThumbnailImage(string name, byte[] data, int width, int height);

	Image RegisterThumbnailImage(string name, string newName, int width, int height);

	BitmapImage GetBitmapImage(string imageName);

	BitmapImage ToBitmapImage(Image image);

	void DropCache();

	int TrimCache(int maximumItems);
}
