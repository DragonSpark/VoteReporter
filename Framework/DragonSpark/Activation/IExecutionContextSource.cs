using DragonSpark.Runtime;
using DragonSpark.Runtime.Sources;
using System;

namespace DragonSpark.Activation
{
	public static class Execution
	{
		public static IAssignableSource<ISource> Context { get; } = new FixedSource<ISource>( ExecutionContext.Instance );

		readonly static Func<object> Get = Context.Delegate();

		public static object Current() => Get();
	}

	[Priority( Priority.Low )]
	class ExecutionContext : SourceBase<object>
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();
		ExecutionContext() {}

		public override object Get() => this;
	}
}