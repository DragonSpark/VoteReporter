using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem.Generics
{
	public abstract class DelegateCreationContextBase<T> : FilteredMethodContextBase where T : class
	{
		readonly ICache<string, IGenericMethodContext<T>> candidates;

		readonly Func<MethodInfo, T> creator;
		
		protected DelegateCreationContextBase( Func<MethodInfo, T> creator, IEnumerable<MethodInfo> methods, Func<MethodInfo, bool> filter ) : base( methods, filter )
		{
			this.creator = creator;
			candidates = new EqualityReferenceCache<string, IGenericMethodContext<T>>( Get );
		}

		public IGenericMethodContext<T> this[ string methodName ] => candidates.Get( methodName );

		IGenericMethodContext<T> Get( string name )
		{
			var descriptors = Methods.ToArray().Introduce( name, tuple => tuple.Item1.Name == tuple.Item2, tuple => new Descriptor( tuple.Item1 ) );
			var result = new GenericMethodContext<T>( descriptors, creator );	
			return result;
		}
	}
}