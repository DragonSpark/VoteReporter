using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Runtime.Values
{
	public static class ValueExtensions
	{
		public static T Assigned<T, U>( this T @this, U value ) where T : IWritableStore<U>
		{
			@this.Assign( value );
			return @this;
		}
	}

	public abstract class WritableStore<T> : StoreBase<T>, IWritableStore<T>, IDisposable
	{
		readonly ICoercer<T> coercer;
		// readonly Func<object, T> projection;

		protected WritableStore() : this( Coercer<T>.Instance ) {}

		protected WritableStore( ICoercer<T> coercer )
		{
			this.coercer = coercer;
		}

		/*protected WritableStore( Func<object, T> projection )
		{
			this.projection = projection;
		}*/

		public abstract void Assign( T item );

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		protected virtual void OnDispose() {}

		void IWritableStore.Assign( object item ) => Assign( coercer.Coerce( item ) );
	}

	/*public class ExecutionAssociatedStore<T> : AssociatedStore<T>
	{
		public ExecutionAssociatedStore( object instance, Func<T> create = null ) : base( instance, create ) {}
	}*/

	public class DeferredInstanceStore<T> : StoreBase<T>
	{
		readonly Lazy<T> lazy;

		public DeferredInstanceStore( Func<T> factory ) : this( new Lazy<T>( factory ) ) {}

		public DeferredInstanceStore( Lazy<T> lazy )
		{
			this.lazy = lazy;
		}

		protected override T Get() => lazy.Value;
	}

	public class DeferredStore<T> : WritableStore<T>
	{
		readonly Func<IWritableStore<T>> deferred;
		
		public DeferredStore( [Required]Func<IWritableStore<T>> deferred )
		{
			this.deferred = deferred;
		}

		public override void Assign( T item ) => deferred.Use( value => value.Assign( item ) );

		protected override T Get() => deferred.Use( value => value.Value );
	}

	public class DecoratedStore<T> : WritableStore<T>
	{
		readonly IWritableStore<T> inner;

		public DecoratedStore( [Required]IWritableStore<T> inner )
		{
			this.inner = inner;
		}

		public override void Assign( T item ) => inner.Assign( item );

		protected override T Get() => inner.Value;

		protected override void OnDispose()
		{
			inner.TryDispose();
			base.OnDispose();
		}
	}
}