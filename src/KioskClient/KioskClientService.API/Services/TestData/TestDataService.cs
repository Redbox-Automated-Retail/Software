using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.TestData;

public class TestDataService : ITestDataService
{
	private class Constants
	{
		public class Errors
		{
			public const string ConfigNotEnabledCode = "100";

			public const string ConfigNotEnabledMessage = "Test data configuration is not enabled.";

			public const string TestDataFileWriteErrorCode = "200";

			public const string TestDataFileWriteErrorMessage = "Unable to write test data to file.";

			public const string TestDataFileReadErrorCode = "300";

			public const string TestDataFileReadErrorMessage = "Unable to read test data to file.";

			public const string TestDataFileDeserializationErrorCode = "400";

			public const string TestDataFileDeserializationErrorMessage = "Unable to deserialize test data from file.";

			public const string ActionIsNullCode = "500";

			public const string ActionIsNullMessage = "IsEnabledAction can not be null.";
		}
	}

	private class TestDataCategory
	{
		public SemaphoreSlim SemaphoreSlim { get; }

		public ConcurrentDictionary<string, string> KeyValuePairs { get; }

		public TestDataCategory()
		{
			SemaphoreSlim = new SemaphoreSlim(1, 1);
			KeyValuePairs = new ConcurrentDictionary<string, string>();
		}
	}

	private string _testDataPath;

	private readonly ILogger _logger;

	private IKioskConfiguration _kioskConfiguration;

	private IStoreService _storeService;

	private static ConcurrentDictionary<string, TestDataCategory> _dataValues = new ConcurrentDictionary<string, TestDataCategory>();

	private string TestDataPath
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_testDataPath))
			{
				string path = Path.Combine(_storeService.DataPath, "Data\\Test");
				_testDataPath = Path.GetFullPath(path);
				Directory.CreateDirectory(_testDataPath);
			}
			return _testDataPath;
		}
	}

	public TestDataService(ILogger<TestDataService> logger, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, IStoreService storeService)
	{
		_logger = logger;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_storeService = storeService;
	}

	public ITestDataHelper GetTestDataHelper(string category, string liveCallPropertyName)
	{
		return new TestDataHelper(this, category, liveCallPropertyName);
	}

	public async Task<IsTestDataEnabledResponse> GetIsTestDataEnabledAsync()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		IsTestDataEnabledResponse result = new IsTestDataEnabledResponse();
		await Task.Run(delegate
		{
			result.IsTestDataEnabled = _kioskConfiguration?.TestData?.TestDataEnabled == true;
		});
		result.ResponseTime = GetStopwatchMilliseconds(stopwatch);
		return result;
	}

	public async Task<WriteTestDataResponse> WriteTestDataValueAsync(string category, string propertyName, string propertyValue)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		WriteTestDataResponse result = new WriteTestDataResponse();
		await VerifyTestDataConfigIsEnabledAsync(result, async delegate
		{
			TestDataCategory testDataCategory = await GetOrReadCategoryTestDataFromFileAsync(category, result);
			if (testDataCategory == null)
			{
				testDataCategory = GetCategoryValues(category);
			}
			await testDataCategory.SemaphoreSlim.WaitAsync();
			try
			{
				_logger.LogInformation("TestDataService writing to category " + category + " property name " + propertyName);
				if (propertyValue != null)
				{
					testDataCategory.KeyValuePairs[propertyName] = propertyValue;
				}
				else
				{
					testDataCategory.KeyValuePairs.TryRemove(propertyName, out var _);
				}
				string value2 = JsonConvert.SerializeObject(testDataCategory.KeyValuePairs, Formatting.Indented, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				try
				{
					using StreamWriter outputFile = new StreamWriter(GetCategoryFileName(category));
					await outputFile.WriteAsync(value2);
				}
				catch (Exception exception)
				{
					_logger.LogError(exception, "TestDataService.WriteTestDataValueAsync failed to write category " + category + " property name " + propertyName + " to file " + GetCategoryFileName(category));
					result.Success = false;
					result.Errors.Add(new Error
					{
						Code = "200",
						Message = "Unable to write test data to file."
					});
				}
			}
			finally
			{
				testDataCategory.SemaphoreSlim.Release();
			}
		});
		result.ResponseTime = GetStopwatchMilliseconds(stopwatch);
		return result;
	}

	public async Task<ReadTestDataResponse> ReadTestDataValueAsync(string category, string propertyName)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		ReadTestDataResponse result = new ReadTestDataResponse();
		await VerifyTestDataConfigIsEnabledAsync(result, async delegate
		{
			TestDataCategory testDataCategory = await GetOrReadCategoryTestDataFromFileAsync(category, result);
			if (testDataCategory != null && testDataCategory.KeyValuePairs.TryGetValue(propertyName, out var value))
			{
				result.PropertyValue = value;
			}
		});
		result.ResponseTime = GetStopwatchMilliseconds(stopwatch);
		return result;
	}

	private async Task<TestDataCategory> GetOrReadCategoryTestDataFromFileAsync(string category, BaseResponse result)
	{
		if (!_dataValues.TryGetValue(category, out var testDataCategory) && File.Exists(GetCategoryFileName(category)))
		{
			try
			{
				using StreamReader streamReader = File.OpenText(GetCategoryFileName(category));
				string value = await streamReader.ReadToEndAsync();
				try
				{
					ConcurrentDictionary<string, string> concurrentDictionary = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(value);
					testDataCategory = GetCategoryValues(category);
					if (concurrentDictionary != null && testDataCategory?.KeyValuePairs != null)
					{
						foreach (KeyValuePair<string, string> item in concurrentDictionary)
						{
							testDataCategory.KeyValuePairs[item.Key] = item.Value;
						}
					}
				}
				catch (Exception exception)
				{
					result.Errors.Add(new Error
					{
						Code = "400",
						Message = "Unable to deserialize test data from file."
					});
					result.Success = false;
					_logger.LogError(exception, "TestDataService.ReadTestDataValueAsync failed to deserialize category " + category + " data from file " + GetCategoryFileName(category));
				}
			}
			catch (Exception exception2)
			{
				result.Errors.Add(new Error
				{
					Code = "300",
					Message = "Unable to read test data to file."
				});
				result.Success = false;
				_logger.LogError(exception2, "TestDataService.ReadTestDataValueAsync failed to read category " + category + " data from file " + GetCategoryFileName(category));
			}
		}
		return testDataCategory;
	}

	private async Task VerifyTestDataConfigIsEnabledAsync(BaseResponse response, Func<Task> isEnabledAction)
	{
		if (_kioskConfiguration?.TestData?.TestDataEnabled == true)
		{
			if (isEnabledAction != null)
			{
				await isEnabledAction();
				return;
			}
			response.Success = false;
			response.Errors.Add(new Error
			{
				Code = "500",
				Message = "IsEnabledAction can not be null."
			});
			_logger.LogError("TestDataService: isEnabledAction can not be null.");
		}
		else
		{
			response.Success = false;
			response.Errors.Add(new Error
			{
				Code = "100",
				Message = "Test data configuration is not enabled."
			});
			_logger.LogError("TestDataService: test data configuration is not enabled.");
		}
	}

	private int GetStopwatchMilliseconds(Stopwatch stopwatch)
	{
		try
		{
			return Convert.ToInt32(stopwatch.Elapsed.TotalMilliseconds);
		}
		catch
		{
			return 0;
		}
	}

	private TestDataCategory GetCategoryValues(string category)
	{
		return _dataValues.GetOrAdd(category, delegate
		{
			_logger.LogInformation("TestDataService.GetCategoryValues: added cache for category " + category);
			return new TestDataCategory();
		});
	}

	private string GetCategoryFileName(string category)
	{
		return Path.Combine(TestDataPath, Path.ChangeExtension(category, "json"));
	}
}
