using System;
using System.Data.SqlTypes;

namespace Redbox.NAnt.TimeKeeper.Tasks;

public class DateTimePrimitive : Primitive
{
	public static DateTimePrimitive Now = null;

	private DateTime _data;

	public DateTime Data => _data;

	public static DateTime GetNow()
	{
		if (Now != null)
		{
			return Now._data;
		}
		return DateTime.Now;
	}

	public DateTimePrimitive(object datePrimitive)
	{
		_data = Convert.ToDateTime(datePrimitive);
	}

	public override string ToString()
	{
		return _data.ToString();
	}

	public static implicit operator DateTime(DateTimePrimitive aDate)
	{
		return aDate._data;
	}

	public static implicit operator DateTimePrimitive(DateTime aDate)
	{
		return new DateTimePrimitive(aDate);
	}

	public static TimeSpan operator -(DateTimePrimitive left, DateTimePrimitive right)
	{
		return (DateTime)left - (DateTime)right;
	}

	public virtual SqlDateTime ToSqlDateTime()
	{
		return _data;
	}

	public override bool Equals(object obj)
	{
		if (obj is SqlDateTime)
		{
			SqlDateTime sqlDateTime = (SqlDateTime)obj;
			return sqlDateTime.Equals(ToSqlDateTime());
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
