﻿using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Composition;

namespace DragonSpark.Windows
{
	[Export( typeof(ISetup) )]
	public partial class InitializationCommand
	{
		[ModuleInitializer( 0 )]
		public static void Initialize() => ExecutionContextRepository.Instance.Add( ExecutionContextStore.Instance );

		public static InitializationCommand Instance { get; } = new InitializationCommand();
		InitializationCommand() : base( AttributeConfigurations.TypeDefinitionProviders.From( Runtime.TypeDefinitionProviderStore.Instance ) )
		{
			Priority = Priority.BeforeNormal;
			// InitializeComponent();
		}
	}

	[Priority( Priority.AfterNormal )]
	class ExecutionContextStore : Store<AppDomain>, IExecutionContextStore
	{
		public static ExecutionContextStore Instance { get; } = new ExecutionContextStore();
		ExecutionContextStore() : base( AppDomain.CurrentDomain ) {}
	}
}
