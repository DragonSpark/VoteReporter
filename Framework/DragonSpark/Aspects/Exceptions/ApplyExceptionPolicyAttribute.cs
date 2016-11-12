﻿using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using JetBrains.Annotations;
using Polly;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Exceptions
{
	[IntroduceInterface( typeof(IPolicySource) )]
	[ProvideAspectRole( StandardRoles.ExceptionHandling ), LinesOfCodeAvoided( 1 ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ParameterValidation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.EnhancedValidation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )
		]
	public sealed class ApplyExceptionPolicyAttribute : InstanceAspectBase, IPolicySource
	{
		readonly Policy policy;

		public ApplyExceptionPolicyAttribute( Type policyType ) :  base( Constructors.Default.Get( policyType ), Definition.Default ) {}

		[UsedImplicitly]
		public ApplyExceptionPolicyAttribute( Policy policy )
		{
			this.policy = policy;
		}

		public Policy Get() => policy;
		// object ISource.Get() => Get();

		sealed class Constructors : TypedAspectConstructors<Policy, ApplyExceptionPolicyAttribute>
		{
			public static Constructors Default { get; } = new Constructors();
			Constructors() : base( Activator.Default.Get<Policy> ) {}
		}
	}
}