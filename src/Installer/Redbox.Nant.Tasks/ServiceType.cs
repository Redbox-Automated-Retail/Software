namespace Redbox.Nant.Tasks;

public class ServiceType
{
	public string ClassName;

	public string Assembly;

	public string Version;

	public string Culture = "neutral";

	public string PublicKeyToken;

	public ServiceType(string type)
	{
		string[] array = type.Split(',');
		ClassName = array[0];
		Assembly = array[1];
		if (array.Length > 2)
		{
			Version = array[2];
			PublicKeyToken = array[4];
		}
	}

	public override string ToString()
	{
		return $"{ClassName}, {Assembly}, Version={Version}, Culture={Culture}, PublicKeyToken={PublicKeyToken}";
	}
}
