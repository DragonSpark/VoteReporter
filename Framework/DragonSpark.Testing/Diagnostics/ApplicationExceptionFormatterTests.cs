﻿using DragonSpark.Testing.Framework;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class ApplicationExceptionFormatterTests
	{
		/*[Theory, Framework.Setup.AutoData]
		public void Format( [Frozen]AssemblyInformation information, ApplicationExceptionFormatter sut, Exception exception )
		{
			var message = sut.Format( exception );
			Assert.Contains( information.Title, message );
			Assert.Contains( information.Product, message );
			Assert.Contains( information.Version.ToString(), message );
			Assert.Contains( exception.GetType().ToString(), message );
			Assert.Contains( exception.Message, message );
		}*/

		/*[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void FormatException( [Frozen]AssemblyInformation information, Exception error, ApplicationExceptionFormatter sut )
		{
			var message = sut.Format( error );
			var fullName = error.GetType().FullName;
			var sections = new[] { $@"Exception occured in application {information.Title} ({information.Product}).
[Version: {information.Version}]

Exception of type '{fullName}' was thrown.
{fullName}
Details:
==============================================
An exception of type '{fullName}' occurred and was caught.
----------------------------------------------------------------", $@"Type : {error.GetType().AssemblyQualifiedName}
Message : {error.Message}
Source : {error.Source}
Help link : {error.HelpLink}
Data : System.Collections.ListDictionaryInternal
TargetSite : 
HResult : {error.HResult}
Stack Trace : The stack trace is unavailable.
Additional Info:

MachineName : {Environment.MachineName}", $@"FullName : {typeof(ExceptionPolicy).Assembly.FullName}
AppDomainName : {AppDomain.CurrentDomain.FriendlyName}
ThreadIdentity : {Thread.CurrentPrincipal.Identity.Name}
WindowsIdentity : {WindowsIdentity.GetCurrent().Name}" };
			sections.Each( s => Assert.Contains( s, message ) );
			
		}*/
	}
}