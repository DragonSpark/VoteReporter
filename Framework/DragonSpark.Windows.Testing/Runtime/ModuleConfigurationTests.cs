﻿using DragonSpark.Windows.Modularity;
using DragonSpark.Windows.Testing.TestObjects;
using Xunit;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class ModuleConfigurationTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Load( ModulesConfiguration sut )
		{
			var section = new ModulesConfigurationSectionFactory( sut.Get ).Get();
			Assert.True( section.Modules.Count > 0 );
			Assert.True( section.Modules[0].Dependencies.Count > 0 );
			Assert.NotNull( section.Modules[0].Dependencies[0]  );
		}
	}
}