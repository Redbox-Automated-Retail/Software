namespace Redbox.Rental.Model.KioskClientService.TestData;

public interface IKioskClientServiceTestData
{
	IReadTestDataValueResult ReadPropertyValue(string category, string propertyName);

	IWriteTestDataValueResult WritePropertyValue(string category, string propertyName, string propertyValue);

	IIsTestDataEnabledResult GetIsTestDataEnabled();
}
