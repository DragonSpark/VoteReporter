using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Sources;
using Microsoft.Practices.ServiceLocation;
using System;

namespace DragonSpark.Activation
{
	public sealed class GlobalServiceProvider : Scope<IServiceProvider>
	{
		public static IScope<IServiceProvider> Instance { get; } = new GlobalServiceProvider();

		GlobalServiceProvider() : base( () => DefaultServiceProvider.Instance )
		{
			ServiceLocator.SetLocatorProvider( GetService<IServiceLocator> );
		}

		public static T GetService<T>() => GetService<T>( typeof(T) );

		public static T GetService<T>( Type type ) => Instance.Get().Get<T>( type );
	}

	public sealed class DefaultServiceProvider : CompositeServiceProvider
	{
		public static IServiceProvider Instance { get; } = new DefaultServiceProvider();
		DefaultServiceProvider() : base( new SourceInstanceServiceProvider( GlobalServiceProvider.Instance, Activator.Instance, Exports.Instance, ApplicationParts.Instance, ApplicationAssemblies.Instance, ApplicationTypes.Instance, LoggingHistory.Instance.ToScope(), LoggingController.Instance.ToScope(), Logging.Instance.ToScope() ), new InstanceServiceProvider( SingletonLocator.Instance ), new DecoratedServiceProvider( Activator.Activate<object> ) ) {}
	}

	// public delegate object ServiceSource( Type serviceType );

	public class DecoratedServiceProvider : IServiceProvider
	{
		readonly Func<Type, object> inner;

		public DecoratedServiceProvider( IServiceProvider provider ) : this( provider.GetService ) {}

		public DecoratedServiceProvider( Func<Type, object> inner )
		{
			this.inner = inner;
		}

		public virtual object GetService( Type serviceType ) => inner( serviceType );
	}
}