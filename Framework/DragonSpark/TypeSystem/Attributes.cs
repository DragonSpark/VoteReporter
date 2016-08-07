using DragonSpark.Runtime.Properties;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public static class Attributes
	{
		//readonly static Func<object, IAttributeProvider> Source = AttributeProviders.Instance.Delegate();
		public static IAttributeProvider Get( object target ) => target as IAttributeProvider ?? AttributeProviders.Instance.Get( target );
	}

	static class AttributeSupport<T> where T : Attribute
	{
		public static ICache<Type, T> Local { get; } = new Cache<Type, T>( type => type.GetTypeInfo().GetCustomAttribute<T>() );
		public static ICache<Type, T> All { get; } = new Cache<Type, T>( type => type.GetTypeInfo().GetCustomAttribute<T>( true ) );
	}
}