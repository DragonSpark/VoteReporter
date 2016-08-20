﻿using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation
{
	public static class ActivatorExtensions
	{
		public static Func<Type, T> Delegate<T>( this ISource<IActivator> @this ) => @this.ToDelegate().Delegate<T>();
		public static Func<Type, T> Delegate<T>( this Func<IActivator> @this ) => Delegates<T>.Default.Get( @this );
		class Delegates<T> : FactoryCache<Func<IActivator>, Func<Type, T>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() {}

			protected override Func<Type, T> Create( Func<IActivator> parameter ) => parameter().Activate<T>;
		}

		public static Func<IServiceProvider> Provider( this ISource<IActivator> @this ) => @this.ToDelegate().Provider();
		public static Func<IServiceProvider> Provider( this Func<IActivator> @this ) => Providers.Default.Get( @this );

		class Providers : FactoryCache<Func<IActivator>, Func<IServiceProvider>>
		{
			public static Providers Default { get; } = new Providers();
			Providers() /*: base( source => new Factory( source ).Create )*/ {}

			/*class Factory : FactoryBase<IServiceProvider>
			{
				readonly Func<IActivator> source;
				public Factory( Func<IActivator> source )
				{
					this.source = source;
				}

				
				public override IServiceProvider Create() => new DecoratedServiceProvider( source().Create );
			}*/

			protected override Func<IServiceProvider> Create( Func<IActivator> parameter ) => new ServiceProvider( parameter ).Self;

			sealed class ServiceProvider : IServiceProvider
			{
				readonly Func<IActivator> parameter;
				public ServiceProvider( Func<IActivator> parameter )
				{
					this.parameter = parameter;
				}

				public object GetService( Type serviceType ) => parameter().Activate<object>( serviceType );
			}
		}

		public static T Activate<T>( this IActivator @this ) => Activate<T>( @this, typeof(T) );

		public static T Activate<T>( this IActivator @this, [Required] Type requestedType ) => (T)@this.Get( requestedType );

		public static T Activate<T>( this IActivator @this, TypeRequest request ) => (T)@this.Get( request );
		
		public static T Construct<T>( this IActivator @this, params object[] parameters ) => Construct<T>( @this, typeof(T), parameters );

		public static T Construct<T>( this IActivator @this, Type type, params object[] parameters ) => (T)@this.Get( new ConstructTypeRequest( type, parameters ) );

		public static ImmutableArray<T> ActivateMany<T>( this IActivator @this, IEnumerable<Type> types ) => @this.ActivateMany<T>( typeof(T), types );

		public static ImmutableArray<T> ActivateMany<T>( this IActivator @this, Type objectType, IEnumerable<Type> types ) => @this.CreateMany<T>( types.Where( objectType.Adapt().IsAssignableFrom ) );
	}

	public sealed class Activator : CompositeActivator
	{
		public static ISource<IActivator> Instance { get; } = new Scope<IActivator>( Factory.Global( () => new Activator() ) );
		Activator() : base( new Locator(), Constructor.Instance ) {}

		public static T Activate<T>( Type type ) => Instance.Get().Get<T>( type );

		sealed class Locator : LocatorBase
		{
			readonly Func<Type, Type> convention;
			readonly ISingletonLocator singleton;

			public Locator() : this( ConventionTypes.Instance.Get, SingletonLocator.Instance ) {}

			Locator( Func<Type, Type> convention, ISingletonLocator singleton )
			{
				this.convention = convention;
				this.singleton = singleton;
			}

			public override object Get( LocateTypeRequest parameter ) => singleton.Get( convention( parameter.RequestedType ) ?? parameter.RequestedType );
		}
	}

	public interface ISingletonLocator : IParameterizedSource<Type, object> {}

	public class SingletonSpecification : SpecificationBase<SingletonRequest>
	{
		public static SingletonSpecification Instance { get; } = new SingletonSpecification();
		SingletonSpecification() : this( "Instance", "Default" ) {}

		readonly ImmutableArray<string> candidates;

		public SingletonSpecification( params string[] candidates ) : this( candidates.ToImmutableArray() ) {}

		public SingletonSpecification( ImmutableArray<string> candidates )
		{
			this.candidates = candidates;
		}

		public override bool IsSatisfiedBy( SingletonRequest parameter )
		{
			var result =
				SourceTypeAssignableSpecification.Instance.IsSatisfiedBy( new SourceTypeAssignableSpecification.Parameter( parameter.RequestedType, parameter.Candidate.PropertyType ) )
				&& 
				parameter.Candidate.GetMethod.IsStatic && !parameter.Candidate.GetMethod.ContainsGenericParameters 
				&& 
				( candidates.Contains( parameter.Candidate.Name ) || parameter.Candidate.Has<SingletonAttribute>() );
			return result;
		}
	}

	public struct SingletonRequest
	{
		public SingletonRequest( Type requestedType, PropertyInfo candidate )
		{
			RequestedType = requestedType;
			Candidate = candidate;
		}

		public Type RequestedType { get; }
		public PropertyInfo Candidate { get; }
	}

	public sealed class SourceTypeAssignableSpecification : SpecificationBase<SourceTypeAssignableSpecification.Parameter>
	{
		public static SourceTypeAssignableSpecification Instance { get; } = new SourceTypeAssignableSpecification();
		SourceTypeAssignableSpecification() {}

		public override bool IsSatisfiedBy( Parameter parameter )
		{
			foreach ( var candidate in Candidates( parameter.Candidate ) )
			{
				if ( candidate.Adapt().IsAssignableFrom( parameter.TargetType ) )
				{
					return true;
				}
			}
			return false;
		}

		static IEnumerable<Type> Candidates( Type type )
		{
			yield return type;
			var implementations = type.Adapt().GetImplementations( typeof(ISource<>) );
			if ( implementations.Any() )
			{
				yield return implementations.First().Adapt().GetInnerType();
			}
		}

		public struct Parameter
		{
			public Parameter( Type targetType, Type candidate )
			{
				TargetType = targetType;
				Candidate = candidate;
			}

			public Type TargetType { get; }
			public Type Candidate { get; }
		}
	}

	/*public sealed class SourceTypeAssignableSpecification : GuardedSpecificationBase<Type>
	{
		public static ISpecification<Type> Instance { get; } = new SourceTypeAssignableSpecification().ToCache();
		SourceTypeAssignableSpecification() {}

		readonly static TypeAdapter Source = typeof(ISource).Adapt();

		public override bool IsSatisfiedBy( Type parameter ) => Source.IsAssignableFrom( parameter );
	}*/

	sealed class SingletonDelegateCache : FactoryCache<PropertyInfo, Func<object>>
	{
		public static SingletonDelegateCache Instance { get; } = new SingletonDelegateCache();
		SingletonDelegateCache() {}

		protected override Func<object> Create( PropertyInfo parameter ) => 
			parameter.PropertyType.Adapt().IsGenericOf( typeof(ISource<>), false ) ? parameter.GetMethod.CreateDelegate<Func<ISource>>().Invoke().Get : parameter.GetMethod.CreateDelegate<Func<object>>();
	}

	public class SingletonDelegates : SingletonDelegates<Func<object>>
	{
		public static SingletonDelegates Instance { get; } = new SingletonDelegates();
		SingletonDelegates() : this( SingletonProperties.Instance ) {}
		public SingletonDelegates( IParameterizedSource<Type, PropertyInfo> source ) : base( source.ToSourceDelegate(), SingletonDelegateCache.Instance.Get ) {}
		// public SingletonDelegates( ISpecification<SingletonRequest> specification, Func<PropertyInfo, Func<object>> source ) : base( specification, source ) {}
	}

	public class SingletonDelegates<T> : FactoryCache<Type, T>
	{
		readonly Func<Type, PropertyInfo> propertySource;
		readonly Func<PropertyInfo, T> source;

		public SingletonDelegates( Func<Type, PropertyInfo> propertySource, Func<PropertyInfo, T> source )
		{
			this.propertySource = propertySource;
			this.source = source;
		}

		protected override T Create( Type parameter )
		{
			var property = propertySource( parameter );
			var result = property != null ? source( property ) : default(T);
			return result;
		}
	}

	public class SingletonProperties : ParameterizedSourceBase<Type, PropertyInfo>
	{
		public static IParameterizedSource<Type, PropertyInfo> Instance { get; } = new SingletonProperties().ToCache();
		SingletonProperties() : this( SingletonSpecification.Instance ) {}

		readonly ISpecification<SingletonRequest> specification;

		public SingletonProperties( ISpecification<SingletonRequest> specification )
		{
			this.specification = specification;
		}

		public override PropertyInfo Get( Type parameter )
		{
			foreach ( var property in parameter.GetTypeInfo().DeclaredProperties.Fixed() )
			{
				if ( specification.IsSatisfiedBy( new SingletonRequest( parameter, property ) ) )
				{
					return property;
				}
			}
			return null;
		}
	}

	public class SingletonLocator : FactoryCache<Type, object>, ISingletonLocator
	{
		[Export( typeof(ISingletonLocator) )]
		public static SingletonLocator Instance { get; } = new SingletonLocator();
		SingletonLocator() : this( SingletonDelegates.Instance.Get ) {}

		readonly Func<Type, Func<object>> provider;

		public SingletonLocator( Func<Type, Func<object>> provider )
		{
			this.provider = provider;
		}

		protected override object Create( Type parameter ) => provider( parameter )?.Invoke();
	}
}