using DragonSpark.Extensions;
using DragonSpark.Logging;
using Microsoft.Practices.Unity;
using System;
using System.Linq;
using System.Windows.Markup;

namespace DragonSpark.Setup
{
	[ContentProperty( "InjectionMembers" )]
	public class UnityType : UnityRegistrationCommand
	{
		public Type MapTo { get; set; }
		
		public System.Collections.ObjectModel.Collection<InjectionMember> InjectionMembers => injectionMembers.Value;
		readonly Lazy<System.Collections.ObjectModel.Collection<InjectionMember>> injectionMembers = new Lazy<System.Collections.ObjectModel.Collection<InjectionMember>>( () => new System.Collections.ObjectModel.Collection<InjectionMember>() );

		public System.Collections.ObjectModel.Collection<IUnityContainerTypeConfiguration> TypeConfigurations => typeConfigurations.Value;
		readonly Lazy<System.Collections.ObjectModel.Collection<IUnityContainerTypeConfiguration>> typeConfigurations = new Lazy<System.Collections.ObjectModel.Collection<IUnityContainerTypeConfiguration>>( () => new System.Collections.ObjectModel.Collection<IUnityContainerTypeConfiguration>() );

		protected override void Configure( IUnityContainer container )
		{
			var type = MapTo ?? RegistrationType;

			InjectionMembers.Any().IsFalse( () => InjectionMembers.Add( new InjectionConstructor() ) );

			var mapping = string.Concat( RegistrationType.ToString(), RegistrationType != type ? $" -> {type}" : string.Empty );
			container.Resolve<ILoggerFacade>().Log( $"Registering Unity Type: {mapping}", Category.Debug, DragonSpark.Logging.Priority.None );
			var members = InjectionMembers.ToArray();
			container.RegisterType( RegistrationType, type, BuildName, Lifetime, members );

			TypeConfigurations.Apply( item => item.Configure( container, this ) );
		}
	}
}