namespace Redbox.HAL.Controller.Framework;

public enum CommandType
{
	[CommandProperties(Address = AddressSelector.H101, Command = "X", CommandWait = 60000)]
	Reset,
	[CommandProperties(Address = AddressSelector.H002, Command = "M1", StatusBit = 6, ResetCommand = "K1", WaitPauseTime = 500)]
	QlmEngage,
	[CommandProperties(Address = AddressSelector.H002, Command = "L1", StatusBit = 5, ResetCommand = "K1", WaitPauseTime = 500)]
	QlmDisengage,
	[CommandProperties(Address = AddressSelector.H002, Command = "K1")]
	QlmHalt,
	[CommandProperties(Address = AddressSelector.H002, Command = "P2")]
	QlmDoorLock,
	[CommandProperties(Address = AddressSelector.H002, Command = "O2")]
	QlmDoorUnlock,
	[CommandProperties(Address = AddressSelector.H001, Command = "O1")]
	SensorBarOn,
	[CommandProperties(Address = AddressSelector.H001, Command = "P1")]
	SensorBarOff,
	[CommandProperties(Address = AddressSelector.H101, Command = "I")]
	AudioOn,
	[CommandProperties(Address = AddressSelector.H101, Command = "J")]
	AudioOff,
	[CommandProperties(Address = AddressSelector.H001, Command = "J4")]
	RollerIn,
	[CommandProperties(Address = AddressSelector.H001, Command = "I4")]
	RollerOut,
	[CommandProperties(Address = AddressSelector.H001, Command = "K4")]
	RollerStop,
	[CommandProperties(Address = AddressSelector.H001, Command = "U1", StatusBit = 18, ResetCommand = "K4,P1", WaitPauseTime = 20)]
	RollerToPos1,
	[CommandProperties(Address = AddressSelector.H001, Command = "U2", StatusBit = 19, ResetCommand = "K4,P1", WaitPauseTime = 20)]
	RollerToPos2,
	[CommandProperties(Address = AddressSelector.H001, Command = "U3", StatusBit = 20, ResetCommand = "K4,P1", WaitPauseTime = 20)]
	RollerToPos3,
	[CommandProperties(Address = AddressSelector.H001, Command = "T4", StatusBit = 15, ResetCommand = "K4,P1", WaitPauseTime = 20)]
	RollerToPos4,
	[CommandProperties(Address = AddressSelector.H001, Command = "T5", StatusBit = 16, ResetCommand = "K4,P1", WaitPauseTime = 20)]
	RollerToPos5,
	[CommandProperties(Address = AddressSelector.H001, Command = "T6", StatusBit = 17, ResetCommand = "K4,P1", WaitPauseTime = 20)]
	RollerToPos6,
	[CommandProperties(Address = AddressSelector.H001, Command = "O4")]
	FraudSensorEnablePowerTransistor,
	[CommandProperties(Address = AddressSelector.H001, Command = "P4")]
	FraudSensorDisablePowerTransistor,
	[CommandProperties(Address = AddressSelector.H001, Command = "O3")]
	FraudSensorEnableTransistor,
	[CommandProperties(Address = AddressSelector.H001, Command = "P3")]
	FraudSensorDisableTransistor,
	[CommandProperties(Address = AddressSelector.H001, Command = "O2")]
	RinglightOn,
	[CommandProperties(Address = AddressSelector.H001, Command = "P2")]
	RinglightOff,
	[CommandProperties(Address = AddressSelector.H001, Command = "O1")]
	Junction4On,
	[CommandProperties(Address = AddressSelector.H001, Command = "P1")]
	Junction4Off,
	[CommandProperties(Address = AddressSelector.H002, Command = "O1")]
	AuxSensorsOn,
	[CommandProperties(Address = AddressSelector.H002, Command = "P1")]
	AuxSensorsOff,
	[CommandProperties(Address = AddressSelector.H002, Command = "R")]
	AuxSensorsRead,
	[CommandProperties(Address = AddressSelector.H002, Command = "M2", StatusBit = 8, ResetCommand = "K2", WaitPauseTime = 60)]
	VendDoorOpen,
	[CommandProperties(Address = AddressSelector.H002, Command = "V", StatusBit = 10, ResetCommand = "K2", WaitPauseTime = 60)]
	VendDoorRent,
	[CommandProperties(Address = AddressSelector.H002, Command = "L2", StatusBit = 7, ResetCommand = "K2", WaitPauseTime = 60)]
	VendDoorClose,
	[CommandProperties(Address = AddressSelector.H001, Command = "R")]
	ReadPickerInputs,
	[CommandProperties(Address = AddressSelector.H001, Command = "K1")]
	GripperExtendHalt,
	[CommandProperties(Address = AddressSelector.H001, Command = "I1")]
	ExtendGripperArmForTime,
	[CommandProperties(Address = AddressSelector.H001, Command = "L1", StatusBit = 5, ResetCommand = "K1", WaitPauseTime = 50)]
	GripperExtend,
	[CommandProperties(Address = AddressSelector.H001, Command = "M1", StatusBit = 6, ResetCommand = "K1", WaitPauseTime = 50)]
	GripperRetract,
	[CommandProperties(Address = AddressSelector.H001, Command = "M3", StatusBit = 10, ResetCommand = "K3", WaitPauseTime = 50)]
	GripperOpen,
	[CommandProperties(Address = AddressSelector.H001, Command = "V", StatusBit = 12, ResetCommand = "K3", WaitPauseTime = 50)]
	GripperRent,
	[CommandProperties(Address = AddressSelector.H001, Command = "L3", StatusBit = 9, ResetCommand = "K3", WaitPauseTime = 50)]
	GripperClose,
	[CommandProperties(Address = AddressSelector.H001, Command = "M2", StatusBit = 8, ResetCommand = "K2", WaitPauseTime = 50)]
	TrackOpen,
	[CommandProperties(Address = AddressSelector.H001, Command = "L2", StatusBit = 7, ResetCommand = "K2", WaitPauseTime = 50)]
	TrackClose,
	[CommandProperties(Address = AddressSelector.H101, Command = "Y")]
	Version101,
	[CommandProperties(Address = AddressSelector.H001, Command = "W")]
	Version001,
	[CommandProperties(Address = AddressSelector.H002, Command = "W")]
	Version002,
	[CommandProperties(Address = AddressSelector.H001, Command = "S")]
	Status001,
	[CommandProperties(Address = AddressSelector.H002, Command = "S")]
	Status002,
	[CommandProperties(Address = AddressSelector.H555, Command = "I1")]
	TurnOnGreenButtonLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "I3")]
	TurnOffGreenButtonLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "I2")]
	BlinkGreenButtonLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "R1")]
	TurnOnRedButtonLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "R3")]
	TurnOffRedButtonLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "R2")]
	BlinkRedButtonLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "T1")]
	TurnOnGreenArrowLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "T2")]
	BlinkGreenArrowLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "T3")]
	TurnOffGreenArrowLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "Z1")]
	TurnOnRedArrowLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "Z2")]
	BlinkRedArrowLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "Z3")]
	TurnOffRedArrowLed,
	[CommandProperties(Address = AddressSelector.H555, Command = "W1")]
	TurnOnBackLight,
	[CommandProperties(Address = AddressSelector.H555, Command = "W3")]
	TurnOffBackLight,
	[CommandProperties(Address = AddressSelector.H555, Command = "W2")]
	BlinkBackLight,
	[CommandProperties(Address = AddressSelector.H555, Command = "S{0}")]
	SendText,
	[CommandProperties(Address = AddressSelector.H555, Command = "X{0}")]
	ClearDisplayMemory,
	[CommandProperties(Address = AddressSelector.H555, Command = "Y")]
	SideTerminalVersion,
	[CommandProperties(Address = AddressSelector.H555, Command = "J")]
	ReadQRButton,
	[CommandProperties(Address = AddressSelector.H555, Command = "K")]
	ClearQRButtonStatus,
	[CommandProperties(Address = AddressSelector.H555, Command = "U90")]
	TurnOffPixels,
	[CommandProperties(Address = AddressSelector.H555, Command = "U94")]
	SetTextOnlyDisplayMode,
	[CommandProperties(Address = AddressSelector.H555, Command = "U98")]
	SetGraphicsOnlyDisplayMode,
	[CommandProperties(Address = AddressSelector.H555, Command = "U80")]
	DisplaySettingOR,
	[CommandProperties(Address = AddressSelector.H555, Command = "U81")]
	DisplaySettingEXOR,
	[CommandProperties(Address = AddressSelector.H555, Command = "U83")]
	DisplaySettingAND,
	[CommandProperties(Address = AddressSelector.H555, Command = "M{0}")]
	SetStartTextPagePointer,
	[CommandProperties(Address = AddressSelector.H555, Command = "L{0}")]
	SetStartGraphicsPagePointer,
	[CommandProperties(Address = AddressSelector.H555, Command = "V{0}")]
	SetMemoryWritePointer,
	[CommandProperties(Address = AddressSelector.H555, Command = "O{0}")]
	SetTextColumns,
	[CommandProperties(Address = AddressSelector.H555, Command = "N{0}")]
	SetGraphicsColumns,
	[CommandProperties(Address = AddressSelector.H555, Command = "g{0}")]
	WriteGraphicData,
	[CommandProperties(Address = AddressSelector.H555, Command = "l{0}")]
	WriteGraphicDataToEEPROM,
	[CommandProperties(Address = AddressSelector.H555, Command = "d{0}")]
	SetEEPROMPointer,
	[CommandProperties(Address = AddressSelector.H555, Command = "e{0}")]
	SetEEPROMReadSize,
	[CommandProperties(Address = AddressSelector.H555, Command = "m")]
	LoadFromEEPROMToDisplayMemory,
	[CommandProperties(Address = AddressSelector.H555, Command = "c")]
	TerminalRevision,
	[CommandProperties(Address = AddressSelector.H002, Command = "J2")]
	UnknownVendDoorCloseCommand,
	[CommandProperties(Address = AddressSelector.H002, Command = "I2")]
	UnknownVendDoorOpenCommand,
	[CommandProperties(Address = AddressSelector.H002, Command = "K2")]
	VendDoorKillCommand,
	[CommandProperties(Address = AddressSelector.H002, Command = "O3")]
	PowerAux20,
	[CommandProperties(Address = AddressSelector.H002, Command = "P3")]
	DisableAux20,
	[CommandProperties(Address = AddressSelector.H002, Command = "O4")]
	PowerAux21,
	[CommandProperties(Address = AddressSelector.H002, Command = "P4")]
	DisableAux21,
	[CommandProperties(Address = AddressSelector.H002, Command = "M1")]
	QlmLift,
	[CommandProperties(Address = AddressSelector.H002, Command = "L1")]
	QlmDrop
}
