﻿using DragonSpark.Windows.Runtime;
using Xunit;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class TaskLocalValueTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Assign( int number, TaskLocalValue<int> sut )
		{
			sut.Assign( number );
			Assert.Equal( number, sut.Item );
		}
	}
}