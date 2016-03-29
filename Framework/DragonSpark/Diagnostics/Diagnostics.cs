using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using System;
using System.Diagnostics;

namespace DragonSpark.Diagnostics
{
	public class Diagnostics : IDiagnostics
	{
		public Diagnostics( [Required] ILogger logger, [Required] LoggingLevelSwitch levelSwitch )
		{
			Logger = logger;
			Switch = levelSwitch;
		}

		public ILogger Logger { get;  }

		public LoggingLevelSwitch Switch { get; }

		/*public IEnumerable<LogEvent> Events => sink.Events;

		public void Purge( Action<string> writer ) => LogEventMessageFactory.Instance.Create( sink ).Each( writer );*/

		/*public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		[Freeze]
		protected virtual void OnDispose() {}*/
	}

	public static class ExceptionSupport
	{
		public static Exception Try( Action action ) => Try( Services.Get<TryContext>, action );

		public static Exception Try( this Func<TryContext> @this, Action action ) => @this().Try( action );

		public static void Process( this IExceptionHandler target, Exception exception ) => target.Handle( exception ).With( a => a.RethrowRecommended.IsTrue( () => { throw a.Exception; } ) );
	}

	// [Disposable( ThrowObjectDisposedException = true )]
	public class Profiler : IProfiler
	{
		readonly ILogger logger;

		readonly Tracker tracker = new Tracker();

		public Profiler( [Required] ILogger logger, [NotEmpty] string context )
		{
			this.logger = logger.ForContext( Constants.SourceContextPropertyName, context );
		}

		public void Start()
		{
			Mark( "Starting" );
			tracker.Initialize();
		}

		public void Mark( string @event ) => logger.Information( "@ {Time:ss':'fff} ({Since:ss':'fff}): {Event:l}", tracker.Time, tracker.Mark(), @event );

		class Tracker : IDisposable
		{
			readonly Stopwatch watcher = Stopwatch.StartNew();

			readonly FixedValue<TimeSpan> last = new FixedValue<TimeSpan>();

			public void Initialize() => Reset( watcher.Restart );

			void Reset( Action action )
			{
				last.Assign( default(TimeSpan) );
				action();
			}

			public TimeSpan Time => watcher.Elapsed;

			public TimeSpan Mark()
			{
				var result = watcher.Elapsed - last.Item;
				last.Assign( watcher.Elapsed );
				return result;
			}

			public void Dispose() => Reset( watcher.Stop );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		[Freeze]
		protected virtual void OnDispose() => Mark( "Complete" );
	}
}