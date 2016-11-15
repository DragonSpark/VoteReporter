using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Windows.Legacy.Markup
{
	public class CollectionMarkupProperty : MarkupPropertyBase
	{
		readonly IList collection;
		
		public CollectionMarkupProperty( IList collection, PropertyReference reference ) : base( reference )
		{
			this.collection = collection;
		}

		protected override object OnGetValue() => collection;

		protected override object Apply( object value )
		{
			var index = collection.Cast<object>().WithFirst( o => o == null, o =>
			{
				var i = collection.IndexOf( o );
				collection.RemoveAt( i );
				return i;
			}, () => -1 );

			var itemType = collection.GetType().GetInnerType();
			var items = new Stack<object>( itemType.IsInstanceOfType( value ) ? value.Fix() : value.GetType().GetInnerType().With( itemType.IsAssignableFrom ) ? value.To<IEnumerable>().Cast<object>() : Items<object>.Default );

			var result = index == -1 && items.Any() ? items.Pop() : null;

			var insert = Math.Max( 0, index );

			foreach ( var item in items )
			{
				collection.Insert( insert, item );
			}
			return result;
		}
	}
}