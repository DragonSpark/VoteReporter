﻿using DragonSpark.Composition;
using DragonSpark.Extensions;
using JetBrains.Annotations;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Composition
{
	public class ConstructorSelectorTests
	{

		[Fact]
		public void Basic()
		{
			var parts = this.Adapt().WithNested().AsApplicationParts();
			var builder = ConventionBuilderFactory.Default.Get();
			var container = new ContainerConfiguration().WithParts( parts.AsEnumerable(), builder ).CreateContainer();
			var dependency = container.GetExport<Dependency>();
			Assert.NotNull( dependency );

			var primary = Assert.IsType<Primary>( container.GetExport<IPrimary>() );
			Assert.Equal( 2, primary.Selected.GetParameters().Length );

			var exported = container.GetExport<Exported>();
			Assert.Equal( 3, exported.Selected.GetParameters().Length );

			Assert.Throws<CompositionFailedException>( () => container.GetExport<Protected>() );
		}

		interface IPrimary {}
		class Primary : IPrimary
		{
			[UsedImplicitly]
			public Primary( Dependency dependency, AnotherDependency anotherDependency )
			{
				Selected = MethodBase.GetCurrentMethod();
			}

			[UsedImplicitly]
			public Primary( Dependency dependency, AnotherDependency anotherDependency, NotKnown notKnown )
			{
				Selected = MethodBase.GetCurrentMethod();
			}

			public MethodBase Selected { get; }
		}

		[Export]
		class Exported
		{
			[UsedImplicitly]
			public Exported( Dependency dependency, AnotherDependency anotherDependency )
			{
				Selected = MethodBase.GetCurrentMethod();
			}

			[UsedImplicitly]
			public Exported( Dependency dependency, AnotherDependency anotherDependency, IAnotherDependencyAgain again )
			{
				Selected = MethodBase.GetCurrentMethod();
			}

			public MethodBase Selected { get; }
		}


		[Export]
		class Protected
		{
			[UsedImplicitly]
			protected Protected( Dependency dependency, AnotherDependency anotherDependency ) {}
		}

		interface IDependency {}
		class Dependency : IDependency {}

		interface IAnotherDependency {}
		class AnotherDependency : IAnotherDependency {}

		interface IAnotherDependencyAgain {}
		[UsedImplicitly]
		class AnotherDependencyAgain : IAnotherDependencyAgain {}

		class NotKnown {}
	}
}
