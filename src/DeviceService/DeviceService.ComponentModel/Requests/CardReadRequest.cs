using System;
using System.Collections.Generic;

namespace DeviceService.ComponentModel.Requests;

public class CardReadRequest : ICardReadRequest
{
	public decimal? Amount { get; set; }

	public DeviceInputType InputType { get; set; }

	public int Timeout { get; set; }

	public Guid? SessionId { get; set; }

	public IEnumerable<CardBrandAndSource> ExcludeCardBrandBySource { get; set; }

	public VasMode VasMode { get; set; }

	public string AppleVasUrl { get; set; }

	public string GoogleVasUrl { get; set; }
}
