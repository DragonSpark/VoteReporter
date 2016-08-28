using DragonSpark.Activation.Location;
using System;

namespace DragonSpark.ComponentModel
{
	public abstract class ServicesValueBase : DefaultValueBase
	{
		protected ServicesValueBase( ServicesValueProvider.Converter converter ) : this( converter, Defaults.ServiceSource ) {}

		protected ServicesValueBase( ServicesValueProvider.Converter converter, Func<Type, object> creator ) : base( Sources.Parameterized.Extensions.Wrap( new ServicesValueProvider( converter.Get, creator ) ) ) {}

		protected ServicesValueBase( Func<object, IDefaultValueProvider> provider ) : base( provider ) {}
	}
}