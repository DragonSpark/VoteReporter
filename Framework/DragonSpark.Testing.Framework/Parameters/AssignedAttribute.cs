using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework.Setup.Location;
using Microsoft.Practices.ServiceLocation;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Parameters
{
	public class AssignedAttribute : CustomizeAttribute
	{
		class Customization : CustomizationBase
		{
			readonly Type type;

			public Customization( Type type )
			{
				this.type = type;
			}

			[Activate]
			IServiceLocation Location { get; set; }

			protected override void Customize( IFixture fixture )
			{
				var serviceLocator = fixture.Create<IServiceLocator>( type );
				serviceLocator.With( Location.Assign );
			}
		}

		public override ICustomization GetCustomization( ParameterInfo parameter ) => new Customization( parameter.ParameterType );
	}
}