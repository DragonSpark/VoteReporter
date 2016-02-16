﻿using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;

namespace DragonSpark.Testing.Objects.Setup
{
	public partial class DefaultSetup
	{
		public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( SetupFixtureFactory<DefaultSetup>.Instance.Create ) {}
		}

		public DefaultSetup()
		{
			InitializeComponent();
		}
	}

	public class SetupFixtureFactory<T> : FixtureFactory<SetupCustomization<T>> where T : class, ISetup<AutoData> {}

	public class SetupCustomization<T> : Framework.Setup.SetupCustomization<T> where T : class, ISetup<AutoData> {}

	/*[Discoverable]
	public class ServiceLocatorFactory : Activation.IoC.ServiceLocatorFactory
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(ServiceLocatorFactory) ) {}
		}

		public static ServiceLocatorFactory Instance { get; } = new ServiceLocatorFactory();

		ServiceLocatorFactory() : base( UnityContainerFactory.Instance.Create ) {}
	}*/

	public class AssignServiceLocation : AssignLocationCommand<UnityContainerFactory> {}

	[Discoverable]
	public class UnityContainerFactory : UnityContainerFactory<AssemblyProvider>
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}

		public UnityContainerFactory() : base( MessageLogger.Create() ) {}

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();
	}
}
