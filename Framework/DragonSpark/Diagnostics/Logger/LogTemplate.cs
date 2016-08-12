using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Diagnostics.Logger
{
	public delegate void LogTemplate( string template, params object[] parameters );

	public delegate void LogException( Exception exception, string template, params object[] parameters );

	public abstract class LogCommandBase<TDelegate, TTemplate> : CommandBase<TTemplate> where TTemplate : ILoggerTemplate
	{
		readonly ILogger logger;
		readonly Func<TTemplate, LogEventLevel> levelSource;
		readonly Func<TTemplate, object[]> parameterSource;

		protected LogCommandBase( ILogger logger, Func<TTemplate, LogEventLevel> levelSource, Func<TTemplate, object[]> parameterSource )
		{
			this.logger = logger;
			this.levelSource = levelSource;
			this.parameterSource = parameterSource;
		}

		public override void Execute( TTemplate parameter )
		{
			var parameters = parameterSource( parameter );
			var level = levelSource( parameter );
			var method = typeof(ILogger).GetRuntimeMethod( level.ToString(), parameters.Select( o => o.GetType() ).ToArray() );
			var @delegate = method.CreateDelegate( typeof(TDelegate), logger );
			@delegate.DynamicInvoke( parameters );
		}
	}

	public class LogCommand : FirstCommand<ILoggerTemplate>
	{
		public LogCommand( ILogger logger ) : base( new LogExceptionCommand( logger ), new LogTemplateCommand( logger ) ) {}
	}

	abstract class LoggerTemplateParameterFactoryBase<T> : ParameterizedSourceBase<T, object[]> where T : ILoggerTemplate
	{
		// public static TemplateParameterFactoryBase Instance { get; } = new TemplateParameterFactoryBase<T>();

		readonly FormatterFactory source;

		protected LoggerTemplateParameterFactoryBase() : this( FormatterFactory.Instance ) {}

		protected LoggerTemplateParameterFactoryBase( FormatterFactory source )
		{
			this.source = source;
		}

		protected object[] Parameters( T parameter ) => parameter.Parameters.Select( source.From ).ToArray();
	}

	class LoggerTemplateParameterFactory : LoggerTemplateParameterFactoryBase<ILoggerTemplate>
	{
		public static LoggerTemplateParameterFactory Instance { get; } = new LoggerTemplateParameterFactory();

		public override object[] Get( ILoggerTemplate parameter ) => new object[] { parameter.Template, Parameters( parameter ) };
	}

	class LoggerExceptionTemplateParameterFactory : LoggerTemplateParameterFactoryBase<ILoggerExceptionTemplate>
	{
		public static LoggerExceptionTemplateParameterFactory Instance { get; } = new LoggerExceptionTemplateParameterFactory();

		public override object[] Get( ILoggerExceptionTemplate parameter ) => new object[] { parameter.Exception, parameter.Template, Parameters( parameter ) };
	}

	public class LogTemplateCommand : LogCommandBase<LogTemplate, ILoggerTemplate>
	{
		public LogTemplateCommand( ILogger logger ) : this( logger, template => template.IntendedLevel ) {}
		public LogTemplateCommand( ILogger logger, LogEventLevel level ) : this( logger, template => level ) {}
		public LogTemplateCommand( ILogger logger, Func<ILoggerTemplate, LogEventLevel> levelSource ) : base( logger, levelSource, LoggerTemplateParameterFactory.Instance.Get ) {}
	}

	public class LogExceptionCommand : LogCommandBase<LogException, ILoggerExceptionTemplate>
	{
		public LogExceptionCommand( ILogger logger ) : this( logger, template => template.IntendedLevel ) {}
		public LogExceptionCommand( ILogger logger, LogEventLevel level ) : this( logger, template => level ) {}

		public LogExceptionCommand( ILogger logger, Func<ILoggerTemplate, LogEventLevel> levelSource ) : base( logger, levelSource, LoggerExceptionTemplateParameterFactory.Instance.Get ) {}
	}

	public sealed class Logging : ConfigurableParameterizedFactoryBase<LoggerConfiguration, ILogger>
	{
		public static IParameterizedSource<ILogger> Instance { get; } = new Logging().ToCache();
		Logging() : base( o => new LoggerConfiguration(), LoggerConfigurationSource.Instance.Get, ( configuration, parameter ) => configuration.CreateLogger().ForSource( parameter ) ) {}
	}

	public sealed class LoggingHistory : ActivatedCache<LoggerHistorySink>
	{
		public new static LoggingHistory Instance { get; } = new LoggingHistory();
		LoggingHistory() {}
	}

	public sealed class LoggingController : Cache<LoggingLevelSwitch>
	{
		public static LoggingController Instance { get; } = new LoggingController();
		LoggingController() : base( o => new LoggingLevelSwitch( MinimumLevelConfiguration.Instance.Get() ) ) {}
	}

	sealed class LoggerConfigurationSource : LoggerConfigurationSourceBase
	{
		public static LoggerConfigurationSource Instance { get; } = new LoggerConfigurationSource();

		protected override IEnumerable<ITransformer<LoggerConfiguration>> Yield( object parameter )
		{
			foreach ( var transformer in base.Yield( parameter ) )
			{
				yield return transformer;
			}

			yield return new HistoryTransform( LoggingHistory.Instance.Get( parameter ) );
		}

		class HistoryTransform : TransformerBase<LoggerConfiguration>
		{
			readonly ILoggerHistory history;

			public HistoryTransform( ILoggerHistory history )
			{
				this.history = history;
			}

			public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( history );
		}
	}

	public abstract class LoggerConfigurationSourceBase : ConfigurationSourceBase<object, LoggerConfiguration>
	{
		protected override IEnumerable<ITransformer<LoggerConfiguration>> Yield( object parameter )
		{
			yield return new ControllerTransform( LoggingController.Instance.Get( parameter ) );
			yield return new CreatorFilterTransformer();
		}

		sealed class ControllerTransform : TransformerBase<LoggerConfiguration>
		{
			readonly LoggingLevelSwitch controller;
			public ControllerTransform( LoggingLevelSwitch controller )
			{
				this.controller = controller;
			}

			public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.MinimumLevel.ControlledBy( controller );
		}
	}

	public sealed class CreatorFilterTransformer : TransformerBase<LoggerConfiguration>
	{
		readonly CreatorFilter filter;

		public CreatorFilterTransformer() : this( new CreatorFilter() ) {}

		public CreatorFilterTransformer( CreatorFilter filter )
		{
			this.filter = filter;
		}

		public override LoggerConfiguration Get( LoggerConfiguration parameter )
		{
			var item = filter.ToItem();
			var result = parameter.Filter.With( item ).Enrich.With( item );
			return result;
		}

		public sealed class CreatorFilter : DecoratedSpecification<LogEvent>, ILogEventEnricher, ILogEventFilter
		{
			const string CreatorId = "CreatorId";
			readonly Guid id;

			public CreatorFilter() : this( Guid.NewGuid() ) {}

			public CreatorFilter( Guid id ) : base( new CreatorSpecification( id ) )
			{
				this.id = id;
			}
			
			public void Enrich( LogEvent logEvent, ILogEventPropertyFactory propertyFactory ) => logEvent.AddPropertyIfAbsent( propertyFactory.CreateProperty( CreatorId, id ) );

			/*class MigratingSpecification : CacheValueSpecification<LogEvent, bool>
			{
				public static MigratingSpecification Instance { get; } = new MigratingSpecification();

				MigratingSpecification() : base( MigrationProperties.IsMigrating, () => true ) {}
			}*/

			sealed class CreatorSpecification : SpecificationBase<LogEvent>
			{
				readonly Guid id;
				public CreatorSpecification( Guid id )
				{
					this.id = id;
				}

				public override bool IsSatisfiedBy( LogEvent parameter )
				{
					if ( parameter.Properties.ContainsKey( CreatorId ) )
					{
						var property = parameter.Properties[CreatorId] as ScalarValue;
						var result = property != null && (Guid)property.Value == id;
						return result;
					}

					return false;
				}
			}

			public bool IsEnabled( LogEvent logEvent ) => IsSatisfiedBy( logEvent );
		}
	}
}