using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Testing.Framework.Setup
{
	public class LocalAutoDataAttribute : AutoDataAttribute
	{
		public LocalAutoDataAttribute( bool includeFromParameters = true, params Type[] others ) : base( data => new Application( data.Method.DeclaringType, others.Concat( includeFromParameters ? data.Method.GetParameters().Select( info => info.ParameterType ) : Default<Type>.Items ) ) ) {}

		public class Application : ApplicationBase
		{
			public Application( Type primaryType, IEnumerable<Type> others ) : this( primaryType.Adapt().WithNested().Concat( TypesFactory.Instance.Create( new ApplicationAssemblyFilter().Create( others.Assemblies() ) ) ) ) {}

			Application( IEnumerable<Type> enumerable ) : base( new ServiceProviderFactory( enumerable.Fixed() ).Create() ) {}
		}
	}

	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly Func<AutoData, IApplication> source;

		public AutoDataAttribute() : this( autoData => new Application() ) {}

		protected AutoDataAttribute( Func<AutoData, IApplication> application ) : this( FixtureFactory<DefaultAutoDataCustomization>.Instance.Create, application ) {}

		protected AutoDataAttribute( Func<IFixture> fixture  ) : this( fixture, autoData => new Application() ) {}

		protected AutoDataAttribute( [Required]Func<IFixture> fixture, [Required]Func<AutoData, IApplication> source  ) : base( fixture() )
		{
			this.source = source;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( methodUnderTest ) ) )
			{
				var autoData = new AutoData( Fixture, methodUnderTest );
				var application = source( autoData );
				using ( new ExecuteApplicationCommand( application ).ExecuteWith( autoData ) )
				{
					var result = base.GetData( methodUnderTest );
					return result;
				}
			}
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance>( info => new AspectInstance( info, new AssignExecutionContextAspect() ) ).ToItem();
	}
}