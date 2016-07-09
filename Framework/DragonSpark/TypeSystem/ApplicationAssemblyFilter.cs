using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using PostSharp.Aspects.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DragonSpark.TypeSystem
{
	public class ApplicationAssemblyFilter : TransformerBase<Assembly[]>
	{
		// public static ApplicationAssemblyFilter Instance { get; } = new ApplicationAssemblyFilter( Items<string>.Default );

		readonly ISpecification<Assembly> specification;
		
		static string[] Determine( IEnumerable<Assembly> coreAssemblies ) => coreAssemblies.WhereAssigned().Append( typeof(ApplicationAssemblyFilter).Assembly() ).Distinct().Select( assembly => assembly.GetRootNamespace() ).ToArray();

		public ApplicationAssemblyFilter( params Assembly[] coreAssemblies ) : this( Determine( coreAssemblies ) ) {}

		public ApplicationAssemblyFilter( string[] namespaces ) : this( new ApplicationAssemblySpecification( namespaces ) ) {}

		public ApplicationAssemblyFilter( ISpecification<Assembly> specification )
		{
			this.specification = specification;
		}

		public override Assembly[] Create( Assembly[] parameter ) => parameter.Where( specification.ToDelegate() ).Prioritize().ToArray();
	}

	public class ApplicationTypeSpecification : GuardedSpecificationBase<Type>
	{
		public static ICache<Type, bool> Instance { get; } = new ApplicationTypeSpecification().Cached();
		ApplicationTypeSpecification() {}

		public override bool IsSatisfiedBy( Type parameter ) => CanInstantiateSpecification.Instance.IsSatisfiedBy( parameter ) && !typeof(MethodBinding).Adapt().IsAssignableFrom( parameter ) && !parameter.Has<CompilerGeneratedAttribute>();
	}

	public class ApplicationAssemblySpecification : GuardedSpecificationBase<Assembly>
	{
		public static ApplicationAssemblySpecification Instance { get; } = new ApplicationAssemblySpecification( Items<string>.Default );

		readonly string[] rootNamespaces;

		public ApplicationAssemblySpecification( [PostSharp.Patterns.Contracts.Required] params string[] rootNamespaces )
		{
			this.rootNamespaces = rootNamespaces;
		}

		public override bool IsSatisfiedBy( Assembly parameter ) => parameter.Has<RegistrationAttribute>() || rootNamespaces.Any( parameter.GetName().Name.StartsWith );
	}
}