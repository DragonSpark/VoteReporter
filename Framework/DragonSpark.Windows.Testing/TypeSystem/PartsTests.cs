﻿using DragonSpark.Application;
using DragonSpark.Sources;
using DragonSpark.Testing.Framework.Application;
using DragonSpark.Testing.Objects.FileSystem;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace DragonSpark.Windows.Testing.TypeSystem
{
	public class PartsTests
	{
		[Fact]
		public void Public()
		{
			Assert.Empty( ApplicationAssemblies.Default.Unwrap() );
			InitializePartsCommand.Default.Execute();
			PublicAttributed( PublicPartsLocator.Default.Get( GetType().Assembly ) );
		}

		[Theory, AutoData, InitializePartsCommand.Public]
		public void PublicAttributed( ImmutableArray<Type> types )
		{
			Assert.Single( types );
			Assert.Equal( "DragonSpark.Testing.Parts.PublicClass", types.Single().FullName );
		}

		[Fact]
		public void All()
		{
			Assert.Empty( ApplicationAssemblies.Default.Unwrap() );
			InitializePartsCommand.Default.Execute();
			AllAttributed( AllPartsLocator.Default.Get( GetType().Assembly ) );
		}

		[Theory, AutoData, InitializePartsCommand.All]
		public void AllAttributed( ImmutableArray<Type> types )
		{
			Assert.Equal( 2, types.Length );
			var names = types.Select( type => type.FullName ).ToArray();
			Assert.Contains( "DragonSpark.Testing.Parts.PublicClass", names );
			Assert.Contains( "DragonSpark.Testing.Parts.NonPublicClass", names );
		}
	}
}