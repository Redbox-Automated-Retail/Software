using System;

namespace Redbox.Rental.Services.App;

public class PersistentDataWrapper<T> where T : IPersistentData
{
	public string DataType { get; set; }

	public DateTime Modified { get; set; }

	public T Data { get; set; }
}
