using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RBA_SDK_ComponentModel;

namespace RBA_SDK_Simulator;

/// <summary>
/// Drives the simulated terminal: queue customer actions, inspect state, and change the device's condition.
/// DeviceService.WebApi adds this assembly as an MVC application part, so these routes are served by the
/// device service itself (e.g. http://localhost:9000/api/Simulator).
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SimulatorController : ControllerBase
{
	public class CardRequest
	{
		/// <summary>A preset name (Visa, Mastercard, Amex, Discover, RedboxGiftCard). Ignored when Card is set.</summary>
		public string Preset { get; set; }

		public SimulatedCard Card { get; set; }

		public bool? Decline { get; set; }

		public bool? MobileWallet { get; set; }

		public bool? Encrypted { get; set; }
	}

	private readonly ISimulatorDataProvider _data;

	private readonly ICommandQueue _commands;

	private readonly RBA_API_Simulator _terminal;

	public SimulatorController(ISimulatorDataProvider data, ICommandQueue commands, IRBA_API terminal)
	{
		_data = data;
		_commands = commands;
		_terminal = terminal as RBA_API_Simulator;
	}

	[HttpGet]
	public ActionResult<object> GetState()
	{
		return new
		{
			Connected = _terminal?.IsConnected ?? false,
			_data.DeviceAttached,
			_data.Tampered,
			PendingCardRead = _terminal?.PendingRead,
			CardInserted = _terminal?.IsCardInserted ?? false,
			EmvTransaction = _terminal?.EmvTransactionState,
			QueuedActions = _commands.Pending,
			_data.DefaultCard,
			_data.ResponseDelay,
			_data.Configuration,
			_data.Variables,
			Files = _data.Files.Keys
		};
	}

	[HttpGet("History")]
	public ActionResult<IReadOnlyList<string>> GetHistory()
	{
		return new ActionResult<IReadOnlyList<string>>(_terminal?.History ?? new string[0]);
	}

	[HttpGet("Cards")]
	public ActionResult<IReadOnlyList<SimulatedCard>> GetCards()
	{
		return new ActionResult<IReadOnlyList<SimulatedCard>>(SimulatedCard.Presets);
	}

	[HttpPost("Swipe")]
	public IActionResult Swipe([FromBody] CardRequest request = null)
	{
		return Queue(SimulatorCommandType.Swipe, request);
	}

	[HttpPost("Insert")]
	public IActionResult Insert([FromBody] CardRequest request = null)
	{
		return Queue(SimulatorCommandType.InsertChip, request);
	}

	[HttpPost("Tap")]
	public IActionResult Tap([FromBody] CardRequest request = null)
	{
		return Queue(SimulatorCommandType.Tap, request);
	}

	[HttpPost("TapMsd")]
	public IActionResult TapMsd([FromBody] CardRequest request = null)
	{
		return Queue(SimulatorCommandType.TapMsd, request);
	}

	[HttpPost("Remove")]
	public IActionResult Remove()
	{
		return Queue(SimulatorCommandType.RemoveCard, null);
	}

	[HttpPost("Cancel")]
	public IActionResult Cancel()
	{
		return Queue(SimulatorCommandType.Cancel, null);
	}

	[HttpPost("BadRead")]
	public IActionResult BadRead()
	{
		return Queue(SimulatorCommandType.BadRead, null);
	}

	[HttpDelete("Queue")]
	public IActionResult ClearQueue()
	{
		_commands.Clear();
		return Ok();
	}

	[HttpPut("DefaultCard")]
	public IActionResult SetDefaultCard([FromBody] CardRequest request)
	{
		_data.DefaultCard = ResolveCard(request);
		return Ok(_data.DefaultCard);
	}

	[HttpPut("Tampered/{tampered}")]
	public IActionResult SetTampered(bool tampered)
	{
		_data.Tampered = tampered;
		return Ok();
	}

	[HttpPut("ResponseDelay/{milliseconds}")]
	public IActionResult SetResponseDelay(int milliseconds)
	{
		_data.ResponseDelay = milliseconds < 0 ? 0 : milliseconds;
		return Ok();
	}

	/// <summary>Unplug the terminal: the device service is notified and reconnects fail until Attach.</summary>
	[HttpPost("Detach")]
	public IActionResult Detach()
	{
		_terminal?.SimulateDisconnect();
		return Ok();
	}

	[HttpPost("Attach")]
	public IActionResult Attach()
	{
		_terminal?.SimulateAttach();
		return Ok();
	}

	[HttpPut("Config/{group}/{index}")]
	public IActionResult SetConfig(string group, string index, [FromBody] string value)
	{
		_data.WriteConfig(group, index, value);
		return Ok();
	}

	[HttpPut("Variable/{id}")]
	public IActionResult SetVariable(string id, [FromBody] string value)
	{
		_data.SetVariable(id, value);
		return Ok();
	}

	private IActionResult Queue(SimulatorCommandType type, CardRequest request)
	{
		SimulatorCommand simulatorCommand = new SimulatorCommand
		{
			Type = type,
			Card = (request == null) ? null : ResolveCard(request)
		};
		_commands.Enqueue(simulatorCommand);
		return Accepted(new
		{
			Queued = simulatorCommand.ToString(),
			Note = "Runs when the device service starts a card read (or immediately for Remove when a card is inserted)."
		});
	}

	private SimulatedCard ResolveCard(CardRequest request)
	{
		SimulatedCard simulatedCard = request.Card?.Clone() ?? (string.IsNullOrEmpty(request.Preset) ? _data.DefaultCard.Clone() : SimulatedCard.Preset(request.Preset));
		if (request.Decline.HasValue)
		{
			simulatedCard.Decline = request.Decline.Value;
		}
		if (request.MobileWallet.HasValue)
		{
			simulatedCard.MobileWallet = request.MobileWallet.Value;
		}
		if (request.Encrypted.HasValue)
		{
			simulatedCard.Encrypted = request.Encrypted.Value;
		}
		return simulatedCard;
	}
}
