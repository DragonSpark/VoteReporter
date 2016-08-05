﻿using DragonSpark.Activation.IoC.Specifications;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation.IoC
{
	public class BuildPlanCreatorPolicy : IBuildPlanCreatorPolicy
	{
		readonly ISpecification<LocateTypeRequest> specification;
		readonly ImmutableArray<IBuildPlanPolicy> policies;
		readonly IBuildPlanCreatorPolicy creator;

		public BuildPlanCreatorPolicy( IBuildPlanCreatorPolicy creator, ImmutableArray<IBuildPlanPolicy> policies ) : this( creator, policies, HasFactorySpecification.Instance.Inverse() ) {}

		public BuildPlanCreatorPolicy( IBuildPlanCreatorPolicy creator, ImmutableArray<IBuildPlanPolicy> policies, ISpecification<LocateTypeRequest> specification )
		{
			this.creator = creator;
			this.policies = policies;
			this.specification = specification;
		}

		public IBuildPlanPolicy CreatePlan( IBuilderContext context, NamedTypeBuildKey buildKey )
		{
			var planPolicies = policies.AsEnumerable();
			var isSatisfiedBy = specification.IsSatisfiedBy( new LocateTypeRequest( buildKey.Type, buildKey.Name ) );
			var buildPlanPolicies = isSatisfiedBy ? planPolicies.StartWith( creator.CreatePlan( context, buildKey ) ) : planPolicies;
			var result = new CompositeBuildPlanPolicy( buildPlanPolicies );
			return result;
		}
	}

	public class CompositeBuildPlanPolicy : IBuildPlanPolicy
	{
		readonly ImmutableArray<IBuildPlanPolicy> policies;

		public CompositeBuildPlanPolicy( IEnumerable<IBuildPlanPolicy> policies )
		{
			this.policies = policies.ToImmutableArray();
		}

		public void BuildUp( IBuilderContext context )
		{
			Exception thrown = null;
			foreach ( var policy in policies )
			{
				try
				{
					policy.BuildUp( context );
					if ( context.Existing != null )
					{
						return;
					}
				}
				catch ( Exception exception )
				{
					thrown = exception;
				}
			}
			throw thrown;
		}
	}

	class SingletonBuildPlanPolicy : IBuildPlanPolicy
	{
		public static SingletonBuildPlanPolicy Instance { get; } = new SingletonBuildPlanPolicy();
		SingletonBuildPlanPolicy() : this( SingletonLocator.Instance ) {}

		readonly ISingletonLocator locator;

		public SingletonBuildPlanPolicy( [Required] ISingletonLocator locator )
		{
			this.locator = locator;
		}

		public void BuildUp( IBuilderContext context ) => context.Existing = locator.Get( context.BuildKey.Type ) ?? context.Existing;
	}

	public class EnumerableResolutionStrategy : BuilderStrategy
	{
		readonly static MethodInfo GenericResolveArrayMethod = typeof(EnumerableResolutionStrategy).GetTypeInfo().DeclaredMethods.First( m => m.Name == nameof(Resolve) && !m.IsPublic );
		readonly static string[] DefaultName = { null };

		delegate object Resolver( IBuilderContext context );

		readonly IUnityContainer container;
		readonly Func<Type, object> provider;

		public EnumerableResolutionStrategy( IUnityContainer container, Func<Type, object> provider )
		{
			this.container = container;
			this.provider = provider;
		}

		public override void PreBuildUp( [Required]IBuilderContext context )
		{
			if ( !context.HasBuildPlan() )
			{
				BuildUp( context );

				if ( context.BuildComplete )
				{
					var array = context.Existing as Array;
					if ( array != null )
					{
						var result = array.Length > 0 ? array : provider( context.BuildKey.Type ) ?? array;
						context.Complete( result );
					}
				}
			}
		}

		void BuildUp( IBuilderContext context )
		{
			var adapt = context.BuildKey.Type.Adapt();
			var isGenericOf = adapt.IsGenericOf( typeof(IEnumerable<>), false );
			if ( isGenericOf )
			{
				var enumerableType = adapt.GetEnumerableType();
				if ( enumerableType != null )
				{
					var resolver = (Resolver)GenericResolveArrayMethod.MakeGenericMethod( enumerableType.ToItem() ).CreateDelegate( typeof(Resolver), this );
					var result = resolver( context );
					context.Complete( result );
				}
			}
		}

		object Resolve<T>( IBuilderContext context )
		{
			var defaultName = container.IsRegistered<T>() ? DefaultName : Items<string>.Default;
			var policy = context.Policies.Get<IRegisteredNamesPolicy>( null );
			var result = policy != null ? 
				policy.GetRegisteredNames( typeof(T) )
					.Concat( defaultName ).Concat( typeof(T).GetTypeInfo().IsGenericType ? policy.GetRegisteredNames( typeof(T).GetGenericTypeDefinition() ) : Enumerable.Empty<string>() )
					.Distinct()
					.Select( context.NewBuildUp<T> )
					.ToArray() : Items<T>.Default;
			return result;
		}
	}
}
