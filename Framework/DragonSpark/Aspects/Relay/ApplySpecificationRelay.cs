﻿using JetBrains.Annotations;
using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Relay
{
	[IntroduceInterface( typeof(ISpecificationRelay) )]
	public sealed class ApplySpecificationRelay : SpecificationRelayAspectBase, ISpecificationRelay
	{
		public ApplySpecificationRelay()  : base( SpecificationDescriptor.Default ) {}

		[UsedImplicitly]
		public ApplySpecificationRelay( ISpecificationRelay relay ) : base( relay ) {}
	}
}