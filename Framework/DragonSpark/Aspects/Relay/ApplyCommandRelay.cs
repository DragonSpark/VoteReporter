﻿using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Relay
{
	[IntroduceInterface( typeof(ISource<ICommandAdapter>) )]
	public sealed class ApplyCommandRelay : InstanceAspectBase, ISource<ICommandAdapter>
	{
		readonly ICommandAdapter adapter;

		public ApplyCommandRelay() : base( CommandRelaySelectors.Default.Get, CommandRelayDefinition.Default ) {}

		[UsedImplicitly]
		public ApplyCommandRelay( ICommandAdapter adapter )
		{
			this.adapter = adapter;
		}

		public ICommandAdapter Get() => adapter;
	}
}