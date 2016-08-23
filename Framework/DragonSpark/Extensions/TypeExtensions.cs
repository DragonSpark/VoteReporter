﻿using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Extensions
{
	public static class TypeExtensions
	{
		public static Type GetMemberType(this MemberInfo memberInfo)
		{
		  if (memberInfo is MethodInfo)
			return ((MethodInfo) memberInfo).ReturnType;
		  if (memberInfo is PropertyInfo)
			return ((PropertyInfo) memberInfo).PropertyType;
		  if (memberInfo is FieldInfo)
			return ((FieldInfo) memberInfo).FieldType;
		  return null;
		}

		public static IEnumerable<Assembly> Assemblies( this IEnumerable<Type> @this ) => @this.Select( x => x.Assembly() ).Distinct();

		public static TypeAdapter Adapt( this Type @this ) => TypeAdapterCache.Default.Get( @this );

		public static TypeAdapter Adapt( this object @this ) => @this.GetType().Adapt();

		public static TypeAdapter Adapt( this TypeInfo @this ) => Adapt( @this.AsType() );

		public static Assembly Assembly( this Type @this ) => Adapt( @this ).Assembly;

		readonly static TypeInfo Structural = typeof(IStructuralEquatable).GetTypeInfo();

		public static bool IsStructural( this Type @this ) => Structural.IsAssignableFrom( @this.GetTypeInfo() );
		
		public static bool IsAssignableFrom( this ImmutableArray<TypeAdapter> @this, Type type )
		{
			foreach ( var adapter in @this )
			{
				if ( adapter.IsAssignableFrom( type ) )
				{
					return true;
				}
			}
			return false;
		}

		readonly static IParameterizedSource<MethodBase, ImmutableArray<Type>> ParameterTypes = CacheFactory.Create<MethodBase, ImmutableArray<Type>>( method => method.GetParameters().Select( info => info.ParameterType ).ToImmutableArray() );
		public static ImmutableArray<Type> GetParameterTypes( this MethodBase @this ) => ParameterTypes.Get( @this );
	}
}