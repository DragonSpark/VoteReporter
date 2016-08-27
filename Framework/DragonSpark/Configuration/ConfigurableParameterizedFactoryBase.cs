using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Configuration
{
	public abstract class ConfigurableParameterizedFactoryBase<TConfiguration, TResult> : ConfigurableParameterizedFactoryBase<TConfiguration, object, TResult>
	{
		protected ConfigurableParameterizedFactoryBase( Func<object, TConfiguration> seed, Func<object, ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, object, TResult> factory ) : base( seed, configurators, factory ) {}
		protected ConfigurableParameterizedFactoryBase( IParameterizedScope<TConfiguration> seed, IParameterizedScope<ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, object, TResult> factory ) : base( seed, configurators, factory ) {}
	}

	public abstract class ConfigurableParameterizedFactoryBase<TConfiguration, TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TConfiguration, TParameter, TResult> factory;

		protected ConfigurableParameterizedFactoryBase( Func<TParameter, TConfiguration> seed, Func<TParameter, ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, TParameter, TResult> factory ) : 
			this( new ParameterizedScope<TParameter, TConfiguration>( seed ), new ParameterizedScope<TParameter, ImmutableArray<ITransformer<TConfiguration>>>( configurators ), factory ) {}

		protected ConfigurableParameterizedFactoryBase( IParameterizedScope<TParameter, TConfiguration> seed, IParameterizedScope<TParameter, ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, TParameter, TResult> factory )
		{
			Seed = seed;
			Configurators = configurators;
			this.factory = factory;
		}

		public IParameterizedScope<TParameter, TConfiguration> Seed { get; }

		public IParameterizedScope<TParameter, ImmutableArray<ITransformer<TConfiguration>>> Configurators { get; }

		public override TResult Get( TParameter parameter )
		{
			var configured = Configurators.Get( parameter ).Aggregate( Seed.Get( parameter ), ( configuration, transformer ) => transformer.Get( configuration ) );
			var result = factory( configured, parameter );
			return result;
		}
	}
}