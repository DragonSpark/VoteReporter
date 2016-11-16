using DragonSpark.Specifications;
using System;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class SpecificationCache<T> : SpecificationCache<T, T> where T : class
	{
		public SpecificationCache( Func<T, ISpecification<T>> create ) : base( create ) {}
	}

	public class SpecificationCache<TKey, TSpecification> : Cache<TKey, ISpecification<TSpecification>> where TKey : class
	{
		public SpecificationCache( Func<TKey, ISpecification<TSpecification>> create ) : base( create ) {}
	}
}