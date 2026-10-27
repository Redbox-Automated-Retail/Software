namespace Redbox.KioskEngine.ComponentModel;

public interface IWPFActor
{
	IActor Actor { get; set; }

	event WPFHitHandler OnWPFHit;
}
