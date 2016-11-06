﻿using DragonSpark.Specifications;
using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Implementations // TODO: Delete
{
	[IntroduceInterface( typeof(ISpecification<object>) )]
	public sealed class GeneralizedSpecificationAspect : GeneralizedAspectBase, ISpecification<object>
	{
		public bool IsSatisfiedBy( object parameter ) => false;
	}
}