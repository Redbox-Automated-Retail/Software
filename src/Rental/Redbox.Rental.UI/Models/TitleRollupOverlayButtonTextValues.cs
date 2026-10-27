using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.UI.Models;

public class TitleRollupOverlayButtonTextValues : ITitleRollupFormatButtonTextValues
{
	private string _titleRollupOverlayUnavailableText;

	private string _titleRollupOverlayCartText;

	private string _titleRollupOverlayOosText;

	private string _titleRollupOverlayAddText;

	private string _titleRollupOverlayBuyText;

	private string _currencySymbol;

	public string Unavailable
	{
		get
		{
			if (_titleRollupOverlayUnavailableText == null)
			{
				_titleRollupOverlayUnavailableText = Resources.title_rollup_overlay_unavailable_text;
			}
			return _titleRollupOverlayUnavailableText;
		}
	}

	public string InCartFormat
	{
		get
		{
			if (_titleRollupOverlayCartText == null)
			{
				_titleRollupOverlayCartText = Resources.title_rollup_overlay_cart_text;
			}
			return _titleRollupOverlayCartText;
		}
	}

	public string OutOfStockFormat
	{
		get
		{
			if (_titleRollupOverlayOosText == null)
			{
				_titleRollupOverlayOosText = Resources.title_rollup_overlay_oos_text;
			}
			return _titleRollupOverlayOosText;
		}
	}

	public string Rent
	{
		get
		{
			if (_titleRollupOverlayAddText == null)
			{
				_titleRollupOverlayAddText = Resources.title_rollup_overlay_add_text;
			}
			return _titleRollupOverlayAddText;
		}
	}

	public string Buy
	{
		get
		{
			if (_titleRollupOverlayBuyText == null)
			{
				_titleRollupOverlayBuyText = Resources.title_rollup_overlay_buy_text;
			}
			return _titleRollupOverlayBuyText;
		}
	}

	public string CurrencySymbol
	{
		get
		{
			if (_currencySymbol == null)
			{
				_currencySymbol = Resources.currency_symbol;
			}
			return _currencySymbol;
		}
	}

	public string OutOfStockText => Resources.title_rollup_overlay_oos_text;

	public string InCartText => Resources.title_rollup_overlay_cart_text;
}
