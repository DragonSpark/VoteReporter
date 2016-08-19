using DragonSpark.Diagnostics.Exceptions;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;

namespace DragonSpark.Windows.Runtime
{
	public class ExceptionHandler : DragonSpark.Diagnostics.Exceptions.IExceptionHandler
	{
		public const string DefaultExceptionPolicy = "Default Exception Policy";
		readonly ExceptionManager manager;
		readonly string policyName;

		public ExceptionHandler( ExceptionManager manager, string policyName = DefaultExceptionPolicy )
		{
			this.manager = manager;
			this.policyName = policyName;
		}

		public virtual ExceptionHandlingResult Handle( Exception exception )
		{
			Exception resultingException;
			var rethrow = manager.HandleException( exception, policyName, out resultingException );
			var result = new ExceptionHandlingResult( rethrow, resultingException ?? exception );
			return result;
		}
	}
}