using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "change-audio-channel-state", Operand = "CHANGE-AUDIO-CHANNEL-STATE")]
internal sealed class ChangeAudioChannelStateJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		AudioChannelState audio = base.Context.PopTop<AudioChannelState>();
		if (base.ControlSystem.SetAudio(audio).Success)
		{
			base.Context.CreateInfoResult("AudioChangeSuccessful", $"Changed audio channel state to {audio.ToString().ToUpper()}");
			return;
		}
		base.Context.CreateInfoResult("AudioChangeFailure", "Failed to change channel state.");
		AddError("STATE CHANGE FAILURE");
	}

	internal ChangeAudioChannelStateJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
