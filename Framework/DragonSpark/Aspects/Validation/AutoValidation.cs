﻿using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using PostSharp.Aspects;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	public static class AutoValidation
	{
		public static ImmutableArray<IProfile> DefaultProfiles { get; } = new IProfile[] { GenericSourceProfile.Instance, GenericCommandProfile.Instance, SourceProfile.Instance, CommandProfile.Instance }.ToImmutableArray();

		sealed class GenericSourceProfile : ProfileBase
		{
			public static GenericSourceProfile Instance { get; } = new GenericSourceProfile();
			GenericSourceProfile() : base( typeof(IValidatedParameterizedSource<,>), typeof(ISpecification<>), nameof(ISpecification.IsSatisfiedBy), typeof(IParameterizedSource<,>), nameof(IParameterizedSource.Get), GenericFactoryProfileFactory.Instance.Create ) {}
		}

		sealed class SourceProfile : ProfileBase
		{
			public static SourceProfile Instance { get; } = new SourceProfile();
			SourceProfile() : base( typeof(IValidatedParameterizedSource), typeof(ISpecification), nameof(ISpecification.IsSatisfiedBy), typeof(IParameterizedSource), nameof(IParameterizedSource.Get), SourceAdapterSource.Instance.Get ) {}
		}

		sealed class GenericCommandProfile : ProfileBase
		{
			public static GenericCommandProfile Instance { get; } = new GenericCommandProfile();
			GenericCommandProfile() : base( typeof(ICommand<>), typeof(ISpecification<>), nameof(ISpecification.IsSatisfiedBy), typeof(ICommand<>), nameof(ICommand.Execute), GenericCommandProfileFactory.Instance.Create ) {}
		}

		sealed class CommandProfile : ProfileBase
		{
			public static CommandProfile Instance { get; } = new CommandProfile();
			CommandProfile() : base( typeof(ICommand), typeof(ICommand), nameof(ICommand.CanExecute), typeof(ICommand), nameof(ICommand.Execute), CommandProfileSource.Instance.Get ) {}
		}
	}
	
	class AdapterLocator : ParameterizedSourceBase<object, IParameterValidationAdapter>
	{
		public static AdapterLocator Instance { get; } = new AdapterLocator();
		AdapterLocator() : this( Adapters.Instance.Get ) {}

		readonly Func<Type, Func<object, IParameterValidationAdapter>> factorySource;

		AdapterLocator( Func<Type, Func<object, IParameterValidationAdapter>> factorySource )
		{
			this.factorySource = factorySource;
		}

		sealed class Adapters : Cache<Type, Func<object, IParameterValidationAdapter>>
		{
			public static Adapters Instance { get; } = new Adapters();
			Adapters() : this( AutoValidation.DefaultProfiles ) {}

			readonly ImmutableArray<IProfile> profiles;
			
			Adapters( ImmutableArray<IProfile> profiles )
			{
				this.profiles = profiles;
			}

			public override Func<object, IParameterValidationAdapter> Get( Type parameter )
			{
				foreach ( var profile in profiles )
				{
					if ( profile.InterfaceType.IsAssignableFrom( parameter ) )
					{
						return profile.ProfileSource;
					}
				}
				return null;
			}
		}

		public override IParameterValidationAdapter Get( object parameter )
		{
			var other = parameter.GetType();
			var factory = factorySource( other );
			if ( factory != null )
			{
				return factory( parameter );
			}

			throw new InvalidOperationException( $"Profile not found for {other}." );
		}
	}

	public static class Extensions
	{
		public static bool Marked( this IAutoValidationController @this, object parameter, bool valid )
		{
			@this.MarkValid( parameter, valid );
			return valid;
		}
	}

	public interface IAutoValidationController : ISpecification
	{
		void MarkValid( object parameter, bool valid );

		object Execute( object parameter, Func<object> proceed );
	}

	public interface IMethodAware
	{
		MethodInfo Method { get; }
	}

	public interface IAspectHub
	{
		void Register( IAspect aspect );
	}

	class LinkedParameterAwareHandler : IParameterAwareHandler
	{
		readonly IParameterAwareHandler current;
		readonly IParameterAwareHandler next;

		public LinkedParameterAwareHandler( IParameterAwareHandler current, IParameterAwareHandler next )
		{
			this.current = current;
			this.next = next;
		}

		public bool Handles( object parameter ) => current.Handles( parameter ) || next.Handles( parameter );

		public bool Handle( object parameter, out object handled ) => current.Handle( parameter, out handled ) || next.Handle( parameter, out handled );
	}

	class AutoValidationController : ConcurrentDictionary<int, object>, IAutoValidationController, IAspectHub
	{
		readonly static object Executing = new object();

		readonly IParameterValidationAdapter validator;
		
		public AutoValidationController( IParameterValidationAdapter validator )
		{
			this.validator = validator;
		}

		IParameterAwareHandler Handler { get; set; }

		public bool IsSatisfiedBy( object parameter ) => Handler?.Handles( parameter ) ?? false;

		bool? Current( object parameter )
		{
			object current;
			var contains = TryGetValue( Environment.CurrentManagedThreadId, out current );
			var result = contains && current != Executing ? (bool?)Equals( current, parameter ) : null;
			return result;
		}

		public void MarkValid( object parameter, bool valid )
		{
			if ( valid )
			{
				this[Environment.CurrentManagedThreadId] = parameter;
			}
			else
			{
				object removed;
				TryRemove( Environment.CurrentManagedThreadId, out removed );
			}
		}

		public object Execute( object parameter, Func<object> proceed )
		{
			object handled;
			if ( Handler != null && Handler.Handle( parameter, out handled ) )
			{
				MarkValid( parameter, false );
				return handled;
			}

			var valid = Current( parameter ).GetValueOrDefault() || CheckAndMark( parameter );
			if ( valid )
			{
				var result = proceed();
				MarkValid( parameter, false );
				return result;
			}
			return null;
		}

		bool CheckAndMark( object parameter )
		{
			MarkValid( Executing, true );
			var result = validator.IsSatisfiedBy( parameter );
			MarkValid( parameter, result );
			return result;
		}

		public void Register( IAspect aspect )
		{
			var methodAware = aspect as IMethodAware;
			if ( methodAware != null && validator.IsSatisfiedBy( methodAware.Method ) )
			{
				var handler = aspect as IParameterAwareHandler;
				if ( handler != null )
				{
					Handler = Handler != null ? new LinkedParameterAwareHandler( handler, Handler ) : handler;
				}
			}
		}
	}
}
