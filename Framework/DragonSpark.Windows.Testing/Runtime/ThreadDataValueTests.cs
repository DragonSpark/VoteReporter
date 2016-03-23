﻿using DragonSpark.Windows.Runtime;
using Xunit;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class ThreadDataValueTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Assign( ThreadDataValue<int> sut, int number )
		{
			sut.Assign( number );
			Assert.Equal( number, sut.Item );
		} 
	}
}