using System;
using System.Reflection;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using Microsoft.Practices.ServiceLocation;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;

namespace DragonSpark.Testing.Framework
{
	public class AssignedAttribute : CustomizeAttribute
	{
		class Customization : ICustomization
		{
			readonly Type type;

			public Customization( Type type )
			{
				this.type = type;
			}

			public void Customize( IFixture fixture )
			{
				fixture.TryCreate<IServiceLocator>( type ).With( locator =>
				{
					var location = fixture.Create<IServiceLocation>();
					location.Assign( locator );
				});
			}
		}

		public override ICustomization GetCustomization( ParameterInfo parameter )
		{
			var result = new Customization( parameter.ParameterType );
			return result;
		}
	}
}