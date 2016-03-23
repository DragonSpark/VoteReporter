﻿using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using Serilog;
using Serilog.Events;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	public class MessageRecorderTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Message( RecordingLogEventSink sut, string message )
		{
			var logger = new LoggerConfiguration().WriteTo.Sink( sut ).CreateLogger();

			logger.Information( message );

			var item = sut.Events.Only();
			Assert.NotNull( item );

			Assert.Equal( LogEventLevel.Information, item.Level );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Fatal( RecordingLogEventSink sut, string message, FatalApplicationException error )
		{
			var logger = new LoggerConfiguration().WriteTo.Sink( sut ).CreateLogger();

			logger.Fatal( error, message );

			var item = sut.Events.Only();
			Assert.NotNull( item );

			Assert.Equal( LogEventLevel.Fatal, item.Level );
			Assert.Contains( message, item.RenderMessage() );
			Assert.Equal( error, item.Exception );
		}
	}
}