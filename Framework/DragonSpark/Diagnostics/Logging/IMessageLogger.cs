using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Markup;

namespace DragonSpark.Diagnostics.Logging
{
	[ContentProperty( nameof(Commands) )]
	public class ConfiguringLoggerConfigurationFactory : TransformerBase<LoggerConfiguration>
	{
		public DeclarativeCollection<CommandBase<LoggerConfiguration>> Commands { get; } = new DeclarativeCollection<CommandBase<LoggerConfiguration>>();

		public override LoggerConfiguration Get( LoggerConfiguration configuration ) => Commands.Aggregate( configuration, ( loggerConfiguration, command ) => loggerConfiguration.With( command.Execute ) );
	}

	public class AddTextWriterCommand : AddSinkCommand
	{
		public AddTextWriterCommand() : this( new StringWriter(), "{Timestamp} [{Level}] {Message}{NewLine}{Exception}" ) {}

		public AddTextWriterCommand( [Required]TextWriter writer, string outputTemplate )
		{
			Writer = writer;
			OutputTemplate = outputTemplate;
		}

		public TextWriter Writer { get; set; }

		public string OutputTemplate { get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => configuration.TextWriter( Writer, RestrictedToMinimumLevel, OutputTemplate, FormatProvider );
	}

	public abstract class MinimumLevelCommandBase : LoggerConfigurationCommandBase<LoggerMinimumLevelConfiguration>
	{
		protected MinimumLevelCommandBase() : base( configuration => configuration.MinimumLevel ) {}
	}

	public class MinimumLevelSwitchCommand : MinimumLevelCommandBase
	{
		public MinimumLevelSwitchCommand() : this( LogEventLevel.Information ) {}

		public MinimumLevelSwitchCommand( LogEventLevel level ) : this( new Serilog.Core.LoggingLevelSwitch( level ) ) {}

		public MinimumLevelSwitchCommand( [Required]Serilog.Core.LoggingLevelSwitch controller )
		{
			Controller = controller;
		}

		[Required]
		public Serilog.Core.LoggingLevelSwitch Controller { [return: Required]get; set; }

		protected override void Configure( LoggerMinimumLevelConfiguration configuration ) => configuration.ControlledBy( Controller );
	}

	public abstract class ReadFromCommandBase : LoggerConfigurationCommandBase<LoggerSettingsConfiguration>
	{
		protected ReadFromCommandBase() : base( configuration => configuration.ReadFrom ) {}
	}

	public class ReadFromSettingsCommand : ReadFromCommandBase
	{
		public ReadFromSettingsCommand( [Required]ILoggerSettings settings )
		{
			Settings = settings;
		}

		[Required]
		public ILoggerSettings Settings { [return: Required]get; set; }

		protected override void Configure( LoggerSettingsConfiguration configuration ) => configuration.Settings( Settings );
	}

	public class ReadFromKeyValuePairsCommand : ReadFromCommandBase
	{
		public ReadFromKeyValuePairsCommand() : this( new Dictionary<string, string>() ) {}

		public ReadFromKeyValuePairsCommand( [Required]IDictionary<string, string> dictionary )
		{
			Dictionary = dictionary;
		}

		[Required]
		public IDictionary<string, string> Dictionary { [return: Required]get; set; }

		protected override void Configure( LoggerSettingsConfiguration configuration ) => configuration.KeyValuePairs( Dictionary );
	}

	public abstract class DestructureCommandBase : LoggerConfigurationCommandBase<LoggerDestructuringConfiguration>
	{
		protected DestructureCommandBase() : base( configuration => configuration.Destructure ) {}
	}

	/*public class DestructureMethodCommand : DestructureByFactoryCommand<MethodInfo>
	{
		public static DestructureMethodCommand Instance { get; } = new DestructureMethodCommand();

		public DestructureMethodCommand() : base( MethodFormatter.Instance ) {}
	}*/

	public abstract class DestructureByFactoryCommand<TParameter> : DestructureCommandBase
	{
		protected DestructureByFactoryCommand( IParameterizedSource<TParameter, object> factory )
		{
			Factory = factory;
		}

		public IParameterizedSource<TParameter, object> Factory { get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.ByTransforming( Factory.ToSourceDelegate() );
	}

	[ContentProperty( nameof(Policies) )]
	public class DestructureCommand : DestructureCommandBase
	{
		public DeclarativeCollection<IDestructuringPolicy> Policies { get; } = new DeclarativeCollection<IDestructuringPolicy>();

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.With( Policies.Fixed() );
	}

	public class DestructureTypeCommand : DestructureCommandBase
	{
		[Required]
		public Type ScalarType { [return: Required]get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.AsScalar( ScalarType );
	}

	public class DestructureMaximumDepthCommand : DestructureCommandBase
	{
		[Range( 0, 100 )]
		public int MaximumDepth { get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.ToMaximumDepth( MaximumDepth );
	}

	public abstract class DestructureByTransformingCommandBase<T> : DestructureCommandBase
	{
		protected DestructureByTransformingCommandBase( [Required]IParameterizedSource<T, object> source )
		{
			Source = source;
		}

		[Required]
		public IParameterizedSource<T, object> Source { [return: Required]get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.ByTransforming<T>( /*Factory.IsSatisfiedBy,*/ Source.Get );
	}

	public abstract class FilterCommandBase : LoggerConfigurationCommandBase<LoggerFilterConfiguration>
	{
		protected FilterCommandBase() : base( configuration => configuration.Filter ) {}
	}

	[ContentProperty( nameof(Items) )]
	public class FilterCommand : FilterCommandBase
	{
		public DeclarativeCollection<ILogEventFilter> Items { get; } = new DeclarativeCollection<ILogEventFilter>();

		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.With( Items.Fixed() );
	}

	public class FilterByIncludingOnlyCommand : FilterBySpecificationCommandBase
	{
		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.ByIncludingOnly( Specification.IsSatisfiedBy );
	}

	public abstract class FilterBySpecificationCommandBase : FilterCommandBase
	{
		[Required]
		public ISpecification<LogEvent> Specification { [return: Required]get; set; }
	}

	public class FilterByExcludingCommand : FilterBySpecificationCommandBase
	{
		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.ByExcluding( Specification.IsSatisfiedBy );
	}

	[ContentProperty( nameof(Items) )]
	public class EnrichCommand : EnrichCommandBase
	{
		public DeclarativeCollection<ILogEventEnricher> Items { get; } = new DeclarativeCollection<ILogEventEnricher>();
		
		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.With( EnumerableExtensions.Fixed( Items ) );
	}

	public class EnrichWithPropertyCommand : EnrichCommandBase
	{
		public string PropertyName { get; set; }

		public object Value { get; set; }

		public bool DestructureObjects { get; set; }

		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.WithProperty( PropertyName, Value, DestructureObjects );
	}

	public abstract class EnrichCommandBase : LoggerConfigurationCommandBase<LoggerEnrichmentConfiguration>
	{
		protected EnrichCommandBase() : base( configuration => configuration.Enrich ) {}
	}

	public class MinimumLevelIsCommand : MinimumLevelCommandBase
	{
		public MinimumLevelIsCommand() : this( LogEventLevel.Information ) {}

		public MinimumLevelIsCommand( LogEventLevel level )
		{
			Level = level;
		}

		public LogEventLevel Level { get; set; }

		protected override void Configure( LoggerMinimumLevelConfiguration configuration ) => configuration.Is( Level );
	}

	public abstract class LoggerConfigurationCommandBase<T> : CommandBase<LoggerConfiguration>
	{
		readonly Func<LoggerConfiguration, T> projection;

		protected LoggerConfigurationCommandBase( Func<LoggerConfiguration, T> projection )
		{
			this.projection = projection;
		}

		public sealed override void Execute( LoggerConfiguration parameter ) => Configure( projection( parameter ) );

		protected abstract void Configure( T configuration );
	}
}