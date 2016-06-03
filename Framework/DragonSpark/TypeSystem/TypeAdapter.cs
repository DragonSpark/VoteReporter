using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class TypeAdapter
	{
		public TypeAdapter( [Required]Type type ) : this( type, type.GetTypeInfo() ) {}

		public TypeAdapter( [Required]TypeInfo info ) : this( info.AsType(), info ) {}

		public TypeAdapter( [Required]Type type,  [Required]TypeInfo info )
		{
			Type = type;
			Info = info;
		}

		public Type Type { get; }

		public TypeInfo Info { get; }

		// readonly static MethodInfo LambdaMethod = typeof(Expression).GetRuntimeMethods().First(x => x.Name == nameof(Expression.Lambda) && x.GetParameters()[1].ParameterType == typeof(ParameterExpression[]));
// static readonly MethodInfo EqualsMethod = typeof (object).GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public);

		// Attribution: https://weblogs.asp.net/ricardoperes/detecting-default-values-of-value-types
		/*bool IsDefaultUsingLinqAndDynamic()
		{
			var arguments = new Expression[] { Expression.Default( Type ) };
			var paramExpression = Expression.Parameter(Type, "x");
			var equalsMethod = Type.GetRuntimeMethod( nameof(Equals), new [] { Type } );
			var call = Expression.Call(paramExpression, equalsMethod, arguments);
			var lambdaArgType = typeof(Func<,>).MakeGenericType(Type, typeof(bool));
			var lambdaMethod = LambdaMethod.MakeGenericMethod(lambdaArgType);
			var expression = lambdaMethod.Invoke(null, new object[] { call, new[] { paramExpression } }) as LambdaExpression;
 
			//cache this, please
			var func = expression.Compile();
			dynamic arg = obj;
			dynamic del = func;
			var result = del( arg );
			return result;
		}*/

		public Type[] WithNested() => Info.Append( Info.DeclaredNestedTypes ).AsTypes().Where( ApplicationTypeSpecification.Instance.IsSatisfiedBy ).ToArray();

		public bool IsDefined<T>( [Required] bool inherited = false ) where T : Attribute => Info.IsDefined( typeof(T), inherited );

		/*[Freeze]
		public object GetDefaultValue() => null; // Info.IsValueType && Nullable.GetUnderlyingType( Type ) == null ? CreateUsing() : null;*/

		public ConstructorInfo FindConstructor( params object[] parameters ) => FindConstructor( parameters.Select( o => o?.GetType() ).ToArray() );

		public ConstructorInfo FindConstructor( params Type[] parameterTypes ) => Info.DeclaredConstructors.SingleOrDefault( c => c.IsPublic && !c.IsStatic && Match( c.GetParameters(), parameterTypes ) );

		static bool Match( IReadOnlyCollection<ParameterInfo> parameters, IReadOnlyCollection<Type> provided )
		{
			var result = 
				provided.Count >= parameters.Count( info => !info.IsOptional ) && 
				provided.Count <= parameters.Count && 
				parameters
					.Select( ( t, i ) => provided.ElementAtOrDefault( i ).With( t.ParameterType.Adapt().IsAssignableFrom, () => i < provided.Count || t.IsOptional ) )
					.All( b => b );
			return result;
		}

		public object Invoke( string methodName, Type[] types ) => Invoke( methodName, types, Items<object>.Default );

		public object Invoke( string methodName, Type[] types, params object[] parameters ) => Invoke( null, methodName, types, parameters );

		public object Invoke( object instance, string methodName, Type[] types, params object[] parameters ) => Invoke<object>( instance, methodName, types, parameters );

		public T Invoke<T>( string methodName, Type[] types, params object[] parameters ) => Invoke<T>( null, methodName, types, parameters );

		public T Invoke<T>( object instance, string methodName, Type[] types, params object[] parameters )
		{
			var methodInfo = MakeGenericMethod( methodName, types );
			var result = (T)methodInfo.Invoke( methodInfo.IsStatic ? null : instance, parameters );
			return result;
		}

		[Freeze]
		MethodInfo MakeGenericMethod( string methodName, Type[] types )
		{
			return Type.GetRuntimeMethods().Single( info => info.IsGenericMethod && info.Name == methodName && info.GetGenericArguments().Length == types.Length ).MakeGenericMethod( types );
		}

		// public object Qualify( object instance ) => instance.With( o => Info.IsAssignableFrom( o.GetType().GetTypeInfo() ) ? o : /*GetCaster( o.GetType() ).With( caster => caster.Invoke( null, new[] { o } ) ) )*/ null );

		public bool IsAssignableFrom( TypeInfo other ) => IsAssignableFrom( other.AsType() );

		public bool IsAssignableFrom( Type other ) => Info.IsAssignableFrom( other.GetTypeInfo() ) /*|| GetCaster( other ) != null*/;

		public bool IsInstanceOfType( object context ) => IsAssignableFrom( context.GetType() );

		// MethodInfo GetCaster( Type other ) => null; // Info.DeclaredMethods.SingleOrDefault( method => method.Name == "op_Implicit" && method.GetParameters().First().ParameterType.GetTypeInfo().IsAssignableFrom( other.GetTypeInfo() ) );

		public Assembly Assembly => Info.Assembly;

		public Type[] GetHierarchy( bool includeRoot = true )
		{
			var result = new List<Type> { Type };
			var current = Info.BaseType;
			while ( current != null )
			{
				if ( current != typeof(object) || includeRoot )
				{
					result.Add( current );
				}
				current = current.GetTypeInfo().BaseType;
			}
			return result.ToArray();
		}

		public Type GetEnumerableType() => InnerType( Type, types => types.FirstOrDefault(), i => i.Adapt().IsGenericOf( typeof(IEnumerable<>) ) );

		// public Type GetResultType() => type.Append( ExpandInterfaces( type ) ).FirstWhere( t => InnerType( t, types => types.LastOrDefault() ) );

		public Type GetInnerType() => InnerType( Type, types => types.Only() );

		static Type InnerType( Type target, Func<Type[], Type> fromGenerics, Func<TypeInfo, bool> check = null )
		{
			var info = target.GetTypeInfo();
			var result = info.IsGenericType && info.GenericTypeArguments.Any() && check.With( func => func( info ), () => true ) ? fromGenerics( info.GenericTypeArguments ) :
				target.IsArray ? target.GetElementType() : null;
			return result;
		}

		public bool IsGenericOf<T>( bool includeInterfaces = true ) => IsGenericOf( typeof(T).GetGenericTypeDefinition(), includeInterfaces );

		[Freeze]
		public Type[] GetTypeArgumentsFor( Type implementationType, bool includeInterfaces = true ) => GetImplementations( implementationType, includeInterfaces ).First().GenericTypeArguments;

		// public Type[] GetImplementations<T>( bool includeInterfaces = true ) => GetImplementations( typeof(T).GetGenericTypeDefinition(), includeInterfaces );

		[Freeze]
		public Type[] GetImplementations( Type genericDefinition, bool includeInterfaces = true ) =>
			Type.Append( includeInterfaces ? ExpandInterfaces( Type ) : Items<Type>.Default )
				.AsTypeInfos()
				.Where( typeInfo => typeInfo.IsGenericType && genericDefinition.GetTypeInfo().IsGenericType && typeInfo.GetGenericTypeDefinition() == genericDefinition.GetGenericTypeDefinition() )
				.AsTypes()
				.Fixed();

		[Freeze]
		public Tuple<MethodInfo, MethodInfo>[] GetMappedMethods( Type implementedType )
		{
			var implementation = CheckGeneric( implementedType ) ?? ( implementedType.Adapt().IsAssignableFrom( Type ) ? implementedType : null );
			if ( implementation != null )
			{
				var map = Info.GetRuntimeInterfaceMap( implementation );
				var result = map.InterfaceMethods.TupleWith( map.TargetMethods ).Fixed();
				return result;
			}
			return Items<Tuple<MethodInfo, MethodInfo>>.Default;
		}

		Type CheckGeneric( Type type )
		{
			if ( type.GetTypeInfo().IsGenericTypeDefinition )
			{
				var implementations = GetImplementations( type );
				if ( implementations.Any() )
				{
					return implementations.First();
				}
			}
			return null;
		}

		public bool IsGenericOf( Type genericDefinition, bool includeInterfaces = true ) => GetImplementations( genericDefinition, includeInterfaces ).Any();

		public Type[] GetAllInterfaces() => ExpandInterfaces( Type ).ToArray();

		static IEnumerable<Type> ExpandInterfaces( Type target ) => target.Append( target.GetTypeInfo().ImplementedInterfaces.SelectMany( ExpandInterfaces ) ).Where( x => x.GetTypeInfo().IsInterface ).Distinct();

		public Type[] GetEntireHierarchy() => ExpandInterfaces( Type ).Union( GetHierarchy( false ) ).Distinct().ToArray();
	}
}