using System.Collections.Generic;

namespace Redbox.Rental.Model;

public interface IObfuscationService
{
	string ObfuscateMessageData(string msgText, char newChar, int leading, int trailing);

	string ObfuscateStringData(string value, int leading, int trailing);

	string ObfuscateNumericData(string value, int leading, int trailing);

	void ObfuscateDictionary(Dictionary<string, object> workDictionary);
}
