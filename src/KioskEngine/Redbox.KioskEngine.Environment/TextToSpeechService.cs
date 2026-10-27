using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.Environment;

public class TextToSpeechService : ITextToSpeechService
{
	public struct RECT
	{
		public int X;

		public int Y;

		public int Right;

		public int Bottom;

		public int Width => Right - X;

		public int Height => Bottom - Y;
	}

	private const int MOUSEEVENTF_LEFTDOWN = 2;

	private const int MOUSEEVENTF_LEFTUP = 4;

	private const int MOUSEEVENTF_RIGHTDOWN = 8;

	private const int MOUSEEVENTF_RIGHTUP = 16;

	private bool m_audioDeviceConnected;

	private SpeechSynthesizer m_reader;

	private ISpeechControl _speechControl;

	private ISpeechPart _currentSpeechPart;

	private int m_speechRate;

	private int m_speechVolume;

	private object m_speechSynthesizerLock = new object();

	private Dictionary<string, Delegate> m_keyHandlers = new Dictionary<string, Delegate>();

	private Dictionary<string, string> m_macrosFound = new Dictionary<string, string>();

	private Queue<string> m_textQueue;

	private string m_keySequence;

	private int m_keySequenceDelay;

	private Dictionary<string, string> m_textReplacement = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public static TextToSpeechService Instance => Singleton<TextToSpeechService>.Instance;

	public bool AudioDeviceConnected
	{
		get
		{
			return m_audioDeviceConnected;
		}
		set
		{
			if (m_audioDeviceConnected != value)
			{
				m_audioDeviceConnected = value;
				_speechControl = null;
				_currentSpeechPart = null;
				if (!m_audioDeviceConnected)
				{
					LogHelper.Instance.Log("TTS((> Headphones disconnected.");
					Reset();
					FireOnAudioDeviceConnectionChanged(isConnected: false);
				}
				else
				{
					LogHelper.Instance.Log("TTS((> Headphones connected.");
					FireOnAudioDeviceConnectionChanged(isConnected: true);
				}
			}
		}
	}

	public bool TTSEnabled => AudioDeviceConnected;

	public int GetTextToSpeechVolume
	{
		get
		{
			int num = ServiceLocator.Instance.GetService<IConfiguration>()?.TextToSpeechVolume ?? 100;
			if (num < 0)
			{
				num = 0;
			}
			if (num > 100)
			{
				num = 100;
			}
			return num;
		}
	}

	public event AudioDeviceConnectionChanged OnAudioDeviceConnectionChanged;

	public void ClearOnAudioDeviceConnectionChanged()
	{
		OnAudioDeviceConnectionChanged = null;
	}

	public void Reset()
	{
		LogHelper.Instance.Log("TTS((> Reset Text-To-Speech service.");
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		if (service != null)
		{
			service.RemoveTimer("TTSTimer");
			service.RemoveTimer("TTSInputTimer");
		}
		SilenceTTS();
		m_keyHandlers.Clear();
		m_speechRate = 0;
		m_speechVolume = GetTextToSpeechVolume;
	}

	public void ClearAudioDeviceConnected()
	{
		m_audioDeviceConnected = false;
		_speechControl = null;
		_currentSpeechPart = null;
		LogHelper.Instance.Log("TTS((> ClearAudioDeviceConnected");
		Reset();
	}

	public int GetTimeout(string timeoutType, int defaultTimeout)
	{
		if (!TTSEnabled)
		{
			return defaultTimeout;
		}
		return timeoutType switch
		{
			"shorter" => 15000, 
			"short" => 30000, 
			"medium" => 60000, 
			"long" => 180000, 
			"help" => 480000, 
			_ => 30000, 
		};
	}

	public void RunSpeechWorkflow(string resourceName)
	{
		m_keyHandlers.Clear();
		if (_speechControl != null)
		{
			LogHelper.Instance.Log("TTS((> stop existing speech workflow {0} #{1} {2}", _speechControl?.Name, _currentSpeechPart?.Sequence ?? 0, _currentSpeechPart?.Name);
			ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
			if (service != null)
			{
				service.RemoveTimer("TTSTimer");
				service.RemoveTimer("TTSInputTimer");
			}
			SilenceTTS();
			m_keyHandlers.Clear();
		}
		LogHelper.Instance.Log("TTS((> Run speech workflow for " + resourceName);
		_speechControl = LoadSpeechControlFromResouce(resourceName);
		if (_speechControl == null)
		{
			LogHelper.Instance.Log("TTS((> Attempting to get Speech Control for {0} from the view.", resourceName);
			ITextToSpeechControl textToSpeechControl = null;
			IViewFrameInstance viewFrameInstance = ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame();
			IViewFrame viewFrame = viewFrameInstance?.ViewFrame as IViewFrame;
			if (viewFrame?.Scene != null)
			{
				foreach (IActor actor in viewFrame.Scene.Actors)
				{
					textToSpeechControl = actor?.WPFFrameworkElement as ITextToSpeechControl;
					if (textToSpeechControl != null)
					{
						break;
					}
				}
			}
			else if (viewFrameInstance?.ViewFrame is IWPFViewFrame)
			{
				textToSpeechControl = ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.GetWPFGridChild(viewFrameInstance?.Id) as ITextToSpeechControl;
			}
			if (textToSpeechControl != null)
			{
				_speechControl = textToSpeechControl.GetSpeechControl();
				LogHelper.Instance.Log("TTS((> Loaded Speech Control for {0} from the view. {1} speech parts loaded", resourceName, (_speechControl?.SpeechParts?.Count).GetValueOrDefault());
			}
		}
		if (_speechControl == null)
		{
			return;
		}
		ISpeechPart speechPart = _speechControl.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Sequence == _speechControl.SpeechParts.Min((ISpeechPart y) => y.Sequence));
		if (speechPart.AutoRun)
		{
			RunSpeechPart(speechPart);
			return;
		}
		_currentSpeechPart = speechPart;
		LogHelper.Instance.Log("TTS((> AutoRun is false. Setting current speech part to {0} #{1} {2} but not running it.", _currentSpeechPart?.SpeechControl?.Name, _currentSpeechPart.Sequence.ToString(), _currentSpeechPart?.Name);
	}

	private void FireOnAudioDeviceConnectionChanged(bool isConnected)
	{
		if (OnAudioDeviceConnectionChanged != null)
		{
			OnAudioDeviceConnectionChanged(this, isConnected);
		}
	}

	private TextToSpeechService()
	{
		LogHelper.Instance.Log("TTS((> Service Initialized.");
		m_speechVolume = GetTextToSpeechVolume;
		m_textReplacement.Add("blu-ray", "blue ray");
		m_textReplacement.Add("sec", "second");
		m_textReplacement.Add("pg", "p g");
		m_textReplacement.Add("pg13", "p g 13");
		m_textReplacement.Add("pg-13", "p g 13");
		m_textReplacement.Add("esrb", "e s r b");
		m_textReplacement.Add("727272", "7 2 7 2 7 2");
		m_textReplacement.Add("msg", "message");
		m_textReplacement.Add("msgs", "messages");
		m_textReplacement.Add("*", "");
		m_textReplacement.Add("a-z", "'a' to z");
		m_textReplacement.Add("uhd", "u h d");
		m_textReplacement.Add("pssst", "");
		m_textReplacement.Add("pts", "points");
		m_textReplacement.Add("onceonly", "once only");
	}

	private void ProcessSpeechPart(ISpeechPart speechPart)
	{
		if (speechPart != null)
		{
			m_textQueue = new Queue<string>();
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			speechPart.EnqueueText(m_textQueue, m_macrosFound, service);
			if (m_textQueue.Count > 0)
			{
				SayTTS(m_textQueue.Dequeue());
			}
			ISpeechPart speechPart2 = speechPart.EvaluateRegularExpression(m_macrosFound);
			if (speechPart2 != null)
			{
				RunSpeechPartAfterDelay(speechPart2, speechPart.EndPause);
			}
		}
	}

	private void RunSpeechPart(ISpeechPart speechPart)
	{
		if (speechPart == null)
		{
			return;
		}
		if (speechPart.Refresh != null)
		{
			speechPart?.Refresh();
		}
		int num = -1;
		if (_currentSpeechPart != null && speechPart.SpeechControl == _currentSpeechPart.SpeechControl)
		{
			num = _currentSpeechPart.Sequence;
		}
		if (num == speechPart.Sequence)
		{
			LogHelper.Instance.Log("TTS((> Repeat speech control {0} part #{1} {2}", speechPart?.SpeechControl?.Name, speechPart?.Sequence.ToString(), speechPart?.Name);
		}
		else
		{
			LogHelper.Instance.Log("TTS((> Run speech part {0} #{1} {2}", speechPart?.SpeechControl?.Name, speechPart.Sequence.ToString(), speechPart?.Name);
		}
		_currentSpeechPart = speechPart;
		m_keyHandlers.Clear();
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		m_macrosFound = new Dictionary<string, string>();
		speechPart.ValidateMacros(service, m_macrosFound);
		ResetTTSEventHandlers(speechPart);
		if (_currentSpeechPart.KeySequenceDelay > 0)
		{
			m_keySequenceDelay = _currentSpeechPart.KeySequenceDelay;
		}
		else
		{
			m_keySequenceDelay = 1000;
		}
		ITimerService service2 = ServiceLocator.Instance.GetService<ITimerService>();
		if (service2 == null)
		{
			LogHelper.Instance.Log("TTS((> no delay TimerService NULL - passing directly to SayTTs");
			ProcessSpeechPart(speechPart);
			return;
		}
		service2.RemoveTimer("TTSTimer");
		_ = speechPart.Loop;
		int startPause = speechPart.StartPause;
		TimerCallback callback = delegate
		{
			ProcessSpeechPart(speechPart);
		};
		if (startPause == 0)
		{
			LogHelper.Instance.Log("TTS((> no delay startPause = 0 - passing directly to SayTTs");
			ProcessSpeechPart(speechPart);
		}
		else
		{
			LogHelper.Instance.Log("TTS((> dueTime = " + startPause);
			service2.CreateTimer("TTSTimer", startPause, null, callback).Start();
		}
	}

	private void RunSpeechPartAfterDelay(ISpeechPart speechPart, int delay)
	{
		if (delay > 0)
		{
			ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
			service.RemoveTimer("TTSTimer");
			service.CreateTimer("TTSTimer", delay, null, delegate
			{
				RunSpeechPart(speechPart);
			}).Start();
		}
		else
		{
			RunSpeechPart(speechPart);
		}
	}

	public void TTSRepeatSequence()
	{
		if (_currentSpeechPart != null)
		{
			RunSpeechPart(_currentSpeechPart);
		}
	}

	private void ResetTTSEventHandlers(ISpeechPart speechPart)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		m_keySequence = "";
		service.RemoveKeyPressHandler("TTS-KeyHandler");
		if (speechPart.Events == null)
		{
			return;
		}
		foreach (ISpeechPartEvent eachSpeechPartEvent in speechPart.Events)
		{
			if (eachSpeechPartEvent is MapKeyPress)
			{
				if (eachSpeechPartEvent.Function != null)
				{
					switch (eachSpeechPartEvent.Function)
					{
					case "TTSVolumeUp":
					{
						Action value8 = delegate
						{
							TTSVolumeUp();
							string speech = ((eachSpeechPartEvent.Text != null) ? eachSpeechPartEvent.Text : "Volume Up");
							SayTTS(speech);
						};
						m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, value8);
						break;
					}
					case "TTSVolumeDown":
					{
						Action value7 = delegate
						{
							TTSVolumeDown();
							string speech = ((eachSpeechPartEvent.Text != null) ? eachSpeechPartEvent.Text : "Volume Down");
							SayTTS(speech);
						};
						m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, value7);
						break;
					}
					case "TTSFaster":
					{
						Action value6 = delegate
						{
							TTSFaster();
							SayTTS("Faster");
						};
						m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, value6);
						break;
					}
					case "TTSSlower":
					{
						Action value5 = delegate
						{
							TTSSlower();
							SayTTS("Slower");
						};
						m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, value5);
						break;
					}
					case "TTSRepeatSpeechPart":
					{
						Action value4 = delegate
						{
							int sequence = _currentSpeechPart.Sequence;
							RunSpeechPart(_speechControl.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Sequence == sequence));
						};
						m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, value4);
						break;
					}
					case "TTSSkipToSpeechPart":
					{
						Action value3 = delegate
						{
							if (eachSpeechPartEvent.Value != null)
							{
								int goToSequence = 0;
								bool num = int.TryParse(eachSpeechPartEvent.Value, out goToSequence);
								ISpeechPart speechPart2 = null;
								speechPart2 = ((!num) ? _speechControl.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Name == eachSpeechPartEvent.Value) : _speechControl.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Sequence == goToSequence));
								RunSpeechPart(speechPart2);
							}
						};
						m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, value3);
						break;
					}
					case "TTSNextSpeechPart":
					{
						Action value2 = delegate
						{
							int sequence = _currentSpeechPart.Sequence;
							ISpeechPart speechPart2 = _speechControl.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Sequence == sequence + 1);
							if (speechPart2 != null)
							{
								ServiceLocator.Instance.GetService<ITimerService>()?.RemoveTimer("TTSTimer");
								RunSpeechPart(speechPart2);
							}
						};
						m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, value2);
						break;
					}
					case "TTSNextAudioSnippet":
					{
						Action value = delegate
						{
							if (m_textQueue.Count > 0)
							{
								SayTTS(m_textQueue.Dequeue());
							}
							else
							{
								ReaderNothingLeftToSay();
							}
						};
						m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, value);
						break;
					}
					}
				}
				else
				{
					if (eachSpeechPartEvent.Text == null)
					{
						continue;
					}
					Action value9 = delegate
					{
						string text = eachSpeechPartEvent.Text;
						foreach (KeyValuePair<string, string> item in m_macrosFound)
						{
							text = text.Replace("{{" + item.Key + "}}", item.Value);
						}
						SayTTS(text);
					};
					m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, value9);
				}
			}
			else if (eachSpeechPartEvent is IMapKeyPressToActorHit)
			{
				IMapKeyPressToActorHit mapKeyPressToActorHitEvent = eachSpeechPartEvent as IMapKeyPressToActorHit;
				if (mapKeyPressToActorHitEvent == null || string.IsNullOrEmpty(mapKeyPressToActorHitEvent.ActorName))
				{
					continue;
				}
				string actor_name = mapKeyPressToActorHitEvent.ActorName;
				foreach (KeyValuePair<string, string> item2 in m_macrosFound)
				{
					actor_name = actor_name.Replace("{{" + item2.Key + "}}", item2.Value);
				}
				Action value10 = delegate
				{
					LogHelper.Instance.Log("TTS((> actor_name:" + actor_name);
					if (!string.IsNullOrEmpty(mapKeyPressToActorHitEvent.Command))
					{
						ExecuteActorCommand(actor_name, mapKeyPressToActorHitEvent.Command, mapKeyPressToActorHitEvent.Parameter);
					}
					else
					{
						ClickActor(actor_name);
					}
					if (mapKeyPressToActorHitEvent.Function != null)
					{
						if (mapKeyPressToActorHitEvent.Text != null)
						{
							string text = mapKeyPressToActorHitEvent.Text;
							LogHelper.Instance.Log("TTS((> Say Action: " + text);
							foreach (KeyValuePair<string, string> item3 in m_macrosFound)
							{
								text = text.Replace("{{" + item3.Key + "}}", item3.Value);
							}
							LogHelper.Instance.Log("TTS((> Say Action: " + text);
							SayTTS(text);
						}
						if (mapKeyPressToActorHitEvent.Function == "TTSSkipToSpeechPart" && mapKeyPressToActorHitEvent.Value != null)
						{
							int goToSequence = 0;
							if (int.TryParse(mapKeyPressToActorHitEvent.Value, out goToSequence))
							{
								RunSpeechPart(_speechControl.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Sequence == goToSequence));
							}
						}
					}
				};
				if ((ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame())?.ViewFrame is IViewFrame viewFrame)
				{
					IActor actor = viewFrame.Scene.GetActor(actor_name);
					if (actor != null && (!string.IsNullOrEmpty(mapKeyPressToActorHitEvent.Command) || (actor.Visible && actor.Enabled)))
					{
						m_keyHandlers.Add(mapKeyPressToActorHitEvent.KeyCode, value10);
					}
				}
			}
			else
			{
				if (!(eachSpeechPartEvent is IMapKeyPressToAction))
				{
					continue;
				}
				IMapKeyPressToAction mapKeyPressToAction = eachSpeechPartEvent as IMapKeyPressToAction;
				if (mapKeyPressToAction == null)
				{
					continue;
				}
				Action value11 = delegate
				{
					mapKeyPressToAction.Action();
					if (mapKeyPressToAction.ActionText != null)
					{
						mapKeyPressToAction.Text = mapKeyPressToAction.ActionText();
					}
					if (mapKeyPressToAction.Text != null)
					{
						string text = mapKeyPressToAction.Text;
						LogHelper.Instance.Log("Say Action: " + text);
						foreach (KeyValuePair<string, string> item4 in m_macrosFound)
						{
							text = text.Replace("{{" + item4.Key + "}}", item4.Value);
						}
						LogHelper.Instance.Log("Say Action: " + text);
						SayTTS(text);
					}
					if (mapKeyPressToAction.Function == "TTSSkipToSpeechPart" && mapKeyPressToAction.Value != null)
					{
						int goToSequence = 0;
						ISpeechPart speechPart2 = null;
						speechPart2 = ((!int.TryParse(mapKeyPressToAction.Value, out goToSequence)) ? _speechControl.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Name == mapKeyPressToAction.Value) : _speechControl.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Sequence == goToSequence));
						if (speechPart2 != null)
						{
							RunSpeechPartAfterDelay(speechPart2, _currentSpeechPart?.EndPause ?? 0);
						}
					}
					if (mapKeyPressToAction.Function == "TTSRepeatSpeechPart")
					{
						TTSRepeatSequence();
					}
				};
				m_keyHandlers.Add(mapKeyPressToAction.KeyCode, value11);
			}
		}
		KeyPressHandler handler = delegate(string key, int keyCode, string modifier)
		{
			LogHelper.Instance.Log("TTS((> Key Sequence, mapping:" + key);
			if (key == "A")
			{
				key = "ENTER";
			}
			if (key == "B")
			{
				key = "CLEAR";
			}
			if (key == "C")
			{
				key = "CANCEL";
			}
			if (key == ".")
			{
				key = "PLUS";
			}
			if (key == "*")
			{
				key = "MINUS";
			}
			if (key == "#")
			{
				key = "TAB";
			}
			if (key == "a")
			{
				key = "1";
			}
			if (key == "b")
			{
				key = "2";
			}
			if (key == "d")
			{
				key = "4";
			}
			if (key == "e")
			{
				key = "5";
			}
			if (key == "f")
			{
				key = "6";
			}
			if (key == "g")
			{
				key = "7";
			}
			if (key == "h")
			{
				key = "8";
			}
			if (key == "i")
			{
				key = "9";
			}
			if (key == "`")
			{
				key = "0";
			}
			if (key == "k")
			{
				key = "PLUS";
			}
			if (key == "m")
			{
				key = "MINUS";
			}
			if (key == "\r")
			{
				key = "ENTER";
			}
			if (key == "c")
			{
				key = "CANCEL";
			}
			if (key == "C")
			{
				key = "CANCEL";
			}
			if (key == "x")
			{
				key = "CANCEL";
			}
			if (key == "X")
			{
				key = "CANCEL";
			}
			if (key == "\b")
			{
				key = "CLEAR";
			}
			if (key == "+")
			{
				key = "PLUS";
			}
			if (key == "-")
			{
				key = "MINUS";
			}
			if (key == "\t")
			{
				key = "TAB";
			}
			if (key == "#")
			{
				key = "TAB";
			}
			LogHelper.Instance.Log("TTS((> Key Sequence, adding:" + key);
			m_keySequence += key;
			ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
			if (m_keySequenceDelay > 0 && timerService != null)
			{
				timerService.RemoveTimer("TTSInputTimer");
				timerService.CreateTimer("TTSInputTimer", m_keySequenceDelay, null, delegate
				{
					timerService.RemoveTimer("TTSInputTimer");
					TTSFinalizeKeyEntry();
				}).Start();
			}
			else
			{
				TTSFinalizeKeyEntry();
			}
			IViewFrame viewFrame2 = (ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame())?.ViewFrame as IViewFrame;
			if (viewFrame2?.Scene?.Actors != null)
			{
				foreach (IActor actor2 in viewFrame2.Scene.Actors)
				{
					if (actor2?.WPFFrameworkElement is ITextToSpeechControl textToSpeechControl)
					{
						textToSpeechControl.HandleWPFHit();
						break;
					}
				}
			}
		};
		service.RegisterKeyPressHandler("TTS-KeyHandler", handler);
	}

	private void TTSFinalizeKeyEntry()
	{
		LogHelper.Instance.Log("TTS((> Final Key Sequence: " + m_keySequence);
		if (m_keyHandlers.ContainsKey(m_keySequence))
		{
			Delegate obj = m_keyHandlers[m_keySequence];
			m_keySequence = "";
			if ((object)obj != null)
			{
				obj.DynamicInvoke();
			}
			else
			{
				SayTTS("Sorry, there was a problem.  Please make your selection again.");
			}
		}
		else
		{
			SayTTS("Sorry, there was a problem.  Please make your selection again.");
		}
		m_keySequence = "";
	}

	private SpeechControl LoadSpeechControlFromResouce(string resourceName)
	{
		SpeechControl speechControl = null;
		LogHelper.Instance.Log("TTS((> Attempting to load XML Control markup for " + resourceName);
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service != null)
		{
			XmlNode xml = service.GetXml(resourceName);
			if (xml != null)
			{
				speechControl = new SpeechControl
				{
					Name = resourceName
				};
				XmlNodeList xmlNodeList = xml.SelectNodes("speechPart");
				if (xmlNodeList != null)
				{
					foreach (XmlNode item in xmlNodeList)
					{
						int attributeValue = item.GetAttributeValue("keySequenceDelay", 0);
						int attributeValue2 = item.GetAttributeValue("sequence", 0);
						string attributeValue3 = item.GetAttributeValue("language", "en-US");
						bool attributeValue4 = item.GetAttributeValue("loop", defaultValue: false);
						bool attributeValue5 = item.GetAttributeValue("auto_run", defaultValue: true);
						int attributeValue6 = item.GetAttributeValue("startPause", 0);
						int attributeValue7 = item.GetAttributeValue("endPause", 0);
						SpeechPart speechPart = new SpeechPart
						{
							SpeechControl = speechControl,
							KeySequenceDelay = attributeValue,
							Sequence = attributeValue2,
							Language = attributeValue3,
							Loop = attributeValue4,
							StartPause = attributeValue6,
							EndPause = attributeValue7,
							AutoRun = attributeValue5
						};
						ReadNeededMacros(item, speechPart);
						ReadRegularExpressions(item, speechPart);
						ReadText(item, speechPart);
						ReadEvents(item, speechPart);
						speechControl.SpeechParts.Add(speechPart);
					}
				}
				LogHelper.Instance.Log("TTS((> Loaded XML Control Markup for {0}. {1} speech parts into memory.", resourceName, speechControl.SpeechParts.Count.ToString());
			}
		}
		return speechControl;
	}

	private static void ReadNeededMacros(XmlNode part, SpeechPart speechPart)
	{
		XmlNode xmlNode = part.SelectSingleNode("macrosNeeded");
		if (xmlNode == null)
		{
			return;
		}
		XmlNodeList xmlNodeList = xmlNode.SelectNodes("value");
		if (xmlNodeList == null)
		{
			return;
		}
		foreach (XmlNode item2 in xmlNodeList)
		{
			string attributeValue = item2.GetAttributeValue<string>("default");
			NeededMacro item = new NeededMacro
			{
				Name = item2.InnerText,
				Default = attributeValue
			};
			speechPart.NeededMacros.Add(item);
		}
	}

	private static void ReadRegularExpressions(XmlNode part, SpeechPart speechPart)
	{
		XmlNode xmlNode = part.SelectSingleNode("regex");
		if (xmlNode != null)
		{
			XmlNode xmlNode2 = xmlNode.SelectSingleNode("value");
			XmlNode xmlNode3 = xmlNode.SelectSingleNode("expression");
			XmlNode xmlNode4 = xmlNode.SelectSingleNode("success");
			XmlNode xmlNode5 = xmlNode.SelectSingleNode("failure");
			if (xmlNode2 != null && xmlNode3 != null && xmlNode4 != null && xmlNode5 != null)
			{
				speechPart.RegularExpression = new RegularExpression
				{
					Expression = xmlNode3.InnerText,
					Value = xmlNode2.InnerText,
					Success = xmlNode4.InnerText,
					Failure = xmlNode5.InnerText
				};
			}
		}
	}

	private static void ReadText(XmlNode part, SpeechPart speechPart)
	{
		XmlNodeList xmlNodeList = part.SelectNodes("text");
		if (xmlNodeList == null)
		{
			return;
		}
		foreach (XmlNode item in xmlNodeList)
		{
			string attributeValue = item.GetAttributeValue<string>("textAvailable");
			string attributeValue2 = item.GetAttributeValue<string>("ifActorVisible");
			string attributeValue3 = item.GetAttributeValue<string>("ifControlEnabled");
			string innerText = item.InnerText;
			speechPart.Texts.Add(new TextPart
			{
				TextAvailable = attributeValue,
				IfActorVisible = attributeValue2,
				IfControlEnabled = attributeValue3,
				Text = innerText
			});
		}
	}

	private static void ReadEvents(XmlNode part, SpeechPart speechPart)
	{
		XmlNode xmlNode = part.SelectSingleNode("events");
		if (xmlNode == null)
		{
			return;
		}
		foreach (XmlNode item2 in xmlNode)
		{
			SpeechPartEvent item = null;
			string attributeValue = item2.GetAttributeValue<string>("keyCode");
			XmlNode xmlNode3 = item2.SelectSingleNode("function");
			XmlNode xmlNode4 = item2.SelectSingleNode("text");
			XmlNode xmlNode5 = item2.SelectSingleNode("value");
			string name = item2.Name;
			if (!(name == "mapKeyPress"))
			{
				if (name == "mapKeyPressToActorHit")
				{
					XmlNode xmlNode6 = item2.SelectSingleNode("actorName");
					XmlNode xmlNode7 = item2.SelectSingleNode("command");
					XmlNode xmlNode8 = item2.SelectSingleNode("parameter");
					item = new MapKeyPressToActorHit
					{
						KeyCode = attributeValue,
						Function = xmlNode3?.InnerText,
						Value = xmlNode5?.InnerText,
						Text = xmlNode4?.InnerText,
						ActorName = xmlNode6?.InnerText,
						Command = xmlNode7?.InnerText,
						Parameter = xmlNode8?.InnerText
					};
				}
			}
			else
			{
				item = new MapKeyPress
				{
					KeyCode = attributeValue,
					Function = xmlNode3?.InnerText,
					Value = xmlNode5?.InnerText,
					Text = xmlNode4?.InnerText
				};
			}
			speechPart.Events.Add(item);
		}
	}

	private void TTSVolumeUp()
	{
		m_speechVolume += 10;
		if (m_speechVolume > 100)
		{
			m_speechVolume = 100;
		}
	}

	private void TTSVolumeDown()
	{
		m_speechVolume -= 10;
		if (m_speechVolume < 0)
		{
			m_speechVolume = 0;
		}
	}

	private void TTSFaster()
	{
		m_speechRate++;
		if (m_speechRate > 10)
		{
			m_speechRate = 10;
		}
	}

	private void TTSSlower()
	{
		m_speechRate--;
		if (m_speechRate < -10)
		{
			m_speechRate = -10;
		}
	}

	private string ReplaceCommonText(string input)
	{
		if (string.IsNullOrEmpty(input) || m_textReplacement.Count <= 0)
		{
			return input;
		}
		m_textReplacement.ForEach(delegate(KeyValuePair<string, string> x)
		{
			input = input.ToLowerInvariant().Replace(x.Key, x.Value);
		});
		return input;
	}

	private void SayTTS(string speech)
	{
		SilenceTTS();
		speech = ReplaceCommonText(speech);
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				lock (m_speechSynthesizerLock)
				{
					if (!string.IsNullOrEmpty(speech) && AudioDeviceConnected)
					{
						m_reader = new SpeechSynthesizer
						{
							Rate = m_speechRate,
							Volume = m_speechVolume
						};
						LogHelper.Instance.Log("TTS((> Say: " + speech);
						m_reader.SpeakAsync(speech);
						m_reader.SpeakCompleted += ReaderSpeakCompleted;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("TTS((> An exception was raised in TextToSpeechService:SayTTS()", e);
			}
		});
	}

	private void SilenceTTS()
	{
		lock (m_speechSynthesizerLock)
		{
			if (m_reader != null)
			{
				m_reader.SpeakAsyncCancelAll();
				m_reader?.Dispose();
				m_reader = null;
			}
		}
	}

	private void RunNextSpeechPart()
	{
		if (_currentSpeechPart == null)
		{
			return;
		}
		ISpeechPart speechPart = _currentSpeechPart;
		if (!_currentSpeechPart.Loop && _speechControl.SpeechParts.Count > 1)
		{
			int num = _speechControl.SpeechParts.Max((ISpeechPart x) => x.Sequence);
			int minSequence = _speechControl.SpeechParts.Min((ISpeechPart x) => x.Sequence);
			if (_currentSpeechPart.Sequence == num)
			{
				speechPart = _speechControl.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Sequence == minSequence);
			}
			else
			{
				List<ISpeechPart> list = _speechControl.SpeechParts.Where((ISpeechPart x) => x.Sequence > _currentSpeechPart.Sequence).ToList();
				if (list.Count > 0)
				{
					int nextSpeechSequence = list.Min((ISpeechPart x) => x.Sequence);
					speechPart = _speechControl.SpeechParts.FirstOrDefault((ISpeechPart x) => x.Sequence == nextSpeechSequence);
				}
			}
		}
		RunSpeechPart(speechPart);
	}

	private void ReaderNothingLeftToSay()
	{
		int sequence = _currentSpeechPart.Sequence;
		LogHelper.Instance.Log("TTS((> Reader speak completed speech part #" + sequence);
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		if (service == null)
		{
			return;
		}
		service.RemoveTimer("TTSTimer");
		if (_currentSpeechPart == null)
		{
			return;
		}
		int endPause = _currentSpeechPart.EndPause;
		if (endPause > 0)
		{
			service.CreateTimer("TTSTimer", endPause, null, delegate
			{
				RunNextSpeechPart();
			}).Start();
		}
		else
		{
			RunNextSpeechPart();
		}
	}

	private void ReaderSpeakCompleted(object sender, SpeakCompletedEventArgs e)
	{
		try
		{
			if (m_textQueue.Count > 0)
			{
				SayTTS(m_textQueue.Dequeue());
			}
			else
			{
				ReaderNothingLeftToSay();
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("TTS((> An exception was raised in TextToSpeechService:SayTTS()", e2);
		}
	}

	private void ExecuteActorCommand(string actorName, string command, string parameter)
	{
		LogHelper.Instance.Log("TTS((> Execute Command requested: {0}, {1}, {2}", actorName, command, parameter);
		IActor actor = ((ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame())?.ViewFrame as IViewFrame)?.Scene?.GetActor(actorName);
		if (actor != null)
		{
			if (actor.WPFFrameworkElement != null)
			{
				if (actor.WPFFrameworkElement is ITextToSpeechControl textToSpeechControl)
				{
					LogHelper.Instance.Log("TTS((> Execute command: {0} parameter: {1}", command, parameter);
					textToSpeechControl.ExecuteCommand(command, parameter);
				}
				else
				{
					LogHelper.Instance.Log("TTS((> actor WPFFrameworkElement is not a ITextToSpeechControl.");
				}
			}
			else
			{
				LogHelper.Instance.Log("TTS((> actor does not have a WPFFrameworkElement.");
			}
		}
		else
		{
			LogHelper.Instance.Log("TTS((> actor NOT FOUND in scene.");
		}
	}

	private void ClickActor(string actorName)
	{
		LogHelper.Instance.Log("TTS((> Click Actor requested: " + actorName);
		try
		{
			IActor actor = ((ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame())?.ViewFrame as IViewFrame)?.Scene?.GetActor(actorName);
			if (actor == null)
			{
				LogHelper.Instance.Log("TTS((> actor NOT FOUND in scene.");
				return;
			}
			if (!actor.Visible)
			{
				LogHelper.Instance.Log("TTS((> actor is NOT VISIBLE.");
				return;
			}
			if (!actor.Enabled)
			{
				LogHelper.Instance.Log("TTS((> actor is NOT ENABLED.");
				return;
			}
			int num = FindWindow(null, "Redbox Kiosk Engine");
			if (num > 0)
			{
				RECT lprect = default(RECT);
				GetWindowRect(num, out lprect);
				int x = lprect.X + actor.X + (int)Math.Floor((double)(actor.Width / 2));
				int num2 = lprect.Y + actor.Y + (int)Math.Floor((double)(actor.Height / 2));
				if (Debugger.IsAttached)
				{
					num2 += 25;
				}
				Cursor.Position = new Point(x, num2);
				Thread.Sleep(10);
				MouseClick(x, num2);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("TTS((> ClickActor: exception - " + ex.Message);
		}
	}

	[DllImport("user32.dll")]
	private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

	[DllImport("user32.dll")]
	private static extern IntPtr FindWindow(string strClassName, int nptWindowName);

	[DllImport("user32.dll")]
	private static extern int FindWindow(string ClassName, string WindowName);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(int hWnd, out RECT lprect);

	private void MouseClick(int x, int y)
	{
		mouse_event(2, x, y, 0, 0);
		mouse_event(4, x, y, 0, 0);
	}
}
