﻿using DragonSpark.Sources;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Logging
{
	public class MinimumLevelConfiguration : Scope<LogEventLevel>
	{
		public static MinimumLevelConfiguration Instance { get; } = new MinimumLevelConfiguration();
		MinimumLevelConfiguration() : base( () => LogEventLevel.Information ) {}
	}

	public class ProfilerLevelConfiguration : Scope<LogEventLevel>
	{
		public static ProfilerLevelConfiguration Instance { get; } = new ProfilerLevelConfiguration();
		ProfilerLevelConfiguration() : base( () => LogEventLevel.Debug ) {}
	}
}