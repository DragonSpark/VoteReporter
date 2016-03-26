using DragonSpark.Extensions;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DragonSpark.Activation;

namespace DragonSpark.TypeSystem
{
	[Export, Shared]
	public class AssemblyInformationFactory : FactoryBase<Assembly, AssemblyInformation>
	{
		readonly static System.Type[] Attributes =
		{
			typeof(AssemblyTitleAttribute),
			typeof(AssemblyProductAttribute),
			typeof(AssemblyCompanyAttribute),
			typeof(AssemblyDescriptionAttribute),
			typeof(AssemblyConfigurationAttribute),
			typeof(AssemblyCopyrightAttribute)
		};

		protected override AssemblyInformation CreateItem( Assembly parameter )
		{
			var result = new AssemblyInformation { Version = parameter.GetName().Version };
			Attributes.Select( parameter.GetCustomAttribute ).Cast<object>().NotNull().Each( item => item.MapInto( result ) );
			result.Configuration = result.Configuration.NullIfEmpty() ?? TypeSystem.Attributes.Get( parameter ).From<DebuggableAttribute, string>( attribute => "DEBUG" );
			return result;
		}
	}
}