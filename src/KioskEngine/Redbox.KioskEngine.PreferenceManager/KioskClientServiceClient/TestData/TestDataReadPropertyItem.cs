using System;

namespace Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.TestData;

public class TestDataReadPropertyItem
{
	public string PropertyName { get; set; }

	public Action<ReadTestDataValueResult> ReadSuccessAction { get; set; }

	public Action ReadFailAction { get; set; }

	public ReadTestDataValueResult ReadResult { get; set; }

	public TestDataReadPropertyItem(string propertyName, Action<ReadTestDataValueResult> readSuccessAction, Action readFailureAction)
	{
		PropertyName = propertyName;
		ReadSuccessAction = readSuccessAction;
		ReadFailAction = readFailureAction;
	}
}
