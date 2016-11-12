﻿using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelayDefinition : PairedAspectBuildDefinition
	{
		public static CommandRelayDefinition Default { get; } = new CommandRelayDefinition();
		CommandRelayDefinition() : base( new Dictionary<ITypeDefinition, IEnumerable<IAspectDefinition>>
												   {
													   { GenericCommandTypeDefinition.Default, CommandRelaySelectors.Default }
												   }.ToImmutableDictionary() ) {}
	}

	public sealed class CommandRelaySelectors : AspectSelectors<ICommandAdapter, ApplyCommandRelay>
	{
		public static CommandRelaySelectors Default { get; } = new CommandRelaySelectors();
		CommandRelaySelectors() : base( 
			GenericCommandTypeDefinition.Default.ReferencedType, 
			typeof(CommandAdapter<>),
			new MethodAspectDefinition<SpecificationRelay>( CommandTypeDefinition.Default.Validation ),
			new MethodAspectDefinition<CommandRelay>( CommandTypeDefinition.Default.Execution )
		) {}
	}
}