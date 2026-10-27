using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services;

public class FraudService : IFraudService
{
	private const string _imageFolderPath = "c:\\program files\\redbox\\halservice\\video";

	private readonly string _fraudImageFolderPath = Path.Combine("c:\\program files\\redbox\\halservice\\video", "fraud_images");

	public static FraudService Instance => Singleton<FraudService>.Instance;

	public void AddToFraudStatistics(string fraudBarcodes)
	{
		if (string.IsNullOrWhiteSpace(fraudBarcodes))
		{
			LogHelper.Instance.Log("AddToFraudStatistics No fraud barcodes data provided.", LogEntryType.Info);
			return;
		}
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null)
		{
			LogHelper.Instance.Log("Enqueuing Statistic FraudCleanSync Failed.", LogEntryType.Info);
			return;
		}
		LogHelper.Instance.Log("Enqueuing Statistic FraudCleanSync with value: " + fraudBarcodes, LogEntryType.Info);
		service.EnqueueStatistic("FraudCleanSync", fraudBarcodes);
	}

	private void DeleteImage(string imageName)
	{
		if (string.IsNullOrWhiteSpace(imageName))
		{
			LogHelper.Instance.Log("Delete Image failed because no image name provided.", LogEntryType.Info);
			return;
		}
		string path = Path.Combine("c:\\program files\\redbox\\halservice\\video", imageName);
		if (!File.Exists(path))
		{
			LogHelper.Instance.Log("Image " + imageName + " doesn't exist.", LogEntryType.Info);
			return;
		}
		try
		{
			LogHelper.Instance.Log("Delete Image " + imageName, LogEntryType.Info);
			File.Delete(path);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to delete image " + imageName, e);
		}
	}

	private void DeleteImages(IEnumerable<string> images)
	{
		LogHelper.Instance.Log($"Attempting to delete {images?.Count() ?? 0} images.", LogEntryType.Info);
		images?.ForEach(delegate(string x)
		{
			DeleteImage(x);
		});
	}

	public void CleanImageFolder()
	{
		try
		{
			if (!Directory.Exists("c:\\program files\\redbox\\halservice\\video"))
			{
				LogHelper.Instance.Log("FraudService.CleanImageFolder did not find the image folder path: c:\\program files\\redbox\\halservice\\video - bailing", LogEntryType.Info);
				return;
			}
			FileInfo[] files = new DirectoryInfo("c:\\program files\\redbox\\halservice\\video").GetFiles();
			int keepImagesForNumberOfDays = Redbox.Rental.Services.Configuration.Configuration.Instance.KeepHalImagesForNumberOfDays;
			IEnumerable<string> images = from x in files
				where (DateTime.Now - x.CreationTime).TotalDays > (double)keepImagesForNumberOfDays
				select x.Name;
			DeleteImages(images);
			if (!Directory.Exists(_fraudImageFolderPath))
			{
				return;
			}
			LogHelper.Instance.Log("Remove Old fraud image folder files.");
			FileInfo[] files2 = new DirectoryInfo(_fraudImageFolderPath).GetFiles();
			if (files2.Any())
			{
				IEnumerable<string> images2 = from x in files2
					where (DateTime.Now - x.CreationTime).TotalDays > 0.0
					select x.Name;
				DeleteImages(images2);
			}
			else
			{
				LogHelper.Instance.Log("Remove Old fraud image directory.");
				Directory.Delete(_fraudImageFolderPath);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("FraudService.CleanImageFolder experienced an unhandled exception", e);
		}
	}

	private FraudService()
	{
	}
}
