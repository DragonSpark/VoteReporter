using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Sources.Scopes
{
	public static class Source
	{
		public static Source<T> Sourced<T>( this T @this ) => Support<T>.Sources.Get( @this );

		static class Support<T>
		{
			public static ICache<T, Source<T>> Sources { get; } = new DecoratedCache<T, Source<T>>();
		}
	}
}