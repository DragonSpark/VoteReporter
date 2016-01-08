﻿using DragonSpark.Testing.Framework.Setup;

namespace DragonSpark.Windows.Testing.Setup
{
	public partial class ProgramSetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Framework.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( SetupFixtureFactory<ProgramSetup>.Instance.Create )
			{ }
		}

		public ProgramSetup()
		{
			InitializeComponent();
		}
	}
}