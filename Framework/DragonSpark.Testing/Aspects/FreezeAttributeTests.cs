﻿using DragonSpark.Aspects;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Aspects
{
	public class FreezeAttributeTests
	{
		[Fact]
		public void ProviderCached()
		{
			var sut = new Source();

			sut.Create();
			Assert.Equal( 1, sut.Count );
			sut.Create();
			Assert.Equal( 1, sut.Count );

			Assert.Equal( 2, sut.Cached );
			Assert.Equal( 2, sut.Cached );
		}

		[Fact]
		public void CheckFreeze()
		{
			var sut = new Disposable();
			sut.Dispose();
			Assert.Equal( 1, sut.Count );
			sut.Dispose();
			Assert.Equal( 1, sut.Count );

			sut.Other();
			Assert.Equal( 2, sut.Count );
			sut.Other();
			Assert.Equal( 2, sut.Count );
		}

		public class Disposable : IDisposable
		{
			public int Count { get; private set; }
			

			public void Dispose() => Dispose( true );

			public void Other() => Dispose( false );

			[Freeze]
			protected virtual void Dispose( bool disposing ) => Count++;
		}

		public class Source : AssemblySourceBase
		{
			public int Count { get; private set; }

			[Freeze]
			public int Cached => ++Count;

			protected override Assembly[] CreateItem()
			{
				Count++;
				return new Assembly[0];
			}
		}

		[Theory, AutoData]
		public void BasicCache( CacheItem sut )
		{
			Assert.Equal( 1, sut.Count );
			sut.Up();
			Assert.Equal( 2, sut.Count );
			sut.Up();
			Assert.Equal( 2, sut.Count );

			sut.UpWith( 2 );
			Assert.Equal( 4, sut.Count );
			sut.UpWith( 2 );
			Assert.Equal( 4, sut.Count );
			sut.UpWith( 3 );
			Assert.Equal( 7, sut.Count );
			sut.UpWith( 3 );
			Assert.Equal( 7, sut.Count );
		}

		public class CacheItem
		{
			public int Count { get; private set; } = 1;

			[Freeze]
			public void Up()
			{
				Count++;
			}

			[Freeze]
			public void UpWith( int i )
			{
				Count += i;
			}
		}
	}
}