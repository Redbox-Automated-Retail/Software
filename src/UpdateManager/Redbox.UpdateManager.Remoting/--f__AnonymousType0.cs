using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public sealed class Redbox_002EUpdateManager_002ERemoting_002E_003C_003Ef__AnonymousType0<_003CResult_003Ej__TPar, _003CException_003Ej__TPar, _003CMessage_003Ej__TPar>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CResult_003Ej__TPar _003CResult_003Ei__Field;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CException_003Ej__TPar _003CException_003Ei__Field;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CMessage_003Ej__TPar _003CMessage_003Ei__Field;

	public _003CResult_003Ej__TPar Result => _003CResult_003Ei__Field;

	public _003CException_003Ej__TPar Exception => _003CException_003Ei__Field;

	public _003CMessage_003Ej__TPar Message => _003CMessage_003Ei__Field;

	[DebuggerHidden]
	public Redbox_002EUpdateManager_002ERemoting_002E_003C_003Ef__AnonymousType0(_003CResult_003Ej__TPar Result, _003CException_003Ej__TPar Exception, _003CMessage_003Ej__TPar Message)
	{
		_003CResult_003Ei__Field = Result;
		_003CException_003Ei__Field = Exception;
		_003CMessage_003Ei__Field = Message;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		if (value is Redbox_002EUpdateManager_002ERemoting_002E_003C_003Ef__AnonymousType0<_003CResult_003Ej__TPar, _003CException_003Ej__TPar, _003CMessage_003Ej__TPar> obj && EqualityComparer<_003CResult_003Ej__TPar>.Default.Equals(_003CResult_003Ei__Field, obj._003CResult_003Ei__Field) && EqualityComparer<_003CException_003Ej__TPar>.Default.Equals(_003CException_003Ei__Field, obj._003CException_003Ei__Field))
		{
			return EqualityComparer<_003CMessage_003Ej__TPar>.Default.Equals(_003CMessage_003Ei__Field, obj._003CMessage_003Ei__Field);
		}
		return false;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		return ((unchecked(-14392326 * -1521134295) + EqualityComparer<_003CResult_003Ej__TPar>.Default.GetHashCode(_003CResult_003Ei__Field)) * -1521134295 + EqualityComparer<_003CException_003Ej__TPar>.Default.GetHashCode(_003CException_003Ei__Field)) * -1521134295 + EqualityComparer<_003CMessage_003Ej__TPar>.Default.GetHashCode(_003CMessage_003Ei__Field);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		return string.Format(null, "{{ Result = {0}, Exception = {1}, Message = {2} }}", new object[3]
		{
			_003CResult_003Ei__Field?.ToString(),
			_003CException_003Ei__Field?.ToString(),
			_003CMessage_003Ei__Field?.ToString()
		});
	}
}
