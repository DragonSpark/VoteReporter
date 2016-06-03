using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DragonSpark.Runtime.Properties
{
	public abstract class ExecutionAttachedPropertyStoreBase<T> : DeferredAttachedPropertyTargetStore<object, T>
	{
		protected ExecutionAttachedPropertyStoreBase() : this( Delegates<T>.Default ) {}
		protected ExecutionAttachedPropertyStoreBase( Func<T> create ) : this( new AttachedProperty<T>( new Func<object, T>( new Context( create ).Create ) ) ) {}
		protected ExecutionAttachedPropertyStoreBase( IAttachedProperty<object, T> property ) : this( Execution.GetCurrent, property ) {}
		protected ExecutionAttachedPropertyStoreBase( Func<object> instance, IAttachedProperty<object, T> property ) : this( instance, property, Coercer<T>.Instance ) {}
		protected ExecutionAttachedPropertyStoreBase( Func<object> instance, IAttachedProperty<object, T> property, ICoercer<T> coercer ) : base( instance, property, coercer ) {}

		struct Context
		{
			readonly Func<T> create;
			public Context( Func<T> create )
			{
				this.create = create;
			}

			public T Create( object instance ) => create();
		}
	}

	public class Stack<T> : IStack<T>
	{
		readonly System.Collections.Generic.Stack<T> store;
		readonly Action<IStack<T>> onEmpty;
		public Stack() : this( Delegates<IStack<T>>.Empty ) {}

		public Stack( System.Collections.Generic.Stack<T> store ) : this( store, Delegates<IStack<T>>.Empty ) {}

		public Stack( Action<IStack<T>> onEmpty ) : this( new System.Collections.Generic.Stack<T>(), onEmpty ) {}

		public Stack( System.Collections.Generic.Stack<T> store, Action<IStack<T>> onEmpty )
		{
			this.store = store;
			this.onEmpty = onEmpty;
		}

		public bool Contains( T item ) => store.Contains( item );

		public ImmutableArray<T> All() => store.ToImmutableArray();

		public T Peek() => store.PeekOrDefault();

		public void Push( T item ) => store.Push( item );

		public T Pop()
		{
			var result = store.Pop();
			if ( !store.Any() )
			{
				onEmpty( this );
			}
			return result;
		}
	}

	public interface IPropertyRegistry<T>
	{
		void Register( object key, IAttachedProperty<T> instance );
		void Clear( object key, object instance );
	}

	class PropertyRegistry<T> : IPropertyRegistry<T>
	{
		public static PropertyRegistry<T> Instance { get; } = new PropertyRegistry<T>();

		readonly ConditionalWeakTable<object, IAttachedProperty<T>> cache = new ConditionalWeakTable<object, IAttachedProperty<T>>();

		public void Register( object key, IAttachedProperty<T> instance ) => cache.Add( key, instance );

		public void Clear( object key, object instance )
		{
			IAttachedProperty<T> property;
			if ( cache.TryGetValue( key, out property ) )
			{
				property.Clear( instance );
			}
		}
	}

	public static class AmbientStack
	{
		public static object GetCurrentItem( [Required]Type type ) => typeof(AmbientStack).Adapt().Invoke( nameof(GetCurrentItem), type.ToItem() );

		public static T GetCurrentItem<T>() => AmbientStack<T>.Instance.GetCurrentItem();

		// public static object GetCurrent( [Required]Type type ) => typeof(AmbientStack).Adapt().Invoke( nameof(GetCurrent), type.ToItem() );

		// static ImmutableArray<T> List<T>() => GetCurrent<T>().All();

		/*public static IStack<T> GetCurrent<T>() => AmbientStackProperty<T>.Default.Get( Execution.Current );*/
	}

	public class AmbientStack<T> : ExecutionAttachedPropertyStoreBase<IStack<T>>
	{
		public static AmbientStack<T> Instance { get; } = new AmbientStack<T>();

		public AmbientStack() : this( Execution.GetCurrent ) {}
		public AmbientStack( Func<object> host ) : this( host, AmbientStackProperty<T>.Default ) {}
		public AmbientStack( Func<object> host, IAttachedProperty<object, IStack<T>> property ) : base( host, property ) {}

		public T GetCurrentItem() => Value.Peek();
	}

	public class AmbientStackProperty<T> : ThreadLocalAttachedProperty<IStack<T>>
	{
		public static AmbientStackProperty<T> Default { get; } = new AmbientStackProperty<T>();

		public AmbientStackProperty() : this( PropertyRegistry<IStack<T>>.Instance ) {}

		protected AmbientStackProperty( IPropertyRegistry<IStack<T>> registry ) : this( registry, new Store( registry.Clear ) ) {}

		protected AmbientStackProperty( IPropertyRegistry<IStack<T>> registry, IAttachedPropertyStore<object, IStack<T>> store ) : base( store )
		{
			registry.Register( store, this );
		}

		public class Store : AttachedPropertyStoreBase<object, IStack<T>>
		{
			readonly Action<Store, object> callback;

			public Store( Action<Store, object> callback )
			{
				this.callback = callback;
			}

			public override IWritableStore<IStack<T>> Create( object instance ) => new Factory( this, instance ).Create();

			class Factory
			{
				readonly Store owner;
				readonly object instance;
				readonly ThreadLocalStore<IStack<T>> store;
				readonly ConcurrentDictionary<IStack<T>, bool> empty = new ConcurrentDictionary<IStack<T>, bool>();
				readonly ThreadLocal<IStack<T>> local;

				public Factory( Store owner, object instance )
				{
					this.owner = owner;
					this.instance = instance;

					local = new ThreadLocal<IStack<T>>( New, true );
					store = new ThreadLocalStore<IStack<T>>( local ).Configured( false );
				}

				public IWritableStore<IStack<T>> Create() => store;

				IStack<T> New() => new Stack<T>( OnEmpty );

				void OnEmpty( IStack<T> item )
				{
					// bool current;
					
					if ( empty.TryAdd( item, true ) && local.Values.All( IsEmpty ) )
					//if ( empty.TryGetValue( item, out current ) && empty.TryUpdate( item, true, current ) && empty.Values.All( b => b ) )
					{
						empty.Clear();
						owner.Clear( instance );
						store.Dispose();
					}
				}

				bool IsEmpty( IStack<T> stack )
				{
					bool isEmpty;
					return empty.ContainsKey( stack ) && empty.TryGetValue( stack, out isEmpty ) && isEmpty;
				}
			}

			void Clear( object instance ) => callback( this, instance );
		}
	}

	public class ThreadLocalStore<T> : WritableStore<T>
	{
		readonly ThreadLocal<T> local;

		public ThreadLocalStore( [Required]Func<T> create ) : this( new ThreadLocal<T>( create ) ) {}

		public ThreadLocalStore( ThreadLocal<T> local )
		{
			this.local = local;
		}

		public override void Assign( T item ) => local.Value = item;

		protected override T Get() => local.Value;

		protected override void OnDispose()
		{
			local.Dispose();
			
			base.OnDispose();
		}
	}

	public class EqualityReference<T> : DeferredAttachedPropertyTargetStore<object, ConcurrentDictionary<int, T>>
	{
		// public override bool IsAttached( object instance ) => property.Get( Execution.GetCurrent() ).ContainsKey( instance.GetHashCode() );

		// public void Set( T instance, T value ) => new Context( Value, instance ).Set();

		public T From( T instance ) => new Context( Value, instance ).Get();

		struct Context
		{
			readonly ConcurrentDictionary<int, T> store;
			readonly T item;

			public Context( ConcurrentDictionary<int, T> store, T item )
			{
				this.store = store;
				this.item = item;
			}

			T Add( int code ) => item;

			T Update( int code, T current ) => item;

			public T Get() => store.GetOrAdd( item.GetHashCode(), Add );

			public void Set() => store.AddOrUpdate( item.GetHashCode(), Add, Update );
		}

		public EqualityReference() : this( Execution.GetCurrent, new AttachedProperty<ConcurrentDictionary<int, T>>( ActivatedAttachedPropertyStore<ConcurrentDictionary<int, T>>.Instance ), Coercer<ConcurrentDictionary<int, T>>.Instance ) {}

		public EqualityReference( Func<object> instance, IAttachedProperty<object, ConcurrentDictionary<int, T>> property, ICoercer<ConcurrentDictionary<int, T>> coercer ) : base( instance, property, coercer ) {}
	}

	public class Condition : Condition<object>
	{
		public new static Condition Property { get; } = new Condition();
	}
	
	public class Condition<T> : AttachedProperty<T, ConditionMonitor> where T : class
	{
		public static Condition<T> Property { get; } = new Condition<T>();

		public Condition() : base( key => new ConditionMonitor() ) {}
	}
}