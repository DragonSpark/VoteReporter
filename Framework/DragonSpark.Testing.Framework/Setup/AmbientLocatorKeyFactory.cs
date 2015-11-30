using DragonSpark.Activation.FactoryModel;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using Microsoft.Practices.ServiceLocation;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class AmbientLocatorKeyFactory : FactoryBase<MethodInfo, IAmbientKey>
	{
		public static AmbientLocatorKeyFactory Instance { get; } = new AmbientLocatorKeyFactory();

		protected override IAmbientKey CreateItem( MethodInfo parameter )
		{
			var specification = new CurrentMethodSpecification( parameter ).Or( new CurrentTaskSpecification() );
			var result = new AmbientKey<IServiceLocator>( specification );
			return result;
		}
	}
}