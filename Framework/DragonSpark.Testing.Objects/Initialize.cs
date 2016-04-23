﻿using DragonSpark.Testing.Objects.Setup;
using PostSharp.Aspects;

namespace DragonSpark.Testing.Objects
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Warmup() => DefaultUnityContainerFactory.Instance.Create();
	}
}
