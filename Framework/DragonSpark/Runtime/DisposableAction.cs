using System;

namespace DragonSpark.Runtime
{
	public class DisposableAction : Disposable
	{
		readonly Action action;

		public DisposableAction( Action action )
		{
			this.action = action;
		}

		protected override void OnDispose( bool disposing ) => action();
	}

	public class InitializedDisposableAction : DisposableAction
	{
		public InitializedDisposableAction( Action action ) : base( action )
		{
			action();
		}
	}
}