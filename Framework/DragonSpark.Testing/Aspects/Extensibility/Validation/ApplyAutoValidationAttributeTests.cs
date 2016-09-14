using DragonSpark.Aspects;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects.Extensibility.Validation
{
	public class ApplyAutoValidationAttributeTests : TestCollectionBase
	{
		readonly ExtendedFactory factory = new ExtendedFactory();
		readonly AutoValidatingSource<int, float> validating;
		readonly AppliedExtendedFactory applied = new AppliedExtendedFactory();

		public ApplyAutoValidationAttributeTests( ITestOutputHelper output ) : base( output )
		{
			validating = new AutoValidatingSource<int, float>( factory, factory );
		}

		[Fact]
		[Trait( Traits.Category, Traits.Categories.Performance )]
		/*
		Test                                | Average |  Median |    Mode
		-----------------------------------------------------------------
		BasicAutoValidation                 | 00.0261 | 00.0261 | 00.0261
		BasicAutoValidationWithAspect       | 00.0467 | 00.0467 | 00.0467
		BasicAutoValidationInline           | 00.0400 | 00.0400 | 00.0400
		BasicAutoValidationInlineWithAspect | 00.1685 | 00.1685 | 00.1685
		*/
		public void Performance()
		{
			new PerformanceSupport( WriteLine, BasicAutoValidation, BasicAutoValidationWithAspect, BasicAutoValidationInline, BasicAutoValidationInlineWithAspect ).Run( 1 );
			// new PerformanceSupport( WriteLine, Performance_Argument_Contains, Performance_Argument_Get, Performance_Weak_Contains, Performance_Weak_Get ).Run();
		}

		/*[Fact]
		[Trait( Traits.Category, Traits.Categories.Performance )]
		public void RunCommandBodies()
		{
			new PerformanceSupport( WriteLine, RunBasicCoreCommand, RunAspectCommand, RunExtensibleCommand, RunEnabledExtensibleCommand ).Run( 1 );
		}

		static void RunBasicCoreCommand() => BasicCoreCommand.Default.Execute( Defaults.Parameter );
		static void RunAspectCommand() => AspectCommand.Default.Execute( Defaults.Parameter );
		static void RunExtensibleCommand() => ExtensibleCommand.Default.Execute( Defaults.Parameter );
		static void RunEnabledExtensibleCommand() => EnabledExtensibleCommand.Default.Execute( Defaults.Parameter );

		class BasicCoreCommand : CommandBase<object>
		{
			public static BasicCoreCommand Default { get; } = new BasicCoreCommand();
			BasicCoreCommand() {}

			public override void Execute( object parameter ) {}
		}

		class AspectCommand : CommandBase<object>
		{
			public static AspectCommand Default { get; } = new AspectCommand();
			AspectCommand() {}

			[Aspect]
			public override void Execute( object parameter ) {}
		}

		[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
		[LinesOfCodeAvoided( 1 ), AttributeUsage( AttributeTargets.Method )]
		public sealed class Aspect : MethodInterceptionAspect {}
		class ExtensibleCommand : CommandBase<object>
		{
			public static ExtensibleCommand Default { get; } = new ExtensibleCommand();
			ExtensibleCommand() {}

			[ExtensionPoint]
			public override void Execute( object parameter ) {}
		}

		[EnableExtensions]
		class EnabledExtensibleCommand : CommandBase<object>
		{
			public static EnabledExtensibleCommand Default { get; } = new EnabledExtensibleCommand();
			EnabledExtensibleCommand() {}

			[ExtensionPoint]
			public override void Execute( object parameter ) {}
		}*/

/*
				Test                          | Average |  Median |    Mode
				-----------------------------------------------------------
				Performance_Argument_Contains | 00.0046 | 00.0045 | 00.0044
				Performance_Argument_Get      | 00.0039 | 00.0039 | 00.0039
				Performance_Weak_Contains     | 00.0049 | 00.0048 | 00.0051
				Performance_Weak_Get          | 00.0050 | 00.0049 | 00.0048
				*/
		/*readonly object key = new object();
		readonly ICache<IInvocationChain> cache = new Cache<IInvocationChain>( o => new InvocationChain() );
		readonly IArgumentCache<object, IInvocationChain> argument = new ArgumentCache<object, IInvocationChain>( o => new InvocationChain() );

		[Fact]
		public void Performance_Argument_Get() => argument.Get( key );

		[Fact]
		public void Performance_Argument_Contains() => argument.Contains( key );

		[Fact]
		public void Performance_Weak_Get() => cache.Get( key );

		[Fact]
		public void Performance_Weak_Contains() => cache.Contains( key );
		*/

		[Fact]
		public void BasicAutoValidation() => BasicAutoValidationWith( validating, validating, factory );

		[Fact]
		public void BasicAutoValidationInline()
		{
			var sut = new ExtendedFactory();
			var source = new AutoValidatingSource<int, float>( sut, sut );
			BasicAutoValidationWith( source, source, sut );
		}

		[Fact]
		public void BasicAutoValidationWithAspect() => BasicAutoValidationWith( applied, applied, applied );

		[Fact]
		public void BasicAutoValidationInlineWithAspect()
		{
			var sut = new AppliedExtendedFactory();
			BasicAutoValidationWith( sut, sut, sut );
		}

		[Fact]
		public void ParameterHandler()
		{
			var sut = new CachedAppliedExtendedFactory();
			var first = sut.Get( 6776 );
			// Assert.Equal( 0, sut.CanCreateCalled );
			// Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, first );

			var can = sut.IsSatisfiedBy( 6776 );
			// Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.True( can );

			var second = sut.Get( 6776 );
			// Assert.Equal( 0, sut.CanCreateCalled );
			// Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( first, second );
		}

		static void BasicAutoValidationWith( IParameterizedSource<int, float> factory, ISpecification<int> specification, IExtendedFactory sut )
		{
			// Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );

			/*var invalid = specification.IsSatisfiedBy( "Message" );
			Assert.False( invalid );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );*/

			var cannot = specification.IsSatisfiedBy( 456 );
			Assert.False( cannot );
			// Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			factory.Get( 123 );

			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var can = specification.IsSatisfiedBy( 6776 );
			Assert.True( can );
			// Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 3, sut.CanCreateGenericCalled );

			// Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = factory.Get( 6776 );
			// Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 3, sut.CanCreateGenericCalled );
			// Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
			sut.Reset();
		}

		interface IExtendedFactory : IParameterizedSource<int, float>, ISpecification<int>
		{
			// int CanCreateCalled { get; }

			// int CreateCalled { get; }

			int CanCreateGenericCalled { get; }

			int CreateGenericCalled { get; }

			void Reset();
		}

		class CachedAppliedExtendedFactory : AppliedExtendedFactory
		{
			[Freeze]
			public override float Get( int parameter ) => base.Get( parameter );
		}

		[DragonSpark.Aspects.Extensions.ApplyAutoValidation]
		class AppliedExtendedFactory : IExtendedFactory
		{
			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }
			public void Reset() => CanCreateGenericCalled = CreateGenericCalled = 0;

			public bool IsSatisfiedBy( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public virtual float Get( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}
		}

		class ExtendedFactory : IExtendedFactory
		{
			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }
			public void Reset() => CanCreateGenericCalled = CreateGenericCalled = 0;

			public bool IsSatisfiedBy( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public float Get( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}
		}
	}
}