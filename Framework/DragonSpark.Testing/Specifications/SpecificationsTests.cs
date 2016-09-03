﻿using DragonSpark.Sources.Parameterized;
using Xunit;

namespace DragonSpark.Testing.Specifications
{
	public class SpecificationsTests
	{
		[Fact]
		public void Never()
		{
			Assert.False( DragonSpark.Specifications.Specifications.Never.IsSatisfiedBy( Defaults.Parameter ) );
		} 
	}
}