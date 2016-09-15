﻿using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Extensions.Build
{
	sealed class Specification : SpecificationWithContextBase<Type, ImmutableArray<TypeAdapter>>
	{
		public static Specification Default { get; } = new Specification();
		Specification() : this( AutoValidation.DefaultProfiles.Select( profile => profile.DeclaringType.Adapt() ).ToImmutableArray() ) {}

		public Specification( params Type[] types ) : base( types.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		public Specification( ImmutableArray<TypeAdapter> context ) : base( context ) {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			if ( !Context.IsAssignableFrom( parameter ) )
			{
				throw new InvalidOperationException( $"{parameter} does not implement any of the types defined in {GetType()}, which are: {string.Join( ",", Context.Select( t => t.Type.FullName ) )}" );
			}
			return true;
		}
	}
}