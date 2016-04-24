﻿using DragonSpark.Modularity;
using DragonSpark.Testing.Framework;
using Xunit;

namespace DragonSpark.Windows.Testing.Modularity
{
	[Trait( Traits.Category, Traits.Categories.Modularity )]
	public class ModuleTypeLoaderTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Cover( ModuleInfo info, ModuleTypeLoaderExtended sut )
		{
			Assert.True( sut.CanLoadModuleType(info) );
			var called = false;
			sut.LoadModuleCompleted += ( sender, args ) => called = true;
			sut.LoadModuleType( info );
			Assert.True( called );
			Assert.False( sut.Disposed );
			sut.Dispose();
			Assert.True( sut.Disposed );
		}

		public class ModuleTypeLoaderExtended : ModuleTypeLoader
		{
			public bool Disposed { get; private set; }

			protected override void Dispose( bool disposing )
			{
				base.Dispose( disposing );
				Disposed = true;
			}
		}
	}
}