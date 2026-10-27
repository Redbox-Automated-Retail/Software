using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.KioskEngine.Environment.TextToSpeech;

public class SpeechPart : ISpeechPart
{
	private List<INeededMacro> _neededMacros = new List<INeededMacro>();

	private List<ITextPart> _texts = new List<ITextPart>();

	private List<ISpeechPartEvent> _events = new List<ISpeechPartEvent>();

	private bool _autoRun = true;

	public ISpeechControl SpeechControl { get; set; }

	public int KeySequenceDelay { get; set; }

	public int Sequence { get; set; }

	public string Name { get; set; }

	public string Language { get; set; }

	public bool Loop { get; set; }

	public int StartPause { get; set; }

	public int EndPause { get; set; }

	public bool AutoRun
	{
		get
		{
			return _autoRun;
		}
		set
		{
			_autoRun = value;
		}
	}

	public List<INeededMacro> NeededMacros => _neededMacros;

	public List<ITextPart> Texts => _texts;

	public Action Refresh { get; set; }

	public IRegularExpression RegularExpression { get; set; }

	public List<ISpeechPartEvent> Events => _events;

	public void ValidateMacros(IMacroService macroService, Dictionary<string, string> macrosFound)
	{
		foreach (INeededMacro neededMacro in NeededMacros)
		{
			if (macroService[neededMacro.Name] != null)
			{
				macrosFound.Add(neededMacro.Name, macroService[neededMacro.Name]);
				continue;
			}
			macrosFound.Add(neededMacro.Name, neededMacro?.Default?.ToString());
			LogHelper.Instance.Log("TTS((> Required macro {{" + neededMacro.Name + "}} not found; using default value");
		}
		if (macrosFound.Count != NeededMacros.Count)
		{
			LogHelper.Instance.Log("TTS((> Required text macro lookup mismatch: requires " + NeededMacros.Count + ", found only " + macrosFound.Count);
		}
	}

	public ISpeechPart EvaluateRegularExpression(Dictionary<string, string> macrosFound)
	{
		ISpeechPart speechPart = null;
		if (RegularExpression != null)
		{
			string text = RegularExpression.Value;
			foreach (KeyValuePair<string, string> item in macrosFound)
			{
				text = text.Replace("{{" + item.Key + "}}", item.Value);
			}
			string expression = RegularExpression.Expression;
			LogHelper.Instance.Log("TTS((> regex:" + expression + "  value: " + text);
			bool flag = expression != null && new Regex(expression).IsMatch(text);
			LogHelper.Instance.Log("TTS((> regex result:" + flag);
			string text2 = (flag ? RegularExpression.Success : RegularExpression.Failure);
			int goToSequence = 0;
			speechPart = ((!int.TryParse(text2, out goToSequence)) ? SpeechControl?.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Name == RegularExpression.Success) : SpeechControl?.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Sequence == goToSequence));
			if (speechPart != null)
			{
				LogHelper.Instance.Log("TTS((> Skipping to Sequence: #{0} {1}", speechPart.Sequence, speechPart.Name);
			}
			else
			{
				LogHelper.Instance.Log("TTS((> Skipping to Sequence: {0}", text2);
				LogHelper.Instance.Log("TTS((> Unable to find to Sequence: {0}", text2);
			}
		}
		return speechPart;
	}

	public void EnqueueText(Queue<string> queue, Dictionary<string, string> macrosFound, IViewService viewService)
	{
		if (queue == null || macrosFound == null || viewService == null)
		{
			return;
		}
		foreach (ITextPart text2 in Texts)
		{
			string text = string.Empty;
			if (text2.TextAvailable != null)
			{
				bool flag = false;
				foreach (KeyValuePair<string, string> item in macrosFound)
				{
					if (text2.TextAvailable.Contains(item.Key) && !string.IsNullOrEmpty(item.Value))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					text += text2.Text;
				}
			}
			else if (text2.IfActorVisible != null)
			{
				IActor actor = ((viewService?.PeekViewFrame())?.ViewFrame as IViewFrame)?.Scene?.GetActor(text2.IfActorVisible);
				bool flag2 = true;
				if (!string.IsNullOrEmpty(text2.IfControlEnabled) && actor?.WPFFrameworkElement != null && actor.WPFFrameworkElement is ITextToSpeechControl textToSpeechControl)
				{
					flag2 = textToSpeechControl.IsControlEnabled(text2.IfControlEnabled);
				}
				if ((actor != null && actor.Visible && actor.Enabled) & flag2)
				{
					text += text2.Text;
				}
			}
			else
			{
				text += text2.Text;
			}
			foreach (KeyValuePair<string, string> item2 in macrosFound)
			{
				text = text.Replace("{{" + item2.Key + "}}", item2.Value);
			}
			text = text.Replace("\r\n", " ");
			text = text.Replace("\t", "");
			if (text.Length > 0)
			{
				queue.Enqueue(text);
			}
		}
	}

	public void Clear()
	{
		_neededMacros.Clear();
		_texts.Clear();
		_events.Clear();
		RegularExpression?.Clear();
	}
}
