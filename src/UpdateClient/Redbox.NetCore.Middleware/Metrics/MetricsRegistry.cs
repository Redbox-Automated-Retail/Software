using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.ReservoirSampling.SlidingWindow;

namespace Redbox.NetCore.Middleware.Metrics;

public static class MetricsRegistry
{
	public static readonly CounterOptions Errors = new CounterOptions
	{
		MeasurementUnit = Unit.Errors,
		ResetOnReporting = true,
		Name = "Errors"
	};

	public static readonly CounterOptions ResponseCodes = new CounterOptions
	{
		MeasurementUnit = Unit.Calls,
		ResetOnReporting = true,
		Name = "ResponseCodes"
	};

	public static readonly HistogramOptions ResponseTimes = new HistogramOptions
	{
		MeasurementUnit = Unit.Requests,
		Reservoir = () => new DefaultSlidingWindowReservoir(250),
		Name = "ResponseTimes"
	};

	public static readonly CounterOptions ExternalErrors = new CounterOptions
	{
		MeasurementUnit = Unit.Errors,
		ResetOnReporting = true,
		Name = "ExternalErrors"
	};

	public static readonly CounterOptions ExternalRequests = new CounterOptions
	{
		MeasurementUnit = Unit.Requests,
		ResetOnReporting = true,
		Name = "ExternalRequests"
	};

	public static readonly HistogramOptions ExternalResponseTimes = new HistogramOptions
	{
		MeasurementUnit = Unit.Requests,
		Reservoir = () => new DefaultSlidingWindowReservoir(250),
		Name = "ExternalResponseTimes"
	};

	public static readonly CounterOptions InvalidRequests = new CounterOptions
	{
		MeasurementUnit = Unit.Requests,
		ResetOnReporting = true,
		Name = "InvalidRequests"
	};

	public static readonly CounterOptions DbExecutions = new CounterOptions
	{
		MeasurementUnit = Unit.Requests,
		ResetOnReporting = true,
		Name = "DbExecutions"
	};

	public static readonly HistogramOptions DbExecutionTimes = new HistogramOptions
	{
		MeasurementUnit = Unit.Requests,
		Reservoir = () => new DefaultSlidingWindowReservoir(250),
		Name = "DbExecutionTimes"
	};

	public static readonly CounterOptions DbExecutionErrors = new CounterOptions
	{
		MeasurementUnit = Unit.Requests,
		ResetOnReporting = true,
		Name = "DbExecutionErrors"
	};

	public static readonly GaugeOptions GenericGauge = new GaugeOptions
	{
		MeasurementUnit = Unit.Items,
		ResetOnReporting = true,
		Name = "GenericGauge"
	};

	public static readonly CounterOptions GenericCounter = new CounterOptions
	{
		MeasurementUnit = Unit.Requests,
		ResetOnReporting = true,
		Name = "GenericCounter"
	};

	public static readonly HistogramOptions GenericHistogram = new HistogramOptions
	{
		MeasurementUnit = Unit.Requests,
		Reservoir = () => new DefaultSlidingWindowReservoir(250),
		Name = "GenericHistogram"
	};
}
