using System;
using DragonSpark.Extensions;

namespace DragonSpark.Diagnostics
{
	public class CompositeLogger : ILogger
	{
		readonly ILogger[] loggers;

		public CompositeLogger( params ILogger[] loggers )
		{
			this.loggers = loggers;
		}

		public void Information( string message, Priority priority )
		{
			loggers.Each( logger => logger.Information( message, priority ) );
		}

		public void Warning( string message, Priority priority )
		{
			loggers.Each( logger => logger.Warning( message, priority ) );
		}

		public void Exception( string message, Exception exception )
		{
			loggers.Each( logger => logger.Exception( message, exception ) );
		}

		public void Fatal( string message, Exception exception )
		{
			loggers.Each( logger => logger.Fatal( message, exception ) );
		}
	}
}