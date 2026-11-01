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
		string[] parts = type.Split(',');
		ClassName = parts[0];
		Assembly = parts[1];
		if (parts.Length > 2)
		{
			Version = parts[2];
			PublicKeyToken = parts[4];
		}
	}

	public override string ToString()
	{
		return $"{ClassName}, {Assembly}, Version={Version}, Culture={Culture}, PublicKeyToken={PublicKeyToken}";
	}
}
