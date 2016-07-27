using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation
{
	public sealed class IsFactorySpecification : AdapterSpecificationBase
	{
		public static ISpecification<Type> Instance { get; } = new IsFactorySpecification( typeof(IFactory), typeof(IFactoryWithParameter) ).Cached();
		IsFactorySpecification( params Type[] types ) : base( types ) {}

		public override bool IsSatisfiedBy( Type parameter ) => Adapters.IsAssignableFrom( parameter );
	}

	public class IsGenericFactorySpecification : AdapterSpecificationBase
	{
		public static ISpecification<Type> Instance { get; } = new IsGenericFactorySpecification( typeof(IFactory<>), typeof(IFactory<,>) ).Cached();
		IsGenericFactorySpecification( params Type[] types ) : base( types ) {}

		public override bool IsSatisfiedBy( Type parameter ) => Adapters.Select( adapter => adapter.Type ).Any( parameter.Adapt().IsGenericOf );
	}

	public abstract class AdapterSpecificationBase : SpecificationBase<Type>
	{
		protected AdapterSpecificationBase( params Type[] types ) : this( types.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		AdapterSpecificationBase( ImmutableArray<TypeAdapter> adapters )
		{
			Adapters = adapters;
		}

		protected ImmutableArray<TypeAdapter> Adapters { get; }
	}

	public class FactoryInterfaces : FactoryCache<Type, Type>
	{
		readonly static Func<Type, bool> GenericFactory = IsGenericFactorySpecification.Instance.ToDelegate();
		readonly static Func<Type, bool> Factory = IsFactorySpecification.Instance.ToDelegate();

		public static ICache<Type, Type> Instance { get; } = new FactoryInterfaces();

		protected override Type Create( Type parameter ) => parameter.Adapt().GetAllInterfaces().With( types => types.FirstOrDefault( GenericFactory ) ?? types.FirstOrDefault( Factory ) );
	}

	public class ParameterTypes : TypeLocatorBase
	{
		public static ICache<Type, Type> Instance { get; } = new ParameterTypes( ImmutableArray.Create( typeof(Func<,>), typeof(IFactory<,>), typeof(ICommand<>) ) );
		ParameterTypes( ImmutableArray<Type> types ) : base( types ) {}

		protected override Type Select( IEnumerable<Type> genericTypeArguments ) => genericTypeArguments.First();
	}

	public class ResultTypes : TypeLocatorBase
	{
		public static ICache<Type, Type> Instance { get; } = new ResultTypes( ImmutableArray.Create( typeof(IFactory<,>), typeof(IFactory<>), typeof(Func<>), typeof(Func<,>) ) );
		ResultTypes( ImmutableArray<Type> types ) : base( types ) {}

		protected override Type Select( IEnumerable<Type> genericTypeArguments ) => genericTypeArguments.Last();
	}

	public abstract class TypeLocatorBase : FactoryCache<Type, Type>
	{
		readonly ImmutableArray<TypeAdapter> adapters;
		readonly Func<TypeInfo, bool> isAssignable;
		readonly Func<Type[], Type> selector;

		protected TypeLocatorBase( ImmutableArray<Type> types ) : this( types.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		TypeLocatorBase( ImmutableArray<TypeAdapter> adapters )
		{
			this.adapters = adapters;
			isAssignable = IsAssignable;
			selector = Select;
		}

		protected override Type Create( Type parameter )
		{
			var result = parameter.Append( parameter.Adapt().GetAllInterfaces() )
				.AsTypeInfos()
				.Where( isAssignable )
				.Select( info => info.GenericTypeArguments )
				.Select( selector )
				.FirstOrDefault();
			return result;
		}

		bool IsAssignable( TypeInfo type ) => type.IsGenericType && adapters.IsAssignableFrom( type.GetGenericTypeDefinition() );

		protected abstract Type Select( IEnumerable<Type> genericTypeArguments );
	}

	public class ProjectedFactory<TFrom, TTo> : ProjectedFactory<object, TFrom, TTo>
	{
		public ProjectedFactory( Func<TFrom, TTo> convert ) : base( convert ) {}
	}

	public class ProjectedFactory<TBase, TFrom, TTo> where TFrom : TBase
	{
		readonly Func<TFrom, TTo> convert;

		public ProjectedFactory( Func<TFrom, TTo> convert )
		{
			this.convert = convert;
		}

		public virtual TTo Create( TBase parameter ) => parameter is TFrom ? convert( (TFrom)parameter ) : default(TTo);
	}

	public class InstanceFromFactoryTypeFactory : FactoryBase<Type, object>
	{
		public static InstanceFromFactoryTypeFactory Instance { get; } = new InstanceFromFactoryTypeFactory();
		InstanceFromFactoryTypeFactory() : this( FactoryDelegateLocatorFactory.Instance.Create ) {}

		readonly Func<Type, Func<object>> factory;

		InstanceFromFactoryTypeFactory( Func<Type, Func<object>> factory )
		{
			this.factory = factory;
		}

		public override object Create( Type parameter ) => factory( parameter )?.Invoke();
	}

	public class FactoryDelegateLocatorFactory : CompositeFactory<Type, Func<object>>
	{
		readonly static Func<Type, bool> FactorySpecification = TypeAssignableSpecification<IFactory>.Instance.ToDelegate();
		readonly static Func<Type, bool> FactoryWithParameterSpecification = TypeAssignableSpecification<IFactoryWithParameter>.Instance.ToDelegate();

		public static FactoryDelegateLocatorFactory Instance { get; } = new FactoryDelegateLocatorFactory();
		FactoryDelegateLocatorFactory() : base( FactoryDelegateFactory.Instance, FactoryWithActivatedParameterDelegateFactory.Instance ) {}

		public FactoryDelegateLocatorFactory( FactoryDelegateFactory factory, FactoryWithActivatedParameterDelegateFactory factoryWithParameter ) 
			: base( new AutoValidatingFactory<Type, Func<object>>( factory, FactorySpecification ), new AutoValidatingFactory<Type, Func<object>>( factoryWithParameter, FactoryWithParameterSpecification ) ) {}
	}

	public class FactoryTypes : EqualityReferenceCache<LocateTypeRequest, Type>
	{
		public static ISource<FactoryTypes> Instance { get; } = new ExecutionScope<FactoryTypes>( () => new FactoryTypes() );
		FactoryTypes() : this( FactoryTypeRequests.Instance.GetMany( ApplicationParts.Instance.Get().Types ) ) {}

		public static IParameterizedSource<MemberInfo, Type> Members { get; } = new ParameterizedSource<MemberInfo, Type>( new FactoryTypeLocator<MemberInfo>( member => member.GetMemberType(), member => member.DeclaringType ).Create );

		public static IParameterizedSource<ParameterInfo, Type> Parameters { get; } = new ParameterizedSource<ParameterInfo, Type>( new FactoryTypeLocator<ParameterInfo>( parameter => parameter.ParameterType, parameter => parameter.Member.DeclaringType ).Create );

		public FactoryTypes( ImmutableArray<FactoryTypeRequest> requests ) : base( new Factory( requests ).Create ) {}

		sealed class Factory :  FactoryBase<LocateTypeRequest, Type>
		{
			readonly ImmutableArray<FactoryTypeRequest> types;

			public Factory( ImmutableArray<FactoryTypeRequest> types )
			{
				this.types = types;
			}

			public override Type Create( LocateTypeRequest parameter )
			{
				var candidates = types.Introduce( parameter, tuple => tuple.Item1.Name == tuple.Item2.Name && tuple.Item1.ResultType.Adapt().IsAssignableFrom( tuple.Item2.RequestedType ) ).ToArray();
				var item = 
					candidates.Introduce( $"{parameter.RequestedType.Name}Factory", info => info.Item1.RequestedType.Name == info.Item2 ).Only()
					??
					candidates.Introduce( parameter, arg => arg.Item1.ResultType == arg.Item2.RequestedType ).FirstOrDefault()
					??
					candidates.FirstOrDefault();

				var result = item?.RequestedType;
				return result;
			}
		}
	}
}