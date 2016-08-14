using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture.Kernel;
using System;

namespace DragonSpark.Testing.Framework.Setup
{
	public class CanLocateSpecification : IRequestSpecification
	{
		readonly IActivator activator;

		/*public CanLocateSpecification( IServiceLocator locator ) : this( locator.GetInstance<IActivator>() )
		{}*/

		public CanLocateSpecification( IActivator activator )
		{
			this.activator = activator;
		}

		public bool IsSatisfiedBy( object request ) => TypeSupport.From( request ).With( CanLocate );

		protected virtual bool CanLocate( Type type ) => activator.IsSatisfiedBy( type );
	}
}