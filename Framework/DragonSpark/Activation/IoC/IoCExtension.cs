﻿using DragonSpark.Activation.FactoryModel;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragonSpark.ComponentModel;

namespace DragonSpark.Activation.IoC
{
	public class IoCExtension : UnityContainerExtension, IDisposable
	{
		[Activate]
		public RecordingMessageLogger Logger { get; set; }

		class BuildKeyMonitorStrategy : BuilderStrategy
		{
			readonly IList<NamedTypeBuildKey> keys = new List<NamedTypeBuildKey>();

			public IEnumerable<NamedTypeBuildKey> Purge() => keys.Purge();

			public override void PreBuildUp( IBuilderContext context ) => keys.Ensure( context.BuildKey );
		}

		protected override void Initialize()
		{
			Context.RegisteringInstance += ContextOnRegisteringInstance;

			Context.Policies.SetDefault<IConstructorSelectorPolicy>( DefaultUnityConstructorSelectorPolicy.Instance );

			Context.Strategies.Clear();
			Context.Strategies.AddNew<BuildKeyMappingStrategy>( UnityBuildStage.TypeMapping );
			Context.Strategies.AddNew<HierarchicalLifetimeStrategy>( UnityBuildStage.Lifetime );
			Context.Strategies.AddNew<LifetimeStrategy>( UnityBuildStage.Lifetime );
			Context.Strategies.AddNew<ArrayResolutionStrategy>( UnityBuildStage.Creation );
			Context.Strategies.AddNew<EnumerableResolutionStrategy>( UnityBuildStage.Creation );
			Context.Strategies.AddNew<BuildPlanStrategy>( UnityBuildStage.Creation );

			var monitor = new BuildKeyMonitorStrategy();
			Context.BuildPlanStrategies.Add( monitor, UnityBuildStage.Setup );

			Container.Registration<EnsuredRegistrationSupport>().With( support =>
			{
				support.Instance<IMessageLogger>( Logger );
				support.Instance<IResolutionSupport>( new ResolutionSupport( Context ) );
				support.Instance( CreateActivator );
				support.Instance( CreateRegistry );
			} );

			monitor.Purge().Each( Context.Policies.Clear<IBuildPlanPolicy> );

			Context.BuildPlanStrategies.Clear();
			Context.BuildPlanStrategies.AddNew<DynamicMethodConstructorStrategy>( UnityBuildStage.Creation );
			Context.BuildPlanStrategies.AddNew<DynamicMethodPropertySetterStrategy>( UnityBuildStage.Initialization );
			Context.BuildPlanStrategies.AddNew<DynamicMethodCallStrategy>( UnityBuildStage.Initialization );

			var policy = Context.Policies.Get<IBuildPlanCreatorPolicy>( null );
			var builder = new Builder<TryContext>( Context.Strategies, policy.CreatePlan );
			Context.Policies.SetDefault<IBuildPlanCreatorPolicy>( new BuildPlanCreatorPolicy( builder.Create, Policies, policy ) );
		}

		public class Builder<T> : FactoryBase<IBuilderContext, T>
		{
			readonly NamedTypeBuildKey key = NamedTypeBuildKey.Make<T>();
			readonly IStagedStrategyChain strategies;
			readonly Func<IBuilderContext, NamedTypeBuildKey, IBuildPlanPolicy> creator;

			public Builder( [Required]IStagedStrategyChain strategies, [Required]Func<IBuilderContext, NamedTypeBuildKey, IBuildPlanPolicy> creator )
			{
				this.strategies = strategies;
				this.creator = creator;
			}

			protected override T CreateItem( IBuilderContext parameter )
			{
				var context = new BuilderContext( strategies.MakeStrategyChain(), parameter.Lifetime, parameter.PersistentPolicies, parameter.Policies, key, null );
				var plan = creator( context, key );
				plan.BuildUp( context );
				var result = context.Existing.To<T>();
				return result;
			}
		}

		public IList<IBuildPlanPolicy> Policies { get; } = new List<IBuildPlanPolicy> { new SingletonBuildPlanPolicy() };
		
		public override void Remove()
		{
			base.Remove();

			Context.RegisteringInstance -= ContextOnRegisteringInstance;
		}

		void ContextOnRegisteringInstance( object sender, RegisterInstanceEventArgs args )
		{
			var type = args.Instance.GetType();

			var register = args.Instance.AsTo<IMessageLogger, bool>( logger =>
			{
				var result = logger != Logger;
				if ( result )
				{
					Logger.Purge().Each( logger.Log );
				}
				return result;
			}, () => !Container.IsRegistered( type, args.Name ) );

			if ( args.RegisteredType != type && register )
			{
				Container.Registration().Instance( type, args.Instance, args.Name, (LifetimeManager)Container.Resolve( args.LifetimeManager.GetType() ) );
			}
		}

		IServiceRegistry CreateRegistry() => new ServiceRegistry( Container );

		IActivator CreateActivator() => new CompositeActivator( Container.Resolve<Activator>(), SystemActivator.Instance );

		void IDisposable.Dispose() => Remove();
	}
}