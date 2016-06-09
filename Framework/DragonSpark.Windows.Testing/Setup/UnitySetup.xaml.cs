﻿using DragonSpark.Activation;
using DragonSpark.Testing.Framework.Setup;
using System;
using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class UnitySetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Objects.IoC.AutoDataAttribute
		{
			readonly static DelegatedFactory<IServiceProvider, IApplication> ApplicationSource = new DelegatedFactory<IServiceProvider, IApplication>( serviceProvider => new Application<UnitySetup>( serviceProvider ) );

			public AutoDataAttribute() : base( ApplicationSource ) {}
		}

		public UnitySetup()
		{
			InitializeComponent();
		}
	}
}
