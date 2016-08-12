using DragonSpark.Activation;
using DragonSpark.Extensions;
using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.ComponentModel
{
	public class CollectionAttribute : ServicesValueBase
	{
		public CollectionAttribute( Type elementType = null ) : base( t => Create( elementType ) ) {}

		static ServicesValueProvider Create( Type type ) => new ServicesValueProvider( p =>
		{
			var elementType = type ?? p.PropertyType.Adapt().GetEnumerableType();
			var result = elementType.With( Transformer.Instance.Get );
			return result;
		} );

		public class Collection<T> : System.Collections.ObjectModel.Collection<T> {}

		class Transformer : ValidatedParameterizedSourceBase<Type, Type>
		{
			public static Transformer Instance { get; } = new Transformer();

			public override Type Get( Type parameter ) => typeof(Collection<>).MakeGenericType( parameter );
		}
	}
}