namespace Redbox.HAL.Component.Model;

public interface IHardwareCorrectionObserver
{
	void HardwareCorrectionStart(HardwareCorrectionEventArgs e);

	void HardwareCorrectionEnd(HardwareCorrectionEventArgs e);
}
