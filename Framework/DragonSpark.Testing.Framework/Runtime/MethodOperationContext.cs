using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using System;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Runtime
{
	public sealed class MethodOperationContext : InitializedDisposableAction
	{
		readonly static Action Run = PurgeLoggerMessageHistoryCommand.Default.Fixed( Output.Default.Get ).Run;

		readonly IDisposable disposable;

		public MethodOperationContext( MethodBase method ) : this( TimedOperationFactory.Default.Get( method ) ?? new Disposable() ) {}

		public MethodOperationContext( IDisposable disposable ) : base( Run )
		{
			this.disposable = disposable;
		}

		protected override void OnDispose( bool disposing )
		{
			disposable.Dispose();
			base.OnDispose( disposing );
		}
	}
}