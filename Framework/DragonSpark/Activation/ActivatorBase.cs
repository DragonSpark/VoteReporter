using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Activation
{
	public abstract class ActivatorBase<TRequest> : ValidatedParameterizedSourceBase<TRequest, object>, IActivator where TRequest : TypeRequest
	{
		protected ActivatorBase( Coerce<TRequest> coercer ) : this( coercer, Specification.Instance ) {}

		protected ActivatorBase( Coerce<TRequest> coercer, ISpecification<TRequest> specification ) : base( coercer, specification ) {}

		bool IValidatedParameterizedSource<TypeRequest, object>.IsValid( TypeRequest parameter ) => IsValid( (TRequest)parameter );

		object IParameterizedSource<TypeRequest, object>.Get( TypeRequest parameter ) => Get( (TRequest)parameter );

		class Specification : IsInstanceOfSpecification<TRequest>
		{
			public new static Specification Instance { get; } = new Specification();

			public override bool IsSatisfiedBy( object parameter ) => base.IsSatisfiedBy( parameter ) || IsInstanceOfSpecification<Type>.Instance.IsSatisfiedBy( parameter );
		}
	}

	public abstract class LocatorBase : ActivatorBase<LocateTypeRequest>
	{
		readonly protected static Coerce<LocateTypeRequest> Coerce = Coercer.Instance.ToDelegate();

		protected LocatorBase() : base( Coerce ) {}

		protected LocatorBase( ISpecification<LocateTypeRequest> specification ) : base( Coerce, specification ) {}

		public class Coercer : TypeRequestCoercer<LocateTypeRequest>
		{
			public static Coercer Instance { get; } = new Coercer();
		
			protected override LocateTypeRequest Create( Type type ) => new LocateTypeRequest( type );
		}
	}
}