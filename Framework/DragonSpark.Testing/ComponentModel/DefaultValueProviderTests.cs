﻿using DragonSpark.Activation;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Objects;
using DragonSpark.Testing.Objects.Setup;
using Microsoft.Practices.ServiceLocation;
using System;
using Xunit;
using ServiceLocator = DragonSpark.Activation.IoC.ServiceLocator;

namespace DragonSpark.Testing.ComponentModel
{
	[UnityContainerFactory.Register]
	[Freeze( typeof(IServiceLocator), typeof(ServiceLocator) )]
	[Freeze( typeof(IActivator), typeof(Activator) )]
	public class DefaultValueProviderTests
	{
		public class Activator : LocatorBase
		{
			readonly IServiceLocator locator;

			public Activator( IServiceLocator locator )
			{
				this.locator = locator;
			}
			
			protected override object CreateItem( LocateTypeRequest parameter ) => locator.GetInstance( parameter.RequestedType, parameter.Name );
		}

		[Theory, Objects.IoC.AutoData]
		void Apply()
		{
			var current = DateTime.Now;
			var target = new ClassWithDefaultProperties();

			Assert.Equal( 'd', target.Char );
			Assert.Equal( 7, target.Byte );
			Assert.Equal( 8, target.Short );
			Assert.Equal( 9, target.Int );
			Assert.Equal( 6776, target.Long );
			Assert.Equal( 6.7F, target.Float );
			Assert.Equal( 7.1, target.Double );
			Assert.True( target.Boolean );
			Assert.Equal( "Hello World", target.String );
			Assert.Equal( "Legacy", target.Legacy );
			
			Assert.Equal( typeof(ClassWithDefaultProperties), target.Object );

			Assert.NotEqual( DateTime.MinValue, target.CurrentDateTime );
			Assert.NotEqual( DateTimeOffset.MinValue, target.CurrentDateTimeOffset );

			Assert.True( target.CurrentDateTime >= current );
			Assert.True( target.CurrentDateTimeOffset >= current );

			Assert.NotNull( target.Activated );

			Assert.IsType<ClassWithParameter>( target.Factory );

			Assert.NotNull( target.Collection );
			Assert.IsAssignableFrom<System.Collections.ObjectModel.Collection<object>>( target.Collection );
			Assert.NotNull( target.Classes );
			Assert.IsAssignableFrom<System.Collections.ObjectModel.Collection<Class>>( target.Classes );

			Assert.Equal( 6776, target.ValuedInt );

			Assert.NotEqual( Guid.Empty, target.Guid );
			Assert.NotEqual( Guid.Empty, target.AnotherGuid );

			Assert.NotEqual( target.Guid, target.AnotherGuid );

			Assert.Equal( new Guid( "66570344-BA99-4C90-A7BE-AEC903441F97" ), target.ProvidedGuid );

			Assert.Equal( "Already Set", target.AlreadySet );
		}
	}
}