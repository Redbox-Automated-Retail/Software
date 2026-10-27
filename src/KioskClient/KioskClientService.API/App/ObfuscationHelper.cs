namespace KioskClientService.API.App;

public class ObfuscationHelper
{
	public class Constants
	{
		public const char ReplacementCharacter = 'x';

		public const char ReplacementNumber = '#';
	}

	public static string ObfuscateMessageData(string msgText, char newChar, int leading, int trailing)
	{
		if (msgText == null)
		{
			return null;
		}
		if (leading + trailing > msgText.Length)
		{
			leading = 0;
			trailing = 0;
		}
		string text = ((leading > 0) ? msgText.Substring(0, leading) : string.Empty);
		string text2 = ((trailing > 0) ? msgText.Substring(msgText.Length - trailing, trailing) : string.Empty);
		string text3 = new string(newChar, msgText.Length - (leading + trailing));
		return text + text3 + text2;
	}

	public static string ObfuscateStringData(string value, int leading, int trailing)
	{
		return ObfuscateMessageData(value, 'x', leading, trailing);
	}

	public static string ObfuscateNumericData(string value, int leading, int trailing)
	{
		return ObfuscateMessageData(value, '#', leading, trailing);
	}
}
