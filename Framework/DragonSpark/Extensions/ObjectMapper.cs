using AutoMapper;
using AutoMapper.Configuration;
using DragonSpark.Activation;
using DragonSpark.Sources.Parameterized;
using System;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Extensions
{
	public sealed class ObjectMapper<T> : ParameterizedSourceBase<ObjectMappingParameter<T>, T> where T : class
	{
		public static ObjectMapper<T> Default { get; } = new ObjectMapper<T>();
		ObjectMapper() : this( Activator.Instance.Get ) {}

		readonly Func<IActivator> activatorSource;

		readonly IArgumentCache<TypePair, IMapper> mappers = new ArgumentCache<TypePair, IMapper>( Factory.Instance.Get );

		public ObjectMapper( Func<IActivator> activatorSource )
		{
			this.activatorSource = activatorSource;
		}

		public override T Get( ObjectMappingParameter<T> parameter )
		{
			var destination = parameter.Existing ?? activatorSource().Get( parameter.Pair.DestinationType );
			var result = mappers.Get( parameter.Pair ).Map( parameter.Source, (T)destination );
			return result;
		}

		sealed class Factory : ParameterizedSourceBase<TypePair, IMapper>
		{
			public static IParameterizedSource<TypePair, IMapper> Instance { get; } = new Factory();
			Factory() {}

			public override IMapper Get( TypePair parameter )
			{
				var expression = new MapperConfigurationExpression { CreateMissingTypeMaps = true };
					expression
					.IgnoreUnassignable()
					.CreateMap( parameter.SourceType, parameter.DestinationType )
					.IgnoreAllPropertiesWithAnInaccessibleSetter()
					;
				var result = new MapperConfiguration( expression ).CreateMapper();
				return result;
			}
		}
	}
}