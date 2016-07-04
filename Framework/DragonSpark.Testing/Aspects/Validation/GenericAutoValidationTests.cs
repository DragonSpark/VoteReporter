using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Diagnostics;
using PostSharp.Patterns.Model;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects.Validation
{
	public class GenericAutoValidationTests : TestCollectionBase
	{
		[Reference]
		readonly ExtendedFactory factory = new ExtendedFactory();

		[Reference]
		readonly IFactory<int, float> validating;
		[Reference]
		readonly AppliedExtendedFactory applied = new AppliedExtendedFactory();

		public GenericAutoValidationTests( ITestOutputHelper output ) : base( output )
		{
			validating = new AutoValidatingFactory<int, float>( factory );
		}

		[Fact]
		[Trait( Traits.Category, Traits.Categories.Performance )]
		public void Performance()
		{
			new PerformanceSupport( WriteLine, BasicAutoValidation, BasicAutoValidationInline, BasicAutoValidationApplied, BasicAutoValidationAppliedInline ).Run();
		}

		[Fact]
		public void BasicAutoValidation() => BasicAutoValidationWith( validating, factory );

		[Fact]
		public void BasicAutoValidationInline()
		{
			var sut = new ExtendedFactory();
			BasicAutoValidationWith( new AutoValidatingFactory<int, float>( sut ), sut );
		}

		[Fact]
		public void BasicAutoValidationApplied() => BasicAutoValidationWith( applied, applied );

		[Fact]
		public void BasicAutoValidationAppliedInline()
		{
			var sut = new AppliedExtendedFactory();
			BasicAutoValidationWith( sut, sut );
		}

		[Fact]
		public void ParameterHandler()
		{
			var mapped = typeof(CachedAppliedExtendedFactory).Adapt().GetMappedMethods<IFactory<int, float>>();

			/*var sut = new CachedAppliedExtendedFactory();
			var first = sut.Create( 6776 );
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, first );

			var can = sut.CanCreate( 6776 );
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.True( can );

			var second = sut.Create( 6776 );
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( first, second );*/
		}

		static void BasicAutoValidationWith( IFactory<int, float> factory, IExtendedFactory sut )
		{
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );

			var invalid = factory.CanCreate( "Message" );
			Assert.False( invalid );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );
			
			var cannot = factory.CanCreate( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );

			var can = factory.CanCreate( 6776 );
			Assert.True( can );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );

			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = factory.Create( 6776 );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
			sut.Reset();
		}

		

		interface IExtendedFactory : IFactory<int, float>
		{
			int CanCreateCalled { get; }

			int CreateCalled { get; }

			int CanCreateGenericCalled { get; }

			int CreateGenericCalled { get; }

			void Reset();
		}

		class CachedAppliedExtendedFactory : AppliedExtendedFactory
		{
			[Freeze]
			public override float Create( int parameter ) => base.Create( parameter );
		}
		
		[ApplyAutoValidation]
		class AppliedExtendedFactory : IExtendedFactory
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }
			public void Reset() => CanCreateCalled = CreateCalled = CanCreateGenericCalled = CreateGenericCalled = 0;

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return parameter is int && CanCreate( (int)parameter );
			}

			object IFactoryWithParameter.Create( object parameter )
			{
				CreateCalled++;
				return Create( (int)parameter );
			}

			public bool CanCreate( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public virtual float Create( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}
		}

		class ExtendedFactory : IExtendedFactory
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }
			public void Reset() => CanCreateCalled = CreateCalled = CanCreateGenericCalled = CreateGenericCalled = 0;

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return parameter is int && CanCreate( (int)parameter );
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return Create( (int)parameter );
			}

			public bool CanCreate( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public float Create( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}
		}
	}
}