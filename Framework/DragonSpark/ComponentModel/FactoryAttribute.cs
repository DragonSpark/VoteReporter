using DragonSpark.Activation;
using DragonSpark.Setup.Registration;
using System;

namespace DragonSpark.ComponentModel
{
	public sealed class ServiceAttribute : ServicesValueBase
	{
		public ServiceAttribute( Type serviceType = null ) : base( new ServicesValueProvider.Converter( serviceType ) ) {}
	}

	public sealed class FactoryAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> FactoryMethod = SourceFactory.Default.Get;
		
		public FactoryAttribute( Type factoryType = null ) : base( new ServicesValueProvider.Converter( p => factoryType ?? SourceTypeLocator.Instance.Get( p.GetMethod.ReturnType ) ), FactoryMethod ) {}
	}
}