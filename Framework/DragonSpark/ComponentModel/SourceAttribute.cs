using DragonSpark.Activation.Location;
using DragonSpark.Aspects;
using DragonSpark.Sources;
using System;

namespace DragonSpark.ComponentModel
{
	public class SourceAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> Creator = Create;
		public SourceAttribute( [OfType( typeof(ISource) )]Type sourceType = null ) : base( new ServicesValueProvider.Converter( info => sourceType ?? info.PropertyType ), Creator ) {}

		static object Create( Type type )
		{
			var service = GlobalServiceProvider.GetService<object>( type );
			var source = service as ISource;
			var result = source?.Get() ?? service;
			return result;
		}
	}
}