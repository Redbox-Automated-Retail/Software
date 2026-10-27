using System;
using System.Collections.Generic;

namespace KioskClientService.API.Services.ProfileData;

[UpdateDataTable(UpdateDataTable = UpdateDataTable.Product)]
public class Product
{
	public long ProductId { get; set; }

	public ProductGroup ProductGroup { get; set; } = new ProductGroup();

	public string Duration { get; set; }

	public List<string> Starring { get; set; }

	public string ProductType { get; set; }

	public Formats Format { get; set; }

	public string Rating { get; set; }

	public string LongName { get; set; }

	public string SortName { get; set; }

	public string ImageFile { get; set; }

	public string Description { get; set; }

	public List<string> Genres { get; set; }

	public DateTime? ReleaseDate { get; set; }

	public bool SellThru { get; set; }

	public bool SellThruNew { get; set; }

	public decimal BoxOfficeGross { get; set; }

	public List<string> Directors { get; set; }

	public DateTime? ComingSoonDate { get; set; }

	public DateTime? SortDate { get; set; }

	public DateTime? NationalStreetDate { get; set; }

	public bool? ClosedCaptioned { get; set; }

	public DateTime? MerchandiseDate { get; set; }

	public string ProductWebPageURL { get; set; }
}
