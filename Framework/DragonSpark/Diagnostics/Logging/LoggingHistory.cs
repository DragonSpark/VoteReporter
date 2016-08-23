using DragonSpark.Sources;

namespace DragonSpark.Diagnostics.Logging
{
	public sealed class LoggingHistory : Scope<LoggerHistorySink>
	{
		public static LoggingHistory Default { get; } = new LoggingHistory();
		LoggingHistory() : base( Factory.Global( () => new LoggerHistorySink() ) ) {}
	}
}