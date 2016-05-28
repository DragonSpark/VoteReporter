﻿using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using System;
using System.Threading;
using System.Windows.Input;

namespace DragonSpark.Runtime
{
	/*public interface IParameterWorkflowState
	{
		void Activate( object parameter, bool on );

		void Validate( object parameter, bool on );

		bool IsActive( object parameter );

		bool IsValidated( object parameter );
	}*/

	/*public class Assignment : Assignment<object, bool>
	{
		public Assignment( Action<object, bool> assign, object parameter )
			: base( assign, Assignments.From( parameter ), new Value<bool>( true ) ) {}
	}*/

	public class Disposable : IDisposable
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		~Disposable()
		{
			Dispose( false );
		}

		void IDisposable.Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => monitor.Apply( OnDispose );

		protected virtual void OnDispose() {}
	}

	public static class Assignments
	{
		public static Value<T> From<T>( T item ) => new Value<T>( item, item );
	}

	public struct Value<T>
	{
		public Value( T start ) : this( start, Default<T>.Item ) {}

		public Value( T start, T finish )
		{
			Start = start;
			Finish = finish;
		}

		public T Start { get; }
		public T Finish { get; }
	}

	public struct PropertyAssignment<T1, T2> : IAssign<T1, T2> where T1 : class
	{
		readonly IAttachedProperty<T1, T2> property;
		public PropertyAssignment( IAttachedProperty<T1, T2> property )
		{
			this.property = property;
		}

		public void Assign( T1 first, T2 second ) => property.Set( first, second );
	}

	public interface IAssign<in T1, in T2>
	{
		void Assign( T1 first, T2 second );
	}

	struct DelegatedAssign<T1, T2> : IAssign<T1, T2>
	{
		readonly Action<T1, T2> assign;
		public DelegatedAssign( Action<T1, T2> assign )
		{
			this.assign = assign;
		}

		public void Assign( T1 first, T2 second ) => assign( first, second );
	}

	struct EnabledStateAssign : IAssign<object, bool>
	{
		EnabledState value;

		public EnabledStateAssign( EnabledState value )
		{
			this.value = value;
		}

		public void Assign( object first, bool second ) => value.Enable( first, second );
	}

	// [Disposable]
	public struct Assignment<T, T1, T2> : IDisposable where T : IAssign<T1, T2>
	{
		readonly T assign;
		readonly Value<T1> first;
		readonly Value<T2> second;

		public Assignment( T assign, T1 first, T2 second ) : this( assign, new Value<T1>( first ), new Value<T2>( second ) ) {}

		public Assignment( T assign, Value<T1> first, Value<T2> second )
		{
			this.assign = assign;
			this.first = first;
			this.second = second;

			assign.Assign( first.Start, second.Start );
		}

		// protected override void OnDispose() => assign.Assign( first.Finish, second.Finish );
		public void Dispose() => assign.Assign( first.Finish, second.Finish );
	}

	/*public class Assignment<T> : Disposable
	{
		readonly Action<T> assign;
		readonly Value<T> first;

		public Assignment( Action<T> assign, T first ) : this( assign, new Value<T>( first ) ) {}

		public Assignment( Action<T> assign, Value<T> first )
		{
			this.assign = assign;
			this.first = first;

			assign( first.Start );
		}

		protected override void OnDispose() => assign( first.Finish );
	}*/

	public struct BitwiseValue : IEquatable<BitwiseValue>
	{
		int value;

		public int Value => value;

		public int If( bool condition, int bit ) => condition ? Add( bit ) : Remove( bit );

		public int Add( int bit )
		{
			Interlocked.CompareExchange( ref value, value | bit, value );
			return value;
		}

		public int Remove( int bit )
		{
			Interlocked.CompareExchange( ref value, value & ~bit, value );
			return Value;
		}

		public bool Contains( int bit ) => ( value & bit ) == bit;

		public bool Equals( BitwiseValue other ) => value == other.value;

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( obj is BitwiseValue && Equals( (BitwiseValue)obj ) );

		public override int GetHashCode() => Value;

		public static bool operator ==( BitwiseValue left, BitwiseValue right ) => left.Equals( right );

		public static bool operator !=( BitwiseValue left, BitwiseValue right ) => !left.Equals( right );
	}

	public class ParameterState /*: IParameterWorkflowState*/
	{
		public ParameterState() : this( new EnabledState( new BitwiseValue() ), new EnabledState( new BitwiseValue() ) ) {}

		public ParameterState( EnabledState active, EnabledState valid )
		{
			Active = active;
			Valid = valid;
		}

		/*public ParameterWorkflowState()
		{
			Active = new BitwiseValue();
		}*/

		public EnabledState Active { get; }

		public EnabledState Valid { get; }

		/*readonly static object Null = new object();

		int active, valid;

		public void Activate( object parameter, bool on ) => Set( ref active, parameter, @on );

		public bool IsActive( object parameter ) => Get( active, parameter );

		public void Validate( object parameter, bool on ) => Set( ref valid, parameter, @on );

		public bool IsValidated( object parameter ) => Get( valid, parameter );

		static void Set( ref int store, object parameter, bool on )
		{
			var @checked = ( parameter ?? Null ).GetHashCode();
			if ( on )
			{
				store |= @checked;
			}
			else
			{
				store &= ~@checked;
			}
		}

		static bool Get( int store, object parameter )
		{
			var code = ( parameter ?? Null ).GetHashCode();
			return ( store & code ) == code;
		}*/
	}

	public class EnabledState
	{
		readonly static object Null = new object();

		BitwiseValue store;

		public EnabledState( BitwiseValue store )
		{
			this.store = store;
		}

		static int GetCode( object item ) => ( item ?? Null ).GetHashCode();

		public bool IsEnabled( object item ) => store.Contains( GetCode( item ) );

		public void Enable( object item, bool on )
		{
			var code = GetCode( item );
			store.If( @on, code );
		}
	}

	public interface IParameterValidator
	{
		bool IsValid( object parameter );

		// object Execute( object parameter );
	}

	public class FactoryAdapter<TParameter, TResult> : IParameterValidator
	{
		readonly IFactory<TParameter, TResult> inner;
		public FactoryAdapter( IFactory<TParameter, TResult> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanCreate( (TParameter)parameter );

		// public object Execute( object parameter ) => inner.Create( (TParameter)parameter );
	}

	public class FactoryAdapter : IParameterValidator
	{
		readonly IFactoryWithParameter factory;

		public FactoryAdapter( IFactoryWithParameter factory )
		{
			this.factory = factory;
		}

		public bool IsValid( object parameter ) => factory.CanCreate( parameter );

		// public object Execute( object parameter ) => factory.Create( parameter );
	}

	public class CommandAdapter : IParameterValidator
	{
		readonly ICommand inner;
		public CommandAdapter( ICommand inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanExecute( parameter );

		/*public object Execute( object parameter )
		{
			inner.Execute( parameter );
			return null;
		}*/
	}

	public class CommandAdapter<T> : IParameterValidator
	{
		readonly ICommand<T> inner;
		public CommandAdapter( ICommand<T> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanExecute( (T)parameter );

		/*public object Execute( object parameter )
		{
			inner.Execute( (T)parameter );
			return null;
		}*/
	}

	public interface IInvocation<out T>
	{
		T Invoke( object parameter );
	}

	struct RelayInvocation : IInvocation<bool>
	{
		readonly RelayParameter valid;

		public RelayInvocation( RelayParameter valid )
		{
			this.valid = valid;
		}

		public bool Invoke( object parameter ) => valid.Proceed<bool>();
	}

	public struct AdapterInvocation : IInvocation<bool>
	{
		readonly IParameterValidator adapter;
		public AdapterInvocation( IParameterValidator adapter )
		{
			this.adapter = adapter;
		}

		public bool Invoke( object parameter ) => adapter.IsValid( parameter );
	}

	public struct ValidationInvocation<T> where T : IInvocation<bool>
	{
		readonly T invocation;
		readonly ParameterState state;
		
		public ValidationInvocation( ParameterState state, T invocation )
		{
			this.state = state;
			this.invocation = invocation;
		}

		public bool Invoke( object parameter )
		{
			var result = invocation.Invoke( parameter );
			var validated = result && !state.Valid.IsEnabled( parameter ) && !state.Active.IsEnabled( parameter );
			state.Valid.Enable( parameter, validated );
			return result;
		}
	}

	struct ParameterInvocation : IInvocation<object>
	{
		readonly RelayParameter execute;
		readonly ParameterState state;
		readonly ValidationInvocation<AdapterInvocation> validation;

		public ParameterInvocation( ParameterState state, ValidationInvocation<AdapterInvocation> validation, RelayParameter execute )
		{
			this.state = state;
			this.validation = validation;
			this.execute = execute;
		}

		static Assignment<EnabledStateAssign, object, bool> Create( EnabledState item, object parameter ) => 
			new Assignment<EnabledStateAssign, object, bool>( new EnabledStateAssign( item ), Assignments.From( parameter ), new Value<bool>( true ) ).Configured( false );

		bool AsActive( object parameter )
		{
			using ( Create( state.Active, parameter ) )
			{
				return validation.Invoke( parameter );
			}
		}

		object AsValid( object parameter )
		{
			using ( Create( state.Valid, parameter ) )
			{
				return execute.Proceed<object>();
			}
		}

		public object Invoke( object parameter ) => state.Valid.IsEnabled( parameter ) || AsActive( parameter ) ? AsValid( parameter ) : null;
	}
}
