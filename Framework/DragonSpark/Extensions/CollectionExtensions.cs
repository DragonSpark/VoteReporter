

using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Extensions
{
	/// <summary>
	/// Class that provides extension methods to Collection
	/// </summary>
	public static class CollectionExtensions
	{
		/// <summary>
		/// Add a range of items to a collection.
		/// </summary>
		/// <typeparam name="T">Type of objects within the collection.</typeparam>
		/// <param name="collection">The collection to add items to.</param>
		/// <param name="items">The items to add to the collection.</param>
		/// <returns>The collection.</returns>
		/// <exception cref="System.ArgumentNullException">An <see cref="System.ArgumentNullException"/> is thrown if <paramref name="collection"/> or <paramref name="items"/> is <see langword="null"/>.</exception>
		public static ICollection<T> AddRange<T>( [Required]this ICollection<T> collection, [Required]IEnumerable<T> items)
		{
			foreach (var each in items)
			{
				collection.Add(each);
			}

			return collection;
		}

		public static ImmutableArray<T> Purge<T>( this ICollection<T> @this )
		{
			var result = @this.ToImmutableArray();
			@this.Clear();
			return result;
		}

		public static T Ensure<T, U>( this T @this, U item ) where T : ICollection<U>
		{
			if ( !@this.Contains( item ) )
			{
				@this.Add( item );
			}
			return @this;
		}
	}
}
