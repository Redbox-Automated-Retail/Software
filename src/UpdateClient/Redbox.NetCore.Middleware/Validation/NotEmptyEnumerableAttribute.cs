using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Redbox.NetCore.Middleware.Validation;

public class NotEmptyEnumerableAttribute : ValidationAttribute
{
	public const string DefaultErrorMessage = "The {0} field must not be null and have at least one element";

	public NotEmptyEnumerableAttribute()
		: base("The {0} field must not be null and have at least one element")
	{
	}

	public override bool IsValid(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!(value is IEnumerable enumerable))
		{
			return false;
		}
		IEnumerator enumerator = enumerable.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return true;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		return false;
	}
}
