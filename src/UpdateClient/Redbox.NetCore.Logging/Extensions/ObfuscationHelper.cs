namespace Redbox.NetCore.Logging.Extensions;

public static class ObfuscationHelper
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
		string head = ((leading > 0) ? msgText.Substring(0, leading) : string.Empty);
		string tail = ((trailing > 0) ? msgText.Substring(msgText.Length - trailing, trailing) : string.Empty);
		string mask = new string(newChar, msgText.Length - (leading + trailing));
		return head + mask + tail;
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
