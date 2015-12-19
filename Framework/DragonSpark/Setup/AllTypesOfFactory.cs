using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using System;
using System.Linq;

namespace DragonSpark.Setup
{
	[Register]
	public class AllTypesOfFactory : FactoryBase<Type, Array>
	{
		readonly IAssemblyProvider provider;
		readonly IActivator activator;

		public AllTypesOfFactory( IAssemblyProvider provider, IActivator activator )
		{
			this.provider = provider;
			this.activator = activator;
		}

		public T[] Create<T>()
		{
			var result = Create( typeof(T) ).Cast<T>().ToArray();
			return result;
		}

		protected override Array CreateItem( Type parameter )
		{
			var types = provider.GetAssemblies().SelectMany( assembly => assembly.ExportedTypes );
			var result = activator.ActivateMany( parameter, types ).ToArray();
			return result;
		}
	}
}