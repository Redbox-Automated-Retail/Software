using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.Services.Ads;

public class VistarAd : IVistarAd, IAd
{
	private BitmapImage _image;

	public string ImageName { get; set; }

	public int? MinimumDisplay { get; set; }

	public BitmapImage Image
	{
		get
		{
			if (_image == null && !string.IsNullOrEmpty(ImageName))
			{
				try
				{
					Image image = System.Drawing.Image.FromFile(ImageName);
					_image = ServiceLocator.Instance.GetService<IBitmapCacheService>()?.ToBitmapImage(image);
				}
				catch (Exception arg)
				{
					LogHelper.Instance.Log($"Error in VistarAd.Image.  Unable to load image {ImageName} {arg}");
				}
			}
			return _image;
		}
	}

	public string AdvertisementId { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }
}
