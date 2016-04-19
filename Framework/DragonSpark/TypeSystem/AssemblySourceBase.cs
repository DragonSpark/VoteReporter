using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Extensibility;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public interface IAssemblyLoader
	{
		void Load( Assembly reference, string search );
	}

	public class LoadPartAssemblyCommand : Command<Assembly>
	{
		readonly IAssemblyLoader provider;
		readonly string searchQuery;

		public LoadPartAssemblyCommand( IAssemblyLoader provider, string searchQuery = "{0}.Parts.*" )
		{
			this.provider = provider;
			this.searchQuery = searchQuery;
		}

		protected override void OnExecute( Assembly parameter ) => provider.Load( parameter, searchQuery );
	}

	public class AssemblyHintProvider : FactoryBase<Assembly, string>
	{
		public static AssemblyHintProvider Instance { get; } = new AssemblyHintProvider();

		protected override string CreateItem( Assembly parameter ) => parameter.GetName().Name;
	}

	public class AssemblyLoader : IAssemblyLoader
	{
		readonly Func<Assembly, string> hintSource;
		readonly Func<string, IEnumerable<Assembly>> assemblySource;
		readonly Action<Assembly> initialize;

		// public static AssemblyLoader Instance { get; } = new AssemblyLoader( AssemblyHintProvider.Instance.Create );

		public AssemblyLoader( Func<string, IEnumerable<Assembly>> assemblySource, Action<Assembly> initialize ) : this( AssemblyHintProvider.Instance.Create, assemblySource, initialize ) {}

		public AssemblyLoader( Func<Assembly, string> hintSource, Func<string, IEnumerable<Assembly>> assemblySource, Action<Assembly> initialize )
		{
			this.hintSource = hintSource;
			this.assemblySource = assemblySource;
			this.initialize = initialize;
		}

		public void Load( Assembly reference, string search )
		{
			var hint = hintSource( reference );
			var stack = new Stack<string>( hint.Split( '.' ) );
			while ( stack.Any() )
			{
				var name = string.Join( ".", stack.Reverse() );
				var path = string.Format( search, name );
				var items = assemblySource( path ).Fixed();
				if ( items.Any() )
				{
					items.Each( initialize );
					stack.Clear();
				}
				else
				{
					stack.Pop();
				}
			}
		}
	}

	public class FactoryTypeRequest : LocateTypeRequest
	{
		public FactoryTypeRequest( [Required]Type runtimeType, string name, [Required]Type resultType ) :  base( runtimeType, name )
		{
			ResultType = resultType;
		}

		public Type ResultType { get; }
	}

	public class AssembliesFactory : FactoryBase<Type[], Assembly[]>
	{
		public static AssembliesFactory Instance { get; } = new AssembliesFactory();

		[Freeze]
		protected override Assembly[] CreateItem( Type[] parameter ) => parameter.Assemblies();
	}

	public static class AssemblyTypes
	{
		public static AssemblyTypesFactory All { get; } = new AssemblyTypesFactory( assembly => assembly.DefinedTypes.AsTypes() );

		public static AssemblyTypesFactory Public { get; } = new AssemblyTypesFactory( assembly => assembly.ExportedTypes );
	}

	public class AssemblyTypesFactory : FactoryBase<Assembly, Type[]>
	{
		readonly Func<Assembly, IEnumerable<Type>> types;

		public AssemblyTypesFactory( [Required] Func<Assembly, IEnumerable<Type>> types )
		{
			this.types = types;
		}

		[Freeze]
		protected override Type[] CreateItem( Assembly parameter ) => types( parameter ).Where( ApplicationTypeSpecification.Instance.IsSatisfiedBy ).Fixed();
	}

	public class TypesFactory : FactoryBase<Assembly[], Type[]>
	{
		public static TypesFactory Instance { get; } = new TypesFactory();

		[Freeze]
		protected override Type[] CreateItem( Assembly[] parameter ) => parameter.SelectMany( AssemblyTypes.All.Create ).ToArray();
	}

	public abstract class AssemblySourceBase : FactoryBase<Assembly[]>
	{
		[Freeze( AttributeInheritance = MulticastInheritance.Multicast, AttributeTargetMemberAttributes = MulticastAttributes.Instance )]
		protected abstract override Assembly[] CreateItem();
	}

	public abstract class AssemblyProviderBase : AssemblySourceBase, IAssemblyProvider
	{
		readonly Assembly[] assemblies;

		protected AssemblyProviderBase( IEnumerable<Type> types, params Assembly[] assemblies ) : this( AssembliesFactory.Instance.Create( types.Fixed() ).Union( assemblies ).ToArray() ) {}

		protected AssemblyProviderBase( params Type[] types ) : this( AssembliesFactory.Instance.Create( types ) ) {}

		AssemblyProviderBase( [Required] IEnumerable<Assembly> assemblies )
		{
			this.assemblies = assemblies.NotNull().Distinct().Prioritize().Fixed();
		}

		protected override Assembly[] CreateItem() => assemblies;
	}

	public class AggregateAssemblyFactory : AggregateFactory<Assembly[]>, IAssemblyProvider
	{
		// public AggregateAssemblyFactory( IFactory<Assembly[]> primary, params ITransformer<Assembly[]>[] transformers ) : base( primary, transformers ) {}

		public AggregateAssemblyFactory( Func<Assembly[]> primary, params Func<Assembly[], Assembly[]>[] transformers ) : base( primary, transformers ) {}
	}
}