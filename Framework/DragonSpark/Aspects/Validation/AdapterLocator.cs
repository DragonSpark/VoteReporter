﻿using System;
using System.Collections.Immutable;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Aspects.Validation
{
	sealed class AdapterLocator : ParameterizedSourceBase<IParameterValidationAdapter>
	{
		public static AdapterLocator Default { get; } = new AdapterLocator();
		AdapterLocator() : this( AdapterSources.DefaultNested.Get ) {}

		readonly Func<Type, IAdapterSource> factorySource;

		AdapterLocator( Func<Type, IAdapterSource> factorySource )
		{
			this.factorySource = factorySource;
		}

		sealed class AdapterSources : Cache<Type, IAdapterSource>
		{
			public static AdapterSources DefaultNested { get; } = new AdapterSources();
			AdapterSources() : this( AutoValidation.DefaultSources ) {}

			readonly ImmutableArray<IAdapterSource> sources;
			
			AdapterSources( ImmutableArray<IAdapterSource> sources )
			{
				this.sources = sources;
			}

			public override IAdapterSource Get( Type parameter )
			{
				foreach ( var source in sources )
				{
					if ( source.IsSatisfiedBy( parameter ) )
					{
						return source;
					}
				}
				return null;
			}
		}

		public override IParameterValidationAdapter Get( object parameter )
		{
			var other = parameter.GetType();
			var adapter = factorySource( other )?.Get( parameter );
			if ( adapter != null )
			{
				return adapter;
			}

			throw new InvalidOperationException( $"Adapter not found for {other}." );
		}
	}
}