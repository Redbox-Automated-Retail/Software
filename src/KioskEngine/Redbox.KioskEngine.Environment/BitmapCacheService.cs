using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class BitmapCacheService : IBitmapCacheService, IDisposable
{
	internal sealed class ImageUsage
	{
		public string Name { get; set; }

		public int Uses { get; set; }
	}

	internal sealed class ImageTracker : IDisposable
	{
		public Stream UnderlyingStream { get; set; }

		public Image Instance { get; set; }

		public void Dispose()
		{
			try
			{
				if (Instance != null)
				{
					Instance.Dispose();
				}
				if (UnderlyingStream != null)
				{
					UnderlyingStream.Dispose();
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in ImageTracker.Dispose.", e);
			}
		}
	}

	private IDictionary<string, ImageTracker> m_images;

	private IDictionary<string, BitmapImage> _bitmapImages = new Dictionary<string, BitmapImage>();

	private readonly SortedDictionary<string, int> m_usageCounts = new SortedDictionary<string, int>();

	private Dictionary<string, int> _bitmapImageUsageCount = new Dictionary<string, int>();

	public static BitmapCacheService Instance => Singleton<BitmapCacheService>.Instance;

	internal IDictionary<string, ImageTracker> Images
	{
		get
		{
			if (m_images == null)
			{
				m_images = new Dictionary<string, ImageTracker>();
			}
			return m_images;
		}
	}

	private IDictionary<string, BitmapImage> BitmapImages => _bitmapImages;

	public ErrorList Initialize()
	{
		LogHelper.Instance.Log("Initialize bitmap cache service.");
		ServiceLocator.Instance.AddService(typeof(IBitmapCacheService), Instance);
		return new ErrorList();
	}

	public Image GetImage(string name)
	{
		if (!Images.ContainsKey(name))
		{
			return null;
		}
		ImageTracker imageTracker = Images[name];
		int num = (m_usageCounts.ContainsKey(name) ? m_usageCounts[name] : 0);
		num++;
		if (num > 65536)
		{
			num = 1;
		}
		m_usageCounts[name] = num;
		return imageTracker.Instance;
	}

	public Image RegisterImage(string name, byte[] data)
	{
		Image image = GetImage(name);
		if (image != null)
		{
			return image;
		}
		if (data == null || data.Length == 0)
		{
			return null;
		}
		try
		{
			MemoryStream memoryStream = new MemoryStream(data);
			image = Image.FromStream(memoryStream);
			Images[name] = new ImageTracker
			{
				Instance = image,
				UnderlyingStream = memoryStream
			};
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterImage from stream.", e);
		}
		return image;
	}

	public BitmapImage RegisterBitmapImage(string name, byte[] data)
	{
		BitmapImage bitmapImage = GetBitmapImage(name);
		if (bitmapImage == null && data != null && data.Length != 0)
		{
			try
			{
				MemoryStream streamSource = new MemoryStream(data);
				bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.StreamSource = streamSource;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();
				BitmapImages.Add(name, bitmapImage);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterBitmapImage from stream.", e);
			}
		}
		return bitmapImage;
	}

	public Image RegisterImage(string name, string path)
	{
		Image image = GetImage(name);
		if (image == null)
		{
			try
			{
				image = Image.FromFile(path);
				Images[name] = new ImageTracker
				{
					Instance = image
				};
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterImage from file.", e);
			}
		}
		return image;
	}

	public Image RegisterThumbnailImage(string name, byte[] data, int width, int height)
	{
		Image image = GetImage(name);
		if (image != null)
		{
			return image;
		}
		if (data == null || data.Length == 0)
		{
			return null;
		}
		try
		{
			using MemoryStream stream = new MemoryStream(data);
			using Image image2 = Image.FromStream(stream);
			image = image2.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
			Images[name] = new ImageTracker
			{
				Instance = image
			};
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterThumbnailImage from stream.", e);
		}
		return image;
	}

	public Image RegisterThumbnailImage(string name, string newName, int width, int height)
	{
		Image image = GetImage(newName);
		if (image != null)
		{
			return image;
		}
		Image image2 = GetImage(name);
		if (image2 == null)
		{
			return null;
		}
		try
		{
			image = image2.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
			Images[newName] = new ImageTracker
			{
				Instance = image
			};
			return image;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterThumbnailImage from existing image.", e);
		}
		return null;
	}

	public void DropCache()
	{
		DropImageCache();
		DropBitmapImageCache();
	}

	private void DropBitmapImageCache()
	{
		int count = _bitmapImages.Count;
		LogHelper.Instance.Log("Drop bitmap cache.");
		foreach (KeyValuePair<string, BitmapImage> bitmapImage in _bitmapImages)
		{
			LogHelper.Instance.Log("...Dropping bitmap image '{0}'.", bitmapImage.Key);
		}
		_bitmapImages.Clear();
		_bitmapImageUsageCount.Clear();
		LogHelper.Instance.Log("Drop bitmap cache complete. {0} bitmaps removed.", count);
	}

	private void DropImageCache()
	{
		LogHelper.Instance.Log("Drop image cache.");
		List<string> list = new List<string>();
		foreach (string key in Images.Keys)
		{
			LogHelper.Instance.Log("...Dispose image '{0}'.", key);
			try
			{
				Images[key].Dispose();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.DropImageCache.", e);
			}
			finally
			{
				list.Add(key);
			}
		}
		LogHelper.Instance.Log("...Remove image keys from dictionary.");
		foreach (string item in list)
		{
			Images.Remove(item);
		}
	}

	public int TrimCache(int maximumItems)
	{
		return TrimBitmapImageCache(maximumItems) + TrimImageCache(maximumItems);
	}

	private int TrimBitmapImageCache(int maximumItems)
	{
		int num = Math.Max(0, _bitmapImages.Count - maximumItems);
		LogHelper.Instance.Log("Trim the bitmap image cache:");
		List<ImageUsage> list = new List<ImageUsage>();
		foreach (KeyValuePair<string, int> item in _bitmapImageUsageCount)
		{
			list.Add(new ImageUsage
			{
				Name = item.Key,
				Uses = item.Value
			});
		}
		list.Sort((ImageUsage x, ImageUsage y) => x.Uses.CompareTo(y.Uses));
		foreach (ImageUsage item2 in list.Take(num))
		{
			LogHelper.Instance.Log("...Trim bitmap image '{0}' from the cache.", item2.Name);
			_bitmapImages.Remove(item2.Name);
			_bitmapImageUsageCount.Remove(item2.Name);
		}
		LogHelper.Instance.Log("...{0} bitmap images trimmed.", num);
		return num;
	}

	private int TrimImageCache(int maximumItems)
	{
		int num = 0;
		if (m_images.Count <= maximumItems)
		{
			return num;
		}
		LogHelper.Instance.Log("Trim the image cache:");
		List<ImageUsage> list = new List<ImageUsage>();
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service != null)
		{
			foreach (KeyValuePair<string, ImageTracker> image in m_images)
			{
				if (!service.IsBitmapReferenced(image.Value.Instance))
				{
					list.Add(new ImageUsage
					{
						Name = image.Key,
						Uses = (m_usageCounts.ContainsKey(image.Key) ? m_usageCounts[image.Key] : 0)
					});
				}
			}
		}
		list.Sort((ImageUsage x, ImageUsage y) => x.Uses.CompareTo(y.Uses));
		for (int num2 = 0; num2 < m_images.Count - maximumItems; num2++)
		{
			string name = list[num2].Name;
			LogHelper.Instance.Log("...Trim image '{0}' from the cache.", name);
			ImageTracker imageTracker = m_images[name];
			try
			{
				imageTracker.Dispose();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.TrimCache.", e);
			}
			m_images.Remove(name);
			m_usageCounts.Remove(name);
			num++;
		}
		LogHelper.Instance.Log("...{0} images trimmed.", num);
		return num;
	}

	public BitmapImage GetBitmapImage(string imageName)
	{
		BitmapImage value = null;
		BitmapImages.TryGetValue(imageName, out value);
		if (value != null)
		{
			int value2 = 0;
			_bitmapImageUsageCount.TryGetValue(imageName, out value2);
			value2++;
			_bitmapImageUsageCount[imageName] = value2;
		}
		return value;
	}

	public BitmapImage ToBitmapImage(Image image)
	{
		if (image == null)
		{
			return null;
		}
		return ToBitmapImage(new Bitmap(image));
	}

	public BitmapImage ToBitmapImage(Bitmap bitmap)
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

	public void Dispose()
	{
		DropCache();
	}

	private BitmapCacheService()
	{
	}
}
