﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Sources.Parameterized
{
	public interface IParameterizedItemSource<in TParameter, TItem> : IParameterizedSource<TParameter, ImmutableArray<TItem>>
	{
		IEnumerable<TItem> Yield( TParameter parameter );
	}

	public abstract class ParameterizedItemSourceBase<TParameter, TItem> : ParameterizedSourceBase<TParameter, ImmutableArray<TItem>>, IParameterizedItemSource<TParameter, TItem>
	{
		public override ImmutableArray<TItem> Get( TParameter parameter ) => Yield( parameter ).ToImmutableArray();

		public abstract IEnumerable<TItem> Yield( TParameter parameter );
	}

	public class DelegatedParameterizedItemSource<TParameter, TItem> : ParameterizedItemSourceBase<TParameter, TItem>
	{
		readonly Func<TParameter, IEnumerable<TItem>> factory;
		public DelegatedParameterizedItemSource( Func<TParameter, IEnumerable<TItem>> factory )
		{
			this.factory = factory;
		}

		public override IEnumerable<TItem> Yield( TParameter parameter ) => factory( parameter );
	}
}
