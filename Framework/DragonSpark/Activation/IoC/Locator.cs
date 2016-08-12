using DragonSpark.Activation.IoC.Specifications;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System.Linq;

namespace DragonSpark.Activation.IoC
{
	public class Locator : LocatorBase
	{
		readonly IUnityContainer container;
		
		public Locator( IUnityContainer container, IResolutionSpecification support ) : base( new DecoratedSpecification<LocateTypeRequest>( support, Coerce ) )
		{
			this.container = container;
		}

		public override object Get( LocateTypeRequest parameter ) => container.TryResolve( parameter.RequestedType, parameter.Name );
	}

	public class Constructor : ConstructorBase
	{
		readonly IUnityContainer container;
		
		public Constructor( [Required]IUnityContainer container, [Required]IResolutionSpecification support ) : base( new DecoratedSpecification<ConstructTypeRequest>( support, DefaultCoerce ) )
		{
			this.container = container;
		}

		public override object Get( ConstructTypeRequest parameter )
		{
			using ( var child = container.CreateChildContainer().Extend<DefaultRegistrationsExtension>() )
			{
				var command = child.Resolve<RegisterEntireHierarchyCommand>();
				parameter.Arguments.WhereAssigned().Select( o => new InstanceRegistrationParameter( o ) ).Each( command.Execute );

				var result = child.TryResolve( parameter.RequestedType );
				return result;
			}
		}
	}
}