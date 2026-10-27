using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Redbox.NetCore.Logging.Extensions;

public static class SerilogExtensions
{
	public static IWebHostBuilder UseSerilogLogging(this IWebHostBuilder builder)
	{
		SerilogWebHostBuilderExtensions.UseSerilog(builder, (Action<WebHostBuilderContext, LoggerConfiguration>)delegate(WebHostBuilderContext ctx, LoggerConfiguration logger)
		{
			ctx.Configuration.CheckForMinimumRequiredSerilogConfiguration();
			logger.ReadFrom.Configuration(ctx.Configuration);
		}, false, false);
		return builder;
	}

	public static IHostBuilder UseSerilogLogging(this IHostBuilder builder)
	{
		builder.UseSerilog(delegate(HostBuilderContext ctx, LoggerConfiguration logger)
		{
			ctx.Configuration.CheckForMinimumRequiredSerilogConfiguration();
			logger.ReadFrom.Configuration(ctx.Configuration);
		});
		return builder;
	}

	private static void CheckForMinimumRequiredSerilogConfiguration(this IConfiguration configuration)
	{
		if (configuration.GetSection("Serilog").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog' configuration file section is missing!");
		}
		if (configuration.GetSection("Serilog:Using").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:Using' configuration file section is missing!");
		}
		if (configuration.GetSection("Serilog:Using").GetChildren().DoesNotExist((IConfigurationSection value) => value.Value == "Serilog.Sinks.Console"))
		{
			throw new ConfigurationErrorsException("'Serilog:Using' configuration file section is missing Serilog.Sinks.Console value!");
		}
		if (configuration.GetSection("Serilog:MinimumLevel").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:MinimumLevel' configuration file section is missing!");
		}
		if (configuration.GetSection("Serilog:MinimumLevel:Default").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:MinimumLevel:Default' configuration file value is missing!");
		}
		if (configuration.GetSection("Serilog:MinimumLevel:Override").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:MinimumLevel:Override' configuration file section is missing!");
		}
		if (configuration.GetSection("Serilog:MinimumLevel:Override:System").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:MinimumLevel:Override:System' configuration file value is missing!");
		}
		if (configuration.GetSection("Serilog:MinimumLevel:Override:Microsoft").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:MinimumLevel:Override:Microsoft' configuration file value is missing!");
		}
		if (configuration.GetSection("Serilog:MinimumLevel:Override:Microsoft.AspNetCore.Mvc.Internal").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:MinimumLevel:Override:Microsoft.AspNetCore.Mvc.Internal' configuration file value is missing!");
		}
		if (configuration.GetSection("Serilog:Enrich").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:Enrich' configuration file section is missing!");
		}
		if (configuration.GetSection("Serilog:Enrich").GetChildren().DoesNotExist((IConfigurationSection value) => value.Value == "FromLogContext"))
		{
			throw new ConfigurationErrorsException("'Serilog:Enrich' configuration file section is missing FromLogContext value!");
		}
		if (configuration.GetSection("Serilog:Enrich").GetChildren().DoesNotExist((IConfigurationSection value) => value.Value == "WithThreadId"))
		{
			throw new ConfigurationErrorsException("'Serilog:Enrich' configuration file section is missing WithThreadId value!");
		}
		if (configuration.GetSection("Serilog:WriteTo").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:WriteTo' configuration file section is missing!");
		}
		if (configuration.GetSection("Serilog:WriteTo:0").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:WriteTo' configuration file section is missing first section!");
		}
		if (configuration.GetSection("Serilog:WriteTo:0:Name").DoesNotExist())
		{
			throw new ConfigurationErrorsException("'Serilog:WriteTo' configuration file first section is missing Name value!");
		}
		if (configuration.GetSection("Serilog:WriteTo:0:Name").Value != "Async")
		{
			throw new ConfigurationErrorsException("'Serilog:WriteTo' configuration file first section is not the Async section!");
		}
	}

	private static bool DoesNotExist(this IConfigurationSection section)
	{
		return !section.Exists();
	}

	private static bool DoesNotExist(this IEnumerable<IConfigurationSection> enumerable, Predicate<IConfigurationSection> match)
	{
		return !enumerable.ToList().Exists(match);
	}
}
