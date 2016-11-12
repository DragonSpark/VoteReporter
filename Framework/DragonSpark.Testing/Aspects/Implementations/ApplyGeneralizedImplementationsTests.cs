﻿using DragonSpark.Aspects.Implementations;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Windows.Input;
using Xunit;

namespace DragonSpark.Testing.Aspects.Implementations
{
	public class ApplyGeneralizedImplementationsTests
	{
		[Fact]
		public void VerifyCommand()
		{
			var sut = new Command();
			Assert.Null( sut.As<ISpecification<object>>() );
		}

		[Fact]
		public void VerifySpecification()
		{
			var sut = new Specification();
			Assert.IsAssignableFrom<ISpecification<object>>( sut );
		}

		[Fact]
		public void VerifySource()
		{
			var sut = new Source();
			Assert.IsAssignableFrom<IParameterizedSource<object, object>>( sut );
		}

		[Fact]
		public void VerifyImplementedSource()
		{
			var sut = new AlreadyImplementedSource();
			var parameter = new object();
			Assert.Same( parameter, sut.Get( parameter ) );
		}

		[Fact]
		public void VerifyImplementedSpecification()
		{
			var sut = new AlreadyImplementedSpecification();
			Assert.True( sut.IsSatisfiedBy( new object() ) );
		}

		[ApplyGeneralizedImplementations]
		sealed class AlreadyImplementedSource : ParameterizedSourceBase<string, bool>, IParameterizedSource<object, object>
		{
			public override bool Get( string parameter ) => false;
			public object Get( object parameter ) => parameter;
		}

		[ApplyGeneralizedImplementations]
		sealed class AlreadyImplementedSpecification : SpecificationBase<string>, ISpecification<object>
		{
			public override bool IsSatisfiedBy( string parameter )
			{
				return true;
			}

			public bool IsSatisfiedBy( object parameter )
			{
				return true;
			}
		}

		[ApplyGeneralizedImplementations]
		sealed class Source : ParameterizedSourceBase<string, bool>
		{
			public override bool Get( string parameter ) => false;
		}

		[ApplyGeneralizedImplementations]
		sealed class Specification : SpecificationBase<DateTime>
		{
			public override bool IsSatisfiedBy( DateTime parameter ) => false;
		}

		sealed class Command : ICommand<int>
		{
			bool ICommand.CanExecute( object parameter ) => false;

			void ICommand.Execute( object parameter ) {}

			public event EventHandler CanExecuteChanged = delegate {};
			bool ISpecification<int>.IsSatisfiedBy( int parameter ) => false;

			void ICommand<int>.Execute( int parameter ) {}
			void ICommand<int>.Update() {}
		}
	}
}