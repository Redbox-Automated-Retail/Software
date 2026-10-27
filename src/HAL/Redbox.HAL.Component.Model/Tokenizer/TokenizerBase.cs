using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Redbox.HAL.Component.Model.Tokenizer;

public abstract class TokenizerBase<T>
{
	internal delegate StateResult StateHandler();

	protected readonly ITokenReader m_tokenReader;

	private readonly ErrorList m_errors = new ErrorList();

	private readonly IDictionary<T, StateHandler> m_handlers = new Dictionary<T, StateHandler>();

	public ErrorList Errors => m_errors;

	protected internal StringBuilder Accumulator { get; private set; }

	protected internal T CurrentState { get; set; }

	protected TokenizerBase(Stream tokenStream)
		: this()
	{
		m_tokenReader = new StreamTokenReader(tokenStream);
	}

	protected TokenizerBase(int lineNumber, string statement)
		: this()
	{
		m_tokenReader = new StringTokenReader(lineNumber, statement);
	}

	protected TokenizerBase()
	{
		BuildStateHandlerDictionary();
	}

	public void Tokenize()
	{
		Reset();
		StartStateMachine();
	}

	protected internal void StartStateMachine()
	{
		if (m_tokenReader.IsEmpty())
		{
			return;
		}
		while (true)
		{
			StateHandler obj = (m_handlers.ContainsKey(CurrentState) ? m_handlers[CurrentState] : null);
			if (obj == null)
			{
				break;
			}
			switch (obj())
			{
			case StateResult.Terminal:
				StopMachine();
				return;
			case StateResult.Continue:
				if (!MoveToNextToken())
				{
					StopMachine();
					return;
				}
				break;
			}
		}
		throw new ArgumentException("No handler found where CurrentState = " + CurrentState);
	}

	protected internal string FormatError(string message)
	{
		return $"(Line: {m_tokenReader.Row} Column: {m_tokenReader.Column}) {message}";
	}

	protected internal void Reset()
	{
		m_errors.Clear();
		ResetAccumulator();
		m_tokenReader.Reset();
		OnReset();
	}

	protected internal bool MoveToNextToken()
	{
		return m_tokenReader.MoveToNextToken();
	}

	protected internal char? PeekNextToken()
	{
		return m_tokenReader.PeekNextToken();
	}

	protected internal char? PeekNextToken(int i)
	{
		return m_tokenReader.PeekNextToken(i);
	}

	protected internal char GetCurrentToken()
	{
		return m_tokenReader.GetCurrentToken();
	}

	protected internal void ResetAccumulator()
	{
		Accumulator = new StringBuilder();
	}

	protected internal void AppendToAccumulator()
	{
		Accumulator.Append(GetCurrentToken());
	}

	protected internal string GetAccumulatedToken()
	{
		return Accumulator.ToString();
	}

	protected virtual void OnReset()
	{
	}

	protected virtual void OnEndOfStream()
	{
	}

	private void BuildStateHandlerDictionary()
	{
		m_handlers.Clear();
		MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			StateHandlerAttribute stateHandlerAttribute = (StateHandlerAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(StateHandlerAttribute));
			if (stateHandlerAttribute != null)
			{
				m_handlers[(T)stateHandlerAttribute.State] = (StateHandler)Delegate.CreateDelegate(typeof(StateHandler), this, methodInfo.Name);
			}
		}
	}

	private void StopMachine()
	{
		OnEndOfStream();
		if (Accumulator.Length > 0)
		{
			throw new Exception($"State Machine Terminated with [{Accumulator}] left in the buffer.");
		}
	}
}
