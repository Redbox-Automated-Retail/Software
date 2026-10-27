using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.TestData;

public class TestDataUtility
{
	public bool ReadDataSucceeded { get; set; }

	public string CategoryName { get; set; }

	public Action<string> UpdateUIErrorMessage { get; set; }

	public async Task ReadKCSTestDataProperties(List<TestDataReadPropertyItem> readPropertyItems)
	{
		if (readPropertyItems == null)
		{
			return;
		}
		bool readSuccess = true;
		foreach (TestDataReadPropertyItem eachReadPropertyItem in readPropertyItems)
		{
			eachReadPropertyItem.ReadResult = ReadTestDataValueResult.CreateFromReadTestDataResponse((await KioskClientServiceTestDataClient.ReadPropertyValue(CategoryName, eachReadPropertyItem.PropertyName))?.Response);
			if (eachReadPropertyItem.ReadResult == null || !eachReadPropertyItem.ReadResult.Success)
			{
				readSuccess = false;
			}
		}
		ReadDataSucceeded = readSuccess;
	}

	public void ProcessReadActions(List<TestDataReadPropertyItem> readPropertyItems)
	{
		if (readPropertyItems == null)
		{
			return;
		}
		foreach (TestDataReadPropertyItem readPropertyItem in readPropertyItems)
		{
			if (readPropertyItem.ReadResult != null && readPropertyItem.ReadResult.Success)
			{
				readPropertyItem.ReadSuccessAction(readPropertyItem.ReadResult);
				continue;
			}
			readPropertyItem.ReadFailAction();
			SetErrorMessage("Error in KCS reading property " + readPropertyItem.PropertyName + ": ", readPropertyItem.ReadResult?.Errors, "Unable to read property " + readPropertyItem.PropertyName);
		}
	}

	public async Task WriteKCSTestDataProperty(string propertyName, string propertyValue)
	{
		if (ReadDataSucceeded)
		{
			WriteTestDataValueResult writeTestDataValueResult = WriteTestDataValueResult.CreateFromWriteTestDataResponse((await KioskClientServiceTestDataClient.WritePropertyValue(CategoryName, propertyName, propertyValue))?.Response);
			if (writeTestDataValueResult != null && writeTestDataValueResult.Success)
			{
				UpdateUIErrorMessage?.Invoke(null);
				return;
			}
			SetErrorMessage("Error in KCS saving property " + propertyName + " with value '" + propertyValue + "': ", writeTestDataValueResult?.Errors, "Unable to save property " + propertyName);
		}
		else
		{
			UpdateUIErrorMessage?.Invoke("Save of property " + propertyName + " aborted because prior read failed.  Exit F4 screen and re-enter to attempt data reload.");
		}
	}

	public void SetErrorMessage(string messageHeader, List<IError> errors, string genericErrorMessage)
	{
		if (errors != null)
		{
			StringBuilder stringBuilder = new StringBuilder(messageHeader);
			string text = null;
			foreach (IError error in errors)
			{
				stringBuilder.Append("(" + error.Code + ") " + error.Message + text);
				text = ", ";
			}
			UpdateUIErrorMessage?.Invoke(stringBuilder.ToString());
		}
		else
		{
			UpdateUIErrorMessage?.Invoke(genericErrorMessage);
		}
	}
}
