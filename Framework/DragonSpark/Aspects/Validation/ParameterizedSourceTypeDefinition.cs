﻿namespace DragonSpark.Aspects.Validation
{
	sealed class ParameterizedSourceTypeDefinition : ValidatedTypeDefinition
	{
		public static ParameterizedSourceTypeDefinition Default { get; } = new ParameterizedSourceTypeDefinition();
		ParameterizedSourceTypeDefinition() : base( Aspects.ParameterizedSourceTypeDefinition.Default.ReferencedType, Aspects.ParameterizedSourceTypeDefinition.Default.Method ) {}
	}
}