using System.IO;
using System.Text.RegularExpressions;

namespace Redbox.Core;

public class RegExFileEditor
{
	private readonly string m_filePath;

	public RegExFileEditor(string filePath, bool backup, string backupExtension)
	{
		m_filePath = filePath;
		if (backup)
		{
			FileHelper.CreateBackup(m_filePath, backupExtension);
		}
	}

	public void Replace(string pattern, string token, string newValue)
	{
		Replace(new string[1] { pattern }, new string[1] { token }, new string[1] { newValue });
	}

	public void Replace(string[] patterns, string[] tokens, string[] newValues)
	{
		Update(ReplaceTokens(newValues, patterns, tokens));
	}

	internal static string Apply(string regEx, string originalText, string token)
	{
		Match match = new Regex(regEx).Match(originalText);
		if (!match.Success)
		{
			return null;
		}
		return match.Groups[token].ToString();
	}

	internal string ReplaceTokens(string[] newValues, string[] patterns, string[] tokens)
	{
		using TextReader textReader = new StreamReader(m_filePath);
		return ReplaceTokens(textReader.ReadToEnd(), newValues, patterns, tokens);
	}

	private static string ReplaceTokens(string originalFileContents, string[] newValues, string[] patterns, string[] tokens)
	{
		for (int i = 0; i < newValues.Length; i++)
		{
			originalFileContents = originalFileContents.Replace(Apply(patterns[i], originalFileContents, tokens[i]), newValues[i]);
		}
		return originalFileContents;
	}

	private void Update(string newFileContents)
	{
		using TextWriter textWriter = new StreamWriter(m_filePath);
		textWriter.WriteLine(newFileContents);
	}
}
