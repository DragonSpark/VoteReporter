﻿using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class MetadataTypeDefinitionProvider : TransformerBase<TypeInfo>, ITypeDefinitionProvider
	{
		public static MetadataTypeDefinitionProvider Instance { get; } = new MetadataTypeDefinitionProvider();

		MetadataTypeDefinitionProvider() {}

		public override TypeInfo Create( TypeInfo parameter ) => parameter.GetCustomAttribute<MetadataTypeAttribute>().With( item => item.MetadataClassType.GetTypeInfo() );
	}
}
