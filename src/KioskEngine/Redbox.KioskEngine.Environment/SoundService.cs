using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class SoundService : ISoundService
{
	public static SoundService Instance => Singleton<SoundService>.Instance;

	public void Reset()
	{
		LogHelper.Instance.Log("Reset sound service.");
		StopPlay();
	}

	public void PlaySync(byte[] buffer)
	{
		SimpleSound.Play(buffer, SoundFlags.SND_MEMORY);
	}

	public void PlaySync(string fileName)
	{
		SimpleSound.Play(fileName, SoundFlags.SND_FILENAME);
	}

	public void PlayAsync(byte[] buffer)
	{
		SimpleSound.Play(buffer, SoundFlags.SND_ASYNC | SoundFlags.SND_MEMORY);
	}

	public void PlayAsync(string fileName)
	{
		SimpleSound.Play(fileName, SoundFlags.SND_ASYNC | SoundFlags.SND_FILENAME);
	}

	public bool StopPlay()
	{
		return SimpleSound.StopPlay();
	}

	private SoundService()
	{
	}
}
