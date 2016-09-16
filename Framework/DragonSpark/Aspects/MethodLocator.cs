﻿using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public sealed class MethodLocator : FactoryCache<Type, MethodInfo>, IMethodLocator
	{
		readonly string methodName;

		public MethodLocator( Type declaringType, string methodName )
		{
			DeclaringType = declaringType;
			this.methodName = methodName;
		}

		public Type DeclaringType { get; }

		protected override MethodInfo Create( Type parameter )
		{
			var mapping = parameter.Adapt().GetMappedMethods( DeclaringType ).Introduce( methodName, tuple => tuple.Item1.InterfaceMethod.Name == tuple.Item2 ).Only();
			var result = mapping.MappedMethod?.AsDeclared().AccountForGenericDefinition();
			return result;
		}
	}
}