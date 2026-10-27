using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.TextToSpeech;

public interface ISpeechPart
{
	ISpeechControl SpeechControl { get; set; }

	int KeySequenceDelay { get; set; }

	int Sequence { get; set; }

	string Name { get; set; }

	string Language { get; set; }

	bool Loop { get; set; }

	int StartPause { get; set; }

	int EndPause { get; set; }

	IRegularExpression RegularExpression { get; set; }

	List<ISpeechPartEvent> Events { get; }

	List<ITextPart> Texts { get; }

	List<INeededMacro> NeededMacros { get; }

	Action Refresh { get; set; }

	bool AutoRun { get; set; }

	void ValidateMacros(IMacroService macroService, Dictionary<string, string> macrosFound);

	ISpeechPart EvaluateRegularExpression(Dictionary<string, string> macrosFound);

	void EnqueueText(Queue<string> queue, Dictionary<string, string> macrosFound, IViewService viewService);

	void Clear();
}
