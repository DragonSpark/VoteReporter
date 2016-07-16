﻿using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Runtime.Stores;
using System;

namespace DragonSpark.Windows
{
	public partial class Configure
	{
		public Configure()
		{
			InitializeComponent();
		}
	}

	public class InitializationCommand : InitializationCommandBase
	{
		public InitializationCommand() : base( /*ConfigureExecutionLocator.Instance,*/ new Configure() ) {}
	}

	public class ExecutionContextLocatorConfiguration : FixedStore<Func<IExecutionContext>>
	{
		public static ExecutionContextLocatorConfiguration Instance { get; } = new ExecutionContextLocatorConfiguration();
		ExecutionContextLocatorConfiguration() : base( () => ExecutionContext.Instance ) {}
	}

	/*class ConfigureExecutionLocator : DecoratedCommand<IStore>
	{
		public static ConfigureExecutionLocator Instance { get; } = new ConfigureExecutionLocator();
		ConfigureExecutionLocator() : base( /*new AssignValueCommand( Activation.ExecutionContextLocatorConfiguration.Instance ).Fixed( ExecutionContextLocatorConfiguration.Instance ), new OnlyOnceSpecification()#1# ) {}
	}*/

	class ExecutionContext : Store<AppDomain>, IExecutionContext
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();
		ExecutionContext() : base( AppDomain.CurrentDomain ) {}
	}
}
