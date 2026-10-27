namespace Redbox.Rental.Model.KioskClientService.Campaign;

internal class CarouselControl : Control, ICarouselControl, IControl
{
	public int MaxTitles { get; internal set; }

	public CarouselControl()
	{
		base.ControlType = ControlType.Carousel;
	}
}
