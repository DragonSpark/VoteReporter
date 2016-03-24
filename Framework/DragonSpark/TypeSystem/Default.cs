using AutoMapper.Internal;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Threading;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public static class Default<T>
	{
		public static Func<T, T> Self => t => t;

		[Freeze]
		public static T Item => DefaultFactory<T>.Instance.Create();

		[Freeze]
		public static T[] Items => DefaultFactory<T[]>.Instance.Create();
	}

	[Synchronized] // http://stackoverflow.com/questions/35976558/is-constructorinfo-getparameters-thread-safe/35976798
	public class InitializeTypeCommand : Command<System.Type>
	{
		public static InitializeTypeCommand Instance { get; } = new InitializeTypeCommand();

		public InitializeTypeCommand() : this( CanBuildSpecification.Instance ) {}

		public InitializeTypeCommand( ISpecification<System.Type> specification ) : base( specification ) {}

		[Freeze]
		protected override void OnExecute( System.Type parameter ) => parameter.GetTypeInfo().DeclaredConstructors.Each( info => info.GetParameters() );
	}

	public static class TypeSupport
	{
		public static Type From( object item ) => item.AsTo<ParameterInfo, System.Type>( info => info.ParameterType ) ?? item.AsTo<MemberInfo, System.Type>( info => info.GetMemberType() ) ?? item as System.Type;
	}
}