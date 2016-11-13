﻿using DragonSpark.Aspects.Definitions;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Build
{
	public class AspectSelector<T> : CompositeAspectSelector where T : IMethodLevelAspect
	{
		public AspectSelector( Func<ITypeDefinition, IEnumerable<IAspects>> types ) : this( types, MethodAspectSelector<T>.Default.Yield ) {}

		[UsedImplicitly]
		public AspectSelector( Func<ITypeDefinition, IEnumerable<IAspects>> types, Func<ITypeDefinition, IEnumerable<IAspects>> methods ) 
			: base( types, methods ) {}
	}
}