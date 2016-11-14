﻿using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Sources.Scopes;
using JetBrains.Annotations;
using PostSharp;
using PostSharp.Extensibility;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DragonSpark.Aspects
{
	public class LoggingSink : ILogEventSink
	{
		public static LoggingSink Default { get; } = new LoggingSink();
		LoggingSink() : this( MessageFactory.Default.Get, MessageSource.MessageSink.Write ) {}

		readonly Func<LogEvent, Message> source;
		readonly Action<Message> write;

		public LoggingSink( Func<LogEvent, Message> source, Action<Message> write )
		{
			this.source = source;
			this.write = write;
		}

		public void Emit( LogEvent logEvent ) => source( logEvent ).With( write );
	}

	sealed class LogMessageTemplate : LogCommandBase<string>
	{
		public LogMessageTemplate( ILogger logger ) : base( logger, "" ) {}
	}

	public sealed class MessageFactory : ParameterizedSourceBase<LogEvent, Message>
	{
		public static MessageFactory Default { get; } = new MessageFactory();
		MessageFactory() : this( LevelMappings.Default.Get, HashFactory.Default.Get ) {}

		readonly Func<LogEventLevel, SeverityType> mappings;
		readonly Alter<string> hasher;
		
		[UsedImplicitly]
		public MessageFactory( Func<LogEventLevel, SeverityType> mappings, Alter<string> hasher )
		{
			this.mappings = mappings;
			this.hasher = hasher;
		}

		public override Message Get( LogEvent parameter )
		{
			var source = parameter.Properties.ContainsKey( Constants.SourceContextPropertyName ) ? parameter.Properties[Constants.SourceContextPropertyName].As<ScalarValue>().With( MessageLocation.Of ) : null;
			var messageId = hasher( parameter.MessageTemplate.Text );
			var text = parameter.RenderMessage();
			var level = mappings( parameter.Level );
			var result = new Message( source ?? MessageLocation.Unknown, level, messageId, text, null, null, parameter.Exception );
			return result;
		}
	}

	sealed class LevelMappings : DictionaryCache<LogEventLevel, SeverityType>
	{
		public static LevelMappings Default { get; } = new LevelMappings();
		LevelMappings() : base( new Dictionary<LogEventLevel, SeverityType>
								{
									{ LogEventLevel.Verbose, SeverityType.Verbose },
									{ LogEventLevel.Debug, SeverityType.Debug },
									{ LogEventLevel.Information, SeverityType.Info },
									{ LogEventLevel.Warning, SeverityType.Warning },
									{ LogEventLevel.Error, SeverityType.Error },
									{ LogEventLevel.Fatal, SeverityType.Fatal },
								}.ToImmutableDictionary() ) {}
	}

	public interface IHasher : IAlteration<ImmutableArray<byte>> {}
	public sealed class Hasher : ParameterizedSingletonScope<ImmutableArray<byte>, ImmutableArray<byte>>, IHasher
	{
		public static Hasher Default { get; } = new Hasher();
		Hasher() : base( bytes => Hash.GetFnvHashCode( bytes ).With( hash => ImmutableArray.Create( (byte)(hash >> 24), (byte)(hash >> 16), (byte)(hash >> 8), (byte)hash ) ) ) {}
	}

	sealed class HashFactory : AlterationBase<string>
	{
		public static HashFactory Default { get; } = new HashFactory();
		HashFactory() : this( Hasher.Default ) {}

		readonly IHasher hasher;
		
		public HashFactory( IHasher hasher )
		{
			this.hasher = hasher;
		}

		public override string Get( string parameter )
		{
			var hash = hasher.Get( Encoding.UTF8.GetBytes( parameter ) );
			var builder = new StringBuilder();
			foreach ( var item in hash )
			{
				builder.Append( item.ToString( "X2" ) );
			}

			var result = builder.ToString();
			return result;
		}
	}
}
