using System;
using System.Collections.Generic;

namespace DeviceService.ComponentModel.Requests;

public interface ICardReadRequest
{
	decimal? Amount { get; }

	DeviceInputType InputType { get; }

	int Timeout { get; set; }

	Guid? SessionId { get; set; }

	IEnumerable<CardBrandAndSource> ExcludeCardBrandBySource { get; set; }

	VasMode VasMode { get; }

	string AppleVasUrl { get; set; }

	string GoogleVasUrl { get; set; }
}
